using System;
using NUnit.Framework;

namespace Enterprise.DataTools.DbBackupAndRestore.Business
{
	sealed class DirectoryEntryTest : TestCase
	{
		public void TestDirectoryEntryType()
		{
			AssertEquals("Number of Directory Entry Types", 2, Enum.GetValues(typeof(DirectoryEntryType)).Length);
			AssertEquals("Folder directory entry type", 0, (int)DirectoryEntryType.Folder);
			AssertEquals("File directory entry type", 1, (int)DirectoryEntryType.File);
		}
	}
}
