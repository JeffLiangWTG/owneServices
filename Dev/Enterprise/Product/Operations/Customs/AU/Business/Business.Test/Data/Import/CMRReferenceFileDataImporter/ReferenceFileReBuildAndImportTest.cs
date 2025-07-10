using System.Data;
using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class ReferenceFileReBuildAndImportTest : TestCaseWithFactory
	{
		public void TestMappingDataRow()
		{
			AssertNotNull("Mapping Data Row", builderTestClass.MappingDataRow);
		}

		public void TestGetDataItemColumnValue()
		{
			AssertEquals("Data Item", "VesselGrossWeight", builderTestClass.GetDataItemColumnValue(GetTableStructureRow()));
		}

		public void TestGetFormatColumnValue()
		{
			AssertEquals("Format", "Numeric", builderTestClass.GetFormatColumnValue(GetTableStructureRow()));
		}

		public void TestGetStartPositionColumnValue()
		{
			AssertEquals("Start Position", "21", builderTestClass.GetStartPositionColumnValue(GetTableStructureRow()));
		}

		public void TestGetLengthColumnValue()
		{
			AssertEquals("Length", "7", builderTestClass.GetLengthColumnValue(GetTableStructureRow()));
		}

		public void TestTablePrefix()
		{
			AssertEquals("Prefix", "PU_", builderTestClass.TablePrefix);
		}

		public void TestTableName()
		{
			AssertEquals("Table Name", CMRPreferenceRulePeriodSnapshotSchema.Constants.TableName, builderTestClass.TableName);
		}

		public void TestPKColumnName()
		{
			AssertEquals("PK Column Name", "PU_PK", builderTestClass.PKColumnName);
		}

		public void TestMappingsDataSet()
		{
			AssertNotNull("Data Set", builderTestClass.MappingsDataSet);
		}

		public void TestMappingsTable()
		{
			AssertNotNull("Mapping Table", builderTestClass.MappingsTable);
		}

		public void TestGetFilteredAndSortedDataSet()
		{
			DataRow[] rows = builderTestClass.GetFilteredAndSortedDataSet();
			AssertEquals("Number of rows", 13, rows.Length);

			AssertEquals("Row 1", "RuleType", rows[0][CMRReferenceFileBuilderConstants.TablesStructure.DataItemColumn].ToString());
			AssertEquals("Row 2", "PeriodIdentifier", rows[1][CMRReferenceFileBuilderConstants.TablesStructure.DataItemColumn].ToString());
			AssertEquals("Row 3", "CreationTimestamp", rows[2][CMRReferenceFileBuilderConstants.TablesStructure.DataItemColumn].ToString());
			AssertEquals("Row 4", "ConcessionalItemNumber", rows[3][CMRReferenceFileBuilderConstants.TablesStructure.DataItemColumn].ToString());
			AssertEquals("Row 5", "StartDate", rows[4][CMRReferenceFileBuilderConstants.TablesStructure.DataItemColumn].ToString());
			AssertEquals("Row 6", "EndDate", rows[5][CMRReferenceFileBuilderConstants.TablesStructure.DataItemColumn].ToString());
			AssertEquals("Row 7", "Sequence", rows[6][CMRReferenceFileBuilderConstants.TablesStructure.DataItemColumn].ToString());
			AssertEquals("Row 8", "LowerLocalContentPercentage", rows[7][CMRReferenceFileBuilderConstants.TablesStructure.DataItemColumn].ToString());
			AssertEquals("Row 9", "HigherLocalContentPercentage", rows[8][CMRReferenceFileBuilderConstants.TablesStructure.DataItemColumn].ToString());
			AssertEquals("Row 10", "CountryValidationType", rows[9][CMRReferenceFileBuilderConstants.TablesStructure.DataItemColumn].ToString());
			AssertEquals("Row 11", "TariffValidationType", rows[10][CMRReferenceFileBuilderConstants.TablesStructure.DataItemColumn].ToString());
			AssertEquals("Row 12", "Description", rows[11][CMRReferenceFileBuilderConstants.TablesStructure.DataItemColumn].ToString());
			AssertEquals("Row 13", "ActionIndicator", rows[12][CMRReferenceFileBuilderConstants.TablesStructure.DataItemColumn].ToString());
		}

		protected override void SetUp()
		{
			base.SetUp();
			builderTestClass = new ReferenceFileReBuildAndImportTestClass();
			builderTestClass.MappingDataRow = GetTableMappingRow();
		}
		ReferenceFileReBuildAndImportTestClass builderTestClass;

		DataRow GetTableMappingRow()
		{
			ZString filter = builderTestClass.MappingsTable.Columns[CMRReferenceFileBuilderConstants.TablesMappings.CustomsFileNameColumn] + " = 'PRRPSNAP'";
			DataRow[] filteredRows = builderTestClass.MappingsTable.Select(filter);
			return filteredRows.Length > 0 ? filteredRows[0] : null;
		}

		DataRow GetTableStructureRow()
		{
			DataTable table = builderTestClass.StructureDataSet.Tables[CMRReferenceFileBuilderConstants.TablesStructure.TableName];
			ZString filter = table.Columns[CMRReferenceFileBuilderConstants.TablesStructure.DataItemColumn] + " = 'VesselGrossWeight'";
			DataRow[] filteredRows = table.Select(filter);
			return filteredRows.Length > 0 ? filteredRows[0] : null;
		}

		sealed class ReferenceFileReBuildAndImportTestClass : ReferenceFileBuilder
		{
			public ReferenceFileReBuildAndImportTestClass()
				: base()
			{
			}

			public override Stream GetTableMappings()
				=> new MemoryStream(new EmbeddedResourceRetriever(GetType().Assembly).GetBytes("Enterprise.Customs.AU.Declaration.Business.Data.Import.CMRReferenceFileDataImporter.XMLTables.TableMappings.xml"));

			public override Stream GetTableStructure()
				=> new MemoryStream(new EmbeddedResourceRetriever(GetType().Assembly).GetBytes("Enterprise.Customs.AU.Declaration.Business.Data.Import.CMRReferenceFileDataImporter.XMLTables.TableStructure.xml"));
		}
	}
}
