using System;
using System.Data;
using System.Globalization;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting;
using Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDW.Functions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Functions.Accounting.Testing
{
	[TestedType(typeof(RawPLBSHALTAccBranch))]
	class RawPLBSHALTAccBranchTest : BiCreateScriptTest
	{
		public void TestRawPLBSHALTAccBranch()
		{
			PrepareData();
			var result = Execute(1, "", "");
			AssertEquals("Should has 6 rows", 6, result.Rows.Count);
			AssertEquals(2, result.Select("GLAccountKey = 4 and Amount in (0, -10) and BranchKey in (1,2)").Length);
			AssertEquals(2, result.Select("GLAccountKey = 1 and Amount in (0, 20) and BranchKey in (1,2)").Length);
			AssertEquals(1, result.Select("GLAccountKey = 3 and Amount in (0) and BranchKey in (2)").Length);
			AssertEquals(1, result.Select("GLAccountKey = 2 and Amount in (-10) and BranchKey in (2)").Length);

			result = Execute(1, "AU1", "B");
			AssertEquals("Should has 6 rows", 6, result.Rows.Count);
			AssertEquals(1, result.Select("GLAccountKey = 2 and Amount in (-20) and BranchKey in (2)").Length);

			result = Execute(1, "TW1", "B");
			AssertEquals("Should has 0 row", 0, result.Rows.Count);
		}

		DataTable Execute(int companyID, string department, string transactionCategory)
		{
			return DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM [{ScriptDbName}].[{ScriptToTest.SchemaName}].[{ScriptToTest.Name}] (202302, 202302, 202304, {companyID},'{department}','{transactionCategory}')");
		}

		void PrepareData()
		{
			Helper.InsertPeriodForInputYear(2022);
			Helper.InsertStmData(new DateTime(2022, 01, 01));

			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].[Finance].[BAS__GLAccount]
					([GLAccountKey]
					,[GLAccountID]
					,[AccountTypeCode]
					,[AccountNo]
					,[AccountGroup]
					,[Description]
					,[DebitCreditCode]
					,[Units]
					,[AlternateAccountKey]
					)
					VALUES
						(1, newid(), 'BSH', '1.23.456', 'G', 'ACC1', 'DR','', 4),
						(2, newid(), 'P&L', '2.45.000', 'G', 'ACC2', 'CR','', 3),
						(3, newid(), 'ALT', '6.87.000', 'G', 'ACC3', 'DR','', null),
						(4, newid(), 'BSH', '6.97.000', 'G', 'ACC4', 'CR','', null);

				INSERT [{0}].[Organization].[BAS__Branch]
					([BranchKey], [BranchID], [CompanyKey], [BranchCode], [OrganizationKey])
					VALUES
						(1, newid(), 1, 'SYN', 1),
						(2, newid(), 1, 'DTW', 2);

				INSERT [{0}].[Organization].[BAS__Company]
					([CompanyKey], [CompanyID], [LocalCurrency], [CompanyCode], [IsGSTCashBasis], [IsGSTRegistered] )
					VALUES
						(1, newid(), 'AUD', 'DAU', 1, 1),
						(2, newid(), 'USD', 'DTW', 0, 0);

				INSERT [{0}].[Organization].[BAS__Department]
					([DepartmentKey], [DepartmentID], [Code])
					VALUES
						(1, newid(), 'AU1'),
						(2, newid(), 'TW1');
				INSERT [{0}].[Finance].[GRP__GeneralLedgerAggregateData]
					( [GLAccountKey], [PostPeriod], [GLAmountLocalCredit], [GLAmountLocalDebit], [CompanyKey], [TransactionCategory], [DepartmentKey], [BranchKey], [GLAmountLocalBalance])
					VALUES
						(4, 202304, 10, 0, 1, '' , 1, 1, -10),
						(2, 202302, 10, 0, 1, 'B', 1, 2, -10),
						(3, 202303, 10, 0, 1, '' , 2, 1, -10),
						(1, 202303, 10, 0, 1, '' , 1, 1, -10),
						(1, 202303, 10, 0, 1, '' , 1, 2, 20),
						(2, 202303, 10, 0, 1, '', 1, 2, -10),
						(3, 202203, 10, 0, 1, '' , 2, 1, -10),
						(4, 202201, 10, 0, 1, 'C', 1, 2, -20)

				INSERT [{0}].[Customs].[BAS__Account]
					([AccountKey], [AccountID], [GLAccountKey], [AccountType])
					VALUES
						(1, newid(), 4, 'GL_PL_APPROPRIATION_ACCOUNT'),
						(2, newid(), 3, 'GL_BS_ACCOUNT_START');
			", ScriptDbName);
			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}

		PrepareDataHelper Helper => helper ?? (helper = new PrepareDataHelper(TestConnection, ScriptDbName));
		PrepareDataHelper helper;
	}
}
