using System;
using System.Data;
using System.Globalization;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting;
using Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDW.Functions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Functions.Accounting.Testing
{
	[TestedType(typeof(RawPLBSHALTAccBranchRetainedEarnings))]
	class RawPLBSHALTAccBranchRetainedEarningsTest : BiCreateScriptTest
	{
		public void TestRawPLBSHALTAccBranchRetainedEarnings()
		{
			PreparaData();

			var result = Execute(CompanyPK, "", "");
			AssertEquals(1, result.Rows.Count);
			AssertEquals(1, result.Select("GLAccountKey=4 and Amount = -10 and BranchKey is null").Length);

			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].[Finance].[GRP__GeneralLedgerAggregateData]
					( [GLAccountKey], [PostPeriod], [GLAmountLocalCredit], [GLAmountLocalDebit], [CompanyKey], [TransactionCategory], [DepartmentKey], [BranchKey], [GLAmountLocalBalance])
					VALUES
						(4, 202303, 10, 0, 1, '', 1, 1, -30)
			", ScriptDbName);
			TestConnection.ExecuteNonQuery(sqlText);
			result = Execute(CompanyPK, "", "");
			AssertEquals(1, result.Rows.Count);
			AssertEquals(1, result.Select("GLAccountKey=4 and Amount = -40 and BranchKey=1").Length);

			result = Execute(CompanyPK, "", "B");
			AssertEquals(1, result.Rows.Count);
			AssertEquals(1, result.Select("GLAccountKey=4 and Amount = -50 and BranchKey=1").Length);

			result = Execute(CompanyPK, "AU1", "B");
			AssertEquals(1, result.Rows.Count);
			AssertEquals(1, result.Select("GLAccountKey=4 and Amount = -40 and BranchKey=1").Length);
		}

		public void TestLastProcessDateIsEmpty()
		{
			PreparaData();
			Helper.InsertBASGLAggregate(4, 202302, 1, presentationCategory: "B");
			Helper.InsertBASGLAggregate(4, 202303, 2, presentationCategory: "B");
			Helper.InsertBASGLAggregate(4, 202304, 3, presentationCategory: "B");

			var result = Execute(CompanyPK, "", "B");
			AssertEquals("Data comes from GRP__GeneralLedgerAggregateData", 1, result.Select("Amount = -20").Length);
			
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
					DELETE FROM [{0}].[Finance].[BAS__StmDataDate];
					", ScriptDbName);
			TestConnection.ExecuteNonQuery(sqlText);
			result = Execute(CompanyPK, "", "B");
			AssertEquals("Data comes from BAS__GLAggregate", 1, result.Select("Amount = 4").Length);
		}

		public void TestCanGetAllAggregateData()
		{
			PreparaData();

			Helper.InsertBASGLAggregate(-10, 202212, 4);

			var result = Execute(CompanyPK, "", "", 202211);
			AssertEquals(1, result.Rows.Count);
			AssertEquals(1, result.Select("GLAccountKey=4 and Amount = -20 and BranchKey=1").Length);

			result = Execute(CompanyPK, "", "B", 202211);
			AssertEquals(1, result.Rows.Count);
			AssertEquals(1, result.Select("GLAccountKey=4 and Amount = -30 and BranchKey=1").Length);

			result = Execute(CompanyPK, "AU1", "B", 202211);
			AssertEquals(1, result.Rows.Count);
			AssertEquals(1, result.Select("GLAccountKey=4 and Amount = -20 and BranchKey=1").Length);
		}

		DataTable Execute(Guid companyPK, string department, string transactionCategory, int bshStartPeriod = 202302)
		{
			return DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM [{ScriptDbName}].[{ScriptToTest.SchemaName}].[{ScriptToTest.Name}] ({bshStartPeriod}, 202304,202304, '{companyPK}','{department}','{transactionCategory}')");
		}

		void PreparaData()
		{
			Helper.InsertPeriodForInputYear(2023);

			CompanyPK = Guid.NewGuid();
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
						(3, newid(), 'P&L', '6.87.000', 'G', 'ACC3', 'DEBIT' ,''),
						(4, newid(), 'BSH', '6.97.000', 'G', 'ACC4', 'DEBIT' ,'');

				INSERT [{0}].[Organization].[BAS__Branch]
					([BranchKey], [BranchID], [CompanyKey], [BranchCode], [OrganizationKey])
					VALUES
						(1, newid(), 1, 'SYN', 1),
						(2, newid(), 1, 'DTW', 2);

				INSERT [{0}].[Organization].[BAS__Company]
					([CompanyKey], [CompanyID], [LocalCurrency], [CompanyCode], [IsGSTCashBasis], [IsGSTRegistered] )
					VALUES
						(1, '{1}', 'AUD', 'DAU', 1, 1),
						(2, newid(), 'USD', 'DTW', 0, 0);

				INSERT [{0}].[Organization].[BAS__Department]
					([DepartmentKey], [DepartmentID], [Code])
					VALUES
						(1, newid(), 'AU1'),
						(2, newid(), 'TW1');
				INSERT [{0}].[Finance].[GRP__GeneralLedgerAggregateData]
					( [GLAccountKey], [PostPeriod], [GLAmountLocalCredit], [GLAmountLocalDebit], [CompanyKey], [TransactionCategory], [DepartmentKey], [BranchKey], [GLAmountLocalBalance])
					VALUES
						(1, 202303, 10, 0, 1, '' , 1, 1, -10),
						(2, 202302, 10, 0, 1, 'B', 1, 2, -10),
						(3, 202303, 10, 0, 1, '' , 2, 1, -10),
						(4, 202301, 10, 0, 1, 'C', 1, 2, -20)

				INSERT [{0}].[Customs].[BAS__Account]
					([AccountKey], [AccountID], [GLAccountKey], [AccountType])
					VALUES
						(1, newid(), 4, 'GL_PL_APPROPRIATION_ACCOUNT'),
						(2, newid(), 3, 'GL_BS_ACCOUNT_START');
				INSERT [{0}].[Finance].[BAS__PeriodManagement]
					([PeriodManagementKey], [PeriodManagementID], [CompanyKey], [StartDate], [EndDate],  [Period], [Year])
					VALUES
						(1, newid(), 1, '2023-01-01', '2023-01-31',  202301, 2023),
						(2, newid(), 1, '2023-02-01', '2023-02-28',  202302, 2023),
						(3, newid(), 1, '2023-04-01', '2023-04-28',  202304, 2023);

				INSERT [{0}].[Finance].[BAS__StmDataDate]
					([StmDataDateKey], [StmDataDateID], [Name], [CompanyID], [Value], [CompanyKey])
					VALUES
						(1, newid(), 'JournalEntriesLastProcessedDate', '{1}', '2023-01-01 00:00:00.000', 1);
			", ScriptDbName, CompanyPK);
			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}

		Guid CompanyPK;

		PrepareDataHelper Helper => helper ?? (helper = new PrepareDataHelper(TestConnection, ScriptDbName));
		PrepareDataHelper helper;
	}
}
