using System;
using System.Data;
using System.Globalization;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting;
using Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDW.Functions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Functions.Accounting.Testing
{
	[TestedType(typeof(ProfitAndLossReport))]
	class ProfitAndLossReportEDWTest : BiCreateScriptTest
	{
		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}

		public void TestReportTypeTBSHasData_WhenPeriodLessThanLastProcessedDate()
		{
			Helper.InsertBASAggregate(1, 202212, 2m);

			var result = Execute(202301, CompanyPK, "TBS");
			AssertEquals(9, result.Rows.Count);

			result = Execute(202212, CompanyPK, "TBS");
			AssertEquals(9, result.Rows.Count);
		}

		public void TestReportTypePNLHasEmpty_WhenPeriodLessThanLastProcessedDate()
		{
			Helper.InsertBASGLAggregate(10m, 202302, 11);
			Helper.InsertBASGLAggregate(10m, 202302, 12);
			Helper.InsertBASGLAggregate(10m, 202302, 13);

			var result = Execute(202301, CompanyPK, "PNL");
			AssertEquals(8, result.Rows.Count);

			result = Execute(202212, CompanyPK, "BSH");
			AssertEquals(3, result.Rows.Count);
		}

		public void TestReportTypeBSHHasEmpty_WhenPeriodLessThanLastProcessedDate()
		{
			Helper.InsertBASGLAggregate(10m, 202302, 11);
			Helper.InsertBASGLAggregate(10m, 202302, 12);
			Helper.InsertBASGLAggregate(10m, 202302, 13);

			var result = Execute(202301, CompanyPK, "BSH");
			AssertEquals(3, result.Rows.Count);

			result = Execute(202212, CompanyPK, "BSH");
			AssertEquals(3, result.Rows.Count);
		}

		protected override void SetUp()
		{
			base.SetUp();
			PrepareData();
		}

		void PrepareData()
		{
			CompanyPK = Guid.NewGuid();
			DepartmentPK = Guid.NewGuid();
			BranchPK = Guid.NewGuid();

			Helper.InsertPeriodForInputYear(2022);
			Helper.InsertPeriodForInputYear(2023);
			Helper.InsertPeriodForInputYear(2024);

			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].[Organization].[BAS__Organization]
					([OrganizationKey], [OrganizationID], [Code])
					VALUES
						(1, newid(), 'OR1'),
						(2, newid(), 'OR2'),
						(3, newid(), 'OR3');

				INSERT [{0}].[Finance].[BAS__GLAccount]
					([GLAccountKey]
					,[GLAccountID]
					,[AccountTypeCode]
					,[AccountNo]
					,[AccountGroup]
					,[Description]
					,[DebitCredit]
					,[Units]
					,[SectionType]
					)
					VALUES
						(1, newid(), 'NTE', '1.23.456', 'G', 'ACC1', 'DEBIT' ,'KG','TS'),
						(2, newid(), 'P&L', '2.45.000', 'G', 'ACC2', 'CREDIT','','TS'),
						(3, newid(), 'BSH', '6.87.000', 'G', 'ACC3', 'DEBIT' ,'','TS'),
						(4, newid(), 'BSH', '6.97.000', 'G', 'ACC4', 'DEBIT' ,'','TS'),
						(5, newid(), 'CLN', '1.23.600', 'G', 'ACC5', 'DEBIT' ,'','TS'),
						(6, newid(), 'TTL', '2666.00' , 'G', 'ACC6', 'CREDIT','','TS'),
						(7, newid(), 'ALT', '2.00'    , 'G', 'ACC7', 'CREDIT','','TS'),
						(8, newid(), 'HDR', '3888'    , 'G', 'ACC8', 'DEBIT' ,'','TS'),
						(9, newid(), 'NTE', '6.99.456', 'G', 'ACC9', 'DEBIT' ,'KCN','OV'),
						(11, newid(), 'NTE', '11.23.456', 'G', 'ACC11', 'DEBIT' ,'KG','OE'),
						(12, newid(), 'P&L', '21.45.000', 'G', 'ACC12', 'CREDIT','','AS'),
						(13, newid(), 'BSH', '61.87.000', 'G', 'ACC13', 'DEBIT' ,'','LI');

				INSERT [{0}].[Organization].[BAS__Branch]
					([BranchKey], [BranchID], [CompanyKey], [BranchCode], [OrganizationKey])
					VALUES
						(1, '{3}'  , 1, 'SYN', 1);

				INSERT [{0}].[Organization].[BAS__Company]
					([CompanyKey], [CompanyID], [LocalCurrency], [CompanyCode], [IsGSTCashBasis], [IsGSTRegistered] )
					VALUES
						(1, '{1}', 'AUD', 'DAU', 1, 1);

				INSERT [{0}].[Finance].[BAS__Currency]
					([CurrencyKey], [CurrencyID], [CurrencyCode], [SubUnitRatio])
					VALUES
						(1, newid(), 'AUD' , 100),
						(2, newid(), 'USD', 50);

				INSERT [{0}].[Organization].[BAS__Department]
					([DepartmentKey], [DepartmentID], [Code])
					VALUES
						(1, '{2}', 'AUU');

				INSERT [{0}].[Finance].[BAS__ChargeCode]
					([ChargeCodeKey], [ChargeCodeID], [ChargeType], [Code], [Desc], [LocalLanguageDescription])
					VALUES
						(901, newid(), 'CMT', 'CMT1', 'CMT desc', 'CMT local desc'),
						(902, newid(), 'ABC', 'MJA1', 'MJA desc', 'MJA local desc');

				INSERT [{0}].[Customs].[BAS__Account]
					([AccountKey], [AccountID], [GLAccountKey], [AccountType])
					VALUES
						(1, newid(), 4, 'GL_PL_APPROPRIATION_ACCOUNT'),
						(2, newid(), 3, 'GL_BS_ACCOUNT_START'),
						(3, newid(), 2, 'GL_GROSS_PROFIT_TOTAL_ACCOUNT'),
						(4, newid(), 1, 'GL_NET_PROFIT_TOTAL_ACCOUNT'),
						(5, newid(), 6, 'GL_OVERHEAD_TOTAL_ACCOUNT');

				INSERT [{0}].[Finance].[BAS__StmDataDate]
					([StmDataDateKey], [StmDataDateID], [Name], [CompanyID], [Value], [CompanyKey])
					VALUES
						(1, newid(), 'JournalEntriesLastProcessedDate', '{1}', '2023-01-01 00:00:00.000', 1);
				",
				ScriptDbName, CompanyPK, DepartmentPK, BranchPK
			);

			TestConnection.ExecuteNonQuery(sqlText);
		}

		DataTable Execute(int period, Guid companyPK, string reportType)
		{
			var sqlText = $@"EXEC [{ScriptDbName}].[{ScriptToTest.SchemaName}].[{ScriptToTest.Name}]
						{period}, '{companyPK}', '', '', 'Y', '', '', '{reportType}', '', '', '', '', '', '', '', '', 'Y', NULL, ':YearToPeriod:'";

			return DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
		}

		Guid CompanyPK, DepartmentPK, BranchPK;

		PrepareDataHelper Helper => helper ?? (helper = new PrepareDataHelper(TestConnection, ScriptDbName));
		PrepareDataHelper helper;
	}
}
