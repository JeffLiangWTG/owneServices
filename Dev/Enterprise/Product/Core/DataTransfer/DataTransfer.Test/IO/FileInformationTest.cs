using System.IO;
using CargoWise.Types;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.DataTransfer.IO.Testing
{
	sealed class FileInformationTest : FileSystemInformationTest
	{
		public void TestOpen()
		{
			Stream stream = FileInformation.Open();
			AssertNotNull(stream);
			stream.Close();
		}

		public void TestOpen_FileInUse()
		{
			Stream firstStream = FileInformation.Open();
			AssertNotNull(firstStream);

			Stream streamToSameFile = FileInformation.Open();
			AssertNull(streamToSameFile);

			Stream differentStreamToSameFile = FileInformation2.Open();
			AssertNull(differentStreamToSameFile);

			firstStream.Close();
		}

		[ExpectException(typeof(System.IO.IOException))]
		public void TestOpenThrowsOtherException()
		{
			FileInformation.FileSystemInfo = new FileSystemInfoClassThatThrowsIOException();
			FileInformation.Open();
		}

		public void TestExtension()
		{
			AssertEquals(".txt", FileInformation.Extension);
		}

		public void TestParentDirectory()
		{
			Assert(FileInformation.ParentDirectory.Exists);
			AssertEquals(Env.TempPath, FileInformation.ParentDirectory.FullName + "\\");
		}

		public void TestFileInfo()
		{
			AssertEquals("Should return a System.IO.FileInfo type", typeof(FileInfo), FileInformation.FileInfo.GetType());
			AssertEquals(FileInformation.Name, FileInformation.FileInfo.Name);
			AssertEquals(FileInformation.FullName, FileInformation.FileInfo.FullName);
		}

		#region FileSystemInfoTest

		[ExpectNoExceptions("Should only delete if file exists")]
		public override void TestDelete()
		{
			FileInformation.Delete();
			Assert("File has not been deleted when Delete() was called", !File.Exists(WholeFileUri));
			FileInformation.Delete();
		}

		public override void TestExists()
		{
			AssertEquals("File does not exist when it should.", File.Exists(WholeFileUri), FileInformation.Exists);

			File.Delete(WholeFileUri);

			AssertEquals("File exist when it should not exist.", !File.Exists(WholeFileUri), FileInformation.Exists);
		}

		public override void TestFullName()
		{
			Assert("The Full name ie the URI and Name of the file was not correct", ((ZString)FileInformation.FullName).Contains(WholeFileUri));
		}

		public override void TestName()
		{
			AssertEquals("The name of the file did not match.", FileSystemObjectName, FileInformation.Name);
		}

		string WholeFileUri;

		public override string FileSystemObjectName
		{
			get { return "TestFile_FileInfoTest.txt"; }
		}

		#endregion

		#region Setup

		protected override void SetUp()
		{
			base.SetUp();

			WholeFileUri = Path.Combine(Env.TempPath, FileSystemObjectName);
			DeleteIfExists(WholeFileUri);
			FileStream stream = File.Create(WholeFileUri);
			stream.Close();
			FileInformation = new FileInformationTestClass(WholeFileUri);
			FileInformation2 = new FileInformationTestClass(WholeFileUri);
		}

		protected override void TearDown()
		{
			DeleteIfExists(WholeFileUri);
			base.TearDown();
		}

		FileInformationTestClass FileInformation;
		FileInformationTestClass FileInformation2;

		class FileInformationTestClass : FileInformation
		{
			public FileInformationTestClass(string uri)
				: base(uri)
			{
			}

			public new FileSystemInfo FileSystemInfo
			{
				get { return base.FileSystemInfo; }
				set { base.FileSystemInfo = value; }
			}
		}

		#region FileSystemInfoClassThatThrowsIOException

		class FileSystemInfoClassThatThrowsIOException : FileSystemInfo
		{
			public override void Delete()
			{
			}

			public override bool Exists
			{
				get { return false; }
			}

			public override string Name
			{
				get { return string.Empty; }
			}

			public override string FullName
			{
				get
				{
					try
					{
						return string.Empty;
					}
					finally
					{
						throw new System.IO.IOException();
					}
				}
			}
		}

		#endregion

		#endregion
	}
}
