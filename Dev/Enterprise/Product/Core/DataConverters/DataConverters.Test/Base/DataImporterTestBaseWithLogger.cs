using System.IO;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DataConverters.Testing.Base
{
	internal abstract class DataImporterTestBaseWithLogger : TestCaseWithFactory
	{
		public abstract void TestGetsFirstRecordFromSampleFileAndFillsItInCorrectly();

		public void TestHasHasRecordsNotProcessedYetIsFalseByDefault()
		{
			AssertEquals("HasRecordsNotPrcessedYet", false, Importer.HasRecordsNotProcessedYet);
		}

		public void TestRecordCount()
		{
			Importer.ReadData();
			AssertEquals("Reader.RecordCount", RecordCountInTestData, Importer.RecordCount);
			AssertEquals("Logger.RecordsToBeProcessed", RecordCountInTestData, Logger.RecordsToBeProcessed);
		}

		public void TestReadData()
		{
			AssertEquals("DataImporter.ReadData(path)", true, Importer.ReadData());
		}

		protected virtual string PathForTestData
		{
			get { return Path.Combine(NUnit.Framework.Dat.DatServerConnection.DatFileSharePath, @"TestFiles\DataConvertersTestData\Deliver\NZ"); }
		}

		protected abstract int RecordCountInTestData { get; }

		public void TestHasRecordsNotProcessedYet()
		{
			var counter = 0;
			AssertEquals("Reader.HasRecordsNotProcessedYet", false, Importer.HasRecordsNotProcessedYet);
			Importer.ReadData();
			AssertEquals("Reader.HasRecordsNotProcessedYet", true, Importer.HasRecordsNotProcessedYet);
			while (Importer.HasRecordsNotProcessedYet)
			{
				var writer = Importer.GetNextDataWriter(Factory);
				counter++;
			}
			AssertEquals("Records found using HasRecordsProcessedYet/GetNextClassification()", RecordCountInTestData, counter);
		}

		public void TestCreateCSVTemplate()
		{
			try
			{
				Importer.ImportToCSVFile = true;
				Importer.Import();

				Assert("Precondition", File.Exists(Importer.TemplateFile));
				using (var reader = new StreamReader(Importer.TemplateFile))
				{
					var line = reader.ReadLine();
					AssertEquals(line, Importer.CSVOutputHeader);
				}
			}
			finally
			{
				File.Delete(Importer.TemplateFile);
			}
		}

		[ExpectNoExceptions]
		public void TestSampleImplementation()
		{
			Importer.ReadData();

			while (Importer.HasRecordsNotProcessedYet)
			{
				var writer = Importer.GetNextDataWriter(Factory);
				//This is where you could do something....
			}
		}

		#region Logger
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
		ProgressLogger fLogger;
		#endregion

		#region Importer
		protected DataImporter Importer
		{
			get
			{
				if (fImporter == null)
				{
					fImporter = GetDataImporter(Logger);
				}
				return fImporter;
			}
		}
		DataImporter fImporter;
		#endregion

		protected abstract DataImporter GetDataImporter(ProgressLogger logger);
	}
}
