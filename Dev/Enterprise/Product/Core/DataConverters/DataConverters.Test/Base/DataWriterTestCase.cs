using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;

namespace Enterprise.DataConverters.Testing.Base
{
	internal abstract class DataWriterTestCase : TestCaseWithFactory
	{
		public abstract void TestAllFieldsInRecordAreImportedProperly();

		public void TestSaveRecordToEnterprise()
		{
			AssertEquals("Precondition: Logger.RecordsCreated", 0, Logger.RecordsCreated);
			AssertEquals("Precondition: Logger.RecordsExcluded", 0, Logger.RecordsExcluded);
			AssertEquals("Precondition: Logger.RecordsUpdated", 0, Logger.RecordsUpdated);

			var writer = GetNewDataWriter();
			FillInRecordWithUniqueAndCompleteDetails(writer);
			writer.SaveRecordToEnterprise(false, Logger);
			AssertEquals("Precondition: Logger.RecordsCreated", 1, Logger.RecordsCreated);
			AssertEquals("Precondition: Logger.RecordsExcluded", 0, Logger.RecordsExcluded);
			AssertEquals("Precondition: Logger.RecordsUpdated", 0, Logger.RecordsUpdated);

			writer = GetNewDataWriter();
			FillInRecordWithUniqueAndCompleteDetails(writer);
			writer.SaveRecordToEnterprise(true, Logger);
			AssertEquals("Precondition: Logger.RecordsCreated", 1, Logger.RecordsCreated);
			AssertEquals("Precondition: Logger.RecordsExcluded", 1, Logger.RecordsExcluded);
			AssertEquals("Precondition: Logger.RecordsUpdated", 0, Logger.RecordsUpdated);

			writer = GetNewDataWriter();
			FillInRecordWithUniqueAndCompleteDetails(writer);
			writer.SaveRecordToEnterprise(false, Logger);
			AssertEquals("Precondition: Logger.RecordsCreated", 1, Logger.RecordsCreated);
			AssertEquals("Precondition: Logger.RecordsExcluded", 1, Logger.RecordsExcluded);
			AssertEquals("Precondition: Logger.RecordsUpdated", 1, Logger.RecordsUpdated);
		}

		public void TestSaveRecordToEnterpriseWithReasonRecordShouldBeExcluded()
		{
			var writer = GetNewDataWriter();
			FillInRecordWithInvalidDetails(writer);
			writer.SaveRecordToEnterprise(true, Logger);
			AssertEquals("Excluded " + writer.RecordDescription + " - " + writer.GetAnyReasonRecordShouldBeExcluded(), Logger[0]);
		}

		public void TestAddRecordToCSVFile()
		{
			var writer = GetNewDataWriter();
			FillInRecordWithUniqueAndCompleteDetails(writer);
			ZString templateFile = Env.GetTempFileName(Env.TempPath, "csv");
			try
			{
				writer.AddRecordToCSVFile(templateFile, Logger);
				var currentLine = "";
				using (var sw = new StreamReader(templateFile))
				{
					currentLine = sw.ReadLine();
				}
				AssertNotNull(currentLine);
				AssertEquals(currentLine, writer.CSVOutputLine);
			}
			finally
			{
				if (File.Exists(templateFile))
				{
					File.Delete(templateFile);
				}
			}
		}

		public void TestRemoveComma()
		{
			var writer = GetNewDataWriter();
			AssertEquals("Text1 Text2", writer.RemoveComma("Text1, Text2"));
			AssertEquals("TextA \\TextB", writer.RemoveComma("TextA \\TextB"));
		}

		protected abstract DataWriter GetNewDataWriter();
		protected abstract void FillInRecordWithUniqueAndCompleteDetails(DataWriter writer);
		protected abstract void FillInRecordWithInvalidDetails(DataWriter writer);

		ProgressLogger fLogger;
		protected ProgressLogger Logger
		{
			get
			{
				if (fLogger == null)
				{
					fLogger = new ProgressLogger();
				}
				return fLogger;
			}
		}
	}
}
