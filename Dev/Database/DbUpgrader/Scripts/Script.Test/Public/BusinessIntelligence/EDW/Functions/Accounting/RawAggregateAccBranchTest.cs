using System;
using System.Data;
using System.Globalization;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting;
using Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDW.Functions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Functions.Accounting.Testing
{
	[TestedType(typeof(RawAggregateAccBranch))]
	class RawAggregateAccBranchTest : BiCreateScriptTest
	{
		public void TestRawAggregateAccBranch()
		{
			PrepareData();
			var departmentCode = "AU1";
			var companyKey = 1;
			var transactionCategory = "B";

			var result = Execute(departmentCode, companyKey, transactionCategory);
			AssertEquals("Result should have 2 rows", 2, result.Rows.Count);
			AssertEquals(2, result.Select("GLAccountKey in (1,2) and CurrentAmount =-10 ").Length);

			transactionCategory = "";
			result = Execute(departmentCode, companyKey, transactionCategory);
			AssertEquals("Result should have one row", 1, result.Rows.Count);

			departmentCode = "";
			result = Execute(departmentCode, companyKey, transactionCategory);
			AssertEquals("Result should have 2 rows", 2, result.Rows.Count);
			AssertEquals(2, result.Select("GLAccountKey in (1,3) ").Length);

			result = Execute(departmentCode, companyKey, transactionCategory, "1");
			AssertEquals("Result should have 1 row", 1, result.Rows.Count);
			AssertEquals(1, result.Select("GLAccountKey in (1) ").Length);

			companyKey = 2;
			result = Execute(departmentCode, companyKey, transactionCategory);
			AssertEquals("Result should have 0 rows", 0, result.Rows.Count);
		}

		public void TestCanGetAllAggregateData()
		{
			PrepareData();

			Helper.InsertBASGLAggregate(-10, 202212, 1, presentationCategory: "B");

			var departmentCode = "AU1";
			var companyKey = 1;
			var transactionCategory = "B";

			var result = Execute(departmentCode, companyKey, transactionCategory, startPeriod: 202210);
			AssertEquals("Result should have 2 rows", 2, result.Rows.Count);
			AssertEquals(1, result.Select("GLAccountKey = 1 and CurrentAmount =-20 ").Length);
			AssertEquals(1, result.Select("GLAccountKey = 2 and CurrentAmount =-10 ").Length);
		}

		DataTable Execute(string departmentCode, int companyKey, string transactionCategory, string pLAccount = null, int startPeriod = 202302)
		{
			if (pLAccount == null)
			{
				return DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM [{ScriptDbName}].[{ScriptToTest.SchemaName}].[{ScriptToTest.Name}] ({startPeriod},'{departmentCode}',202304,{companyKey},'{transactionCategory}', NULL)");
			}

			return DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM [{ScriptDbName}].[{ScriptToTest.SchemaName}].[{ScriptToTest.Name}] ({startPeriod},'{departmentCode}',202304,{companyKey},'{transactionCategory}', {pLAccount})");
		}

		void PrepareData()
		{
			Helper.InsertPeriodForInputYear(2023);
			Helper.InsertStmData(new DateTime(2023, 01, 01));

			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].[Finance].[BAS__GLAccount]
					([GLAccountKey]
					,[GLAccountID]
					,[AccountTypeCode]
					,[AccountNo]
					,[AccountGroup]
					,[Description]
					,[DebitCredit]
					,[Units]
					)
					VALUES
						(1, newid(), 'NTE', '1.23.456', 'G', 'ACC1', 'DEBIT' ,'KG'),
						(2, newid(), 'P&L', '2.45.000', 'G', 'ACC2', 'CREDIT',''),
						(3, newid(), 'BSH', '6.87.000', 'G', 'ACC3', 'DEBIT' ,''),
						(4, newid(), 'BSH', '6.97.000', 'G', 'ACC4', 'DEBIT' ,'');

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
						(1, 202304, 10, 0, 1, '' , 1, 1, -10),
						(2, 202302, 10, 0, 1, 'B', 1, 2, -10),
						(3, 202304, 10, 0, 1, '' , 2, 1, -10),
						(4, 202302, 10, 0, 1, 'C', 1, 2, -10)
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
