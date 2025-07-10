using System;
using System.IO;
using NUnit.Framework;

namespace AppDomainWrappers.Net.Test
{
	public class TemporaryWorkspaceTests : TestCase
	{
		public void TestTemporaryWorkspace_CreatesBaseDirectory()
		{
			using var tempWorkspace = new TemporaryWorkspace();
			AssertEquals($"Expected that {nameof(TemporaryWorkspace)} would create it's root directory on initialisation, but it didn't.", expected: true, Directory.Exists(tempWorkspace.RootDirectory));
		}

		public void TestTemporaryWorkspace_CreatesManagedDirectory()
		{
			using var tempWorkspace = new TemporaryWorkspace();
			var managedDir = tempWorkspace.CreateManagedDirectory();

			AssertEquals($"Expected that {nameof(TemporaryWorkspace.CreateManagedDirectory)} creates a new {nameof(ManagedDirectory)} object and a directory location, but it didn't.", expected: true, Directory.Exists(managedDir.FullPath));
		}

		public void TestTemporaryWorkspace_CreateManagedDirectory_MultipleTimes_UniqueDirectories()
		{
			using var tempWorkspace = new TemporaryWorkspace();
			var dir1 = tempWorkspace.CreateManagedDirectory();
			var dir2 = tempWorkspace.CreateManagedDirectory();

			AssertEquals($"Expected that each created {nameof(ManagedDirectory)} would have it's own unique directory, but it didn't.", expected: false, string.Compare(dir1.FullPath, dir2.FullPath, StringComparison.InvariantCultureIgnoreCase) == 0);
		}

		public void TestTemporaryWorkspace_DeletesRootDirectoryOnDispose_WhenEmpty()
		{
			string rootPath;
			using (var tempWorkspace = new TemporaryWorkspace())
			{
				rootPath = tempWorkspace.RootDirectory;
			}
			AssertEquals($"Expected that {nameof(TemporaryWorkspace)} would be deleted, outside of using context, but it was not.", expected: false, Directory.Exists(rootPath));
		}

		public void TestTemporaryWorkspace_DeletesDirectoryAndArtifactsOnDispose()
		{
			string rootPath;
			string artifactPath;
			bool directoryExistsAfterCreation;
			using (var tempWorkspace = new TemporaryWorkspace())
			{
				artifactPath = Path.Combine(tempWorkspace.RootDirectory, $"{Guid.NewGuid()}.txt");
				File.Create(artifactPath).Close();
				rootPath = tempWorkspace.RootDirectory;
				directoryExistsAfterCreation = Directory.Exists(tempWorkspace.RootDirectory);
			}

			CombineAssertions(() =>
			{
				AssertEquals($"Expected that a {nameof(TemporaryWorkspace)} directory would exist following instantiation, but it didn't.", expected: true, directoryExistsAfterCreation);
				AssertEquals($"Expected that the artifact created during the using of {nameof(TemporaryWorkspace)} would not exist after {nameof(TemporaryWorkspace.Dispose)}, but it did exist.", expected: false, File.Exists(artifactPath));
				AssertEquals($"Expected that the directory created by {nameof(TemporaryWorkspace)} '{rootPath}' would not exist after {nameof(TemporaryWorkspace.Dispose)}, but it did exist.", expected: false, Directory.Exists(rootPath));
			});
		}

		public void TestTemporaryWorkspace_Dispose_CleansUpManagedDirectories()
		{
			using var tempWorkspace = new TemporaryWorkspace();
			var managedDir = tempWorkspace.CreateManagedDirectory();
			var fullPath = managedDir.FullPath;

			tempWorkspace.Dispose();

			AssertEquals($"Expected {nameof(TemporaryWorkspace)} to dispose of created a {nameof(ManagedDirectory)} item on {nameof(TemporaryWorkspace.Dispose)}, but it didn't.", expected: false, Directory.Exists(fullPath));
		}

		public void TestTemporaryWorkspace_ManagesMultipleDirectoriesProperlyOnDispose()
		{
			using var tempWorkspace = new TemporaryWorkspace();
			var dir1 = tempWorkspace.CreateManagedDirectory();
			var dir2 = tempWorkspace.CreateManagedDirectory();

			tempWorkspace.Dispose();

			CombineAssertions($"Expected {nameof(TemporaryWorkspace)} to dispose of all created {nameof(ManagedDirectory)} items (multiple) on {nameof(TemporaryWorkspace.Dispose)}, but it didn't.", () =>
			{
				AssertEquals($"{nameof(ManagedDirectory)} '{dir1}' was not deleted, when it should have been.", expected: false, Directory.Exists(dir1.FullPath));
				AssertEquals($"{nameof(ManagedDirectory)} '{dir2}' was not deleted, when it should have been.", expected: false, Directory.Exists(dir2.FullPath));
			});
		}

		public void TestTemporaryWorkspace_CreatesSubdirectory_WhenDirectoryNameProvided()
		{
			var dirName = "testSubDir";
			using var tempWorkspace = new TemporaryWorkspace(dirName);
			AssertEquals($"Expected new '{dirName}' sub-directory to be created on initialisation of {nameof(TemporaryWorkspace)}, but it wasn't.", expected: true, Directory.Exists(tempWorkspace.RootDirectory));
		}

		public void TestTemporaryWorkspace_Finalizer_DisposesCorrectly()
		{
			var tempWorkspace = new TemporaryWorkspace();
			var rootPath = tempWorkspace.RootDirectory;
			tempWorkspace.Dispose();

			// Force garbage collection
			GC.Collect();
			GC.WaitForPendingFinalizers();

			AssertEquals($"Expected finalizer to delete {nameof(TemporaryWorkspace)} root directory, but it didn't.", expected: false, Directory.Exists(rootPath));
		}
	}
}
