using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using static Enterprise.DocumentScanning.Business.Test.DirectoryPreviewHelpers;

namespace Enterprise.DocumentScanning.Business.Test
{
	[TestedType(typeof(DirectoryBusinessObject))]
	public class DirectoryPreviewBusinessObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDirectoryEndsInSlash()
		{
			var fileSystem = new MockFileSystem(@"C:\", @"C:\foo\"); // Testing how it handles filepaths, not actually hitting the file system

			AssertEquals("Directory should keep their final slash", @"C:\", new DirectoryBusinessObject(fileSystem, @"C:\").FullPath);
			AssertEquals("Directory should keep their final slash", @"C:\foo\", new DirectoryBusinessObject(fileSystem, @"C:\foo").FullPath);
		}

		public void TestDirectories()
		{
			var fileSystem = CreateMockTempDirTree();
			var rootDir = fileSystem.Roots.Single();

			AssertContainsDirectories("RootDir - Has two sub dirs", rootDir, "D11", "D12");
			AssertContainsDirectories("D11 - Only has one dir", rootDir.Directories["D11"], "D111");
			AssertContainsDirectories("D111 - Should be empty", rootDir.Directories["D11"].Directories["D111"]);
			AssertContainsDirectories("D12 - Only contains files", rootDir.Directories["D12"]);
		}

		public void TestFiles()
		{
			var fileSystem = CreateMockTempDirTree();
			var d1 = fileSystem.Roots.Single();

			AssertContainsFiles("D1 has three files", d1, "D1F1", "D1F2", "D1F3");

			var d11 = d1.Directories["D11"];
			AssertContainsFiles("D11 has three files", d11, "D11F1", "D11F2", "D11F3");

			var d111 = d11.Directories["D111"];
			AssertContainsFiles("D111 is empty", d111);

			var d12 = d1.Directories["D12"];
			AssertContainsFiles("D12 has two files", d12, "D12F1", "D12F2");
		}

		void AssertContainsDirectories(string message, DirectoryBusinessObject root, params string[] expectedDirectories)
		{
			var directoryNames = root.Directories.Cast<DirectoryBusinessObject>().Select(dir => dir.Name.ToString());
			AssertContainsExactElementsInAnyOrder(message, expectedDirectories, directoryNames);
		}

		void AssertContainsFiles(string message, DirectoryBusinessObject root, params string[] expectedFiles)
		{
			var fileNames = root.Files.Cast<FileBusinessObject>().Select(f => f.FileNameWithExtension);
			AssertContainsExactElementsInAnyOrder(message, expectedFiles, fileNames);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return CreateMockTempDirTree().Roots.Single();
		}
	}

	[TestedType(typeof(DirectoryBusinessObjectCollection))]
	public class DirectoryPreviewBusinessObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DirectoryBusinessObjectCollection>
	{
		MockFileSystem fileSystem;

		protected override Type GetExpectedCollectionType() => typeof(DirectoryBusinessObjectCollection);

		protected override DirectoryBusinessObjectCollection GetCollectionToTest()
		{
			fileSystem = CreateMockTempDirTree();
			return new DirectoryBusinessObjectCollection(fileSystem.Roots.Single());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DirectoryBusinessObject(fileSystem, "D1F4");
		}
	}
}
