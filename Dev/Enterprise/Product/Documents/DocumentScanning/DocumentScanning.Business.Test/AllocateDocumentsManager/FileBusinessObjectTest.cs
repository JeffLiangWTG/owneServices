using System;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using NUnit.Framework;
using static Enterprise.DocumentScanning.Business.Test.DirectoryPreviewHelpers;

namespace Enterprise.DocumentScanning.Business.Test
{
	[TestedType(typeof(FileBusinessObject))]
	public class FileBusinessObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDelete()
		{
			var tempFile = Temp.GetTempFileNameWithExtension("tmp");

			try
			{
				File.WriteAllText(tempFile, "Hello world");

				var fileSystem = new FileSystem(new[] { ".tmp" });
				var directoryBizo = new DirectoryBusinessObject(fileSystem, Path.GetDirectoryName(tempFile));
				var fileBizo = new FileBusinessObject(directoryBizo, Path.GetFileName(tempFile));

				Assert("PRE: File should exist", File.Exists(tempFile));

				Assert("We want to be able to delete through the preview window", fileBizo.CanDelete);
				fileBizo.Delete();

				Assert("The file should be deleted", !File.Exists(tempFile));
			}
			finally
			{
				DeleteIfExists(tempFile);
			}
		}

		public void TestFileProperties()
		{
			CombineAssertions(() =>
			{
				var fileSystem = new FileSystem(new[] { "txt" });
				var directory = new DirectoryBusinessObject(fileSystem, @"some\path\to\something");
				var file = new FileBusinessObject(directory, "foo.txt");

				AssertEquals("Filename", "foo.txt", file.FileNameWithExtension);
				AssertEquals("Extension", "txt", file.Extension);
				AssertEquals("Full Path", @"some\path\to\something\foo.txt", file.FullPath);
				AssertEquals("Directory", directory, file.Directory);
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var fileSystem = new FileSystem(new[] { "txt" });
			var directory = new DirectoryBusinessObject(fileSystem, @"some\path\to\something");
			return new FileBusinessObject(directory, "foo.txt");
		}
	}

	[TestedType(typeof(FileBusinessObjectCollection))]
	public class FileBusinessObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<FileBusinessObjectCollection>
	{
		public void TestExtensionsFilter()
		{
			var tempDir = Temp.GetNewTempSubdirectory();
			try
			{
				var expectedFiles = new[] { "bitmap.bmp", "image.jpeg", "CAPITALISED.BMP", "adobe.pdf" };
				var unexpectedFiles = new[] { "badExtension.png.txt", "textfile.txt", "random.zzx" };
				CreateFolderWithFiles(tempDir, expectedFiles.Concat(unexpectedFiles).ToArray());

				var validExtensions = new[] { "bmp", "jpeg", "png", "pdf" };
				var fileSystem = new FileSystem(validExtensions);
				var files = new FileBusinessObjectCollection(new DirectoryBusinessObject(fileSystem, tempDir));
				files.Load();

				AssertEquals("Should only contain files that match the filter", 4, files.Count);
				AssertContainsExactElementsInAnyOrder("We should only contain files that match the filter", expectedFiles, files.Cast<FileBusinessObject>().Select(f => f.FileNameWithExtension));
			}
			finally
			{
				Directory.Delete(tempDir, recursive: true);
			}
		}

		#region Housekeeping

		protected override Type GetExpectedCollectionType() => typeof(FileBusinessObjectCollection);

		MockFileSystem fileSystem;

		protected override FileBusinessObjectCollection GetCollectionToTest()
		{
			fileSystem = CreateMockTempDirTree();

			return new FileBusinessObjectCollection(fileSystem.Roots.Single());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var directory = fileSystem.Roots.Single();
			return new FileBusinessObject(directory, "D1F5");
		}

		#endregion
	}
}
