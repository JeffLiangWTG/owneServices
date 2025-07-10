using System.IO;
using CargoWise.Data;
using CargoWise.IO;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DataTools.DbBackupAndRestore.Business
{
	sealed class DbServerBrowserTest : TestCase
	{
		public void TestFixedDrives()
		{
			using (DbServerBrowserForTesting testBrowser = new DbServerBrowserForTesting(false))
			{
				AssertEquals("Should have at least 1 local hard drive", true, testBrowser.FixedDrives.Count > 0);
				AssertEquals("C: should be one the local hard drives", true, testBrowser.FixedDrives.Contains("C:"));
			}
		}

		public void TestGetDirectoryList()
		{
			using (TempDirectory tempDir = new TempDirectory())
			using (TempFile file1 = TempFile.NewInDirectory(tempDir.DirectoryName))
			using (TempFile file2 = TempFile.NewInDirectory(tempDir.DirectoryName))
			{
				string subDirName = "SubDirectory";
				Directory.CreateDirectory(Path.Combine(tempDir.DirectoryName, subDirName));

				using (DbServerBrowserForTesting testBrowser = new DbServerBrowserForTesting(false))
				{
					DirectoryEntryCollection directoryEntries = testBrowser.GetDirectoryList(tempDir.DirectoryName);
					AssertEquals("Number of files/directories in the list", 3, directoryEntries.Count);

					bool file1Found = false;
					bool file2Found = false;
					bool subDirFound = false;

					foreach (DirectoryEntry dirEntry in directoryEntries)
					{
						AssertEquals("Parent Directory", tempDir.DirectoryName, dirEntry.ParentDirectory);

						if (file1.Filename == dirEntry.FullPath)
						{
							file1Found = true;
							AssertEquals("Is File (File 1)", DirectoryEntryType.File, dirEntry.EntryType);
						}
						else if (file2.Filename == dirEntry.FullPath)
						{
							file2Found = true;
							AssertEquals("Is File (File 2)", DirectoryEntryType.File, dirEntry.EntryType);
						}
						else if (dirEntry.Name == subDirName)
						{
							subDirFound = true;
							AssertEquals("Is File (SubDir)", DirectoryEntryType.Folder, dirEntry.EntryType);
						}
					}

					AssertEquals("File 1 found", true, file1Found);
					AssertEquals("File 2 found", true, file2Found);
					AssertEquals("SubDir found", true, subDirFound);
				}

				// Browsing Folders Only
				using (DbServerBrowserForTesting testBrowser = new DbServerBrowserForTesting(true))
				{
					var directoryEntries = testBrowser.GetDirectoryList(tempDir.DirectoryName);
					AssertEquals("Number of files/directories in the list", 1, directoryEntries.Count);
					AssertEquals("Is File (SubDir - Folders Only)", DirectoryEntryType.Folder, directoryEntries[0].EntryType);
					AssertEquals("Parent Dirctory (SubDir - Folders Only)", tempDir.DirectoryName, directoryEntries[0].ParentDirectory);
					AssertEquals("Name (SubDir - Folders Only)", subDirName, directoryEntries[0].Name);
				}
			}
		}

		public void TestGetBrowseConnection()
		{
			using (DbConnection connection = DbServerBrowser.GetBrowseConnection(Db.ServerName))
			{
				AssertEquals(connection.ServerName, Db.ServerName);
				AssertEquals(connection.CurrentDatabase, Db.SqlMasterDb);
			}
		}
	}
}
