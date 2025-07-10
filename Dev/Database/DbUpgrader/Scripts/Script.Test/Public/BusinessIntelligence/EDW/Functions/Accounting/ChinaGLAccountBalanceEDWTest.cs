using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting;
using Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDW.Functions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Functions.Accounting.Testing
{
	[TestedType(typeof(ChinaGLAccountBalance))]
	class ChinaGLAccountBalanceEDWTest : BiCreateScriptTest
	{
		public void TestLastProcessedDate()
		{
			var helper = new PrepareDataHelper(TestConnection, ScriptDbName);
			helper.InsertCompanyBranchAndDepartment();
			helper.InsertGLAccount(1, "3987");
			helper.InsertBASAccount(2);
			helper.InsertPeriodForInputYear(2023);
			helper.InsertGLAggregate(2, 202304, 1, "", 2);
			var result = Excute("878D7ACA-FFC3-49FC-9710-969CA0C0F2AC", 202304, "");
			AssertEquals(0, result.Rows.Count);

			helper.InsertStmData(new DateTime(2023, 02, 06));
			result = Excute("878D7ACA-FFC3-49FC-9710-969CA0C0F2AC", 202304, "");
			AssertEquals(0, result.Rows.Count);

			helper.InsertStmData(new DateTime(2022, 02, 06));
			result = Excute("878D7ACA-FFC3-49FC-9710-969CA0C0F2AC", 202304, "");
			AssertEquals(1, result.Rows.Count);
		}

		public void TestOpenBalanceForPNLAccountWithDifferentCompanies()
		{
			PrepareData();
			var result = Excute(CompanyPK.ToString(), 201909, "");
			AssertEquals("Should be 2 rows in report", 2, result.Rows.Count);
			var row = result.Select("AccountNum ='4900.00.00'").FirstOrDefault();
			AssertEquals("The opening balance of Account '4900.00.00' should be 0m", 0m, row["Amount"]);
			AssertEquals("The opening balance of Account '4900.00.00' should be 366m", 366m, row["OpeningBalance"]);

			row = result.Select("AccountNum ='3333.22.22'").FirstOrDefault();
			AssertEquals("The opening balance of Account '3333.22.22' should be 500m", 500m, row["Amount"]);
			AssertEquals("The opening balance of Account '3333.22.22' should be 700m", 700m, row["OpeningBalance"]);

			void PrepareData()
			{
				var sqlText = string.Format(CultureInfo.InvariantCulture, @"
					INSERT INTO [{0}].Finance.BAS__PeriodManagement
						(CompanyKey, EndDate, EndDateFormat, Period, PeriodManagementID, PeriodManagementKey, StartDate, Year, CompanyID)
					VALUES
						(1, '2020-03-31 23:59:00', '2020-03-31', '201909', '22FFD1BD-2703-4EDD-B6BC-189E49EB9F5C', 1, '2020-03-01 00:00:00', 2019, '8495ACCB-9829-44B1-B8BA-1B9801A33233'),
						(1, '2020-01-31 23:59:00', '2020-01-31', '201907', '59239AF1-EF4F-4E25-855C-611AEB88D6C5', 2, '2020-01-01 00:00:00', 2019, '8495ACCB-9829-44B1-B8BA-1B9801A33233'),
						(1, '2020-02-29 23:59:00', '2020-02-29', '201908', '2F4A5501-81DF-4D65-9F37-84ED6CF9A995', 3, '2020-02-01 00:00:00', 2019, '8495ACCB-9829-44B1-B8BA-1B9801A33233'),
						(1, '2020-04-30 23:59:00', '2020-04-30', '201910', 'C10866FD-7BDE-479F-A1F0-E5873CEECCB4', 4, '2020-04-01 00:00:00', 2019, '8495ACCB-9829-44B1-B8BA-1B9801A33233'),
						(2, '2019-06-30 23:59:00', '2019-06-30', '201906', '2F8C1302-365E-4037-8AA7-E62FF8817EBA', 5, '2019-06-01 00:00:00', 2019, '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC'),
						(2, '2019-05-31 23:59:00', '2019-05-31', '201905', '4369AF78-5528-4888-895F-E7C4C16FE868', 6, '2019-05-01 00:00:00', 2019, '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC');

					INSERT INTO [{0}].[Finance].[GRP__GeneralLedgerAggregateData]
						(BranchKey, CompanyKey, Currency, DepartmentKey, GLAccountKey, GLAmountLocalCredit, GLAmountLocalDebit, GLAmountOSCredit, GLAmountOSDebit, PostPeriod, TaxBranchKey, TransactionCategory, GLAmountLocalBalance, GLAmountOSBalance)
					VALUES
						(1, 1, NULL, 88, 220, 0.00, 100.00, NULL, NULL, '201905', NULL, '', 100.00, 100.00),
						(1, 1, NULL, 88, 220, 0.00, 200.00, NULL, NULL, '201906', NULL, '', 200.00, 200.00),
						(1, 1, NULL, 88, 343, 0.00, 66.00, NULL, NULL, '201907', NULL, '', 66.00, 66.00),
						(1, 1, NULL, 88, 220, 0.00, 300.00, NULL, NULL, '201907', NULL, '', 300.00, 300.00),
						(1, 1, NULL, 88, 220, 0.00, 400.00, NULL, NULL, '201908', NULL, '', 400.00, 400.00),
						(1, 1, NULL, 88, 220, 0.00, 500.00, NULL, NULL, '201909', NULL, '', 500.00, 500.00),
						(1, 1, NULL, 88, 220, 0.00, 600.00, NULL, NULL, '201910', NULL, '', 600.00, 600.00);
				", ScriptDbName);
				TestConnection.ExecuteNonQuery(sqlText);
			}
		}

		public void TestOpeningBalanceWhenPartiallyMatchedTransactionsInvolved()
		{
			PrepareData();
			var result = Excute(CompanyPK.ToString(), 201602, "");
			AssertEquals(1, result.Select("AccountNum='6210.00.00' and AccountType ='BSH' and AG_DebitCredit='DR' and OsOpeningBalance =70 and Period=201602 and Account='WALHAT'").Length);

			void PrepareData()
			{
				var sqlText = string.Format(CultureInfo.InvariantCulture, @"
					
					INSERT INTO [{0}].Finance.BAS__PeriodManagement
						(CompanyKey, PeriodManagementID, Period, Year, StartDate, EndDate, PeriodManagementKey)
					VALUES
						(1, NEWID(), 201512, 2015, '12/01/2015', '12/30/2015 23:59', 1),
						(1, NEWID(), 201601, 2016, '01/01/2016', '01/31/2016 23:59', 2),
						(1, NEWID(), 201602, 2016, '02/01/2016', '02/29/2016 23:59', 3)

					INSERT INTO [{0}].Finance.BAS__GLTransactionHeader
						(GLTransactionHeaderKey, GLTransactionHeaderID, TransactionTypeCode, CompanyKey, BranchKey, Currency, DepartmentKey, ExchangeRate, LedgerCode, LocalAmount, OrganizationHeaderKey, IsOSOutstandingAmountApplicable, LocalTotal, PostToGL, PostDateTime)
					VALUES
						(1, '7BE5FA30-4618-497C-8E83-182104925E4F', 'JNL', 1, 1, 'CNY', 1, 1.0, 'AR', 100.00, 1, 0, 100.00, 'Y', '2015-12-05'),
						(2, '344319BA-BBFE-4E80-8B1F-0E587B3240C6', 'REC', 1, 1, 'CNY', 1, 1.0, 'AR', -30.00, 1, 0, -30.00, 'Y', '2016-01-05')

					INSERT INTO [{0}].Finance.BAS__GLTransactionMatchLink
						(GLTransactionMatchLinkID, GLTransactionMatchLinkKey, GLTransactionHeaderKey, MatchDate, MatchGroupNum, MatchPeriod, Amount, OSAmount)
					VALUES
						(NEWID(), 1, 2, '2016-01-20 15:44:00', 'M10000', 0, -30.00, 0),
						(NEWID(), 2, 1, '2016-01-20 15:44:00', 'M10000', 0, 30.00, 0)
				", ScriptDbName);
				TestConnection.ExecuteNonQuery(sqlText);
			}
		}

		public void TestCalculateCorrectOSAmountBasedOnPeriod()
		{
			PrepareData();
			var result = Excute(CompanyPK.ToString(), 201504, "");
			AssertEquals(1, result.Select("AccountNum='8210.00.00' and AccountType ='BSH' and AG_DebitCredit='CR' and Amount =-120 and OsAmount = -20 and Period=201504 and Account='WALHAT'").Length);

			void PrepareData()
			{
				var sqlText = string.Format(CultureInfo.InvariantCulture, @"
					
					INSERT INTO [{0}].Finance.BAS__PeriodManagement
						(CompanyKey, PeriodManagementID, Period, Year, StartDate, EndDate, PeriodManagementKey)
					VALUES
						(1, NEWID(), 201504, 2015, '04/01/2015', '04/30/2015 23:59', 1)

					INSERT INTO [{0}].Finance.BAS__GLTransactionHeader
						(GLTransactionHeaderKey, GLTransactionHeaderID, TransactionTypeCode, CompanyKey, BranchKey, Currency, DepartmentKey, ExchangeRate, LedgerCode, LocalAmount, OverseasAmount, OrganizationHeaderKey, IsOSOutstandingAmountApplicable, LocalTotal, PostToGL, PostDateTime, GLAccountKey, BankAccountKey)
					VALUES
						(1, '7BE5FA30-4618-497C-8E83-182104925E4F', 'PAY', 1, 1, 'USD', 1, 6.0, 'AP', -120.00, -20, 1, 0, -120.00, 'Y', '2015-04-30', 2, 1),
						(2, '344319BA-BBFE-4E80-8B1F-0E587B3240C6', 'PAY', 1, 1, 'USD', 1, 6.0, 'AP', -180.00, -30, 1, 0, -180.00, 'Y', '2015-05-01', 2, 1)

				", ScriptDbName);
				TestConnection.ExecuteNonQuery(sqlText);
			}
		}

		public void TestAmountWithOtherTaxes()
		{
			PrepareData();
			var result = Excute(CompanyPK.ToString(), 202007, "");
			AssertEquals(1, result.Select("AccountNum='8210.00.00' and AccountType ='BSH' and AG_DebitCredit='CR' and Amount =100 and OsAmount = 100 and Period=202007 and Account='WALHAT'").Length);
			
			void PrepareData()
			{
				var sqlText = string.Format(CultureInfo.InvariantCulture, @"
					
					INSERT INTO [{0}].Finance.BAS__PeriodManagement
						(CompanyKey, PeriodManagementID, Period, Year, StartDate, EndDate, PeriodManagementKey)
					VALUES
						(1, NEWID(), 202007, 2020, '07/01/2020', '07/31/2020 23:59', 1)

					INSERT INTO [{0}].Finance.BAS__GLTransactionHeader
						(GLTransactionHeaderKey, GLTransactionHeaderID, TransactionTypeCode, CompanyKey, BranchKey, Currency, DepartmentKey, ExchangeRate, LedgerCode, LocalAmount, TaxAmount, OverseasAmount, OrganizationHeaderKey, IsOSOutstandingAmountApplicable, LocalTotal, PostToGL, PostDateTime, GLAccountKey, BankAccountKey)
					VALUES
						(1, '7BE5FA30-4618-497C-8E83-182104925E4F', 'PAY', 1, 1, 'USD', 1, 1.0, 'AP', -120.00, 120, 100, 1, 0, 100.00, 'Y', '2020-07-30', 2, 1)

				", ScriptDbName);
				TestConnection.ExecuteNonQuery(sqlText);
			}
		}

		public void TestCalculateCorrectOpeningBalanceWithSecondLocalGLAccount()
		{
			PrepareData();
			var result = Excute(CompanyPK.ToString(), 201504, "");
			AssertEquals("USD Currency Row Count", 2, result.Select("Currency = 'USD'").Length);
			AssertEquals(1, result.Select("OpeningBalance = -180 and OsOpeningBalance=-30 and OSAmount =0 and Account ='COLFAB' and Amount = 0 and AccountNum='8210.00.00' and Period = 201504").Length);
			AssertEquals(1, result.Select("AccountNum='8210.00.00' and AccountType ='BSH' and AG_DebitCredit='CR' and Amount =-120 and OsAmount = -20 and OpeningBalance = -120 and Period=201504 and Account='WALHAT'").Length);

			void PrepareData()
			{
				var sqlText = string.Format(CultureInfo.InvariantCulture, @"
					INSERT INTO [{0}].Organization.BAS__Organization
						(OrganizationID, OrganizationKey, Code)
					VALUES
						(NEWID(), 2, 'COLFAB')

					INSERT INTO [{0}].Finance.BAS__PeriodManagement
						(CompanyKey, PeriodManagementID, Period, Year, StartDate, EndDate, PeriodManagementKey)
					VALUES
						(1, NEWID(), 201503, 2015, '03/01/2015', '03/31/2015 23:59', 1),
						(1, NEWID(), 201504, 2015, '04/01/2015', '04/30/2015 23:59', 2)

					INSERT INTO [{0}].Finance.BAS__GLTransactionHeader
						(GLTransactionHeaderKey, GLTransactionHeaderID, TransactionTypeCode, CompanyKey, BranchKey, Currency, DepartmentKey, ExchangeRate, LedgerCode, LocalAmount, OverseasAmount, OrganizationHeaderKey, IsOSOutstandingAmountApplicable, LocalTotal, PostToGL, PostDateTime, GLAccountKey, BankAccountKey)
					VALUES
						(1, NEWID(), 'PAY', 1, 1, 'USD', 1, 6.0, 'AP', -120.00, -20.00, 1, 0, -120.00, 'Y', '2015-03-30', 2, 1),
						(2, NEWID(), 'PAY', 1, 1, 'USD', 1, 6.0, 'AP', -180.00, -30.00, 2, 0, -180.00, 'Y', '2015-03-01', 2, 1),
						(3, NEWID(), 'PAY', 1, 1, 'USD', 1, 6.0, 'AP', -120.00, -20.00, 1, 0, -120.00, 'Y', '2015-04-30', 2, 1)

					INSERT INTO [{0}].Finance.BAS__GLAccountDescriptor
						(AccountDescription, DebitCredit, GLAccountDescriptorID, GLAccountDescriptorKey, GLAccountKey, Language, LocalAccountNumber, ReportCategory, ReportType, TotalLevel, AlternativeDescriptorKey, CarriedForwardDescriptorKey, ConsolidationDescriptorKey, CountryOfCompliance, HeaderDependsOnTotalAccountKey, PercentNumKey, PrintSequence)
					VALUES
						( '01', 'CR', 'E94C4723-A84E-4D01-85B3-B09F6C6D3AFB', 1, NULL, 'ZH-CN', '12340100', '', 'COA', 0, NULL, NULL, NULL, 'CN', NULL, NULL, 0)

					INSERT INTO [{0}].Finance.BAS__GLDescriptorPivot
						(GLDescriptorPivotID, GLDescriptorPivotKey, GLAccountKey, GLAccountDescriptorKey)
					VALUES
						(NEWID(), 1, 2, 1)

					INSERT INTO [{0}].Finance.BAS__LocalNumberFormatData
						(LocalNumberFormatDataID, LocalNumberFormatDataKey, LocalNumberFormatDataValue)
					VALUES
						(NEWID(), 1, '﻿<ArrayOfGLLocalNumberFormat xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema""><GLLocalNumberFormat><Language>ZH-CN</Language><CountryCode>CN</CountryCode><NumberFormat>4-2-2</NumberFormat><IsFixedLength>Y</IsFixedLength></GLLocalNumberFormat></ArrayOfGLLocalNumberFormat>')
				", ScriptDbName);
				TestConnection.ExecuteNonQuery(sqlText);
			}
		}

		public void TestChinaGLAccountBalanceWithIncludePresentationJournals()
		{
			PrepareData();
			var result = Excute("878D7ACA-FFC3-49FC-9710-969CA0C0F2AC", 201504, "", "Y");
			AssertEquals("Row Count", 1, result.Rows.Count);

			result = Excute("878D7ACA-FFC3-49FC-9710-969CA0C0F2AC", 201504, "");
			AssertEquals("Row Count", 0, result.Rows.Count);
			void PrepareData()
			{
				var sqlText = string.Format(CultureInfo.InvariantCulture, @"

					DECLARE @registryRawValue nvarchar(MAX)
					Set @registryRawValue = '<?xml version=""1.0"" encoding=""utf-16""?><GLPresentationJournalCategoryCollection xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
						<GLPresentationJournalCategory>
							<CodeMaxLength>3</CodeMaxLength>
							<Code>INT</Code>
							<Description>Inter Company</Description>
							<Bool>Y</Bool>
							<Bool2>N</Bool2>
							<Bool3>Y</Bool3>
						</GLPresentationJournalCategory>
					</GLPresentationJournalCategoryCollection>'

					INSERT INTO [{0}].Finance.BAS__AccountStmData
						(AccountStmDataID, AccountStmDataKey, BinaryValue, OwnerID, SDName)
					VALUES
						(NEWID(), 1, CONVERT(varbinary(MAX), CONVERT(XML, @registryRawValue)), '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', 'GLJournalAdjustmentCategoriesList')

					INSERT INTO
						[{0}].Organization.BAS__Company (CompanyKey, CompanyID, CountryCode, CompanyCode, LocalCurrency, Name, IsActive, IsReciprocal, IsGSTRegistered)
					VALUES
						(2, '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', 'AU', 'EDI', 'AUD', 'EDI Company', 1, 0, 1)

					INSERT INTO
						[{0}].Organization.BAS__Branch (BranchKey, BranchID, CompanyKey, IsActive, BranchCode, BranchName)
					VALUES
						(2, 'FDD429D2-648C-4895-8F9F-06E90DED2BE5', 2, 1, 'SYD', '(SYD) EDIHQ')

					INSERT INTO [{0}].Finance.BAS__GLAccount
						(Account, AccountGroup, AccountNo, AccountNumberForTotals, AccountType, AccountTypeCode, AlternateAccountKey, ConsolidationAccountKey, DebitCredit, Description, GLAccountID, GLAccountKey, HeaderDependsOnTotal, ParentGLAccountNo, SectionType, TotalLevel, DebitCreditCode, HeaderDependsOnTotalKey, PercentAccountKey, PrintSequence, CashFlowType)
					VALUES
						('(4710.00.00)', NULL, '4710.00.00', '14710.00.00', 'Profit & Loss', 'P&L', NULL, NULL, 'Debit', NULL, 'AC129D82-B88D-45EE-BCE5-25592F734023', 56, NULL, NULL, 'TS', 0, 'DR', NULL, NULL, 0, 'XXX')

					INSERT INTO [{0}].Finance.BAS__BankAccount
						(BankAccountKey, BankAccountID, BankCode, BankCurrency, BranchKey, CompanyKey, GLAccountKey)
					VALUES
						(2, NEWID(), 'BBB', 'AUD', NULL, 2, 56)	

					INSERT INTO [{0}].Finance.BAS__PeriodManagement
						(CompanyKey, PeriodManagementID, Period, Year, StartDate, EndDate, PeriodManagementKey)
					VALUES
						(2, NEWID(), 201504, 2015, '04/01/2015', '04/30/2015 23:59', 1)

					INSERT INTO [{0}].[Finance].[GRP__GeneralLedgerAggregateData]
						(BranchKey, CompanyKey, Currency, DepartmentKey, GLAccountKey, GLAmountLocalCredit, GLAmountLocalDebit, GLAmountOSCredit, GLAmountOSDebit, PostPeriod, TaxBranchKey, TransactionCategory, GLAmountLocalBalance, GLAmountOSBalance)
					VALUES
						(2, 2, NULL, 1, 56, 0.00, 2.00, NULL, NULL, 201504, NULL, 'INT', 2.00, 2.00)

					INSERT INTO [{0}].Finance.BAS__GLTransactionHeader
						(GLTransactionHeaderKey, GLTransactionHeaderID, TransactionTypeCode, CompanyKey, BranchKey, Currency, DepartmentKey, ExchangeRate, LedgerCode, LocalAmount, OverseasAmount, OrganizationHeaderKey, IsOSOutstandingAmountApplicable, LocalTotal, PostToGL, PostDateTime, GLAccountKey, BankAccountKey, TransactionNo, TransactionCategory)
					VALUES
						(1, NEWID(), 'PAY', 2, 2, '', 1, 1.0, 'AP', -100.00, -100.00, 1, 0, -100.00, 'N', '2015-04-20 15:44:00', 56, 2, '', ''),
						(2, NEWID(), 'GJL', 2, 2, '', 1, 1.0, 'GL', -100.00, -100.00, 1, 0, -100.00, 'N', '2015-04-20 15:44:00', 56, 2, 'Test2', 'INT')

					INSERT INTO [{0}].Finance.BAS__AccGLTransactionLine
						(AccGLTransactionLineID, AccGLTransactionLineKey, BranchKey, CompanyKey, Currency, DepartmentID, DepartmentKey, Description, ExchangeRate, GLAccountKey, GLTransactionHeaderKey, LineType)
					VALUES
						('CBF177D8-EFA3-408E-9F79-DCC679A70DF4', 1, 2, 2, '', 'F5C72696-19AD-4759-879F-89C8532FF238', 1, '', 0.000000000, 56, 2, 'GJL')

					INSERT INTO [{0}].Finance.BAS__StmDataDate
						(CompanyID, Name, StmDataDateID, StmDataDateKey, Value, CompanyKey)
					VALUES
						('878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', 'JournalEntriesLastProcessedDate', NEWID(), 2, '2001-01-01 00:00:00.000', 2)
				", ScriptDbName);
				TestConnection.ExecuteNonQuery(sqlText);
			}
		}

		public void TestOsOpeningBalanceAndOsAmountForBranchPKList()
		{
			PrepareData();
			var result = Excute(CompanyPK.ToString(), 201602, "");
			AssertEquals(1, result.Select("AG_DebitCredit = 'DR' and OpeningBalance = 70 and OsOpeningBalance=70 and AccountNum ='6210.00.00'").Length);

			result = Excute(CompanyPK.ToString(), 201602, "51FB2284-2FCA-4F5F-A346-6950CB4D5211");
			AssertEquals(1, result.Select("AG_DebitCredit = 'DR' and OpeningBalance = 100 and Amount =0").Length);

			result = Excute(CompanyPK.ToString(), 201601, "");
			AssertEquals(1, result.Select("AG_DebitCredit = 'DR' and Amount =-30 and OpeningBalance = 100 and Period =201601").Length);

			result = Excute(CompanyPK.ToString(), 201601, "51FB2284-2FCA-4F5F-A346-6950CB4D5211");
			AssertEquals(1, result.Select("AG_DebitCredit = 'DR' and Amount =0 and OpeningBalance = 100 and Currency = 'USD'").Length);

			void PrepareData()
			{
				var sqlText = string.Format(CultureInfo.InvariantCulture, @"
					INSERT INTO
						[{0}].Organization.BAS__Branch (BranchKey, BranchID, CompanyKey, IsActive, BranchCode, BranchName)
					VALUES
						(2, NEWID(), 1, 1, 'OTH', 'OTH 2')

					INSERT INTO [{0}].Finance.BAS__PeriodManagement
						(CompanyKey, PeriodManagementID, Period, Year, StartDate, EndDate, PeriodManagementKey)
					VALUES
						(1, NEWID(), 201601, 2016, '01/01/2016', '01/31/2016 23:59', 1)

					INSERT INTO [{0}].Finance.BAS__GLTransactionHeader
						(GLTransactionHeaderKey, GLTransactionHeaderID, TransactionTypeCode, CompanyKey, BranchKey, Currency, DepartmentKey, ExchangeRate, LedgerCode, LocalAmount, OverseasAmount, OrganizationHeaderKey, IsOSOutstandingAmountApplicable, LocalTotal, PostToGL, PostDateTime, GLAccountKey, BankAccountKey, TransactionNo, TransactionCategory)
					VALUES
						(1, NEWID(), 'REC', 1, 1, 'USD', 1, 1.0, 'AR', 100.00, 100.00, 1, 0, 100.00, 'Y', '2015-12-25 15:44:00', NULL, 1, '00001001', ''),
						(2, NEWID(), 'ORC', 1, 1, 'USD', 1, 1.0, 'CB', -500.00, -500.00, 1, 0, -500.00, 'Y', '2015-12-05 15:44:00', NULL, 1, '00001003', ''),
						(3, NEWID(), 'REC', 1, 2, 'USD', 1, 1.0, 'AR', -30.00, -30.00, 1, 0, -30.00, 'Y', '2016-01-05 15:44:00', NULL, 1, '00001002', ''),
						(4, NEWID(), 'OPY', 1, 2, 'USD', 1, 1.0, 'CB', 300.00, 300.00, 1, 0, 300.00, 'Y', '2016-01-05 15:44:00', NULL, 1, '00001004', '')
				", ScriptDbName);
				TestConnection.ExecuteNonQuery(sqlText);
			}
		}

		public void TestTier2IsEmptyWithLocalNumberFormat()
		{
			PrepareTestDataForEmptyTier2("ZH-CN");
			var result = Excute(CompanyPK.ToString(), 202403, "");
			AssertEquals(1, result.Select("GLAccount = '10010000' and Amount = 60").Length);
			AssertEquals(1, result.Select("GLAccount = '10010001' and Amount = 20").Length);
			AssertEquals(1, result.Select("GLAccount = '10010100' and Amount = 40").Length);
			AssertEquals(1, result.Select("GLAccount = '10010101' and Amount = 10").Length);
		}

		public void TestTier2IsEmptyWithoutLocalNumberFormat()
		{
			PrepareTestDataForEmptyTier2("SQ-AL");
			var result = Excute(CompanyPK.ToString(), 202403, "");
			AssertEquals(0, result.Select("GLAccount = '10010000' and Amount = 60").Length);
			AssertEquals(1, result.Select("GLAccount = '10010001' and Amount = 20").Length);
			AssertEquals(1, result.Select("GLAccount = '10010100' and Amount = 30").Length);
			AssertEquals(1, result.Select("GLAccount = '10010101' and Amount = 10").Length);
		}

		void PrepareTestDataForEmptyTier2(string language)
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
					INSERT INTO [{0}].Finance.BAS__LocalNumberFormatData
						(LocalNumberFormatDataID, LocalNumberFormatDataKey, LocalNumberFormatDataValue)
					VALUES
						(NEWID(), 1, '<?xml version=""1.0"" encoding=""utf-16""?><ArrayOfGLLocalNumberFormat xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""><GLLocalNumberFormat><Language>{1}</Language><CountryCode>CN</CountryCode><NumberFormat>4-2-2</NumberFormat><IsFixedLength>N</IsFixedLength></GLLocalNumberFormat></ArrayOfGLLocalNumberFormat>')

					INSERT INTO [{0}].Finance.BAS__PeriodManagement
						(CompanyKey, PeriodManagementID, Period, Year, StartDate, EndDate, PeriodManagementKey)
					VALUES
						(1, NEWID(), 202401, 2024, '01/01/2024', '01/31/2024 23:59', 1),
						(1, NEWID(), 202402, 2024, '02/01/2024', '02/29/2024 23:59', 2),
						(1, NEWID(), 202403, 2024, '03/01/2024', '03/31/2024 23:59', 3),
						(1, NEWID(), 202404, 2024, '04/01/2024', '04/30/2024 23:59', 4),
						(1, NEWID(), 202405, 2024, '05/01/2024', '05/31/2024 23:59', 5),
						(1, NEWID(), 202406, 2024, '06/01/2024', '06/30/2024 23:59', 6),
						(1, NEWID(), 202407, 2024, '07/01/2024', '07/31/2024 23:59', 7),
						(1, NEWID(), 202408, 2024, '08/01/2024', '08/31/2024 23:59', 8),
						(1, NEWID(), 202409, 2024, '09/01/2024', '09/30/2024 23:59', 9),
						(1, NEWID(), 2024010, 2024, '10/01/2024', '10/31/2024 23:59', 10),
						(1, NEWID(), 2024011, 2024, '11/01/2024', '11/30/2024 23:59', 11),
						(1, NEWID(), 2024012, 2024, '12/01/2024', '12/31/2024 23:59', 12)

					INSERT INTO [{0}].Finance.BAS__GLAccount
						(Account, AccountGroup, AccountNo, AccountNumberForTotals, AccountType, AccountTypeCode, AlternateAccountKey, ConsolidationAccountKey, DebitCredit, Description, GLAccountID, GLAccountKey, HeaderDependsOnTotal, ParentGLAccountNo, SectionType, TotalLevel, TranslationKey, Units, DebitCreditCode, HeaderDependsOnTotalKey, PercentAccountKey, PrintSequence, CashFlowType)
					VALUES
						('(989895)', NULL, '989895', '00989895', 'Balance Sheet', 'BSH', NULL, NULL, 'Debit', NULL, NEWID(), 33, NULL, NULL, 'TS', 0, 'AG_Description$', NULL, 'DR', NULL, NULL, 0, NULL),
						('(989896)', NULL, '989896', '00989896', 'Balance Sheet', 'BSH', NULL, NULL, 'Debit', NULL, NEWID(), 34, NULL, NULL, 'TS', 0, 'AG_Description$', NULL, 'DR', NULL, NULL, 0, NULL),
						('(989897)', NULL, '989897', '00989897', 'Balance Sheet', 'BSH', NULL, NULL, 'Debit', NULL, NEWID(), 35, NULL, NULL, 'TS', 0, 'AG_Description$', NULL, 'DR', NULL, NULL, 0, NULL),
						('(989898)', NULL, '989898', '00989898', 'Balance Sheet', 'BSH', NULL, NULL, 'Debit', NULL, NEWID(), 36, NULL, NULL, 'TS', 0, 'AG_Description$', NULL, 'DR', NULL, NULL, 0, NULL)

					INSERT INTO [{0}].Finance.BAS__GLAccountDescriptor
						(AccountDescription, DebitCredit, GLAccountDescriptorID, GLAccountDescriptorKey, GLAccountKey, Language, LocalAccountNumber, ReportCategory, ReportType, TotalLevel, AlternativeDescriptorKey, CarriedForwardDescriptorKey, ConsolidationDescriptorKey, CountryOfCompliance, HeaderDependsOnTotalAccountKey, PercentNumKey, PrintSequence)
					VALUES
						( '10010100 - Description', 'DR', NEWID(), 1, NULL, 'ZH-CN', '10010000', '', 'COA', 0, NULL, NULL, NULL, 'CN', NULL, NULL, 0),
						( '10010101 - Description', 'DR', NEWID(), 2, NULL, 'ZH-CN', '10010101', '', 'COA', 0, NULL, NULL, NULL, 'CN', NULL, NULL, 0),
						( '10010001 - Description', 'DR', NEWID(), 3, NULL, 'ZH-CN', '10010001', '', 'COA', 0, NULL, NULL, NULL, 'CN', NULL, NULL, 0),
						( '10010100 - Description', 'DR', NEWID(), 4, NULL, 'ZH-CN', '10010100', '', 'COA', 0, NULL, NULL, NULL, 'CN', NULL, NULL, 0)

					INSERT INTO [{0}].Finance.BAS__GLDescriptorPivot
						(GLDescriptorPivotID, GLDescriptorPivotKey, GLAccountKey, GLAccountDescriptorKey)
					VALUES
						(NEWID(), 1, 36, 1),
						(NEWID(), 2, 34, 2),
						(NEWID(), 3, 33, 3),
						(NEWID(), 4, 35, 4)

					INSERT INTO [{0}].[Finance].[GRP__GeneralLedgerAggregateData]
						(BranchKey, CompanyKey, Currency, DepartmentKey, GLAccountKey, GLAmountLocalCredit, GLAmountLocalDebit, GLAmountOSCredit, GLAmountOSDebit, PostPeriod, TaxBranchKey, TransactionCategory, GLAmountLocalBalance, GLAmountOSBalance)
					VALUES
						(1, 1, NULL, 1, 35, 0.00, 30.00, NULL, NULL, 202403, NULL, '', 30.00, 30.00),
						(1, 1, NULL, 1, 34, 0.00, 10.00, NULL, NULL, 202403, NULL, '', 10.00, 10.00),
						(1, 1, NULL, 1, 33, 0.00, 20.00, NULL, NULL, 202403, NULL, '', 20.00, 20.00)

			", ScriptDbName, language);
			TestConnection.ExecuteNonQuery(sqlText);
		}

		DataTable Excute(string companyPK, int endPeriod, string branchList, string includePeriodEndCLosing = "")
		{
			var sqlBuilder = new StringBuilder();
			sqlBuilder.AppendLine($"EXEC [{ScriptDbName}].[dbo].[ChinaGLAccountBalance]");
			sqlBuilder.Append($"@CompanyPK = '{companyPK}'");
			sqlBuilder.Append(
				$@",@EndPeriod = ").Append(endPeriod == 0 ? "NULL" : endPeriod.ToString());
			sqlBuilder.Append(
				$@",@BranchPKList = ").Append(branchList == null ? "NULL" : $"'{branchList}'");
			sqlBuilder.Append(
				$@",@IncludePeriodEndCLosing = ").Append($"'{includePeriodEndCLosing}'");

			var sqlText = string.Format(CultureInfo.InvariantCulture,
				sqlBuilder.ToString()
			);

			return DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
		}

		protected override void SetUp()
		{
			base.SetUp();
			CompanyPK = Guid.NewGuid();
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
					DELETE FROM [{0}].Customs.BAS__Account
					DELETE FROM [{0}].Finance.BAS__GLAccount
					DELETE FROM [{0}].Organization.BAS__Company
					DELETE FROM [{0}].Organization.BAS__Branch
					DELETE FROM [{0}].Finance.BAS__PeriodManagement
					
					INSERT INTO
						[{0}].Customs.BAS__Account (AccountID, AccountKey, AccountType, CompanyKey, DepartmentKey, GLAccountKey)
					VALUES
						('98905FB2-BDD1-46B6-9AE7-85FD07CAAF49', 5, 'GL_PL_APPROPRIATION_ACCOUNT', NULL, NULL, 343),
						('98905FB2-BDD1-46B6-9AE7-85FD07CAAF48', 4, 'GL_AR_CONTROL_ACCOUNT', NULL, NULL, 1),
						('98905FB2-BDD1-46B6-9AE7-85FD07CAAF47', 3, 'GL_AP_CONTROL_ACCOUNT', NULL, NULL, 2)

					INSERT INTO [{0}].Finance.BAS__GLAccount
						(Account, AccountGroup, AccountNo, AccountNumberForTotals, AccountType, AccountTypeCode, AlternateAccountKey, ConsolidationAccountKey, DebitCredit, Description, GLAccountID, GLAccountKey, HeaderDependsOnTotal, ParentGLAccountNo, SectionType, TotalLevel, TranslationKey, Units, DebitCreditCode, HeaderDependsOnTotalKey, PercentAccountKey, PrintSequence, CashFlowType)
					VALUES
						('(3333.22.22)', NULL, '3333.22.22', '13333.22.22', 'Profit & Loss', 'P&L', NULL, NULL, 'Credit', NULL, '407FC0EF-AA91-480A-8417-9AF801A2425D', 220, NULL, '3333.22.00', 'TS', 0, 'AG_Description$', NULL, 'CR', NULL, NULL, 0, NULL),
						('(4900.00.00)', NULL, '4900.00.00', '14900.00.00', 'BSH', 'BSH', NULL, NULL, 'Credit', NULL, '176E946D-3483-4B2B-AA32-E348CBD94DFD', 343, NULL, NULL, 'TS', 0, 'AG_Description$', NULL, 'CR', NULL, NULL, 0, 'XXX'),
						('(6210.00.00)', NULL, '6210.00.00', '56210.00.00', 'BSH', 'BSH', NULL, NULL, 'Credit', NULL, 'F3E08B3D-4B2D-414D-951E-8ECD5E85B29B', 1, NULL, NULL, 'AS', 0, 'AG_Description$', NULL, 'DR', NULL, NULL, 0, 'XXX'),
						('(8210.00.00)', NULL, '8210.00.00', '58210.00.00', 'BSH', 'BSH', NULL, NULL, 'Credit', NULL, '7A5FF88B-76BE-41AE-A3EC-CEAC983125EF', 2, NULL, NULL, 'AS', 0, 'AG_Description$', NULL, 'CR', NULL, NULL, 0, 'XXX')

					INSERT INTO [{0}].Finance.BAS__BankAccount
						(BankAccountKey, BankAccountID, BankCode, BankCurrency, BranchKey, CompanyKey, GLAccountKey)
					VALUES
						(1, 'd4d40998-b563-4e6f-b1ed-8ea852e0f2fd', 'AAA', 'USD', 1, 1, 2)	

					INSERT INTO [{0}].Finance.BAS__StmDataDate
						(CompanyID, Name, StmDataDateID, StmDataDateKey, Value, CompanyKey)
					VALUES
						('{1}', 'JournalEntriesLastProcessedDate', '8495ACCB-9829-44B1-B8BA-1B9801A33233', 1, '2001-01-01 00:00:00.000', 1)

					INSERT INTO [{0}].Organization.BAS__Organization
						(OrganizationID, OrganizationKey, Code)
					VALUES
						('c72bfdd2-e264-4dd9-a5ab-3f75efd8569b', 1, 'WALHAT')

					INSERT INTO [{0}].Finance.BAS__Currency
						(CurrencyID, CurrencyKey, CurrencyCode, Currency, IsActive)
					VALUES
						('99D67987-AAFE-410D-A567-B6F9792D1F76', 1, 'CNY', 'Y (CNY) Chinese Yuan', 1),
						(NEWID(), 2, 'USD', 'United States Dollar', 1),
						(NEWID(), 3, 'AUD', '(AUD) Australian Dollar', 1)

					INSERT INTO
						[{0}].Organization.BAS__Company (CompanyKey, CompanyID, CountryCode, CompanyCode, LocalCurrency, Name, IsActive, IsReciprocal, IsGSTRegistered)
					VALUES
						(1, '{1}', 'CN', 'ABC', 'CNY', 'ABC Company', 1, 1, 1)

					INSERT INTO
						[{0}].Organization.BAS__Branch (BranchKey, BranchID, CompanyKey, IsActive, BranchCode, BranchName)
					VALUES
						(1, '51FB2284-2FCA-4F5F-A346-6950CB4D5211', 1, 1, 'BR2', 'Branch 2')

					INSERT INTO
						[{0}].Organization.BAS__Department (DepartmentKey, DepartmentID, IsActive, Code, Department)
					VALUES
						(1, 'C39F0A7D-4F00-4106-BD6D-F0979DA7719C', 1, 'DP2', 'Department 2')
			", ScriptDbName, CompanyPK);
			TestConnection.ExecuteNonQuery(sqlText);
		}

		Guid CompanyPK;

		protected override string ScriptDbName => Db.EdwDatabaseName;
	}
}
