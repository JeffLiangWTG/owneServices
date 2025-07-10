using Enterprise.DataConverters.CustomsFiles.NZ.DataImporters.Excel;
using Enterprise.DataConverters.Testing.DataImporters;
using ClassificationWriter = Enterprise.DataConverters.CustomsFiles.NZ.ClassificationWriter;

namespace Enterprise.DataConverters.Testing.CustomsFiles.NZ.DataImporters.Excel
{
	sealed internal class NZClassificationDataImporterTest : ExcelDataImporterTestBaseWithLogger
	{
		public override void TestGetsFirstRecordFromSampleFileAndFillsItInCorrectly()
		{
			var logger = new ProgressLogger();
			var importer = (ClassificationDataImporter)GetDataImporter(logger);
			AssertEquals("DataImporter.ReadData(path)", true, importer.ReadData());
			AssertEquals("Reader.HasRecordsNotProcessedYet after reading data", true, importer.HasRecordsNotProcessedYet);

			var writer = (ClassificationWriter)importer.GetNextDataWriter(Factory);
			AssertEquals("ONE", writer.LookupCode);
			AssertEquals("GEARS & GEARBOXS FOR USE WITH MOTORCYCLES OF 8711.90", writer.Description);
			AssertEquals("8483.40.20.00J", writer.TariffCode);
			AssertEquals(Customs.Common.ClassificationType.Both, writer.ClassificationType);
			AssertEquals("996257D", writer.ConcessionCode);
			AssertEquals("8711.90.00.00H", writer.PartsOfTariffCode);
			AssertEquals("PM1", writer.PermitCode1);
			AssertEquals("PC1", writer.PermitNumber1);
			AssertEquals("PM2", writer.PermitCode2);
			AssertEquals("PC2", writer.PermitNumber2);
			AssertEquals("PM3", writer.PermitCode3);
			AssertEquals("PC3", writer.PermitNumber3);
			AssertEquals("PH1", writer.ProhibitedCode1);
			AssertEquals("PH2", writer.ProhibitedCode2);
		}

		protected override DataImporter GetDataImporter(ProgressLogger logger)
		{
			return new ClassificationDataImporter(logger, PathForTestData, false);
		}

		protected override string PathForTestData
		{
			get { return base.PathForTestData + @"\Classification.CSV"; }
		}

		protected override int RecordCountInTestData
		{
			get { return 5; }
		}
	}
}
