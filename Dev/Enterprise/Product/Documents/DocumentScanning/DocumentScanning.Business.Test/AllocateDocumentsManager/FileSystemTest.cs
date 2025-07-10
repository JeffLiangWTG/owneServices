using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.IO;
using Enterprise.RemoteDesktopServices.MessageElements;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.Test
{
	public class FileSystemTest : TestCase
	{
		public void TestNormalisesExtensions()
		{
			var extensions = new[] { "txt", ".NTT", "bMp", ".zzy" };
			var directory = new FileSystem(extensions);
			AssertContainsExactElementsInAnyOrder(new[] { "TXT", "NTT", "BMP", "ZZY" }, directory.FileExtensionsFilter);
		}

		public void TestDoesntRecreateDirectoryObjects()
		{
			using (DirectoryPreviewHelpers.CreateTempDirTree(out var root))
			{
				var fileSystem = new FileSystem(new[] { root }, new[] { "txt" });
				var dir = fileSystem.FindDirectory(Path.Combine(root, @"D11\D111"));

				var otherDir = fileSystem.FindDirectory(Path.Combine(root, @"D12"));
				AssertNotNull("PRE: Touching the object to ensure we've changed dirs", otherDir);

				AssertSame("We should be scanning the tree to find the same instance - not recreating", dir, fileSystem.FindDirectory(Path.Combine(root, @"D11\D111")));
			}
		}

		public void TestSelectedDirectoryMatchesPath()
		{
			using (DirectoryPreviewHelpers.CreateTempDirTree(out var root))
			{
				var fileSystem = new FileSystem(new[] { root }, new[] { "txt" });

				AssertEquals(Path.Combine(root, @"D11\"), fileSystem.FindDirectory(Path.Combine(root, @"D11")).FullPath);
				AssertEquals(Path.Combine(root, @"D11\D111\"), fileSystem.FindDirectory(Path.Combine(root, @"D11\D111")).FullPath);
				AssertEquals(Path.Combine(root, @"D12\"), fileSystem.FindDirectory(Path.Combine(root, @"D12")).FullPath);
				AssertNull("When the path doesnt exist, the SelectedDirectory should be null", fileSystem.FindDirectory(Path.Combine(root, @"DoesntExist")));
			}
		}

		public void TestFetchDirectories_NotIncludeUncPathInLocalSearch()
		{
			using (DirectoryPreviewHelpers.CreateTempDirTree(out var root))
			{
				var fileSystem = new FileSystem(new[] { root }, new[] { "txt" });

				var localPath = Path.Combine(root, "D11");
				AssertNoExceptionThrown(() => fileSystem.FetchDirectories(new[] { localPath, @"\\127.0.0.1\c$\users" }));
				AssertEquals("D11 only has one subdirectory D111.", 1, fileSystem.GetDirectories(localPath).Count());
				AssertEquals("UNC path should be excluded in the result for local search.", 0, fileSystem.GetDirectories(@"\\127.0.0.1\c$\users").Count());
			}
		}

		public void TestFileSystem_IsLocalDirectory()
		{
			AssertEquals(true, FileSystem.IsLocalDirectory(@"c:\foo"));
			AssertEquals(true, FileSystem.IsLocalDirectory(@"d:\foo\bar\foo.txt"));
			AssertEquals(false, FileSystem.IsLocalDirectory(@"\\foo\bar"));
			AssertEquals(false, FileSystem.IsLocalDirectory(@"\\foo\bar\foo.txt"));
			AssertEquals(false, FileSystem.IsLocalDirectory(@"ftp://foo:bar/bar.com/foo.txt"));
			AssertEquals(false, FileSystem.IsLocalDirectory(@"https://google.com/foo"));
			AssertEquals(false, FileSystem.IsLocalDirectory(@"c:\foo>"));
			AssertEquals(false, FileSystem.IsLocalDirectory(@"invalidUri\foo.txt"));
		}
	}

	public class MockFileSystem : IFileSystem
	{
		readonly Regex rootRegex = new Regex(@"^[\w\.]+\\", RegexOptions.IgnoreCase);
		readonly ICollection<DirectoryBusinessObject> roots;
		readonly List<string> paths;

		public IList<string[]> FetchRequests { get; } = new List<string[]>();
		public IList<string> ThingsYouProbablyWantToFetchFirst { get; } = new List<string>(); // For requests that weren't prefetched - potential performance hit if this gets high

		public IEnumerable<DirectoryBusinessObject> Roots => roots;

		public MockFileSystem(params string[] paths)
		{
			this.paths = new List<string>(paths);

			roots = paths
				.Select(path => rootRegex.Match(path).Value)
				.Distinct()
				.Select(path => new DirectoryBusinessObject(this, path))
				.ToList();
		}

		public void AddPath(string path)
		{
			var potentiallyNewRoot = rootRegex.Match(path).Value;
			if (!roots.Any(r => r.FullPath == potentiallyNewRoot))
			{
				roots.Add(new DirectoryBusinessObject(this, potentiallyNewRoot));
			}

			paths.Add(path);
		}

		public void RemovePath(string path)
		{
			if (!paths.Remove(path))
			{
				throw new ArgumentException("Failed to remove the path",nameof(path));
			}
		}

		public bool CanAccessDirectory(string path)
			=> paths.Any(p => p.StartsWith(path));

		public IEnumerable<string> GetDirectories(string path)
		{
			if (!string.IsNullOrEmpty(path) && !FetchRequests.SelectMany(p => p).Contains(path.TrimEnd('\\')))
			{
				ThingsYouProbablyWantToFetchFirst.Add(path);
			}

			return GetUniquePathsMatching($@"^{Regex.Escape(path.TrimEnd('\\'))}\\[\w+\.]+\\");
		}

		public IEnumerable<string> GetFiles(string path)
			=> GetUniquePathsMatching($@"^{Regex.Escape(path.TrimEnd('\\'))}\\[\w+\.]+$");

		IEnumerable<string> GetUniquePathsMatching(string regex)
		{
			var r = new Regex(regex, RegexOptions.IgnoreCase);

			var selectedOnes = paths.Select(path => r.Match(path))
				.Where(m => m.Success)
				.Select(m => m.Value);
#if NETFRAMEWORK
			return selectedOnes.DistinctBy(v => v.ToUpperInvariant());
#else
			return System.Linq.Enumerable.DistinctBy(selectedOnes, v => v.ToUpperInvariant());
#endif
		}

		public void FetchDirectories(IEnumerable<string> paths)
			=> FetchRequests.Add(paths.Select(path => path.TrimEnd('\\')).ToArray());
	}

	public class MockTestFileSystem : FileSystem
	{
		public MockTestFileSystem(IEnumerable<string> extensionsFilter) : base(extensionsFilter)
		{
		}

		public MockTestFileSystem(IEnumerable<string> validRoots, IEnumerable<string> extensionsFilter) : base(validRoots, extensionsFilter)
		{
		}

		internal override string[] PerformSearch(string path, SearchMode mode)
		{
			return new string[] { "C:\\" };
		}
	}

	public static class DirectoryPreviewHelpers
	{
		public static MockFileSystem CreateMockTempDirTree()
		{
			return new MockFileSystem(
				@"D1\D11\D111\",
				@"D1\D11\D11F1",
				@"D1\D11\D11F2",
				@"D1\D11\D11F3",
				@"D1\D12\D12F1",
				@"D1\D12\D12F2",
				@"D1\D1F1",
				@"D1\D1F2",
				@"D1\D1F3"
			);
		}

		public static DirectoryBusinessObject FindDirectory(this IFileSystem fileSystem, string path)
		{
			var root = fileSystem.Roots.FirstOrDefault(r => path.StartsWith(r.FullPath, StringComparison.OrdinalIgnoreCase));
			if (root != null)
			{
				return path.Substring(root.FullPath.Length)
					.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries)
					.Aggregate(root, (r, dirName) => r?.Directories[dirName]);
			}
			return null;
		}

		public static IDisposable CreateTempDirTree(out string root)
		{
			/*
			 * root
			 *  + D11
			 *    + D111
			 *      + (nothing / empty dir)
			 *    + D11F1
			 *    + D11F2
			 *    + D11F3
			 *  + D12
			 *    + D12F1
			 *    + D12F2
			 *  + D1F1
			 *  + D1F2
			 *  + D1F3
			 */

			var d1 = root = CreateFolderWithFiles(Temp.GetNewTempSubdirectory(), "D1F1.txt", "D1F2.txt", "D1F3.txt");
			var d11 = CreateFolderWithFiles(Path.Combine(d1, "D11"), "D11F1.txt", "D11F2.txt", "D11F3.txt");
			var d111 = CreateFolderWithFiles(Path.Combine(d11, "D111"));
			var d12 = CreateFolderWithFiles(Path.Combine(d1, "D12"), "D12F1.txt", "D12F2.txt");

			return new DisposableAction(() => Directory.Delete(d1, recursive: true));
		}

		public static string CreateFolderWithFiles(string rootDir, params string[] files)
		{
			if (!rootDir.StartsWith(Temp.TempPath))
			{
				throw new ArgumentException("Directory MUST be in temp");
			}

			Directory.CreateDirectory(rootDir);
			foreach (var file in files)
			{
				File.WriteAllText(Path.Combine(rootDir, file), file);
			}

			return rootDir;
		}
	}
}
