using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting.ReportingBooks;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Functions.Accounting.Testing
{
	[TestedType(typeof(GLJournalListingForReportingBook))]
	class GLJournalListingForReportingBookTest : BiCreateScriptTest
	{
		public void TestColumns()
		{
			var whiteList = new List<string>
			{
				"TransactionType",
				"InvoiceDate",
				"PostDate",
				"DueDate",
				"Branch",
				"TaxBranch",
				"Department",
				"Ledger",
				"TransactionNum",
				"JournalEntriesNumber",
				"JournalEntriesType",
				"TransactionDesc",
				"AlternateGLAccount",
				"AlternateGLAccountDesc",
				"Units",
				"Job",
				"Account",
				"ChargeCode",
				"ChargeCodeDescription",
				"LocalLanguageChargeCodeDescription",
				"ReportingPeriod",
				"TranslatedCurrency",
				"TranslatedAmount",
				"TranslatedDebit",
				"TranslatedCredit",
				"OsAmount",
				"LocalAmount",
				"LocalDebit",
				"LocalCredit",
				"GLAmountOSDebit",
				"GLAmountOSCredit",
				"ExRate",
				"TranslatedExRate",
				"Currency",
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
				"TransactionCategory",
				"ReportingBookCode",
				"Period",
				"CompanyCode",
				"CurrencyTranslationLevel",
				"MultiSubAccountTypeCode",
				"OrganisationSubAccount",
				"SalesExpenseGroupsSubAccount",
				"StaffAndResourcesSubAccount",
				"StaffGroupSubAccount"
			};

			var result = Execute(ReportingBookPK, CompanyPK);
			AssertEquals(whiteList.Count, result.Columns.Count);
			foreach (DataColumn column in result.Columns)
			{
				Assert($@"Should contain column '{column.ColumnName}'", whiteList.Contains(column.ColumnName));
			}
		}

		public void TestRun()
		{
			var result = Execute(ReportingBookPK, CompanyPK, 202303, 202306);
			AssertEquals("Should be 2 rows in report", 2, result.Rows.Count);

			AssertEquals(1, result.Select("AlternateGLAccount = '1231' and Period = 202303 and TransactionType = 'DPY' and InvoiceDate = '2023-09-08 00:00:00' and PostDate = '2023-03-07 00:00:00' and Branch = 'UUU' and Department = 'YYY' and Ledger ='AP' and TransactionNum = '121' and TransactionDesc = 'lineDesc' and Units = 'KG' and Job = 'JJJoiu' and TranslatedAmount is null and TranslatedDebit is null and TranslatedCredit is null").Length);
			AssertEquals(1, result.Select("AlternateGLAccount = '1232' and Period = 202304 and Account = 'UUU' and ChargeCode = 'cuu' and ChargeCodeDescription ='cdesc' and LocalLanguageChargeCodeDescription ='clocaldesc' and ExRate = 3.4 and GLAccountNo = '10100019' and TranslatedAmount = -3 and TranslatedDebit is null and TranslatedCredit = 3").Length);

			result = Execute(ReportingBookPK, CompanyPK, 202303, 202310);
			AssertEquals("Should be 5 rows in report", 5, result.Rows.Count);
			AssertEquals(1, result.Select("AlternateGLAccount = '1233' and ReportingPeriod = 202308 and Period = 202308 and TransactionType ='INV' and Ledger ='AR' and MultiSubAccountTypeCode = 'ORG: OR1, SEG: SaExGroup1, STR: SRE, SGP: StaGro1' and OrganisationSubAccount = 'OR1' and SalesExpenseGroupsSubAccount = 'SaExGroup1' and StaffAndResourcesSubAccount = 'SRE' and StaffGroupSubAccount = 'StaGro1'and TranslatedAmount = -4 and TranslatedDebit is null and TranslatedCredit = 4").Length);
			AssertEquals(1, result.Select("AlternateGLAccount = '1234' and ReportingPeriod = 100001 and Period = 202309 and TransactionType = 'AJL' and TranslatedAmount is null and TranslatedDebit is null and TranslatedCredit is null").Length);
			AssertEquals(1, result.Select("AlternateGLAccount = '1235' and Period = 202310 and Attribute_ORG = 'NAV-No Attribute Value' and Attribute_OCG = 'INT' and Attribute_LFE = 'LOC-Local' and Attribute_LFO = 'FOR-Foreign' and Attribute_TIC = 'ETI-Tax ID with Extra Tax' and Attribute_SPR = 'SPS-Sales/Purchases'").Length);

			result = Execute(ReportingBookPK2, CompanyPK2, 202310, 202311);
			AssertEquals("Should be 2 rows in report", 2, result.Rows.Count);
			AssertEquals(1, result.Select("AlternateGLAccount = '1237' and ReportingPeriod = 202311 and Period = 202311 and TransactionType ='REC' and Ledger ='AR' and TranslatedAmount = -7 and TranslatedDebit is null and TranslatedCredit = 5").Length);
			AssertEquals(1, result.Select("AlternateGLAccount = '1238' and ReportingPeriod = 202311 and Period = 202311 and TransactionType = 'PAY' and Ledger ='AP'").Length);

			result = Execute(ReportingBookPK2, CompanyPK2, 202312, 202312);
			AssertEquals("Should be 1 rows in report", 1, result.Rows.Count);
			AssertEquals(1, result.Select("TranslatedAmount IS NULL AND TranslatedCredit IS NULL AND TranslatedDebit IS NULL").Length);
		}

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
				DELETE FROM [{0}].[Finance].[BAS__ChargeCode];
				DELETE FROM [{0}].[Finance].[BAS__JobHeader]
			", ScriptDbName);
			TestConnection.ExecuteNonQuery(sqlText);

			TestHelper.InsertCurrency("USD");
			CompanyPK = TestHelper.InsertCompany("CNY", "UUU");
			CompanyPK2 = TestHelper.InsertCompany("USD", "DAU");
			TestHelper.CreateBASBranch(1, Guid.NewGuid(), 1, "UUU");
			TestHelper.CreateBASBranch(2, Guid.NewGuid(), 1, "UUY");
			TestHelper.CreateBASBranch(3, Guid.NewGuid(), 2, "UUO");
			TestHelper.CreateBASDepartment(1, Guid.NewGuid(), "YYY");
			TestHelper.CreateBASDepartment(2, Guid.NewGuid(), "YYU");

			TestHelper.InsertALternateChart("uuu", isFixedLength: 0, isGlobal: 1);
			TestHelper.InsertALternateChart("uu2", isFixedLength: 0, isGlobal: 1, companyID: CompanyPK2);
			ReportingBookPK = TestHelper.InsertReportingBook(1, 1, "RUR", presentationJournals: "UUU");
			ReportingBookPK2 = TestHelper.InsertReportingBook(2, 2, "RUY");

			TestHelper.InsertPeriodForInputYear(2023, 1);
			TestHelper.InsertPeriodForInputYear(2023, 2);

			sqlText = string.Format(CultureInfo.InvariantCulture, @"

				INSERT [{0}].[Finance].[CUS__HeaderLineSubAccounts]
					([ParentKey], [ParentTableCode], [OrganisationSubAccount], [SalesExpenseGroupsSubAccount], [StaffAndResourcesSubAccount], [StaffGroupSubAccount])
					VALUES
						(4, 'L', 'OR1', 'SaExGroup1', 'SRE', 'StaGro1')

				INSERT [{0}].[Finance].[CUS__GLHeaderSubAccounts]
					([GLHeaderSubAccountsKey], [GLAccountKey], [HasOrganisationSubAccount], [HasSalesExpenseGroupsSubAccount], [HasStaffAndResourcesSubAccount], [HasStaffGroupSubAccount])
					VALUES
						(1, 1, 1, 1, 1, 1)

				INSERT [{0}].[Finance].[CUS__GeneralLedgerTransactionData]
					(CompanyKey, BranchKey, Currency, DepartmentKey, GLAccountKey, GLAccountName, GLAccountNum, GLAmountLocalCredit, GLAmountLocalDebit, GLAmountLocalBalance, GLAmountOSCredit, GLAmountOSDebit, GLAmountOSBalance, TransactionCategory, GeneralLedgerDataKey, PostDate, PostPeriod, CompanyID,
					TransactionDate, TransactionNum, TransactionType, LineJobKey, LineDescription, HeaderDescription, LedgerCode, JobInvoiceNumber, ExchangeRate, ComplianceSubType, ComplianceNumber, ChequeOrReference, ChargeCodeKey, OrganizationKey, TransactionLineKey)
				VALUES
					(1, 1, 'CNY', 1, 1, 'GLAccount1', '10100009', 2, 0, -2, 3, 0, -3, '', 1, '2023-03-07', 202303, '{1}', '2023-09-08', '121', 'DPY', 1, 'lineDesc', 'HDesc', 'AP', 'Jiu01', 3.4, 'ert', '345', '456', 1, 1, 4),
					(1, 2, 'CNY', 1, 2, 'GLAccount2', '10100019', 3, 0, -3, 3, 0, -3, '', 2, '2023-04-07', 202304, '{1}', '2023-09-08', '122', 'WIP', 1, 'lineDesc', 'HDesc', 'AP', 'Jiu02', 3.4, 'ert', '345', '456', 1, 1, 4),
					(1, 1, 'CNY', 1, 1, 'GLAccount1', '10100029', 4, 0, -4, 3, 0, -3, '', 3, '2023-08-07', 202308, '{1}', '2023-09-08', '123', 'INV', 1, 'lineDesc', 'HDesc', 'AR', 'Jiu03', 3.4, 'ert', '345', '456', 1, 1, 4),
					(1, 1, 'CNY', 1, 1, 'GLAccount1', '10100029', 4, 0, -5, 3, 0, -3, '', 4, '2023-09-07', 202309, '{1}', '2023-09-08', '124', 'AJL', 1, 'lineDesc', 'HDesc', 'AP', 'Jiu04', 3.4, 'ert', '345', '456', 1, 1, 4),
					(1, 1, 'CNY', 1, 1, 'GLAccount1', '10100029', 4, 0, -6, 3, 0, -3, '', 5, '2023-10-07', 202310, '{1}', '2023-09-08', '126', 'ADJ', 1, 'lineDesc', 'HDesc', 'AP', 'Jiu09', 3.4, 'ert', '345', '456', 1, 1, 4),
					(1, 1, 'CNY', 1, 1, 'GLAccount1', '10100029', 5, 0, -7, 3, 0, -3, '', 6, '2023-11-07', 202311, '{1}', '2023-09-08', '125', 'PAY', 1, 'lineDesc', 'HDesc', 'AP', 'Jiu10', 3.4, 'ert', '345', '456', 1, 1, 4),
					(1, 3, 'CNY', 1, 1, 'GLAccount1', '10100029', 5, 0, -7, 3, 0, -3, '', 7, '2023-11-07', 202311, '{2}', '2023-09-08', '125', 'REC', 1, 'lineDesc', 'HDesc', 'AR', 'Jiu11', 3.4, 'ert', '345', '456', 1, 1, 4),
					(1, 3, 'CNY', 1, 1, 'GLAccount1', '10100029', 5, 0, -7, 3, 0, -3, '', 8, '2023-11-08', 202311, '{2}', '2023-09-08', '125', 'PAY', 1, 'lineDesc', 'HDesc', 'AP', 'Jiu12', 3.4, 'ert', '345', '456', 1, 1, 4),
					(1, 3, 'CNY', 1, 1, 'GLAccount1', '10100029', 5, 0, -7, 3, 0, -3, '', 9, '2023-12-09', 202312, '{2}', '2023-09-08', '125', 'PAY', 1, 'lineDesc', 'HDesc', 'AP', 'Jiu13', 3.4, 'ert', '345', '456', 1, 1, 4)

				INSERT [{0}].[Finance].[CUS__GeneralLedgerTranslatedData]
					(GeneralLedgerTranslatedDataKey, GeneralLedgerDataKey, Attribute_OCG, Attribute_LFE, Attribute_LFO, Attribute_SPR, Attribute_TIC, Attribute_ORG, PostPeriodForReportingBook, OriginalAttribute_OCG, OriginalAttribute_LFE, OriginalAttribute_LFO, OriginalAttribute_SPR, OriginalAttribute_TIC, OriginalAttribute_ORG, AlternateGLAccount, AlternateGLAccountDescription, AlternateGLAccountKey, AlternateChartKey, Units, OriginalAccount, ReportingBookKey, CompanyOfPeriodKey, LocalCurrency, TranslatedCurrency, AccountTypeCode, CurrencyTranslationLevel, TranslatedAmount, IsReciprocal, TranslatedRateType, SubUnitRatio, TranslatedExchangeRate)
				VALUES
				(1, 1, 'INT', 'WEU', 'LOC', 'SPS', 'ETI', 'OR1', 202303, 'INT', 'WEU', 'LOC', 'SPS', 'ETI', 'OR1', '1231', 'Account1', 1, 1, 'KG', '165', 1, 1, 'CNY', 'USD', 'BSH', 'BAL',    2, 1, 'SEL', 100, 1),
				(2, 2, 'TPY', 'OEU', 'FOR', 'NAV', ''   , 'OR2', 202304, 'TPY', 'OEU', 'FOR', 'NAV', 'STI', 'OR2', '1232', 'Account2', 2, 1, 'KG', '265', 1, 1, 'CNY', 'CNY', 'NTE', 'BAL',    2, 1, 'SEL', 100, 1),
				(3, 3, 'INT', 'LOC', 'FOR', 'SPS', 'ETI', 'NAV', 202308, 'INT', 'LOC', 'FOR', 'SPS', 'ETI', 'NAV', '1233', 'Account3', 3, 1, 'KG', '365', 1, 1, 'CNY', 'CNY', 'P&L', 'BAL', null, 1, 'SEL', 100, 1),
				(4, 4, 'INT', 'LOC', 'FOR', ''   , 'ETI', 'NAV', 100001, 'INT', 'LOC', 'FOR', 'SPS', 'ETI', 'NAV', '1234', 'Account4', 4, 1, 'KG', '465', 1, 1, 'CNY', 'USD', 'BSH', 'BAL',    2, 0, 'SEL', 100, 1),
				(5, 5, 'INT', 'LOC', ''   , 'SPS', 'ETI', 'NAV', 202310, 'INT', 'LOC', 'FOR', 'SPS', 'ETI', 'NAV', '1235', 'Account5', 5, 1, 'KG', '565', 1, 1, 'CNY', 'USD', 'BSH', 'JNL',    2, 1, 'SEL', 100, 1),
				(6, 6, 'NAV', 'NAV', 'NAV', 'SPR', 'NAV', 'OR1', 202311, 'NAV', 'NAV', 'NAV', 'SPR', 'NAV', 'OR1', '1236', 'Account6', 6, 1, 'KG', '665', 1, 1, 'CNY', 'CNY', 'BSH', 'JNL', null, 1, 'SEL', 100, 1),
				(7, 7, 'NAV', 'NAV', 'NAV', 'SPR', 'NAV', 'OR1', 202311, 'NAV', 'NAV', 'NAV', 'SPR', 'NAV', 'OR1', '1237', 'Account7', 7, 1, 'KG', '765', 2, 2, 'CNY', 'CNY', 'BSH', 'JNL', null, 1, 'SEL', 100, 1),
				(8, 8, 'NAV', 'NAV', 'NAV', 'SPR', 'NAV', 'OR1', 202311, 'NAV', 'NAV', 'NAV', 'SPR', 'NAV', 'OR1', '1238', 'Account8', 8, 1, 'KG', '865', 2, 2, 'CNY', 'CNY', 'BSH', 'JNL', null, 1, 'SEL', 100, 1),
				(9, 9, 'NAV', 'NAV', 'NAV', 'SPR', 'NAV', 'OR1', 202312, 'NAV', 'NAV', 'NAV', 'SPR', 'NAV', 'OR1', '1239', 'Account9', 9, 1, 'KG', '965', 2, 2, 'CNY', 'USD', 'BSH', 'BAL', null, 1, 'SEL', 100, 1)

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
					(10, 10, '1539', 'NTE', 1, 1, 4, '', '', '', '', '', '', '123456','NTE', 'HH', '2236'),
					(11, 11, '1540', 'BSH', 2, 2, 3, '', '', '', '', '', '', '123456','NTE', 'HH', '2236')

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

				INSERT [{0}].[Finance].[BAS__ComplianceDocumentHeader]
					(ComplianceDocumentHeaderKey, ComplianceDocumentHeaderID, ComplianceSubType, DocumentNumber, DocumentStatus)
				VALUES
					(1, newid(), 'TXC', '', 'ADD')

				INSERT [{0}].[Finance].[BAS__ComplianceDocumentLine]
					(ComplianceDocumentHeaderKey, ComplianceDocumentLineID, ComplianceDocumentLineKey)
				VALUES
					(1, newid(), 1)

				INSERT [{0}].[Finance].[BAS__ComplianceDocumentPivot]
					(ComplianceDocumentPivotKey, ComplianceDocumentPivotID, ComplianceDocumentLineKey)
				VALUES
					(1, newid(), 1)

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
				ScriptDbName, CompanyPK, CompanyPK2);
			TestConnection.ExecuteNonQuery(sqlText);
			
			TestHelper.InsertGLAccount("1234", "P&L");
			TestHelper.InsertGLAccount("41234", "BSH");
			TestHelper.InsertGLAccount("41232", "BSH");
			TestHelper.InsertGLAccount("21232", "NTE");
			TestHelper.InsertGLAccount("61232", "NTE");
			TestHelper.InsertBASAccount(3);
			TestHelper.InsertBASAccount(2, "GL_BS_ACCOUNT_START");
			TestHelper.InsertOrganization("UUU", Guid.NewGuid());
		}

		DataTable Execute(Guid reportingBookPK, Guid companyPK, int startPeriod = 202203, int endPeriod = 202204)
		{
			var sqlText = $"SELECT * FROM [{ScriptDbName}].[{ScriptToTest.SchemaName}].[{ScriptToTest.Name}]('{reportingBookPK}', '{companyPK}', {startPeriod}, {endPeriod})";
			return DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
		}

		Guid CompanyPK, CompanyPK2, ReportingBookPK, ReportingBookPK2;
		AccountingFunctionTestingHelper TestHelper => fTestHelper ?? (fTestHelper = new AccountingFunctionTestingHelper(TestConnection, ScriptDbName));
		AccountingFunctionTestingHelper fTestHelper;
	}
}
