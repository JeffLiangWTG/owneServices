using System;
using System.Globalization;
using CargoWise.Bi.Common;
using CargoWise.Bi.Configuration;
using NUnit.Framework;

namespace CargoWise.Bi.ConfigLoader.Testing
{
	class BiAutomationConfigDataSetValidationTest : TestCase
	{
		public void TestCdcTables()
		{
			var cdcTable = ConfigData.CdcTableConfig.AddCdcTableConfigRow("TestSchema", "TestCdcTable", "", "", "", false, false, null, null, null, false);

			cdcTable.AuditFilter = "Column = 'A'";
			CheckValidation("Audit Filter should only be for tables enabled for Audit.");

			cdcTable.EdwFilter = "Column = 'A'";
			CheckValidation("EDW Filter should only be for tables enabled for EDW.");

			cdcTable.TableInEdw = true;
			CheckValidation("Columns used to uniquely identify a row for net change tracking must be included in the list of captured columns. Enable Cdc for the primary key of the source table");

			var pk = ConfigData.CdcColumnConfig.AddCdcColumnConfigRow(cdcTable, "PK", "int", 0, 0, 0, false, true, null, null, true, "", "", true, true, "");
			CheckValidation("Primary key should be of data type 'uniqueidentifier'.");

			ConfigData.CdcColumnConfig.AddCdcColumnConfigRow(cdcTable, "PK2", "uniqueidentifier", 0, 0, 0, false, true, null, null, true, "", "", true, true, "");
			CheckValidation("Only one Primary Key allowed for each table.");

			pk.DataType = "uniqueidentifier";

			CheckValidation("Missing Indexed Column");

			cdcTable.IndexedColumn = "InvalidPk";
			CheckValidation("Indexed column is not part of the table schema.");

			var indexedColumn = ConfigData.CdcColumnConfig.AddCdcColumnConfigRow(cdcTable, "Index", "int", 0, 0, 0, true, true, null, null, true, "", "", true, true, "");
			cdcTable.IndexedColumn = "Index";
			CheckValidation("Indexed column cannot be nullable.");

			indexedColumn.Nullable = false;
			CheckValidation("Indexed column data type should be uniqueidentifier or datetime.");
		}

		public void TestCdcTableShouldEnablePKForCdc()
		{
			var errorMessage = "Columns used to uniquely identify a row for net change tracking must be included in the list of captured columns. Enable Cdc for the primary key of the source table";
			var cdcTable = ConfigData.CdcTableConfig.AddCdcTableConfigRow("TestSchema", "TestCdcTable", "", "", "", TableInAudit: true, TableInEdw: false, null, null, null, IsEdiClient: false);

			var fk = ConfigData.CdcColumnConfig.AddCdcColumnConfigRow(cdcTable, "FK", "uniqueidentifier", 0, 0, 0, Nullable: false, IsPrimaryKey: false, null, null, CdcEnabled: true, "", "", ColumnInAudit: true, ColumnInEdw: false, "");
			CheckValidation(errorMessage);

			var pk = ConfigData.CdcColumnConfig.AddCdcColumnConfigRow(cdcTable, "PK", "uniqueidentifier", 0, 0, 0, Nullable: false, IsPrimaryKey: true, null, null, CdcEnabled: false, "", "", ColumnInAudit: false, ColumnInEdw: false, "");
			CheckValidation(errorMessage);

			cdcTable.TableInAudit = false;
			cdcTable.TableInEdw = true;
			fk.ColumnInEdw = true;
			CheckValidation(errorMessage);

			fk.Delete();
			CheckValidation(errorMessage);
		}

		public void TestBaseTables()
		{
			var cdcTable = ConfigData.CdcTableConfig.AddCdcTableConfigRow("TestSchema", "TestCdcTable", "TestSchema", "", "", true, true, null, null, null, false);
			ConfigData.CdcColumnConfig.AddCdcColumnConfigRow(cdcTable, "Value", "varchar", 50, 0, 0, true, false, "", "", true, "", "", true, true, "");
			ConfigData.CdcColumnConfig.AddCdcColumnConfigRow(cdcTable, "XmlData", "xml", 50, 0, 0, true, false, "", "", true, "", "", true, true, "");

			var edwTable = ConfigData.EdwTableConfig.AddEdwTableConfigRow("TestSchema", "BAS__TestBaseTable", "", "TestSchema", "TestCdcTable", "", 1, false, 1, "");
			var edwColumn = ConfigData.EdwColumnConfig.AddEdwColumnConfigRow("BAS__TestBaseTable", 1, "varchar", 25, 0, 0, "BaseValue", "Value", false, false, "", "", "", "", "", "", false);
			CheckValidation("Max length does not match with the source column.");

			edwColumn.DataType = "nvarchar";
			CheckValidation("Data type does not match with the source column.");

			ConfigData.EdwColumnConfig.AddEdwColumnConfigRow("BAS__TestBaseTable", 1, "xml", 50, 0, 0, "XmlData", "XmlData", false, false, "", "", "", "", "", "", false);
			CheckValidation("XML data cannot be in EDW.");

			edwTable.StagingTable = "TableNotExisting";
			CheckValidation("Staging Table does not exist.");

			edwColumn.ParentColumn = "ColumnNotExisting";
			CheckValidation("Missing 'Parent Table'.");

			edwColumn.ParentTable = "TableNotExisting";
			edwColumn.ParentColumn = null;
			CheckValidation("Missing 'Parent Column'.");

			edwColumn.ParentColumn = "ColumnNotExisting";
			CheckValidation("Parent Table does not exist.");

			edwColumn.ParentTable = "BAS__TestBaseTable";
			CheckValidation("Parent Column does not exist");

			var customTable = ConfigData.EdwCustomTableConfig.AddEdwCustomTableConfigRow("TestSchema", "TestTable", 0, "", "", "", "", false, "");
			edwColumn.ParentTable = "TestTable";
			CheckValidation("Parent table is not a pre-transform custom table.");
			CheckValidation("Target Column should not be empty for references to custom tables.");

			edwColumn.KeepAs = "RefValue6";
			CheckValidation("Invalid Keep As value for the column.");

			var edwColumn2 = ConfigData.EdwColumnConfig.AddEdwColumnConfigRow("BAS__TestBaseTable", 1, "varchar", 25, 0, 0, "BaseValue", "Value", false, false, "", "", "", "", "", "RefValue1", false);
			edwColumn.KeepAs = "RefValue1";
			CheckValidation("Keep As value should be unique for a column in the table.");

			edwColumn.UsesFunction = true;
			CheckValidation("Column should not use function if it has a Parent Column.");
		}

		public void TestAggregateTables()
		{
			var aggTable = ConfigData.EdwDenormalizedTableConfig.AddEdwDenormalizedTableConfigRow("TestSchema", "AGG__TestAggTable", "(SELECT * FROM Test.BAS__TestBaseTable)", 1, false, "", "");
			CheckValidation("Subqueries are not supported.");

			aggTable.Expression = "Test.AGG__TestAnotherTable AS A INNER JOIN Test.AGG__TestAnotherTable AS B";
			CheckValidation("Aggregate Table expression can only use base tables.");

			aggTable.Expression = "Test.BAS__TestBaseTable AS A INNER JOIN Test.BAS__TestBaseTable AS B WITH (NOLOCK)";
			CheckValidation("Hints are not allowed. Remove WITH expression.");

			aggTable.Expression = "Test.BAS__TestBaseTable AS A INNER JOIN Test.BAS__TestBaseTable";
			CheckValidation("Missing alias for table [Test].[BAS__TestBaseTable].");

			aggTable.Expression = "Test.BAS__TestBaseTable AS A INNER JOIN Test.BAS__TestBaseTable AS B ON ISNULL(A.TestColumn, 0)";
			CheckValidation("Use CASE END for joins instead of ISNULL() or COALESCE.");

			aggTable.Expression = "Test.BAS__TestBaseTable AS A INNER JOIN Test.BAS__TestBaseTable AS B ON COALESCE(A.TestColumn, B.TestColumn)";
			CheckValidation("Use CASE END for joins instead of ISNULL() or COALESCE.");

			aggTable.Expression = "Test.BAS__TestBaseTable AS A INNER JOIN Test.BAS__TestBaseTable AS B";

			var aggColumn = ConfigData.EdwDenormalizedColumnConfig.AddEdwDenormalizedColumnConfigRow(aggTable, "TestColumn", "varchar", 20, 0, 0, "CAST(BaseValue AS DATE)", false, "", false);
			CheckValidation("Column expression should be created in base table level.");

			aggColumn.Expression = "CONVERT(DATE, BaseValue)";
			CheckValidation("Column expression should be created in base table level.");

			aggColumn.Expression = "LTRIM(RTRIM(BaseValue))";
			CheckValidation("Column expression should be created in base table level.");

			var edwTable = ConfigData.EdwTableConfig.AddEdwTableConfigRow("TestSchema", "BAS__TestBaseTable", "", "SrcSch", "TestCdcTable", "", 1, false, 1, "");
			var edwColumn = ConfigData.EdwColumnConfig.AddEdwColumnConfigRow("BAS__TestBaseTable", 1, "varchar", 20, 0, 0, "BaseValue", "Value", false, false, "", "", "", "", "", "", false);
			aggColumn.Expression = "A.BaseValue";
			aggColumn.DataType = "nvarchar";
			CheckValidation("Data type does not match with the source column.");

			aggColumn.DataType = "varchar";
			aggColumn.MaxLength = 10;
			CheckValidation("Max length does not match with the source column.");
		}

		public void TestCustomTables()
		{
			var customTable = ConfigData.EdwCustomTableConfig.AddEdwCustomTableConfigRow("TestSchema", "TestTable", 0, "", "", "", "", false, "");
			ConfigData.EdwCustomColumnConfig.AddEdwCustomColumnConfigRow(customTable, "PK", "uniqueidentifier", 0, 0, 0, "");
			CheckValidation("Dependency Order should be greater than 0.");

			customTable.DependencyOrder = 1;
			var customTable2 = ConfigData.EdwCustomTableConfig.AddEdwCustomTableConfigRow("TestSchema", "TestTable2", 1, "", "", "", "", false, "");
			ConfigData.EdwCustomColumnConfig.AddEdwCustomColumnConfigRow(customTable2, "PK", "uniqueidentifier", 0, 0, 0, "");
			CheckValidation("Dependency Order should be unique for all custom tables.");

			customTable.Name = BiConstants.EdwBaseTablePrefix + "TestTable";
			CheckValidation(String.Format(CultureInfo.InvariantCulture, "Name cannot start with '{0}'.", BiConstants.EdwBaseTablePrefix));

			customTable.Name = BiConstants.EdwAggregateTablePrefix + "TestTable";
			CheckValidation(String.Format(CultureInfo.InvariantCulture, "Name cannot start with '{0}'.", BiConstants.EdwAggregateTablePrefix));

			customTable.Name = BiConstants.EdwAggregateViewPrefix + "TestTable";
			CheckValidation(String.Format(CultureInfo.InvariantCulture, "Name cannot start with '{0}'.", BiConstants.EdwAggregateViewPrefix));

			customTable.Name = BiConstants.EdwModelViewPrefix + "TestTable";
			CheckValidation(String.Format(CultureInfo.InvariantCulture, "Name cannot start with '{0}'.", BiConstants.EdwModelViewPrefix));

			customTable.Name = "SUP__TestTable";
			customTable.RunBeforeTransform = true;
			customTable.InitialLoadQuery = "[TestSchema].[BAS__TestTable]";
			CheckValidation("Queries cannot use base tables if running before transform.");

			customTable.InitialLoadQuery = "[TestSchema].[AGG__TestTable]";
			CheckValidation("Queries cannot use aggregate tables.");
		}

		public void TestModelViews()
		{
			var modelView = ConfigData.EdwModelViewTableConfig.AddEdwModelViewTableConfigRow("TestSchema", "MDL__TestModelView", "Test.BAS__TestBaseTable WITH (NOLOCK)", "", "");
			CheckValidation("Hints are not allowed. Remove WITH expression.");

			modelView.Expression = "Test.BAS__TestBaseTable AS A";
			var modelViewColumn = ConfigData.EdwModelViewColumnConfig.AddEdwModelViewColumnConfigRow(modelView, "TestColumn", "CAST(Value AS DATE", "", "");
			CheckValidation("Column expression should be created in base table level.");

			modelViewColumn.Expression = "CONVERT(DATE, Value)";
			CheckValidation("Column expression should be created in base table level.");

			modelViewColumn.Expression = "LTRIM(RTRIM(Value))";
			CheckValidation("Column expression should be created in base table level.");
		}

		#region Implementation

		void CheckValidation(string expectedMessage)
		{
			try
			{
				ConfigData.Validate();
			}
			catch (BiConfigurationException ex)
			{
				Assert("Actual:\r\n" + ex.Message + "\r\n\r\nExpected:\r\n" + expectedMessage, ex.Message.ToUpperInvariant().Contains(expectedMessage.ToUpperInvariant()));
			}
		}

		protected override void TearDown()
		{
			ResetConfiguration();
			base.TearDown();
		}

		void ResetConfiguration()
		{
			BiAutomationConfigLoader.Instance.ResetConfiguration();
		}

		BiConfigurationData ConfigData
		{
			get
			{
				return BiAutomationConfigLoader.Instance.ConfigData;
			}
		}

		#endregion
	}
}
