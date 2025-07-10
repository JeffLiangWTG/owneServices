using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Testing
{
	[TestedType(typeof(ARCreditNotesIssued))]
	class ARCreditNotesIssuedTest : DbCreateScriptTest
	{
		public void TestFunctionReadCreateUserAndTimeFromAccTransactonHeaderTable()
		{
			TestConnection.ExecuteNonQuery(insertCreditNote);
			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM ARCreditNotesIssued('878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', NULL)");
			AssertEquals("Result should have rows even without any StmALog created, because we now look for the 'add staff' through AccTransactionHeader table in order to improve performance", 1, result.Rows.Count);
			AssertEquals("DateTimeAdded should be read from dbo.AccTransactionHeader", "30/7/2010 11:58:00", string.Format("{0:d/M/yyyy HH:mm:ss}", result.Rows[0]["DateTimeAdded"]));
			AssertEquals("StaffCode should be read from dbo.AccTransactionHeader", "E", result.Rows[0]["StaffCode"].ToString().Trim());
		}

		public void TestARCreditNotesInvoiceLocalTotalWithOtherTaxes()
		{
			TestConnection.ExecuteNonQuery(insertCreditNote);
			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM ARCreditNotesIssued('878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', NULL)");
			AssertEquals(11m, result.Rows[0]["InvoiceAmount"]);
		}

		public void TestExecuteWithoutErrors()
		{
			TestConnection.ExecuteNonQuery(insertCreditNote);
			TestConnection.ExecuteNonQuery(insertAtmALog);

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM ARCreditNotesIssued('878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', NULL)");
			AssertEquals("Result should have rows", 1, result.Rows.Count);
			AssertEquals("Description field should show correct description", "AR CREDIT NOTE 中英字典", result.Rows[0]["AH_Desc"]);
		}

		public void TestMultipleApprovers()
		{
			TestConnection.ExecuteNonQuery(insertApproverTestData);

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM ARCreditNotesIssued('878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', NULL) ORDER BY DateTimeAdded ASC");
			AssertEquals("There should be 5 Approver test cases.", 5, result.Rows.Count);

			var testCaseRow = result.Rows[0];
			AssertEquals("'Zero Approvers, no StmALog' test case should have NULL StaffAuthCode.", DBNull.Value, testCaseRow["AuthStaffCode"]);
			AssertEquals("'Zero Approvers, no StmALog' test case should have NULL ApprovedByStaffFullNames.", DBNull.Value, testCaseRow["ApprovedByStaffFullNames"]);

			testCaseRow = result.Rows[1];
			AssertEquals("'One Approver, one StmALog SL_Reference' test case should have 'L22' StaffAuthCode.", "L22", testCaseRow["AuthStaffCode"]);
			AssertEquals("'One Approver, one StmALog SL_Reference' test case should have 'Level Two' ApprovedByStaffFullNames.", "Level Two", testCaseRow["ApprovedByStaffFullNames"]);

			testCaseRow = result.Rows[2];
			AssertEquals("'One Approver, blank StmALog SL_Reference' test case should have 'L22' StaffAuthCode.", "L22", testCaseRow["AuthStaffCode"]);
			AssertEquals("'One Approver, blank StmALog SL_Reference' test case should have blank ApprovedByStaffFullNames.", "", testCaseRow["ApprovedByStaffFullNames"]);

			testCaseRow = result.Rows[3];
			AssertEquals("'Two Approvers, two StmALog SL_References' test case should have 'L11' StaffAuthCode.", "L11", testCaseRow["AuthStaffCode"]);
			AssertEquals("'Two Approvers, two StmALog SL_References' test case should have 'Level One, Level Two' ApprovedByStaffFullNames.", "Level One, Level Two", testCaseRow["ApprovedByStaffFullNames"]);

			testCaseRow = result.Rows[4];
			AssertEquals("'One Approver, blank GS_FullName' test case should have 'LLL' StaffAuthCode.", "LLL", testCaseRow["AuthStaffCode"]);
			AssertEquals("'One Approver, blank GS_FullName' test case should have blank ApprovedByStaffFullNames.", "", testCaseRow["ApprovedByStaffFullNames"]);
		}

		#region Insert Commands

		const string insertCreditNote = @"INSERT INTO dbo.AccTransactionHeader
(
	AH_PK, 
	AH_AgePeriod, 
	AH_CashBasisGSTIndicator, 
	AH_CashBasisGSTRealisedToGL, 
	AH_ChequeDrawer, 
	AH_ChequeOrReference, 
	AH_ConsolidatedInvoiceRef, 
	AH_Desc, 
	AH_DrawerBank, 
	AH_DrawerBranch, 
	AH_DueDate, 
	AH_ExchangeRate, 
	AH_GB, 
	AH_GC, 
	AH_GE, 
	AH_GSTAmount, 
	AH_InvoiceAmount, 
	AH_InvoiceApproved, 
	AH_InvoiceDate, 
	AH_InvoicePrinted, 
	AH_InvoiceTerm, 
	AH_InvoiceTermDays, 
	AH_IsCancelled, 
	AH_Ledger, 
	AH_NotAllocated, 
	AH_OH, 
	AH_OSTotal, 
	AH_OutstandingAmount, 
	AH_POST1, 
	AH_POST2, 
	AH_POST3, 
	AH_POST4, 
	AH_PostDate, 
	AH_PostedInternal, 
	AH_PostedToEFT, 
	AH_PostPeriod, 
	AH_PostToGL, 
	AH_ReceiptBatchNo, 
	AH_ReceiptType, 
	AH_RequisitionStatus, 
	AH_RX_NKTransactionCurrency, 
	AH_SystemCreateTimeUtc, 
	AH_SystemCreateUser, 
	AH_TransactionCategory, 
	AH_TransactionCount, 
	AH_TransactionCreatedByMatching, 
	AH_TransactionNum, 
	AH_TransactionReference, 
	AH_TransactionType, 
	AH_WithholdingTax,
	AH_LocalTaxAmountOtherTaxes,
	AH_SystemLastEditTimeUtc,
	AH_SystemLastEditUser
)
VALUES
(
	'24399180-a934-474b-af2c-626284614413', 
	0, 
	0, 
	0, 
	N'', 
	'', 
	'', 
	N'AR CREDIT NOTE 中英字典', 
	N'', 
	N'', 
	'2010-07-30 11:55:00.000', 
	1, 
	'27a55065-ac88-4ec3-8bed-e575e79172cb', 
	'878d7aca-ffc3-49fc-9710-969ca0c0f2ac', 
	'86bb1c22-0865-4685-996e-d56cbd136491', 
	0, 
	-10, 
	0, 
	'2010-07-30 11:55:00.000', 
	0, 
	'COD', 
	0, 
	0, 
	'AR', 
	0, 
	'0daab61b-255e-4ad7-afc5-4e7b03c3bda1', 
	-10, 
	-10, 
	0, 
	0, 
	0, 
	0, 
	'2010-07-30 11:55:00.000', 
	0, 
	0, 
	0, 
	0, 
	'', 
	'', 
	'', 
	'AUD', 
	'2010-07-30 11:58:00.000', 
	'E', 
	'FIN', 
	1, 
	0, 
	'00001000901234567890', 
	'', 
	'CRD', 
	0,
	-1,
	GetUtcDate(),
	'~BP'
)";
		const string insertAtmALog = @"INSERT INTO dbo.StmALog
(
	SL_PK, 
	SL_EventTime, 
	SL_GS_NKUser, 
	SL_IsCancelled, 
	SL_IsEstimate, 
	SL_Parent, 
	SL_PostedTimeUtc, 
	SL_Reference, 
	SL_SE_NKEvent, 
	SL_Table
)
VALUES
(
	'575e964f-d52d-4d44-ae9b-22102d74fd59', 
	'2010-07-30 11:55:53.377', 
	'E', 
	'N', 
	'N', 
	'24399180-a934-474b-af2c-626284614413', 
	'2010-07-30 01:58:49.060', 
	'', 
	'ADD', 
	'AccTransactionHeader'
)";

		const string insertApproverTestData = @"
-- Test cases for zero, one and two approvers.

--BEGIN TRANSACTION;

INSERT INTO [dbo].[GlbStaff]
(
	[GS_PK]
	,[GS_IsValid]
	,[GS_Code]
	,[GS_IsActive]
	,[GS_LoginName]
	,[GS_FullName]
	,[GS_SystemCreateTimeUtc]
	,[GS_SystemCreateUser]
	,[GS_SystemLastEditTimeUtc]
	,[GS_SystemLastEditUser]
)
VALUES
(
	'D6583230-7411-4527-B59E-E6AFCCE01000'
	, 1
	, 'L00'
	, 1
	, 'L0'
	, 'Level Zero'
	, GetUtcDate()
	, '~BP'
	, GetUtcDate()
	, '~BP'
),
(
	'D6583230-7411-4527-B59E-E6AFCCE01001'
	, 1
	, 'L11'
	, 1
	, 'L1'
	, 'Level One'
	, GetUtcDate()
	, '~BP'
	, GetUtcDate()
	, '~BP'
),
(
	'D6583230-7411-4527-B59E-E6AFCCE01002'
	, 1
	, 'L22'
	, 1
	, 'L2'
	, 'Level Two'
	, GetUtcDate()
	, '~BP'
	, GetUtcDate()
	, '~BP'
),
(
	'D6583230-7411-4527-B59E-E6AFCCE01003'
	, 1
	, 'LLL'
	, 1
	, 'LL'
	, ''
	, GetUtcDate()
	, '~BP'
	, GetUtcDate()
	, '~BP'
)
;

INSERT INTO [dbo].[AccTransactionHeader] 
(	   [AH_PK]
      ,[AH_Ledger]
      ,[AH_TransactionType]
      ,[AH_TransactionNum]
      ,[AH_TransactionCount]
      ,[AH_Desc]
      ,[AH_InvoiceDate]
      ,[AH_DueDate]
      ,[AH_InvoiceAmount]
      ,[AH_GSTAmount]
      ,[AH_WithholdingTax]
      ,[AH_OSTotal]
      ,[AH_RX_NKTransactionCurrency]
      ,[AH_ExchangeRate]
      ,[AH_PostDate]
      ,[AH_TransactionCategory]
      ,[AH_OutstandingAmount]
      ,[AH_InvoiceTerm]
      ,[AH_InvoiceTermDays]
      ,[AH_NumberOfSupportingDocuments]
      ,[AH_OH]
      ,[AH_OA_InvoiceAddressOverride]
      ,[AH_GB]
      ,[AH_GC]
      ,[AH_GE]
      ,[AH_SystemCreateTimeUtc]
      ,[AH_SystemCreateUser]
      ,[AH_SystemLastEditTimeUtc]
      ,[AH_SystemLastEditUser]
)
VALUES 
(
	'9D7244B8-2CEC-4A77-B1BB-C703E62AC001'
	, 'AR'
	, 'CRD'
	, '00002001'
	, 1
	, 'Zero Approvers, no StmALog'
	, '2019-06-12 12:01:00'
	, '2019-06-12 12:01:00'
	, -500.00
	, 0.00
	, 0.00
	, -500.00
	, 'AUD'
	, 1.000000000
	, '2019-06-12 12:01:00'
	, 'FIN'
	, -500.00
	, 'COD'
	, 0
	, 1
	, 'B4B76F20-B5B6-4FC7-B0E1-00F5DF06D88A'
	, '34DE5AEB-6668-4F48-A04F-E7D100F13DB7'
	, '27A55065-AC88-4EC3-8BED-E575E79172CB'
	, '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC'
	, '86BB1C22-0865-4685-996E-D56CBD136491'
	, '2019-06-12 00:01:00'
	, 'L00'
	, '2019-06-12 00:01:00'
	, 'L00'
),
(
	'9D7244B8-2CEC-4A77-B1BB-C703E62AC002'
	, 'AR'
	, 'CRD'
	, '00002002'
	, 1
	, 'One Approver, one StmALog SL_Reference'
	, '2019-06-12 12:02:00'
	, '2019-06-12 12:02:00'
	, -600.00
	, 0.00
	, 0.00
	, -600.00
	, 'AUD'
	, 1.000000000
	, '2019-06-12 12:02:00'
	, 'FIN'
	, -600.00
	, 'COD'
	, 0
	, 1
	, 'B4B76F20-B5B6-4FC7-B0E1-00F5DF06D88A'
	, '34DE5AEB-6668-4F48-A04F-E7D100F13DB7'
	, '27A55065-AC88-4EC3-8BED-E575E79172CB'
	, '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC'
	, '86BB1C22-0865-4685-996E-D56CBD136491'
	, '2019-06-12 00:02:00'
	, 'L00'
	, '2019-06-12 00:02:00'
	, 'L00'
),
(
	'9D7244B8-2CEC-4A77-B1BB-C703E62AC003'
	, 'AR'
	, 'CRD'
	, '00002003'
	, 1
	, 'One Approver, blank StmALog SL_Reference'
	, '2019-06-12 12:03:00'
	, '2019-06-12 12:03:00'
	, -700.00
	, 0.00
	, 0.00
	, -700.00
	, 'AUD'
	, 1.000000000
	, '2019-06-12 12:03:00'
	, 'FIN'
	, -700.00
	, 'COD'
	, 0
	, 1
	, 'B4B76F20-B5B6-4FC7-B0E1-00F5DF06D88A'
	, '34DE5AEB-6668-4F48-A04F-E7D100F13DB7'
	, '27A55065-AC88-4EC3-8BED-E575E79172CB'
	, '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC'
	, '86BB1C22-0865-4685-996E-D56CBD136491'
	, '2019-06-12 00:03:00'
	, 'L11'
	, '2019-06-12 00:03:00'
	, 'L11'
),
(
	'9D7244B8-2CEC-4A77-B1BB-C703E62AC004'
	, 'AR'
	, 'CRD'
	, '00002004'
	, 1
	, 'Two Approvers, two StmALog SL_References'
	, '2019-06-12 12:04:00'
	, '2019-06-12 12:04:00'
	, -800.00
	, 0.00
	, 0.00
	, -800.00
	, 'AUD'
	, 1.000000000
	, '2019-06-12 12:04:00'
	, 'FIN'
	, -800.00
	, 'COD'
	, 0
	, 1
	, 'B4B76F20-B5B6-4FC7-B0E1-00F5DF06D88A'
	, '34DE5AEB-6668-4F48-A04F-E7D100F13DB7'
	, '27A55065-AC88-4EC3-8BED-E575E79172CB'
	, '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC'
	, '86BB1C22-0865-4685-996E-D56CBD136491'
	, '2019-06-12 00:04:00'
	, 'L00'
	, '2019-06-12 00:04:00'
	, 'L00'
),
(
	'9D7244B8-2CEC-4A77-B1BB-C703E62AC005'
	, 'AR'
	, 'CRD'
	, '00002005'
	, 1
	, 'One Approver, blank GS_FullName'
	, '2019-06-12 12:05:00'
	, '2019-06-12 12:05:00'
	, -900.00
	, 0.00
	, 0.00
	, -900.00
	, 'AUD'
	, 1.000000000
	, '2019-06-12 12:05:00'
	, 'FIN'
	, -800.00
	, 'COD'
	, 0
	, 1
	, 'B4B76F20-B5B6-4FC7-B0E1-00F5DF06D88A'
	, '34DE5AEB-6668-4F48-A04F-E7D100F13DB7'
	, '27A55065-AC88-4EC3-8BED-E575E79172CB'
	, '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC'
	, '86BB1C22-0865-4685-996E-D56CBD136491'
	, '2019-06-12 00:05:00'
	, 'LLL'
	, '2019-06-12 00:05:00'
	, 'LLL'
)
;


INSERT INTO [dbo].[StmALog]
(
	[SL_PK]
    ,[SL_Table]
    ,[SL_Parent]
    ,[SL_IsEstimate]
    ,[SL_IsCancelled]
    ,[SL_Reference]
    ,[SL_PostedTimeUtc]
    ,[SL_EventTime]
    ,[SL_GS_NKUser]
    ,[SL_SE_NKEvent]
    ,[SL_GB_NKBranch]
    ,[SL_GE_NKDepartment]
    ,[SL_FireWorkflow]
)
VALUES
(
	'0E477E20-B8DB-4275-9194-D551E3313002'
	, 'AccTransactionHeader'
	, '9D7244B8-2CEC-4A77-B1BB-C703E62AC002'
	, 'N'
	, 'N'
	, 'Post authorized by L2 (Level Two)'
	, '2019-06-11 23:56:24.973'
	, '2019-06-12 09:56:24.590'
	, 'L22'
	, 'ATH'
	, 'BNE'
	, 'BRN'
	, 0
),
-- Unexpected case: authorised but blank SL_Reference.
--   Included to ensure this produces a blank string, and for the possibility that this exists in prior data.
(
	'0E477E20-B8DB-4275-9194-D551E3313003'
	, 'AccTransactionHeader'
	, '9D7244B8-2CEC-4A77-B1BB-C703E62AC003'
	, 'N'
	, 'N'
	, ''
	, '2019-06-11 23:56:24.973'
	, '2019-06-12 09:56:24.590'
	, 'L22'
	, 'ATH'
	, 'BNE'
	, 'BRN'
	, 0
),
(
	'0E477E20-B8DB-4275-9194-D551E3313004'
	, 'AccTransactionHeader'
	, '9D7244B8-2CEC-4A77-B1BB-C703E62AC004'
	, 'N'
	, 'N'
	, 'Post authorized by L1 (Level One), L2 (Level Two)'
	, '2019-06-11 23:56:24.973'
	, '2019-06-12 09:56:24.590'
	, 'L11'
	, 'ATH'
	, 'BNE'
	, 'BRN'
	, 0
),
(
	'0E477E20-B8DB-4275-9194-D551E3313005'
	, 'AccTransactionHeader'
	, '9D7244B8-2CEC-4A77-B1BB-C703E62AC005'
	, 'N'
	, 'N'
	, 'Post authorized by LLL ()'
	, '2019-06-11 23:56:24.973'
	, '2019-06-12 09:56:24.590'
	, 'LLL'
	, 'ATH'
	, 'BNE'
	, 'BRN'
	, 0
)
;

--SELECT * FROM ARCreditNotesIssued('878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', NULL) ORDER BY PostDate;

--ROLLBACK;
";
		#endregion
	}
}

