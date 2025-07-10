using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class JobDeclarationAmendmentMessageSendingObjectLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestYNCodeList()
		{
			AssertEquals("N, Y", sendingObject.Lookups.YNCodeList.CodesAsString);
		}

		public void TestFaultPartyList()
		{
			AssertEquals("01, 02, 03, 04, 07, 08, 09, 99", sendingObject.Lookups.FaultPartyList.CodesAsString);
		}

		public void TestRefundTypeList()
		{
			AssertEquals("A, B, C, D, E", sendingObject.Lookups.RefundTypeList.CodesAsString);
		}

		public void TestRefundCauseCodeList()
		{
			AssertEquals("01, 02, 03, 04, 05, 06, 07, 08, 09, 10, 11, 12, 13", sendingObject.Lookups.RefundCauseCodeList.CodesAsString);
		}

		public void TestRefundReasonCodeList()
		{
			AssertEquals("01, 02, 03", sendingObject.Lookups.RefundReasonCodeList.CodesAsString);
		}

		public void TestTaxOfficeList()
		{
			sendingObject.Lookups.TaxOfficeList.Load();
			AssertEquals(1, sendingObject.Lookups.TaxOfficeList.Count);
			AssertEquals("010", sendingObject.Lookups.TaxOfficeList[0].ZZD_Code);
			AssertEquals("국세청", sendingObject.Lookups.TaxOfficeList[0].ZZD_Description);
		}

		public void TestDutyPenaltyTypeCodeList()
		{
			AssertEquals("01, 02, 0A, 03, 04, 0B, 05", sendingObject.Lookups.DutyPenaltyTypeCodeList.CodesAsString);
		}
		public void TestDutyPenaltyReducedYNCodeList()
		{
			AssertEquals("Y, N", sendingObject.Lookups.DutyPenaltyReducedYNCodeList.CodesAsString);
		}
		public void TestDomesticTaxPenaltyTypeCodeList()
		{
			AssertEquals("01, 02, 0A, 03, 04, 0B, 05", sendingObject.Lookups.DomesticTaxPenaltyTypeCodeList.CodesAsString);
		}
		public void TestDutyPenaltyExemptionCodeList()
		{
			var entry = new TestDataSetupHelper(Factory).Create929SnapShot(BondedFactoryUseCodeList.Codes.A);
			var sendingObj = new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5FE);
			AssertEquals(DutyTaxCorrectionCodeList.Codes.X + DeclarationCorrectionCodeList.Codes.X, sendingObj.AmendmentType);
			AssertEquals("X", sendingObj.Lookups.DutyPenaltyExemptionCodeList.CodesAsString);

			entry.Declaration.UnderbondMovementArrivalDate = new ZDateTime("2024-02-01 12:30:00");
			var vat = entry.Charges[0];
			vat.C1_ChargeAmount = vat.C1_ChargeAmount + 1m;
			var statement1 = Factory.New<CusStatementHeader>();
			statement1.B2_ProcessDate = new ZDateTime(2024, 2, 1);
			statement1.B2_StatementType = StatementHeaderTypeList.Codes.CustomsDisbursementBill;
			statement1.B2_PaymentStatus = StatementHeaderPaymentStatusList.Codes.PYC;
			var line = statement1.StatementLines.AddNew();
			line.B3_EntryType = KRJobMessageTypeList.Codes.Import;
			line.B3_EntryNum = "1234520000045M";
			statement1.B2_PaymentAuthorizationDate = ZDateTime.Today.AddMonths(-5);
			Factory.Save();

			entry.Reload();
			sendingObj = new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5FE);
			AssertEquals(DutyTaxCorrectionCodeList.Codes.A + DeclarationCorrectionCodeList.Codes.D, sendingObj.AmendmentType);
			AssertEquals("Y, N", sendingObj.Lookups.DutyPenaltyExemptionCodeList.CodesAsString);
		}
		public void TestAdditiveTaxExemptionReasonCodeList()
		{
			AssertEquals("A1, A2, A3, A4, A5, A6, B1, B2, B3, B4, B5, B6, B7, B8, B9, C1, C2", sendingObject.Lookups.AdditiveTaxExemptionReasonCodeList.CodesAsString);
		}
		public void TestPenaltyExemption5UAOnlyCodeList()
		{
			AssertEquals("A3, A4, A6, B5, B7, B9", sendingObject.Lookups.PenaltyExemption5UAOnlyCodeList.CodesAsString);
		}
		public void TestDetectionPatternCodeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(ZZ.NKCodeType.KR006, "Detection pattern code");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, ZZ.NKCodeType.KR006, "U00000", "물류 관련 오류항목", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, ZZ.NKCodeType.KR006, "UA0100", "적하목록 작성/제출", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			Factory.Save();

			var messageSendingObject = new JobDeclarationAmendmentMessageSendingObject(Factory.New<CusEntryHeader>(), ElectronicDocumentTypeList.Codes._5FE);
			var detectionPatternCodeList = messageSendingObject.Lookups.DetectionPatternCodeList;
			detectionPatternCodeList.Load();

			Assert(detectionPatternCodeList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "U00000"));
			Assert(detectionPatternCodeList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Description == "물류 관련 오류항목"));
			Assert(detectionPatternCodeList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "UA0100"));
			Assert(detectionPatternCodeList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Description == "적하목록 작성/제출"));
			Assert(!detectionPatternCodeList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "UA0601"));
			Assert(!detectionPatternCodeList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Description == "재고조사"));
		}

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Messaging.Constants.ZZ.NKCodeType.TaxOffice, "Tax Office");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Messaging.Constants.ZZ.NKCodeType.TaxOffice, "010", "국세청", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			Factory.Save();

			var entry = Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew();

			sendingObject = new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5FE);
		}
		JobDeclarationAmendmentMessageSendingObject sendingObject;
	}
}
