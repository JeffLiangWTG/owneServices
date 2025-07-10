using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Functions.Accounting.Testing
{
	[TestedType(typeof(ProfitAndLossReportPeriodAnalysis))]
	class ProfitAndLossReportPeriodAnalysisTest : BiCreateScriptTest
	{
		public void TestColumnsOfProfitAndLossReportPeriodAnalysis()
		{
			var whiteList = new List<string>
			{
				"AccountNumber",
				"AccountName",
				"AccountType",
				"Units",
				"PrintSequence",
				"AccountNumberForSequence",
				"AdditionalDissection"
			};
			var msgToHint = "Please confirm the columns of ProfitAndLossReportPeriodAnalysis";

			var result = Execute(202301, CompanyPK, new List<string>());
			Assert(msgToHint, result.Columns.Count >= whiteList.Count);
			foreach (string column in whiteList)
			{
				Assert($@"{msgToHint} Should contain column '{column}'", result.Columns.Contains(column));
			}
		}

		public void TestFilterCompanyPK()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, $@"
				INSERT {ScriptDbName}.Finance.GRP__GeneralLedgerAggregateData
						( [GLAccountKey], [PostPeriod], [GLAmountLocalCredit], [GLAmountLocalDebit], [GLAmountLocalBalance], [CompanyKey], [TransactionCategory], [DepartmentKey], [BranchKey])
					VALUES
						(1, 202302, 2, 4, 2, 1, '', 1, 1),
						(2, 202302, 2, 4, 2, 2, '', 1, 1),
						(3, 202303, 2, 4, 2, 2, '', 1, 1),
						(4, 202304, 2, 4, 2, 1, '', 1, 1),
						(5, 202305, 2, 4, 2, 1, '', 1, 1),
						(6, 202306, 2, 4, 2, 1, '', 1, 1)
				",
				ScriptDbName
			);

			TestConnection.ExecuteNonQuery(sqlText);

			var result = Execute(202305, CompanyPK, new List<string> { "", "", "", "", "N" } );
			AssertEquals("Should have 24 columns ", 24,result.Columns.Count);
			AssertEquals("Should has 4 rows", 4, result.Rows.Count);
			AssertEquals("P2 of ACC1 has value", 1,result.Select("AccountName='ACC1' and P2=-2 and P7 is null and P2ForCalculateTotal=0").Length);
			AssertEquals("ACC8 has null value", 1, result.Select("AccountName='ACC8' and P7 is null").Length);
			AssertEquals("P5 of ACC5 has value", 1, result.Select("AccountName='ACC5' and P5=-2 and P5ForCalculateTotal=-2").Length);
			AssertEquals("ACC6 has zero amount", 1, result.Select("AccountName='ACC6' and P5=0 and P5ForCalculateTotal=0").Length);
			AssertEquals("ACC2 is not here", 0, result.Select("AccountName='ACC2' ").Length);
		}

		public void TestFilterIncludeBudgetIsY()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].Finance.GRP__GeneralLedgerAggregateData
						( [GLAccountKey], [PostPeriod], [GLAmountLocalCredit], [GLAmountLocalDebit], [GLAmountLocalBalance], [CompanyKey], [TransactionCategory], [DepartmentKey], [BranchKey])
					VALUES
						(1, 202302, 2, 8, 6, 1, '', 1, 1),
						(2, 202302, 2, 12, 10, 1, '', 1, 1),
						(3, 202303, 2, 6, 4, 1, '', 1, 1),
						(4, 202304, 2, 5, 3, 1, '', 1, 1),
						(5, 202305, 2, 4, 2, 1, '', 1, 1),
						(6, 202306, 2, 4, 2, 1, '', 1, 1)

				INSERT [{0}].[Finance].[BAS__GLBudget]
					([GLBudgetKey]
					,[GLBudgetID]
					,[DepartmentKey]
					,[BranchKey]
					,[Opening]
					,[GLAccountKey]
					)
					VALUES
						(1, newid(), 1, 1, 2.45, 1),
						(2, newid(), 1, 1, 3.45, 2),
						(3, newid(), 1, 1, 4.45, 3),
						(4, newid(), 1, 1, 5.45, 4);
				
				INSERT [{0}].[Finance].[BAS__GLBudgetLines]
					([GLBudgetKey]
					,[GLBudgetLinesKey]
					,[GLBudgetLinesID]
					,[Period]
					,[Amount]
					)
					VALUES
						(1, 1, newid(), 202301, 4),
						(2, 2, newid(), 202302, 3),
						(3, 3, newid(), 202303, 2),
						(4, 4, newid(), 202304, 1);
				",
				ScriptDbName
			);

			TestConnection.ExecuteNonQuery(sqlText);

			var result = Execute(202305, CompanyPK, new List<string> { "", "", "", "", "Y" });
			AssertEquals("Should have 38 columns ", 38, result.Columns.Count);
			AssertEquals("Should has 5 rows", 5, result.Rows.Count);
			AssertEquals("ACC1 has value", 1, result.Select("AccountName='ACC1' and B1=-4 and P2=-6").Length);
			AssertEquals("ACC2 has value", 1, result.Select("AccountName='ACC2' and B2=-3 and P2=-10 and B3 is null").Length);
			AssertEquals("ACC8 has null value", 1, result.Select("AccountName='ACC8' and B7 is null").Length);
			AssertEquals("P5 of ACC5 has value", 1, result.Select("AccountName='ACC5' and P5=-2 and P5ForCalculateTotal=-2 and B5 is null").Length);
			AssertEquals("ACC6 has value with sum value of(P&L and BSH)", 1, result.Select("AccountName='ACC6' and P2=-10 and B2 =-3 and P5=0 and B7 is null and B7ForCalculateTotal is null").Length);
		}

		public void TestFilterBranchList()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].Finance.GRP__GeneralLedgerAggregateData
						( [GLAccountKey], [PostPeriod], [GLAmountLocalCredit], [GLAmountLocalDebit], [GLAmountLocalBalance], [CompanyKey], [TransactionCategory], [DepartmentKey], [BranchKey])
					VALUES
						(1, 202302, 2, 8, 6, 1, '', 1, 1),
						(2, 202302, 2, 12, 10, 1, '', 1, 2);
				",
				ScriptDbName
			);

			TestConnection.ExecuteNonQuery(sqlText);
			var result = Execute(202305, CompanyPK, new List<string> { "", "SYN" });
			AssertEquals("Should have 4 Rows ", 4, result.Rows.Count);
			AssertEquals("ACC1 is here", 4, result.Select("AccountName in ('ACC6','ACC1','ACC8','ACC5')").Length);
			AssertEquals("ACC2 is not here", 0, result.Select("AccountName in ('ACC2')").Length);
		}

		public void TestFilterDepartmentList()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].Finance.GRP__GeneralLedgerAggregateData
						( [GLAccountKey], [PostPeriod], [GLAmountLocalCredit], [GLAmountLocalDebit], [GLAmountLocalBalance], [CompanyKey], [TransactionCategory], [DepartmentKey], [BranchKey])
					VALUES
						(1, 202302, 2, 8, 6, 1, '', 1, 1),
						(2, 202302, 2, 12, 10, 1, '', 2, 1);
				",
				ScriptDbName
			);

			TestConnection.ExecuteNonQuery(sqlText);
			var result = Execute(202305, CompanyPK, new List<string> { "AUU" });
			AssertEquals("Should have 4 Rows ", 4, result.Rows.Count);
			AssertEquals("ACC1 is here", 4, result.Select("AccountName in ('ACC6','ACC1','ACC8','ACC5')").Length);
			AssertEquals("ACC2 is not here", 0, result.Select("AccountName in ('ACC2')").Length);
		}

		public void TestFilterSummaryType()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].Finance.GRP__GeneralLedgerAggregateData
						( [GLAccountKey], [PostPeriod], [GLAmountLocalCredit], [GLAmountLocalDebit], [GLAmountLocalBalance], [CompanyKey], [TransactionCategory], [DepartmentKey], [BranchKey])
					VALUES
						(1, 202302, 2, 4, 2, 1, '', 1, 1),
						(2, 202302, 2, 8, 6, 1, '', 1, 1);
				",
				ScriptDbName
			);

			TestConnection.ExecuteNonQuery(sqlText);
			var result = Execute(202305, CompanyPK, new List<string> { "", "", "", "TTLONLY" });
			AssertEquals("Should have 1 Rows ", 1, result.Rows.Count);
			AssertEquals("ACC6 is here", 1, result.Select("AccountName in ('ACC6')").Length);

			result = Execute(202305, CompanyPK, new List<string> { "", "", "", "NETONLY" });
			AssertEquals("Should have 3 Rows ", 3, result.Rows.Count);
			AssertEquals("Net Account is here", 3, result.Select("AccountName in ('ACC1', 'ACC2', 'ACC6')").Length);
		}

		public void TestFilterAdditionalDissection()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].Finance.GRP__GeneralLedgerAggregateData
						( [GLAccountKey], [PostPeriod], [GLAmountLocalCredit], [GLAmountLocalDebit], [GLAmountLocalBalance], [CompanyKey], [TransactionCategory], [DepartmentKey], [BranchKey])
					VALUES
						(1, 202302, 2, 4, 2, 1, '', 1, 1),
						(2, 202302, 2, 8, 6, 1, '', 2, 2);
				",
				ScriptDbName
			);

			TestConnection.ExecuteNonQuery(sqlText);
			var result = Execute(202305, CompanyPK, new List<string> { "", "", "", "", "N", "PNL", "", null, "Branch" });
			AssertEquals("Should have 5 Rows ", 5, result.Rows.Count);
			AssertEquals("ACC1 is here", 2, result.Select("AccountName in ('ACC1') and AdditionalDissection in ('SYN','')").Length);

			result = Execute(202305, CompanyPK, new List<string> { "", "", "", "", "N", "PNL", "", null, "Department" });
			AssertEquals("Should have 5 Rows ", 5, result.Rows.Count);
			AssertEquals("ACC1 is here", 2, result.Select("AccountName in ('ACC1') and AdditionalDissection in ('AUU','')").Length);
		}

		public void TestFilterTransactionCategory()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].Finance.GRP__GeneralLedgerAggregateData
						( [GLAccountKey], [PostPeriod], [GLAmountLocalCredit], [GLAmountLocalDebit], [GLAmountLocalBalance], [CompanyKey], [TransactionCategory], [DepartmentKey], [BranchKey])
					VALUES
						(1, 202302, 2, 4, 2, 1, '345', 1, 1),
						(2, 202302, 2, 8, 6, 1, '', 1, 1),
						(9, 202302, 2, 12, 10, 1, '123', 1, 1);
				",
				ScriptDbName
			);

			TestConnection.ExecuteNonQuery(sqlText);
			var result = Execute(202305, CompanyPK, new List<string> { "", "", "", "", "N", "PNL", "345" });
			AssertEquals("Should have 5 Rows ", 5, result.Rows.Count);
			AssertEquals("ACC1 and ACC2 are here", 5, result.Select("AccountName in ('ACC6','ACC1','ACC8','ACC5','ACC2')").Length);
			AssertEquals("ACC9 is not here", 0, result.Select("AccountName in ('ACC9')").Length);

			result = Execute(202305, CompanyPK, new List<string> { "", "", "", "", "N", "PNL", "" });
			AssertEquals("Should have 4 Rows ", 4, result.Rows.Count);
			AssertEquals("ACC1 is not here", 0, result.Select("AccountName in ('ACC1')").Length);
		}

		public void TestFilterInclZeroBalISY()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].Finance.GRP__GeneralLedgerAggregateData
						( [GLAccountKey], [PostPeriod], [GLAmountLocalCredit], [GLAmountLocalDebit], [GLAmountLocalBalance], [CompanyKey], [TransactionCategory], [DepartmentKey], [BranchKey])
					VALUES
						(1, 202302, 2, 4, 2, 1, '', 1, 1);
				",
				ScriptDbName
			);

			TestConnection.ExecuteNonQuery(sqlText);
			var result = Execute(202305, CompanyPK, new List<string> { "", "", "Y" });
			AssertEquals("Should have 6 Rows ", 6, result.Rows.Count);
			AssertEquals("BSH and ALT are not here", 0, result.Select("AccountName in ('ACC3', 'ACC4', 'ACC7')").Length);
		}

		public void TestReportTypeTBS()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].Finance.GRP__GeneralLedgerAggregateData
						( [GLAccountKey], [PostPeriod], [GLAmountLocalCredit], [GLAmountLocalDebit], [GLAmountLocalBalance], [CompanyKey], [TransactionCategory], [DepartmentKey], [BranchKey])
					VALUES
						(1, 202302, 2, 4, 2, 1, '', 1, 1);
				",
				ScriptDbName
			);

			TestConnection.ExecuteNonQuery(sqlText);
			var result = Execute(202305, CompanyPK, new List<string> { "", "", "", "", "N", "TBS" });
			AssertEquals("Should have 1 Rows ", 1, result.Rows.Count);
			AssertEquals("Units is KG", 1, result.Select("Units='KG' and P0ForCalculateTotal = 0 and P2 = 2 and AdditionalDissection ='' ").Length);
			AssertEquals("No TTL, HDR, CLN", 0, result.Select("AccountName in ('ACC6', 'ACC8', 'ACC5')").Length);
		}

		public void TestReportTypeTBSHasData_WhenPeriodLessThanLastProcessedDate()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].Finance.GRP__GeneralLedgerAggregateData
						( [GLAccountKey], [PostPeriod], [GLAmountLocalCredit], [GLAmountLocalDebit], [GLAmountLocalBalance], [CompanyKey], [TransactionCategory], [DepartmentKey], [BranchKey])
					VALUES
						(3, 202302, 2, 4, 2, 1, '', 1, 1);
				INSERT [{0}].[Finance].[BAS__GLAggregate]
						([GLAggregateKey], [GLAggregateID], [GLAccountKey], [Period], [Amount], [CompanyKey], [TransactionCategory], [DepartmentKey], [BranchKey])
					VALUES
					(1, newid(), 3, 202202, 10, 1, '', 1, 1)
					",
					ScriptDbName
			);

			TestConnection.ExecuteNonQuery(sqlText);
			var result = Execute(202305, CompanyPK, new List<string> { "", "", "", "", "N", "TBS" });
			AssertEquals("Should have 1 Rows ", 1, result.Rows.Count);
			AssertEquals("OpeningBalance should be 10", 10m, result.Rows[0]["P0"]);
		}

		public void TestReportTypeBSH()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].Finance.GRP__GeneralLedgerAggregateData
						( [GLAccountKey], [PostPeriod], [GLAmountLocalCredit], [GLAmountLocalDebit], [GLAmountLocalBalance], [CompanyKey], [TransactionCategory], [DepartmentKey], [BranchKey])
					VALUES
						(1, 202302, 2, 4, 2, 1, '', 1, 1),
						(2, 202303, 2, 5, 3, 1, '', 1, 1),
						(3, 202304, 2, 6, 4, 1, '', 1, 1);
				",
				ScriptDbName
			);

			TestConnection.ExecuteNonQuery(sqlText);
			var result = Execute(202305, CompanyPK, new List<string> { "", "", "", "", "N", "BSH" });
			AssertEquals("Should have 0 Rows ", 0, result.Rows.Count);

			sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].Finance.GRP__GeneralLedgerAggregateData
						( [GLAccountKey], [PostPeriod], [GLAmountLocalCredit], [GLAmountLocalDebit], [GLAmountLocalBalance], [CompanyKey], [TransactionCategory], [DepartmentKey], [BranchKey])
					VALUES
						(11, 202302, 2, 7, 5, 1, '', 1, 1),
						(12, 202303, 2, 8, 6, 1, '', 1, 1),
						(13, 202304, 2, 9, 7, 1, '', 1, 1);

				INSERT [{0}].[Finance].[BAS__GLAccount]
					([GLAccountKey]
					,[GLAccountID]
					,[AccountTypeCode]
					,[AccountNo]
					,[AccountGroup]
					,[Description]
					,[DebitCreditCode]
					,[Units]
					,[SectionType]
					)
					VALUES
						(11, newid(), 'NTE', '11.23.456', 'G', 'ACC11', 'DR' ,'KG','OE'),
						(12, newid(), 'P&L', '21.45.000', 'G', 'ACC12', 'CR','','AS'),
						(13, newid(), 'BSH', '61.87.000', 'G', 'ACC13', 'DR' ,'','LI');
				",
				ScriptDbName
			);

			TestConnection.ExecuteNonQuery(sqlText);

			result = Execute(202305, CompanyPK, new List<string> { "", "", "", "", "N", "BSH" });
			AssertEquals("Should have 3 Rows ", 3, result.Rows.Count);
			AssertEquals("ACC11 has value", 1, result.Select("AccountName='ACC11' and P2=5 and P3=5 and P4=5").Length);
			AssertEquals("ACC12 has value", 1, result.Select("AccountName='ACC12' and P3=-6 and P4=-6 and P5=-6").Length);
			AssertEquals("ACC13 has value", 1, result.Select("AccountName='ACC13' and P4=7 and P5=7").Length);
		}

		public void TestLastProcessedDate()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].Finance.GRP__GeneralLedgerAggregateData
						( [GLAccountKey], [PostPeriod], [GLAmountLocalCredit], [GLAmountLocalDebit], [GLAmountLocalBalance], [CompanyKey], [TransactionCategory], [DepartmentKey], [BranchKey])
					VALUES
						(1, 202302, 2, 4, 2, 1, '', 1, 1),
						(2, 202302, 2, 8, 6, 1, '', 2, 2);
				",
				ScriptDbName
			);

			TestConnection.ExecuteNonQuery(sqlText);
			var result = Execute(202305, CompanyPK, new List<string>());
			AssertEquals("Should have 4 Rows ", 4, result.Rows.Count);

			sqlText = $@"
				DELETE FROM [{ScriptDbName}].[Finance].[BAS__StmDataDate]
				WHERE
					[Name] = 'JournalEntriesLastProcessedDate' AND [CompanyKey] = 1";

			TestConnection.ExecuteNonQuery(sqlText);

			result = Execute(202305, CompanyPK, new List<string>());
			AssertEquals("Should have 3 Rows ", 3, result.Rows.Count);
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
						(9, newid(), 'NTE', '6.99.456', 'G', 'ACC9', 'DEBIT' ,'KCN','OV');

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

				INSERT [{0}].[Finance].[BAS__PeriodManagement]
					([PeriodManagementKey], [PeriodManagementID], [CompanyKey], [StartDate], [EndDate],  [Period], [Year])
					VALUES
						(1, newid(), 1, '2023-08-01', '2023-08-31',  202307, 2023),
						(2, newid(), 1, '2023-07-01', '2023-07-31',  202306, 2023),
						(3, newid(), 1, '2023-06-01', '2023-06-30',  202305, 2023),
						(4, newid(), 1, '2023-05-01', '2023-05-31',  202304, 2023),
						(5, newid(), 1, '2023-03-01', '2023-03-28',  202303, 2023),
						(6, newid(), 1, '2023-02-01', '2023-02-28',  202302, 2023),
						(7, newid(), 1, '2023-01-01', '2023-01-31',  202301, 2023),
						(8, newid(), 1, '2022-12-01', '2022-12-31',  202212, 2022);

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

		DataTable Execute(int period, Guid companyPK, List<string> parameters)
		{
			if (parameters == null)
			{
				parameters = new List<string>(ParameterLength);
			}

			while (parameters.Count < ParameterLength)
			{
				parameters.Add(null);
			}

			StringBuilder sqlBuilder = new StringBuilder();
			sqlBuilder.AppendLine($"EXEC [{ScriptDbName}].[{ScriptToTest.SchemaName}].[{ScriptToTest.Name}]");
			sqlBuilder.Append($"@Period = {period}");
			sqlBuilder.Append(
				$@",@CompanyPK = '{companyPK}'");
			sqlBuilder.Append(
				$@",@DepartmentList = ").Append(parameters[0] == null ? "NULL" : $"'{parameters[0]}'");
			sqlBuilder.Append(
				$@",@BranchList = ").Append(parameters[1] == null ? "NULL" : $"'{parameters[1]}'");
			sqlBuilder.Append(
				$@",@InclZeroBal = ").Append(parameters[2] == null ? "''" : $"'{parameters[2]}'");
			sqlBuilder.Append(
				$@",@SummaryType = ").Append(parameters[3] == null ? "''" : $"'{parameters[3]}'");
			sqlBuilder.Append(
				$@",@IncludeBudget = ").Append(parameters[4] == null ? "'N'" : $"'{parameters[4]}'");
			sqlBuilder.Append(
				$@",@ReportType = ").Append(parameters[5] == null ? "PNL" : $"'{parameters[5]}'");
			sqlBuilder.Append(
				$@",@TransactionCategory = ").Append(parameters[6] == null ? "NULL" : $"'{parameters[6]}'");
			sqlBuilder.Append(
				$@",@BranchManagementCode = ").Append(parameters[7] == null ? "NULL" : $"'{parameters[7]}'");
			sqlBuilder.Append(
				$@",@AdditionalDissection = ").Append(parameters[8] == null ? "''" : $"'{parameters[8]}'");

			var sqlText = string.Format(CultureInfo.InvariantCulture, sqlBuilder.ToString());

			return DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
		}

		static readonly int ParameterLength = 9;
		Guid CompanyPK, DepartmentPK, BranchPK;

		protected override string ScriptDbName => Db.EdwDatabaseName;
	}
}
