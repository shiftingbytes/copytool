using FluentAssertions;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using Xunit;

namespace CopyTool.Tests;

public class CopyOperationTests
{
    private readonly CopyOperation _sut;
    private readonly MockFileSystem _fileSystem;
    private const string _file1 = @"c:\testfolder\testfile1.txt";
    private const string _file2 = @"c:\testfolder\testfileCopy.txt";
    private const string _file3 = @"c:\testfolder\testfileCopy3.txt";
    private const string _folder1 = @"c:\testfolder";
    private const string _text = "Testing is meh.";

    public CopyOperationTests()
    {
        _fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { @$"{_folder1}", new MockDirectoryData() },
            { @$"{_file1}", new MockFileData(_text) },
            { @$"{_file3}", new MockFileData(_text) },
            { @"c:\myfile.txt", new MockFileData(_text) },
            { @"c:\demo\jQuery.js", new MockFileData("some js") },
            { @"c:\demo\image.gif", new MockFileData(new byte[] { 0x12, 0x34, 0x56, 0xd2 }) }
        });
        _sut = new CopyOperation(_fileSystem);
    }

    [Fact]
    public async void FileCopy_TwoFiles_CopyOk()
    {
        //given
        var testFileSrc = _file1;
        var testFileDest = _file2;
        //when
        await _sut.FileCopy(testFileSrc, testFileDest);
        //then
        var expectedFile = testFileDest;       
        _fileSystem.FileExists(expectedFile).Should().Be(true);

    }

    [Fact]
    public async void FileRead_Ok()
    {
        //given
        var testFileSrc = _file1;

        //when
        var text = await _fileSystem.File.ReadAllLinesAsync(testFileSrc);

        //then
        var expectedText = _text;
        string.Join("",text).Should().Be(expectedText);
    }

    [Fact]
    public async void FileCopy_DestinationReadOnly_ThrowsException()
    {
        //given
        var testFileSrc = _file1;
        var testFileDest = _file3;

        var expectedFile = testFileDest;
        _fileSystem.FileInfo.FromFileName(expectedFile).IsReadOnly = true;

        //when
        var action = async () => await _sut.FileCopy(testFileSrc, testFileDest);

        //then
        await action.Should().ThrowAsync<System.UnauthorizedAccessException>();        
    }

    [Fact]
    public async void FolderCopy_TwoFolders_CopyOk()
    {
        //given
        const string testFolderSrc = @"c:\testfolder\";
        const string testFolderDest = @"c:\testfolder1\";

        //when
        await _sut.FolderCopy(testFolderSrc, testFolderDest);
        //then
        string? expectedFolder = testFolderDest;
        _fileSystem.FileExists(expectedFolder).Should().Be(true);

    }
}