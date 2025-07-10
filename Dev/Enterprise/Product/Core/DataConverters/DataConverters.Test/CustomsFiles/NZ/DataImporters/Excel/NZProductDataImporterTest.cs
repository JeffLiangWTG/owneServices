using Enterprise.DataConverters.CustomsFiles.NZ.DataImporters.Excel;
using Enterprise.DataConverters.Testing.DataImporters;
using PartWriter = Enterprise.DataConverters.CustomsFiles.NZ.PartWriter;

namespace Enterprise.DataConverters.Testing.CustomsFiles.NZ.DataImporters.Excel
{
	sealed internal class NZProductDataImporterTest : ExcelDataImporterTestBaseWithLogger
	{
		public override void TestGetsFirstRecordFromSampleFileAndFillsItInCorrectly()
		{
			var logger = new ProgressLogger();
			var importer = (ProductDataImporter)GetDataImporter(logger);
			AssertEquals("DataImporter.ReadData(path)", true, importer.ReadData());
			AssertEquals("Reader.HasRecordsNotProcessedYet after reading data", true, importer.HasRecordsNotProcessedYet);

			var writer = (PartWriter)importer.GetNextDataWriter(Factory);

			AssertEquals("", writer.DeliveranceSystemIDCode);
			AssertEquals("PARTNUMBER", writer.PartNumber);
			AssertEquals("DESCRIPTION", writer.Description);
			AssertEquals("LOOKUP", writer.LookupCode);
			AssertEquals("SUPPLIER", writer.SupplierCode);
			AssertEquals("IMPORTER", writer.ImporterCode);
			AssertEquals("UQ", writer.DefaultStockUnit);
			AssertEquals(22m, writer.Weight);
			AssertEquals(Core.Constants.Weight.Kilograms, writer.WeightUQ);
			AssertEquals(0.11m, writer.Volume);
			AssertEquals(Core.Constants.Volume.CubicMetres, writer.VolumeUQ);
		}

		protected override DataImporter GetDataImporter(ProgressLogger logger)
		{
			return new ProductDataImporter(logger, PathForTestData, false);
		}

		protected override string PathForTestData
		{
			get { return base.PathForTestData + @"\Product.CSV"; }
		}

		protected override int RecordCountInTestData
		{
			get { return 5; }
		}
	}
}
