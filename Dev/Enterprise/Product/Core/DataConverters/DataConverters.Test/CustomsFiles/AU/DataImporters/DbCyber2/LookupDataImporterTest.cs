using System.Data;
using CargoWise.Types;
using Enterprise.DataConverters.CustomsFiles.AU.DataImporters.DbCyber2;
using Enterprise.DataConverters.Testing.DataImporters;
using Enterprise.DataConverters.Testing.DataImporters.Testing;

namespace Enterprise.DataConverters.Testing.CustomsFiles.AU.DataImporters.DbCyber2
{
	class LookupDataImporterTest : InterbaseImporterTestBase
	{
		public void TestSetInstrumentCode()
		{
			AssertEquals("TC1", Importer.SetInstrumentType("tc"));
			AssertEquals("TC1", Importer.SetInstrumentType("TC"));
			AssertEquals("MD1", Importer.SetInstrumentType("md"));
			AssertEquals("MD1", Importer.SetInstrumentType("MD"));
			AssertEquals("OTHER", Importer.SetInstrumentType("other"));
		}

		public override void TestGetsFirstRecordFromSampleFileAndFillsItInCorrectly()
		{
			AssertEquals("DataImporter.ReadData(path)", true, Importer.ReadData());
			AssertEquals("Reader.HasRecordsNotProcessedYet after reading data", true, Importer.HasRecordsNotProcessedYet);

			var writer = (DataConverters.CustomsFiles.AU.ClassificationWriter)Importer.GetNextDataWriter(Factory);

			AssertEquals("TARIFFST", writer.TariffCode);
			AssertEquals("INTERBASELOOKUP", writer.LookupCode);
			AssertEquals("EXP", writer.ClassificationType);
			AssertEquals("TARIFFDESCRIPTION", writer.Description);
			AssertEquals("TREATMENT", writer.Treatment);
			AssertEquals("TC1", writer.InstrumentType);
			AssertEquals("INSTR", writer.InstrumentCode);
		}

		protected override ZString ExpectedSqlText
		{
			get { return "select code, tariff, statistic, treat, uq, instument, instr_type, description, add_info1, uq, by_law, drawback, ahecc from CustPartLookup"; }
		}

		protected override int RecordCountInTestData
		{
			get { return 1; }
		}

		protected new LookupImporterForTesting Importer
		{
			get { return (LookupImporterForTesting)base.Importer; }
		}

		protected override DataImporter GetDataImporter(ProgressLogger logger)
		{
			return new LookupImporterForTesting(logger, "", true, false);
		}

		#region class LookupImporterForTesting
		sealed internal class LookupImporterForTesting : LookupDataImporter
		{
			public LookupImporterForTesting(ProgressLogger logger, ZString path, ZBool exclude, ZBool toCSV) : base(logger, path, exclude, toCSV)
			{
			}

			protected override DataTable GetDataTableFromSqlText()
			{
				return new DataTableCreator().GetTable("Classification");
			}
		}
		#endregion
	}
}
