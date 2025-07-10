using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Data;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Testing.ScriptTests.DbCreateScriptTests
{
	class ChinaGLAccountBalanceTest : ScriptTest
	{
		public void TestOpenBalanceForPNLAccountWithDifferentCompanies()
		{
			var companyPKStr = "";
			PrepareData();
			var result = Excute(companyPKStr, 201909, "");
			AssertEquals("Should be 2 rows in report", 2, result.Rows.Count);
			var row = result.Select("AccountNum ='4900.00.00'").FirstOrDefault();
			AssertEquals("The opening balance of Account '4900.00.00' should be 0m", 0m, row["Amount"]);
			AssertEquals("The opening balance of Account '4900.00.00' should be 366m", 366m, row["OpeningBalance"]);

			row = result.Select("AccountNum ='3333.22.22'").FirstOrDefault();
			AssertEquals("The opening balance of Account '3333.22.22' should be 500m", 500m, row["Amount"]);
			AssertEquals("The opening balance of Account '3333.22.22' should be 700m", 700m, row["OpeningBalance"]);

			void PrepareData()
			{
				var helper = new TestDbHelper(TestConnection);

				var glAccount1 = TestObjectCreator.CreateAccGLHeader("3333.22.22", "TS", "", "P&L", "CR");
				var glAccountPK1 = glAccount1.PK.ToGuid();
				var plAppropriationAccountPK = new Guid(TestConnection.ExecuteScalar(@"SELECT CONVERT(UNIQUEIDENTIFIER, CONVERT(NVARCHAR(4000), CONVERT(VARBINARY(8000), SD_BinaryValue)))
			FROM dbo.stmdata WHERE sd_name = 'GL_PL_APPROPRIATION_ACCOUNT'").ToString());
				var desc1 = TestObjectCreator.CreateAccountDesriptorLight("4444.22.22", "COA", "ZH-CN", "CN");
				var desc2 = TestObjectCreator.CreateAccountDesriptorLight("4900.22.22", "COA", "ZH-CN", "CN");
				TestObjectCreator.CreateGLDescriptorPivotLight(desc1, glAccount1);
				TestObjectCreator.CreateGLDescriptorPivotLight(desc2, Factory.Load<AccGLHeader>(plAppropriationAccountPK));

				var companyPK = helper.InsertCompany("ABC", "ABC Compay", "CNY", "CN", true, true);
				companyPKStr = companyPK.ToString();
				var branch2PK = helper.InsertBranch("BR2", companyPK, "Branch 2");
				var departmentPK = helper.InsertDepartment("DP2", "Department 2");

				helper.InsertAccPeriod(2019, 05, TestDbHelper.DefaultCompanyPK);
				helper.InsertAccPeriod(2019, 06, TestDbHelper.DefaultCompanyPK);

				helper.InsertAccPeriod(2019, 07, companyPK, new DateTime(2020, 01, 01));
				helper.InsertAccPeriod(2019, 08, companyPK, new DateTime(2020, 02, 01));
				helper.InsertAccPeriod(2019, 09, companyPK, new DateTime(2020, 03, 01));
				helper.InsertAccPeriod(2019, 10, companyPK, new DateTime(2020, 04, 01));

				helper.InsertGLAggregate(66, "", 201907, plAppropriationAccountPK, branch2PK, departmentPK, companyPK);
				helper.InsertGLAggregate(100, "", 201905, glAccountPK1, branch2PK, departmentPK, companyPK);
				helper.InsertGLAggregate(200, "", 201906, glAccountPK1, branch2PK, departmentPK, companyPK);
				helper.InsertGLAggregate(300, "", 201907, glAccountPK1, branch2PK, departmentPK, companyPK);
				helper.InsertGLAggregate(400, "", 201908, glAccountPK1, branch2PK, departmentPK, companyPK);
				helper.InsertGLAggregate(500, "", 201909, glAccountPK1, branch2PK, departmentPK, companyPK);
				helper.InsertGLAggregate(600, "", 201910, glAccountPK1, branch2PK, departmentPK, companyPK);

				TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.StmData(SD_PK, SD_Name, SD_Owner, SD_DepartmentGuid, SD_Type, SD_BinaryValue, SD_GuidValue) VALUES(NEWID(), 'JournalEntriesLastProcessedDate', '{companyPKStr}', NULL, 'DT', convert(varbinary(8000), N'2001-01-01 00:00:00.000'), NULL)");
			}
		}

		public void TestOpeningBalanceWhenPartiallyMatchedTransactionsInvolved()
		{
			PrepareData();
			var result = Excute("878D7ACA-FFC3-49FC-9710-969CA0C0F2AC", 201602, "");
			AssertEquals(1, result.Select("AccountNum='6210.00.00' and AccountType ='BSH' and AG_DebitCredit='DR' and OsOpeningBalance =70 and Period=201602 and Account='WALHAT'").Length);

			void PrepareData()
			{
				TestConnection.ExecuteNonQuery(@"INSERT INTO dbo.AccPeriodManagement(AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_GC_Company)
								values(NEWID(), 201512, 2015, '12/01/2015', '12/30/2015 23:59', 1, 1, '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')");

				TestConnection.ExecuteNonQuery(@"INSERT INTO dbo.AccPeriodManagement(AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_GC_Company)
								values(NEWID(), 201601, 2016, '01/01/2016', '01/31/2016 23:59', 1, 1, '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')");

				TestConnection.ExecuteNonQuery(@"INSERT INTO dbo.AccPeriodManagement(AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_GC_Company)
								values(NEWID(), 201602, 2016, '02/01/2016', '02/29/2016 23:59', 1, 1, '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')");

				TestConnection.ExecuteNonQuery(@"INSERT INTO dbo.AccTransactionHeader(AH_PK, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_InvoiceDate, AH_DueDate, AH_PostToGL, AH_InvoiceAmount, AH_OSTotal, AH_RX_NKTransactionCurrency, AH_ExchangeRate, AH_PostDate, AH_OH, AH_GC, AH_GB, AH_GE)
								VALUES('7BE5FA30-4618-497C-8E83-182104925E4F', 'AR', 'JNL', '00001001', 'DEC 05 2015  3:44:00:000PM', 'DEC 25 2015  3:44:00:000PM', 'Y', 100.0000, 100.0000, 'CNY', 1, 'DEC 05 2015  3:44:00:000PM', 'c72bfdd2-e264-4dd9-a5ab-3f75efd8569b', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '27a55065-ac88-4ec3-8bed-e575e79172cb', 'f5c72696-19ad-4759-879f-89c8532ff238')");

				TestConnection.ExecuteNonQuery(@"INSERT INTO dbo.AccTransactionHeader(AH_PK, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_InvoiceDate, AH_DueDate, AH_PostToGL, AH_InvoiceAmount, AH_OSTotal, AH_RX_NKTransactionCurrency, AH_ExchangeRate, AH_PostDate, AH_OH, AH_GC, AH_GB, AH_GE)
								VALUES('344319BA-BBFE-4E80-8B1F-0E587B3240C6', 'AR', 'REC', '123456', 'JAN 05 2016  3:44:00:000PM', 'JAN 05 2016  3:44:00:000PM', 'Y', -30.0000, -30.0000, 'CNY', 1, 'JAN 05 2016  3:44:00:000PM', 'c72bfdd2-e264-4dd9-a5ab-3f75efd8569b', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '27a55065-ac88-4ec3-8bed-e575e79172cb', 'f5c72696-19ad-4759-879f-89c8532ff238')");

				TestConnection.ExecuteNonQuery(@"INSERT INTO dbo.AccTransactionMatchLink(AP_PK, AP_Amount, AP_MatchGroupNum, AP_MatchDate, AP_AH)
								VALUES(NEWID(), 30, 'M10000', 'JAN 20 2016  3:44:00:000PM', '7BE5FA30-4618-497C-8E83-182104925E4F')");

				TestConnection.ExecuteNonQuery(@"INSERT INTO dbo.AccTransactionMatchLink(AP_PK, AP_Amount, AP_MatchGroupNum, AP_MatchDate, AP_AH)
								VALUES(NEWID(), -30, 'M10000', 'JAN 20 2016  3:44:00:000PM', '344319BA-BBFE-4E80-8B1F-0E587B3240C6')");
				TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.StmData(SD_PK, SD_Name, SD_Owner, SD_DepartmentGuid, SD_Type, SD_BinaryValue, SD_GuidValue) VALUES(NEWID(), 'JournalEntriesLastProcessedDate', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', NULL, 'DT', convert(varbinary(8000), N'2001-01-01 00:00:00.000'), NULL)");
			}
		}

		public void TestCalculateCorrectOSAmountBasedOnPeriod()
		{
			PrepareData();
			var result = Excute("878D7ACA-FFC3-49FC-9710-969CA0C0F2AC", 201504, "");
			AssertEquals(1, result.Select("AccountNum='8210.00.00' and AccountType ='BSH' and AG_DebitCredit='CR' and Amount =-120 and OsAmount = -20 and Period=201504 and Account='WALHAT'").Length);

			void PrepareData()
			{
				TestConnection.ExecuteNonQuery(@"INSERT INTO dbo.AccPeriodManagement
								(AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_GC_Company)
								values(NEWID(), 201504, 2015, '04/01/2015', '04/30/2015 23:59', 1, 1, '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')");

				TestConnection.ExecuteNonQuery(@"INSERT INTO dbo.AccBankAccount(AB_PK, AB_Code, AB_RX_NKAccountCurrency, AB_AG, AB_GC, AB_AccountNum, AB_BSB)
								VALUES('d4d40998-b563-4e6f-b1ed-8ea852e0f2fd', 'AAA', 'USD', 'EFBF74F0-024D-41E7-AF80-C6B858217AF6', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '111111111', '123456')");

				TestConnection.ExecuteNonQuery(@"INSERT INTO dbo.AccTransactionHeader(AH_PK, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_InvoiceDate, AH_DueDate, AH_PostToGL, AH_RX_NKTransactionCurrency, AH_ExchangeRate, AH_InvoiceAmount, AH_OSTotal, AH_PostDate, AH_OH, AH_GC, AH_GB, AH_GE, AH_AG, AH_AB)
								VALUES(NEWID(), 'AP', 'PAY', '001', 'Apr 30 2015  3:44:00:000PM', 'Apr 30 2015  3:44:00:000PM', 'Y', 'USD', 6.0, -120.0000, -20.0000, 'Apr 30 2015  3:44:00:000PM', 'c72bfdd2-e264-4dd9-a5ab-3f75efd8569b', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '27a55065-ac88-4ec3-8bed-e575e79172cb', 'f5c72696-19ad-4759-879f-89c8532ff238', '7A5FF88B-76BE-41AE-A3EC-CEAC983125EF', 'd4d40998-b563-4e6f-b1ed-8ea852e0f2fd')");

				TestConnection.ExecuteNonQuery(@"INSERT INTO dbo.AccTransactionHeader(AH_PK, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_InvoiceDate, AH_DueDate, AH_PostToGL, AH_RX_NKTransactionCurrency, AH_ExchangeRate, AH_InvoiceAmount, AH_OSTotal, AH_PostDate, AH_OH, AH_GC, AH_GB, AH_GE, AH_AG, AH_AB)
								VALUES(NEWID(), 'AP', 'PAY', '002', 'May 01 2015  3:44:00:000PM', 'May 01 2015  3:44:00:000PM', 'Y', 'USD', 6.0, -180.0000, -30.0000, 'May 01 2015  3:44:00:000PM', 'c72bfdd2-e264-4dd9-a5ab-3f75efd8569b', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '27a55065-ac88-4ec3-8bed-e575e79172cb', 'f5c72696-19ad-4759-879f-89c8532ff238', '7A5FF88B-76BE-41AE-A3EC-CEAC983125EF', 'd4d40998-b563-4e6f-b1ed-8ea852e0f2fd')");
				TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.StmData(SD_PK, SD_Name, SD_Owner, SD_DepartmentGuid, SD_Type, SD_BinaryValue, SD_GuidValue) VALUES(NEWID(), 'JournalEntriesLastProcessedDate', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', NULL, 'DT', convert(varbinary(8000), N'2001-01-01 00:00:00.000'), NULL)");
			}
		}

		public void TestAmountWithOtherTaxes()
		{
			PrepareData();
			var result = Excute("878D7ACA-FFC3-49FC-9710-969CA0C0F2AC", 202007, "");
			AssertEquals(1, result.Select("AccountNum='8210.00.00' and AccountType ='BSH' and AG_DebitCredit='CR' and Amount =100 and OsAmount = 100 and Period=202007 and Account='WALHAT'").Length);

			void PrepareData()
			{
				TestConnection.ExecuteNonQuery(@"INSERT INTO dbo.AccPeriodManagement
								(AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_GC_Company)
								values(NEWID(), 202007, 2020, '07/01/2020', '07/31/2020 23:59', 1, 1, '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')");

				TestConnection.ExecuteNonQuery(@"INSERT INTO dbo.AccBankAccount(AB_PK, AB_Code, AB_RX_NKAccountCurrency, AB_AG, AB_GC, AB_AccountNum, AB_BSB)
								VALUES('d4d40998-b563-4e6f-b1ed-8ea852e0f2fd', 'AAA', 'USD', 'EFBF74F0-024D-41E7-AF80-C6B858217AF6', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '111111111', '123456')");

				TestConnection.ExecuteNonQuery(@"INSERT INTO dbo.AccTransactionHeader(AH_PK, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_InvoiceDate, AH_DueDate, AH_PostToGL, AH_RX_NKTransactionCurrency, AH_ExchangeRate, AH_InvoiceAmount, AH_GSTAmount, AH_LocalTaxAmountOtherTaxes, AH_OSTotal, AH_PostDate, AH_OH, AH_GC, AH_GB, AH_GE, AH_AG, AH_AB)
								VALUES(NEWID(), 'AP', 'PAY', '001', 'Jul 30 2020  3:44:00:000PM', 'Jul 30 2020  3:44:00:000PM', 'Y', 'USD', 1.0, -120.0000, 120.0000, 100.0000, 100.0000, 'Jul 30 2020  3:44:00:000PM', 'c72bfdd2-e264-4dd9-a5ab-3f75efd8569b', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '27a55065-ac88-4ec3-8bed-e575e79172cb', 'f5c72696-19ad-4759-879f-89c8532ff238', '7A5FF88B-76BE-41AE-A3EC-CEAC983125EF', 'd4d40998-b563-4e6f-b1ed-8ea852e0f2fd')");
				TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.StmData(SD_PK, SD_Name, SD_Owner, SD_DepartmentGuid, SD_Type, SD_BinaryValue, SD_GuidValue) VALUES(NEWID(), 'JournalEntriesLastProcessedDate', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', NULL, 'DT', convert(varbinary(8000), N'2001-01-01 00:00:00.000'), NULL)");
			}
		}

		public void TestCalculateCorrectOpeningBalanceWithSecondLocalGLAccount()
		{
			PrepareData();
			var result = Excute("878D7ACA-FFC3-49FC-9710-969CA0C0F2AC", 201504, "");
			AssertEquals("USD Currency Row Count", 2, result.Select("Currency = 'USD'").Length);
			AssertEquals(1, result.Select("OpeningBalance = -180 and OsOpeningBalance=-30 and OSAmount =0 and Account ='COLFAB' and Amount = 0 and AccountNum='8210.00.00' and Period = 201504").Length);
			AssertEquals(1, result.Select("AccountNum='8210.00.00' and AccountType ='BSH' and AG_DebitCredit='CR' and Amount =-120 and OsAmount = -20 and OpeningBalance = -120 and Period=201504 and Account='WALHAT'").Length);

			void PrepareData()
			{
				#region setup

				var insertRegistryValues = @"DECLARE @registryRawValue nvarchar(MAX)
Set @registryRawValue = '<?xml version=""1.0"" encoding=""utf-16""?>
<ArrayOfGLLocalNumberFormat xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
	<GLLocalNumberFormat>
		<Language>ZH-CN</Language>
		<CountryCode>CN</CountryCode>
		<NumberFormat>4-2-2</NumberFormat>
		<IsFixedLength>Y</IsFixedLength>
	</GLLocalNumberFormat>
</ArrayOfGLLocalNumberFormat>'
INSERT INTO dbo.StmData
(SD_PK, SD_Name, SD_Type, SD_Owner, SD_BinaryValue, SD_IsLogged, SD_IsCancelled)
Values
(newID(), 'LocalNumberFormats', 'BIN', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', CONVERT(varbinary(MAX), CONVERT(XML, @registryRawValue)), 1, 0)";
				TestConnection.ExecuteNonQuery(insertRegistryValues);

				TestConnection.ExecuteNonQuery(@"INSERT INTO dbo.AccBankAccount(AB_PK, AB_Code, AB_RX_NKAccountCurrency, AB_AG, AB_GC, AB_AccountNum, AB_BSB)
								VALUES('d4d40998-b563-4e6f-b1ed-8ea852e0f2fd', 'AAA', 'USD', 'EFBF74F0-024D-41E7-AF80-C6B858217AF6', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '111111111', '123456')");

				TestConnection.ExecuteNonQuery(@"INSERT INTO dbo.AccGLAccountDescriptor(AJ_PK,AJ_Language,AJ_AccountDescription,AJ_LocalAccountNumber,AJ_ReportType,AJ_DebitCredit,AJ_RN_NKCountryOfCompliance) 
VALUES('E94C4723-A84E-4D01-85B3-B09F6C6D3AFB','ZH-CN','测试01','12340100','COA','CR','CN')");
				TestConnection.ExecuteNonQuery(@"INSERT INTO dbo.AccGLDescriptorPivot (YJ_PK, YJ_AG, YJ_AJ) values (NEWID(), '7A5FF88B-76BE-41AE-A3EC-CEAC983125EF', 'E94C4723-A84E-4D01-85B3-B09F6C6D3AFB')");

				#endregion

				#region previous period

				TestConnection.ExecuteNonQuery(@"INSERT INTO dbo.AccPeriodManagement
								(AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_GC_Company)
								values(NEWID(), 201503, 2015, '03/01/2015', '03/31/2015 23:59', 1, 1, '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')");

				TestConnection.ExecuteNonQuery(@"INSERT INTO dbo.AccTransactionHeader(AH_PK, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_InvoiceDate, AH_DueDate, AH_PostToGL, AH_RX_NKTransactionCurrency, AH_ExchangeRate, AH_InvoiceAmount, AH_OSTotal, AH_PostDate, AH_OH, AH_GC, AH_GB, AH_GE, AH_AG, AH_AB)
								VALUES(NEWID(), 'AP', 'PAY', '001', 'Mar 30 2015  3:44:00:000PM', 'Mar 30 2015  3:44:00:000PM', 'Y', 'USD', 6.0, -120.0000, -20.0000, 'Mar 30 2015  3:44:00:000PM', 'c72bfdd2-e264-4dd9-a5ab-3f75efd8569b', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '27a55065-ac88-4ec3-8bed-e575e79172cb', 'f5c72696-19ad-4759-879f-89c8532ff238', '7A5FF88B-76BE-41AE-A3EC-CEAC983125EF', 'd4d40998-b563-4e6f-b1ed-8ea852e0f2fd')");

				TestConnection.ExecuteNonQuery(@"INSERT INTO dbo.AccTransactionHeader(AH_PK, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_InvoiceDate, AH_DueDate, AH_PostToGL, AH_RX_NKTransactionCurrency, AH_ExchangeRate, AH_InvoiceAmount, AH_OSTotal, AH_PostDate, AH_OH, AH_GC, AH_GB, AH_GE, AH_AG, AH_AB)
								VALUES(NEWID(), 'AP', 'PAY', '002', 'Mar 02 2015  3:44:00:000PM', 'Mar 02 2015  3:44:00:000PM', 'Y', 'USD', 6.0, -180.0000, -30.0000, 'Mar 01 2015  3:44:00:000PM', 'F9095D98-B463-4616-AB41-47E8E1FD9E73', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '27a55065-ac88-4ec3-8bed-e575e79172cb', 'f5c72696-19ad-4759-879f-89c8532ff238', '7A5FF88B-76BE-41AE-A3EC-CEAC983125EF', 'd4d40998-b563-4e6f-b1ed-8ea852e0f2fd')");

				#endregion

				#region current period

				TestConnection.ExecuteNonQuery(@"INSERT INTO dbo.AccPeriodManagement
								(AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_GC_Company)
								values(NEWID(), 201504, 2015, '04/01/2015', '04/30/2015 23:59', 1, 1, '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')");

				TestConnection.ExecuteNonQuery(@"INSERT INTO dbo.AccTransactionHeader(AH_PK, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_InvoiceDate, AH_DueDate, AH_PostToGL, AH_RX_NKTransactionCurrency, AH_ExchangeRate, AH_InvoiceAmount, AH_OSTotal, AH_PostDate, AH_OH, AH_GC, AH_GB, AH_GE, AH_AG, AH_AB)
								VALUES(NEWID(), 'AP', 'PAY', '003', 'Apr 30 2015  3:44:00:000PM', 'Apr 30 2015  3:44:00:000PM', 'Y', 'USD', 6.0, -120.0000, -20.0000, 'Apr 30 2015  3:44:00:000PM', 'c72bfdd2-e264-4dd9-a5ab-3f75efd8569b', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '27a55065-ac88-4ec3-8bed-e575e79172cb', 'f5c72696-19ad-4759-879f-89c8532ff238', '7A5FF88B-76BE-41AE-A3EC-CEAC983125EF', 'd4d40998-b563-4e6f-b1ed-8ea852e0f2fd')");

				#endregion
				TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.StmData(SD_PK, SD_Name, SD_Owner, SD_DepartmentGuid, SD_Type, SD_BinaryValue, SD_GuidValue) VALUES(NEWID(), 'JournalEntriesLastProcessedDate', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', NULL, 'DT', convert(varbinary(8000), N'2001-01-01 00:00:00.000'), NULL)");
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
				var insertRegistryValues = @"DECLARE @registryRawValue nvarchar(MAX)
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
INSERT INTO dbo.StmData
(SD_PK, SD_Name, SD_Type, SD_Owner, SD_BinaryValue, SD_IsLogged, SD_IsCancelled)
Values
(newID(), 'GLJournalAdjustmentCategoriesList', 'BIN', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', CONVERT(varbinary(MAX), CONVERT(XML, @registryRawValue)), 1, 0)";
				TestConnection.ExecuteNonQuery(insertRegistryValues);
				TestConnection.ExecuteNonQuery(@"INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate,AM_EndDate,AM_IsSubLedgerClosed,AM_IsGeneralLedgerClosed, AM_GC_Company) values (NEWID(), 201504,2015,'04/01/2015','04/30/2015 23:59',1,1,'878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')");
				TestConnection.ExecuteNonQuery(@"INSERT INTO dbo.[AccGLAggregate] ([AA_PK],[AA_Amount],[AA_Period],[AA_AG],[AA_GB],[AA_GC],[AA_GE],[AA_TransactionCategory])VALUES('14F778B3-6C56-4A92-9027-AC3D9822BC80',2.0000,201504,'ac129d82-b88d-45ee-bce5-25592f734023','FDD429D2-648C-4895-8F9F-06E90DED2BE5','878D7ACA-FFC3-49FC-9710-969CA0C0F2AC','F5C72696-19AD-4759-879F-89C8532FF238','INT')");
				TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.AccBankAccount (AB_PK, AB_Code, AB_RX_NKAccountCurrency, AB_AG, AB_GC, AB_AccountNum, AB_BSB)
VALUES('d4d40998-b563-4e6f-b1ed-8ea852e0f2fd', 'AAA', 'AUD', 'ac129d82-b88d-45ee-bce5-25592f734023', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '111111111','123456')");

				TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_InvoiceDate, AH_DueDate, AH_InvoiceAmount, AH_OSTotal, AH_PostDate, AH_OH, AH_GC, AH_GB, AH_GE, AH_AG, AH_AB) 
VALUES(NEWID(), 'AP', 'PAY', 'Apr 20 2015  3:44:00:000PM', 'Apr 20 2015  3:44:00:000PM', -100.0000, -100.0000, 'Apr 20 2015  3:44:00:000PM', 'c72bfdd2-e264-4dd9-a5ab-3f75efd8569b', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '27a55065-ac88-4ec3-8bed-e575e79172cb', 'f5c72696-19ad-4759-879f-89c8532ff238', 'ac129d82-b88d-45ee-bce5-25592f734023', 'd4d40998-b563-4e6f-b1ed-8ea852e0f2fd')");
				TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_InvoiceDate, AH_DueDate, AH_InvoiceAmount, AH_OSTotal, AH_PostDate, AH_OH, AH_GC, AH_GB, AH_GE, AH_AG, AH_AB, AH_TransactionNum, AH_TransactionCategory) 
VALUES('ab530eca-4866-4691-b409-8f13682aa798', 'GL', 'GJL', 'Apr 20 2015  3:44:00:000PM', 'Apr 20 2015  3:44:00:000PM', -100.0000, -100.0000, 'Apr 20 2015  3:44:00:000PM', 'c72bfdd2-e264-4dd9-a5ab-3f75efd8569b', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '27a55065-ac88-4ec3-8bed-e575e79172cb', 'f5c72696-19ad-4759-879f-89c8532ff238', 'ac129d82-b88d-45ee-bce5-25592f734023', 'd4d40998-b563-4e6f-b1ed-8ea852e0f2fd', 'Test2', 'INT')

INSERT INTO dbo.AccTransactionLines (AL_PK, AL_AH, AL_AG, AL_GC, AL_GB, AL_GE, AL_LineType)
VALUES(NEWID(), 'ab530eca-4866-4691-b409-8f13682aa798', 'ac129d82-b88d-45ee-bce5-25592f734023', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '27a55065-ac88-4ec3-8bed-e575e79172cb', 'f5c72696-19ad-4759-879f-89c8532ff238', 'GJL')");
				TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.StmData(SD_PK, SD_Name, SD_Owner, SD_DepartmentGuid, SD_Type, SD_BinaryValue, SD_GuidValue) VALUES(NEWID(), 'JournalEntriesLastProcessedDate', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', NULL, 'DT', convert(varbinary(8000), N'2001-01-01 00:00:00.000'), NULL)");
			}
		}

		public void TestOsOpeningBalanceAndOsAmountForBranchPKList()
		{
			var companyPKStr = "";
			var branchStr = "";
			PrepareData();
			var result = Excute(companyPKStr, 201602, "");
			AssertEquals(1, result.Select("AG_DebitCredit = 'DR' and OpeningBalance = 70 and OsOpeningBalance=70 and AccountNum ='6210.00.00'").Length);

			result = Excute(companyPKStr, 201602, branchStr);
			AssertEquals(1, result.Select("AG_DebitCredit = 'DR' and OpeningBalance = 100 and Amount =0").Length);

			result = Excute(companyPKStr, 201601, "");
			AssertEquals(1, result.Select("AG_DebitCredit = 'DR' and Amount =-30 and OpeningBalance = 100 and Period =201601").Length);

			result = Excute(companyPKStr, 201601, branchStr);
			AssertEquals(1, result.Select("AG_DebitCredit = 'DR' and Amount =0 and OpeningBalance = 100 and Currency = 'AUD'").Length);

			void PrepareData()
			{
				var helper = new TestDbHelper(TestConnection);
				var currentCompany = TestDbHelper.DefaultCompanyPK;
				companyPKStr = currentCompany.ToString();
				var currentBranch = helper.InsertBranch("CBH", TestDbHelper.DefaultCompanyPK);
				branchStr = currentBranch.ToString();
				var currentDep = helper.InsertDepartment("CDP");

				var nonCurrentBranch = helper.InsertBranch("OTH", currentCompany);
				helper.InsertAccPeriod(2016, 01, currentCompany);

				var bankAccountPK = helper.InsertBankAccount("BANKCDE", GetFirstGLAccount());

				helper.InsertTransactionHeader("AR", "REC", "00001001", 100m, new DateTime(2015, 12, 25), currentBranch, currentDep, bankAccountPK);
				helper.InsertTransactionHeader("CB", "ORC", "00001003", -500m, new DateTime(2015, 12, 05), currentBranch, currentDep, bankAccountPK);
				helper.InsertTransactionHeader("AR", "REC", "00001002", -30m, new DateTime(2016, 01, 05), nonCurrentBranch, currentDep, bankAccountPK);
				helper.InsertTransactionHeader("CB", "OPY", "00001004", 300m, new DateTime(2016, 01, 05), nonCurrentBranch, currentDep, bankAccountPK);
				TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.StmData(SD_PK, SD_Name, SD_Owner, SD_DepartmentGuid, SD_Type, SD_BinaryValue, SD_GuidValue) VALUES(NEWID(), 'JournalEntriesLastProcessedDate', '{currentCompany}', NULL, 'DT', convert(varbinary(8000), N'2001-01-01 00:00:00.000'), NULL)");
			}
		}

		Guid GetFirstGLAccount()
		{
			string sQL = $"SELECT TOP 1 AG_PK FROM {Db.DatabaseName}.dbo.AccGLHeader";
			return (Guid)TestConnection.ExecuteScalar(sQL);
		}

		public void TestTier2IsEmptyWithLocalNumberFormat()
		{
			var companyPKStr = GlbCompany.CurrentCompany.PK.ToString();
			PrepareData();
			var result = Excute(companyPKStr, 202403, "");
			AssertEquals(1, result.Select("GLAccount = '10010000' and Amount = 60").Length);
			AssertEquals(1, result.Select("GLAccount = '10010001' and Amount = 20").Length);
			AssertEquals(1, result.Select("GLAccount = '10010100' and Amount = 40").Length);
			AssertEquals(1, result.Select("GLAccount = '10010101' and Amount = 10").Length);

			void PrepareData()
			{
				PrepareTestDataForEmptyTier2(Core.Constants.Languages.ChineseSimplified);
			}
		}

		public void TestTier2IsEmptyWithoutLocalNumberFormat()
		{
			var companyPKStr = GlbCompany.CurrentCompany.PK.ToString();
			PrepareData();
			var result = Excute(companyPKStr, 202403, "");
			AssertEquals(0, result.Select("GLAccount = '10010000' and Amount = 60").Length);
			AssertEquals(1, result.Select("GLAccount = '10010001' and Amount = 20").Length);
			AssertEquals(1, result.Select("GLAccount = '10010100' and Amount = 30").Length);
			AssertEquals(1, result.Select("GLAccount = '10010101' and Amount = 10").Length);

			void PrepareData()
			{
				PrepareTestDataForEmptyTier2(Core.Constants.Languages.Albanian);
			}
		}

		void PrepareTestDataForEmptyTier2(string language)
		{
			TestConnection.ExecuteNonQuery(@"DELETE FROM dbo.AccPeriodManagement");
			var list = new GLLocalNumberFormatCollection();
			var localNumberFormat = list.AddNew();
			localNumberFormat.NumberFormat = "4-2-2";
			localNumberFormat.CountryCode = Core.Constants.CountryCodes.China;
			localNumberFormat.Language = language;
			localNumberFormat.IsFixedLength = false;
			AccountingMasterFilesRegistry.Instance.LocalNumberFormats.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			var creator = new TestObjectCreator(Factory);
			var companyPK = GlbCompany.CurrentCompany.PK;
			creator.CreateTestPeriodsForEntireYear(GlbCompany.CurrentCompany, 2024);
			AccountingMasterFilesRegistry.Instance.JournalEntriesLastProcessedDate.SetValue(companyPK.ToGuid(), Guid.Empty, Guid.Empty, new DateTime(2023, 12, 01));

			var header1 = creator.CreateGLHeader("989898");
			var header2 = creator.CreateGLHeader("989897");
			var header3 = creator.CreateGLHeader("989896");
			var header4 = creator.CreateGLHeader("989895");
			var descriptor1 = creator.CreateAccountDesriptorLight("10010000", "COA", "ZH-CN", "CN");
			creator.CreateGLDescriptorPivotLight(descriptor1, header1);
			var descriptor2 = creator.CreateAccountDesriptorLight("10010100", "COA", "ZH-CN", "CN");
			creator.CreateGLDescriptorPivotLight(descriptor2, header2);
			var descriptor3 = creator.CreateAccountDesriptorLight("10010101", "COA", "ZH-CN", "CN");
			creator.CreateGLDescriptorPivotLight(descriptor3, header3);
			var descriptor4 = creator.CreateAccountDesriptorLight("10010001", "COA", "ZH-CN", "CN");
			creator.CreateGLDescriptorPivotLight(descriptor4, header4);

			var branchPK = GlbBranch.CurrentBranch.PK;
			var departmentPK = GlbDepartment.CurrentDepartment.PK;
			creator.CreateAccGLAggregate(10, 202403, header3.PK, branchPK, companyPK, departmentPK, "");
			creator.CreateAccGLAggregate(20, 202403, header4.PK, branchPK, companyPK, departmentPK, "");
			creator.CreateAccGLAggregate(30, 202403, header2.PK, branchPK, companyPK, departmentPK, "");
			Factory.Save();
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

		protected string ScriptDbName
		{
			get { return Db.DatabaseName; }
		}
	}
}
