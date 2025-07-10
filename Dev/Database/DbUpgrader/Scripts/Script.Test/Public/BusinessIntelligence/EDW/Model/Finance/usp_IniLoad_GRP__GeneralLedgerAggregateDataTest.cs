using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Model.Finance;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Model.Finance.Testing
{
	[TestedType(typeof(vw_GRP__GeneralLedgerAggregateData))]
	internal class usp_IniLoad_GRP__GeneralLedgerAggregateDataTest : BiCreateScriptTest
	{
		[ExpectNoExceptions]
		public void TestRun()
		{
			PrepareTestData();
			Execute();
			var resultTable = SelectRows();
			AssertEquals("Rowcount", 2, resultTable.Rows.Count);

			CombineAssertions(() =>
			{
				AssertRowValues(resultTable, 202308, 1, 1, 1, 1, "", 1, "AUD", 300.00, 600.00, 300.00, 300.00, 600.00, 300.00);
				AssertRowValues(resultTable, 202308, 2, 1, 1, 1, "TS3", 1, "AUD", 700.00, 1400.00, 700.00, 700.00, 1400.00, 700.00);
			});
		}

		void AssertRowValues(DataTable resultTable, int postPeriod, int gLAccountKey, int branchkey, int departmentkey, int companykey, string transactionCategory, int taxBranchKey, string currency, double gLAmountLocalCredit, double gLAmountLocalDebit, double gLAmountLocalBalance, double gLAmountOSCredit, double gLAmountOSDebit, double gLAmountOSBalance)
		{
			var selectqry = $@"[PostPeriod] = {postPeriod}
				AND [GLAccountKey] = {gLAccountKey}
				AND [BranchKey] = {branchkey}
				AND [DepartmentKey] = {departmentkey}
				AND [CompanyKey] = {companykey}
				AND [TransactionCategory] = '{transactionCategory}'
				AND [TaxBranchKey] = {taxBranchKey}
				AND [Currency] = '{currency}'
				AND [GLAmountLocalCredit] = {gLAmountLocalCredit}
				AND [GLAmountLocalDebit] = {gLAmountLocalDebit}
				AND [GLAmountLocalBalance] = {gLAmountLocalBalance}
				AND [GLAmountOSCredit] = {gLAmountOSCredit}
				AND [GLAmountOSDebit] = {gLAmountOSDebit}
				AND [GLAmountOSBalance] = {gLAmountOSBalance}";

			var rows = resultTable.Select(selectqry);

			AssertEquals("Rowcount should be 1", 1, rows.Length);
		}

		void PrepareTestData()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].[Finance].[BAS__GeneralLedgerData]
					([AccGLTransactionLineKey], [BranchKey], [CompanyKey], [DepartmentKey],
					[GeneralLedgerDataID], [GeneralLedgerDataKey],
					[GLAccountKey], [Currency],[ExchangeRate],[GLAccountType],
					[LocalCreditAmount],  [LocalDebitAmount], [LocalBalance], [OSBalance], [OSCreditAmount], [OSDebitAmount], [PostDate], [PostPeriod],
					[SystemCreateTimeUtc], [SystemCreateUser], [SystemLastEditTimeUtc], [SystemLastEditUser],
					[Type], [GLTransactionHeaderKey], [TaxBranchKey], [TaxGLMovementKey])
					VALUES
						(1, 1, 1, 1, newid(), 1, 1, 'AUD', 1, 'ARC', 100.00, 200.00, 100.00, 100.00, 100.00, 200.00, '2023-08-10',202308, '2023-08-21', '~BP', '2023-08-21', '~BP', 'PST', 1, 1, 1),
						(1, 1, 1, 1, newid(), 2, 1, 'AUD', 1, 'ARC', 200.00, 400.00, 200.00, 200.00, 200.00, 400.00, '2023-08-10',202308, '2023-08-21', '~BP', '2023-08-21', '~BP', 'PST', 2, 1, 1),
						(1, 1, 1, 1, newid(), 3, 2, 'AUD', 1, 'ARC', 300.00, 600.00, 300.00, 300.00, 300.00, 600.00, '2023-08-10',202308, '2023-08-21', '~BP', '2023-08-21', '~BP', 'PST', 3, 1, 1),
						(1, 1, 1, 1, newid(), 4, 2, 'AUD', 1, 'ARC', 400.00, 800.00, 400.00, 400.00, 400.00, 800.00, '2023-08-10',202308, '2023-08-21', '~BP', '2023-08-21', '~BP', 'PST', 4, 1, 1);

				INSERT [{0}].[Finance].[BAS__GLTransactionHeader]
					([GLTransactionHeaderKey], [GLTransactionHeaderID], [CompanyKey], [LocalAmount], [TransactionTypeCode], [LedgerCode], [PostDateTime],  [OrganizationHeaderKey], [CreateDateTimeUtc], [ComplianceSubType], [TransactionNo], [TransactionReference], [BranchKey], [TransactionCategory])
					VALUES
						(1, newid(), 1, 100.00, 'INV', 'AR', '2023-08-10', 1 ,'2023-08-10', '', '001', '001', 1, 'TS1'),
						(2, newid(), 1, 200.00, 'INV', 'AR', '2023-08-10', 1 ,'2023-08-10', '', '002', '002', 1, 'TS2'),
						(3, newid(), 1, 100.00, 'GJL', 'GL', '2023-08-10', 1 ,'2023-08-10', '', '001', '001', 1, 'TS3'),
						(4, newid(), 1, 200.00, 'GJL', 'GL', '2023-08-10', 1 ,'2023-08-10', '', '002', '002', 1, 'TS3')",

					ScriptDbName
			);

			TestConnection.ExecuteNonQuery(sqlText);
		}

		DataTable SelectRows()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
					"SELECT * FROM [{0}].[Finance].[GRP__GeneralLedgerAggregateData]",
					ScriptDbName
			);

			return DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
		}

		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}

		string GetIniLoadSQLText()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
					"SELECT [InitialLoadQuery] FROM [{0}].[biAdmin].[CustomTableConfiguration] WHERE  [ModelSchemaName] = 'Finance' AND [ModelTableName] = 'GRP__GeneralLedgerAggregateData'",
					ScriptDbName
			);

			var resultTable = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
			var record = resultTable.Select().Single();
			var iniLoadSQLText = record.ItemArray[0].ToString();
			return iniLoadSQLText;
		}

		void Execute()
		{
			var iniLoadSQLText = GetIniLoadSQLText();
			var sqlText1 = "USE " + ScriptDbName + " " + iniLoadSQLText;
			var sqlText2 = "USE " + Db.DatabaseName;

			TestConnection.ExecuteNonQuery(sqlText1);
			TestConnection.ExecuteNonQuery(sqlText2);
		}
	}
}
