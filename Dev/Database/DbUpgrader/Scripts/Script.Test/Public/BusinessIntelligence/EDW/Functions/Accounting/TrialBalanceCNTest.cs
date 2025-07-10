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
	[TestedType(typeof(TrialBalanceCN))]
	class TrialBalanceCNTest : BiCreateScriptTest
	{
		public void TestTrialBalanceCNColumns()
		{
			var whiteList = new List<string>
			{
				"AccountNumber",
				"AccountName",
				"CurrentDebit",
				"CurrentCredit",
				"MovementDebit",
				"MovementCredit",
				"LastDebit",
				"LastCredit",
				"PrintSequence"
			};
			var msgToHint = "If you see this error, you must be adding a new column into #PLConsol table in store procedure 'TrialBalanceCN'.";

			var result = Execute(202301, 202302, CompanyPK, new List<string>());
			AssertEquals(msgToHint, 9, result.Columns.Count);
			foreach (DataColumn column in result.Columns)
			{
				Assert($@"{msgToHint}

Should contain column '{column.ColumnName}'", whiteList.Contains(column.ColumnName));
			}
		}

		public void TestTrialBalanceCNFilterCompanyPK()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].[Finance].[GRP__GeneralLedgerAggregateData]
					( [GLAccountKey], [PostPeriod], [GLAmountLocalCredit], [GLAmountLocalDebit], [CompanyKey], [TransactionCategory], [DepartmentKey], [BranchKey], [GLAmountLocalBalance])
					VALUES
						(1, 202305, 10, 0, 1, '', 1, 1, -10),
						(2, 202304, 10, 0, 1, '', 1, 1, -10),
						(3, 202307, 20, 0, 1, '', 1, 1, -20);
			",
				ScriptDbName
			);

			TestConnection.ExecuteNonQuery(sqlText);
			var result = Execute(202301, 202302, CompanyPK, new List<string>());
			AssertEquals(0, result.Rows.Count);

			result = Execute(202304, 202305, Guid.NewGuid(), new List<string>());
			AssertEquals(0, result.Rows.Count);

			result = Execute(202304, 202305, CompanyPK, new List<string>());
			AssertEquals(1, result.Rows.Count);
			AssertEquals(1, result.Select("AccountNumber = '1000' and PrintSequence = '0031000' and MovementCredit = 10 and LastDebit =0").Length);
		}

		public void TestTrialBalanceCNFilterBranchList()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].[Finance].[GRP__GeneralLedgerAggregateData]
					( [GLAccountKey], [PostPeriod], [GLAmountLocalCredit], [GLAmountLocalDebit], [CompanyKey], [TransactionCategory], [DepartmentKey], [BranchKey], [GLAmountLocalBalance])
					VALUES
						(1, 202305, 10, 0, 1, '', 1, 2, -10),
						(2, 202304, 10, 0, 1, '', 1, 1, -10);

				INSERT [{0}].[Finance].[BAS__GLAccountDescriptor]
					([GLAccountDescriptorKey], [GLAccountDescriptorID], [LocalAccountNumber], [Language], [CountryOfCompliance], [ReportType], [AccountDescription],[PrintSequence])
					VALUES
						(12, newid(), '1030.00.01', 'ZH-CN', 'CN', 'COA', 'LADesc12', 3),
						(13, newid(), '1040.00.01', 'ZH-CN', 'CN', 'COA', 'LADesc13', 3)
				INSERT [{0}].[Finance].[BAS__GLDescriptorPivot]
					([GLDescriptorPivotKey], [GLDescriptorPivotID], [GLAccountKey], [GLAccountDescriptorKey])
					VALUES
						(12, newid(), 2, 12),
						(13, newid(), 4, 13)
			",
				ScriptDbName
			);

			TestConnection.ExecuteNonQuery(sqlText);
			var result = Execute(202304, 202307, CompanyPK, new List<string> { null, "AU1,AU2" });
			AssertEquals("Zero row if BranchCode is wrong", 0, result.Rows.Count);

			result = Execute(202304, 202307, CompanyPK, new List<string> { null, "SYN,AU2" });
			AssertEquals("One row if BranchCode matches", 1, result.Rows.Count);
			AssertEquals(1, result.Select("AccountNumber='1030' and CurrentCredit =10 and LastCredit = 0 and MovementCredit =10").Length);
		}

		public void TestTrialBalanceCNFilterDepartmentList()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].[Finance].[GRP__GeneralLedgerAggregateData]
					( [GLAccountKey], [PostPeriod], [GLAmountLocalCredit], [GLAmountLocalDebit], [CompanyKey], [TransactionCategory], [DepartmentKey], [BranchKey], [GLAmountLocalBalance])
					VALUES
						(1, 202305, 10, 0, 1, '', 1, 1, -10),
						(2, 202304, 10, 0, 1, '', 1, 1, -10),
						(3, 202307, 20, 0, 1, '', 1, 1, -20),
						(1, 202302, 10, 0, 1, '', 1, 1, -10),
						(2, 202301, 10, 0, 1, '', 1, 1, -10),
						(3, 202301, 20, 0, 1, '', 1, 1, -20),
						(4, 202306, 20, 0, 1, '', 1, 1, -20);

				INSERT [{0}].[Finance].[BAS__GLAggregate] 
					([GLAggregateKey], [GLAggregateID], [GLAccountKey], [Period], [Amount], [CompanyKey], [TransactionCategory], [DepartmentKey], [BranchKey])
					VALUES
						(1, newid(), 2, 202301, -10, 1, '', 1, 1),
						(2, newid(), 3, 202301, -20, 1, '', 2, 1),
						(3, newid(), 3, 202301, -20, 1, '', 1, 1)

				INSERT [{0}].[Finance].[BAS__GLAccountDescriptor]
					([GLAccountDescriptorKey], [GLAccountDescriptorID], [LocalAccountNumber], [Language], [CountryOfCompliance], [ReportType], [AccountDescription],[PrintSequence])
					VALUES
						(12, newid(), '1030.00.01', 'ZH-CN', 'CN', 'COA', 'LADesc12', 3),
						(13, newid(), '1040.00.01', 'ZH-CN', 'CN', 'COA', 'LADesc13', 3)
				INSERT [{0}].[Finance].[BAS__GLDescriptorPivot]
					([GLDescriptorPivotKey], [GLDescriptorPivotID], [GLAccountKey], [GLAccountDescriptorKey])
					VALUES
						(12, newid(), 2, 12),
						(13, newid(), 4, 13)
			",
				ScriptDbName
			);

			TestConnection.ExecuteNonQuery(sqlText);
			var result = Execute(202304, 202307, CompanyPK, new List<string> { "AU1,AU2" } );
			AssertEquals(4, result.Rows.Count);
			AssertEquals(0, result.Select("AccountNumber='4000'").Length);
			AssertEquals(1, result.Select("AccountNumber='1040' and CurrentCredit =30 and LastCredit = 10 and MovementCredit =20").Length);
			AssertEquals(1, result.Select("AccountNumber='1030' and CurrentCredit =10 and LastCredit = 0 and MovementCredit =10").Length);
			AssertEquals(1, result.Select("AccountNumber='3000' and CurrentCredit =40 and LastCredit = 20 and MovementCredit =20").Length);

			result = Execute(202304, 202307, CompanyPK, new List<string> { "BU2" });
			AssertEquals(0, result.Rows.Count);
		}

		public void TestTrialBalanceCNFilterPeriod()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].[Finance].[GRP__GeneralLedgerAggregateData]
					( [GLAccountKey], [PostPeriod], [GLAmountLocalCredit], [GLAmountLocalDebit], [CompanyKey], [TransactionCategory], [DepartmentKey], [BranchKey], [GLAmountLocalBalance])
					VALUES
						(1, 202305, 10, 0, 1, '', 1, 1, -10),
						(2, 202304, 10, 0, 1, '', 1, 1, -10),
						(3, 202307, 20, 0, 1, '', 1, 1, -20);
			",
				ScriptDbName
			);

			TestConnection.ExecuteNonQuery(sqlText);
			var result = Execute(202304, 202305, CompanyPK, new List<string>());
			AssertEquals(1, result.Rows.Count);
			AssertEquals(1, result.Select("AccountNumber = '1000' and PrintSequence = '0031000' and MovementCredit = 10 and LastDebit =0").Length);

			result = Execute(202304, 202310, CompanyPK, new List<string>());
			AssertEquals(2, result.Rows.Count);
			AssertEquals(1, result.Select("AccountNumber = '1000' and PrintSequence = '0031000' and MovementCredit = 10 and LastDebit =0").Length);
			AssertEquals(1, result.Select("AccountNumber = '3000' and PrintSequence = '0033000' and MovementCredit = 20 and LastDebit =0 and CurrentCredit = 20").Length);
		}

		public void TestTrialBalanceCNFilterLanguage()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].[Finance].[GRP__GeneralLedgerAggregateData]
					( [GLAccountKey], [PostPeriod], [GLAmountLocalCredit], [GLAmountLocalDebit], [CompanyKey], [TransactionCategory], [DepartmentKey], [BranchKey], [GLAmountLocalBalance])
					VALUES
						(1, 202305, 10, 0, 1, '', 1, 1, -10),
						(2, 202304, 10, 0, 1, '', 1, 1, -10),
						(9, 202305, 20, 0, 1, '', 1, 1, -20);
			",
				ScriptDbName
			);

			TestConnection.ExecuteNonQuery(sqlText);
			var result = Execute(202304, 202305, CompanyPK, new List<string> { null, null, null, "ZH" });
			AssertEquals(1, result.Rows.Count);
			AssertEquals(1, result.Select("AccountNumber = '8000' and PrintSequence = '0038000' and MovementCredit = 20 and LastDebit =0").Length);

			result = Execute(202304, 202305, CompanyPK, new List<string> { null, null, null, "ZH-CN" });
			AssertEquals(1, result.Rows.Count);
			AssertEquals(1, result.Select("AccountNumber = '1000' and PrintSequence = '0031000' and MovementCredit = 10 and LastDebit =0").Length);
		}

		public void TestTrialBalanceCNFilterCountryCode()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].[Finance].[GRP__GeneralLedgerAggregateData]
					( [GLAccountKey], [PostPeriod], [GLAmountLocalCredit], [GLAmountLocalDebit], [CompanyKey], [TransactionCategory], [DepartmentKey], [BranchKey], [GLAmountLocalBalance])
					VALUES
						(1, 202305, 10, 0, 1, '', 1, 1, -10),
						(6, 202304, 10, 0, 1, '', 1, 1, -10),
						(9, 202305, 20, 0, 1, '', 1, 1, -20);
			",
				ScriptDbName
			);

			TestConnection.ExecuteNonQuery(sqlText);
			var result = Execute(202304, 202305, CompanyPK, new List<string> { null, null, null, "ZH" });
			AssertEquals(1, result.Rows.Count);
			AssertEquals(1, result.Select("AccountNumber = '8000' and PrintSequence = '0038000' and MovementCredit = 20 and LastDebit =0").Length);

			result = Execute(202304, 202305, CompanyPK, new List<string> { null, null, null, "ZH", "C6" });
			AssertEquals(1, result.Rows.Count);
			AssertEquals(1, result.Select("AccountNumber = '8000'").Length);
		}

		public void TestLastProcessedDate()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].[Finance].[GRP__GeneralLedgerAggregateData]
					( [GLAccountKey], [PostPeriod], [GLAmountLocalCredit], [GLAmountLocalDebit], [CompanyKey], [TransactionCategory], [DepartmentKey], [BranchKey], [GLAmountLocalBalance])
					VALUES
						(1, 202202, 10, 0, 1, '', 1, 1, -10),
						(6, 202201, 10, 0, 1, '', 1, 1, -10),
						(9, 202204, 20, 0, 1, '', 1, 1, -20);
				Update [{0}].[Finance].[BAS__StmDataDate] set [Value] = '2022-04-01';
				INSERT [{0}].[Finance].[BAS__GLAggregate] 
					([GLAggregateKey], [GLAggregateID], [GLAccountKey], [Period], [Amount], [CompanyKey], [TransactionCategory], [DepartmentKey], [BranchKey])
					VALUES
						(1, newid(), 1, 202201, 4, 1, '', 1, 1),
						(2, newid(), 2, 202201, 3, 1, '', 1, 1),
						(3, newid(), 3, 202202, 4, 1, '', 1, 1),
						(4, newid(), 4, 202202, 3, 1, '', 1, 1)
			",
				ScriptDbName
			);

			TestConnection.ExecuteNonQuery(sqlText);
			var result = Execute(202201, 202202, CompanyPK, new List<string> { null, null, null, "ZH-CN" });
			AssertEquals(2, result.Rows.Count);
		}

		public void TestTrialBalanceCNWithIncludeZeroBalance()
		{
			var stmXmlInfo = @"<?xml version=""1.0"" encoding=""utf-16""?><ArrayOfGLLocalNumberFormat>
<GLLocalNumberFormat><Language>ZH</Language><CountryCode>CN</CountryCode><NumberFormat>4-4-4</NumberFormat><IsFixedLength>N</IsFixedLength></GLLocalNumberFormat>
<GLLocalNumberFormat><Language>EN-US</Language><CountryCode>CN</CountryCode><NumberFormat>4-2-3</NumberFormat><IsFixedLength>N</IsFixedLength></GLLocalNumberFormat>
</ArrayOfGLLocalNumberFormat>";

			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].[Finance].[GRP__GeneralLedgerAggregateData]
					( [GLAccountKey], [PostPeriod], [GLAmountLocalCredit], [GLAmountLocalDebit], [CompanyKey], [TransactionCategory], [DepartmentKey], [BranchKey], [GLAmountLocalBalance])
					VALUES
						(1, 202305, 10, 0, 1, '', 1, 1, -10),
						(6, 202304, 10, 0, 1, '', 1, 1, -10),
						(9, 202305, 20, 0, 1, '', 1, 1, -20);
				INSERT [{0}].[Finance].[BAS__LocalNumberFormatData]
					( [LocalNumberFormatDataID], [LocalNumberFormatDataKey], [LocalNumberFormatDataValue])
					VALUES
						(newid(), 1, '{1}')

				INSERT [{0}].[Finance].[BAS__GLAccountDescriptor]
					([GLAccountDescriptorKey], [GLAccountDescriptorID], [LocalAccountNumber], [Language], [CountryOfCompliance], [ReportType], [AccountDescription],[PrintSequence])
					VALUES
						(12, newid(), '8000', 'ZH', 'CN', 'COA', 'LADesc12', 3)
			",
				ScriptDbName, stmXmlInfo
			);

			TestConnection.ExecuteNonQuery(sqlText);

			var result = Execute(202304, 202305, CompanyPK, new List<string> { null, null, null, "ZH", "CN", "Y" });
			AssertEquals(2, result.Rows.Count);
			AssertEquals(1, result.Select("AccountNumber = '8000' and PrintSequence = '0038000' and MovementCredit = 20 and LastDebit =0 and AccountName = 'LADesc12'").Length);
			AssertEquals(1, result.Select("AccountNumber = '4000' and PrintSequence = '0034000' and MovementCredit = 0 and LastDebit =0 and AccountName is null").Length);
		}

		protected override void SetUp()
		{
			base.SetUp();
			PrepareBaseTestData();
		}

		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}

		void PrepareBaseTestData()
		{
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
						(3, newid(), 'BSH', '6.87.000', 'G', 'ACC3', 'DEBIT' ,''),
						(4, newid(), 'BSH', '6.97.000', 'G', 'ACC4', 'DEBIT' ,''),
						(5, newid(), 'CLN', '1.23.600', 'G', 'ACC5', 'DEBIT' ,''),
						(6, newid(), 'TTL', '2.00'    , 'G', 'ACC6', 'CREDIT',''),
						(7, newid(), 'ALT', '2.00'    , 'G', 'ACC7', 'CREDIT',''),
						(8, newid(), 'HDR', '3'       , 'G', 'ACC8', 'DEBIT' ,''),
						(9, newid(), 'NTE', '6.99.456', 'G', 'ACC9', 'DEBIT' ,'KCN');
				INSERT [{0}].[Finance].[BAS__GLAccountDescriptor]
					([GLAccountDescriptorKey], [GLAccountDescriptorID], [LocalAccountNumber], [Language], [CountryOfCompliance], [ReportType], [AccountDescription],[PrintSequence])
					VALUES
						(1, newid(), '1000.00.01', 'ZH-CN', 'CN', 'COA', 'LADesc1', 3),
						(2, newid(), '2000.00.03', 'ZH-CN', 'CN', 'BOA', 'LADesc2', 3),
						(3, newid(), '3000.00.01', 'ZH-CN', 'CN', 'COA', 'LADesc3', 3),
						(4, newid(), '4000.00.03', 'ZH', 'CN', 'BOA', 'LADesc4', 3),
						(5, newid(), '5000.00.01', 'ZH', 'C5', 'COA', 'LADesc5', 3),
						(6, newid(), '6000.00.03', 'ZH', 'C6', 'COA', 'LADesc6', 3),
						(7, newid(), '7000.00.01', 'ZH-CN', 'C7', 'COA', 'LADesc7', 3),
						(8, newid(), '8000.00.03', 'ZH', 'CN', 'COA', 'LADesc8', 3);

				INSERT [{0}].[Finance].[BAS__GLDescriptorPivot]
					([GLDescriptorPivotKey], [GLDescriptorPivotID], [GLAccountKey], [GLAccountDescriptorKey])
					VALUES
						(1, newid(), 1, 1),
						(2, newid(), 2, 2),
						(3, newid(), 3, 3),
						(4, newid(), 4, 4),
						(5, newid(), 5, 5),
						(6, newid(), 6, 6),
						(7, newid(), 7, 7),
						(8, newid(), 9, 8);

				INSERT [{0}].[Organization].[BAS__Branch]
					([BranchKey], [BranchID], [CompanyKey], [BranchCode], [OrganizationKey])
					VALUES
						(1, newid(), 1, 'SYN', 1),
						(2, newid(), 2, 'DTW', 2);

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

				INSERT [{0}].[Finance].[BAS__PeriodManagement]
					([PeriodManagementKey], [PeriodManagementID], [CompanyKey], [StartDate], [EndDate],  [Period], [Year])
					VALUES
						(1, newid(), 1, '2023-08-01', '2023-08-31',  202308, 2023),
						(2, newid(), 1, '2023-07-01', '2023-07-31',  202307, 2023),
						(3, newid(), 1, '2023-06-01', '2023-06-30',  202306, 2023),
						(4, newid(), 1, '2023-05-01', '2023-05-31',  202305, 2023),
						(5, newid(), 1, '2023-04-01', '2023-04-30',  202304, 2023),
						(6, newid(), 1, '2023-03-01', '2023-03-31',  202303, 2023),
						(7, newid(), 1, '2023-02-01', '2023-02-28',  202302, 2023),
						(8, newid(), 1, '2022-02-01', '2022-02-28',  202202, 2022),
						(9, newid(), 1, '2022-01-01', '2022-01-28',  202201, 2022);

				INSERT [{0}].[Finance].[BAS__GLAggregate] 
					([GLAggregateKey], [GLAggregateID], [GLAccountKey], [Period], [Amount], [CompanyKey], [TransactionCategory], [DepartmentKey], [BranchKey])
					VALUES
						(1, newid(), 1, 202105, 4, 1, '', 1, 1),
						(2, newid(), 2, 202201, 0, 1, '', 1, 1),
						(3, newid(), 3, 202201, 0, 1, '', 1, 1);

				INSERT [{0}].[Customs].[BAS__Account]
					([AccountKey], [AccountID], [GLAccountKey], [AccountType])
					VALUES
						(1, newid(), 4, 'GL_PL_APPROPRIATION_ACCOUNT'),
						(2, newid(), 3, 'GL_BS_ACCOUNT_START');

				INSERT [{0}].[Finance].[BAS__StmDataDate]
					([StmDataDateKey], [StmDataDateID], [Name], [CompanyID], [Value], CompanyKey)
					VALUES
						(1, newid(), 'JournalEntriesLastProcessedDate', '{1}', '2023-02-01 00:00:00.000', 1);
				",
				ScriptDbName, CompanyPK
			);

			TestConnection.ExecuteNonQuery(sqlText);
		}

		DataTable Execute(int startPeriod, int endPeriod, Guid companyPK, List<string> parameters)
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
			sqlBuilder.Append(
				$@"@StartPeriod = ").Append(startPeriod == 0 ? "NULL" : startPeriod.ToString());
			sqlBuilder.Append(
				$@",@Period = ").Append(endPeriod == 0 ? "NULL" : endPeriod.ToString());
			sqlBuilder.Append(
				$@",@CompanyPK = ").Append($"'{companyPK}'");
			sqlBuilder.Append(
				$@",@DepartmentList = ").Append(parameters[0] == null ? "NULL" : $"'{parameters[0]}'");
			sqlBuilder.Append(
				$@",@BranchList = ").Append(parameters[1] == null ? "NULL" : $"'{parameters[1]}'");
			sqlBuilder.Append(
				$@",@ReportType = ").Append(parameters[2] == null ? "NULL" : $"'{parameters[2]}'");
			sqlBuilder.Append(
				$@",@Language = ").Append(parameters[3] == null ? "'ZH-CN'" : $"'{parameters[3]}'");
			sqlBuilder.Append(
				$@",@CountryCode = ").Append(parameters[4] == null ? "'CN'" : $"'{parameters[4]}'");
			sqlBuilder.Append(
				$@",@InclZeroBal = ").Append(parameters[5] == null ? "'N'" : $"'{parameters[5]}'");

			var sqlText = string.Format(CultureInfo.InvariantCulture,
				sqlBuilder.ToString()
			);

			return DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
		}

		static readonly int ParameterLength = 6;
		Guid CompanyPK;
	}
}
