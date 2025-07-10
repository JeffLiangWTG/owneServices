using System.IO;
using CargoWise.Types;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.DataTransfer.IO.Testing
{
	sealed class DirectoryInformationTest : FileSystemInformationTest
	{
		[ExpectNoExceptions]
		public void TestRecursiveDelete()
		{
			DirectoryInformation.Delete(true);
			Assert(!DirectoryInformation.Exists);
		}

		#region TestGetFiles

		public void TestGetFiles()
		{
			try
			{
				FileInformation[] files = DirectoryInformation.GetFiles();
				SetFileFounds(files, ref FoundFile1, ref FoundFile2, ref FoundFile3, ref FoundFile4, ref FoundFile5);
			}
			finally
			{
				Assert("Did not find expected file", FoundFile1);
				Assert("Did not find expected file", FoundFile2);
				Assert("Did not find expected file", FoundFile3);
				Assert("Did not find expected file", FoundFile4);
				Assert("Did not find expected file", FoundFile5);
			}
		}

		public void TestGetFilesUsingSearchPattern()
		{
			try
			{
				FileInformation[] files = DirectoryInformation.GetFiles("*.csv");
				SetFileFounds(files, ref FoundFile1, ref FoundFile2, ref FoundFile3, ref FoundFile4, ref FoundFile5);
			}
			finally
			{
				Assert("Found a file that doesnt match the search pattern", !FoundFile1);
				Assert("Did not find expected file", FoundFile2);
				Assert("Found a file that doesnt match the search pattern", !FoundFile3);
				Assert("Did not find expected file", FoundFile4);
				Assert("Found a file that doesnt match the search pattern", !FoundFile5);
			}
		}

		public void TestGetFilesThatAreNotAlreadyAccessed()
		{
			FileInformation fileInformation = new FileInformation(File3);
			using (Stream stream = fileInformation.Open())
			{
				AssertNotNull("This stream should be accessing the File3", stream);

				try
				{
					FileInformation[] files = DirectoryInformation.GetFiles();
					SetFileFounds(files, ref FoundFile1, ref FoundFile2, ref FoundFile3, ref FoundFile4, ref FoundFile5);
					AssertEquals("4 Files were not returned", 4, files.Length);
				}
				finally
				{
					Assert("Did not find expected file", FoundFile1);
					Assert("Did not find expected file", FoundFile2);
					Assert("Found a non-accessable file", !FoundFile3);
					Assert("Did not find expected file", FoundFile4);
					Assert("Did not find expected file", FoundFile5);
				}
			}
		}

		#endregion

		#region FileSystemInfoTest

		public override void TestDelete()
		{
			DirectoryInformation.Delete(true);
			Assert("Directory has not been deleted when Delete() was called", !Directory.Exists(WholeFileUri));
		}

		public override void TestExists()
		{
			AssertEquals("Directory does not exist when it should.", Directory.Exists(WholeFileUri), DirectoryInformation.Exists);
			Directory.Delete(WholeFileUri, true);
			AssertEquals("Directory exist when it should not exist.", !Directory.Exists(WholeFileUri), DirectoryInformation.Exists);
		}

		public override void TestFullName()
		{
			Assert("The Full name ie the URI and Name of the Directory was not correct", ((ZString)DirectoryInformation.FullName).Contains(WholeFileUri));
		}

		public override void TestName()
		{
			AssertEquals("The name of the Directory did not match.", FileSystemObjectName, DirectoryInformation.Name);
		}

		string WholeFileUri;

		public override string FileSystemObjectName
		{
			get { return "TestFile_DirectoryInfoTest"; }
		}

		#endregion

		#region Helpers

		void CreateFiles()
		{
			try
			{
				File.Create(File1).Close();
				File.Create(File2).Close();
				File.Create(File3).Close();
				File.Create(File4).Close();
				File.Create(File5).Close();
			}
			catch (System.IO.IOException) { }
		}

		void DeleteFiles()
		{
			try
			{
				DeleteIfExists(File1);
				DeleteIfExists(File2);
				DeleteIfExists(File3);
				DeleteIfExists(File4);
				DeleteIfExists(File5);
			}
			catch (System.IO.IOException) { }
		}

		void SetFileFounds(FileInformation[] files, ref bool foundFile1, ref bool foundFile2, ref bool foundFile3, ref bool foundFile4, ref bool foundFile5)
		{
			foreach (FileInformation fileInformation in files)
			{
				if (fileInformation.FullName == File1)
				{
					foundFile1 = true;
				}
				else if (fileInformation.FullName == File2)
				{
					foundFile2 = true;
				}
				else if (fileInformation.FullName == File3)
				{
					foundFile3 = true;
				}
				else if (fileInformation.FullName == File4)
				{
					foundFile4 = true;
				}
				else if (fileInformation.FullName == File5)
				{
					foundFile5 = true;
				}
			}
		}

		#endregion

		#region Setup

		protected override void SetUp()
		{
			base.SetUp();

			WholeFileUri = Path.Combine(Env.TempPath, FileSystemObjectName);

			if (Directory.Exists(WholeFileUri))
			{
				Directory.Delete(WholeFileUri, true);
			}
			DirectoryInfo directoryInfo = Directory.CreateDirectory(WholeFileUri);
			DirectoryInformation = new DirectoryInformation(WholeFileUri);

			File1 = Path.Combine(DirectoryInformation.FullName, "File1.txt");
			File2 = Path.Combine(DirectoryInformation.FullName, "File2.csv");
			File3 = Path.Combine(DirectoryInformation.FullName, "File3.txt");
			File4 = Path.Combine(DirectoryInformation.FullName, "File4.csv");
			File5 = Path.Combine(DirectoryInformation.FullName, "File5.txt");

			DeleteFiles();
			CreateFiles();
		}

		protected override void TearDown()
		{
			if (Directory.Exists(WholeFileUri))
			{
				Directory.Delete(WholeFileUri, true);
			}

			base.TearDown();
		}

		string File1;
		string File2;
		string File3;
		string File4;
		string File5;

		bool FoundFile1;
		bool FoundFile2;
		bool FoundFile3;
		bool FoundFile4;
		bool FoundFile5;

		DirectoryInformation DirectoryInformation;

		#endregion
	}
}
