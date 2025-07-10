using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Model.Finance;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Model.Finance.Testing
{
	[TestedType(typeof(vw_CUS__TaxGLMovementInfo))]
	internal class vw_CUS__TaxGLMovementInfoTest : BiCreateScriptTest
	{
		public void TestColumns()
		{
			var columns = new[]
			{
				"TaxGLMovementInfoKey",
				"TaxGLMovementKey",
				"TaxHeaderDescription",
				"TaxLineDescription",
				"TransactionTypeCode",
				"LedgerCode",
				"InvoiceDateTime",
				"DueDateTime",
				"GLTransactionHeaderID",
				"GLTransactionHeaderKey",
				"TransactionNo"
			};

			var result = SelectRows();
			foreach (string column in columns)
			{
				Assert($"{column} should be contained", result.Columns.Contains(column));
			}
			AssertEquals(columns.Length, result.Columns.Count);
		}

		public void TestViewData()
		{
			var result = SelectRows("vw_CUS__TaxGLMovementInfo");
			AssertInitialData(result);
		}

		public void TestIniLoad()
		{
			var result = SelectRows();
			AssertInitialData(result);
		}

		public void TestIncLoad_InsertTaxGLMovement()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, $@"
				INSERT [{ScriptDbName}].[Finance].[BAS__GLTransactionHeader]
					([GLTransactionHeaderKey], [GLTransactionHeaderID], [CompanyKey],  [LocalAmount], [TransactionType], [TransactionTypeCode], [LedgerCode], [PostDateTime],  [OrganizationHeaderKey], [CreateDateTimeUtc], [ComplianceSubType], [TransactionNo], [TransactionReference], [BranchKey], [InvoiceDateTime], [DueDateTime], [Description], [TransactionCategory], [TransactionBelongsToGroup], [IsCancelled])
				VALUES
					(10, '333F1051-BC83-47EB-8419-3B342D5F27D8', 1, 400.00, 'Invoice', 'INV', 'AR', '2023-08-10', 1 ,'2023-08-10', '', '66', 'ABC124', 1, '2023-08-10', '2023-08-11', 'desc3', 'FIN', NULL, 0),
					(11, newid(), 1, 400.00, 'Invoice', 'INV', 'AR', '2023-08-10', 1 ,'2023-08-10', '', '778', 'ABC124', 1, '2023-08-12', '2023-08-13', 'desc4', 'FIN', NULL, 0)

				INSERT [{ScriptDbName}].[Finance].[BAS__TaxGLMovement]
					([TaxGLMovementKey],[TaxTransactionKey],[TaxGLMovementID], [Amount])
				VALUES
					(2, 3, newid(), 300)

				INSERT [{ScriptDbName}].[Finance].[BAS__TaxTransaction]
					([TaxTransactionKey], [GLTransactionHeaderKey], [MatchTransactionHeaderID], [MatchTransactionHeaderKey], [LedgerCode], [TaxSystemCode],[TaxTransactionID])
				VALUES
					(3, 10, newid(), 11, 'AP', 'ASD', newid())

				INSERT INTO[{ScriptDbName}].[biadmin].[TransformedRow]
					(SchemaName, TableName, KeyValue)
				VALUES
					('Finance', 'BAS__TaxGLMovement', 1),
					('Finance', 'BAS__TaxGLMovement', 2)
				"
			);
			TestConnection.ExecuteNonQuery(sqlText);
			ExecuteCusTableLoad("IncrementalLoadQuery");
			var result = SelectRows();

			AssertEquals("Rowcount", 2, result.Rows.Count);
			AssertEquals(1, result.Select("TaxGLMovementKey = 1 and TaxHeaderDescription = 'desc1' and TaxLineDescription = 'DUU AP FIN Invoice 115' and TransactionTypeCode = 'INV' and LedgerCode = 'AP' and InvoiceDateTime = '2023-08-10' and DueDateTime = '2023-08-11' and GLTransactionHeaderID = '402F1051-BC83-47EB-8419-3B342D5F27D8' and GLTransactionHeaderKey = 8 and TransactionNo = '34'").Length);
			AssertEquals(1, result.Select("TaxGLMovementKey = 2 and TaxHeaderDescription = 'desc3' and TaxLineDescription = 'ASD AP FIN Invoice 778' and TransactionTypeCode = 'INV' and LedgerCode = 'AP' and InvoiceDateTime = '2023-08-10' and DueDateTime = '2023-08-11' and GLTransactionHeaderID = '333F1051-BC83-47EB-8419-3B342D5F27D8' and GLTransactionHeaderKey = 10 and TransactionNo = '66'").Length);
			AssertTransformedRow();
		}

		public void TestIncLoad_UpdateTaxHeaderDesc()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, $@"
				UPDATE [{ScriptDbName}].[Finance].[BAS__GLTransactionHeader] SET Description = 'DDD' WHERE GLTransactionHeaderKey = 8

				INSERT INTO[{ScriptDbName}].[biadmin].[TransformedRow]
					(SchemaName, TableName, KeyValue)
				VALUES
					('Finance', 'BAS__GLTransactionHeader', 8)
				"
			);
			TestConnection.ExecuteNonQuery(sqlText);
			ExecuteCusTableLoad("IncrementalLoadQuery");
			var result = SelectRows();

			AssertEquals("Rowcount", 1, result.Rows.Count);
			AssertEquals(1, result.Select("TaxGLMovementKey = 1 and TaxHeaderDescription = 'DDD' and TaxLineDescription = 'DUU AP FIN Invoice 115' and TransactionTypeCode = 'INV' and LedgerCode = 'AP' and InvoiceDateTime = '2023-08-10' and DueDateTime = '2023-08-11' and GLTransactionHeaderID = '402F1051-BC83-47EB-8419-3B342D5F27D8' and GLTransactionHeaderKey = 8 and TransactionNo = '34'").Length);
		}

		void AssertTransformedRow()
		{
			var transformedRow = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM [{ScriptDbName}].[biadmin].[TransformedRow] WHERE SchemaName = 'Finance' AND TableName = 'CUS__TaxGLMovementInfo'");
			AssertEquals("transformedRowcount", 3, transformedRow.Rows.Count);
			AssertEquals(1, transformedRow.Select("KeyValue = 1 and RefValue1 = 8").Length);
			AssertEquals(1, transformedRow.Select("KeyValue = 2").Length);
		}

		void AssertInitialData(DataTable result)
		{
			AssertEquals("Rowcount", 1, result.Rows.Count);
			AssertEquals(1, result.Select("TaxGLMovementKey = 1 and TaxHeaderDescription = 'desc1' and TaxLineDescription = 'DUU AP FIN Invoice 115' and TransactionTypeCode = 'INV' and LedgerCode = 'AP' and InvoiceDateTime = '2023-08-10' and DueDateTime = '2023-08-11' and GLTransactionHeaderID = '402F1051-BC83-47EB-8419-3B342D5F27D8' and GLTransactionHeaderKey = 8 and TransactionNo = '34'").Length);
		}

		DataTable SelectRows(string tableViewName = "CUS__TaxGLMovementInfo")
		{
			return DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM [{ScriptDbName}].[{ScriptToTest.SchemaName}].[{tableViewName}]");
		}

		protected override void SetUp()
		{
			base.SetUp();
			PrepareData();
			ExecuteCusTableLoad("InitialLoadQuery");
		}

		void PrepareData()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, $@"
				INSERT INTO [{ScriptDbName}].[Finance].[BAS__GLTransactionHeader]
					([GLTransactionHeaderKey], [GLTransactionHeaderID], [CompanyKey],  [LocalAmount], [TransactionType], [TransactionTypeCode], [LedgerCode], [PostDateTime],  [OrganizationHeaderKey], [CreateDateTimeUtc], [ComplianceSubType], [TransactionNo], [TransactionReference], [BranchKey], [InvoiceDateTime], [DueDateTime], [Description], [TransactionCategory], [TransactionBelongsToGroup], [IsCancelled])
				VALUES
					(8, '402F1051-BC83-47EB-8419-3B342D5F27D8', 1, 400.00, 'Invoice', 'INV', 'AR', '2023-08-10', 1 ,'2023-08-10', '', '34', 'ABC124', 1, '2023-08-10', '2023-08-11', 'desc1', 'FIN', NULL, 0),
					(9, newid(), 1, 400.00, 'Invoice', 'INV', 'AR', '2023-08-10', 1 ,'2023-08-10', '', '115', 'ABC124', 1, '2023-08-12', '2023-08-13', 'desc2', 'FIN', NULL, 0)

				INSERT INTO [{ScriptDbName}].[Finance].[BAS__TaxGLMovement]
					([TaxGLMovementKey],[TaxTransactionKey],[TaxGLMovementID], [Amount])
				VALUES
					(1, 2, newid(), 300)

				INSERT INTO [{ScriptDbName}].[Finance].[BAS__TaxTransaction]
					([TaxTransactionKey], [GLTransactionHeaderKey], [MatchTransactionHeaderID], [MatchTransactionHeaderKey], [LedgerCode], [TaxSystemCode],[TaxTransactionID])
				VALUES
					(2, 8, newid(), 9, 'AP', 'DUU', newid())
				"
			);

			TestConnection.ExecuteNonQuery(sqlText);
		}

		void ExecuteCusTableLoad(string sqlName)
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
				"SELECT {0} FROM [{1}].[biAdmin].[CustomTableConfiguration] WHERE [ModelSchemaName] = 'Finance' AND [ModelTableName] = 'CUS__TaxGLMovementInfo'",
				sqlName, ScriptDbName
			);
			var resultTable = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
			var record = resultTable.Select().Single();
			var loadSQLText = record.ItemArray[0].ToString();
			var sqlTextDL = "USE " + ScriptDbName + " " + loadSQLText;
			TestConnection.ExecuteNonQuery(sqlTextDL);
		}

		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}
	}
}
