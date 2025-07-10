using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Accounting;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Accounting
{
	[TestedType(typeof(AccEPaymentDealUsageCollector))]
	sealed class AccEPaymentDealUsageCollectorTest : RefStlScriptWithDefaultsTest
	{
		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2022, 3);

		protected override bool IsMandatoryForMilestones => false;

		protected override void PrepareTestData()
		{
			//Update the test date when generating the script to run in UAT - useful when setting up for developer functional testing
			var testDate = "2022-3-29";
			var sqlQuery = $@"
				DECLARE @SYDCompanyPK UNIQUEIDENTIFIER = NEWID();
				DECLARE @SYDBranchPK  UNIQUEIDENTIFIER = NEWID();
				DECLARE @SYDOrgProxyPK UNIQUEIDENTIFIER = NEWID();
				DECLARE @SYDHeader UNIQUEIDENTIFIER = NEWID();
				DECLARE @SYDHeader2 UNIQUEIDENTIFIER = NEWID();
				DECLARE @SYDBankAccount UNIQUEIDENTIFIER = NEWID();
				DECLARE @SYDBankAccount2 UNIQUEIDENTIFIER = NEWID();

				DECLARE @TYOCompanyPK  UNIQUEIDENTIFIER = NEWID();
				DECLARE @TYOBranchPK  UNIQUEIDENTIFIER = NEWID();
				DECLARE @TYOOrgProxyPK UNIQUEIDENTIFIER = NEWID();
				DECLARE @TYOHeader UNIQUEIDENTIFIER = NEWID();
				DECLARE @TYOBankAccount UNIQUEIDENTIFIER = NEWID();

				DECLARE @Quote1 UNIQUEIDENTIFIER = NEWID();
				DECLARE @Quote2 UNIQUEIDENTIFIER = NEWID();
				DECLARE @Quote3 UNIQUEIDENTIFIER = NEWID();
				DECLARE @Quote4 UNIQUEIDENTIFIER = NEWID();
				DECLARE @Quote5 UNIQUEIDENTIFIER = NEWID();
				DECLARE @Quote6 UNIQUEIDENTIFIER = NEWID();

				DECLARE @Batch1 UNIQUEIDENTIFIER = NEWID();

				DECLARE @Payment1 UNIQUEIDENTIFIER = NEWID();
				DECLARE @Payment2 UNIQUEIDENTIFIER = NEWID();
				DECLARE @Payment3 UNIQUEIDENTIFIER = NEWID();

				DECLARE @Deal1 UNIQUEIDENTIFIER = NEWID();
				DECLARE @Deal2 UNIQUEIDENTIFIER = NEWID();
				DECLARE @Deal3 UNIQUEIDENTIFIER = NEWID();
				DECLARE @Deal4 UNIQUEIDENTIFIER = NEWID();

				INSERT dbo.OrgHeader (OH_PK, OH_Code) VALUES
					(@SYDOrgProxyPK, 'SYD100'),
					(@TYOOrgProxyPK, 'TOKYOJ');

				INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RX_NKLocalCurrency, GC_RN_NKCountryCode, GC_OH_OrgProxy) VALUES
					(@SYDCompanyPK, 'SD1', 'AU company', 'AUD', 'AU', @SYDOrgProxyPK),
					(@TYOCompanyPK, 'JTK', 'JP company', 'JPY', 'JP', @TYOOrgProxyPK);

				INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC) VALUES
					(@SYDBranchPK, 'SD1',  @SYDCompanyPK),
					(@TYOBranchPK, 'TK1',  @TYOCompanyPK);

				INSERT dbo.AccGLHeader (AG_PK, AG_AccountType, AG_AccountNum, AG_DebitCredit) VALUES
					(@SYDHeader, 'XXX', 1, 'DR'),
					(@TYOHeader, 'XXX', 2, 'DR'),
					(@SYDHeader2, 'XXX', 3, 'DR');

				INSERT dbo.AccBankAccount (AB_PK, AB_GC, AB_AG, AB_Code, AB_AccountNum) VALUES
					(@SYDBankAccount, @SYDCompanyPK, @SYDHeader, 'SYDBA', 1),
					(@TYOBankAccount, @TYOCompanyPK, @TYOHeader, 'TYOBA', 2),
					(@SYDBankAccount2, @SYDCompanyPK, @SYDHeader2, 'ERROR', 3);

				INSERT INTO dbo.AccEPaymentStaffToken (TK_PK, TK_AccountName, TK_GS_NKStaffCode, TK_Scope, TK_AB, TK_RequestedUtc, TK_GC, TK_SystemCreateTimeUtc, TK_SystemCreateUser, TK_SystemLastEditTimeUtc, TK_SystemLastEditUser, TK_Status, TK_ExpiryUtc) VALUES
					(NEWID(), 'A_Account', 'AAA', 'payments', @SYDBankAccount, '2022-1-21 02:00', @SYDCompanyPK, '2021-3-21 11:01', 'AAA', '2021-3-21 11:01', 'AAA', 'ATH', '2023-3-21 11:01'),
					(NEWID(), 'Wrong_Account', 'AAA', 'randomScope', @SYDBankAccount, '2022-1-20 02:00', @SYDCompanyPK, '2021-3-21 11:01', 'AAA', '2021-3-21 11:01', 'AAA', 'ATH', '2023-3-21 11:01'),
					(NEWID(), 'Error_Account', 'AAA', 'payments', @SYDBankAccount2, '2022-1-21 02:00', @SYDCompanyPK, '2021-3-20 10:01', 'AAA', '2021-3-20 10:01', 'AAA', 'ERR', null),
					(NEWID(), 'B_Account', 'BBB', 'payments', @SYDBankAccount, '2022-2-21 11:10', @SYDCompanyPK, '2021-3-21 11:01', 'AAA', '2021-3-21 11:01', 'AAA', 'ATH', '2023-3-21 11:01'),
					(NEWID(), 'C_Account', 'CCC', 'payments', @TYOBankAccount, '2022-1-07 07:07', @TYOCompanyPK, '2021-3-21 11:01', 'CCC', '2021-3-21 11:01', 'CCC', 'ATH', '2023-3-21 11:01');

				INSERT INTO dbo.AccPaymentBatch (APB_PK, APB_GC, APB_BatchNumber, APB_SystemCreateTimeUtc, APB_SystemCreateUser, APB_SystemLastEditTimeUtc, APB_SystemLastEditUser, APB_ChequeOrReference, APB_PaymentType, APB_AB, APB_GB, APB_PaymentDate, APB_PostDate) VALUES
					(@Batch1, @SYDCompanyPK, 1, '{testDate} 23:59', 'AAA', '{testDate} 23:59', 'AAA', 'WRK', 'EPA', @SYDBankAccount, @SYDBranchPK, '{testDate} 23:59', '{testDate} 23:59');

				INSERT INTO dbo.AccPaymentApproval (AV_PK,AV_PayRunNo,AV_Status,AV_PaymentType,AV_PayExRate,AV_Amount,AV_GB,AV_OH,AV_AB,AV_Ledger,AV_ExchangeDifference,AV_Discount,AV_PaymentDate,AV_PostDate,AV_GS_NKApproval1st,AV_GS_NKApproval2nd,AV_GS_NKApproval3rd,AV_RX_NKPaymentCurrency,AV_ChequeOrReference,AV_PaymentComment,AV_RejectionReasonCode,AV_RejectionReasonDetails,AV_APB_PaymentBatch,AV_GC,AV_PaymentApprovalReference,AV_SystemCreateTimeUtc,AV_SystemCreateUser,AV_SystemLastEditTimeUtc,AV_SystemLastEditUser,AV_EPaymentReasonCode) VALUES
					(@Payment1, 0, 'APP', 'EPA', 0.000000000, 5500, @SYDBranchPK, @SYDOrgProxyPK, @SYDBankAccount, 'AP', 0.00, 0.00, '{testDate} 00:00', '{testDate} 00:00', '', 'BBB', '', 'JPY', '', 'AP PAYMENT', '', '', NULL, @SYDCompanyPK, 'p1', '{testDate} 00:59', 'AAA', '{testDate} 00:59', 'AAA', ''),
					(@Payment2, 0, 'APP', 'EPA', 0.000000000, 999992, @SYDBranchPK, @SYDOrgProxyPK, @SYDBankAccount, 'AP', 0.00, 0.00, '{testDate} 00:00', '{testDate} 00:00', '', '', '', 'JPY', '', 'AP PAYMENT', '', '', NULL, @SYDCompanyPK, 'p2', '{testDate} 21:03', 'AAA', '{testDate} 21:03', 'AAA', ''),
					(@Payment3, 0, 'APP', 'EPA', 0.000000000, 1000, @TYOBranchPK, @TYOOrgProxyPK, @TYOBankAccount, 'AP', 0.00, 0.00, '{testDate} 00:00', '{testDate} 00:00', '', '', '', 'JPY', '', 'AP PAYMENT', '', '', @Batch1, @TYOCompanyPK, 'p3', '{testDate} 05:28', 'CCC', '{testDate} 05:28', 'CCC', '');

				INSERT INTO dbo.AccEPaymentQuote (QU_PK, QU_AV, QU_RX_NKFromCurrency, QU_RX_NKToCurrency, QU_FromAmount, QU_ToAmount, QU_Status, QU_FeeAmount, QU_SystemCreateTimeUtc, QU_GC, QU_InternalReference, QU_ProviderCode, QU_SystemCreateUser, QU_LastResponseReceivedUtc, QU_ExchangeRate, QU_ExchangeRateInverted, QU_ProviderReference, QU_SystemLastEditTimeUtc, QU_SystemLastEditUser) VALUES
					(@Quote1, @Payment1, 'AUD', 'JPY', 0, 5500, 'DCD', 20, '{testDate} 01:59', @SYDCompanyPK, '10', 'OFX', 'AAA',  null, 0, 0, '1111', '{testDate} 01:59', 'AAA'),
					(@Quote2, @Payment1, 'AUD', 'JPY', 5381.24, 5381.24, 'EXP', 15, '{testDate} 02:07', @SYDCompanyPK, '11', 'OFX', 'AAA', '{testDate} 02:10', 1, 1, '2222', '{testDate} 02:10', '~BP'),
					(@Quote3, @Payment1, 'AUD', 'JPY', 5500, 5500, 'ACP', 14, '{testDate} 02:23', @SYDCompanyPK, '12', 'OFX', 'BBB', '{testDate} 02:26', 1, 1, '3333', '{testDate} 02:26', '~BP'),
					(@Quote4, @Payment2, 'AUD', 'JPY', 999992, 999992, 'ACP', 201.33, '{testDate} 21:24', @SYDCompanyPK, '13', 'OFX', 'AAA', '{testDate} 21:28', 1, 1, '4444', '{testDate} 21:28', '~BP'),
					(@Quote5, @Payment3, 'JPY', 'AUD', 0, 1002.30, 'DCD', 39.22, '{testDate} 12:30', @TYOCompanyPK, '14', 'OFX', 'CCC', null, 0, 0, '5555', '{testDate} 12:30', 'CCC'),
					(@Quote6, @Payment3, 'JPY', 'AUD', 1000, 1000, 'ACP', 36.87, '{testDate} 12:48', @TYOCompanyPK, '15', 'OFX', 'CCC', '{testDate} 12:49', 1, 1, '6666', '{testDate} 12:49', '~BP');

				INSERT INTO dbo.AccEPaymentDeal (AED_PK, AED_Status, AED_ErrorDescription, AED_ProviderReference, AED_QU_Quote, AED_GC_Company, AED_SystemCreateUser, AED_SystemCreateTimeUtc, AED_InternalReference, AED_ProviderCode, AED_LastResponseReceivedUtc, AED_SystemLastEditTimeUtc, AED_SystemLastEditUser) VALUES
					(@Deal1, 'REQ', '', '', @Quote3, @SYDCompanyPK, 'BBB', '{testDate} 02:54', '20', 'OFX', null, '{testDate} 02:54', 'BBB'),
					(@Deal2, 'DEC', 'ERROR: message about Deal being declined', '', @Quote2, @SYDCompanyPK, 'AAA', '{testDate} 02:13', '21', 'OFX', '{testDate} 02:33', '{testDate} 02:33', '~BP'),
					(@Deal3, 'ACP', '', 'Reference1', @Quote4, @SYDCompanyPK, 'AAA', '{testDate} 21:42', '22', 'OFX', '{testDate} 21:53', '{testDate} 21:53', '~BP'),
					(@Deal4, 'PAI', '', 'Reference2', @Quote6, @TYOCompanyPK, 'CCC', '{testDate} 15:01', '23', 'OFX', '{testDate} 20:01', '{testDate} 20:01', '~BP');

				INSERT INTO dbo.StmALog (SL_PK, SL_Table, SL_Parent, SL_SE_NKEvent, SL_Reference, SL_EventTime, SL_PostedTimeUtc, SL_EventTimeUtc) VALUES 
					(NEWID(), 'AccEPaymentDeal', @Deal1, 'DEX', 'Purpose: E-Payment Deal 20 submitted for processing to OFX', '{testDate} 03:01:01.510', '{testDate} 03:01:02.345', '{testDate} 03:01:01.510'),
					(NEWID(), 'AccPaymentApproval', @Payment1, 'DEX', 'Purpose: E-Payment Deal 20 submitted for processing to OFX', '{testDate} 03:01:01.510', '{testDate} 03:01:02.345', '{testDate} 03:01:01.510'),
					(NEWID(), 'AccEPaymentDeal', @Deal3, 'DEX', 'Purpose: E-Payment Deal 22 submitted for processing to OFX', '{testDate} 21:42:01.101', '{testDate} 21:42:22.123', '{testDate} 21:42:01.101'),
					(NEWID(), 'AccPaymentApproval', @Payment2, 'DEX', 'Purpose: E-Payment Deal 22 submitted for processing to OFX', '{testDate} 21:42:01.101', '{testDate} 21:42:22.123', '{testDate} 21:42:01.101'),
					(NEWID(), 'AccEPaymentDeal', @Deal3, 'IAK', 'E-Payment Deal 22 accepted by OFX.', '{testDate} 21:43:02.245', '{testDate} 21:43:46.008', '{testDate} 21:43:02.245'),
					(NEWID(), 'AccPaymentApproval', @Payment2, 'IAK', 'E-Payment Deal 22 accepted by OFX.', '{testDate} 21:43:02.245', '{testDate} 21:43:46.008', '{testDate} 21:43:02.245'),
					(NEWID(), 'AccEPaymentDeal', @Deal4, 'DEX', 'Purpose: E-Payment Deal 23 submitted for processing to OFX', '{testDate} 20:01:01.510', '{testDate} 20:01:02.345', '{testDate} 20:01:01.510'),
					(NEWID(), 'AccPaymentApproval', @Payment3, 'DEX', 'Purpose: E-Payment Deal 23 submitted for processing to OFX', '{testDate} 20:01:01.510', '{testDate} 20:01:02.345', '{testDate} 20:01:01.510'),
					(NEWID(), 'AccEPaymentDeal', @Deal4, 'IAK', 'E-Payment Deal 23 accepted by OFX.', '{testDate} 20:02:51.641', '{testDate} 20:03:48.187', '{testDate} 20:02:51.641'),
					(NEWID(), 'AccPaymentApproval', @Payment3, 'IAK', 'E-Payment Deal 23 accepted by OFX.', '{testDate} 20:02:51.641', '{testDate} 20:03:48.187', '{testDate} 20:02:51.641')
			";

			TestConnection.ExecuteNonQuery(sqlQuery);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Deal Usage Statistics Records", 4, transactions.Count());

			var transaction1 = transactions.Single(t => t.ServiceOccuredUTC == new DateTime(2022, 3, 29, 02, 54, 0));
			AssertEquals("(Deal 1) Company Code should be SD1", "SD1", transaction1.GetCompanyCode());
			AssertEquals("(Deal 1) BranchCode should be SD1", "SD1", transaction1.GetBranchCode());
			AssertEquals("(Deal 1) AdditionalRefs should be as expected",
				"{\"Country\":\"AU\"," +
				"\"DealStatus\":\"REQ\"," +
				"\"DealMessage\":\"\"," +
				"\"FromCurrency\":\"AUD\"," +
				"\"ToCurrency\":\"JPY\"," +
				"\"ToAmount\":5500.0000," +
				"\"FeeAmount\":14.0000," +
				"\"PaymentCreatedDate\":\"2022-03-29T00:59:00\"," +
				"\"QuoteCreatedDate\":\"2022-03-29T02:23:00\"," +
				"\"PaymentIsBatch\":0," +
				"\"PaymentRequiresApproval\":1," +
				"\"DealID\":\"\"," +
				"\"OFXAccountName\":\"B_Account\"}"
				, transaction1.AdditionalRefs);

			var transaction2 = transactions.Single(t => t.ServiceOccuredUTC == new DateTime(2022, 3, 29, 02, 13, 0));
			AssertEquals("(Deal 2) Company Code should be SD1", "SD1", transaction2.GetCompanyCode());
			AssertEquals("(Deal 2) BranchCode should be SD1", "SD1", transaction2.GetBranchCode());
			AssertEquals("(Deal 2) AdditionalRefs should be as expected",
				"{\"Country\":\"AU\"," +
				"\"DealStatus\":\"DEC\"," +
				"\"DealMessage\":\"ERROR: message about Deal being declined\"," +
				"\"FromCurrency\":\"AUD\"," +
				"\"ToCurrency\":\"JPY\"," +
				"\"ToAmount\":5381.2400," +
				"\"FeeAmount\":15.0000," +
				"\"PaymentCreatedDate\":\"2022-03-29T00:59:00\"," +
				"\"QuoteCreatedDate\":\"2022-03-29T02:07:00\"," +
				"\"PaymentIsBatch\":0," +
				"\"PaymentRequiresApproval\":1," +
				"\"DealID\":\"\"," +
				"\"OFXAccountName\":\"A_Account\"}"
				, transaction2.AdditionalRefs);

			var transaction3 = transactions.Single(t => t.ServiceOccuredUTC == new DateTime(2022, 3, 29, 21, 42, 0));
			AssertEquals("(Deal 3) Company Code should be SD1", "SD1", transaction3.GetCompanyCode());
			AssertEquals("(Deal 3) BranchCode should be SD1", "SD1", transaction3.GetBranchCode());
			AssertEquals("(Deal 3) AdditionalRefs should be as expected",
				"{\"Country\":\"AU\"," +
				"\"DealStatus\":\"ACP\"," +
				"\"DealMessage\":\"\"," +
				"\"FromCurrency\":\"AUD\"," +
				"\"ToCurrency\":\"JPY\"," +
				"\"ToAmount\":999992.0000," +
				"\"FeeAmount\":201.3300," +
				"\"PaymentCreatedDate\":\"2022-03-29T21:03:00\"," +
				"\"QuoteCreatedDate\":\"2022-03-29T21:24:00\"," +
				"\"PaymentIsBatch\":0," +
				"\"PaymentRequiresApproval\":0," +
				"\"DealID\":\"Reference1\"," +
				"\"OFXAccountName\":\"A_Account\"," +
				"\"RequestTimeSeconds\":40," +
				"\"ResponseProcessingTimeSeconds\":44," +
				"\"TotalTimeSeconds\":84}"
				, transaction3.AdditionalRefs);

			var transaction4 = transactions.Single(t => t.ServiceOccuredUTC == new DateTime(2022, 3, 29, 15, 01, 0));
			AssertEquals("(Deal 4) Company Code should be JTK", "JTK", transaction4.GetCompanyCode());
			AssertEquals("(Deal 4) BranchCode should be TK1", "TK1", transaction4.GetBranchCode());
			AssertEquals("(Deal 4) AdditionalRefs should be as expected",
				"{\"Country\":\"JP\"," +
				"\"DealStatus\":\"PAI\"," +
				"\"DealMessage\":\"\"," +
				"\"FromCurrency\":\"JPY\"," +
				"\"ToCurrency\":\"AUD\"," +
				"\"ToAmount\":1000.0000," +
				"\"FeeAmount\":36.8700," +
				"\"PaymentCreatedDate\":\"2022-03-29T05:28:00\"," +
				"\"QuoteCreatedDate\":\"2022-03-29T12:48:00\"," +
				"\"PaymentIsBatch\":1," +
				"\"PaymentRequiresApproval\":0," +
				"\"DealID\":\"Reference2\"," +
				"\"OFXAccountName\":\"C_Account\"," +
				"\"RequestTimeSeconds\":109," +
				"\"ResponseProcessingTimeSeconds\":57," +
				"\"TotalTimeSeconds\":166}"
				, transaction4.AdditionalRefs);
		}
	}
}
