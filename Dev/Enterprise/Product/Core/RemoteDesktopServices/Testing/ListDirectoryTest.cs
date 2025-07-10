using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.IO;
using Enterprise.RemoteDesktopServices.MessageElements;
using Enterprise.RemoteDesktopServices.Server;

namespace Enterprise.RemoteDesktopServices.Testing
{
	class ListDirectoryTest : RemoteDesktopServicesTest
	{
		public void TestListDirectoryFiles()
		{
			var tempDir = Temp.GetNewTempSubdirectory();
			try
			{
				var files = GetSubpaths(tempDir, "a.txt", "b.txt", "c.txt");
				EnsureTsFiles(files);
				EnsureTsDirectories(new[] { Path.Combine(tempDir, "ShouldntBeReturned") });

				var dirFiles = RemoteFileDialog.ListDirectoryFiles(tempDir, ".txt");

				AssertContainsExactElementsInAnyOrder("Should have found the three files", EqualityComparer<string>.Default, files, dirFiles);
			}
			finally
			{
				DeleteTsDirs(tempDir);
			}
		}

		public void TestSearchForFiles()
		{
			var tempDir = Temp.GetNewTempSubdirectory();
			try
			{
				var files = GetSubpaths(tempDir, "a.txt", "b.txt", "c.txt");
				EnsureTsFiles(files);
				EnsureTsDirectories(new[] { Path.Combine(tempDir, "ShouldntBeReturned") });

				var request = new ListDirectoryRequest(tempDir, SearchMode.Files, TimeSpan.FromSeconds(30));
				var result = EnterpriseChannel.Instance.SendMessage<ListDirectoryRequest[], ListDirectoryResult[]>(EnterpriseChannelMessageTypes.ListDirectory, new[] { request }).Single();

				AssertContainsExactElementsInAnyOrder("Should have found the three files", EqualityComparer<string>.Default, files, result.result);
			}
			finally
			{
				DeleteTsDirs(tempDir);
			}
		}

		public void TestThrowsExceptionIfNotUsingSystemPathSyntax()
		{
			var request = new[] { new ListDirectoryRequest(@"\\tsclient\C\Some\Path", SearchMode.Directories, TimeSpan.FromSeconds(30)) };

			var exception = AssertExceptionThrown<OperationCanceledException>("We don't want to start mixing the syntaxs, otherwise we wont know when a client is refering to a file on the server or their local machine",
			() =>
			{
				EnterpriseChannel.Instance.SendMessage<ListDirectoryRequest[], ListDirectoryResult[]>(EnterpriseChannelMessageTypes.ListDirectory, request);
			});
			Assert(exception.InnerException is RDPRemoteException rex && rex.RemoteExceptionType == nameof(ArgumentException));
		}

		public void TestSearchForDirectories()
		{
			var tempDir = Temp.GetNewTempSubdirectory();
			try
			{
				var dirs = GetSubpaths(tempDir, "a", "b", "c");
				EnsureTsDirectories(dirs);
				EnsureTsFiles(new[] { Path.Combine(tempDir, "foo.txt") }, "Shouldn't find this one");

				var request = new ListDirectoryRequest(tempDir, SearchMode.Directories, TimeSpan.FromSeconds(30));
				var result = EnterpriseChannel.Instance.SendMessage<ListDirectoryRequest[], ListDirectoryResult[]>(EnterpriseChannelMessageTypes.ListDirectory, new[] { request }).Single();

				AssertContainsExactElementsInAnyOrder("Should have found the three files", EqualityComparer<string>.Default, dirs, result.result);
			}
			finally
			{
				DeleteTsDirs(tempDir);
			}
		}

		public void TestMultipleCommandsAtTheSameTime()
		{
			var tempDir1 = Temp.GetNewTempSubdirectory();
			var tempDir2 = Temp.GetNewTempSubdirectory();

			try
			{
				var tempDir1_SubDirs = GetSubpaths(tempDir1, "a", "b", "c");
				var tempDir2_SubFiles = GetSubpaths(tempDir2, "d.txt", "e.txt", "f.txt");

				EnsureTsDirectories(tempDir1_SubDirs.Concat(GetSubpaths(tempDir2, "DontReadMe")));
				EnsureTsFiles(tempDir2_SubFiles.Concat(GetSubpaths(tempDir1, "DontReadMe.txt")));

				var requests = new[] {
					new ListDirectoryRequest(tempDir1, SearchMode.Directories, TimeSpan.FromSeconds(30)),
					new ListDirectoryRequest(tempDir2, SearchMode.Files, TimeSpan.FromSeconds(30)),
				};

				var results = EnterpriseChannel.Instance.SendMessage<ListDirectoryRequest[], ListDirectoryResult[]>(EnterpriseChannelMessageTypes.ListDirectory, requests);

				CombineAssertions(() =>
				{
					var dirsResult = results.Single(r => r.path == tempDir1);
					AssertContainsExactElementsInAnyOrder("Should have found the three directories", EqualityComparer<string>.Default, tempDir1_SubDirs, dirsResult.result);

					var fileResult = results.Single(r => r.path == tempDir2);
					AssertContainsExactElementsInAnyOrder("Should have found the three files", EqualityComparer<string>.Default, tempDir2_SubFiles, fileResult.result);
				});
			}
			finally
			{
				DeleteTsDirs(tempDir1, tempDir2);
			}
		}

		public void TestRequestingNonExistantDir()
		{
			var request = new[] { new ListDirectoryRequest(@"C:\I\Dont\Exist", SearchMode.Directories, TimeSpan.FromSeconds(30)) };
			var result = EnterpriseChannel.Instance.SendMessage<ListDirectoryRequest[], ListDirectoryResult[]>(EnterpriseChannelMessageTypes.ListDirectory, request).Single();

			AssertEquals(ListDirectoryError.DirectoryNotFound, result.error);
		}

		string[] GetSubpaths(string root, params string[] filenames)
			=> filenames.Select(fileName => Path.Combine(root, fileName)).ToArray();

		void EnsureTsDirectories(IEnumerable<string> paths)
			=> paths.ForEach(EnsureDirectory);

		void EnsureTsFiles(IEnumerable<string> paths, string content = "boop")
			=> paths.ForEach(path => File.WriteAllText(path, content));

		void DeleteTsDirs(params string[] paths)
			=> paths.ForEach(path => Directory.Delete(path, recursive: true));

		const string TsClientPrefix = @"\\tsclient\";
	}
}
