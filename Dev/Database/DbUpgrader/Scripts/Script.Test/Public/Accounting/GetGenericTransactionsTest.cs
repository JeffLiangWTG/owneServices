using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting
{
	[TestedType(typeof(GetGenericTransactions))]
	class GetGenericTransactionsTest : DbCreateScriptTest
	{
		public void TestCompanyFilter()
		{
			var company1PK = "878D7ACA-FFC3-49FC-9710-969CA0C0F2AC";
			var company2PK = "22C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061";

			TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_InvoiceDate, AH_DueDate, AH_InvoiceAmount, AH_OSTotal
, AH_PostDate, AH_OH, AH_GC, AH_GB, AH_GE, AH_AG) 
VALUES(NEWID(), 'AP', 'JNL', 'May 25 2005  3:44:00:000PM', 'May 25 2005  3:44:00:000PM', -100.0000, -100.0000
, 'May 25 2005  3:44:00:000PM', 'c72bfdd2-e264-4dd9-a5ab-3f75efd8569b', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '27a55065-ac88-4ec3-8bed-e575e79172cb', 'f5c72696-19ad-4759-879f-89c8532ff238', '245EE66C-4BCC-424D-956B-34077A3AAB96')");

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("SELECT * FROM GetGenericTransactions ('{0}')", company1PK));
			AssertEquals("Result should have row(s)", 1, result.Rows.Count);

			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("SELECT * FROM GetGenericTransactions ('{0}')", company2PK));
			AssertEquals("Result should have row(s)", 0, result.Rows.Count);
		}

		public void TestVT_TotalWithOtherTaxes()
		{
			TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_InvoiceDate, AH_DueDate, AH_InvoiceAmount, AH_OSTotal, AH_GSTAmount, AH_LocalTaxAmountOtherTaxes
, AH_PostDate, AH_OH, AH_GC, AH_GB, AH_GE, AH_AG) 
VALUES(NEWID(), 'AP', 'JNL', 'May 25 2005  3:44:00:000PM', 'May 25 2005  3:44:00:000PM', -100.0000, -120.0000, -10.0000, -10.0000
, 'May 25 2005  3:44:00:000PM', 'c72bfdd2-e264-4dd9-a5ab-3f75efd8569b', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '27a55065-ac88-4ec3-8bed-e575e79172cb', 'f5c72696-19ad-4759-879f-89c8532ff238', '245EE66C-4BCC-424D-956B-34077A3AAB96')");
			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM GetGenericTransactions ('878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')");
			AssertEquals("Result should have row(s)", 1, result.Rows.Count);
			AssertEquals(-120m, result.Rows[0]["VT_Total"]);
		}

		public void TestGetGenericTransactions()
		{
			// OrgHeader(OH) - ABIGAS - pk = '0daab61b-255e-4ad7-afc5-4e7b03c3bda1'
			// GlbGroup(GG) - PMG - pk = '55896e13-12be-4fd4-ac94-2956795a5be2'
			// AccGroups(AR) - ADMIN - pk = '333810a4-215d-49c0-be06-92dc8138bc60'

			#region GLFromHeader
			// Test: @AP_Control_Account
			// [AP / JNL] AG_PK = '245EE66C-4BCC-424D-956B-34077A3AAB96' (ControlAccount = 2010.00.00, HeaderAccount = 2020.00.00)			
			TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_InvoiceDate, AH_DueDate, AH_InvoiceAmount, AH_OSTotal
, AH_PostDate, AH_OH, AH_GC, AH_GB, AH_GE, AH_AG) 
VALUES(NEWID(), 'AP', 'JNL', 'May 25 2005  3:44:00:000PM', 'May 25 2005  3:44:00:000PM', -100.0000, -100.0000
, 'May 25 2005  3:44:00:000PM', 'c72bfdd2-e264-4dd9-a5ab-3f75efd8569b', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '27a55065-ac88-4ec3-8bed-e575e79172cb', 'f5c72696-19ad-4759-879f-89c8532ff238', '245EE66C-4BCC-424D-956B-34077A3AAB96')");
			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM GetGenericTransactions ('878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')");
			AssertEquals("Result should have row(s)", 1, result.Rows.Count);
			result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM GetGenericTransactions ('878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')");
			AssertEquals("Result should have row(s)", 1, result.Rows.Count);
			result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM GetGenericTransactions ('878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')");
			AssertEquals("Result should have row(s)", 1, result.Rows.Count);

			// Test: @AR_Control_Account
			// [AR / JNL] AG_PK = '05E814CE-284B-41C4-83CF-4B01C0B70E89' (ControlAccount = 1000.00.00, HeaderAccount = 1020.00.00)			
			TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_InvoiceDate, AH_DueDate, AH_InvoiceAmount, AH_OSTotal, AH_PostDate, AH_OH, AH_GC, AH_GB, AH_GE, AH_AG) 
VALUES(NEWID(), 'AR', 'JNL', 'May 25 2005  3:44:00:000PM', 'May 25 2005  3:44:00:000PM', -100.0000, -100.0000, 'May 25 2005  3:44:00:000PM', 'c72bfdd2-e264-4dd9-a5ab-3f75efd8569b', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC'
, '27a55065-ac88-4ec3-8bed-e575e79172cb', 'f5c72696-19ad-4759-879f-89c8532ff238', '05E814CE-284B-41C4-83CF-4B01C0B70E89')");
			result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM GetGenericTransactions ('878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')");
			AssertEquals("Result should have row(s)", 2, result.Rows.Count);

			#endregion

			#region GLFromHeaderBank
			TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.AccBankAccount (AB_PK, AB_Code, AB_RX_NKAccountCurrency, AB_AG, AB_GC, AB_AccountNum, AB_BSB)
VALUES('d4d40998-b563-4e6f-b1ed-8ea852e0f2fd', 'AAA', 'AUD', 'ac129d82-b88d-45ee-bce5-25592f734023', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '111111111','123456')

INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_InvoiceDate, AH_DueDate, AH_InvoiceAmount, AH_OSTotal, AH_PostDate, AH_OH, AH_GC, AH_GB, AH_GE, AH_AG, AH_AB) 
VALUES(NEWID(), 'AP', 'PAY', 'May 25 2005  3:44:00:000PM', 'May 25 2005  3:44:00:000PM', -100.0000, -100.0000, 'May 25 2005  3:44:00:000PM', 'c72bfdd2-e264-4dd9-a5ab-3f75efd8569b', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '27a55065-ac88-4ec3-8bed-e575e79172cb', 'f5c72696-19ad-4759-879f-89c8532ff238', 'ac129d82-b88d-45ee-bce5-25592f734023', 'd4d40998-b563-4e6f-b1ed-8ea852e0f2fd')");
			result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM GetGenericTransactions ('878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')");
			AssertEquals("Result should have row(s)", 3, result.Rows.Count);
			#endregion

			#region GLFromControlAccount ('TRF', 'CTR')
			// Test: @AP_Control_Account
			// [AP / TRF] AG_PK = '245EE66C-4BCC-424D-956B-34077A3AAB96' (ControlAccount = 2010.00.00, Header Account = 2020.00.00)			
			TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_InvoiceDate, AH_DueDate, AH_InvoiceAmount, AH_OSTotal, AH_PostDate, AH_OH, AH_GC, AH_GB, AH_GE, AH_AG, AH_AB) 
VALUES(NEWID(), 'AP', 'TRF', 'May 25 2005  3:44:00:000PM', 'May 25 2005  3:44:00:000PM', -100.0000, -100.0000, 'May 25 2005  3:44:00:000PM', 'c72bfdd2-e264-4dd9-a5ab-3f75efd8569b', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '27a55065-ac88-4ec3-8bed-e575e79172cb', 'f5c72696-19ad-4759-879f-89c8532ff238', '245EE66C-4BCC-424D-956B-34077A3AAB96', 'd4d40998-b563-4e6f-b1ed-8ea852e0f2fd')");
			result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM GetGenericTransactions ('878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')");
			AssertEquals("Result should have row(s)", 4, result.Rows.Count);

			// Test: @AR_Control_Account
			// [AR / TRF] AG_PK = '05E814CE-284B-41C4-83CF-4B01C0B70E89' (ControlAccount = 1000.00.00, HeaderAccount = 1020.00.00)
			TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_InvoiceDate, AH_DueDate, AH_InvoiceAmount, AH_OSTotal, AH_PostDate, AH_OH, AH_GC, AH_GB, AH_GE, AH_AG, AH_AB) 
VALUES(NEWID(), 'AR', 'TRF', 'May 25 2005  3:44:00:000PM', 'May 25 2005  3:44:00:000PM', -100.0000, -100.0000, 'May 25 2005  3:44:00:000PM', 'c72bfdd2-e264-4dd9-a5ab-3f75efd8569b', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '27a55065-ac88-4ec3-8bed-e575e79172cb', 'f5c72696-19ad-4759-879f-89c8532ff238', '05E814CE-284B-41C4-83CF-4B01C0B70E89', 'd4d40998-b563-4e6f-b1ed-8ea852e0f2fd')");
			result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM GetGenericTransactions ('878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')");
			AssertEquals("Result should have row(s)", 5, result.Rows.Count);

			#endregion

			#region GLFromControlAccount ('OVP', 'DSC', 'EXX')
			// Test: @AP_Control_Account
			// [AP / OVP] AG_PK = '245EE66C-4BCC-424D-956B-34077A3AAB96' (ControlAccount = 2010.00.00, HeaderAccount = 2020.00.00)
			TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_InvoiceDate, AH_DueDate, AH_InvoiceAmount, AH_OSTotal, AH_PostDate, AH_OH, AH_GC, AH_GB, AH_GE, AH_AG, AH_AB) 
VALUES(NEWID(), 'AP', 'OVP', 'May 25 2005  3:44:00:000PM', 'May 25 2005  3:44:00:000PM', -100.0000, -100.0000, 'May 25 2005  3:44:00:000PM', 'c72bfdd2-e264-4dd9-a5ab-3f75efd8569b', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '27a55065-ac88-4ec3-8bed-e575e79172cb', 'f5c72696-19ad-4759-879f-89c8532ff238', '245EE66C-4BCC-424D-956B-34077A3AAB96', 'd4d40998-b563-4e6f-b1ed-8ea852e0f2fd')");
			result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM GetGenericTransactions ('878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')");
			AssertEquals("Result should have row(s)", 6, result.Rows.Count);

			// Test: @AR_Control_Account
			// [AR / OVP] AG_PK = '05E814CE-284B-41C4-83CF-4B01C0B70E89' (ControlAccount = 1000.00.00, HeaderAccount = 1020.00.00)
			TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_InvoiceDate, AH_DueDate, AH_InvoiceAmount, AH_OSTotal, AH_PostDate, AH_OH, AH_GC, AH_GB, AH_GE, AH_AG, AH_AB) 
VALUES(NEWID(), 'AR', 'OVP', 'May 25 2005  3:44:00:000PM', 'May 25 2005  3:44:00:000PM', -100.0000, -100.0000, 'May 25 2005  3:44:00:000PM', 'c72bfdd2-e264-4dd9-a5ab-3f75efd8569b', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '27a55065-ac88-4ec3-8bed-e575e79172cb', 'f5c72696-19ad-4759-879f-89c8532ff238', '05E814CE-284B-41C4-83CF-4B01C0B70E89', 'd4d40998-b563-4e6f-b1ed-8ea852e0f2fd')");
			result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM GetGenericTransactions ('878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')");
			AssertEquals("Result should have row(s)", 7, result.Rows.Count);

			#endregion

			#region DirectReceiptPaymentLine
			// Test: @GST_OUTPUT_ACCOUNT
			// [CB / DRC] AG_PK = '010038a8-6368-4050-9586-d71d52ededf0' (ControlAccount = 3000.00.00, HeaderAccount = 3020.00.00)
			TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.AccBankAccount (AB_PK, AB_Code, AB_RX_NKAccountCurrency, AB_AG, AB_GC, AB_AccountNum, AB_BSB)
VALUES('240f20f7-b257-49f7-b791-bed96f4ee6a0', 'BBB', 'AUD', '010038a8-6368-4050-9586-d71d52ededf0', '878d7aca-ffc3-49fc-9710-969ca0c0f2ac', '222222222','123456')

INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_InvoiceDate, AH_DueDate, AH_InvoiceAmount, AH_OSTotal, AH_PostDate, AH_OH, AH_GC, AH_GB, AH_GE, AH_AG, AH_AB, AH_GSTAmount) 
VALUES('8EABF9FC-D984-4252-814F-4645DD817154', 'CB', 'DRC', 'May 25 2005  3:44:00:000PM', 'May 25 2005  3:44:00:000PM', -100.0000, -100.0000, 'May 25 2005  3:44:00:000PM', 'c72bfdd2-e264-4dd9-a5ab-3f75efd8569b', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '27a55065-ac88-4ec3-8bed-e575e79172cb', 'f5c72696-19ad-4759-879f-89c8532ff238', '010038a8-6368-4050-9586-d71d52ededf0', '240f20f7-b257-49f7-b791-bed96f4ee6a0', -10.000)

INSERT INTO dbo.AccTransactionLines (AL_PK, AL_AH, AL_AG, AL_GB, AL_GC, AL_GE, AL_LineType)
VALUES(NEWID(), '8EABF9FC-D984-4252-814F-4645DD817154', '010038A8-6368-4050-9586-D71D52EDEDF0', '27a55065-ac88-4ec3-8bed-e575e79172cb','878d7aca-ffc3-49fc-9710-969ca0c0f2ac', 'f5c72696-19ad-4759-879f-89c8532ff238', 'DRC')
");
			result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM GetGenericTransactions ('878D7ACA-FFC3-49FC-9710-969CA0C0F2AC') WHERE VT_Type = 'DRC'");
			CombineAssertions(() =>
			{
				AssertEquals("Result should have row(s)", 1, result.Rows.Count);
				AssertEquals("Row 0: VT_GLAccount", "3020.00.00", result.Rows[0]["VT_GLAccount"]);
				AssertEquals("Row 0: VT_GSTGLAccount", "3000.00.00", result.Rows[0]["VT_GSTGLAccount"]);
			});

			// Test: @GST_INPUT_ACCOUNT
			// [CB / DPY] AG_PK = 'B280AA9A-CF21-4CA3-8384-DAED61DB6E63' (ControlAccount = 4099.00.00, HeaderAccount = 4499.00.00)
			TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.AccBankAccount (AB_PK, AB_Code, AB_RX_NKAccountCurrency, AB_AG, AB_GC, AB_AccountNum, AB_BSB)
VALUES('1b9eeced-cf25-4b5d-98c9-0e7e38bbb974', 'CCC', 'AUD', 'b280aa9a-cf21-4ca3-8384-daed61db6e63', '878d7aca-ffc3-49fc-9710-969ca0c0f2ac', '333333333','123456')

INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_InvoiceDate, AH_DueDate, AH_InvoiceAmount, AH_OSTotal, AH_PostDate, AH_OH, AH_GC, AH_GB, AH_GE, AH_AG, AH_AB, AH_GSTAmount) 
VALUES('1FDECA03-76C3-4885-AE61-510332CB9583', 'CB', 'DPY', 'May 25 2005  3:44:00:000PM', 'May 25 2005  3:44:00:000PM', -100.0000, -100.0000, 'May 25 2005  3:44:00:000PM', 'c72bfdd2-e264-4dd9-a5ab-3f75efd8569b', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '27a55065-ac88-4ec3-8bed-e575e79172cb', 'f5c72696-19ad-4759-879f-89c8532ff238', 'b280aa9a-cf21-4ca3-8384-daed61db6e63', '1b9eeced-cf25-4b5d-98c9-0e7e38bbb974', -10.000)

INSERT INTO dbo.AccTransactionLines (AL_PK, AL_AH, AL_AG, AL_GB, AL_GC, AL_GE, AL_LineType)
VALUES(NEWID(), '1FDECA03-76C3-4885-AE61-510332CB9583', 'b280aa9a-cf21-4ca3-8384-daed61db6e63', '27a55065-ac88-4ec3-8bed-e575e79172cb','878d7aca-ffc3-49fc-9710-969ca0c0f2ac', 'f5c72696-19ad-4759-879f-89c8532ff238', 'DPY')
");
			result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM GetGenericTransactions ('878D7ACA-FFC3-49FC-9710-969CA0C0F2AC') WHERE VT_Type = 'DPY'");
			CombineAssertions(() =>
			{
				AssertEquals("Result should have row(s)", 1, result.Rows.Count);
				AssertEquals("Row 0: VT_GLAccount", "4499.00.00", result.Rows[0]["VT_GLAccount"]);
				AssertEquals("Row 0: VT_GSTGLAccount", "4099.00.00", result.Rows[0]["VT_GSTGLAccount"]);
			});

			// Test: @GST_OUTPUT_ACCOUNT
			// [CB / DRC] AG_PK = '010038A8-6368-4050-9586-D71D52EDEDF0' (ControlAccount = 3000.00.00, HeaderAccount = 3020.00.00)
			// [CB / DRC] AG_PK = 'B280AA9A-CF21-4CA3-8384-DAED61DB6E63' (2ndLineAccount = 4499.00.00)
			TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_InvoiceDate, AH_DueDate, AH_InvoiceAmount, AH_OSTotal, AH_PostDate, AH_OH, AH_GC, AH_GB, AH_GE, AH_AG, AH_AB, AH_TransactionNum, AH_GSTAmount) 
VALUES('01f92cd2-eecd-470e-85a8-89914b4c4f44', 'CB', 'DRC', 'May 25 2015  3:44:00:000PM', 'May 25 2015  3:44:00:000PM', -100.0000, -100.0000, 'May 25 2015  3:44:00:000PM', 'c72bfdd2-e264-4dd9-a5ab-3f75efd8569b', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '27a55065-ac88-4ec3-8bed-e575e79172cb', 'f5c72696-19ad-4759-879f-89c8532ff238', '010038A8-6368-4050-9586-D71D52EDEDF0', 'd4d40998-b563-4e6f-b1ed-8ea852e0f2fd', 'Test1', -10.000)

INSERT INTO dbo.AccTransactionLines (AL_PK, AL_AH, AL_AG, AL_GB, AL_GC, AL_GE, AL_LineType)
VALUES(NEWID(), '01f92cd2-eecd-470e-85a8-89914b4c4f44', '010038A8-6368-4050-9586-D71D52EDEDF0', '27a55065-ac88-4ec3-8bed-e575e79172cb','878d7aca-ffc3-49fc-9710-969ca0c0f2ac', 'f5c72696-19ad-4759-879f-89c8532ff238', 'DRC')

INSERT INTO dbo.AccTransactionLines (AL_PK, AL_AH, AL_AG, AL_GB, AL_GC, AL_GE, AL_LineType)
VALUES(NEWID(), '01f92cd2-eecd-470e-85a8-89914b4c4f44', 'B280AA9A-CF21-4CA3-8384-DAED61DB6E63', '27a55065-ac88-4ec3-8bed-e575e79172cb','878d7aca-ffc3-49fc-9710-969ca0c0f2ac', 'f5c72696-19ad-4759-879f-89c8532ff238', 'DRC')
");
			result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM GetGenericTransactions ('878D7ACA-FFC3-49FC-9710-969CA0C0F2AC') WHERE VT_Type = 'DRC' AND VT_PostDate = 'May 25 2015  3:44:00:000PM' ORDER BY VT_GLAccount");
			CombineAssertions(() =>
			{
				AssertEquals("Result should have row(s)", 2, result.Rows.Count);
				AssertEquals("Row 0: VT_GLAccount", "3020.00.00", result.Rows[0]["VT_GLAccount"]);
				AssertEquals("Row 0: VT_GSTGLAccount", "3000.00.00", result.Rows[0]["VT_GSTGLAccount"]);
				AssertEquals("Row 1: VT_GLAccount", "4499.00.00", result.Rows[1]["VT_GLAccount"]);
				AssertEquals("Row 1: VT_GSTGLAccount", "3000.00.00", result.Rows[0]["VT_GSTGLAccount"]);
			});

			// Test: @GST_INPUT_ACCOUNT
			// [CB / DPY] AG_PK = 'B280AA9A-CF21-4CA3-8384-DAED61DB6E63' (ControlAccount = 4099.00.00, HeaderAccount = 4499.00.00)
			// [CB / DPY] AG_PK = '010038A8-6368-4050-9586-D71D52EDEDF0' (2ndLineAccount = 3020.00.00)
			TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_InvoiceDate, AH_DueDate, AH_InvoiceAmount, AH_OSTotal, AH_PostDate, AH_OH, AH_GC, AH_GB, AH_GE, AH_AG, AH_AB, AH_TransactionNum, AH_GSTAmount) 
VALUES('951033b3-1880-4589-be25-a3368d2e164c', 'CB', 'DPY', 'May 25 2015  3:44:00:000PM', 'May 25 2015  3:44:00:000PM', -100.0000, -100.0000, 'May 25 2015  3:44:00:000PM', 'c72bfdd2-e264-4dd9-a5ab-3f75efd8569b', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '27a55065-ac88-4ec3-8bed-e575e79172cb', 'f5c72696-19ad-4759-879f-89c8532ff238', 'B280AA9A-CF21-4CA3-8384-DAED61DB6E63', 'd4d40998-b563-4e6f-b1ed-8ea852e0f2fd', 'Test1-1', -10.000)

INSERT INTO dbo.AccTransactionLines (AL_PK, AL_AH, AL_AG, AL_GB, AL_GC, AL_GE, AL_LineType)
VALUES(NEWID(), '951033b3-1880-4589-be25-a3368d2e164c', 'b280aa9a-cf21-4ca3-8384-daed61db6e63', '27a55065-ac88-4ec3-8bed-e575e79172cb','878d7aca-ffc3-49fc-9710-969ca0c0f2ac', 'f5c72696-19ad-4759-879f-89c8532ff238', 'DPY')

INSERT INTO dbo.AccTransactionLines (AL_PK, AL_AH, AL_AG, AL_GB, AL_GC, AL_GE, AL_LineType)
VALUES(NEWID(), '951033b3-1880-4589-be25-a3368d2e164c', '010038A8-6368-4050-9586-D71D52EDEDF0', '27a55065-ac88-4ec3-8bed-e575e79172cb','878d7aca-ffc3-49fc-9710-969ca0c0f2ac', 'f5c72696-19ad-4759-879f-89c8532ff238', 'DPY')
");
			result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM GetGenericTransactions ('878D7ACA-FFC3-49FC-9710-969CA0C0F2AC') WHERE VT_Type = 'DPY' AND VT_PostDate = 'May 25 2015  3:44:00:000PM' ORDER BY VT_GLAccount");
			CombineAssertions(() =>
			{
				AssertEquals("Result should have row(s)", 2, result.Rows.Count);
				AssertEquals("Row 0: VT_GLAccount", "3020.00.00", result.Rows[0]["VT_GLAccount"]);
				AssertEquals("Row 0: VT_GSTGLAccount", "4099.00.00", result.Rows[0]["VT_GSTGLAccount"]);
				AssertEquals("Row 1: VT_GLAccount", "4499.00.00", result.Rows[1]["VT_GLAccount"]);
				AssertEquals("Row 1: VT_GSTGLAccount", "4099.00.00", result.Rows[0]["VT_GSTGLAccount"]);
			});

			#endregion

			#region InvCrdAdj
			// Test: @AP_Control_Account
			// [AP / INV] AG_PK = '245ee66c-4bcc-424d-956b-34077a3aab96' (ControlAccount = 2010.00.00, HeaderAccount = 2020.00.00)
			TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_InvoiceDate, AH_DueDate, AH_InvoiceAmount, AH_OSTotal, AH_PostDate, AH_OH, AH_GC, AH_GB, AH_GE, AH_AG, AH_AB, AH_TransactionNum) 
VALUES('48c3a58a-af86-477b-8513-4769c37792fa', 'AP', 'INV', 'May 25 2005  3:44:00:000PM', 'May 25 2005  3:44:00:000PM', -100.0000, -100.0000, 'May 25 2005  3:44:00:000PM', 'c72bfdd2-e264-4dd9-a5ab-3f75efd8569b', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '27a55065-ac88-4ec3-8bed-e575e79172cb', 'f5c72696-19ad-4759-879f-89c8532ff238', '245ee66c-4bcc-424d-956b-34077a3aab96', 'd4d40998-b563-4e6f-b1ed-8ea852e0f2fd', 'Test2')

INSERT INTO dbo.AccTransactionLines (AL_PK, AL_AH, AL_AG, AL_GB, AL_GC, AL_GE, AL_LineType)
VALUES(NEWID(), '48c3a58a-af86-477b-8513-4769c37792fa', '245ee66c-4bcc-424d-956b-34077a3aab96', '27a55065-ac88-4ec3-8bed-e575e79172cb','878d7aca-ffc3-49fc-9710-969ca0c0f2ac', 'f5c72696-19ad-4759-879f-89c8532ff238', 'REV')");
			result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM GetGenericTransactions ('878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')");
			AssertEquals("Result should have row(s)", 14, result.Rows.Count);

			// Test: @AR_Control_Account
			// [AR / INV] AG_PK = '05e814ce-284b-41c4-83cf-4b01c0b70e89' (ControlAccount = 1000.00.00, HeaderAccount = 1020.00.00)
			TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_InvoiceDate, AH_DueDate, AH_InvoiceAmount, AH_OSTotal, AH_PostDate, AH_OH, AH_GC, AH_GB, AH_GE, AH_AG, AH_AB, AH_TransactionNum) 
VALUES('231bfa6d-b75f-4834-bf36-51dc4358aa56', 'AR', 'INV', 'May 25 2005  3:44:00:000PM', 'May 25 2005  3:44:00:000PM', -100.0000, -100.0000, 'May 25 2005  3:44:00:000PM', 'c72bfdd2-e264-4dd9-a5ab-3f75efd8569b', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '27a55065-ac88-4ec3-8bed-e575e79172cb', 'f5c72696-19ad-4759-879f-89c8532ff238', '05e814ce-284b-41c4-83cf-4b01c0b70e89', 'd4d40998-b563-4e6f-b1ed-8ea852e0f2fd', 'Test3')

INSERT INTO dbo.AccTransactionLines (AL_PK, AL_AH, AL_AG, AL_GB, AL_GC, AL_GE, AL_LineType)
VALUES(NEWID(), '231bfa6d-b75f-4834-bf36-51dc4358aa56', '05e814ce-284b-41c4-83cf-4b01c0b70e89', '27a55065-ac88-4ec3-8bed-e575e79172cb','878d7aca-ffc3-49fc-9710-969ca0c0f2ac', 'f5c72696-19ad-4759-879f-89c8532ff238', 'REV')");
			result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM GetGenericTransactions ('878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')");
			AssertEquals("Result should have row(s)", 15, result.Rows.Count);

			#endregion

			#region JRJournal
			// Test: @JOB_REVENUE_JOURNAL_CONTROL_ACCOUNT
			// [JC / JRJ] AG_PK = '25448216-975b-4910-9560-ff9249a9ce2f' (ControlAccount = 5000.00.00, HeaderAccount = 5100.00.00)
			TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_InvoiceDate, AH_DueDate, AH_InvoiceAmount, AH_OSTotal, AH_PostDate, AH_OH, AH_GC, AH_GB, AH_GE, AH_AG, AH_AB, AH_TransactionNum) 
VALUES('283a89f9-4afb-45b5-9799-cd5e263223c5', 'JC', 'JRJ', 'May 25 2005  3:44:00:000PM', 'May 25 2005  3:44:00:000PM', -100.0000, -100.0000, 'May 25 2005  3:44:00:000PM', 'c72bfdd2-e264-4dd9-a5ab-3f75efd8569b', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '27a55065-ac88-4ec3-8bed-e575e79172cb', 'f5c72696-19ad-4759-879f-89c8532ff238', '25448216-975b-4910-9560-ff9249a9ce2f', 'd4d40998-b563-4e6f-b1ed-8ea852e0f2fd', 'Test4')

INSERT INTO dbo.AccTransactionLines (AL_PK, AL_AH, AL_AG, AL_GB, AL_GC, AL_GE, AL_LineType)
VALUES(NEWID(), '283a89f9-4afb-45b5-9799-cd5e263223c5', '25448216-975b-4910-9560-ff9249a9ce2f', '27a55065-ac88-4ec3-8bed-e575e79172cb','878d7aca-ffc3-49fc-9710-969ca0c0f2ac', 'f5c72696-19ad-4759-879f-89c8532ff238', 'REV')");
			result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM GetGenericTransactions ('878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')");
			AssertEquals("Result should have row(s)", 16, result.Rows.Count);

			#endregion

			#region WIPSJournal
			// Test: @ACCRUED_REVENUE_ACCOUNT
			// [JC / WIP] AG_PK = '865d227b-f6d0-49a2-963a-48331e168858' (ControlAccount = 6000.00.00, HeaderAccount = 6010.00.00)
			TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.AccTransactionLines (AL_PK, AL_AH, AL_AG, AL_GB, AL_GC, AL_GE, AL_AC, AL_LineType)
VALUES(NEWID(), '48c3a58a-af86-477b-8513-4769c37792fa', '865d227b-f6d0-49a2-963a-48331e168858', '27a55065-ac88-4ec3-8bed-e575e79172cb','878d7aca-ffc3-49fc-9710-969ca0c0f2ac', 'f5c72696-19ad-4759-879f-89c8532ff238', '8319278C-E149-4895-BC52-114E69E069D9', 'WIP')");
			result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM GetGenericTransactions ('878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')");
			AssertEquals("Result should have row(s)", 18, result.Rows.Count);

			// Test: @ACCRUED_COST_ACCOUNT
			// [JC / ACR] AG_PK = '046d1569-defd-499a-af27-c1b9cbdae291' (ControlAccount = 7000.00.00, HeaderAccount = 7100.00.00)
			TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.AccTransactionLines (AL_PK, AL_AH, AL_AG, AL_GB, AL_GC, AL_GE, AL_AC, AL_LineType)
VALUES(NEWID(), '48c3a58a-af86-477b-8513-4769c37792fa', '046d1569-defd-499a-af27-c1b9cbdae291', '27a55065-ac88-4ec3-8bed-e575e79172cb','878d7aca-ffc3-49fc-9710-969ca0c0f2ac', 'f5c72696-19ad-4759-879f-89c8532ff238', '8319278C-E149-4895-BC52-114E69E069D9', 'ACR')");
			result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM GetGenericTransactions ('878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')");
			AssertEquals("Result should have row(s)", 20, result.Rows.Count);

			#endregion

			#region CFX
			// Test: @GL_CFX_ACCOUNT
			// [JC / WIP] AG_PK = 'cf313c85-1ba2-41ca-b44e-5947af24054f' (ControlAccount = 8000.00.00, HeaderAccount = 8010.00.00)
			TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_InvoiceDate, AH_DueDate, AH_InvoiceAmount, AH_OSTotal, AH_PostDate, AH_OH, AH_GC, AH_GB, AH_GE, AH_AG, AH_AB, AH_TransactionNum) 
VALUES('8d5477e1-123d-4e1d-bb68-11a0a78bab6c', 'JC', 'JNL', 'May 25 2005  3:44:00:000PM', 'May 25 2005  3:44:00:000PM', -100.0000, -100.0000, 'May 25 2005  3:44:00:000PM', 'c72bfdd2-e264-4dd9-a5ab-3f75efd8569b', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '27a55065-ac88-4ec3-8bed-e575e79172cb', 'f5c72696-19ad-4759-879f-89c8532ff238', 'cf313c85-1ba2-41ca-b44e-5947af24054f', 'd4d40998-b563-4e6f-b1ed-8ea852e0f2fd', 'Test2')

INSERT INTO dbo.AccTransactionLines (AL_PK, AL_AH, AL_AG, AL_GB, AL_GC, AL_GE, AL_LineType)
VALUES(NEWID(), '8d5477e1-123d-4e1d-bb68-11a0a78bab6c', 'cf313c85-1ba2-41ca-b44e-5947af24054f', '27a55065-ac88-4ec3-8bed-e575e79172cb','878d7aca-ffc3-49fc-9710-969ca0c0f2ac', 'f5c72696-19ad-4759-879f-89c8532ff238', 'WIP')");
			result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM GetGenericTransactions ('878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')");
			AssertEquals("Result should have row(s)", 21, result.Rows.Count);

			#endregion

			#region GLJournal
			TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_InvoiceDate, AH_DueDate, AH_InvoiceAmount, AH_OSTotal, AH_PostDate, AH_OH, AH_GC, AH_GB, AH_GE, AH_AG, AH_AB, AH_TransactionNum) 
VALUES('ab530eca-4866-4691-b409-8f13682aa798', 'GL', 'GJL', 'May 25 2005  3:44:00:000PM', 'May 25 2005  3:44:00:000PM', -100.0000, -100.0000, 'May 25 2005  3:44:00:000PM', 'c72bfdd2-e264-4dd9-a5ab-3f75efd8569b', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '27a55065-ac88-4ec3-8bed-e575e79172cb', 'f5c72696-19ad-4759-879f-89c8532ff238', 'ac129d82-b88d-45ee-bce5-25592f734023', 'd4d40998-b563-4e6f-b1ed-8ea852e0f2fd', 'Test2')

INSERT INTO dbo.AccTransactionLines (AL_PK, AL_AH, AL_AG, AL_GB, AL_GC, AL_GE, AL_LineType)
VALUES(NEWID(), 'ab530eca-4866-4691-b409-8f13682aa798', 'ac129d82-b88d-45ee-bce5-25592f734023', '27a55065-ac88-4ec3-8bed-e575e79172cb','878d7aca-ffc3-49fc-9710-969ca0c0f2ac', 'f5c72696-19ad-4759-879f-89c8532ff238', 'GJL')");
			result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM GetGenericTransactions ('878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')");
			AssertEquals("Result should have row(s)", 22, result.Rows.Count);
			#endregion
		}

		public void TestSystemCreateUserFilter()
		{
			var txnHeaderPK1 = Guid.NewGuid();
			var txnHeaderPK2 = Guid.NewGuid();
			var txnLinePK2 = Guid.NewGuid();

			TestConnection.ExecuteNonQuery($@"
INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_InvoiceDate, AH_DueDate, AH_InvoiceAmount, AH_OSTotal,
AH_PostDate, AH_OH, AH_GC, AH_GB, AH_GE, AH_AG, AH_SystemCreateUser)
VALUES('{txnHeaderPK1}', 'AP', 'JNL', 'May 25 2005  3:44:00:000PM', 'May 25 2005  3:44:00:000PM', -100.0000, -100.0000,
'May 25 2005  3:44:00:000PM', 'c72bfdd2-e264-4dd9-a5ab-3f75efd8569b', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '27a55065-ac88-4ec3-8bed-e575e79172cb', 'f5c72696-19ad-4759-879f-89c8532ff238',
'245EE66C-4BCC-424D-956B-34077A3AAB96', 'UKJ')");

			var rows = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM GetGenericTransactions ('878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')").AsEnumerable();
			AssertNotNull(rows);
			AssertEquals("UKJ", rows.FirstOrDefault().Field<string>("VT_SystemCreateUser"));

			TestConnection.ExecuteNonQuery($@"
INSERT INTO dbo.AccTransactionHeader(AH_PK, AH_Ledger, AH_TransactionType, AH_InvoiceDate, AH_DueDate, AH_InvoiceAmount, AH_OSTotal, AH_PostDate, AH_OH, AH_GC, AH_GB, AH_GE, AH_AG, AH_TransactionNum)
VALUES('{txnHeaderPK2}', 'AP', 'INV', 'May 25 2005  3:44:00:000PM', 'May 25 2005  3:44:00:000PM', -100.0000, -100.0000, 'May 25 2005  3:44:00:000PM', 'c72bfdd2-e264-4dd9-a5ab-3f75efd8569b',
'878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '27a55065-ac88-4ec3-8bed-e575e79172cb', 'f5c72696-19ad-4759-879f-89c8532ff238', '245ee66c-4bcc-424d-956b-34077a3aab96', 'Test2')

INSERT INTO dbo.AccTransactionLines (AL_PK, AL_AH, AL_AG, AL_GB, AL_GC, AL_GE, AL_AC, AL_LineType, AL_SystemCreateUser)
VALUES('{txnLinePK2}', '{txnHeaderPK2}', '865d227b-f6d0-49a2-963a-48331e168858', '27a55065-ac88-4ec3-8bed-e575e79172cb','878d7aca-ffc3-49fc-9710-969ca0c0f2ac',
'f5c72696-19ad-4759-879f-89c8532ff238', '8319278C-E149-4895-BC52-114E69E069D9', 'WIP', 'E')");

			rows = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM GetGenericTransactions ('878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')").AsEnumerable();
			AssertNotNull(rows);
			AssertEquals("UKJ", rows.FirstOrDefault(x => x.Field<Guid>("VT_PK") == txnHeaderPK1).Field<string>("VT_SystemCreateUser"));
			AssertEquals(string.Empty, rows.FirstOrDefault(x => x.Field<Guid>("VT_PK") == txnHeaderPK2).Field<string>("VT_SystemCreateUser"));
			AssertEquals("E", rows.FirstOrDefault(x => x.Field<Guid>("VT_PK") == txnLinePK2).Field<string>("VT_SystemCreateUser"));
		}

		public void TestSubAccountsColumn()
		{
			var helper = new TestDbHelper(TestConnection);

			#region GLFromHeader

			// Test: @AP_Control_Account
			// [AP / JNL] AG_PK = '245EE66C-4BCC-424D-956B-34077A3AAB96' (ControlAccount = 2010.00.00, HeaderAccount = 2020.00.00)
			var transactionHeaderPK = helper.InsertTransactionHeader("AP", "JNL", "AA1", 10M, DateTime.Now, new Guid("27a55065-ac88-4ec3-8bed-e575e79172cb"),
				new Guid("f5c72696-19ad-4759-879f-89c8532ff238"), glAccountPK: new Guid("245EE66C-4BCC-424D-956B-34077A3AAB96"));

			helper.InsertAccTransactionHeaderSubAccount(transactionHeaderPK, "OH", new Guid("0daab61b-255e-4ad7-afc5-4e7b03c3bda1"));
			helper.InsertAccTransactionHeaderSubAccount(transactionHeaderPK, "GG", new Guid("55896e13-12be-4fd4-ac94-2956795a5be2"));
			helper.InsertAccGLHeaderSubAccount(new Guid("245EE66C-4BCC-424D-956B-34077A3AAB96"), true, "OH");
			helper.InsertAccGLHeaderSubAccount(new Guid("245EE66C-4BCC-424D-956B-34077A3AAB96"), true, "GG");
			helper.InsertAccGLHeaderSubAccount(new Guid("245EE66C-4BCC-424D-956B-34077A3AAB96"), false, "GS");
			helper.InsertAccGLHeaderSubAccount(new Guid("245EE66C-4BCC-424D-956B-34077A3AAB96"), false, "AR");

			var rows = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM GetGenericTransactions ('878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')").AsEnumerable();
			AssertNotNull(rows);
			var row = rows.FirstOrDefault(x => x.Field<Guid>("VT_PK") == transactionHeaderPK);
			AssertEquals("ORG: ABIGAS, SEG: , STR: , SGP: PMG", row.Field<string>("VT_SubAccounts"));
			AssertEquals(new Guid("245EE66C-4BCC-424D-956B-34077A3AAB96"), row.Field<Guid>("VT_AGForSubAccount"));
			AssertEquals(transactionHeaderPK, row.Field<Guid>("VT_SubAccountParentPK"));

			// Test: @AR_Control_Account
			// [AR / JNL] AG_PK = '05E814CE-284B-41C4-83CF-4B01C0B70E89' (ControlAccount = 1000.00.00, HeaderAccount = 1020.00.00)
			transactionHeaderPK = helper.InsertTransactionHeader("AR", "JNL", "AA2", 10M, DateTime.Now, new Guid("27a55065-ac88-4ec3-8bed-e575e79172cb"),
				new Guid("f5c72696-19ad-4759-879f-89c8532ff238"), glAccountPK: new Guid("05E814CE-284B-41C4-83CF-4B01C0B70E89"));

			rows = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM GetGenericTransactions ('878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')").AsEnumerable();
			AssertNotNull(rows);
			row = rows.FirstOrDefault(x => x.Field<Guid>("VT_PK") == transactionHeaderPK);
			AssertEquals(null, row.Field<string>("VT_SubAccounts"));
			AssertEquals(new Guid("05E814CE-284B-41C4-83CF-4B01C0B70E89"), row.Field<Guid>("VT_AGForSubAccount"));
			AssertEquals(transactionHeaderPK, row.Field<Guid>("VT_SubAccountParentPK"));

			#endregion

			#region GLFromHeaderBank

			var bankAccount = helper.InsertBankAccount("AAA", new Guid("ac129d82-b88d-45ee-bce5-25592f734023"));
			transactionHeaderPK = helper.InsertTransactionHeader("AP", "PAY", "AA3", 10M, DateTime.Now, new Guid("27a55065-ac88-4ec3-8bed-e575e79172cb"),
				new Guid("f5c72696-19ad-4759-879f-89c8532ff238"), bankAccount, glAccountPK: new Guid("ac129d82-b88d-45ee-bce5-25592f734023"));

			rows = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM GetGenericTransactions ('878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')").AsEnumerable();
			AssertNotNull(rows);
			row = rows.FirstOrDefault(x => x.Field<Guid>("VT_PK") == transactionHeaderPK);
			AssertEquals("", row.Field<string>("VT_SubAccounts"));
			AssertEquals(DBNull.Value, row["VT_AGForSubAccount"]);
			AssertEquals(DBNull.Value, row["VT_SubAccountParentPK"]);

			#endregion

			#region GLFromControlAccount ('TRF', 'CTR')

			// Test: @AP_Control_Account
			// [AP / TRF] AG_PK = '245EE66C-4BCC-424D-956B-34077A3AAB96' (ControlAccount = 2010.00.00, Header Account = 2020.00.00)
			transactionHeaderPK = helper.InsertTransactionHeader("AP", "TRF", "AA4", 10M, DateTime.Now, new Guid("27a55065-ac88-4ec3-8bed-e575e79172cb"),
				new Guid("f5c72696-19ad-4759-879f-89c8532ff238"), bankAccount, glAccountPK: new Guid("245EE66C-4BCC-424D-956B-34077A3AAB96"));

			rows = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM GetGenericTransactions ('878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')").AsEnumerable();
			AssertNotNull(rows);
			row = rows.FirstOrDefault(x => x.Field<Guid>("VT_PK") == transactionHeaderPK);
			AssertEquals("", row.Field<string>("VT_SubAccounts"));
			AssertEquals(DBNull.Value, row["VT_AGForSubAccount"]);
			AssertEquals(DBNull.Value, row["VT_SubAccountParentPK"]);

			// Test: @AR_Control_Account
			// [AR / TRF] AG_PK = '05E814CE-284B-41C4-83CF-4B01C0B70E89' (ControlAccount = 1000.00.00, HeaderAccount = 1020.00.00)
			transactionHeaderPK = helper.InsertTransactionHeader("AR", "TRF", "AA5", 10M, DateTime.Now, new Guid("27a55065-ac88-4ec3-8bed-e575e79172cb"),
				new Guid("f5c72696-19ad-4759-879f-89c8532ff238"), bankAccount, glAccountPK: new Guid("05E814CE-284B-41C4-83CF-4B01C0B70E89"));

			rows = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM GetGenericTransactions ('878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')").AsEnumerable();
			AssertNotNull(rows);
			row = rows.FirstOrDefault(x => x.Field<Guid>("VT_PK") == transactionHeaderPK);
			AssertEquals("", row.Field<string>("VT_SubAccounts"));
			AssertEquals(DBNull.Value, row["VT_AGForSubAccount"]);
			AssertEquals(DBNull.Value, row["VT_SubAccountParentPK"]);

			#endregion

			#region GLFromControlAccount ('OVP', 'DSC', 'EXX')

			// Test: @AP_Control_Account
			// [AP / OVP] AG_PK = '245EE66C-4BCC-424D-956B-34077A3AAB96' (ControlAccount = 2010.00.00, HeaderAccount = 2020.00.00)
			transactionHeaderPK = helper.InsertTransactionHeader("AP", "OVP", "AA6", 10M, DateTime.Now, new Guid("27a55065-ac88-4ec3-8bed-e575e79172cb"),
				new Guid("f5c72696-19ad-4759-879f-89c8532ff238"), bankAccount, glAccountPK: new Guid("245EE66C-4BCC-424D-956B-34077A3AAB96"));

			rows = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM GetGenericTransactions ('878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')").AsEnumerable();
			AssertNotNull(rows);
			row = rows.FirstOrDefault(x => x.Field<Guid>("VT_PK") == transactionHeaderPK);
			AssertEquals("", row.Field<string>("VT_SubAccounts"));
			AssertEquals(DBNull.Value, row["VT_AGForSubAccount"]);
			AssertEquals(DBNull.Value, row["VT_SubAccountParentPK"]);

			// Test: @AR_Control_Account
			// [AR / OVP] AG_PK = '05E814CE-284B-41C4-83CF-4B01C0B70E89' (ControlAccount = 1000.00.00, HeaderAccount = 1020.00.00)
			transactionHeaderPK = helper.InsertTransactionHeader("AR", "OVP", "AA7", 10M, DateTime.Now, new Guid("27a55065-ac88-4ec3-8bed-e575e79172cb"),
				new Guid("f5c72696-19ad-4759-879f-89c8532ff238"), bankAccount, glAccountPK: new Guid("05E814CE-284B-41C4-83CF-4B01C0B70E89"));

			rows = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM GetGenericTransactions ('878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')").AsEnumerable();
			AssertNotNull(rows);
			row = rows.FirstOrDefault(x => x.Field<Guid>("VT_PK") == transactionHeaderPK);
			AssertEquals("", row.Field<string>("VT_SubAccounts"));
			AssertEquals(DBNull.Value, row["VT_AGForSubAccount"]);
			AssertEquals(DBNull.Value, row["VT_SubAccountParentPK"]);

			#endregion

			#region DirectReceiptPaymentLine

			// Test: @GST_OUTPUT_ACCOUNT
			// [CB / DRC] AG_PK = '010038a8-6368-4050-9586-d71d52ededf0' (ControlAccount = 3000.00.00, HeaderAccount = 3020.00.00)
			bankAccount = helper.InsertBankAccount("BBB", new Guid("010038a8-6368-4050-9586-d71d52ededf0"));
			transactionHeaderPK = helper.InsertTransactionHeader("CB", "DRC", "AA8", 10M, DateTime.Now, new Guid("27a55065-ac88-4ec3-8bed-e575e79172cb"),
				new Guid("f5c72696-19ad-4759-879f-89c8532ff238"), bankAccount, glAccountPK: new Guid("010038a8-6368-4050-9586-d71d52ededf0"));
			var transactionLinePK = helper.InsertTransactionLine(transactionHeaderPK, null, null, new Guid("010038A8-6368-4050-9586-D71D52EDEDF0"), new Guid("27a55065-ac88-4ec3-8bed-e575e79172cb"), new Guid("f5c72696-19ad-4759-879f-89c8532ff238"), null, 10M, "DRC", null, null);

			helper.InsertAccTransactionLineSubAccount(transactionLinePK, "OH", new Guid("0daab61b-255e-4ad7-afc5-4e7b03c3bda1"));
			helper.InsertAccTransactionLineSubAccount(transactionLinePK, "GG", new Guid("55896e13-12be-4fd4-ac94-2956795a5be2"));
			helper.InsertAccGLHeaderSubAccount(new Guid("010038a8-6368-4050-9586-d71d52ededf0"), true, "OH");
			helper.InsertAccGLHeaderSubAccount(new Guid("010038a8-6368-4050-9586-d71d52ededf0"), true, "GG");
			helper.InsertAccGLHeaderSubAccount(new Guid("010038a8-6368-4050-9586-d71d52ededf0"), false, "GS");
			helper.InsertAccGLHeaderSubAccount(new Guid("010038a8-6368-4050-9586-d71d52ededf0"), false, "AR");

			rows = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM GetGenericTransactions ('878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')").AsEnumerable();
			AssertNotNull(rows);
			row = rows.FirstOrDefault(x => x.Field<Guid>("VT_PK") == transactionHeaderPK);
			AssertEquals("ORG: ABIGAS, SEG: , STR: , SGP: PMG", row.Field<string>("VT_SubAccounts"));
			AssertEquals(new Guid("010038a8-6368-4050-9586-d71d52ededf0"), row.Field<Guid>("VT_AGForSubAccount"));
			AssertEquals(transactionLinePK, row.Field<Guid>("VT_SubAccountParentPK"));

			// Test: @GST_INPUT_ACCOUNT
			// [CB / DPY] AG_PK = 'B280AA9A-CF21-4CA3-8384-DAED61DB6E63' (ControlAccount = 4099.00.00, HeaderAccount = 4499.00.00)
			bankAccount = helper.InsertBankAccount("CCC", new Guid("b280aa9a-cf21-4ca3-8384-daed61db6e63"));
			transactionHeaderPK = helper.InsertTransactionHeader("CB", "DPY", "AA9", 10M, DateTime.Now, new Guid("27a55065-ac88-4ec3-8bed-e575e79172cb"),
				new Guid("f5c72696-19ad-4759-879f-89c8532ff238"), bankAccount, glAccountPK: new Guid("b280aa9a-cf21-4ca3-8384-daed61db6e63"));
			transactionLinePK = helper.InsertTransactionLine(transactionHeaderPK, null, null, new Guid("b280aa9a-cf21-4ca3-8384-daed61db6e63"), new Guid("27a55065-ac88-4ec3-8bed-e575e79172cb"), new Guid("f5c72696-19ad-4759-879f-89c8532ff238"), null, 10M, "DPY", null, null);

			helper.InsertAccTransactionLineSubAccount(transactionLinePK, "OH", new Guid("0daab61b-255e-4ad7-afc5-4e7b03c3bda1"));
			helper.InsertAccGLHeaderSubAccount(new Guid("B280AA9A-CF21-4CA3-8384-DAED61DB6E63"), true, "OH");
			helper.InsertAccGLHeaderSubAccount(new Guid("B280AA9A-CF21-4CA3-8384-DAED61DB6E63"), true, "GG");
			helper.InsertAccGLHeaderSubAccount(new Guid("B280AA9A-CF21-4CA3-8384-DAED61DB6E63"), false, "GS");

			rows = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM GetGenericTransactions ('878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')").AsEnumerable();
			AssertNotNull(rows);
			row = rows.FirstOrDefault(x => x.Field<Guid>("VT_PK") == transactionHeaderPK);
			AssertEquals("ORG: ABIGAS, STR: , SGP: ", row.Field<string>("VT_SubAccounts"));
			AssertEquals(new Guid("B280AA9A-CF21-4CA3-8384-DAED61DB6E63"), row.Field<Guid>("VT_AGForSubAccount"));
			AssertEquals(transactionLinePK, row.Field<Guid>("VT_SubAccountParentPK"));

			// Test: @GST_OUTPUT_ACCOUNT
			// [CB / DRC] AG_PK = '010038A8-6368-4050-9586-D71D52EDEDF0' (ControlAccount = 3000.00.00, HeaderAccount = 3020.00.00)
			// [CB / DRC] AG_PK = 'B280AA9A-CF21-4CA3-8384-DAED61DB6E63' (2ndLineAccount = 4499.00.00)
			transactionHeaderPK = helper.InsertTransactionHeader("CB", "DRC", "AA10", 20M, DateTime.Now, new Guid("27a55065-ac88-4ec3-8bed-e575e79172cb"),
				new Guid("f5c72696-19ad-4759-879f-89c8532ff238"), bankAccount, glAccountPK: new Guid("010038A8-6368-4050-9586-D71D52EDEDF0"));
			transactionLinePK = helper.InsertTransactionLine(transactionHeaderPK, null, null, new Guid("010038A8-6368-4050-9586-D71D52EDEDF0"), new Guid("27a55065-ac88-4ec3-8bed-e575e79172cb"), new Guid("f5c72696-19ad-4759-879f-89c8532ff238"), null, 10M, "DRC", null, null);
			var transactionLinePK2 = helper.InsertTransactionLine(transactionHeaderPK, null, null, new Guid("B280AA9A-CF21-4CA3-8384-DAED61DB6E63"), new Guid("27a55065-ac88-4ec3-8bed-e575e79172cb"), new Guid("f5c72696-19ad-4759-879f-89c8532ff238"), null, 10M, "DRC", null, null);

			rows = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM GetGenericTransactions ('878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')").AsEnumerable();
			AssertNotNull(rows);
			row = rows.Where(x => x.Field<Guid>("VT_PK") == transactionHeaderPK).FirstOrDefault(x => x.Field<Guid>("VT_SubAccountParentPK") == transactionLinePK);
			AssertEquals("ORG: , SEG: , STR: , SGP: ", row.Field<string>("VT_SubAccounts"));
			AssertEquals(new Guid("010038A8-6368-4050-9586-D71D52EDEDF0"), row.Field<Guid>("VT_AGForSubAccount"));
			AssertEquals(transactionLinePK, row.Field<Guid>("VT_SubAccountParentPK"));
			row = rows.Where(x => x.Field<Guid>("VT_PK") == transactionHeaderPK).FirstOrDefault(x => x.Field<Guid>("VT_SubAccountParentPK") == transactionLinePK2);
			AssertEquals("ORG: , STR: , SGP: ", row.Field<string>("VT_SubAccounts"));
			AssertEquals(new Guid("B280AA9A-CF21-4CA3-8384-DAED61DB6E63"), row.Field<Guid>("VT_AGForSubAccount"));
			AssertEquals(transactionLinePK2, row.Field<Guid>("VT_SubAccountParentPK"));

			// Test: @GST_INPUT_ACCOUNT
			// [CB / DPY] AG_PK = 'B280AA9A-CF21-4CA3-8384-DAED61DB6E63' (ControlAccount = 4099.00.00, HeaderAccount = 4499.00.00)
			// [CB / DPY] AG_PK = '010038A8-6368-4050-9586-D71D52EDEDF0' (2ndLineAccount = 3020.00.00)
			transactionHeaderPK = helper.InsertTransactionHeader("CB", "DPY", "AA11", 20M, DateTime.Now, new Guid("27a55065-ac88-4ec3-8bed-e575e79172cb"),
				new Guid("f5c72696-19ad-4759-879f-89c8532ff238"), bankAccount, glAccountPK: new Guid("B280AA9A-CF21-4CA3-8384-DAED61DB6E63"));
			transactionLinePK = helper.InsertTransactionLine(transactionHeaderPK, null, null, new Guid("B280AA9A-CF21-4CA3-8384-DAED61DB6E63"), new Guid("27a55065-ac88-4ec3-8bed-e575e79172cb"), new Guid("f5c72696-19ad-4759-879f-89c8532ff238"), null, 10M, "DPY", null, null);
			transactionLinePK2 = helper.InsertTransactionLine(transactionHeaderPK, null, null, new Guid("010038A8-6368-4050-9586-D71D52EDEDF0"), new Guid("27a55065-ac88-4ec3-8bed-e575e79172cb"), new Guid("f5c72696-19ad-4759-879f-89c8532ff238"), null, 10M, "DPY", null, null);

			rows = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM GetGenericTransactions ('878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')").AsEnumerable();
			AssertNotNull(rows);
			row = rows.Where(x => x.Field<Guid>("VT_PK") == transactionHeaderPK).FirstOrDefault(x => x.Field<Guid>("VT_SubAccountParentPK") == transactionLinePK);
			AssertEquals("ORG: , STR: , SGP: ", row.Field<string>("VT_SubAccounts"));
			AssertEquals(new Guid("B280AA9A-CF21-4CA3-8384-DAED61DB6E63"), row.Field<Guid>("VT_AGForSubAccount"));
			AssertEquals(transactionLinePK, row.Field<Guid>("VT_SubAccountParentPK"));
			row = rows.Where(x => x.Field<Guid>("VT_PK") == transactionHeaderPK).FirstOrDefault(x => x.Field<Guid>("VT_SubAccountParentPK") == transactionLinePK2);
			AssertEquals("ORG: , SEG: , STR: , SGP: ", row.Field<string>("VT_SubAccounts"));
			AssertEquals(new Guid("010038A8-6368-4050-9586-D71D52EDEDF0"), row.Field<Guid>("VT_AGForSubAccount"));
			AssertEquals(transactionLinePK2, row.Field<Guid>("VT_SubAccountParentPK"));

			#endregion

			#region InvCrdAdj

			// Test: @AP_Control_Account
			// [AP / INV] AG_PK = '245ee66c-4bcc-424d-956b-34077a3aab96' (ControlAccount = 2010.00.00, HeaderAccount = 2020.00.00)
			transactionHeaderPK = helper.InsertTransactionHeader("AP", "INV", "AA12", 10M, DateTime.Now, new Guid("27a55065-ac88-4ec3-8bed-e575e79172cb"),
				new Guid("f5c72696-19ad-4759-879f-89c8532ff238"), bankAccount, glAccountPK: new Guid("245ee66c-4bcc-424d-956b-34077a3aab96"));
			transactionLinePK = helper.InsertTransactionLine(transactionHeaderPK, null, null, new Guid("245ee66c-4bcc-424d-956b-34077a3aab96"), new Guid("27a55065-ac88-4ec3-8bed-e575e79172cb"), new Guid("f5c72696-19ad-4759-879f-89c8532ff238"), null, 10M, "CST", null, null);

			rows = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM GetGenericTransactions ('878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')").AsEnumerable();
			AssertNotNull(rows);
			row = rows.FirstOrDefault(x => x.Field<Guid>("VT_PK") == transactionHeaderPK);
			AssertEquals("ORG: , SEG: , STR: , SGP: ", row.Field<string>("VT_SubAccounts"));
			AssertEquals(new Guid("245ee66c-4bcc-424d-956b-34077a3aab96"), row.Field<Guid>("VT_AGForSubAccount"));
			AssertEquals(transactionLinePK, row.Field<Guid>("VT_SubAccountParentPK"));

			// Test: @AR_Control_Account
			// [AR / INV] AG_PK = '05e814ce-284b-41c4-83cf-4b01c0b70e89' (ControlAccount = 1000.00.00, HeaderAccount = 1020.00.00)
			transactionHeaderPK = helper.InsertTransactionHeader("AR", "INV", "AA13", 10M, DateTime.Now, new Guid("27a55065-ac88-4ec3-8bed-e575e79172cb"),
				new Guid("f5c72696-19ad-4759-879f-89c8532ff238"), bankAccount, glAccountPK: new Guid("05e814ce-284b-41c4-83cf-4b01c0b70e89"));
			transactionLinePK = helper.InsertTransactionLine(transactionHeaderPK, null, null, new Guid("05e814ce-284b-41c4-83cf-4b01c0b70e89"), new Guid("27a55065-ac88-4ec3-8bed-e575e79172cb"), new Guid("f5c72696-19ad-4759-879f-89c8532ff238"), null, 10M, "REV", null, null);

			rows = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM GetGenericTransactions ('878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')").AsEnumerable();
			AssertNotNull(rows);
			row = rows.FirstOrDefault(x => x.Field<Guid>("VT_PK") == transactionHeaderPK);
			AssertEquals(null, row.Field<string>("VT_SubAccounts"));
			AssertEquals(new Guid("05e814ce-284b-41c4-83cf-4b01c0b70e89"), row.Field<Guid>("VT_AGForSubAccount"));
			AssertEquals(transactionLinePK, row.Field<Guid>("VT_SubAccountParentPK"));

			#endregion

			#region JRJournal

			// Test: @JOB_REVENUE_JOURNAL_CONTROL_ACCOUNT
			// [JC / JRJ] AG_PK = '25448216-975b-4910-9560-ff9249a9ce2f' (ControlAccount = 5000.00.00, HeaderAccount = 5100.00.00)
			transactionHeaderPK = helper.InsertTransactionHeader("JC", "JRJ", "AA14", 10M, DateTime.Now, new Guid("27a55065-ac88-4ec3-8bed-e575e79172cb"),
				new Guid("f5c72696-19ad-4759-879f-89c8532ff238"), bankAccount, glAccountPK: new Guid("25448216-975b-4910-9560-ff9249a9ce2f"));
			transactionLinePK = helper.InsertTransactionLine(transactionHeaderPK, null, null, new Guid("25448216-975b-4910-9560-ff9249a9ce2f"), new Guid("27a55065-ac88-4ec3-8bed-e575e79172cb"), new Guid("f5c72696-19ad-4759-879f-89c8532ff238"), null, 10M, "REV", null, null);

			rows = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM GetGenericTransactions ('878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')").AsEnumerable();
			AssertNotNull(rows);
			row = rows.FirstOrDefault(x => x.Field<Guid>("VT_PK") == transactionHeaderPK);
			AssertEquals("", row.Field<string>("VT_SubAccounts"));
			AssertEquals(DBNull.Value, row["VT_AGForSubAccount"]);
			AssertEquals(DBNull.Value, row["VT_SubAccountParentPK"]);

			#endregion

			#region WIPSJournal

			// Test: @ACCRUED_REVENUE_ACCOUNT
			// [JC / WIP] AG_PK = '865d227b-f6d0-49a2-963a-48331e168858' (ControlAccount = 6000.00.00, HeaderAccount = 6010.00.00)
			var chargeCodePK = helper.InsertChargeCode(TestDbHelper.DefaultCompanyPK, "CC1");
			transactionLinePK = helper.InsertTransactionLine(null, null, chargeCodePK, new Guid("865d227b-f6d0-49a2-963a-48331e168858"), new Guid("27a55065-ac88-4ec3-8bed-e575e79172cb"), new Guid("f5c72696-19ad-4759-879f-89c8532ff238"), null, 10M, "WIP", null, null);

			rows = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM GetGenericTransactions ('878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')").AsEnumerable();
			AssertNotNull(rows);
			row = rows.FirstOrDefault(x => x.Field<Guid>("VT_PK") == transactionLinePK);
			AssertEquals("", row.Field<string>("VT_SubAccounts"));
			AssertEquals(DBNull.Value, row["VT_AGForSubAccount"]);
			AssertEquals(DBNull.Value, row["VT_SubAccountParentPK"]);

			// Test: @ACCRUED_COST_ACCOUNT
			// [JC / ACR] AG_PK = '046d1569-defd-499a-af27-c1b9cbdae291' (ControlAccount = 7000.00.00, HeaderAccount = 7100.00.00)
			transactionLinePK = helper.InsertTransactionLine(null, null, chargeCodePK, new Guid("046d1569-defd-499a-af27-c1b9cbdae291"), new Guid("27a55065-ac88-4ec3-8bed-e575e79172cb"), new Guid("f5c72696-19ad-4759-879f-89c8532ff238"), null, 10M, "ACR", null, null);

			helper.InsertAccTransactionLineSubAccount(transactionLinePK, "OH", new Guid("0daab61b-255e-4ad7-afc5-4e7b03c3bda1"));
			helper.InsertAccTransactionLineSubAccount(transactionLinePK, "GG", new Guid("55896e13-12be-4fd4-ac94-2956795a5be2"));

			rows = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM GetGenericTransactions ('878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')").AsEnumerable();
			AssertNotNull(rows);
			row = rows.FirstOrDefault(x => x.Field<Guid>("VT_PK") == transactionLinePK);
			AssertEquals("", row.Field<string>("VT_SubAccounts"));
			AssertEquals(DBNull.Value, row["VT_AGForSubAccount"]);
			AssertEquals(DBNull.Value, row["VT_SubAccountParentPK"]);

			#endregion

			#region CFX

			// Test: @GL_CFX_ACCOUNT
			// [JC / WIP] AG_PK = 'cf313c85-1ba2-41ca-b44e-5947af24054f' (ControlAccount = 8000.00.00, HeaderAccount = 8010.00.00)
			transactionHeaderPK = helper.InsertTransactionHeader("JC", "JNL", "AA15", 10M, DateTime.Now, new Guid("27a55065-ac88-4ec3-8bed-e575e79172cb"),
				new Guid("f5c72696-19ad-4759-879f-89c8532ff238"), bankAccount, glAccountPK: new Guid("cf313c85-1ba2-41ca-b44e-5947af24054f"));
			transactionLinePK = helper.InsertTransactionLine(transactionHeaderPK, null, null, new Guid("cf313c85-1ba2-41ca-b44e-5947af24054f"), new Guid("27a55065-ac88-4ec3-8bed-e575e79172cb"), new Guid("f5c72696-19ad-4759-879f-89c8532ff238"), null, 10M, "WIP", null, null);

			rows = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM GetGenericTransactions ('878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')").AsEnumerable();
			AssertNotNull(rows);
			row = rows.FirstOrDefault(x => x.Field<Guid>("VT_PK") == transactionHeaderPK);
			AssertEquals("", row.Field<string>("VT_SubAccounts"));
			AssertEquals(DBNull.Value, row["VT_AGForSubAccount"]);
			AssertEquals(DBNull.Value, row["VT_SubAccountParentPK"]);

			#endregion

			#region GLJournal

			transactionHeaderPK = helper.InsertTransactionHeader("GL", "GJL", "AA16", 10M, DateTime.Now, new Guid("27a55065-ac88-4ec3-8bed-e575e79172cb"),
				new Guid("f5c72696-19ad-4759-879f-89c8532ff238"), bankAccount, glAccountPK: new Guid("ac129d82-b88d-45ee-bce5-25592f734023"));
			transactionLinePK = helper.InsertTransactionLine(transactionHeaderPK, null, null, new Guid("ac129d82-b88d-45ee-bce5-25592f734023"), new Guid("27a55065-ac88-4ec3-8bed-e575e79172cb"), new Guid("f5c72696-19ad-4759-879f-89c8532ff238"), null, 10M, "GJL", null, null);

			helper.InsertAccTransactionLineSubAccount(transactionLinePK, "OH", new Guid("0daab61b-255e-4ad7-afc5-4e7b03c3bda1"));
			helper.InsertAccTransactionLineSubAccount(transactionLinePK, "GG", new Guid("55896e13-12be-4fd4-ac94-2956795a5be2"));

			rows = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM GetGenericTransactions ('878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')").AsEnumerable();
			AssertNotNull(rows);
			row = rows.FirstOrDefault(x => x.Field<Guid>("VT_PK") == transactionHeaderPK);
			AssertEquals("ORG: ABIGAS, SGP: PMG", row.Field<string>("VT_SubAccounts"));
			AssertEquals(new Guid("ac129d82-b88d-45ee-bce5-25592f734023"), row.Field<Guid>("VT_AGForSubAccount"));
			AssertEquals(transactionLinePK, row.Field<Guid>("VT_SubAccountParentPK"));

			transactionHeaderPK = helper.InsertTransactionHeader("GL", "NJL", "AAA20", 10M, DateTime.Now, new Guid("27a55065-ac88-4ec3-8bed-e575e79172cb"), new Guid("f5c72696-19ad-4759-879f-89c8532ff238"));
			transactionLinePK = helper.InsertTransactionLine(transactionHeaderPK, null, null, helper.GLNoteAccountPK1, new Guid("27a55065-ac88-4ec3-8bed-e575e79172cb"), new Guid("f5c72696-19ad-4759-879f-89c8532ff238"), null, 10M, "NJL", null, null);

			rows = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM GetGenericTransactions ('878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')").AsEnumerable();
			AssertNotNull(rows);
			row = rows.FirstOrDefault(x => x.Field<Guid>("VT_PK") == transactionHeaderPK);
			AssertEquals("AAA20", row.Field<string>("VT_TransactionNo"));
			#endregion
		}

		protected override void SetUp()
		{
			base.SetUp();

			TestConnection.ExecuteNonQuery("UPDATE dbo.StmData SET SD_GuidValue = (SELECT AG_PK FROM dbo.AccGLHeader WHERE AG_AccountNum = '1000.00.00') WHERE SD_Name = 'GL_AR_CONTROL_ACCOUNT'");
			TestConnection.ExecuteNonQuery("UPDATE dbo.StmData SET SD_GuidValue = (SELECT AG_PK FROM dbo.AccGLHeader WHERE AG_AccountNum = '2010.00.00') WHERE SD_Name = 'GL_AP_CONTROL_ACCOUNT'");
			TestConnection.ExecuteNonQuery("UPDATE dbo.StmData SET SD_GuidValue = (SELECT AG_PK FROM dbo.AccGLHeader WHERE AG_AccountNum = '3000.00.00') WHERE SD_Name = 'GL_GST_OUTPUT_ACCOUNT'");
			TestConnection.ExecuteNonQuery("UPDATE dbo.StmData SET SD_GuidValue = (SELECT AG_PK FROM dbo.AccGLHeader WHERE AG_AccountNum = '4099.00.00') WHERE SD_Name = 'GL_GST_INPUT_ACCOUNT'");
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.StmData (SD_PK, SD_Name, SD_Owner, SD_DepartmentGuid, SD_Type, SD_BinaryValue, SD_GuidValue) VALUES(NEWID(), 'GL_JOB_REVENUE_JOURNAL_CONTROL_ACCOUNT', NULL, NULL, 'GID', NULL, '590ccb03-a884-449a-91f0-c07043ab9229')"); // WHERE AG_AccountNum = '5000.00.00'
			TestConnection.ExecuteNonQuery("UPDATE dbo.StmData SET SD_GuidValue = (SELECT AG_PK FROM dbo.AccGLHeader WHERE AG_AccountNum = '6000.00.00') WHERE SD_Name = 'GL_ACCRUED_REVENUE_ACCOUNT'");
			TestConnection.ExecuteNonQuery("UPDATE dbo.StmData SET SD_GuidValue = (SELECT AG_PK FROM dbo.AccGLHeader WHERE AG_AccountNum = '7000.00.00') WHERE SD_Name = 'GL_ACCRUED_COST_ACCOUNT'");
			TestConnection.ExecuteNonQuery("UPDATE dbo.StmData SET SD_GuidValue = (SELECT AG_PK FROM dbo.AccGLHeader WHERE AG_AccountNum = '8000.00.00') WHERE SD_Name = 'GL_CFX_ACCOUNT'");
		}
	}
}

