using System;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Business.Testing
{
	sealed class FlatFileDataExporterTest : TestCaseWithFactory
	{
		public void TestIsExportOK()
		{
			Assert("Is Export OK should be true by default", Exporter.IsExportOK);
		}

		public void TestDeliverFile()
		{
			NotificationBuffer notification = new NotificationBuffer();

			Exporter.ValidToDeliver = false;
			Exporter.ExportData(CollectionReader, notification);
			Assert("Exported File should be empty", Exporter.ExportedFileForTesting.IsEmpty);

			Exporter.ValidToDeliver = true;
			Exporter.ExportData(CollectionReader, notification);
			try
			{
				Assert("Exported File should NOT be empty", !Exporter.ExportedFileForTesting.IsEmpty);
			}
			finally
			{
				if (File.Exists(Exporter.ExportedFileForTesting))
				{
					File.Delete(Exporter.ExportedFileForTesting);
				}
			}
		}

		public void TestConvertToIValueObjectsFiresUpdateEventAfterEachBizOExport()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();

			Exporter.ProgressChanged += new EventHandler<ProgressEventArgs>(Exporter_ProgressChanged);
			IValueObject valueObject = Exporter.ConvertToIValueObjectForTesting(org, Exporter.DataAdapterForTesting, 1, 2, new NotificationBuffer());
			AssertEquals(1, AddProgressCallCount);

			AddProgressCallCount = 0;
			valueObject = Exporter.ConvertToIValueObjectForTesting(org, Exporter.DataAdapterForTesting, 2, 2, new NotificationBuffer());
			AssertEquals(1, AddProgressCallCount);
		}

		int AddProgressCallCount;
		void Exporter_ProgressChanged(object sender, ProgressEventArgs e)
		{
			AddProgressCallCount++;
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestExportToFile()
		{
			Exporter.PromptForFilename += new FilenameEventHandler(Exporter_PromptForFilenameExportToFile);
			Exporter.Export(CollectionReader, new NotificationBuffer());
			CompareFiles(Exporter.ExportedFileForTesting);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAppendToAFileThatDoesNotExist()
		{
			ExportInstructions instructions = new ExportInstructions();
			instructions.SpecifiedFilename = "TempFile";
			instructions.BasePath = Temp.TempPath;
			FlatFileDataExporterForTestingAppendTrue exporter = new FlatFileDataExporterForTestingAppendTrue(Factory, instructions, instructions.SpecifiedFilename);
			string comparisonFileName = Path.Combine(BaseSourcePath, @"Enterprise\Product\Core\DataTransfer\DataTransfer.Test\FlatFile\TestFiles\OrgTestFile.csv");
			exporter.PromptForFilename += new FilenameEventHandler(Exporter_PromptForFilenameExportToFile);
			exporter.Export(CollectionReader, new NotificationBuffer());
			CompareFiles(instructions.OutputFile);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAppendToAnExistingFile()
		{
			ExportInstructions instructions = new ExportInstructions();
			instructions.FileExtension = FileExtensionType.Txt;
			string comparisonFileName = Path.Combine(BaseSourcePath, @"Enterprise\Product\Core\DataTransfer\DataTransfer.Test\FlatFile\TestFiles\OrgTestFile.csv");

			using (TempFile file = TempFile.New(Temp.TempPath, "txt"))
			{
				instructions.SpecifiedFilename = file.Filename;
				FlatFileDataExporterForTestingAppendTrue exporter = new FlatFileDataExporterForTestingAppendTrue(Factory, instructions, file.Filename);
				using (TextWriter writer = new StreamWriter(file.Filename))
				{
					writer.WriteLine("this is the first and only line so far");
					writer.Flush();
				}
				exporter.Export(CollectionReader, new NotificationBuffer());
				AssertEquals("Output File", ZString.Empty, instructions.OutputFile);

				using (TextReader exportedFile = new StreamReader(file.Filename))
				{
					AssertEquals("the first line should not have been overwritten", "this is the first and only line so far", exportedFile.ReadLine().Trim());
					using (TextReader comparisonFile = new StreamReader(comparisonFileName))
					{
						AssertEquals("should be same contents written out", comparisonFile.ReadToEnd().Trim(), exportedFile.ReadToEnd().Trim());
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAppendToAnExistingFileIsFalse()
		{
			ExportInstructions instructions = new ExportInstructions();
			instructions.FileExtension = FileExtensionType.Txt;

			using (TempFile file = TempFile.New(Temp.TempPath, "txt"))
			{
				instructions.SpecifiedFilename = file.Filename;
				FlatFileDataExporterForTesting exporter = new FlatFileDataExporterForTesting(Factory, instructions, file.Filename);
				using (TextWriter writer = new StreamWriter(file.Filename))
				{
					writer.WriteLine("this is the first and only line in the file");
					writer.Flush();
				}
				exporter.Export(CollectionReader, new NotificationBuffer());
				AssertEquals("there should be no outputfile because file already exists", ZString.Empty, instructions.OutputFile);
				CompareFiles(file.Filename);
			}
		}

		void CompareFiles(ZString exportedFileToCompare)
		{
			string comparisonFileName = Path.Combine(BaseSourcePath, @"Enterprise\Product\Core\DataTransfer\DataTransfer.Test\FlatFile\TestFiles\OrgTestFile.csv");

			using (TextReader exportedFile = new StreamReader(exportedFileToCompare))
			{
				using (TextReader comparisonFile = new StreamReader(comparisonFileName))
				{
					AssertEquals("should be same contents written out", comparisonFile.ReadToEnd().Trim(), exportedFile.ReadToEnd().Trim());
				}
			}
			DeleteIfExists(exportedFileToCompare);
		}

		public void TestExportToFileWithExceptionCleansUpTempFile()
		{
			FlatFileDataExporterForTestingThrowsException exporter = new FlatFileDataExporterForTestingThrowsException(Factory);
			string tempFileName = Temp.GetTempFileName();
			try
			{
				exporter.ExportToFile(null, new NotificationBuffer(), tempFileName);
			}
			catch
			{
				Assert("Temp file should be deleted", !File.Exists(tempFileName));
			}
			File.Delete(tempFileName);
		}

		public void TestTryDeleteFile()
		{
			FlatFileDataExporterForTestingThrowsException exporter = new FlatFileDataExporterForTestingThrowsException(Factory);
			NotificationBuffer buffer = new NotificationBuffer();
			string tempFile = Temp.GetTempFileName();
			FileStream stream = null;
			try
			{
				stream = File.Open(tempFile, FileMode.Open, FileAccess.Read, FileShare.Write);
				exporter.ExportToFile(null, buffer, tempFile);
			}
			catch (Exception e)
			{
				AssertEquals("NotImplementedException should be thrown, not IOException", typeof(NotImplementedException), e.GetType());
				AssertEquals("Test", e.Message);
				Assert("buffer.HasErrors", buffer.HasErrors);
				AssertContains("Buffer should have Lock notification", string.Format("The file {0} is locked by another process.", tempFile), buffer.AsString);
				Assert("Temp file shouldn't be deleted", File.Exists(tempFile));
			}
			finally
			{
				if (stream != null)
				{ stream.Close(); }
				File.Delete(tempFile);
			}
		}

		void Exporter_PromptForFilenameExportToFile(object sender, FilenameEventArgs e)
		{
			e.UnmappedFilename = Path.Combine(Temp.TempPath, "testing.txt");
		}

		public void TestConvertToIValueObject()
		{
			OrgHeader org = Collection[0];
			IValueObject valueObject = Exporter.ConvertToIValueObjectForTesting(org, Exporter.DataAdapterForTesting, 1, 1, new NotificationBuffer());

			AssertEquals("Should have created objects of the correct value object type", Exporter.DataAdapterForTesting.ValueObjectType, valueObject.GetType());
		}

		public void TestSaveFactory()
		{
			BusinessObject org = Exporter.Factory.New(typeof(OrgHeader));
			try
			{
				Exporter.Export(CollectionReader, new NotificationBuffer());
			}
			catch
			{
				Assert("Exported file should not exist.", !File.Exists(Exporter.ExportedFileForTesting));
			}

			org.Delete();
			try
			{
				Exporter.Export(CollectionReader, new NotificationBuffer());
				Assert("Exported file should exist.", File.Exists(Exporter.ExportedFileForTesting));
			}
			finally
			{
				DeleteTempFile(Exporter.ExportedFileForTesting);
			}
		}

		public void TestFileExtensionForExport()
		{
			AssertEquals("File extension should return whatever the file format says:", Exporter.FlatFileFormatForTesting.FileExtensionForExport, Exporter.FileExtensionType);
		}

		public void TestExportWithFileArgsEmpty()
		{
			PromptForFilenameCallCount = 0;
			Exporter.PromptForFilename += new FilenameEventHandler(Exporter_PromptForFilenameTestExportWithFileArgs);

			Exporter.Export(CollectionReader, new NotificationBuffer());
			AssertEquals("Prompt for filename made with default delivery options (File, no filename provided)", 1, PromptForFilenameCallCount);
			DeleteTempFile(Exporter.ExportedFileForTesting);
		}

		int PromptForFilenameCallCount;
		void Exporter_PromptForFilenameTestExportWithFileArgs(object sender, FilenameEventArgs e)
		{
			PromptForFilenameCallCount++;
		}

		public void TestExportWithEmailArgsEmpty()
		{
			FlatFileDataExporterUsingEmailForTesting exporter = new FlatFileDataExporterUsingEmailForTesting(Factory);
			PromptForEmailDetailsCallCount = 0;
			exporter.PromptForEmailDetails += new EmailExportInstructionsEventHandler(Exporter_PromptForEmailDetailsIncrementCount);

			exporter.Export(CollectionReader, new NotificationBuffer());
			AssertEquals("Prompt for email details with email instructions but no address", 1, PromptForEmailDetailsCallCount);
			DeleteTempFile(exporter.ExportedFileForTesting);
		}

		int PromptForEmailDetailsCallCount;
		void Exporter_PromptForEmailDetailsIncrementCount(object sender, EmailExportInstructionsEventArgs e)
		{
			PromptForEmailDetailsCallCount++;
		}

		class FlatFileDataExporterUsingEmailForTesting : FlatFileDataExporterForTesting
		{
			public FlatFileDataExporterUsingEmailForTesting(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			protected override void PopulateExportInstructions(ExportInstructions instructions, IFlatFileConverter converter)
			{
				instructions.MethodOfExport = ExportType.Email;
			}
		}

		public void TestEnglishDescription()
		{
			AssertEquals("english description from the test class", "Test Data", Exporter.EnglishDescription);
		}

		public void TestGetExportMethodHasDefinitionsForAllTypes()
		{
			ExportInstructions instructions = new ExportInstructions();

			Array values = Enum.GetValues(typeof(ExportType));

			foreach (ExportType type in values)
			{
				instructions.MethodOfExport = type;
				AssertNotNull("FlatFileDataExporter.GetExportMethod() returned null for Export Method '" + type.ToString() + "'. Change this method to include the new ExportMethod type.", Exporter.GetExportMethodForTesting(instructions, new NotificationBuffer()));
			}
		}

		public void TestGetExportMethod()
		{
			ExportInstructions instructions = new ExportInstructions();

			instructions.MethodOfExport = ExportType.File;
			AssertEquals("Should return a FileExport", typeof(FileExport), Exporter.GetExportMethodForTesting(instructions, new NotificationBuffer()).GetType());

			instructions.MethodOfExport = ExportType.Email;
			AssertEquals("Should return an EmailExport", typeof(EmailExport), Exporter.GetExportMethodForTesting(instructions, new NotificationBuffer()).GetType());

			instructions.MethodOfExport = ExportType.Ftp;
			AssertEquals("Should return an EmailExport", typeof(FtpExport), Exporter.GetExportMethodForTesting(instructions, new NotificationBuffer()).GetType());
		}

		public void TestConstructorWithInstructionsAndExportedFile()
		{
			string testFile = "TestFile";
			ExportInstructions instructions = new ExportInstructions();

			instructions.BasePath = "abc";
			instructions.FileExtension = FileExtensionType.Csv;
			instructions.SpecifiedFilename = "xyz";

			FlatFileDataExporterForTesting exporter = new FlatFileDataExporterForTesting(Factory, instructions, testFile);
			AssertEquals("ExportedFile should be TestFile", testFile, exporter.ExportedFileForTesting);
			AssertSame("Instructions should be the same", instructions, exporter.GetExportInstructionsTest);
			AssertEquals("BasePath", "abc", exporter.GetExportInstructionsTest.BasePath);
			AssertEquals("FileExtension", instructions.FileExtension, exporter.GetExportInstructionsTest.FileExtension);
			AssertEquals("Specified File Name", instructions.SpecifiedFilename, exporter.GetExportInstructionsTest.SpecifiedFilename);
		}

		public void TestGetExportInstructions()
		{
			ExportInstructions instructions = Exporter.GetExportInstructionsTest;
			AssertNotNull("should never be null", instructions);
			Exporter = new FlatFileDataExporterForTesting(Factory, instructions, instructions.OutputFile);
			AssertSame("Same instructions passed into the constructor should be returned", instructions, Exporter.GetExportInstructionsTest);
		}

		#region Setup

		FlatFileDataExporterForTesting Exporter;

		void DeleteTempFile(string filename)
		{
			if (File.Exists(filename))
			{
				File.Delete(filename);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			SetupCollections();

			Exporter = new FlatFileDataExporterForTesting(Factory);
		}

		void SetupCollections()
		{
			OrgHeader bestCo = Factory.New<OrgHeader>();
			bestCo.OH_FullName = "The Best Company Ever Pty Ltd";
			bestCo.OH_Code = "BESTCO";
			OrgAddress address = bestCo.MainAddress;
			address.OA_Address1 = "123 Some St";
			address.OA_City = "Alexandria";
			address.OA_PostCode = "2015";
			address.OA_State = "NSW";

			OrgContact contact = bestCo.Contacts.AddNew();
			contact.OC_ContactName = "Robert Smith";

			OrgHeader iceInc = Factory.New<OrgHeader>();

			iceInc.OH_FullName = "Ice cream Inc";
			iceInc.OH_Code = "ICEINC";
			address = iceInc.MainAddress;
			address.OA_Address1 = "Unit 60";
			address.OA_Address2 = "120 George St";
			address.OA_City = "Sydney";
			address.OA_PostCode = "2000";
			address.OA_State = "NSW";

			contact = iceInc.Contacts.AddNew();
			contact.OC_ContactName = "Elizabeth Jones";

			Factory.Save();

			Collection = new OrgHeaderCollection(Factory);
			Collection.Add(bestCo);
			Collection.Add(iceInc);

			CollectionReader = new CollectionWrapperBusinessObjectReader(Collection);
		}

		OrgHeaderCollection Collection;
		CollectionWrapperBusinessObjectReader CollectionReader;

		#endregion
	}
}
