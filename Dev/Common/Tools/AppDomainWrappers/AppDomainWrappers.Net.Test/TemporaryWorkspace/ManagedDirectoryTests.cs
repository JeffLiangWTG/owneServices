using System;
using System.IO;
using NUnit.Framework;

namespace AppDomainWrappers.Net.Test
{
	public class ManagedDirectoryTests : TestCase
	{
		TemporaryWorkspace tempWorkspace;

		protected override void SetUp()
		{
			tempWorkspace = new TemporaryWorkspace();
		}

		protected override void TearDown()
		{
			tempWorkspace.Dispose();
		}

		public void TestManagedDirectory_CreatesDirectoryOnInstantiation()
		{
			var managedDir = new ManagedDirectory(tempWorkspace);
			AssertEquals("Directory wasn't found after initialisation.", expected: true, Directory.Exists(managedDir.FullPath));
		}

		public void TestManagedDirectory_ImplicitConversionToString_ReturnsFullPath()
		{
			var managedDir = new ManagedDirectory(tempWorkspace);
			string fullPath = managedDir;
			AssertEquals("Full path was not equivalent to the string path, meaning the implicit operator isn't working.", expected: true, string.Compare(fullPath, managedDir.FullPath, StringComparison.InvariantCultureIgnoreCase) == 0);
		}

		public void TestManagedDirectory_Dispose_DeletesDirectory()
		{
			var managedDir = new ManagedDirectory(tempWorkspace);
			managedDir.Dispose();
			AssertEquals($"{nameof(ManagedDirectory.Dispose)} expected to  delete {nameof(ManagedDirectory)} directory, but it didn't.", expected: false, Directory.Exists(managedDir.FullPath));
		}

		public void TestManagedDirectory_Finalizer_DeletesDirectory()
		{
			var managedDir = new ManagedDirectory(tempWorkspace);
			var path = managedDir.FullPath;
			managedDir.Dispose();

			// Force garbage collection to invoke finalizer
			GC.Collect();
			GC.WaitForPendingFinalizers();

			AssertEquals($"Finalizer expected to delete {nameof(ManagedDirectory)} directory, but it didn't.", expected: false, Directory.Exists(path));
		}

		public void TestManagedDirectory_MultipleDispose_Calls_DoNotThrowException()
		{
			var managedDir = new ManagedDirectory(tempWorkspace);
			managedDir.Dispose();
			AssertNoExceptionThrown($"Expected running {nameof(ManagedDirectory.Dispose)} multiple times to not throw an exception.", () => managedDir.Dispose());
		}

		public void TestManagedDirectory_CreateMultipleDirectories_UniquePaths()
		{
			var dir1 = new ManagedDirectory(tempWorkspace);
			var dir2 = new ManagedDirectory(tempWorkspace);

			AssertNotEquals($"Creating multiple {nameof(ManagedDirectory)} items should lead to unique sub-directories, but it does not.", dir1.FullPath, dir2.FullPath);
		}

		public void TestManagedDirectory_FullPath_CorrectlyCombinesWithParentWorkspace()
		{
			var managedDir = new ManagedDirectory(tempWorkspace);
			AssertEquals("Provided workspace was not correctly combined with parent.", expected: true, managedDir.FullPath.StartsWith(tempWorkspace.RootDirectory));
		}

		public void TestManagedDirectory_ThrowsException_IfParentWorkspaceIsNull()
		{
			AssertExceptionThrown<ArgumentNullException>($"Expected that sending a null parameter to {nameof(ManagedDirectory)} would thrown a {nameof(ArgumentNullException)} exception.", () => new ManagedDirectory(null));
		}
	}
}
