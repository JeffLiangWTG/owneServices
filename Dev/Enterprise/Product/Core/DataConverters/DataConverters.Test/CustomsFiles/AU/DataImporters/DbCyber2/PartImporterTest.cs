using System.Data;
using CargoWise.Types;
using Enterprise.DataConverters.CustomsFiles.AU.DataImporters.DbCyber2;
using Enterprise.DataConverters.Testing.DataImporters;
using Enterprise.DataConverters.Testing.DataImporters.Testing;
using Enterprise.Environment;

namespace Enterprise.DataConverters.Testing.CustomsFiles.AU.DataImporters.DbCyber2
{
	class PartImporterTest : InterbaseImporterTestBase
	{
		public override void TestGetsFirstRecordFromSampleFileAndFillsItInCorrectly()
		{
			AssertEquals("DataImporter.ReadData(path)", true, Importer.ReadData());
			AssertEquals("Reader.HasRecordsNotProcessedYet after reading data", true, Importer.HasRecordsNotProcessedYet);

			var writer = (DataConverters.CustomsFiles.AU.PartWriter)Importer.GetNextDataWriter(Factory);

			AssertEquals("Buyer", writer.ImporterCode);
			AssertEquals("Supplier", writer.SupplierCode);
			AssertEquals("PartCode", writer.PartNumber);
			AssertEquals("Lookup", writer.LookupCode);
			AssertEquals(1.0m, writer.Weight);
			AssertEquals(1.0m, writer.Volume);
			AssertEquals("Description", writer.Description);

			AssertEquals("UQ", writer.DefaultStockUnit);
			AssertEquals(Env.Registry.PackageWeightUnit, writer.WeightUQ);
			AssertEquals(Env.Registry.PackageVolumeUnit, writer.VolumeUQ);
		}

		protected override int RecordCountInTestData
		{
			get { return 1; }
		}

		protected override ZString ExpectedSqlText
		{
			get { return new ZString("select AccountId, Supplier, PartNo, Description, UQ, UQ1, Lookup, P_group, Weight, Volume from CustPartRecord"); }
		}

		protected new PartImporterForTesting Importer
		{
			get { return (PartImporterForTesting)base.Importer; }
		}

		protected override DataImporter GetDataImporter(ProgressLogger logger)
		{
			return new PartImporterForTesting(logger, "", true, false);
		}

		#region class PartImporterForTesting
		sealed internal class PartImporterForTesting : PartDataImporter
		{
			public PartImporterForTesting(ProgressLogger logger, ZString dataSourcePath, bool excludeExistingRexords, ZBool cSV)
				: base(logger, "", false, cSV)
			{
			}

			protected override DataTable GetDataTableFromSqlText()
			{
				return new DataTableCreator().GetTable("Part");
			}
		}
		#endregion
	}
}
