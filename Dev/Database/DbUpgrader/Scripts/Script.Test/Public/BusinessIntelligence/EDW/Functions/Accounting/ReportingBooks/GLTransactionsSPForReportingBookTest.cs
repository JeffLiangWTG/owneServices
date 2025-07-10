using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting.ReportingBooks;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Functions.Accounting.Testing
{
	[TestedType(typeof(GLTransactionsSPForReportingBook))]
	class GLTransactionsSPForReportingBookTest : BiCreateScriptTest
	{
		public void TestColumns()
		{
			var whiteList = new List<string>
			{
				"PK",
				"TransactionType",
				"InvoiceDate",
				"PostDate",
				"Branch",
				"Department",
				"Ledger",
				"TransactionNum",
				"SecondRef",
				"TransactionDesc",
				"AlternateGLAccount",
				"AlternateGLAccountDesc",
				"Units",
				"Job",
				"Account",
				"ChargeCode",
				"ChargeCodeDescription",
				"LocalLanguageChargeCodeDescription",
				"Period",
				"Amount",
				"Debit",
				"Credit",
				"OpeningPeriodDate",
				"ClosingPeriodDate",
				"OpeningBalance",
				"ClosingBalance",
				"ExRate",
				"Currency",
				"OsExTaxDebit",
				"OsExTaxCredit",
				"ComplianceSubType",
				"ComplianceNumber",
				"GLAccountNo",
				"GLAccountDesc",
				"GLAccountKey",
				"Attribute_ORG",
				"Attribute_OCG",
				"Attribute_LFE",
				"Attribute_LFO",
				"Attribute_TIC",
				"Attribute_SPR",
				"TranslatedExRate",
				"TranslatedCurrency",
				"LocalDebit",
				"LocalCredit"
			};

			var msgToHint = $@"If you see this error, you must be adding a new column into #Transactions table in store procedure 'GLTransactionsSPForReportingBook'.
Please follow below steps: 

1> Read through all parts in store procedure 'GLTransactionsSPForReportingBook' and understand their functional purpose.
2> Add the new column into whiteList.
";

			var result = Execute(ReportingBookPK, CompanyPK);
			AssertEquals("Amount should not be null", 0, result.Select("Amount is null").Length);
			AssertEquals(msgToHint, whiteList.Count, result.Columns.Count);
			foreach (DataColumn column in result.Columns)
			{
				Assert($@"{msgToHint}

Should contain column '{column.ColumnName}'", whiteList.Contains(column.ColumnName));
			}
		}

		public void TestGeneralLedgerTransactionDataFilterPeriod()
		{
			var result = Execute(ReportingBookPK, CompanyPK, 202303, 202306);
			AssertEquals("Should be 2 rows in report", 2, result.Rows.Count);
			AssertEquals(1, result.Select("AlternateGLAccount = '1231' and Period = 202303 and PK = 1 and TransactionType = 'DPY' and InvoiceDate = '2023-09-08 00:00:00' and PostDate = '2023-03-07 00:00:00' and Branch = 'UUU' and Department = 'YYY' and Ledger ='AP' and TransactionNum = '121' and SecondRef = '456' and TransactionDesc = 'lineDesc' and Units = 'KG' and Job = 'JJJoiu'").Length);
			AssertEquals(1, result.Select("AlternateGLAccount = '1232' and Period = 202304 and SecondRef = '' and Account = 'UUU' and ChargeCode = 'cuu' and ChargeCodeDescription ='cdesc' and LocalLanguageChargeCodeDescription ='clocaldesc' and Amount = -3 and ExRate = 3.4 and GLAccountNo = '10100019'").Length);

			result = Execute(ReportingBookPK, CompanyPK, 202303, 202310);
			AssertEquals("Should be 5 rows in report", 5, result.Rows.Count);
			AssertEquals(1, result.Select("AlternateGLAccount = '1231' and Period = 202303").Length);
			AssertEquals(1, result.Select("AlternateGLAccount = '1234' and Period = 100001").Length);
			AssertEquals(1, result.Select("SecondRef = 'Jiu03' and TransactionType ='INV' and Ledger ='AR'").Length);
			AssertEquals(1, result.Select("AlternateGLAccount = '1235' and Period = 202310 and Attribute_ORG = 'NAV-No Attribute Value' and Attribute_OCG = 'INT' and Attribute_LFE = 'LOC-Local' and Attribute_LFO = 'FOR-Foreign'").Length);
		}

		public void TestGeneralLedgerTransactionDataFilterDate()
		{
			var result = Execute(ReportingBookPK, CompanyPK, 0, 0, "2023-02-01", "2023-04-01");
			AssertEquals(1, result.Select("AlternateGLAccount = '1231' and Period = 202303").Length);
		}

		public void TestGeneralLedgerTransactionDataFilterGLAccount()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				UPDATE [{0}].[Finance].BAS__AccReportingBook SET RX_NKCurrency = 'USD'
				INSERT [{0}].[Finance].[GRP__ReportingBookGeneralLedgerAggregateData]
					(BranchKey, CompanyKey, DepartmentKey, GLAmountLocalBalance, PostPeriod, TransactionCategory, AlternateGLAccountKey, AccountTypeCode, ReportingBookKey, GLAccountKey, AlternateChartKey, IsReciprocal, SubUnitRatio, CurrencyTranslationLevel, LocalCurrency, TranslatedCurrency, TranslatedRateType, TranslatedCredit, TranslatedDebit, TranslatedBalance)
				VALUES
					(1, 1, 1, 1, 202205, 'UUU', 1, 'P&L', 1, 1, 1, 1, 100, 'JNL', 'CNY', 'CNY', 'SEL', 1, 5, -1),
					(1, 1, 1, 1, 100001, 'UUU', 1, 'P&L', 1, 1, 1, 1, 100, 'JNL', 'CNY', 'USD', 'SEL', 1, 5, -2),
					(1, 1, 1, 1, 202301,    '', 1, 'P&L', 1, 1, 1, 1, 100, 'JNL', 'CNY', 'USD', 'SEL', 1, 5, -1)
				",
				ScriptDbName, CompanyPK
			);
			TestConnection.ExecuteNonQuery(sqlText);
			TestHelper.InsertAlternateGLAccount("333251", "P&L", 1);
			TestHelper.InsertAlternateGLAccount("343251", "P&L", 1);
			TestHelper.InsertAlternateGLAccount("353251", "P&L", 1);
			TestHelper.InsertAlternateGLAccount("363251", "P&L", 1);
			TestHelper.InsertAlternateGLAccount("373251", "P&L", 1);

			var result = Execute(ReportingBookPK, CompanyPK, 202301, 202312, startGLAccountPK: StartGLAccountPK, endGLAccountPK: EndGLAccountPK);
			AssertEquals("Should be 2 rows in report when GLAccount is limited", 2, result.Rows.Count);
			AssertEquals(1, result.Select("AlternateGLAccount = '1231' AND Attribute_OCG = 'INT' AND Attribute_ORG = 'OR1'").Length);
			AssertEquals(1, result.Select("AlternateGLAccount = '1232' AND Attribute_LFE = 'OEU-Outside EU'").Length);

			result = Execute(ReportingBookPK, CompanyPK, 202301, 202312, startGLAccountPK: EndGLAccountPK);
			AssertEquals("Should be 4 rows in report when GLAccount is limited", 4, result.Rows.Count);
			AssertEquals(1, result.Select("AlternateGLAccount = '1439' AND Amount = 0 AND OpeningBalance = -1").Length);
			AssertEquals(1, result.Select("AlternateGLAccount = '1234' AND Amount = -2.5").Length);
			AssertEquals(0, result.Select("AlternateGLAccount = '1231'").Length);
		}

		public void TestGeneralLedgerTransactionDataWithDisplayDesc()
		{
			var result = Execute(ReportingBookPK, CompanyPK, 202301, 202312, desc: "H");
			AssertEquals("Should be 5 rows in report", 5, result.Rows.Count);
			AssertEquals(4, result.Select("TransactionDesc = 'HDesc'").Length);
			AssertEquals(1, result.Select("TransactionDesc = 'lineDesc' and TransactionType = 'WIP'").Length);
		}

		public void TestGeneralLedgerTransactionDataFilterTransactionCategory()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].[Finance].[CUS__GeneralLedgerTransactionData]
					(CompanyKey, BranchKey, Currency, DepartmentKey, GLAccountKey, GLAccountName, GLAccountNum, GLAmountLocalCredit, GLAmountLocalDebit, GLAmountLocalBalance, GLAmountOSCredit, GLAmountOSDebit, GLAmountOSBalance, TransactionCategory, GeneralLedgerDataKey, PostDate, CompanyID,
					TransactionDate, TransactionNum, TransactionType, LineJobKey, LineDescription, HeaderDescription, LedgerCode, JobInvoiceNumber, ExchangeRate, ComplianceSubType, ComplianceNumber, ChequeOrReference, ChargeCodeKey, OrganizationKey, TransactionLineKey)
				VALUES
					(1, 1, 'CNY', 1, 1, 'GLAccount1', '10100009', 2, 0, -2, 3, 0, -3, 'UUU', 15, '2023-02-07', '{1}', '2023-08-08', '121', 'DPY', 1, 'lineDesc', 'HDesc', 'AP', 'Jiu01', 3.4, 'ert', '345', '456', 1, 1, 4)

			INSERT [{0}].[Finance].[CUS__GeneralLedgerTranslatedData]
				(GeneralLedgerDataKey, Attribute_OCG, Attribute_LFE, Attribute_LFO, Attribute_SPR, Attribute_TIC, Attribute_ORG, PostPeriodForReportingBook, ReportingBookKey, LocalCurrency, TranslatedCurrency, AccountTypeCode, CurrencyTranslationLevel, TranslatedAmount, IsReciprocal, TranslatedRateType, SubUnitRatio)
			VALUES
				(15, 'INT', 'WEU', 'LOC', 'SPS', 'ETI', 'OR1', 202302, 1, 'CNY', 'USD', 'BSH', 'BAL',    2, 1, 'SEL', 100)",
				ScriptDbName, CompanyPK
			);
			TestConnection.ExecuteNonQuery(sqlText);

			var result = Execute(ReportingBookPK, CompanyPK, 202301, 202304);
			AssertEquals("Should be 3 rows in report", 3, result.Rows.Count);
			AssertEquals(1, result.Select("Period = 202302").Length);
			AssertEquals(1, result.Select("Period = 202303").Length);
			AssertEquals(1, result.Select("Period = 202304").Length);

			sqlText = string.Format(CultureInfo.InvariantCulture, @"
				UPDATE [{0}].[Finance].[BAS__AccReportingBook] SET IncludePresentationJournals = 'YYY'",
				ScriptDbName
			);
			TestConnection.ExecuteNonQuery(sqlText);

			result = Execute(ReportingBookPK, CompanyPK, 202301, 202304);
			AssertEquals("Should be 2 rows in report", 2, result.Rows.Count);
			AssertEquals(0, result.Select("Period = 202302").Length);
		}

		public void TestGeneralLedgerTransactionDataWithIncludeZeroBalance()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				UPDATE [{0}].[Finance].[BAS__AccReportingBook] SET RX_NKCurrency = 'USD'",
				ScriptDbName
			);
			TestConnection.ExecuteNonQuery(sqlText);
			var result = Execute(ReportingBookPK, CompanyPK, 202301, 202312, includeZeroBalance: "Y");
			AssertEquals("Should be 10 rows in report", 10, result.Rows.Count);
			AssertEquals(5, result.Select("Period is null and Amount = 0").Length);
			AssertEquals(5, result.Select("Amount <> 0 ").Length);
		}

		#region Opening Balance

		public void TestOpeningBalanceWithTransactionCategory()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].[Finance].[GRP__ReportingBookGeneralLedgerAggregateData]
					(BranchKey, CompanyKey, DepartmentKey, GLAmountLocalBalance, PostPeriod, TransactionCategory, AlternateGLAccountKey, AccountTypeCode, ReportingBookKey, GLAccountKey, AlternateChartKey, IsReciprocal, SubUnitRatio, CurrencyTranslationLevel, LocalCurrency, TranslatedCurrency, TranslatedRateType, TranslatedCredit, TranslatedDebit, TranslatedBalance)
				VALUES
					(1, 1, 1, 1, 202205, 'UUU', 1, 'P&L', 1, 1, 1, 1, 100, 'BAL', 'CNY', 'CNY', 'SEL', 1, 5, -1),
					(1, 1, 1, 1, 100001, 'UUU', 1, 'P&L', 1, 1, 1, 1, 100, 'BAL', 'CNY', 'USD', 'SEL', 1, 5, -1),
					(1, 1, 1, 1, 202301,    '', 1, 'P&L', 1, 1, 1, 1, 100, 'BAL', 'CNY', 'USD', 'SEL', 1, 5, -1),
					(1, 1, 1, 1, 100001, 'UUU', 3, 'BSH', 1, 1, 1, 1, 100, 'JNL', 'CNY', 'USD', 'SEL', 1, 5, -1),
					(1, 1, 1, 1, 202301,    '', 3, 'BSH', 1, 1, 1, 1, 100, 'JNL', 'CNY', 'USD', 'SEL', 1, 5, -1),
					(1, 1, 1, 1, 202201,    '', 3, 'BSH', 1, 1, 1, 1, 100, 'JNL', 'CNY', 'USD', 'SEL', 1, 5, -1),
					(1, 1, 1, 1, 202003, 'UUU', 3, 'BSH', 1, 1, 1, 1, 100, 'JNL', 'CNY', 'USD', 'SEL', 1, 5, -1),
					(1, 1, 1, 1, 202205, 'UUU', 8, 'P&L', 1, 2, 1, 1, 100, 'JNL', 'CNY', 'USD', 'SEL', 1, 5, -1),
					(1, 1, 1, 1, 100001,    '', 8, 'P&L', 1, 2, 1, 1, 100, 'JNL', 'CNY', 'USD', 'SEL', 1, 5, 11),
					(1, 1, 1, 1, 202301,    '', 8, 'P&L', 1, 2, 1, 1, 100, 'JNL', 'CNY', 'USD', 'SEL', 1, 5, 11)
				UPDATE [{0}].[Finance].BAS__AccReportingBook SET RX_NKCurrency = 'USD'
				",
				ScriptDbName
			);
			TestConnection.ExecuteNonQuery(sqlText);

			var result = Execute(ReportingBookPK, CompanyPK, 202303, 202312);
			AssertEquals("Should be 7 rows in report", 7, result.Rows.Count);
			AssertEquals(1, result.Select("AlternateGLAccount = '1439' and Amount = 0 and OpeningBalance = 11.5").Length);
			AssertEquals(1, result.Select("AlternateGLAccount = '1231' and Amount = -1 and OpeningBalance = 0.5").Length);
			AssertEquals(1, result.Select("AlternateGLAccount = '1232' and Amount = -3 and OpeningBalance = 0 and Debit is null and Credit = 3 and LocalDebit is null and LocalCredit = 3").Length);
			AssertEquals(1, result.Select("AlternateGLAccount = '1233' and OpeningBalance = 4 and localCredit = 4 and Amount = -4").Length);

			sqlText = string.Format(CultureInfo.InvariantCulture, @"
				UPDATE [{0}].[Finance].[BAS__AccReportingBook] SET IncludePresentationJournals = 'YYY'",
				ScriptDbName
			);
			TestConnection.ExecuteNonQuery(sqlText);
			result = Execute(ReportingBookPK, CompanyPK, 202303, 202312);
			AssertEquals("Should be 7 rows in report", 7, result.Rows.Count);
			AssertEquals(1, result.Select("AlternateGLAccount = '1439' and Amount = 0 and OpeningBalance = 11").Length);
			AssertEquals(1, result.Select("AlternateGLAccount = '1232' and Amount = -3 and OpeningBalance = 0").Length);
			AssertEquals(1, result.Select("AlternateGLAccount = '1233' and OpeningBalance = 2").Length);
		}

		public void TestOpeningBalanceWithPLAppropriationGLAccount()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				UPDATE [{0}].[Finance].BAS__AccReportingBook SET RX_NKCurrency = 'USD'
				INSERT [{0}].[Finance].[GRP__ReportingBookGeneralLedgerAggregateData]
					(BranchKey, CompanyKey, DepartmentKey, GLAmountLocalBalance, PostPeriod, TransactionCategory, AlternateGLAccountKey, AccountTypeCode, ReportingBookKey, GLAccountKey, AlternateChartKey, IsReciprocal, SubUnitRatio, CurrencyTranslationLevel, LocalCurrency, TranslatedCurrency, TranslatedRateType, TranslatedCredit, TranslatedDebit, TranslatedBalance)
				VALUES
					(1, 1, 1, 1, 202205, 'UUU', 1, 'P&L', 1, 1, 1, 1, 100, 'JNL', 'CNY', 'CNY', 'SEL', 1, 5, -1),
					(1, 1, 1, 1, 100001, 'UUU', 1, 'P&L', 1, 1, 1, 1, 100, 'JNL', 'CNY', 'USD', 'SEL', 1, 5, -2),
					(1, 1, 1, 1, 202301,    '', 1, 'P&L', 1, 1, 1, 1, 100, 'JNL', 'CNY', 'USD', 'SEL', 1, 5, -1),
					(1, 1, 1, 1, 100001, 'UUU', 7, 'BSH', 1, 3, 1, 1, 100, 'BAL', 'CNY', 'USD', 'SEL', 1, 5, -1),
					(1, 1, 1, 1, 202301,    '', 7, 'BSH', 1, 3, 1, 1, 100, 'BAL', 'CNY', 'USD', 'SEL', 1, 5, -1),
					(1, 1, 1, 1, 202201,    '', 7, 'BSH', 1, 3, 1, 1, 100, 'BAL', 'CNY', 'USD', 'SEL', 1, 5, -1),
					(1, 1, 1, 1, 202003, 'UUU', 3, 'BSH', 1, 1, 1, 1, 100, 'BAL', 'CNY', 'USD', 'SEL', 1, 5, -1),
					(1, 1, 1, 1, 202205, 'UUU', 8, 'P&L', 1, 2, 1, 1, 100, 'JNL', 'CNY', 'USD', 'SEL', 1, 5, -1),
					(1, 1, 1, 1, 100001,    '', 8, 'P&L', 1, 2, 1, 1, 100, 'JNL', 'CNY', 'USD', 'SEL', 1, 5, 11),
					(1, 1, 1, 1, 202301,    '', 8, 'P&L', 1, 2, 1, 1, 100, 'JNL', 'CNY', 'USD', 'SEL', 1, 5, 11)
				INSERT [{0}].[Finance].[CUS__GeneralLedgerTransactionData]
					(CompanyKey, BranchKey, Currency, DepartmentKey, GLAccountKey, GLAccountName, GLAccountNum, GLAmountLocalCredit, GLAmountLocalDebit, GLAmountLocalBalance, GLAmountOSCredit, GLAmountOSDebit, GLAmountOSBalance, TransactionCategory, GeneralLedgerDataKey, PostDate, CompanyID,
					TransactionDate, TransactionNum, TransactionType, LineJobKey, LineDescription, HeaderDescription, LedgerCode, JobInvoiceNumber, ExchangeRate, ComplianceSubType, ComplianceNumber, ChequeOrReference, ChargeCodeKey, OrganizationKey, TransactionLineKey)
				VALUES
					(1, 1, 'CNY', 1, 3, 'GLAccount1', '10100009', 2, 0, -2, 3, 0, -3, '', 7, '2023-01-07', '{1}', '2023-01-08', '121', 'DPY', 1, 'lineDesc', 'HDesc', 'AP', 'Jiu01', 3.4, 'ert', '345', '456', 1, 1, 4),
					(1, 1, 'CNY', 1, 3, 'GLAccount1', '10100009', 2, 0, -2, 3, 0, -3, '', 8, '2023-02-07', '{1}', '2023-02-08', '121', 'DPY', 1, 'lineDesc', 'HDesc', 'AP', 'Jiu01', 3.4, 'ert', '345', '456', 1, 1, 4),
					(1, 1, 'CNY', 1, 3, 'GLAccount1', '10100009', 2, 0, -2, 3, 0, -3, '', 9, '2023-03-07', '{1}', '2023-02-08', '121', 'DPY', 1, 'lineDesc', 'HDesc', 'AP', 'Jiu01', 3.4, 'ert', '345', '456', 1, 1, 4)

				INSERT [{0}].[Finance].[CUS__GeneralLedgerTranslatedData]
					(GeneralLedgerTranslatedDataKey, GeneralLedgerDataKey, Attribute_OCG, Attribute_LFE, Attribute_LFO, Attribute_SPR, Attribute_TIC, Attribute_ORG, PostPeriodForReportingBook, OriginalAttribute_OCG, OriginalAttribute_LFE, OriginalAttribute_LFO, OriginalAttribute_SPR, OriginalAttribute_TIC, OriginalAttribute_ORG, AlternateGLAccount, AlternateGLAccountDescription, AlternateGLAccountKey, AlternateChartKey, Units, OriginalAccount, ReportingBookKey, CompanyOfPeriodKey, LocalCurrency, TranslatedCurrency, AccountTypeCode, CurrencyTranslationLevel, TranslatedAmount, IsReciprocal, TranslatedRateType, SubUnitRatio)
				VALUES
				(17, 7, 'INT', 'WEU', 'LOC', 'SPS', 'ETI', 'OR1', 202303, 'INT', 'WEU', 'LOC', 'SPS', 'ETI', 'OR1', '1409', 'Account1', 4, 1, 'KG', '165', 1, 1, 'CNY', 'USD', 'P&L', 'BAL',    2, 1, 'SEL', 100),
				(18, 8, 'TPY', 'OEU', 'FOR', 'NAV', ''   , 'OR2', 202304, 'TPY', 'OEU', 'FOR', 'NAV', 'STI', 'OR2', '1409', 'Account2', 4, 1, 'KG', '265', 1, 1, 'CNY', 'USD', 'P&L', 'BAL',    2, 1, 'SEL', 100),
				(19, 9, 'INT', 'LOC', 'FOR', 'SPS', 'ETI', 'NAV', 202308, 'INT', 'LOC', 'FOR', 'SPS', 'ETI', 'NAV', '1409', 'Account3', 4, 1, 'KG', '365', 1, 1, 'CNY', 'USD', 'P&L', 'BAL', null, 1, 'SEL', 100)
				",
				ScriptDbName, CompanyPK
			);
			TestConnection.ExecuteNonQuery(sqlText);

			var result = Execute(ReportingBookPK, CompanyPK, 202303, 202305);
			AssertEquals("Should be 6 rows in report", 6, result.Rows.Count);
			AssertEquals(1, result.Select("AlternateGLAccount = '1439' and Amount = 0 and OpeningBalance = 10.5").Length);
			AssertEquals(1, result.Select("AlternateGLAccount = '1231' and Amount = -1 and OpeningBalance = 0.5 and LocalCredit = 2").Length);
			AssertEquals(1, result.Select("AlternateGLAccount = '1232' and Amount = -3 and OpeningBalance = 0 and LocalCredit = 3").Length);
			AssertEquals(1, result.Select("AlternateGLAccount = '1209' and Amount = 0 and OpeningBalance = 11").Length);
		}

		public void TestOpeningBalanceANDUnitsWithNTEGLAccount()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				UPDATE [{0}].[Finance].BAS__AccReportingBook SET RX_NKCurrency = 'USD'
				INSERT [{0}].[Finance].[GRP__ReportingBookGeneralLedgerAggregateData]
					(BranchKey, CompanyKey, DepartmentKey, GLAmountLocalBalance, PostPeriod, TransactionCategory, AlternateGLAccountKey, AccountTypeCode, ReportingBookKey, GLAccountKey, AlternateChartKey, IsReciprocal, SubUnitRatio, CurrencyTranslationLevel, LocalCurrency, TranslatedCurrency, TranslatedRateType, TranslatedCredit, TranslatedDebit, TranslatedBalance)
				VALUES
					(1, 1, 1, 1, 202205, 'UUU',  9, 'NTE', 1, 5, 1, 1, 100, 'BAL', 'CNY', 'CNY', 'SEL', 1, 5, -1),
					(1, 1, 1, 1, 100001, 'UUU',  9, 'NTE', 1, 5, 1, 1, 100, 'BAL', 'CNY', 'USD', 'SEL', 1, 5, -1),
					(1, 1, 1, 1, 202301,    '',  9, 'NTE', 1, 5, 1, 1, 100, 'BAL', 'CNY', 'USD', 'SEL', 1, 5, -1),
					(1, 1, 1, 1, 100001, 'UUU',  7, 'BSH', 1, 3, 1, 1, 100, 'BAL', 'CNY', 'USD', 'SEL', 1, 5, -1),
					(1, 1, 1, 1, 202301,    '',  7, 'BSH', 1, 2, 1, 1, 100, 'BAL', 'CNY', 'USD', 'SEL', 1, 5, -1),
					(1, 1, 1, 1, 202201,    '',  7, 'BSH', 1, 1, 1, 1, 100, 'BAL', 'CNY', 'USD', 'SEL', 1, 5, -1),
					(1, 1, 1, 1, 202301, 'UUU', 10, 'NTE', 1, 4, 1, 1, 100, 'BAL', 'CNY', 'USD', 'SEL', 1, 5, -1),
					(1, 1, 1, 1, 202205, 'UUU', 10, 'NTE', 1, 4, 1, 1, 100, 'BAL', 'CNY', 'USD', 'SEL', 1, 5, -1),
					(1, 1, 1, 1, 100001,    '', 10, 'NTE', 1, 4, 1, 1, 100, 'BAL', 'CNY', 'USD', 'SEL', 1, 5, 11),
					(1, 1, 1, 1, 202301,    '',  8, 'P&L', 1, 2, 1, 1, 100, 'BAL', 'CNY', 'USD', 'SEL', 1, 5, 11)
				INSERT [{0}].[Finance].[CUS__GeneralLedgerTransactionData]
					(CompanyKey, BranchKey, Currency, DepartmentKey, GLAccountKey, GLAccountName, GLAccountNum, GLAmountLocalCredit, GLAmountLocalDebit, GLAmountLocalBalance, GLAmountOSCredit, GLAmountOSDebit, GLAmountOSBalance, TransactionCategory, GeneralLedgerDataKey, PostDate, CompanyID,
					TransactionDate, TransactionNum, TransactionType, LineJobKey, LineDescription, HeaderDescription, LedgerCode, JobInvoiceNumber, ExchangeRate, ComplianceSubType, ComplianceNumber, ChequeOrReference, ChargeCodeKey, OrganizationKey, TransactionLineKey)
				VALUES
					(1, 1, 'CNY', 1, 5, 'GLAccount1', '10100009', 2, 0, -2, 3, 0, -3, '', 17, '2023-03-07', '{1}', '2023-01-08', '121', 'DPY', 1, 'lineDesc', 'HDesc', 'AP', 'Jiu01', 3.4, 'ert', '345', '456', 1, 1, 4),
					(1, 1, 'CNY', 1, 4, 'GLAccount2', '10200009', 2, 0, -2, 3, 0, -3, '', 18, '2023-02-07', '{1}', '2023-02-08', '121', 'DPY', 1, 'lineDesc', 'HDesc', 'AP', 'Jiu01', 3.4, 'ert', '345', '456', 1, 1, 4),
					(1, 1, 'CNY', 1, 4, 'GLAccount3', '10300009', 2, 0, -2, 3, 0, -3, '', 19, '2023-03-07', '{1}', '2023-02-08', '121', 'DPY', 1, 'lineDesc', 'HDesc', 'AP', 'Jiu01', 3.4, 'ert', '345', '456', 1, 1, 4)

				INSERT [{0}].[Finance].[CUS__GeneralLedgerTranslatedData]
					(GeneralLedgerTranslatedDataKey, GeneralLedgerDataKey, Attribute_OCG, Attribute_LFE, Attribute_LFO, Attribute_SPR, Attribute_TIC, Attribute_ORG, PostPeriodForReportingBook, OriginalAttribute_OCG, OriginalAttribute_LFE, OriginalAttribute_LFO, OriginalAttribute_SPR, OriginalAttribute_TIC, OriginalAttribute_ORG, AlternateGLAccount, AlternateGLAccountDescription, AlternateGLAccountKey, AlternateChartKey, Units, OriginalAccount, ReportingBookKey, CompanyOfPeriodKey, LocalCurrency, TranslatedCurrency, AccountTypeCode, CurrencyTranslationLevel, TranslatedAmount, IsReciprocal, TranslatedRateType, SubUnitRatio)
				VALUES
				(17, 17, 'INT', 'WEU', 'LOC', 'SPS', 'ETI', 'OR1', 202303, 'INT', 'WEU', 'LOC', 'SPS', 'ETI', 'OR1', '6439', 'Account1',  9, 1, 'KG', '165', 1, 1, 'CNY', 'USD', 'NTE', 'BAL',    2, 1, 'SEL', 100),
				(18, 18, 'TPY', 'OEU', 'FOR', 'NAV', ''   , 'OR2', 202304, 'TPY', 'OEU', 'FOR', 'NAV', 'STI', 'OR2', '1539', 'Account2', 10, 1, 'KG', '265', 1, 1, 'CNY', 'CNY', 'NTE', 'BAL',    2, 1, 'SEL', 100),
				(19, 19, 'INT', 'LOC', 'FOR', 'SPS', 'ETI', 'NAV', 202308, 'INT', 'LOC', 'FOR', 'SPS', 'ETI', 'NAV', '1539', 'Account3', 10, 1, 'KG', '365', 1, 1, 'CNY', 'CNY', 'NTE', 'BAL', null, 1, 'SEL', 100)
				",
				ScriptDbName, CompanyPK
			);
			TestConnection.ExecuteNonQuery(sqlText);

			var result = Execute(ReportingBookPK, CompanyPK, 202303, 202304);
			AssertEquals("Should be 6 rows in report", 6, result.Rows.Count);
			AssertEquals(1, result.Select("AlternateGLAccount = '1439' and Amount = 0 and OpeningBalance = 0.5").Length);
			AssertEquals(1, result.Select("AlternateGLAccount = '1209' and Amount = 0 and OpeningBalance = 0.5").Length);
			AssertEquals(1, result.Select("AlternateGLAccount = '1539' and Amount = -2 and OpeningBalance = 1").Length);
			AssertEquals(1, result.Select("AlternateGLAccount = '6439' and Amount = -2 and OpeningBalance = 3").Length);
			AssertEquals(1, result.Select("AlternateGLAccount = '1231' and Amount = -1 and OpeningBalance = 0").Length);
			AssertEquals(1, result.Select("AlternateGLAccount = '1232' and Amount = -3 and OpeningBalance = 0").Length);
		}

		#endregion

		protected override void SetUp()
		{
			base.SetUp();
			PrepareTestData();
		}

		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}

		void PrepareTestData()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				DELETE FROM [{0}].Organization.BAS__Company;
				DELETE FROM [{0}].Organization.BAS__Branch;
				DELETE FROM [{0}].Organization.BAS__Department;
				DELETE FROM [{0}].Finance.BAS__PeriodManagement;
				DELETE FROM [{0}].[Organization].[BAS__Organization];
				DELETE FROM [{0}].[Organization].[BAS__Organization];
				DELETE FROM [{0}].[Finance].[BAS__ChargeCode];
				DELETE FROM [{0}].[Finance].[BAS__JobHeader]
			", ScriptDbName);
			TestConnection.ExecuteNonQuery(sqlText);

			TestHelper.InsertCurrency("USD");
			CompanyPK = TestHelper.InsertCompany("CNY", "UUU");
			var companyPK2 = TestHelper.InsertCompany("USD", "DAU");

			sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].[Finance].[CUS__GeneralLedgerTransactionData]
					(CompanyKey, BranchKey, Currency, DepartmentKey, GLAccountKey, GLAccountName, GLAccountNum, GLAmountLocalCredit, GLAmountLocalDebit, GLAmountLocalBalance, GLAmountOSCredit, GLAmountOSDebit, GLAmountOSBalance, TransactionCategory, GeneralLedgerDataKey, PostDate, CompanyID,
					TransactionDate, TransactionNum, TransactionType, LineJobKey, LineDescription, HeaderDescription, LedgerCode, JobInvoiceNumber, ExchangeRate, ComplianceSubType, ComplianceNumber, ChequeOrReference, ChargeCodeKey, OrganizationKey, TransactionLineKey)
				VALUES
					(1, 1, 'CNY', 1, 1, 'GLAccount1', '10100009', 2, 0, -2, 3, 0, -3, '', 1, '2023-03-07', '{1}', '2023-09-08', '121', 'DPY', 1, 'lineDesc', 'HDesc', 'AP', 'Jiu01', 3.4, 'ert', '345', '456', 1, 1, 4),
					(1, 2, 'CNY', 1, 2, 'GLAccount2', '10100019', 3, 0, -3, 3, 0, -3, '', 2, '2023-04-07', '{1}', '2023-09-08', '122', 'WIP', 1, 'lineDesc', 'HDesc', 'AP', 'Jiu02', 3.4, 'ert', '345', '456', 1, 1, 4),
					(1, 1, 'CNY', 1, 1, 'GLAccount1', '10100029', 4, 0, -4, 3, 0, -3, '', 3, '2023-08-07', '{1}', '2023-09-08', '123', 'INV', 1, 'lineDesc', 'HDesc', 'AR', 'Jiu03', 3.4, 'ert', '345', '456', 1, 1, 4),
					(1, 1, 'CNY', 1, 1, 'GLAccount1', '10100029', 4, 0, -5, 3, 0, -3, '', 4, '2023-09-07', '{1}', '2023-09-08', '124', 'AJL', 1, 'lineDesc', 'HDesc', 'AP', 'Jiu04', 3.4, 'ert', '345', '456', 1, 1, 4),
					(1, 1, 'CNY', 1, 1, 'GLAccount1', '10100029', 4, 0, -6, 3, 0, -3, '', 5, '2023-10-07', '{1}', '2023-09-08', '126', 'ADJ', 1, 'lineDesc', 'HDesc', 'AP', 'Jiu09', 3.4, 'ert', '345', '456', 1, 1, 4),
					(1, 1, 'CNY', 1, 1, 'GLAccount1', '10100029', 5, 0, -7, 3, 0, -3, '', 6, '2023-11-07', '{2}', '2023-09-08', '125', 'PAY', 1, 'lineDesc', 'HDesc', 'AP', 'Jiu10', 3.4, 'ert', '345', '456', 1, 1, 4)

				INSERT [{0}].[Finance].[CUS__GeneralLedgerTranslatedData]
					(GeneralLedgerTranslatedDataKey, GeneralLedgerDataKey, Attribute_OCG, Attribute_LFE, Attribute_LFO, Attribute_SPR, Attribute_TIC, Attribute_ORG, PostPeriodForReportingBook, OriginalAttribute_OCG, OriginalAttribute_LFE, OriginalAttribute_LFO, OriginalAttribute_SPR, OriginalAttribute_TIC, OriginalAttribute_ORG, AlternateGLAccount, AlternateGLAccountDescription, AlternateGLAccountKey, AlternateChartKey, Units, OriginalAccount, ReportingBookKey, CompanyOfPeriodKey, LocalCurrency, TranslatedCurrency, AccountTypeCode, CurrencyTranslationLevel, TranslatedAmount, IsReciprocal, TranslatedRateType, SubUnitRatio, TranslatedExchangeRate)
				VALUES
				(1, 1, 'INT', 'WEU', 'LOC', 'SPS', 'ETI', 'OR1', 202303, 'INT', 'WEU', 'LOC', 'SPS', 'ETI', 'OR1', '1231', 'Account1', 1, 1, 'KG', '165', 1, 1, 'CNY', 'USD', 'BSH', 'BAL',    2, 1, 'SEL', 100, 1),
				(2, 2, 'TPY', 'OEU', 'FOR', 'NAV', ''   , 'OR2', 202304, 'TPY', 'OEU', 'FOR', 'NAV', 'STI', 'OR2', '1232', 'Account2', 2, 1, 'KG', '265', 1, 1, 'CNY', 'CNY', 'NTE', 'BAL',    2, 1, 'SEL', 100, 1),
				(3, 3, 'INT', 'LOC', 'FOR', 'SPS', 'ETI', 'NAV', 202308, 'INT', 'LOC', 'FOR', 'SPS', 'ETI', 'NAV', '1233', 'Account3', 3, 1, 'KG', '365', 1, 1, 'CNY', 'CNY', 'P&L', 'BAL', null, 1, 'SEL', 100, 1),
				(4, 4, 'INT', 'LOC', 'FOR', ''   , 'ETI', 'NAV', 100001, 'INT', 'LOC', 'FOR', 'SPS', 'ETI', 'NAV', '1234', 'Account4', 4, 1, 'KG', '465', 1, 1, 'CNY', 'USD', 'BSH', 'BAL',    2, 0, 'SEL', 100, 1),
				(5, 5, 'INT', 'LOC', ''   , 'SPS', 'ETI', 'NAV', 202310, 'INT', 'LOC', 'FOR', 'SPS', 'ETI', 'NAV', '1235', 'Account5', 5, 1, 'KG', '565', 1, 1, 'CNY', 'USD', 'BSH', 'JNL',    2, 1, 'SEL', 100, 1),
				(6, 6, 'NAV', 'NAV', 'NAV', 'SPR', 'NAV', 'OR1', 202311, 'NAV', 'NAV', 'NAV', 'SPR', 'NAV', 'OR1', '1236', 'Account6', 6, 1, 'KG', '665', 1, 1, 'CNY', 'CNY', 'BSH', 'JNL', null, 1, 'SEL', 100, 1),
				(7, 7, 'NAV', 'NAV', 'NAV', 'SPR', 'NAV', 'OR1', 202309, 'NAV', 'NAV', 'NAV', 'SPR', 'NAV', 'OR1', '1237', 'Account7', 7, 1, 'KG', '265', 2, 1, 'CNY', 'USD', 'NTE', 'JNL',    2, 1, 'SEL', 100, 1),
				(8, 8, 'NAV', 'NAV', 'NAV', 'SPR', 'NAV', 'OR1', 202309, 'NAV', 'NAV', 'NAV', 'SPR', 'NAV', 'OR1', '1238', 'Account8', 8, 1, 'KG', '265', 2, 1, 'CNY', 'USD', 'BSH', 'BAL',    2, 1, 'SEL', 100, 1),
				(9, 9, 'NAV', 'NAV', 'NAV', 'SPR', 'NAV', 'OR1', 202309, 'NAV', 'NAV', 'NAV', 'SPR', 'NAV', 'OR1', '1239', 'Account9', 8, 1, 'KG', '265', 2, 1, 'CNY', 'USD', 'BSH', 'BAL',    2, 1, 'SEL', 100, 1)

				INSERT [{0}].[Finance].[CUS__AlternateGLAccountData]
					(AlternateGLAccountDataKey, AlternateGLAccountKey, DestinationAccountNo, AccountTypeCode, CompanyKey, AlternateChartKey, GLAccountKey, Attribute_LFE, Attribute_LFO, Attribute_TIC, Attribute_SPR, Attribute_OCG, Attribute_ORG, OriginalAccount, Description, StatisticalUnits, ParentGLAccountNo)
				VALUES
					(1, 1, '1109', 'P&L', 1, 1, 1, 'WEU', 'LOC', 'ETI', 'SPS', 'INT', 'OR1', '123451','description', 'KB', '1231'),
					(2, 2, '1209', 'P&L', 1, 1, 2, 'OEU', 'FOR', 'STI', 'NAV', 'TPY', 'OR2', '223452','description', 'KB', '1232'),
					(3, 3, '1309', 'BSH', 1, 1, 1, 'LOC', 'FOR', 'ETI', 'SPS', 'INT', 'NAV', '323453','description', 'KB', '1233'),
					(4, 4, '1409', 'P&L', 1, 1, 1, 'WEU', 'LOC', 'ETI', 'SPS', 'INT', 'OR2', '423454','description', 'KB', '1234'),
					(5, 5, '1419', 'P&L', 1, 1, 1, 'NAV', 'NAV', 'ETI', 'SPS', 'INT', 'NAV', '523455','description', 'KB', '1235'),
					(6, 6, '1429', 'P&L', 1, 1, 1, 'NAV', 'NAV', 'NAV', 'SPR', 'NAV', 'OR1', '623456','description', 'KB', '1236'),
					(7, 7, '1439', 'BSH', 1, 1, 3, '', '', '', '', '', '', '423456','description', 'KY', '1238'),
					(8, 8, '1209', 'P&L', 1, 1, 2, '', '', 'STI', 'NAV', 'TPY', '', '223452','description', 'KB', '1239'),
					(9, 9, '6439', 'NTE', 1, 1, 5, '', '', '', '', '', '', '623456','NTE', 'UU', '6236'),
					(10, 10, '1539', 'NTE', 1, 1, 4, '', '', '', '', '', '', '123456','NTE', 'HH', '2236')

				INSERT [{0}].[Finance].[GRP__ReportingBookGeneralLedgerAggregateData]
					(BranchKey, CompanyKey, DepartmentKey, GLAccountKey, GLAmountLocalCredit, GLAmountLocalDebit, GLAmountLocalBalance, PostPeriod,
TransactionCategory, ReportingBookCode, ReportingBookKey, CompanyOfPeriodKey, Attribute_TIC, Attribute_SPR, Attribute_ORG, Attribute_OCG, Attribute_LFE, Attribute_LFO, OriginalAttribute_TIC, OriginalAttribute_SPR, OriginalAttribute_ORG,
OriginalAttribute_OCG, OriginalAttribute_LFE, OriginalAttribute_LFO, AlternateGLAccount, AlternateGLAccountKey, AccountTypeCode, AlternateChartKey, PeriodManagementKey, CurrencyTranslationLevel, TranslatedBalance)
				VALUES
					(1, 1, 1, 1, 3, 4, 1, 202305, 'EET', 'RRB', 1, 1, 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', '899989', 1, 'BSH', 1, 1, 'JNL', 1),
					(2, 2, 1, 2, 2, 3, 1, 202405, 'EEY', 'RRW', 2, 1, 'STI', 'SPR', 'INTT2', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT3', 'TPY', 'WEU', 'LOC', '899988', 1, 'BSH', 1, 1, 'JNL', 1),
					(1, 3, 1, 3, 3, 4, 1, 202405, 'EET', 'RRR', 2, 1, 'STI', 'SPR', 'INTT3', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT4', 'TPY', 'WEU', 'LOC', '899987', 1, 'BSH', 1, 1, 'JNL', 1),
					(1, 1, 1, 4, 3, 4, 1, 202605, 'EET', 'RRR', 1, 1, 'STI', 'SPR', 'INTT4', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT4', 'TPY', 'WEU', 'LOC', '899986', 1, 'BSH', 1, 1, 'JNL', 1),
					(2, 1, 2, 5, 3, 4, 1, 202306, 'CAT', 'RRR', 1, 1, 'STI', 'SPR', 'INTT4', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT4', 'TPY', 'WEU', 'LOC', '899985', 1, 'BSH', 2, 1, 'JNL', 1)

				INSERT [{0}].Finance.BAS__JobHeader
					(JobHeaderKey, JobHeaderID, JobNo)
				VALUES
					(1, newID(), 'JJJoiu')

				INSERT [{0}].Finance.BAS__ChargeCode
					(ChargeCodeKey, ChargeCodeID, Code, [Desc], LocalLanguageDescription)
				VALUES
					(1, newID(), 'cuu','cdesc','clocaldesc')

				INSERT [{0}].Finance.CUS__ExchangeRatesforReportingbook
				(ExchangeRatesforReportingbookKey, CompanyKey, StartDate, EndDate, ExRateType, Currency, SellRate)
				VALUES
				(1, 1, '2022-01-01', '2025-12-20', 'SEL', 'USD', 2),
				(2, 2, '2022-01-01', '2024-12-20', 'BUY', 'USD', 5)
				",
				ScriptDbName, CompanyPK, companyPK2);
			TestConnection.ExecuteNonQuery(sqlText);
			TestHelper.CreateBASBranch(1, Guid.NewGuid(), 1, "UUU");
			TestHelper.CreateBASBranch(2, Guid.NewGuid(), 1, "UUY");
			TestHelper.CreateBASBranch(3, Guid.NewGuid(), 2, "UUO");
			TestHelper.CreateBASDepartment(1, Guid.NewGuid(), "YYY");
			TestHelper.CreateBASDepartment(2, Guid.NewGuid(), "YYU");

			TestHelper.InsertPeriodForInputYear(2023, 1);
			TestHelper.InsertPeriodForInputYear(2023, 2);

			TestHelper.InsertALternateChart("uuu", isFixedLength: 0, isGlobal: 1);
			ReportingBookPK = TestHelper.InsertReportingBook(1, 1, "RUR", presentationJournals: "UUU");
			TestHelper.InsertReportingBook(2, 2, "RUY");
			TestHelper.InsertStmDataDate("JournalEntriesLastProcessedDate", new DateTime(2022, 1, 1), 1);
			StartGLAccountPK = TestHelper.InsertAlternateGLAccount("123451", "P&L", 1);
			EndGLAccountPK = TestHelper.InsertAlternateGLAccount("323251", "P&L", 1);

			TestHelper.InsertGLAccount("1234", "P&L");
			TestHelper.InsertGLAccount("41234", "BSH");
			TestHelper.InsertGLAccount("41232", "BSH");
			TestHelper.InsertGLAccount("21232", "NTE");
			TestHelper.InsertGLAccount("61232", "NTE");
			TestHelper.InsertBASAccount(3);
			TestHelper.InsertBASAccount(2, "GL_BS_ACCOUNT_START");
			TestHelper.InsertOrganization("UUU", Guid.NewGuid());
		}

		DataTable Execute(Guid reportingBookPK, Guid companyPK, int startPeriod = 202203, int endPeriod = 202204, string startDate = "", string endDate = "", Guid? startGLAccountPK = null, Guid? endGLAccountPK = null, Guid? branchPK = null, Guid? departmentPK = null, string desc = "L", string includeZeroBalance = "")
		{
			var sqlBuilder = new StringBuilder();
			sqlBuilder.AppendLine($"EXEC [{ScriptDbName}].[{ScriptToTest.SchemaName}].[{ScriptToTest.Name}]");
			sqlBuilder.Append($"@ReportingBookPK = '{reportingBookPK}'");
			sqlBuilder.Append($",@CompanyPK = '{companyPK}'");
			sqlBuilder.Append(
				$@",@StartPeriod = ").Append(startPeriod == 0 ? "NULL" : startPeriod.ToString());
			sqlBuilder.Append(
				$@",@EndPeriod = ").Append(endPeriod == 0 ? "NULL" : endPeriod.ToString());
			sqlBuilder.Append(
				$@",@StartDate = ").Append(string.IsNullOrEmpty(startDate) ? "NULL" : $"'{startDate}'");
			sqlBuilder.Append(
				$@",@EndDate = ").Append(string.IsNullOrEmpty(endDate) ? "NULL" : $"'{endDate}'");
			sqlBuilder.Append(
				$@",@StartGLAccountPK = ").Append(startGLAccountPK == null ? "NULL" : $"'{startGLAccountPK}'");
			sqlBuilder.Append(
				$@",@EndGLAccountPK = ").Append(endGLAccountPK == null ? "NULL" : $"'{endGLAccountPK}'");
			sqlBuilder.Append(
				$@",@BranchPK = ").Append(branchPK == null ? "NULL" : $"'{branchPK}'");
			sqlBuilder.Append(
				$@",@DepartmentPK = ").Append(departmentPK == null ? "NULL" : $"'{departmentPK}'");
			sqlBuilder.Append(
				$@",@DisplayDescription = ").Append(string.IsNullOrEmpty(desc) ? "'L'" : $"'{desc}'");
			sqlBuilder.Append(
				$@",@IncludeZeroBalance = ").Append(string.IsNullOrEmpty(includeZeroBalance) ? "'N'" : includeZeroBalance);

			var sqlText = string.Format(CultureInfo.InvariantCulture,
				sqlBuilder.ToString()
			);

			return DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
		}

		Guid CompanyPK, StartGLAccountPK, EndGLAccountPK, ReportingBookPK;
		AccountingFunctionTestingHelper TestHelper => fTestHelper ?? (fTestHelper = new AccountingFunctionTestingHelper(TestConnection, ScriptDbName));
		AccountingFunctionTestingHelper fTestHelper;
	}
}
