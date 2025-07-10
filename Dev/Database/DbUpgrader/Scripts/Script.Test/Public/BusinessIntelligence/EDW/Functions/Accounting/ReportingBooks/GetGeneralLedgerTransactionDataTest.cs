using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting.ReportingBooks;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Functions.Accounting.Testing
{
	[TestedType(typeof(GetGeneralLedgerTransactionData))]
	class GetGeneralLedgerTransactionDataTest : BiCreateScriptTest
	{
		[ExpectNoExceptions]
		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
		public void TestRun()
		{
			using (var edwConnection = Db.NewAdminConnection(ScriptDbName))
			{
				var resultsTable = Execute(edwConnection, CompanyPK1, ReportingBookPK1, Array.Empty<Guid>(), Array.Empty<Guid>());
				var columns = GetColumns();
				var columnCount = 0;
				CombineAssertions(() =>
				{
					foreach (DataColumn column in resultsTable.Columns)
					{
						var columnExists = columns.Any(x => x == column.ToString());
						AssertEquals($"Expected {column} to exist in existing list of columns", columnExists, true);
						if (columnExists)
						{
							columnCount++;
						}
					}
				});

				AssertEquals(columns.Count, columnCount);

				PrepareTestData(edwConnection);
				var result = Execute(edwConnection, CompanyPK1, ReportingBookPK1, Array.Empty<Guid>(), Array.Empty<Guid>());
				AssertEquals(6, result.Rows.Count);
				AssertEquals(1, result.Select($"BranchKey = 2 and BranchCode = 'UUY' and DepartmentCode = 'YYY'and CompanyKey = 1 and Currency = 'CNY' and DueDate = '2023-06-07' and JournalEntriesNumber = '0000000002' and JournalEntriesType = 'PST' and TaxBranchCode = 'UUO' and TransactionHeaderKey = 2 and LineDescription = 'lineDesc' and ChequeOrReference = '456' and JobInvoiceNumber = 'Jiu02' and ExchangeRate = 3.4 and ComplianceSubType = 'ert' and ComplianceNumber = '345' and OrganizationKey = 3 and LineJobKey = 1 and TransactionType ='REV' AND TranslatedCurrency = 'CNY'").Length);
				AssertEquals(1, result.Select($"TransactionDate = '2023-09-08 00:00:00' and LedgerCode ='AP' and PostDate = '2023-03-07 00:00:00' and HeaderDescription = 'HDesc' and TransactionLineKey = 4 and DepartmentKey = 1 and GLAccountKey = 1 and ChargeCodeKey = 2 and GLAccountNum = '10100009' and GLAmountLocalDebit = 0 and GLAmountLocalCredit = 1 and GLAmountlocalBalance = -1 and GLAmountOSCredit = 3 and TranslatedCredit is null and TranslatedBalance is null and TranslatedExchangeRate =2 and CurrencyTranslationLevel = 'BAL'").Length);
				AssertEquals(1, result.Select($"GLAmountOSDebit = 0 and GLAmountOSBalance = -3 and PostPeriodForReportingBook = 202305 and PostPeriod = 202305 and TransactionCategory = '' and ReportingBookCode = 'RUR' and ReportingBookKey = 1 and COmpanyOfPeriodKey =1 and Attribute_TIC='' and Attribute_SPR ='NAV' and Attribute_ORG = 'OR2' and Attribute_OCG = 'TPY' and Attribute_LFE ='OEU' and Attribute_LFO ='FOR'").Length);
				AssertEquals(1, result.Select($"AlternateGLAccount = '1233' and AlternateGLAccountDescription = 'Account3' and AlternateGLAccountKey = 3 and DueDate = '2023-08-07' and JournalEntriesNumber = '0000000003' and JournalEntriesType = 'PST' and TaxBranchCode is null and TransactionHeaderKey = 3 and AccountTypeCode = 'P&L' and AlternateChartKey = 1 and TranslatedCredit = 0 and TranslatedDebit = 1 and TranslatedBalance = 1 and TranslatedExchangeRate = 1 and PostPeriodForReportingBook = 202306 and PostPeriod = 202306 and CurrencyTranslationLevel = 'BAL' and Units = 'KG' and OriginalAccount = '365' and OriginalAttribute_TIC='ETI' and OriginalAttribute_SPR ='SPS' and OriginalAttribute_ORG = 'NAV' and OriginalAttribute_OCG = 'INT' and OriginalAttribute_LFE ='LOC' and OriginalAttribute_LFO ='FOR'").Length);
				AssertEquals(1, result.Select($"AlternateGLAccount = '1234' and DueDate = '2023-09-07' and JournalEntriesNumber = '0000000004' and JournalEntriesType = 'PST' and TaxBranchCode is null and TransactionHeaderKey = 4").Length);
				AssertEquals(1, result.Select("AlternateGLAccount = '1235' and JournalEntriesType = 'REC' and TranslatedCredit = 2 and TranslatedDebit = 0 and TranslatedBalance = -2").Length);
				AssertEquals(0, result.Select("CompanyKey = 2").Length);
				AssertEquals(2, result.Select("GLAccountType = 'ARC' OR GLAccountType = 'APC'").Length);
				AssertEquals(2, result.Select("CurrencyTranslationLevel = 'BAL' AND TranslatedCurrency = 'USD' AND TranslatedExchangeRate = 2 AND TranslatedBalance IS NULL").Length);
				AssertEquals(2, result.Select("CurrencyTranslationLevel = 'BAL' AND TranslatedCurrency = 'CNY' AND TranslatedExchangeRate = 1 AND TranslatedBalance IS NOT NULL").Length);
				AssertEquals(4, result.Select("GLType = 'PST'").Length);

				result = Execute(edwConnection, CompanyPK1, ReportingBookPK1, transactionHeaderPKList: new[] { new Guid("C77EF93B-DC28-40FE-A22A-AC355A295215"), new Guid("2EAD1186-B082-40F0-81A4-4C7D4D5BC18C") }, Array.Empty<Guid>());
				AssertEquals(4, result.Rows.Count);
				AssertEquals(2, result.Select($"TransactionlineID = 'D774E57E-5FC7-465C-BC24-D6F795550AC6' and TransactionHeaderID = '2EAD1186-B082-40F0-81A4-4C7D4D5BC18C'").Length);
				AssertEquals(2, result.Select($"TransactionlineID = 'AE6D15CE-63EC-41BF-905C-A95894A08095' and TransactionHeaderID = 'C77EF93B-DC28-40FE-A22A-AC355A295215'").Length);

				result = Execute(edwConnection, CompanyPK1, ReportingBookPK1, new[] { new Guid("2EAD1186-B082-40F0-81A4-4C7D4D5BC18C") }, new[] { new Guid("AE6D15CE-63EC-41BF-905C-A95894A08095") });
				AssertEquals(4, result.Rows.Count);
				AssertEquals(2, result.Select($"TransactionlineID = 'D774E57E-5FC7-465C-BC24-D6F795550AC6' and TransactionHeaderID = '2EAD1186-B082-40F0-81A4-4C7D4D5BC18C'").Length);
				AssertEquals(2, result.Select($"TransactionlineID = 'AE6D15CE-63EC-41BF-905C-A95894A08095' and TransactionHeaderID = 'C77EF93B-DC28-40FE-A22A-AC355A295215'").Length);

				result = Execute(edwConnection, CompanyPK2, ReportingBookPK2, Array.Empty<Guid>(), Array.Empty<Guid>());
				AssertEquals(2, result.Rows.Count);
				AssertEquals(2, result.Select($"CompanyKey = 2 and CompanyOfPeriodKey = 3").Length);
				AssertEquals(1, result.Select($"TranslatedCredit = 1 and TranslatedDebit = 0 and TranslatedBalance = -1 and TranslatedExchangeRate = 1 and AccountTypeCode = 'NTE'").Length);
				AssertEquals(1, result.Select($"TranslatedCredit = 0 and TranslatedDebit is null and TranslatedBalance is null and TranslatedExchangeRate is null and AccountTypeCode = 'BSH'").Length);

				result = Execute(edwConnection, CompanyPK1, ReportingBookPK1, Array.Empty<Guid>(), Array.Empty<Guid>(), "2023-01-09", departmentList: "YYY", branchList: "UUY");
				AssertEquals(1, result.Rows.Count);
				AssertEquals(1, result.Select("BranchKey = 2 and DepartmentKey = 1 and CompanyKey = 1 and CompanyOfPeriodKey = 1").Length);
			}
		}

		HashSet<string> GetColumns()
		{
			var columns = new HashSet<string>();

			columns.Add("AccountTypeCode");
			columns.Add("AlternateChartKey");
			columns.Add("AlternateGLAccount");
			columns.Add("AlternateGLAccountDescription");
			columns.Add("AlternateGLAccountKey");
			columns.Add("Attribute_LFE");
			columns.Add("Attribute_LFO");
			columns.Add("Attribute_OCG");
			columns.Add("Attribute_ORG");
			columns.Add("Attribute_SPR");
			columns.Add("Attribute_TIC");
			columns.Add("BranchCode");
			columns.Add("BranchKey");
			columns.Add("ChargeCodeKey");
			columns.Add("ChequeOrReference");
			columns.Add("CompanyKey");
			columns.Add("CompanyOfPeriodKey");
			columns.Add("ComplianceNumber");
			columns.Add("ComplianceSubType");
			columns.Add("Currency");
			columns.Add("DepartmentCode");
			columns.Add("DepartmentKey");
			columns.Add("DueDate");
			columns.Add("ExchangeRate");
			columns.Add("GLAccountKey");
			columns.Add("GLAccountName");
			columns.Add("GLAccountNum");
			columns.Add("GLAccountType");
			columns.Add("GLAmountLocalBalance");
			columns.Add("GLAmountLocalCredit");
			columns.Add("GLAmountLocalDebit");
			columns.Add("GLAmountOSBalance");
			columns.Add("GLAmountOSCredit");
			columns.Add("GLAmountOSDebit");
			columns.Add("JournalEntriesNumber");
			columns.Add("JournalEntriesType");
			columns.Add("TranslatedCredit");
			columns.Add("TranslatedDebit");
			columns.Add("TranslatedBalance");
			columns.Add("TranslatedExchangeRate");
			columns.Add("CurrencyTranslationLevel");
			columns.Add("TranslatedRateType");
			columns.Add("LocalCurrency");
			columns.Add("TranslatedCurrency");
			columns.Add("GLType");
			columns.Add("HeaderDescription");
			columns.Add("JobInvoiceNumber");
			columns.Add("LedgerCode");
			columns.Add("LineDescription");
			columns.Add("LineJobKey");
			columns.Add("OrganizationKey");
			columns.Add("OriginalAccount");
			columns.Add("OriginalAttribute_LFE");
			columns.Add("OriginalAttribute_LFO");
			columns.Add("OriginalAttribute_OCG");
			columns.Add("OriginalAttribute_ORG");
			columns.Add("OriginalAttribute_SPR");
			columns.Add("OriginalAttribute_TIC");
			columns.Add("PostDate");
			columns.Add("PostPeriod");
			columns.Add("PostPeriodForReportingBook");
			columns.Add("ReportingBookCode");
			columns.Add("ReportingBookKey");
			columns.Add("TaxBranchCode");
			columns.Add("TaxGLMovementKey");
			columns.Add("TransactionCategory");
			columns.Add("TransactionDate");
			columns.Add("TransactionHeaderID");
			columns.Add("TransactionHeaderKey");
			columns.Add("TransactionLineID");
			columns.Add("TransactionLineKey");
			columns.Add("TransactionNum");
			columns.Add("TransactionType");
			columns.Add("Units");

			return columns;
		}

		#region PresentationJournalsOfReportingBook

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
		public void TestIncludePresentationJournalsOfReportingBook()
		{
			using (var edwConnection = Db.NewAdminConnection(ScriptDbName))
			{
				PrepareTestData(edwConnection);
				var sql = string.Format(CultureInfo.InvariantCulture, @"
					UPDATE [{0}].[Finance].[CUS__GeneralLedgerTransactionData]
					SET TransactionCategory = 'EET'
					WHERE GeneralLedgerTransactionDataKey = 1", ScriptDbName);
				edwConnection.ExecuteNonQuery(sql);
				var result = Execute(edwConnection, CompanyPK1, ReportingBookPK1, Array.Empty<Guid>(), Array.Empty<Guid>());
				AssertEquals(5, result.Rows.Count);
				AssertEquals(0, result.Select("TransactionCategory = 'EET'").Length);

				sql = string.Format(CultureInfo.InvariantCulture, @"
					UPDATE [{0}].[Finance].[BAS__AccReportingBook]
					SET IncludePresentationJournals = 'EET'", ScriptDbName);
				edwConnection.ExecuteNonQuery(sql);
				result = Execute(edwConnection, CompanyPK1, ReportingBookPK1, Array.Empty<Guid>(), Array.Empty<Guid>());
				AssertEquals(6, result.Rows.Count);
				AssertEquals(1, result.Select("TransactionCategory = 'EET'").Length);
				AssertEquals(5, result.Select("TransactionCategory = ''").Length);

				sql = string.Format(CultureInfo.InvariantCulture, @"
					UPDATE [{0}].[Finance].[BAS__AccReportingBook]
					SET IncludePresentationJournals = 'EEC'", ScriptDbName);
				edwConnection.ExecuteNonQuery(sql);
				result = Execute(edwConnection, CompanyPK1, ReportingBookPK1, Array.Empty<Guid>(), Array.Empty<Guid>());
				AssertEquals(5, result.Rows.Count);
				AssertEquals(0, result.Select("TransactionCategory = 'EET'").Length);
				AssertEquals(5, result.Select("TransactionCategory = ''").Length);

				sql = string.Format(CultureInfo.InvariantCulture, $@"
				DECLARE @registryRawValue nvarchar(MAX)
						Set @registryRawValue = '<?xml version=""1.0"" encoding=""utf-16""?><GLPresentationJournalCategoryCollection xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
							<GLPresentationJournalCategory>
								<CodeMaxLength>3</CodeMaxLength>
								<Code>EET</Code>
								<Description>Inter Company</Description>
								<Bool>Y</Bool>
								<Bool2>N</Bool2>
								<Bool3>Y</Bool3>
								<Bool4>Y</Bool4>
								<ParentCode>EEC</ParentCode>
							</GLPresentationJournalCategory>
						</GLPresentationJournalCategoryCollection>'

						INSERT INTO [{ScriptDbName}].Finance.BAS__AccountStmData
							(AccountStmDataID, AccountStmDataKey, BinaryValue, OwnerID, SDName)
						VALUES
							(NEWID(), 1, CONVERT(varbinary(MAX), CONVERT(XML, @registryRawValue)), null, 'GLJournalAdjustmentCategoriesList');
						UPDATE [{ScriptDbName}].Finance.BAS__AccReportingBook SET IncludeChildPresentation = 1
				");
				edwConnection.ExecuteNonQuery(sql);

				result = Execute(edwConnection, CompanyPK1, ReportingBookPK1, Array.Empty<Guid>(), Array.Empty<Guid>());
				AssertEquals(6, result.Rows.Count);
				AssertEquals(1, result.Select("TransactionCategory = 'EET'").Length);
				AssertEquals(5, result.Select("TransactionCategory = ''").Length);

				sql = string.Format(CultureInfo.InvariantCulture, $@"UPDATE [{ScriptDbName}].Finance.BAS__AccReportingBook SET IncludeChildPresentation = 0");
				edwConnection.ExecuteNonQuery(sql);
				result = Execute(edwConnection, CompanyPK1, ReportingBookPK1, Array.Empty<Guid>(), Array.Empty<Guid>());
				AssertEquals(5, result.Rows.Count);
				AssertEquals(0, result.Select("TransactionCategory = 'EET'").Length);
				AssertEquals(5, result.Select("TransactionCategory = ''").Length);
			}
		}

		#endregion

		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}

		void PrepareTestData(AdminConnection connection)
		{
			var testHelper = new AccountingFunctionTestingHelper(connection, ScriptDbName);
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				DELETE FROM [{0}].Organization.BAS__Branch;
				DELETE FROM [{0}].Organization.BAS__Department;
				DELETE FROM [{0}].Organization.BAS__Company;
			", ScriptDbName);
			connection.ExecuteNonQuery(sqlText);

			CompanyPK1 = testHelper.InsertCompany("CNY", "UUU");
			CompanyPK2 = testHelper.InsertCompany("USD", "DAU");

			sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].[Finance].[CUS__GeneralLedgerTransactionData]
				(GeneralLedgerTransactionDataKey, CompanyKey, BranchKey, Currency, DepartmentKey, DueDate, GLAccountKey, GLAccountName, GLAccountNum, GLAccountType, GLAmountLocalCredit, GLAmountLocalDebit, GLAmountLocalBalance, GLAmountOSCredit, GLAmountOSDebit, GLAmountOSBalance, GLType, TransactionCategory, GeneralLedgerDataKey, PostDate, PostPeriod, CompanyID,
				TransactionDate, TransactionNum, TransactionType, LineJobKey, LineDescription, HeaderDescription, LedgerCode, JobInvoiceNumber, ExchangeRate, ComplianceSubType, ComplianceNumber, ChequeOrReference, ChargeCodeKey, OrganizationKey, TransactionLineKey, TransactionLineID, TransactionHeaderID,
				JournalEntriesNumber, TaxBranchKey, TransactionHeaderKey)
				VALUES
				(1, 1, 1, 'CNY', 1, '2023-03-07', 1, 'GLAccount1', '10100009', 'ARC', 1, 0, -1, 3, 0, -3, 'PST', '', 1, '2023-03-07', 202304, '{1}', '2023-09-08', '121', 'REV', 1, 'lineDesc', 'HDesc', 'AP', 'Jiu01', 3.4, 'ert', '345', '456', 2, 3, 4, '7A5FF88B-76BE-41AE-A3EC-CEAC983125EF', 'AE6D15CE-63EC-41BF-905C-A95894A08095', '0000000001', 3, 1),
				(2, 1, 2, 'CNY', 1, '2023-06-07', 2, 'GLAccount2', '10100019', 'APC', 1, 0, -1, 3, 0, -3, 'PST', '', 2, '2023-06-07', 202305, '{1}', '2023-09-08', '122', 'REV', 1, 'lineDesc', 'HDesc', 'AP', 'Jiu02', 3.4, 'ert', '345', '456', 2, 3, 4, '7A5FF88B-76BE-41AE-A3EC-CEAC983125EF', 'AE6D15CE-63EC-41BF-905C-A95894A08095', '0000000002', 3, 2),
				(3, 1, 1, 'CNY', 1, '2023-08-07', 1, 'GLAccount1', '10100029', 'ARS', 0, 1,  1, 3, 0, -3, 'PST', '', 3, '2023-08-07', 202306, '{1}', '2023-09-08', '123', 'REV', 1, 'lineDesc', 'HDesc', 'AP', 'Jiu03', 3.4, 'ert', '345', '456', 2, 3, 4, 'D774E57E-5FC7-465C-BC24-D6F795550AC6', '2EAD1186-B082-40F0-81A4-4C7D4D5BC18C', '0000000003', null, 3),
				(4, 1, 1, 'CNY', 1, '2023-09-07', 1, 'GLAccount1', '10100029', 'APS', 1, 0, -1, 3, 0, -3, 'PST', '', 4, '2023-09-07', 202307, '{1}', '2023-09-08', '124', 'REV', 1, 'lineDesc', 'HDesc', 'AP', 'Jiu04', 3.4, 'ert', '345', '456', 2, 3, 4, 'D774E57E-5FC7-465C-BC24-D6F795550AC6', '2EAD1186-B082-40F0-81A4-4C7D4D5BC18C', '0000000004', null, 4),
				(5, 1, 1, 'CNY', 1, '2023-10-07', 1, 'GLAccount1', '10100029', 'RGT', 1, 0, -1, 3, 0, -3, 'REC', '', 5, '2023-10-07', 202308, '{1}', '2023-09-08', '126', 'REV', 1, 'lineDesc', 'HDesc', 'AP', 'Jiu09', 3.4, 'ert', '345', '456', 2, 3, 4, 'AE6D15CE-63EC-41BF-905C-A95894A08095', 'C77EF93B-DC28-40FE-A22A-AC355A295215', '0000000005', null, 5),
				(6, 1, 1, 'CNY', 1, '2023-11-07', 1, 'GLAccount1', '10100029', 'PGT', 0, 1,  1, 3, 0, -3, 'REC', '', 6, '2023-11-07', 202309, '{1}', '2023-09-08', '125', 'REV', 1, 'lineDesc', 'HDesc', 'AP', 'Jiu10', 3.4, 'ert', '345', '456', 2, 3, 4, 'AE6D15CE-63EC-41BF-905C-A95894A08095', 'C77EF93B-DC28-40FE-A22A-AC355A295215', '0000000006', null, 6),
				(7, 2, 3, 'CNY', 1, '2023-09-07', 1, 'GLAccount1', '10100029', 'PGO', 1, 0, -1, 3, 0, -3, 'REC', '', 7, '2023-09-07', 202310, '{2}', '2023-09-08', '127', 'REV', 1, 'lineDesc', 'HDesc', 'AP', 'Jiu19', 3.4, 'ert', '345', '456', 2, 3, 4, '7A5FF88B-76BE-41AE-A3EC-CEAC983125EF', 'AE6D15CE-63EC-41BF-905C-A95894A08095', '0000000007', null, 7),
				(8, 2, 3, 'CNY', 1, '2023-08-07', 1, 'GLAccount1', '10100029', 'PGI', 0, 1,  1, 3, 0, -3, 'REC', '', 8, '2023-08-07', 202311, '{2}', '2023-09-08', '128', 'REV', 1, 'lineDesc', 'HDesc', 'AP', 'Jiu29', 3.4, 'ert', '345', '456', 2, 3, 4, '7A5FF88B-76BE-41AE-A3EC-CEAC983125EF', 'AE6D15CE-63EC-41BF-905C-A95894A08095', '0000000008', null, 8)

				INSERT [{0}].[Finance].[CUS__GeneralLedgerTranslatedData]
				(GeneralLedgerTranslatedDataKey, GeneralLedgerDataKey, Attribute_OCG, Attribute_LFE, Attribute_LFO, Attribute_SPR, Attribute_TIC, Attribute_ORG, PostPeriodForReportingBook, OriginalAttribute_OCG, OriginalAttribute_LFE, OriginalAttribute_LFO, OriginalAttribute_SPR, OriginalAttribute_TIC, OriginalAttribute_ORG, AlternateGLAccount, AlternateGLAccountDescription, AlternateGLAccountKey, AlternateChartKey, Units, OriginalAccount, ReportingBookKey, CompanyOfPeriodKey, LocalCurrency, TranslatedCurrency, AccountTypeCode, CurrencyTranslationLevel, TranslatedAmount, IsReciprocal, TranslatedRateType, SubUnitRatio)
				VALUES
				(1, 1, 'INT', 'WEU', 'LOC', 'SPS', 'ETI', 'OR1', 202304, 'INT', 'WEU', 'LOC', 'SPS', 'ETI', 'OR1', '1231', 'Account1', 1, 1, 'KG', '165', 1, 1, 'CNY', 'USD', 'BSH', 'BAL',    2, 1, 'SEL', 100),
				(2, 2, 'TPY', 'OEU', 'FOR', 'NAV', ''   , 'OR2', 202305, 'TPY', 'OEU', 'FOR', 'NAV', 'STI', 'OR2', '1232', 'Account2', 2, 1, 'KG', '265', 1, 1, 'CNY', 'CNY', 'NTE', 'BAL',    2, 1, 'SEL', 100),
				(3, 3, 'INT', 'LOC', 'FOR', 'SPS', 'ETI', 'NAV', 202306, 'INT', 'LOC', 'FOR', 'SPS', 'ETI', 'NAV', '1233', 'Account3', 3, 1, 'KG', '365', 1, 1, 'CNY', 'CNY', 'P&L', 'BAL', null, 1, 'SEL', 100),
				(4, 4, 'INT', 'LOC', 'FOR', ''   , 'ETI', 'NAV', 100001, 'INT', 'LOC', 'FOR', 'SPS', 'ETI', 'NAV', '1234', 'Account4', 4, 1, 'KG', '465', 1, 1, 'CNY', 'USD', 'BSH', 'BAL',    2, 0, 'SEL', 100),
				(5, 5, 'INT', 'LOC', ''   , 'SPS', 'ETI', 'NAV', 202301, 'INT', 'LOC', 'FOR', 'SPS', 'ETI', 'NAV', '1235', 'Account5', 5, 1, 'KG', '565', 1, 1, 'CNY', 'USD', 'BSH', 'JNL',   -2, 1, 'SEL', 100),
				(6, 6, 'NAV', 'NAV', 'NAV', 'SPR', 'NAV', 'OR1', 202309, 'NAV', 'NAV', 'NAV', 'SPR', 'NAV', 'OR1', '1236', 'Account6', 6, 1, 'KG', '665', 1, 1, 'CNY', 'CNY', 'BSH', 'JNL', null, 1, 'SEL', 100),
				(7, 7, 'NAV', 'NAV', 'NAV', 'SPR', 'NAV', 'OR1', 202309, 'NAV', 'NAV', 'NAV', 'SPR', 'NAV', 'OR1', '1237', 'Account7', 7, 1, 'KG', '265', 2, 3, 'CNY', 'USD', 'NTE', 'JNL',    2, 1, 'SEL', 100),
				(8, 8, 'NAV', 'NAV', 'NAV', 'SPR', 'NAV', 'OR1', 202309, 'NAV', 'NAV', 'NAV', 'SPR', 'NAV', 'OR1', '1238', 'Account8', 8, 1, 'KG', '265', 2, 3, 'CNY', 'USD', 'BSH', 'BAL',   -2, 1, 'SEL', 100)

				INSERT [{0}].Finance.CUS__ExchangeRatesforReportingbook
				(ExchangeRatesforReportingbookKey, CompanyKey, StartDate, EndDate, ExRateType, Currency, SellRate)
				VALUES
				(1, 1, '2022-01-01', '2025-10-01', 'SEL', 'USD', 2),
				(2, 2, '2022-01-01', '2025-10-01', 'BUY', 'USD', 5)
				",
				ScriptDbName, CompanyPK1, CompanyPK2);
			connection.ExecuteNonQuery(sqlText);
			testHelper.CreateBASBranch(1, Guid.NewGuid(), 1, "UUU");
			testHelper.CreateBASBranch(2, Guid.NewGuid(), 1, "UUY");
			testHelper.CreateBASBranch(3, Guid.NewGuid(), 2, "UUO");
			testHelper.CreateBASDepartment(1, Guid.NewGuid(), "YYY");
			testHelper.CreateBASDepartment(2, Guid.NewGuid(), "YYU");

			ReportingBookPK1 = testHelper.InsertReportingBook(1, 1, "RUR");
			ReportingBookPK2 = testHelper.InsertReportingBook(2, 2, "RUY");
		}

		DataTable Execute(AdminConnection edwConnection, Guid companyPK, Guid reportingBookPK, Guid[] transactionHeaderPKList, Guid[] transactionLinePKList, string startDate = "2023-01-01", string endDate = "2024-12-20", string branchList = "", string departmentList = "")
		{
			var dbCommand = edwConnection.Command($"USE {ScriptDbName} SELECT * FROM [{ScriptDbName}].[dbo].GetGeneralLedgerTransactionData('{companyPK}', '{reportingBookPK}', '{startDate}', '{endDate}', '{branchList}', '{departmentList}', @TransactionHeaderPKList, @TransactionLinePKList)");
			dbCommand.AddTableValuedParameter("@TransactionHeaderPKList", "dbo.TVP_uniqueidentifier", transactionHeaderPKList);
			dbCommand.AddTableValuedParameter("@TransactionLinePKList", "dbo.TVP_uniqueidentifier", transactionLinePKList);
			return DataUtils.GetDataTableFromCommand(dbCommand);
		}

		Guid CompanyPK1, CompanyPK2, ReportingBookPK1, ReportingBookPK2;
	}
}
