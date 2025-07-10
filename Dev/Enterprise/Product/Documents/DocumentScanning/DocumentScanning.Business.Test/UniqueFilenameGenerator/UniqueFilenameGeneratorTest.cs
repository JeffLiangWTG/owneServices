using System.IO;
using CargoWise.IO;

namespace Enterprise.DocumentScanning.Business.Test
{
	sealed class UniqueFilenameGeneratorTest : TestCaseWithDocumentFactory
	{
		public void TestGetNewUniqueFilePath()
		{
			UniqueFilenameGenerator generator = new UniqueFilenameGenerator();
			string testFilenameOnly = "TestFile.txt";
			string newFilePath = generator.GetNewUniqueFilePath(TestDirectory, testFilenameOnly);
			AssertEquals("Should return original file plus path", Path.Combine(TestDirectory, testFilenameOnly), newFilePath);

			using (FileStream stream = File.Create(Path.Combine(TestDirectory, testFilenameOnly)))
			{
				newFilePath = generator.GetNewUniqueFilePath(TestDirectory, testFilenameOnly);
				AssertEquals("The new filepath should have a numeric suffix", Path.Combine(TestDirectory, "TestFile[2].txt"), newFilePath);
			}
		}

		public void TestGetNewUniqueFilePathWithLongPath()
		{
			var generator = new UniqueFilenameGenerator();
			var testFilename1 =
				"Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very  long namE.txt";
			var newFilePath1 = generator.GetNewUniqueFilePath(TestDirectory, testFilename1);
			Assert("The path should be too long", Path.Combine(TestDirectory, testFilename1).Length > UniqueFilenameGenerator.MaxPath);
			AssertEquals("New filepath length should be equal with MaxPath", newFilePath1.Length, UniqueFilenameGenerator.MaxPath);

			using (File.Create(newFilePath1))
			{
				var testFilename2 =
					"Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very  long namF.txt";
				var newFilePath2 = generator.GetNewUniqueFilePath(TestDirectory, testFilename2);
				Assert("The new filepath should have a numeric suffix of [2]", newFilePath2.Contains("[2]"));
				AssertEquals("New filepath length should be equal with MaxPath", newFilePath2.Length, UniqueFilenameGenerator.MaxPath);

				using (File.Create(newFilePath2))
				{
					var testFilename3 =
					"Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very  long namG.txt";
					var newFilePath3 = generator.GetNewUniqueFilePath(TestDirectory, testFilename3);
					Assert("The new filepath should have a numeric suffix of [3]", newFilePath3.Contains("[3]"));
					AssertEquals("New filepath length should be equal with MaxPath", newFilePath3.Length, UniqueFilenameGenerator.MaxPath);
				}
			}
		}

		public void TestGetNewUniqueFilePath_StripsOutIllegalCharacters()
		{
			UniqueFilenameGenerator generator = new UniqueFilenameGenerator();
			string testFilenameOnly = "TestFile *!? hello.txt";
			string newFilePath = generator.GetNewUniqueFilePath(TestDirectory, testFilenameOnly);
			AssertEquals("Should return original file plus path", Path.Combine(TestDirectory, "TestFile  !  hello.txt"), newFilePath);

			using (FileStream stream = File.Create(newFilePath))
			{
				newFilePath = generator.GetNewUniqueFilePath(TestDirectory, testFilenameOnly);
				AssertEquals("The new filepath should have a numeric suffix", Path.Combine(TestDirectory, "TestFile  !  hello[2].txt"), newFilePath);
			}
		}

		public void TestGetNewUniqueFilenameInCollection()
		{
			var generator = new UniqueFilenameGenerator();

			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				var samplePdf = resourceRetriever.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.Sample.PDF");
				StorageMain parent = MasterFactory.New<StorageMain>();
				StorageFile[] addedFiles = parent.Files.AddOrUpdateFromFilenames(FileAction.CreateNew, samplePdf);
				AssertEquals("Precondition: one file", 1, parent.Files.Count);

				// pass in the same data to force a new unique filename
				string filename = generator.GetNewUniqueFilenameInCollection(parent.Files, addedFiles[0].SC_FileName, addedFiles[0].SC_DataType);
				AssertEquals("File portion should the same except for suffix", addedFiles[0].SC_FileName + "[2]", Path.GetFileNameWithoutExtension(filename));
				AssertEquals("Extension portion should be the same", addedFiles[0].SC_DataType, Path.GetExtension(filename).Trim('.'));
				StorageFile[] moreAddedFiles = parent.Files.AddOrUpdateFromFilenames(FileAction.CreateNew, samplePdf);

				filename = generator.GetNewUniqueFilenameInCollection(parent.Files, moreAddedFiles[0].SC_FileName, moreAddedFiles[0].SC_DataType);
				AssertEquals("File portion should the same except for suffix", addedFiles[0].SC_FileName + "[3]", Path.GetFileNameWithoutExtension(filename));
				AssertEquals("Extension portion should be the same", addedFiles[0].SC_DataType.ToLower(), Path.GetExtension(filename).Trim('.').ToLower());
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			if (!Directory.Exists(TestDirectory))
			{
				Directory.CreateDirectory(TestDirectory);
			}
		}

		protected override void TearDown()
		{
			if (Directory.Exists(TestDirectory))
			{
				Directory.Delete(TestDirectory, true);
			}
			base.TearDown();
		}

		readonly string TestDirectory = Path.Combine(Temp.TempPath, "UniqueFilenameGeneratorTest");
	}
}
