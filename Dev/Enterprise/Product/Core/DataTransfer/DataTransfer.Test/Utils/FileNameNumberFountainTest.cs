using System.IO;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Environment;

namespace Enterprise.DataTransfer.Business.Testing
{
	sealed class FileNameNumberFountainTest : FileNameNumberFountainTestCase
	{
		public void TestPeekFileName()
		{
			ZString fileName1 = FileNameNumberFountainTestClass.PeekNewFileName();
			ZString fileName2 = FileNameNumberFountainTestClass.PeekNewFileName();
			AssertEquals("File name should be the same", fileName1, fileName2);
		}

		public void TestGetFileName()
		{
			ZString nextFountainNumber = FileNameNumberFountainInstance.FileID.PeekPreliminaryFormatted(Db.Connection);

			ZString testFilePath = FileNameNumberFountainTestClass.GetNewFileName();
			Assert("Shouldn't be empty", !testFilePath.IsEmpty);
			Assert("should be next number in sequence", testFilePath.EndsWith(nextFountainNumber));

			ZString testFilePath2 = FileNameNumberFountainTestClass.GetNewFileName();
			Assert("Shouldn't equal first FileName", testFilePath != testFilePath2);
		}

		public void TestFileAlreadyExists()
		{
			ZString fileCreated = ZString.Empty;
			ZString testFilePath1 = FileNameNumberFountainTestClass.PeekNewFileName();
			FileStream stream = File.Create(testFilePath1);
			stream.Close();

			try
			{
				fileCreated = FileNameNumberFountainTestClass.GetNewFileName();
				AssertEquals("File created should be empty since already exists", ZString.Empty, fileCreated);
			}
			finally
			{
				DeleteIfExists(testFilePath1);
				DeleteIfExists(fileCreated);
			}
		}

		#region Overrides

		protected override int FileIDLength
		{
			get { return 1; }
		}

		protected override FileNameNumberFountain FileNameNumberFountainInstance
		{
			get { return FileNameNumberFountainTestClass.New(); }
		}

		protected override long MaxValue
		{
			get { return 99999; }
		}

		protected override void SetUp()
		{
			base.SetUp();
			FileNameNumberFountainTestClass.Initialise();
		}

		#endregion

		#region Implementation

		class FileNameNumberFountainTestClass : FileNameNumberFountain
		{
			protected FileNameNumberFountainTestClass()
			{
			}

			public static FileNameNumberFountain NewDelegate()
			{
				return new FileNameNumberFountainTestClass();
			}

			public static void Initialise()
			{
				OverridableNewDelegate.Value = new ConstructorDelegate(NewDelegate);
			}

			protected override ZString GenerateFilename(IDbConnected connected, bool progressNumber)
			{
				return Path.Combine(Env.TempPath, base.GenerateFilename(connected, progressNumber));
			}
		}

		#endregion
	}
}
