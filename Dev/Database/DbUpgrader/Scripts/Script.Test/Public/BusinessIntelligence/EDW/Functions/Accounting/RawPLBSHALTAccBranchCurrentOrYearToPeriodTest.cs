using System;
using System.Data;
using System.Globalization;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting;
using Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDW.Functions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Functions.Accounting.Testing
{
	[TestedType(typeof(RawPLBSHALTAccBranchCurrentOrYearToPeriod))]
	class RawPLBSHALTAccBranchCurrentOrYearToPeriodTest : BiCreateScriptTest
	{
		public void TestRawPLBSHALTAccBranchCurrentOrYearToPeriod()
		{
			PrepareData();
			var result = Execute(202304, CompanyPK, "", "", 1);
			AssertEquals(2, result.Rows.Count);
			AssertEquals(1, result.Select("Branch is Null").Length);
			AssertEquals(1, result.Select("Branch =1 and Cost =0 and Revenue = 10").Length);

			result = Execute(202304, CompanyPK, "", "", 0);
			AssertEquals(2, result.Rows.Count);
			AssertEquals(1, result.Select("Branch =2 and Cost =-20 and Revenue = 10").Length);
			AssertEquals(1, result.Select("Branch =1 and Cost =20 and Revenue = 20").Length);

			result = Execute(202304, CompanyPK, "AU1", "", 0);
			AssertEquals(2, result.Rows.Count);
			AssertEquals(1, result.Select("Branch =2 and Cost =-20 and Revenue = 10").Length);
			AssertEquals(1, result.Select("Branch =1 and Cost =10 and Revenue = 20").Length);

			result = Execute(202304, CompanyPK, "AU1", "B", 0);
			AssertEquals(2, result.Rows.Count);
			AssertEquals(1, result.Select("Branch =2 and Cost =-20 and Revenue = 20").Length);
			AssertEquals(1, result.Select("Branch =1 and Cost =10 and Revenue = 20").Length);
		}

		public void TestLastProcessDateIsEmpty()
		{
			PrepareData();
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
					DELETE FROM [{0}].[Finance].[BAS__StmDataDate];
					", ScriptDbName);
			TestConnection.ExecuteNonQuery(sqlText);
			var result = Execute(202304, CompanyPK, "", "", 1);
			AssertEquals(1, result.Rows.Count);
		}

		DataTable Execute(int period, Guid companyID, string department, string transactionCategory, int shouldCalculateCurrentAmounts)
		{
			return DataUtils.GetDataTableFromQuery(TestConnection,
				$"SELECT * FROM [{ScriptDbName}].[{ScriptToTest.SchemaName}].[{ScriptToTest.Name}] ({period}, '{companyID}', '{department}','{transactionCategory}',{shouldCalculateCurrentAmounts})");
		}

		void PrepareData()
		{
			Helper.InsertPeriodForInputYear(2022);
			Helper.InsertStmData(new DateTime(2022, 01, 01));

			CompanyPK = Guid.NewGuid();

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
						(1, newid(), 'P&L', '1.23.456', 'G', 'ACC1', 'DR','', 4),
						(2, newid(), 'P&L', '2.45.000', 'G', 'ACC2', 'CR','', 3),
						(3, newid(), 'P&L', '6.87.000', 'G', 'ACC3', 'DR','', null),
						(4, newid(), 'P&L', '6.97.000', 'G', 'ACC4', 'CR','', null);

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
				INSERT [{0}].[Finance].[BAS__PeriodManagement]
					([PeriodManagementKey], [PeriodManagementID], [CompanyKey], [StartDate], [EndDate],  [Period], [Year])
					VALUES
						(1, newid(), 1, '2023-01-01', '2023-01-31',  202301, 2023),
						(2, newid(), 1, '2023-02-01', '2023-02-28',  202302, 2023),
						(3, newid(), 1, '2023-04-01', '2023-04-28',  202304, 2023);

				INSERT [{0}].[Finance].[BAS__StmDataDate]
					([StmDataDateKey], [StmDataDateID], [Name], [CompanyID], [Value])
					VALUES
						(1, newid(), 'JournalEntriesLastProcessedDate', '{1}', '2023-01-01 00:00:00.000');
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
