using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting
{
	[TestedType(typeof(GLTransactionsSP))]
	class GLTransactionsSPTest : DbCreateScriptTest
	{
		//This is testing Enterprise.Accounting.Business.Testing.ScriptTests.GLTransactionSPTest

		public void TestOpenBalanceForPNLAccountWithDifferentCompanies()
		{
			var helper = new TestDbHelper(TestConnection);
			var postDate = new DateTime(2020, 03, 16);

			var glAccountPK1 = helper.InsertGLAccount("3333.22.22", "TestGLAccount 1", "P&L");
			var plAppropriationAccountPK = new Guid(TestConnection.ExecuteScalar(@"SELECT CONVERT(UNIQUEIDENTIFIER, CONVERT(NVARCHAR(4000), CONVERT(VARBINARY(8000), SD_BinaryValue)))
			FROM dbo.stmdata WHERE sd_name = 'GL_PL_APPROPRIATION_ACCOUNT'").ToString());

			var company2PK = helper.InsertCompany("ABC", "ABC Compay", "AUD", "AU", true, true);
			var branch2PK = helper.InsertBranch("BR2", company2PK, "Branch 2");
			var departmentPK = helper.InsertDepartment("DP2", "Department 2");

			helper.InsertAccPeriod(2019, 05, TestDbHelper.DefaultCompanyPK);
			helper.InsertAccPeriod(2019, 06, TestDbHelper.DefaultCompanyPK);

			helper.InsertAccPeriod(2019, 07, company2PK, new DateTime(2020, 01, 01));
			helper.InsertAccPeriod(2019, 08, company2PK, new DateTime(2020, 02, 01));
			helper.InsertAccPeriod(2019, 09, company2PK, new DateTime(2020, 03, 01));
			helper.InsertAccPeriod(2019, 10, company2PK, new DateTime(2020, 04, 01));

			helper.InsertGLAggregate(66, "", 201907, plAppropriationAccountPK, branch2PK, departmentPK, company2PK);
			helper.InsertGLAggregate(100, "", 201905, glAccountPK1, branch2PK, departmentPK, company2PK);
			helper.InsertGLAggregate(200, "", 201906, glAccountPK1, branch2PK, departmentPK, company2PK);
			helper.InsertGLAggregate(300, "", 201907, glAccountPK1, branch2PK, departmentPK, company2PK);
			helper.InsertGLAggregate(400, "", 201908, glAccountPK1, branch2PK, departmentPK, company2PK);
			helper.InsertGLAggregate(500, "", 201909, glAccountPK1, branch2PK, departmentPK, company2PK);
			helper.InsertGLAggregate(600, "", 201910, glAccountPK1, branch2PK, departmentPK, company2PK);

			var headerPK = helper.InsertTransactionHeader("GL", "GJL", "X0001", 160, postDate, branch2PK, departmentPK);
			helper.InsertTransactionLine(headerPK, null, null, glAccountPK1, branch2PK, departmentPK, null, 150, "CST", postDate, null, 10, 1, "A");

			var sqlText = $@"EXEC GLTransactionsSP
				@CompanyPK = '{company2PK}',
				@StartPeriod = NULL,
				@EndPeriod = NULL,
				@StartDate = '2020-03-01',
				@EndDate = '2020-03-31',
				@StartGLAccountPK = NULL,
				@EndGLAccountPK = NULL,
				@BranchPK = NULL,
				@DepartmentPK = NULL,
				@DisplayDescription = 'L',
				@TransactionCategory = '',
				@BatchNumberToGet = 0,
				@BatchNumberToSet = 0,
				@IncludeZeroBalance = 'N',
				@IsExportingBatch = 'Y',
				@HighWaterMark = NULL,
				@StartLocalGLAccountPK = NULL,
				@EndLocalGLAccountPK = NULL,
				@SubAccountType = NULL,
				@SubAccountPK = NULL,
				@NoSubAccount = NULL";

			var result = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
			AssertEquals("Should be 2 rows in report", 2, result.Rows.Count);
			AssertEquals("The balance of Account '3333.22.22' should be 700m", 700m, result.Select("GLAccount ='3333.22.22'").FirstOrDefault().Field<decimal>("OpeningBalance"));
			AssertEquals("The balance of Account '4900.00.00' should be 366m", 366m, result.Select("GLAccount ='4900.00.00'").FirstOrDefault().Field<decimal>("OpeningBalance"));
		}

		public void TestSubAccountType()
		{
			#region prepare data for test
			TestConnection.ExecuteNonQuery(@"INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate,AM_EndDate,AM_IsSubLedgerClosed,AM_IsGeneralLedgerClosed, AM_GC_Company) values (NEWID(), 201504,2015,'04/01/2005','04/30/2015 23:59',1,1,'878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')");

			// 907B7A0F-3700-4C4C-B313-971C8B0AE6FC  -  3510.00.00
			TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.AccGLHeader (AG_PK, AG_AccountNum, AG_AccountType, AG_DebitCredit) VALUES
('62DCAF16-098C-4301-9657-94838A8C411E', '111.11.91', 'P&L', 'DR'),
('A5B7D939-A368-4FE3-BB44-183B6D4A65A3', '111.11.92', 'P&L', 'DR'),
('4A1B02E9-1CBE-4824-AFDA-6AB5A8EC7FE8', '111.11.93', 'P&L', 'DR'),
('7F62F97F-A54F-417F-BF51-D06782377190', '111.11.94', 'P&L', 'DR');

INSERT INTO dbo.AccGLHeaderSubAccount (ASA_PK, ASA_AG, ASA_IsSubClassValidationRuleMandatory, ASA_SubClass) VALUES
(NEWID(), '62DCAF16-098C-4301-9657-94838A8C411E', 0 , 'OH'),
(NEWID(), 'A5B7D939-A368-4FE3-BB44-183B6D4A65A3', 0 , 'AR'),
(NEWID(), '4A1B02E9-1CBE-4824-AFDA-6AB5A8EC7FE8', 0 , 'GS'),
(NEWID(), '7F62F97F-A54F-417F-BF51-D06782377190', 0 , 'GG');

INSERT INTO dbo.OrgHeader (OH_PK, OH_Code) VALUES ('BAB5B9B1-69B1-4380-A355-218A35936ABC', 'TESTORG');

INSERT INTO dbo.AccGroups (AR_PK, AR_Code, AR_Desc) VALUES ('F4E069B2-3A3E-4AC4-B1F1-D6187B54DC1B','TEST','TEST');

INSERT INTO dbo.GlbStaff (GS_PK, GS_Code, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) VALUES
('D83C49D8-2A2B-4B1D-9B3A-AEFF69548368', 'TS1', GETUTCDATE(), 'E', GETUTCDATE(), 'E');

INSERT INTO dbo.GlbGroup (GG_PK, GG_Code) VALUES('5E501B8A-F9BD-4960-A2C9-4D4B2FC8F873', 'TS2');");

			TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_TransactionCount, AH_Desc, AH_InvoiceDate, AH_DueDate, AH_InvoiceAmount, AH_OSTotal, AH_RX_NKTransactionCurrency, AH_ExchangeRate, AH_PostDate, AH_TransactionCategory, AH_OutstandingAmount, AH_PostToGL, AH_InvoiceTerm,AH_NumberOfSupportingDocuments, AH_OH, AH_OA_InvoiceAddressOverride,AH_GB, AH_GC, AH_GE)
VALUES('591E6B52-95DF-42F8-8972-53FFF59C123F', 'AR', 'INV', '00001000', 1, 'AR INVOICE', '2015-04-24 14:04:00', '2015-04-24 14:04:00', 6000, 6000, 'AUD', 1, '2015-04-24 14:04:00', 'FIN', 6000, 'Y', 'COD', 1, '0DAAB61B-255E-4AD7-AFC5-4E7B03C3BDA1', '37A65812-36DB-4D6C-9AB7-71EE9BF599F2', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491')");
			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.AccTransactionHeader where AH_PK = '591E6B52-95DF-42F8-8972-53FFF59C123F'");
			AssertEquals("Result should have row(s)", 1, result.Rows.Count);

			// new line '3E50F5C5-49B2-45D3-AA4C-099FAE6833AD'
			TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.AccTransactionLines (AL_PK,AL_LineType,AL_Sequence,AL_Desc,AL_LineAmount, AL_GSTVATBasis, AL_OSAmount,AL_RX_NKTransactionCurrency,AL_ExchangeRate,AL_InputGSTVATRecoverable, AL_PostDate, AL_PostToGL, AL_ReverseDate,AL_ReverseToGL, AL_RevRecognitionType, AL_GE,AL_GB,AL_GC,AL_AG, AL_AH) 
VALUES('3E50F5C5-49B2-45D3-AA4C-099FAE6833AD', 'REV', 1, 'RENTAL', 1000, 'A', 1000, 'AUD', 1.000000000, 1, '2015-04-24 14:04:00', 'N', '2015-04-24 14:04:00', 'Y', 'IMM', '86BB1C22-0865-4685-996E-D56CBD136491', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '907B7A0F-3700-4C4C-B313-971C8B0AE6FC', '591E6B52-95DF-42F8-8972-53FFF59C123F')");
			result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.AccTransactionLines where AL_AH = '591E6B52-95DF-42F8-8972-53FFF59C123F'");
			AssertEquals("Result should have row(s)", 1, result.Rows.Count);

			TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.AccTransactionLines (AL_PK,AL_LineType,AL_Sequence,AL_Desc,AL_LineAmount, AL_GSTVATBasis, AL_OSAmount,AL_RX_NKTransactionCurrency,AL_ExchangeRate,AL_InputGSTVATRecoverable, AL_PostDate, AL_PostToGL, AL_ReverseDate,AL_ReverseToGL, AL_RevRecognitionType, AL_GE,AL_GB,AL_GC,AL_AG, AL_AH) 
VALUES('3E50F5C5-49B2-45D3-AA4C-099FAE6833BD', 'REV', 1, 'RENTAL', 2000, 'A', 2000, 'AUD', 1.000000000, 1, '2015-04-24 14:04:00', 'N', '2015-04-24 14:04:00', 'Y', 'IMM', '86BB1C22-0865-4685-996E-D56CBD136491', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '907B7A0F-3700-4C4C-B313-971C8B0AE6FC', '591E6B52-95DF-42F8-8972-53FFF59C123F')");
			result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.AccTransactionLines where AL_AH = '591E6B52-95DF-42F8-8972-53FFF59C123F'");
			AssertEquals("Result should have row(s)", 2, result.Rows.Count);

			TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.AccTransactionLines (AL_PK,AL_LineType,AL_Sequence,AL_Desc,AL_LineAmount, AL_GSTVATBasis, AL_OSAmount,AL_RX_NKTransactionCurrency,AL_ExchangeRate,AL_InputGSTVATRecoverable, AL_PostDate, AL_PostToGL, AL_ReverseDate,AL_ReverseToGL, AL_RevRecognitionType, AL_GE,AL_GB,AL_GC,AL_AG, AL_AH) 
VALUES('3E50F5C5-49B2-45D3-AA4C-099FAE6833CD', 'REV', 1, 'RENTAL', 3000, 'A', 3000, 'AUD', 1.000000000, 1, '2015-04-24 14:04:00', 'N', '2015-04-24 14:04:00', 'Y', 'IMM', '86BB1C22-0865-4685-996E-D56CBD136491', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '907B7A0F-3700-4C4C-B313-971C8B0AE6FC', '591E6B52-95DF-42F8-8972-53FFF59C123F')");
			result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.AccTransactionLines where AL_AH = '591E6B52-95DF-42F8-8972-53FFF59C123F'");
			AssertEquals("Result should have row(s)", 3, result.Rows.Count);

			TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.AccTransactionLines (AL_PK,AL_LineType,AL_Sequence,AL_Desc,AL_LineAmount, AL_GSTVATBasis, AL_OSAmount,AL_RX_NKTransactionCurrency,AL_ExchangeRate,AL_InputGSTVATRecoverable, AL_PostDate, AL_PostToGL, AL_ReverseDate,AL_ReverseToGL, AL_RevRecognitionType, AL_GE,AL_GB,AL_GC,AL_AG, AL_AH) 
VALUES('F500FB9E-C7DE-4424-9C6F-B13F7C72DF4F', 'REV', 1, 'RENTAL', 3000, 'A', 3000, 'AUD', 1.000000000, 1, '2015-04-24 14:04:00', 'N', '2015-04-24 14:04:00', 'Y', 'IMM', '86BB1C22-0865-4685-996E-D56CBD136491', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '62DCAF16-098C-4301-9657-94838A8C411E', '591E6B52-95DF-42F8-8972-53FFF59C123F')");
			result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.AccTransactionLines where AL_AH = '591E6B52-95DF-42F8-8972-53FFF59C123F'");
			AssertEquals("Result should have row(s)", 4, result.Rows.Count);

			TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.AccTransactionLines (AL_PK,AL_LineType,AL_Sequence,AL_Desc,AL_LineAmount, AL_GSTVATBasis, AL_OSAmount,AL_RX_NKTransactionCurrency,AL_ExchangeRate,AL_InputGSTVATRecoverable, AL_PostDate, AL_PostToGL, AL_ReverseDate,AL_ReverseToGL, AL_RevRecognitionType, AL_GE,AL_GB,AL_GC,AL_AG, AL_AH) 
VALUES('85FDA62E-91D3-4859-93F7-E3F5D58796FF', 'REV', 1, 'RENTAL', 3000, 'A', 3000, 'AUD', 1.000000000, 1, '2015-04-24 14:04:00', 'N', '2015-04-24 14:04:00', 'Y', 'IMM', '86BB1C22-0865-4685-996E-D56CBD136491', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', 'A5B7D939-A368-4FE3-BB44-183B6D4A65A3', '591E6B52-95DF-42F8-8972-53FFF59C123F')");
			result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.AccTransactionLines where AL_AH = '591E6B52-95DF-42F8-8972-53FFF59C123F'");
			AssertEquals("Result should have row(s)", 5, result.Rows.Count);

			TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.AccTransactionLines (AL_PK,AL_LineType,AL_Sequence,AL_Desc,AL_LineAmount, AL_GSTVATBasis, AL_OSAmount,AL_RX_NKTransactionCurrency,AL_ExchangeRate,AL_InputGSTVATRecoverable, AL_PostDate, AL_PostToGL, AL_ReverseDate,AL_ReverseToGL, AL_RevRecognitionType, AL_GE,AL_GB,AL_GC,AL_AG, AL_AH) 
VALUES('7341631F-2E33-40DF-8EAA-7DFA436C30DA', 'REV', 1, 'RENTAL', 3000, 'A', 3000, 'AUD', 1.000000000, 1, '2015-04-24 14:04:00', 'N', '2015-04-24 14:04:00', 'Y', 'IMM', '86BB1C22-0865-4685-996E-D56CBD136491', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '4A1B02E9-1CBE-4824-AFDA-6AB5A8EC7FE8', '591E6B52-95DF-42F8-8972-53FFF59C123F')");
			result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.AccTransactionLines where AL_AH = '591E6B52-95DF-42F8-8972-53FFF59C123F'");
			AssertEquals("Result should have row(s)", 6, result.Rows.Count);

			TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.AccTransactionLines (AL_PK,AL_LineType,AL_Sequence,AL_Desc,AL_LineAmount, AL_GSTVATBasis, AL_OSAmount,AL_RX_NKTransactionCurrency,AL_ExchangeRate,AL_InputGSTVATRecoverable, AL_PostDate, AL_PostToGL, AL_ReverseDate,AL_ReverseToGL, AL_RevRecognitionType, AL_GE,AL_GB,AL_GC,AL_AG, AL_AH) 
VALUES('3ED91949-129E-480E-8AB1-BA53E3C4E739', 'REV', 1, 'RENTAL', 3000, 'A', 3000, 'AUD', 1.000000000, 1, '2015-04-24 14:04:00', 'N', '2015-04-24 14:04:00', 'Y', 'IMM', '86BB1C22-0865-4685-996E-D56CBD136491', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '7F62F97F-A54F-417F-BF51-D06782377190', '591E6B52-95DF-42F8-8972-53FFF59C123F')");
			result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.AccTransactionLines where AL_AH = '591E6B52-95DF-42F8-8972-53FFF59C123F'");
			AssertEquals("Result should have row(s)", 7, result.Rows.Count);

			TestConnection.ExecuteNonQuery($@"
INSERT INTO dbo.AccTransactionLineSubAccount (AL1_PK,AL1_AL,AL1_SubClassParentTableCode,AL1_SubClassParentId) VALUES
(NEWID(), '3E50F5C5-49B2-45D3-AA4C-099FAE6833BD', 'OH', '24181EEA-D3E5-4AFE-8892-8684AB555879'),
(NEWID(), '3E50F5C5-49B2-45D3-AA4C-099FAE6833CD', 'OH', 'E8BD88D2-A5C1-43FE-A788-A53ADBB86403'),
(NEWID(), 'F500FB9E-C7DE-4424-9C6F-B13F7C72DF4F', 'OH', 'BAB5B9B1-69B1-4380-A355-218A35936ABC'),
(NEWID(), '85FDA62E-91D3-4859-93F7-E3F5D58796FF', 'AR', 'F4E069B2-3A3E-4AC4-B1F1-D6187B54DC1B'),
(NEWID(), '7341631F-2E33-40DF-8EAA-7DFA436C30DA', 'GS', 'D83C49D8-2A2B-4B1D-9B3A-AEFF69548368'),
(NEWID(), '3ED91949-129E-480E-8AB1-BA53E3C4E739', 'GG', '5E501B8A-F9BD-4960-A2C9-4D4B2FC8F873')
");

			#endregion prepare data for test

			#region sql template
			var template = @"EXEC GLTransactionsSP
		@CompanyPK = '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC',
		@StartPeriod = NULL,
		@EndPeriod = NULL,
		@StartDate = '2015-04-01',
		@EndDate = '2015-04-30',
		@StartGLAccountPK = '62DCAF16-098C-4301-9657-94838A8C411E',
		@EndGLAccountPK = '907B7A0F-3700-4C4C-B313-971C8B0AE6FC',
		@BranchPK = NULL,
		@DepartmentPK = NULL,
		@DisplayDescription = NULL,
		@TransactionCategory = NULL,
		@BatchNumberToGet = NULL,
		@BatchNumberToSet = NULL,
		@IncludeZeroBalance = 'n',
		@IsExportingBatch = NULL,
		@HighWaterMark = NULL,
		@StartLocalGLAccountPK = NULL,
		@EndLocalGLAccountPK = NULL,
		@SubAccountType = {0},
		@SubAccountPK = {1},
		@NoSubAccount = {2}";
			#endregion sql template

			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format(template, "NULL", "NULL", "NULL"));
			AssertEquals("Should be 3 rows in report", 7, result.Rows.Count);

			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format(template, "'ORG'", "NULL", "'N'"));
			AssertEquals("Should be 3 rows in report", 3, result.Rows.Count);

			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format(template, "'ORG'", "'E8BD88D2-A5C1-43FE-A788-A53ADBB86403'", "'N'"));
			AssertEquals("Should be 1 row in report", 1, result.Rows.Count);
			AssertEquals("3E50F5C5-49B2-45D3-AA4C-099FAE6833CD", result.Rows[0]["PK"].ToString().ToUpper());

			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format(template, "'ORG'", "'BAB5B9B1-69B1-4380-A355-218A35936ABC'", "'N'"));
			AssertEquals("Should be 1 row in report", 1, result.Rows.Count);
			AssertEquals("F500FB9E-C7DE-4424-9C6F-B13F7C72DF4F", result.Rows[0]["PK"].ToString().ToUpper());

			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format(template, "'SEG'", "'F4E069B2-3A3E-4AC4-B1F1-D6187B54DC1B'", "'N'"));
			AssertEquals("Should be 1 row in report", 1, result.Rows.Count);
			AssertEquals("85FDA62E-91D3-4859-93F7-E3F5D58796FF", result.Rows[0]["PK"].ToString().ToUpper());

			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format(template, "'STR'", "'D83C49D8-2A2B-4B1D-9B3A-AEFF69548368'", "'N'"));
			AssertEquals("Should be 1 row in report", 1, result.Rows.Count);
			AssertEquals("7341631F-2E33-40DF-8EAA-7DFA436C30DA", result.Rows[0]["PK"].ToString().ToUpper());

			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format(template, "'SGP'", "'5E501B8A-F9BD-4960-A2C9-4D4B2FC8F873'", "'N'"));
			AssertEquals("Should be 1 row in report", 1, result.Rows.Count);
			AssertEquals("3ED91949-129E-480E-8AB1-BA53E3C4E739", result.Rows[0]["PK"].ToString().ToUpper());

			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format(template, "'ORG'", "'E8BD88D2-A5C1-43FE-A788-A53ADBB86403'", "'Y'"));
			AssertEquals("Should be 0 row in report", 0, result.Rows.Count);
		}

		#region TestCMTLinesGRVRecordForARAPINVCRDADJ

		public void TestCMTLinesGRVRecordForARInvoice()
		{
			CreateDataForCMTGRVLineTest("AR", "INV", "AR INVOICE", "FIN", "REV", 1000m);
			RunScriptAndAssertResult(true);
			RunScriptAndAssertResult(false);
		}

		public void TestCMTLinesGRVRecordForARCreditNote()
		{
			CreateDataForCMTGRVLineTest("AR", "CRD", "AR CREDIT NOTE", "FIN", "REV", -1000m);
			RunScriptAndAssertResult(true);
			RunScriptAndAssertResult(false);
		}

		public void TestCMTLinesGRVRecordForARAdjustmentNote()
		{
			CreateDataForCMTGRVLineTest("AR", "ADJ", "AR ADJUSTMENT NOTE", "FIN", "REV", 1000m);
			RunScriptAndAssertResult(true);
			RunScriptAndAssertResult(false);
		}

		public void TestCMTLinesGRVRecordForAPInvoice()
		{
			CreateDataForCMTGRVLineTest("AP", "INV", "AP INVOICE", "STD", "CST", -1000m);
			RunScriptAndAssertResult(true);
			RunScriptAndAssertResult(false);
		}

		public void TestCMTLinesGRVRecordForAPCreditNote()
		{
			CreateDataForCMTGRVLineTest("AP", "CRD", "AP CREDIT NOTE", "STD", "CST", 1000m);
			RunScriptAndAssertResult(true);
			RunScriptAndAssertResult(false);
		}

		public void TestCMTLinesGRVRecordForAPAdjustmentNote()
		{
			CreateDataForCMTGRVLineTest("AP", "ADJ", "AP ADJUSTMENT NOTE", "STD", "CST", -1000m);
			RunScriptAndAssertResult(true);
			RunScriptAndAssertResult(false);
		}

		public void TestCMTLinesGRVRecordForAPJournal()
		{
			CreateDataForCMTGRVLineTest("AP", "ADJ", "AP ADJUSTMENT NOTE", "STD", "CST", -1000m);
			RunScriptAndAssertResult(true);
			RunScriptAndAssertResult(false);
		}

		void RunScriptAndAssertResult(bool shouldSetBatchNumber)
		{
			var sqlText = string.Format(@"EXEC GLTransactionsSP
		@CompanyPK = '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC',
		@StartPeriod = NULL,
		@EndPeriod = NULL,
		@StartDate = '2015-04-01',
		@EndDate = '2015-04-30',
		@StartGLAccountPK = '907B7A0F-3700-4C4C-B313-971C8B0AE6FC',
		@EndGLAccountPK = 'F3E08B3D-4B2D-414D-951E-8ECD5E85B29B',
		@BranchPK = NULL,
		@DepartmentPK = NULL,
		@DisplayDescription = NULL,
		@TransactionCategory = NULL,
		@BatchNumberToGet = NULL,
		@BatchNumberToSet = {0},
		@IncludeZeroBalance = 'n',
		@IsExportingBatch = NULL,
		@HighWaterMark = NULL,
		@StartLocalGLAccountPK = NULL,
		@EndLocalGLAccountPK = NULL,
		@SubAccountType = NULL,
		@SubAccountPK = NULL,
		@NoSubAccount = NULL", shouldSetBatchNumber ? "1001" : "NULL");

			var result = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
			if (shouldSetBatchNumber)
			{
				AssertEquals("Precondition: 1 NON CMT GPS Line", 1, result.Rows.Cast<DataRow>().Count(x => x["ChargeCode"].ToString() != "COMMENT" && x["BatchType"].ToString() == "GPS"));
				AssertEquals("Precondition: 1 NON CMT GRV Line", 1, result.Rows.Cast<DataRow>().Count(x => x["ChargeCode"].ToString() != "COMMENT" && x["BatchType"].ToString() == "GRV"));
				AssertEquals("Precondition: 1 CMT GPS Line", 1, result.Rows.Cast<DataRow>().Count(x => x["ChargeCode"].ToString() == "COMMENT" && x["BatchType"].ToString() == "GPS"));
				AssertEquals("1 CMT GRV Line", 1, result.Rows.Cast<DataRow>().Count(x => x["ChargeCode"].ToString() == "COMMENT" && x["BatchType"].ToString() == "GRV"));
			}
			else
			{
				AssertEquals("Precondition: 1 NON CMT GPS Line", 1, result.Rows.Cast<DataRow>().Count(x => x["ChargeCode"].ToString() != "COMMENT" && x["BatchType"].ToString() == "GPS"));
				AssertEquals("Precondition: 1 NON CMT GRV Line", 1, result.Rows.Cast<DataRow>().Count(x => x["ChargeCode"].ToString() != "COMMENT" && x["BatchType"].ToString() == "GRV"));
				AssertEquals("Precondition: 1 CMT GPS Line", 1, result.Rows.Cast<DataRow>().Count(x => x["ChargeCode"].ToString() == "COMMENT" && x["BatchType"].ToString() == "GPS"));
				AssertEquals("should not generate any record for CMT GRV Line when no batch number", 0, result.Rows.Cast<DataRow>().Count(x => x["ChargeCode"].ToString() == "COMMENT" && x["BatchType"].ToString() == "GRV"));
			}
		}

		void CreateDataForCMTGRVLineTest(string ledger, string transactionType, string description, string transactionCategory, string lineType, decimal amount)
		{
			TestConnection.ExecuteNonQuery(@"INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate,AM_EndDate,AM_IsSubLedgerClosed,AM_IsGeneralLedgerClosed, AM_GC_Company, AM_SystemCreateTimeUtc, AM_SystemCreateUser, AM_SystemLastEditTimeUtc, AM_SystemLastEditUser) values (NEWID(), 201504,2015,'04/01/2005','04/30/2015 23:59',1,1,'878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
			TestConnection.ExecuteNonQuery(@"INSERT INTO dbo.AccChargeCode (AC_PK, AC_Code, AC_ChargeGroup, AC_ChargeType, AC_SystemCreateTimeUtc, AC_SystemCreateUser, AC_SystemLastEditTimeUtc, AC_SystemLastEditUser) VALUES ('09eeb087-be7e-4f31-a85d-d2bff05c641e', 'COMMENT', 'FRT', 'CMT', GetUtcDate(), '~BP', GetUtcDate(), '~BP')");

			if (ledger == "AP")
			{
				TestConnection.ExecuteNonQuery("INSERT INTO dbo.StmData (SD_PK, SD_Name, SD_Owner, SD_DepartmentGuid, SD_Type, SD_BinaryValue, SD_GuidValue) ( SELECT NEWID(), 'GL_AP_SUSPENSE_CONTROL_ACCOUNT', NULL, NULL, 'GID', SD_BinaryValue, SD_GuidValue From dbo.StmData Where SD_Name = 'GL_AR_CONTROL_ACCOUNT')");
			}
			else if (ledger == "AR")
			{
				TestConnection.ExecuteNonQuery("INSERT INTO dbo.StmData (SD_PK, SD_Name, SD_Owner, SD_DepartmentGuid, SD_Type, SD_BinaryValue, SD_GuidValue) ( SELECT NEWID(), 'GL_AR_SUSPENSE_CONTROL_ACCOUNT', NULL, NULL, 'GID', SD_BinaryValue, SD_GuidValue From dbo.StmData Where SD_Name = 'GL_AR_CONTROL_ACCOUNT')");
			}

			var command = @"INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_TransactionCount, AH_Desc, AH_InvoiceDate, AH_DueDate, AH_InvoiceAmount, AH_OSTotal, AH_RX_NKTransactionCurrency, AH_ExchangeRate, AH_PostDate, AH_TransactionCategory, AH_OutstandingAmount, AH_PostToGL, AH_InvoiceTerm,AH_NumberOfSupportingDocuments, AH_OH, AH_OA_InvoiceAddressOverride,AH_GB, AH_GC, AH_GE, AH_SystemCreateTimeUtc, AH_SystemCreateUser, AH_SystemLastEditTimeUtc, AH_SystemLastEditUser)
VALUES('591E6B52-95DF-42F8-8972-53FFF59C123F', '{0}', '{1}', '00001000', 1, '{4}', '2015-04-24 14:04:00', '2015-04-24 14:04:00', {3}, {3}, 'AUD', 1, '2015-04-24 14:04:00', '{2}', {3}, 'Y', 'COD', 1, '0DAAB61B-255E-4AD7-AFC5-4E7B03C3BDA1', '37A65812-36DB-4D6C-9AB7-71EE9BF599F2', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', GetUtcDate(), '~BP', GetUtcDate(), '~BP')";
			TestConnection.ExecuteNonQuery(string.Format(command, ledger, transactionType, transactionCategory, amount.ToString(), description));
			var result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.AccTransactionHeader where AH_PK = '591E6B52-95DF-42F8-8972-53FFF59C123F'");
			AssertEquals("Result should have row(s)", 1, result.Rows.Count);

			command = @"INSERT INTO dbo.AccTransactionLines (AL_PK,AL_LineType,AL_Sequence,AL_Desc,AL_LineAmount, AL_GSTVATBasis, AL_OSAmount,AL_RX_NKTransactionCurrency,AL_ExchangeRate,AL_InputGSTVATRecoverable, AL_PostDate, AL_PostToGL, AL_ReverseDate,AL_ReverseToGL, AL_RevRecognitionType, AL_GE,AL_GB,AL_GC,AL_AC, AL_AH, AL_SystemCreateTimeUtc, AL_SystemCreateUser, AL_SystemLastEditTimeUtc, AL_SystemLastEditUser) 
			VALUES('3E50F5C5-49B2-45D3-AA4C-099FAE6833AD', '{0}', 1, 'RENTAL', 0, 'A', 0, 'AUD', 1.000000000, 1, '2015-04-24 14:04:00', 'N', '2015-04-24 14:04:00', 'Y', 'IMM', '86BB1C22-0865-4685-996E-D56CBD136491', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '09eeb087-be7e-4f31-a85d-d2bff05c641e', '591E6B52-95DF-42F8-8972-53FFF59C123F', GetUtcDate(), '~BP', GetUtcDate(), '~BP')";
			TestConnection.ExecuteNonQuery(string.Format(command, lineType));
			result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.AccTransactionLines where AL_AH = '591E6B52-95DF-42F8-8972-53FFF59C123F'");
			AssertEquals("Result should have row(s)", 1, result.Rows.Count);

			command = @"INSERT INTO dbo.AccTransactionLines (AL_PK,AL_LineType,AL_Sequence,AL_Desc,AL_LineAmount, AL_GSTVATBasis, AL_OSAmount,AL_RX_NKTransactionCurrency,AL_ExchangeRate,AL_InputGSTVATRecoverable, AL_PostDate, AL_PostToGL, AL_ReverseDate,AL_ReverseToGL, AL_RevRecognitionType, AL_GE,AL_GB,AL_GC,AL_AG, AL_AH, AL_SystemCreateTimeUtc, AL_SystemCreateUser, AL_SystemLastEditTimeUtc, AL_SystemLastEditUser) 
VALUES('3E50F5C5-49B2-45D3-AA4C-099FAE6833BD', '{0}', 1, 'RENTAL', {1}, 'A', {1}, 'AUD', 1.000000000, 1, '2015-04-24 14:04:00', 'N', '2015-04-24 14:04:00', 'Y', 'IMM', '86BB1C22-0865-4685-996E-D56CBD136491', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '907B7A0F-3700-4C4C-B313-971C8B0AE6FC', '591E6B52-95DF-42F8-8972-53FFF59C123F', GetUtcDate(), '~BP', GetUtcDate(), '~BP')";
			TestConnection.ExecuteNonQuery(string.Format(command, lineType, amount.ToString()));
			result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.AccTransactionLines where AL_AH = '591E6B52-95DF-42F8-8972-53FFF59C123F'");
			AssertEquals("Result should have row(s)", 2, result.Rows.Count);
		}

		#endregion

		public void TestTransactionDescPaymentReferenceNumberMaxLength()
		{
			SetUpDataForTestTransactionDescPaymentReferenceNumberMaxLength(TestConnection);

			var template = @"EXEC GLTransactionsSP
		@CompanyPK = '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC',
		@StartPeriod = NULL,
		@EndPeriod = NULL,
		@StartDate = '2015-04-01',
		@EndDate = '2015-04-30',
		@StartGLAccountPK = '907B7A0F-3700-4C4C-B313-971C8B0AE6FC',
		@EndGLAccountPK = '907B7A0F-3700-4C4C-B313-971C8B0AE6FC',
		@BranchPK = NULL,
		@DepartmentPK = NULL,
		@DisplayDescription = 'L',
		@TransactionCategory = NULL,
		@BatchNumberToGet = NULL,
		@BatchNumberToSet = NULL,
		@IncludeZeroBalance = 'n',
		@IsExportingBatch = 'Y',
		@HighWaterMark = NULL,
		@StartLocalGLAccountPK = NULL,
		@EndLocalGLAccountPK = NULL,
		@SubAccountType = {0},
		@SubAccountPK = {1},
		@NoSubAccount = {2}";

			var result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format(template, "NULL", "NULL", "NULL"));
			AssertEquals("Should be 1 rows in report", 1, result.Rows.Count);

			var transactionDesc = result.Rows[0]["TransactionDesc"].ToString();
			AssertEquals(StringLen1024, transactionDesc);

			var paymentReferenceNumber = result.Rows[0]["PaymentReferenceNumber"].ToString();
			AssertEquals(StringLen35, paymentReferenceNumber);
		}

		[ExpectNoExceptions()]
		public void TestTransactionSecondRefCanHoldMaxLengthForAH_ConsolidatedInvoiceRef()
		{
			string sqlFormat = @"EXEC GLTransactionsSP
              @CompanyPK = '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC',
              @StartPeriod = NULL,
              @EndPeriod = NULL,
              @StartDate = '2015-04-01',
              @EndDate = '2015-04-30',
              @StartGLAccountPK = '907B7A0F-3700-4C4C-B313-971C8B0AE6FC',
              @EndGLAccountPK = '907B7A0F-3700-4C4C-B313-971C8B0AE6FC',
              @BranchPK = NULL,
              @DepartmentPK = NULL,
              @DisplayDescription = 'L',
              @TransactionCategory = NULL,
              @BatchNumberToGet = NULL,
              @BatchNumberToSet = NULL,
              @IncludeZeroBalance = 'n',
              @IsExportingBatch = 'Y',
              @HighWaterMark = NULL,
              @StartLocalGLAccountPK = NULL,
              @EndLocalGLAccountPK = NULL,
              @SubAccountType = {0},
              @SubAccountPK = {1},
              @NoSubAccount = {2}";

			string consolidatedInvoiceRef = new string('X', AccTransactionHeaderSchema.AH_ConsolidatedInvoiceRef.MaxLength);
			SetUpDataForTestTransactionSecondRefMaxLength(TestConnection, consolidatedInvoiceRef);
			var result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format(sqlFormat, "NULL", "NULL", "NULL"));
			AssertEquals("Should be 1 rows in report", 1, result.Rows.Count);
			string sref = result.Rows[0]["SecondRef"].ToString();
			AssertEquals(consolidatedInvoiceRef, sref);
		}

		public void TestTransactionExRatePrecision()
		{
			#region prepare data

			TestConnection.ExecuteNonQuery(@"INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate,AM_EndDate,AM_IsSubLedgerClosed,AM_IsGeneralLedgerClosed, AM_GC_Company) values (NEWID(), 201504,2015,'04/01/2005','04/30/2015 23:59',1,1,'878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')");

			TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_TransactionCount, AH_AG, AH_Desc, AH_InvoiceDate, AH_DueDate, AH_InvoiceAmount, AH_OSTotal, AH_RX_NKTransactionCurrency, AH_ExchangeRate, AH_PostDate, AH_TransactionCategory, AH_OutstandingAmount, AH_PostToGL, AH_InvoiceTerm,AH_NumberOfSupportingDocuments, AH_OH, AH_OA_InvoiceAddressOverride,AH_GB, AH_GC, AH_GE)
VALUES('591E6B52-95DF-42F8-8972-53FFF59C123F', 'AR', 'JNL', '00001000', 1, '907B7A0F-3700-4C4C-B313-971C8B0AE6FC', 'AR Journal', '2015-04-24 14:04:00', '2015-04-24 14:04:00', 25487.28, 30058.95, 'AUD', 0.847910, '2015-04-24 14:04:00', 'FIN', 25487.28, 'Y', 'COD', 1, '0DAAB61B-255E-4AD7-AFC5-4E7B03C3BDA1', '37A65812-36DB-4D6C-9AB7-71EE9BF599F2', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491')");
			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.AccTransactionHeader where AH_PK = '591E6B52-95DF-42F8-8972-53FFF59C123F'");
			AssertEquals("Result should have 1 row", 1, result.Rows.Count);

			#endregion prepare data

			#region sql template

			var template = @"EXEC GLTransactionsSP
					@CompanyPK = '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC',
					@StartPeriod = NULL,
					@EndPeriod = NULL,
					@StartDate = '2015-04-01',
					@EndDate = '2015-04-30',
					@StartGLAccountPK = '907B7A0F-3700-4C4C-B313-971C8B0AE6FC',
					@EndGLAccountPK = '907B7A0F-3700-4C4C-B313-971C8B0AE6FC',
					@BranchPK = NULL,
					@DepartmentPK = NULL,
					@DisplayDescription = NULL,
					@TransactionCategory = NULL,
					@BatchNumberToGet = NULL,
					@BatchNumberToSet = NULL,
					@IncludeZeroBalance = 'n',
					@IsExportingBatch = NULL,
					@HighWaterMark = NULL,
					@StartLocalGLAccountPK = NULL,
					@EndLocalGLAccountPK = NULL,
					@SubAccountType = NULL,
					@SubAccountPK = NULL,
					@NoSubAccount = NULL";

			#endregion sql template
			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format(template, "NULL", "NULL", "NULL"));
			AssertEquals("Should be 1 row in report", 1, result.Rows.Count);
			AssertEquals("Shouldn't lose precision", "0.847910000", result.Rows[0]["ExRate"].ToString());
		}

		public void TestContainsNonclusteredIndexIX_GLAccount()
		{
			var sql = $"SELECT OBJECT_DEFINITION (OBJECT_ID('{new GLTransactionsSP().Name}'))";

			using (var cmd = TestConnection.Command(sql))
			{
				var result = cmd.ExecuteScalar();
				Assert(result.ToString().Contains("CREATE NONCLUSTERED INDEX IX_GLAccount ON #TRANSACTIONS (GLAccount)"));
			}
		}

		public void TestTransactionCategoryGroup()
		{
			var helper = new TestDbHelper(TestConnection);
			var postDate = new DateTime(2020, 03, 16);
			int categoryGroupCount = 26;

			helper.InsertAccPeriod(2020, 3, TestDbHelper.DefaultCompanyPK);

			var sqlText = $@"EXEC GLTransactionsSP
				@CompanyPK = '{TestDbHelper.DefaultCompanyPK}',
				@StartPeriod = NULL,
				@EndPeriod = NULL,
				@StartDate = '2020-03-01',
				@EndDate = '2020-03-31',
				@StartGLAccountPK = NULL,
				@EndGLAccountPK = NULL,
				@BranchPK = NULL,
				@DepartmentPK = NULL,
				@DisplayDescription = 'L',
				@TransactionCategory = '{"{0}"}',
				@BatchNumberToGet = 1,
				@BatchNumberToSet = 0,
				@IncludeZeroBalance = 'N',
				@IsExportingBatch = 'Y',
				@HighWaterMark = NULL,
				@StartLocalGLAccountPK = NULL,
				@EndLocalGLAccountPK = NULL,
				@SubAccountType = NULL,
				@SubAccountPK = NULL,
				@NoSubAccount = NULL";

			#region transaction yype is 'GJL'

			var categoryGroups = TestHelper.CreateTransactionHeaderByTransactionType("A", categoryGroupCount, "GJL", "ATN", InsertTransactionHeader);

			var result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format(sqlText, "A00"));
			AssertEquals("Should be 1 rows in report", 1, result.Rows.Count);
			AssertEquals("ATN00", result.Rows[0]["TransactionNum"].ToString());

			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format(sqlText, categoryGroups));
			AssertEquals("Should be 25 rows in report", 25, result.Rows.Count);
			result.DefaultView.Sort = "TransactionNum";
			var resultForAssert = result.DefaultView.ToTable();
			for (int index = 0; index < categoryGroupCount - 1; index++)
			{
				var transactionNum = $"ATN{index.ToString().PadLeft(2, '0')}";
				AssertEquals($"transaction num should be {transactionNum}", transactionNum, resultForAssert.Rows[index]["TransactionNum"].ToString());
			}

			#endregion

			#region transaction yype is 'AJL'

			categoryGroups = TestHelper.CreateTransactionHeaderByTransactionType("B", categoryGroupCount, "AJL", "BTN", InsertTransactionHeader);

			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format(sqlText, "B00"));
			AssertEquals("Should be 1 rows in report", 1, result.Rows.Count);
			AssertEquals("BTN00", result.Rows[0]["TransactionNum"].ToString());

			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format(sqlText, categoryGroups));
			AssertEquals("Should be 25 rows in report", 25, result.Rows.Count);
			result.DefaultView.Sort = "TransactionNum";
			resultForAssert = result.DefaultView.ToTable();
			for (int index = 0; index < categoryGroupCount - 1; index++)
			{
				var transactionNum = $"BTN{index.ToString().PadLeft(2, '0')}";
				AssertEquals($"transaction num should be {transactionNum}", transactionNum, resultForAssert.Rows[index]["TransactionNum"].ToString());
			}

			#endregion

			#region transaction yype is 'RJL'

			categoryGroups = TestHelper.CreateTransactionHeaderByTransactionType("C", categoryGroupCount, "RJL", "CTN", InsertTransactionHeader);

			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format(sqlText, "C00"));
			AssertEquals("Should be 1 rows in report", 2, result.Rows.Count);
			AssertEquals("Transaction Num is 'CTN00'", 2, result.Select("TransactionNum='CTN00'").Length);

			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format(sqlText, categoryGroups));
			AssertEquals("Should be 50 rows in report", 50, result.Rows.Count);
			for (int index = 0; index < categoryGroupCount - 1; index++)
			{
				var transactionNum = $"CTN{index.ToString().PadLeft(2, '0')}";
				AssertEquals($"transaction num is '{transactionNum}'", 2, result.Select($"TransactionNum='{transactionNum}'").Length);
			}

			#endregion

			void InsertTransactionHeader(string transactionType, string transactionNum, string category)
			{
				var headerPK = helper.InsertTransactionHeader("GL", transactionType, transactionNum, 110, postDate, helper.DefaultBranchPK, helper.DefaultDepartmentPK, category: category);
				var linePK = helper.InsertTransactionLine(headerPK, null, null, helper.GLAccountPK1, helper.DefaultBranchPK, helper.DefaultDepartmentPK, null, 100, "CST", postDate, null, 10, 1, "A");
				helper.InsertGenExportBatchSequence("GPS", 1, "AL", linePK);
			}
		}

		public void TestContainUnits()
		{
			var helper = new TestDbHelper(TestConnection);
			var postDate = new DateTime(2020, 03, 16);
			int categoryGroupCount = 26;

			helper.InsertAccPeriod(2020, 3, TestDbHelper.DefaultCompanyPK);

			var sqlText = $@"EXEC GLTransactionsSP
				@CompanyPK = '{TestDbHelper.DefaultCompanyPK}',
				@StartPeriod = NULL,
				@EndPeriod = NULL,
				@StartDate = '2020-03-01',
				@EndDate = '2020-03-31',
				@StartGLAccountPK = NULL,
				@EndGLAccountPK = NULL,
				@BranchPK = NULL,
				@DepartmentPK = NULL,
				@DisplayDescription = 'L',
				@TransactionCategory = '{"{0}"}',
				@BatchNumberToGet = 1,
				@BatchNumberToSet = 0,
				@IncludeZeroBalance = 'N',
				@IsExportingBatch = 'Y',
				@HighWaterMark = NULL,
				@StartLocalGLAccountPK = NULL,
				@EndLocalGLAccountPK = NULL,
				@SubAccountType = NULL,
				@SubAccountPK = NULL,
				@NoSubAccount = NULL";

			var categoryGroups = TestHelper.CreateTransactionHeaderByTransactionType("A", categoryGroupCount, "NJL", "ATN", InsertTransactionHeader);

			var result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format(sqlText, categoryGroups));
			AssertEquals("Should be 25 rows in report", 25, result.Rows.Count);
			result.DefaultView.Sort = "TransactionNum";
			var resultForAssert = result.DefaultView.ToTable();
			for (int index = 0; index < categoryGroupCount - 1; index++)
			{
				AssertEquals($"transaction num should be KWH", "KWH", resultForAssert.Rows[index]["Units"].ToString());
			}

			void InsertTransactionHeader(string transactionType, string transactionNum, string category)
			{
				var headerPK = helper.InsertTransactionHeader("GL", transactionType, transactionNum, 110, postDate, helper.DefaultBranchPK, helper.DefaultDepartmentPK, category: category);
				var linePK = helper.InsertTransactionLine(headerPK, null, null, helper.GLNoteAccountPK1, helper.DefaultBranchPK, helper.DefaultDepartmentPK, null, 100, "CST", postDate, null, 10, 1, "A");
				helper.InsertGenExportBatchSequence("GPS", 1, "AL", linePK);
			}
		}

		public void TestNotGroupByExchangeRateForBankControlAccountOfCashBookDRCAndDPY()
		{
			var helper = new TestDbHelper(TestConnection);
			var postDate = new DateTime(2020, 03, 16);

			var bankControlAccountPK = helper.InsertGLAccount("2222.20.00", "Bank control account", "BSH");
			var bankAccountPK = helper.InsertBankAccount("CBB", bankControlAccountPK);
			helper.InsertAccPeriod(2020, 3, TestDbHelper.DefaultCompanyPK);

			var sqlText = $@"EXEC GLTransactionsSP
				@CompanyPK = '{TestDbHelper.DefaultCompanyPK}',
				@StartPeriod = NULL,
				@EndPeriod = NULL,
				@StartDate = '2020-03-01',
				@EndDate = '2020-03-31',
				@StartGLAccountPK = NULL,
				@EndGLAccountPK = NULL,
				@BranchPK = NULL,
				@DepartmentPK = NULL,
				@DisplayDescription = 'L',
				@TransactionCategory = '',
				@BatchNumberToGet = 1,
				@BatchNumberToSet = 0,
				@IncludeZeroBalance = 'N',
				@IsExportingBatch = 'Y',
				@HighWaterMark = NULL,
				@StartLocalGLAccountPK = NULL,
				@EndLocalGLAccountPK = NULL,
				@SubAccountType = NULL,
				@SubAccountPK = NULL,
				@NoSubAccount = NULL";

			var headerPK = helper.InsertTransactionHeader("CB", "DRC", "ATN", 110, postDate, helper.DefaultBranchPK, helper.DefaultDepartmentPK, bankAccountPK, exchangeRate: 2.3m);
			var linePK = helper.InsertTransactionLine(headerPK, null, null, helper.GLAccountPK1, helper.DefaultBranchPK, helper.DefaultDepartmentPK, null, 100, "CST", postDate, null, 10, 1, "A", exchangeRate: 1.236m);
			helper.InsertGenExportBatchSequence("GPS", 1, "AL", linePK);
			linePK = helper.InsertTransactionLine(headerPK, null, null, helper.GLAccountPK1, helper.DefaultBranchPK, helper.DefaultDepartmentPK, null, 200, "CST", postDate, null, 10, 1, "A", exchangeRate: 1.235558m);
			helper.InsertGenExportBatchSequence("GPS", 1, "AL", linePK);

			var result = DataUtils.GetDataTableFromQuery(TestConnection, sqlText).Select("GLAccount = '2222.20.00'");
			AssertEquals("Should be 1 rows for bank control account of the report", 1, result.Length);
		}

		public void TestWhetherAddNewColumnsForResultOfGLTransactionsSP()
		{
			var whiteList = new List<string>
			{
				"PK",
				"TransactionType",
				"InvoiceDate",
				"PostDate",
				"DueDate",
				"Branch",
				"Department",
				"Ledger",
				"TransactionNum",
				"SecondRef",
				"TransactionDesc",
				"GLAccount",
				"GLAccountDesc",
				"Units",
				"Job",
				"Account",
				"ChargeCode",
				"ChargeCodeDescription",
				"LocalLanguageChargeCodeDescription",
				"Period",
				"ReversePeriod",
				"Amount",
				"GSTAmount",
				"Debit",
				"Credit",
				"Balance",
				"IsLine",
				"TRPK",
				"OpeningPeriodDate",
				"ClosingPeriodDate",
				"OpeningBalance",
				"ClosingBalance",
				"IsControlTotal",
				"IsRevenueRecognition",
				"BatchNumber",
				"BatchType",
				"SubSelectID",
				"ParentTableCode",
				"PKFORBATCHING",
				"OsAmount",
				"ExRate",
				"Currency",
				"OsDebit",
				"OsCredit",
				"OsExTaxDebit",
				"OsExTaxCredit",
				"MultiSubAccountTypeCode",
				"OrganisationSubAccount",
				"SalesExpenseGroupsSubAccount",
				"StaffAndResourcesSubAccount",
				"StaffGroupSubAccount",
				"BankCode",
				"TaxID",
				"TaxIDType",
				"TaxIDRate",
				"TransactionDebtorOrCreditorGroup",
				"JobLocalClient",
				"JobLocalClientDebtorGroup",
				"TransactionDebtorExternalCreditRatingCode",
				"OriginalTransactionNumberForReversals",
				"OriginalTransactionNumberConsolidatedInvoiceRef",
				"TransactionNumberConsolidatedInvoiceRef",
				"BatchNumberForExport",
				"BatchSequenceNumber",
				"TransactionHeaderBranch",
				"TransactionHeaderDepartment",
				"TransactionLineBranchOrgProxyCode",
				"TransactionHeaderBranchOrgProxyCode",
				"WasImportedFromExternalSystem",
				"PaymentReferenceNumber",
				"ComplianceSubType",
				"ComplianceNumber",
				"JournalEntriesNumber"
			};
			var msgToHint = $@"If you see this error, you must be adding a new column into #Transactions table in store procedure 'GLTransactionsSP'.
Please follow below steps: 

1> Read through below parts in store procedure 'GLTransactionsSP' and understand their functional purpose.
Part1: Line 827 'SetDirectReceiptPaymentBank()';
Part2: Line 4313 'Merge rows For Direct Receipt/Payment Bank Account'.

2> If the evaluation of new column for Part1 happens before Part2, please add the new column into Part2 'select' and 'group by' sections.

3 > Add the new column into whiteList.
";

			var sqlText = $@"EXEC GLTransactionsSP
				@CompanyPK = '{TestDbHelper.DefaultCompanyPK}',
				@StartPeriod = NULL,
				@EndPeriod = NULL,
				@StartDate = '2020-03-01',
				@EndDate = '2020-03-31',
				@StartGLAccountPK = NULL,
				@EndGLAccountPK = NULL,
				@BranchPK = NULL,
				@DepartmentPK = NULL,
				@DisplayDescription = 'L',
				@TransactionCategory = '',
				@BatchNumberToGet = 1,
				@BatchNumberToSet = 0,
				@IncludeZeroBalance = 'N',
				@IsExportingBatch = 'Y',
				@HighWaterMark = NULL,
				@StartLocalGLAccountPK = NULL,
				@EndLocalGLAccountPK = NULL,
				@SubAccountType = NULL,
				@SubAccountPK = NULL,
				@NoSubAccount = NULL";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
			AssertEquals(msgToHint, 73, result.Columns.Count);
			foreach (DataColumn column in result.Columns)
			{
				Assert($@"{msgToHint}

Should contain column '{column.ColumnName}'", whiteList.Contains(column.ColumnName));
			}
		}

		public void TestOpeningBalanceWithoutNoteAccount()
		{
			var helper = new TestDbHelper(TestConnection);
			var postDate = new DateTime(2020, 03, 16);

			var sqlText = $@"EXEC GLTransactionsSP
				@CompanyPK = '{TestDbHelper.DefaultCompanyPK}',
				@StartPeriod = NULL,
				@EndPeriod = NULL,
				@StartDate = '2020-03-01',
				@EndDate = '2020-03-31',
				@StartGLAccountPK = NULL,
				@EndGLAccountPK = NULL,
				@BranchPK = NULL,
				@DepartmentPK = NULL,
				@DisplayDescription = 'L',
				@TransactionCategory = '',
				@BatchNumberToGet = 0,
				@BatchNumberToSet = 0,
				@IncludeZeroBalance = 'N',
				@IsExportingBatch = 'Y',
				@HighWaterMark = NULL,
				@StartLocalGLAccountPK = NULL,
				@EndLocalGLAccountPK = NULL,
				@SubAccountType = NULL,
				@SubAccountPK = NULL,
				@NoSubAccount = NULL";

			var glAccountPK1 = helper.InsertGLAccount("111.222.22", "TestGLAccount 1", "P&L");
			var plAppropriationAccountPK = new Guid(TestConnection.ExecuteScalar(@"SELECT CONVERT(UNIQUEIDENTIFIER, CONVERT(NVARCHAR(4000), CONVERT(VARBINARY(8000), SD_BinaryValue)))
			FROM dbo.stmdata WHERE sd_name = 'GL_PL_APPROPRIATION_ACCOUNT'").ToString());

			helper.InsertAccPeriod(2019, 06, TestDbHelper.DefaultCompanyPK);
			helper.InsertAccPeriod(2020, 01, TestDbHelper.DefaultCompanyPK);
			helper.InsertAccPeriod(2020, 02, TestDbHelper.DefaultCompanyPK);
			helper.InsertAccPeriod(2020, 03, TestDbHelper.DefaultCompanyPK);
			helper.InsertAccPeriod(2020, 05, TestDbHelper.DefaultCompanyPK);

			helper.InsertGLAggregate(66, "", 202002, plAppropriationAccountPK, helper.DefaultBranchPK, helper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);
			helper.InsertGLAggregate(100, "", 201906, glAccountPK1, helper.DefaultBranchPK, helper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);
			helper.InsertGLAggregate(200, "", 202001, glAccountPK1, helper.DefaultBranchPK, helper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);
			helper.InsertGLAggregate(300, "", 202002, glAccountPK1, helper.DefaultBranchPK, helper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);
			helper.InsertGLAggregate(333, "", 202003, glAccountPK1, helper.DefaultBranchPK, helper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);
			helper.InsertGLAggregate(400, "", 202005, glAccountPK1, helper.DefaultBranchPK, helper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);

			var headerPK = helper.InsertTransactionHeader("GL", "GJL", "X0001", 160, postDate, helper.DefaultBranchPK, helper.DefaultDepartmentPK);
			helper.InsertTransactionLine(headerPK, null, null, glAccountPK1, helper.DefaultBranchPK, helper.DefaultDepartmentPK, null, 150, "CST", postDate, null, 10, 1, "A");

			var result = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
			AssertEquals("Should be 2 rows in report", 2, result.Rows.Count);
			AssertEquals("The balance of Account '4900.00.00' should be 166m", 166m, result.Select("GLAccount ='4900.00.00'").FirstOrDefault().Field<decimal>("OpeningBalance"));
			AssertEquals("The balance of Account '111.222.22' should be 500m", 500m, result.Select("GLAccount ='111.222.22'").FirstOrDefault().Field<decimal>("OpeningBalance"));
		}

		public void TestOpeningBalanceWithNoteAccountAndPNLAccount()
		{
			var helper = new TestDbHelper(TestConnection);
			var postDate = new DateTime(2020, 03, 16);

			var sqlText = $@"EXEC GLTransactionsSP
				@CompanyPK = '{TestDbHelper.DefaultCompanyPK}',
				@StartPeriod = NULL,
				@EndPeriod = NULL,
				@StartDate = '2020-03-01',
				@EndDate = '2020-03-31',
				@StartGLAccountPK = NULL,
				@EndGLAccountPK = NULL,
				@BranchPK = NULL,
				@DepartmentPK = NULL,
				@DisplayDescription = 'L',
				@TransactionCategory = '',
				@BatchNumberToGet = 0,
				@BatchNumberToSet = 0,
				@IncludeZeroBalance = 'N',
				@IsExportingBatch = 'Y',
				@HighWaterMark = NULL,
				@StartLocalGLAccountPK = NULL,
				@EndLocalGLAccountPK = NULL,
				@SubAccountType = NULL,
				@SubAccountPK = NULL,
				@NoSubAccount = NULL";

			var glAccountPK1 = helper.InsertGLAccount("3333.22.22", "TestGLAccount 1", "P&L");
			var glAccountPK2 = helper.InsertGLAccount("2222.22.22", "TestGLAccount 2", "NTE");
			var plAppropriationAccountPK = new Guid(TestConnection.ExecuteScalar(@"SELECT CONVERT(UNIQUEIDENTIFIER, CONVERT(NVARCHAR(4000), CONVERT(VARBINARY(8000), SD_BinaryValue)))
			FROM dbo.stmdata WHERE sd_name = 'GL_PL_APPROPRIATION_ACCOUNT'").ToString());

			helper.InsertAccPeriod(2019, 06, TestDbHelper.DefaultCompanyPK);
			helper.InsertAccPeriod(2020, 01, TestDbHelper.DefaultCompanyPK);
			helper.InsertAccPeriod(2020, 02, TestDbHelper.DefaultCompanyPK);
			helper.InsertAccPeriod(2020, 03, TestDbHelper.DefaultCompanyPK);
			helper.InsertAccPeriod(2020, 05, TestDbHelper.DefaultCompanyPK);

			helper.InsertGLAggregate(66, "", 202002, plAppropriationAccountPK, helper.DefaultBranchPK, helper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);
			helper.InsertGLAggregate(100, "", 201906, glAccountPK1, helper.DefaultBranchPK, helper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);
			helper.InsertGLAggregate(200, "", 202001, glAccountPK1, helper.DefaultBranchPK, helper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);
			helper.InsertGLAggregate(300, "", 202002, glAccountPK1, helper.DefaultBranchPK, helper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);
			helper.InsertGLAggregate(333, "", 202003, glAccountPK1, helper.DefaultBranchPK, helper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);
			helper.InsertGLAggregate(350, "", 202005, glAccountPK1, helper.DefaultBranchPK, helper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);
			helper.InsertGLAggregate(400, "", 201906, glAccountPK2, helper.DefaultBranchPK, helper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);
			helper.InsertGLAggregate(500, "", 202001, glAccountPK2, helper.DefaultBranchPK, helper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);
			helper.InsertGLAggregate(600, "", 202002, glAccountPK2, helper.DefaultBranchPK, helper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);
			helper.InsertGLAggregate(633, "", 202003, glAccountPK2, helper.DefaultBranchPK, helper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);
			helper.InsertGLAggregate(650, "", 202005, glAccountPK2, helper.DefaultBranchPK, helper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);

			var headerPK = helper.InsertTransactionHeader("GL", "GJL", "X0001", 160, postDate, helper.DefaultBranchPK, helper.DefaultDepartmentPK);
			helper.InsertTransactionLine(headerPK, null, null, glAccountPK1, helper.DefaultBranchPK, helper.DefaultDepartmentPK, null, 150, "CST", postDate, null, 10, 1, "A");
			var headerPK1 = helper.InsertTransactionHeader("GL", "NJL", "X0002", 260, postDate, helper.DefaultBranchPK, helper.DefaultDepartmentPK);
			helper.InsertTransactionLine(headerPK1, null, null, glAccountPK2, helper.DefaultBranchPK, helper.DefaultDepartmentPK, null, 250, "CST", postDate, null, 10, 1, "A");

			var result = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
			AssertEquals("Should be 3 rows in report", 3, result.Rows.Count);
			AssertEquals("The balance of Account '2222.22.22' should be 1100m", 1100m, result.Select("GLAccount ='2222.22.22'").FirstOrDefault().Field<decimal>("OpeningBalance"));
			AssertEquals("The balance of Account '3333.22.22' should be 500m", 500m, result.Select("GLAccount ='3333.22.22'").FirstOrDefault().Field<decimal>("OpeningBalance"));
			AssertEquals("The balance of Account '4900.00.00' should be 166m", 166m, result.Select("GLAccount ='4900.00.00'").FirstOrDefault().Field<decimal>("OpeningBalance"));
		}

		public void TestOpeningBalanceWithNoteAccountAndBSHAccount()
		{
			var helper = new TestDbHelper(TestConnection);
			var postDate = new DateTime(2020, 03, 16);

			var sqlText = $@"EXEC GLTransactionsSP
				@CompanyPK = '{TestDbHelper.DefaultCompanyPK}',
				@StartPeriod = NULL,
				@EndPeriod = NULL,
				@StartDate = '2020-03-01',
				@EndDate = '2020-03-31',
				@StartGLAccountPK = NULL,
				@EndGLAccountPK = NULL,
				@BranchPK = NULL,
				@DepartmentPK = NULL,
				@DisplayDescription = 'L',
				@TransactionCategory = '',
				@BatchNumberToGet = 0,
				@BatchNumberToSet = 0,
				@IncludeZeroBalance = 'N',
				@IsExportingBatch = 'Y',
				@HighWaterMark = NULL,
				@StartLocalGLAccountPK = NULL,
				@EndLocalGLAccountPK = NULL,
				@SubAccountType = NULL,
				@SubAccountPK = NULL,
				@NoSubAccount = NULL";

			var glAccountPK1 = helper.InsertGLAccount("3333.22.22", "TestGLAccount 1", "BSH");
			var glAccountPK2 = helper.InsertGLAccount("2222.22.22", "TestGLAccount 2", "NTE");

			helper.InsertAccPeriod(2019, 06, TestDbHelper.DefaultCompanyPK);
			helper.InsertAccPeriod(2020, 01, TestDbHelper.DefaultCompanyPK);
			helper.InsertAccPeriod(2020, 02, TestDbHelper.DefaultCompanyPK);
			helper.InsertAccPeriod(2020, 03, TestDbHelper.DefaultCompanyPK);
			helper.InsertAccPeriod(2020, 05, TestDbHelper.DefaultCompanyPK);

			helper.InsertGLAggregate(100, "", 201906, glAccountPK1, helper.DefaultBranchPK, helper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);
			helper.InsertGLAggregate(200, "", 202001, glAccountPK1, helper.DefaultBranchPK, helper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);
			helper.InsertGLAggregate(300, "", 202002, glAccountPK1, helper.DefaultBranchPK, helper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);
			helper.InsertGLAggregate(333, "", 202003, glAccountPK1, helper.DefaultBranchPK, helper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);
			helper.InsertGLAggregate(350, "", 202005, glAccountPK1, helper.DefaultBranchPK, helper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);
			helper.InsertGLAggregate(400, "", 201906, glAccountPK2, helper.DefaultBranchPK, helper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);
			helper.InsertGLAggregate(500, "", 202001, glAccountPK2, helper.DefaultBranchPK, helper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);
			helper.InsertGLAggregate(600, "", 202002, glAccountPK2, helper.DefaultBranchPK, helper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);
			helper.InsertGLAggregate(633, "", 202003, glAccountPK2, helper.DefaultBranchPK, helper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);
			helper.InsertGLAggregate(650, "", 202005, glAccountPK2, helper.DefaultBranchPK, helper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);

			var headerPK = helper.InsertTransactionHeader("GL", "GJL", "X0001", 160, postDate, helper.DefaultBranchPK, helper.DefaultDepartmentPK);
			helper.InsertTransactionLine(headerPK, null, null, glAccountPK1, helper.DefaultBranchPK, helper.DefaultDepartmentPK, null, 150, "CST", postDate, null, 10, 1, "A");
			var headerPK1 = helper.InsertTransactionHeader("GL", "NJL", "X0002", 260, postDate, helper.DefaultBranchPK, helper.DefaultDepartmentPK);
			helper.InsertTransactionLine(headerPK1, null, null, glAccountPK2, helper.DefaultBranchPK, helper.DefaultDepartmentPK, null, 250, "CST", postDate, null, 10, 1, "A");

			var result = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
			AssertEquals("Should be 2 rows in report", 2, result.Rows.Count);
			AssertEquals("The balance of Account '2222.22.22' should be 1100m", 1100m, result.Select("GLAccount ='2222.22.22'").FirstOrDefault().Field<decimal>("OpeningBalance"));
			AssertEquals("The balance of Account '3333.22.22' should be 600m", 600m, result.Select("GLAccount ='3333.22.22'").FirstOrDefault().Field<decimal>("OpeningBalance"));
		}

		public void TestOpeningBalanceWithNoteAccountAndPNLAccountAndBSHAccount()
		{
			var helper = new TestDbHelper(TestConnection);
			var postDate = new DateTime(2020, 03, 16);

			var sqlText = $@"EXEC GLTransactionsSP
				@CompanyPK = '{TestDbHelper.DefaultCompanyPK}',
				@StartPeriod = NULL,
				@EndPeriod = NULL,
				@StartDate = '2020-03-01',
				@EndDate = '2020-03-31',
				@StartGLAccountPK = NULL,
				@EndGLAccountPK = NULL,
				@BranchPK = NULL,
				@DepartmentPK = NULL,
				@DisplayDescription = 'L',
				@TransactionCategory = '',
				@BatchNumberToGet = 0,
				@BatchNumberToSet = 0,
				@IncludeZeroBalance = 'N',
				@IsExportingBatch = 'Y',
				@HighWaterMark = NULL,
				@StartLocalGLAccountPK = NULL,
				@EndLocalGLAccountPK = NULL,
				@SubAccountType = NULL,
				@SubAccountPK = NULL,
				@NoSubAccount = NULL";

			var glAccountPK1 = helper.InsertGLAccount("3333.22.22", "TestGLAccount 1", "BSH");
			var glAccountPK2 = helper.InsertGLAccount("2222.22.22", "TestGLAccount 2", "NTE");
			var glAccountPK3 = helper.InsertGLAccount("1111.22.22", "TestGLAccount 3", "P&L");
			var plAppropriationAccountPK = new Guid(TestConnection.ExecuteScalar(@"SELECT CONVERT(UNIQUEIDENTIFIER, CONVERT(NVARCHAR(4000), CONVERT(VARBINARY(8000), SD_BinaryValue)))
			FROM dbo.stmdata WHERE sd_name = 'GL_PL_APPROPRIATION_ACCOUNT'").ToString());

			helper.InsertAccPeriod(2019, 06, TestDbHelper.DefaultCompanyPK);
			helper.InsertAccPeriod(2020, 01, TestDbHelper.DefaultCompanyPK);
			helper.InsertAccPeriod(2020, 02, TestDbHelper.DefaultCompanyPK);
			helper.InsertAccPeriod(2020, 03, TestDbHelper.DefaultCompanyPK);
			helper.InsertAccPeriod(2020, 05, TestDbHelper.DefaultCompanyPK);

			helper.InsertGLAggregate(66, "", 202002, plAppropriationAccountPK, helper.DefaultBranchPK, helper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);
			helper.InsertGLAggregate(100, "", 201906, glAccountPK1, helper.DefaultBranchPK, helper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);
			helper.InsertGLAggregate(200, "", 202001, glAccountPK1, helper.DefaultBranchPK, helper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);
			helper.InsertGLAggregate(300, "", 202002, glAccountPK1, helper.DefaultBranchPK, helper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);
			helper.InsertGLAggregate(333, "", 202003, glAccountPK1, helper.DefaultBranchPK, helper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);
			helper.InsertGLAggregate(350, "", 202005, glAccountPK1, helper.DefaultBranchPK, helper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);
			helper.InsertGLAggregate(400, "", 201906, glAccountPK2, helper.DefaultBranchPK, helper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);
			helper.InsertGLAggregate(500, "", 202001, glAccountPK2, helper.DefaultBranchPK, helper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);
			helper.InsertGLAggregate(600, "", 202002, glAccountPK2, helper.DefaultBranchPK, helper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);
			helper.InsertGLAggregate(633, "", 202003, glAccountPK2, helper.DefaultBranchPK, helper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);
			helper.InsertGLAggregate(650, "", 202005, glAccountPK2, helper.DefaultBranchPK, helper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);
			helper.InsertGLAggregate(700, "", 201906, glAccountPK3, helper.DefaultBranchPK, helper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);
			helper.InsertGLAggregate(800, "", 202001, glAccountPK3, helper.DefaultBranchPK, helper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);
			helper.InsertGLAggregate(900, "", 202002, glAccountPK3, helper.DefaultBranchPK, helper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);
			helper.InsertGLAggregate(933, "", 202003, glAccountPK3, helper.DefaultBranchPK, helper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);
			helper.InsertGLAggregate(950, "", 202005, glAccountPK3, helper.DefaultBranchPK, helper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);

			var headerPK = helper.InsertTransactionHeader("GL", "GJL", "X0001", 160, postDate, helper.DefaultBranchPK, helper.DefaultDepartmentPK);
			helper.InsertTransactionLine(headerPK, null, null, glAccountPK1, helper.DefaultBranchPK, helper.DefaultDepartmentPK, null, 150, "CST", postDate, null, 10, 1, "A");
			var headerPK1 = helper.InsertTransactionHeader("GL", "NJL", "X0002", 260, postDate, helper.DefaultBranchPK, helper.DefaultDepartmentPK);
			helper.InsertTransactionLine(headerPK1, null, null, glAccountPK2, helper.DefaultBranchPK, helper.DefaultDepartmentPK, null, 250, "CST", postDate, null, 10, 1, "A");
			var headerPK2 = helper.InsertTransactionHeader("GL", "GJL", "X0003", 360, postDate, helper.DefaultBranchPK, helper.DefaultDepartmentPK);
			helper.InsertTransactionLine(headerPK2, null, null, glAccountPK3, helper.DefaultBranchPK, helper.DefaultDepartmentPK, null, 350, "CST", postDate, null, 10, 1, "A");

			var result = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
			AssertEquals("Should be 4 rows in report", 4, result.Rows.Count);
			AssertEquals("The balance of Account '2222.22.22' should be 1100m", 1100m, result.Select("GLAccount ='2222.22.22'").FirstOrDefault().Field<decimal>("OpeningBalance"));
			AssertEquals("The balance of Account '3333.22.22' should be 600m", 600m, result.Select("GLAccount ='3333.22.22'").FirstOrDefault().Field<decimal>("OpeningBalance"));
			AssertEquals("The balance of Account '1111.22.22' should be 1700m", 1700m, result.Select("GLAccount ='1111.22.22'").FirstOrDefault().Field<decimal>("OpeningBalance"));
			AssertEquals("The balance of Account '4900.00.00' should be 766m", 766m, result.Select("GLAccount ='4900.00.00'").FirstOrDefault().Field<decimal>("OpeningBalance"));
		}

		public void TestOpeningBalanceWithNoteAccountWhichAccountNumberGreaterThanBalanceSheetStartAccount()
		{
			var helper = new TestDbHelper(TestConnection);
			var postDate = new DateTime(2020, 03, 16);

			var sqlText = $@"EXEC GLTransactionsSP
				@CompanyPK = '{TestDbHelper.DefaultCompanyPK}',
				@StartPeriod = NULL,
				@EndPeriod = NULL,
				@StartDate = '2020-03-01',
				@EndDate = '2020-03-31',
				@StartGLAccountPK = NULL,
				@EndGLAccountPK = NULL,
				@BranchPK = NULL,
				@DepartmentPK = NULL,
				@DisplayDescription = 'L',
				@TransactionCategory = '',
				@BatchNumberToGet = 0,
				@BatchNumberToSet = 0,
				@IncludeZeroBalance = 'N',
				@IsExportingBatch = 'Y',
				@HighWaterMark = NULL,
				@StartLocalGLAccountPK = NULL,
				@EndLocalGLAccountPK = NULL,
				@SubAccountType = NULL,
				@SubAccountPK = NULL,
				@NoSubAccount = NULL";

			var glAccountPK1 = helper.InsertGLAccount("3333.22.22", "TestGLAccount 1", "P&L");
			var glAccountPK2 = helper.InsertGLAccount("8888.22.22", "TestGLAccount 2", "NTE");
			var plAppropriationAccountPK = new Guid(TestConnection.ExecuteScalar(@"SELECT CONVERT(UNIQUEIDENTIFIER, CONVERT(NVARCHAR(4000), CONVERT(VARBINARY(8000), SD_BinaryValue)))
			FROM dbo.stmdata WHERE sd_name = 'GL_PL_APPROPRIATION_ACCOUNT'").ToString());

			helper.InsertAccPeriod(2019, 06, TestDbHelper.DefaultCompanyPK);
			helper.InsertAccPeriod(2020, 01, TestDbHelper.DefaultCompanyPK);
			helper.InsertAccPeriod(2020, 02, TestDbHelper.DefaultCompanyPK);
			helper.InsertAccPeriod(2020, 03, TestDbHelper.DefaultCompanyPK);
			helper.InsertAccPeriod(2020, 05, TestDbHelper.DefaultCompanyPK);

			helper.InsertGLAggregate(66, "", 202002, plAppropriationAccountPK, helper.DefaultBranchPK, helper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);
			helper.InsertGLAggregate(100, "", 201906, glAccountPK1, helper.DefaultBranchPK, helper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);
			helper.InsertGLAggregate(200, "", 202001, glAccountPK1, helper.DefaultBranchPK, helper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);
			helper.InsertGLAggregate(300, "", 202002, glAccountPK1, helper.DefaultBranchPK, helper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);
			helper.InsertGLAggregate(333, "", 202003, glAccountPK1, helper.DefaultBranchPK, helper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);
			helper.InsertGLAggregate(350, "", 202005, glAccountPK1, helper.DefaultBranchPK, helper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);
			helper.InsertGLAggregate(400, "", 201906, glAccountPK2, helper.DefaultBranchPK, helper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);
			helper.InsertGLAggregate(500, "", 202001, glAccountPK2, helper.DefaultBranchPK, helper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);
			helper.InsertGLAggregate(600, "", 202002, glAccountPK2, helper.DefaultBranchPK, helper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);
			helper.InsertGLAggregate(633, "", 202003, glAccountPK2, helper.DefaultBranchPK, helper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);
			helper.InsertGLAggregate(650, "", 202005, glAccountPK2, helper.DefaultBranchPK, helper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);

			var headerPK = helper.InsertTransactionHeader("GL", "GJL", "X0001", 160, postDate, helper.DefaultBranchPK, helper.DefaultDepartmentPK);
			helper.InsertTransactionLine(headerPK, null, null, glAccountPK1, helper.DefaultBranchPK, helper.DefaultDepartmentPK, null, 150, "CST", postDate, null, 10, 1, "A");
			var headerPK1 = helper.InsertTransactionHeader("GL", "NJL", "X0002", 260, postDate, helper.DefaultBranchPK, helper.DefaultDepartmentPK);
			helper.InsertTransactionLine(headerPK1, null, null, glAccountPK2, helper.DefaultBranchPK, helper.DefaultDepartmentPK, null, 250, "CST", postDate, null, 10, 1, "A");

			var balanceSheetStartAccount = TestConnection.ExecuteScalar(@"SELECT AG_AccountNum FROM dbo.AccGlHeader WHERE AG_PK = (SELECT CONVERT(UNIQUEIDENTIFIER, CONVERT(NVARCHAR(4000), CONVERT(VARBINARY(8000), SD_BinaryValue)))
			FROM dbo.stmdata WHERE sd_name = 'GL_BS_ACCOUNT_START')");
			AssertEquals("Pre condition", "5000.00.00", balanceSheetStartAccount);

			var result = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
			AssertEquals("Should be 3 rows in report", 3, result.Rows.Count);
			AssertEquals("The balance of Account '8888.22.22' should be 1500m", 1500m, result.Select("GLAccount ='8888.22.22'").FirstOrDefault().Field<decimal>("OpeningBalance"));
			AssertEquals("The balance of Account '3333.22.22' should be 500m", 500m, result.Select("GLAccount ='3333.22.22'").FirstOrDefault().Field<decimal>("OpeningBalance"));
			AssertEquals("The balance of Account '4900.00.00' should be 166m", 166m, result.Select("GLAccount ='4900.00.00'").FirstOrDefault().Field<decimal>("OpeningBalance"));
		}

		public void TestOpeningBalanceWithNoteAccountWhichAccountNumberLessThanBalanceSheetStartAccount()
		{
			var helper = new TestDbHelper(TestConnection);
			var postDate = new DateTime(2020, 03, 16);

			var sqlText = $@"EXEC GLTransactionsSP
				@CompanyPK = '{TestDbHelper.DefaultCompanyPK}',
				@StartPeriod = NULL,
				@EndPeriod = NULL,
				@StartDate = '2020-03-01',
				@EndDate = '2020-03-31',
				@StartGLAccountPK = NULL,
				@EndGLAccountPK = NULL,
				@BranchPK = NULL,
				@DepartmentPK = NULL,
				@DisplayDescription = 'L',
				@TransactionCategory = '',
				@BatchNumberToGet = 0,
				@BatchNumberToSet = 0,
				@IncludeZeroBalance = 'N',
				@IsExportingBatch = 'Y',
				@HighWaterMark = NULL,
				@StartLocalGLAccountPK = NULL,
				@EndLocalGLAccountPK = NULL,
				@SubAccountType = NULL,
				@SubAccountPK = NULL,
				@NoSubAccount = NULL";

			var glAccountPK1 = helper.InsertGLAccount("3333.22.22", "TestGLAccount 1", "P&L");
			var glAccountPK2 = helper.InsertGLAccount("2222.22.22", "TestGLAccount 2", "NTE");
			var plAppropriationAccountPK = new Guid(TestConnection.ExecuteScalar(@"SELECT CONVERT(UNIQUEIDENTIFIER, CONVERT(NVARCHAR(4000), CONVERT(VARBINARY(8000), SD_BinaryValue)))
			FROM dbo.stmdata WHERE sd_name = 'GL_PL_APPROPRIATION_ACCOUNT'").ToString());

			helper.InsertAccPeriod(2019, 06, TestDbHelper.DefaultCompanyPK);
			helper.InsertAccPeriod(2020, 01, TestDbHelper.DefaultCompanyPK);
			helper.InsertAccPeriod(2020, 02, TestDbHelper.DefaultCompanyPK);
			helper.InsertAccPeriod(2020, 03, TestDbHelper.DefaultCompanyPK);
			helper.InsertAccPeriod(2020, 05, TestDbHelper.DefaultCompanyPK);

			helper.InsertGLAggregate(66, "", 202002, plAppropriationAccountPK, helper.DefaultBranchPK, helper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);
			helper.InsertGLAggregate(100, "", 201906, glAccountPK1, helper.DefaultBranchPK, helper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);
			helper.InsertGLAggregate(200, "", 202001, glAccountPK1, helper.DefaultBranchPK, helper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);
			helper.InsertGLAggregate(300, "", 202002, glAccountPK1, helper.DefaultBranchPK, helper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);
			helper.InsertGLAggregate(333, "", 202003, glAccountPK1, helper.DefaultBranchPK, helper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);
			helper.InsertGLAggregate(350, "", 202005, glAccountPK1, helper.DefaultBranchPK, helper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);
			helper.InsertGLAggregate(400, "", 201906, glAccountPK2, helper.DefaultBranchPK, helper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);
			helper.InsertGLAggregate(500, "", 202001, glAccountPK2, helper.DefaultBranchPK, helper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);
			helper.InsertGLAggregate(600, "", 202002, glAccountPK2, helper.DefaultBranchPK, helper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);
			helper.InsertGLAggregate(633, "", 202003, glAccountPK2, helper.DefaultBranchPK, helper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);
			helper.InsertGLAggregate(650, "", 202005, glAccountPK2, helper.DefaultBranchPK, helper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);

			var headerPK = helper.InsertTransactionHeader("GL", "GJL", "X0001", 160, postDate, helper.DefaultBranchPK, helper.DefaultDepartmentPK);
			helper.InsertTransactionLine(headerPK, null, null, glAccountPK1, helper.DefaultBranchPK, helper.DefaultDepartmentPK, null, 150, "CST", postDate, null, 10, 1, "A");
			var headerPK1 = helper.InsertTransactionHeader("GL", "NJL", "X0002", 260, postDate, helper.DefaultBranchPK, helper.DefaultDepartmentPK);
			helper.InsertTransactionLine(headerPK1, null, null, glAccountPK2, helper.DefaultBranchPK, helper.DefaultDepartmentPK, null, 250, "CST", postDate, null, 10, 1, "A");

			var balanceSheetStartAccount = TestConnection.ExecuteScalar(@"SELECT AG_AccountNum FROM dbo.AccGlHeader WHERE AG_PK = (SELECT CONVERT(UNIQUEIDENTIFIER, CONVERT(NVARCHAR(4000), CONVERT(VARBINARY(8000), SD_BinaryValue)))
			FROM dbo.stmdata WHERE sd_name = 'GL_BS_ACCOUNT_START')");
			AssertEquals("Pre condition", "5000.00.00", balanceSheetStartAccount);

			var result = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
			AssertEquals("Should be 3 rows in report", 3, result.Rows.Count);
			AssertEquals("The balance of Account '2222.22.22' should be 1100m", 1100m, result.Select("GLAccount ='2222.22.22'").FirstOrDefault().Field<decimal>("OpeningBalance"));
			AssertEquals("The balance of Account '3333.22.22' should be 500m", 500m, result.Select("GLAccount ='3333.22.22'").FirstOrDefault().Field<decimal>("OpeningBalance"));
			AssertEquals("The balance of Account '4900.00.00' should be 166m", 166m, result.Select("GLAccount ='4900.00.00'").FirstOrDefault().Field<decimal>("OpeningBalance"));
		}

		public void TestOpeningBalanceWithAccountWhichAccountNumberEqualToBalanceSheetStartAccount()
		{
			var helper = new TestDbHelper(TestConnection);
			var postDate = new DateTime(2020, 03, 16);

			var sqlText = $@"EXEC GLTransactionsSP
				@CompanyPK = '{TestDbHelper.DefaultCompanyPK}',
				@StartPeriod = NULL,
				@EndPeriod = NULL,
				@StartDate = '2020-03-01',
				@EndDate = '2020-03-31',
				@StartGLAccountPK = NULL,
				@EndGLAccountPK = NULL,
				@BranchPK = NULL,
				@DepartmentPK = NULL,
				@DisplayDescription = 'L',
				@TransactionCategory = '',
				@BatchNumberToGet = 0,
				@BatchNumberToSet = 0,
				@IncludeZeroBalance = 'N',
				@IsExportingBatch = 'Y',
				@HighWaterMark = NULL,
				@StartLocalGLAccountPK = NULL,
				@EndLocalGLAccountPK = NULL,
				@SubAccountType = NULL,
				@SubAccountPK = NULL,
				@NoSubAccount = NULL";

			var balanceSheetStartAccountPK = new Guid(TestConnection.ExecuteScalar(@"SELECT CONVERT(UNIQUEIDENTIFIER, CONVERT(NVARCHAR(4000), CONVERT(VARBINARY(8000), SD_BinaryValue))) FROM dbo.stmdata WHERE sd_name = 'GL_BS_ACCOUNT_START'").ToString());
			TestConnection.ExecuteNonQuery($@"UPDATE dbo.AccGlHeader SET AG_AccountType = 'NTE', AG_SystemLastEditTimeUtc = GETUTCDATE(), AG_SystemLastEditUser = 'TST' WHERE AG_PK = '{balanceSheetStartAccountPK}'");
			var glAccountPK1 = helper.InsertGLAccount("3333.22.22", "TestGLAccount 1", "P&L");
			var plAppropriationAccountPK = new Guid(TestConnection.ExecuteScalar(@"SELECT CONVERT(UNIQUEIDENTIFIER, CONVERT(NVARCHAR(4000), CONVERT(VARBINARY(8000), SD_BinaryValue)))
			FROM dbo.stmdata WHERE sd_name = 'GL_PL_APPROPRIATION_ACCOUNT'").ToString());

			helper.InsertAccPeriod(2019, 06, TestDbHelper.DefaultCompanyPK);
			helper.InsertAccPeriod(2020, 01, TestDbHelper.DefaultCompanyPK);
			helper.InsertAccPeriod(2020, 02, TestDbHelper.DefaultCompanyPK);
			helper.InsertAccPeriod(2020, 03, TestDbHelper.DefaultCompanyPK);
			helper.InsertAccPeriod(2020, 05, TestDbHelper.DefaultCompanyPK);

			helper.InsertGLAggregate(66, "", 202002, plAppropriationAccountPK, helper.DefaultBranchPK, helper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);
			helper.InsertGLAggregate(100, "", 201906, glAccountPK1, helper.DefaultBranchPK, helper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);
			helper.InsertGLAggregate(200, "", 202001, glAccountPK1, helper.DefaultBranchPK, helper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);
			helper.InsertGLAggregate(300, "", 202002, glAccountPK1, helper.DefaultBranchPK, helper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);
			helper.InsertGLAggregate(333, "", 202003, glAccountPK1, helper.DefaultBranchPK, helper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);
			helper.InsertGLAggregate(350, "", 202005, glAccountPK1, helper.DefaultBranchPK, helper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);
			helper.InsertGLAggregate(400, "", 201906, balanceSheetStartAccountPK, helper.DefaultBranchPK, helper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);
			helper.InsertGLAggregate(500, "", 202001, balanceSheetStartAccountPK, helper.DefaultBranchPK, helper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);
			helper.InsertGLAggregate(600, "", 202002, balanceSheetStartAccountPK, helper.DefaultBranchPK, helper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);
			helper.InsertGLAggregate(633, "", 202003, balanceSheetStartAccountPK, helper.DefaultBranchPK, helper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);
			helper.InsertGLAggregate(650, "", 202005, balanceSheetStartAccountPK, helper.DefaultBranchPK, helper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);

			var headerPK = helper.InsertTransactionHeader("GL", "GJL", "X0001", 160, postDate, helper.DefaultBranchPK, helper.DefaultDepartmentPK);
			helper.InsertTransactionLine(headerPK, null, null, glAccountPK1, helper.DefaultBranchPK, helper.DefaultDepartmentPK, null, 150, "CST", postDate, null, 10, 1, "A");
			var headerPK1 = helper.InsertTransactionHeader("GL", "NJL", "X0002", 260, postDate, helper.DefaultBranchPK, helper.DefaultDepartmentPK);
			helper.InsertTransactionLine(headerPK1, null, null, balanceSheetStartAccountPK, helper.DefaultBranchPK, helper.DefaultDepartmentPK, null, 250, "CST", postDate, null, 10, 1, "A");

			var balanceSheetStartAccount = TestConnection.ExecuteScalar(@"SELECT AG_AccountNum FROM dbo.AccGlHeader WHERE AG_PK = (SELECT CONVERT(UNIQUEIDENTIFIER, CONVERT(NVARCHAR(4000), CONVERT(VARBINARY(8000), SD_BinaryValue)))
			FROM dbo.stmdata WHERE sd_name = 'GL_BS_ACCOUNT_START')");
			AssertEquals("Pre condition", "5000.00.00", balanceSheetStartAccount);

			var result = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
			AssertEquals("Should be 3 rows in report", 3, result.Rows.Count);
			AssertEquals("The balance of Account '5000.00.00' should be 1500m", 1500m, result.Select("GLAccount ='5000.00.00'").FirstOrDefault().Field<decimal>("OpeningBalance"));
			AssertEquals("The balance of Account '3333.22.22' should be 500m", 500m, result.Select("GLAccount ='3333.22.22'").FirstOrDefault().Field<decimal>("OpeningBalance"));
			AssertEquals("The balance of Account '4900.00.00' should be 166m", 166m, result.Select("GLAccount ='4900.00.00'").FirstOrDefault().Field<decimal>("OpeningBalance"));
		}

		#region helper

		public static readonly string StringLen1024 = new string('a', 1024);
		public static readonly string StringLen35 = new string('b', 35);

		public static void SetUpDataForTestTransactionDescPaymentReferenceNumberMaxLength(DbConnection testConnection)
		{
			AssertEquals(1024, StringLen1024.Length);
			AssertEquals(35, StringLen35.Length);

			testConnection.ExecuteNonQuery(@"INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate,AM_EndDate,AM_IsSubLedgerClosed,AM_IsGeneralLedgerClosed, AM_GC_Company) values (NEWID(), 201504,2015,'04/01/2005','04/30/2015 23:59',1,1,'878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')");

			testConnection.ExecuteNonQuery(@"
INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_TransactionCount, AH_Desc, AH_InvoiceDate, AH_DueDate, AH_InvoiceAmount, AH_OSTotal, AH_RX_NKTransactionCurrency, AH_ExchangeRate, AH_PostDate, AH_TransactionCategory, AH_OutstandingAmount, AH_PostToGL, AH_InvoiceTerm,AH_NumberOfSupportingDocuments, AH_OH, AH_OA_InvoiceAddressOverride,AH_GB, AH_GC, AH_GE, AH_ChequeOrReference)
VALUES('591E6B52-95DF-42F8-8972-53FFF59C123F', 'AR', 'INV', '00001000', 1, 'AR INVOICE', '2015-04-24 14:04:00', '2015-04-24 14:04:00', 6000, 6000, 'AUD', 1, '2015-04-24 14:04:00', 'FIN', 6000, 'Y', 'COD', 1, '0DAAB61B-255E-4AD7-AFC5-4E7B03C3BDA1', '37A65812-36DB-4D6C-9AB7-71EE9BF599F2', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '" + StringLen35 + @"')");
			var result = DataUtils.GetDataTableFromQuery(testConnection, "SELECT * FROM dbo.AccTransactionHeader where AH_PK = '591E6B52-95DF-42F8-8972-53FFF59C123F'");
			AssertEquals("Result should have row(s)", 1, result.Rows.Count);

			testConnection.ExecuteNonQuery(@"
INSERT INTO dbo.AccTransactionLines (AL_PK,AL_LineType,AL_Sequence,AL_Desc,AL_LineAmount, AL_GSTVATBasis, AL_OSAmount,AL_RX_NKTransactionCurrency,AL_ExchangeRate,AL_InputGSTVATRecoverable, AL_PostDate, AL_PostToGL, AL_ReverseDate,AL_ReverseToGL, AL_RevRecognitionType, AL_GE, AL_GB, AL_GC, AL_AG, AL_AH) 
VALUES('3E50F5C5-49B2-45D3-AA4C-099FAE6833CD', 'CST', 1, '" + StringLen1024 + @"', 3000, 'C', 3000, 'AUD', 1.000000000, 0, '2015-04-24 14:04:00', 'N', '2015-04-24 14:04:00', 'Y', 'IMM', '86BB1C22-0865-4685-996E-D56CBD136491', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '907B7A0F-3700-4C4C-B313-971C8B0AE6FC', '591E6B52-95DF-42F8-8972-53FFF59C123F')");
			result = DataUtils.GetDataTableFromQuery(testConnection, "SELECT * FROM dbo.AccTransactionLines where AL_AH = '591E6B52-95DF-42F8-8972-53FFF59C123F'");

			testConnection.ExecuteNonQuery(@"
				INSERT INTO dbo.AccCashBasisVAT
				(
				YC_PK,
				YC_PostDate,
				YC_MatchGroupNum,
				YC_TaxBaseAmount,
				YC_TaxAmount,
				YC_AL_TransactionLine,
				YC_GC,
				YC_SystemCreateTimeUtc,
				YC_SystemCreateUser)
				VALUES
				(
				'de1cefa5-10c1-4835-a178-9a128b2eccb2',
				'2015-04-24 14:04:00',
				0,
				100,
				100,
				'3E50F5C5-49B2-45D3-AA4C-099FAE6833CD', --YC_AL_TransactionLine,
				'878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', --GC
				'2015-04-24 14:04:00',
				'ABC');
				");

			AssertEquals("Result should have row(s)", 1, result.Rows.Count);
		}

		public static void SetUpDataForTestTransactionSecondRefMaxLength(DbConnection testConnection, string consolidatedInvoiceRef)
		{
			testConnection.ExecuteNonQuery(@"INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate,AM_EndDate,AM_IsSubLedgerClosed,AM_IsGeneralLedgerClosed, AM_GC_Company) values (NEWID(), 201504,2015,'04/01/2005','04/30/2015 23:59',1,1,'878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')");

			testConnection.ExecuteNonQuery(@"
INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_TransactionCount, AH_Desc, AH_InvoiceDate, AH_DueDate, AH_InvoiceAmount, AH_OSTotal, AH_RX_NKTransactionCurrency, AH_ExchangeRate, AH_PostDate, AH_TransactionCategory, AH_OutstandingAmount, AH_PostToGL, AH_InvoiceTerm,AH_NumberOfSupportingDocuments, AH_OH, AH_OA_InvoiceAddressOverride,AH_GB, AH_GC, AH_GE, AH_ChequeOrReference, AH_ConsolidatedInvoiceRef)
VALUES('591E6B52-95DF-42F8-8972-53FFF59C123F', 'AR', 'INV', '00001000', 1, 'AR INVOICE', '2015-04-24 14:04:00', '2015-04-24 14:04:00', 6000, 6000, 'AUD', 1, '2015-04-24 14:04:00', 'FIN', 6000, 'Y', 'COD', 1, '0DAAB61B-255E-4AD7-AFC5-4E7B03C3BDA1', '37A65812-36DB-4D6C-9AB7-71EE9BF599F2', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '" + StringLen35 + @"', '" + consolidatedInvoiceRef + @"')");

			testConnection.ExecuteNonQuery(@"
INSERT INTO dbo.AccTransactionLines (AL_PK,AL_LineType,AL_Sequence,AL_Desc,AL_LineAmount, AL_GSTVATBasis, AL_OSAmount,AL_RX_NKTransactionCurrency,AL_ExchangeRate,AL_InputGSTVATRecoverable, AL_PostDate, AL_PostToGL, AL_ReverseDate,AL_ReverseToGL, AL_RevRecognitionType, AL_GE, AL_GB, AL_GC, AL_AG, AL_AH) 
VALUES('3E50F5C5-49B2-45D3-AA4C-099FAE6833CD', 'CST', 1, '" + StringLen1024 + @"', 3000, 'C', 3000, 'AUD', 1.000000000, 0, '2015-04-24 14:04:00', 'N', '2015-04-24 14:04:00', 'Y', 'IMM', '86BB1C22-0865-4685-996E-D56CBD136491', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '907B7A0F-3700-4C4C-B313-971C8B0AE6FC', '591E6B52-95DF-42F8-8972-53FFF59C123F')");
		}
		#endregion
	}
}

