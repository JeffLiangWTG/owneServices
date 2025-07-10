using System;
using Enterprise.DataTools.DbBackupAndRestore.Business;
using NUnit.Framework;

namespace Enterprise.DataTools.DbBackupAndRestore.GUI
{
	sealed class DbServerBrowseFormTest : TestCase
	{
		public void TestImageList()
		{
			using var testForm = new DbServerBrowseForm();
			var testImages = testForm.DirectoryTreeViewImageListForTesting.Images;

			AssertEquals(
				"Image List count should be the same as the number of Directory Entry Types",
				Enum.GetValues(typeof(DirectoryEntryType)).Length, testImages.Count);

			AssertEquals("Image List count", 2, testImages.Count);
			AssertEquals("Folder image name", "Folder.ico", testImages.Keys[(int)DirectoryEntryType.Folder]);
			AssertEquals("File image name", "File.ico", testImages.Keys[(int)DirectoryEntryType.File]);
		}
	}
}
