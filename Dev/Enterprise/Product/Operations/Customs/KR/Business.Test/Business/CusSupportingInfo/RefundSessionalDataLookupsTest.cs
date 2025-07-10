using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class RefundSessionalDataLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestRefundCauseCode()
		{
			AssertEquals("01, 02, 03, 04, 05, 06, 07, 08, 09, 10, 11, 12, 13", sessionalData.Lookups.RefundCauseCodeList.CodesAsString);
		}

		public void TestRefundReasonCode()
		{
			AssertEquals("01, 02, 03", sessionalData.Lookups.RefundReasonCodeList.CodesAsString);
		}

		public void TestRefundTypeList()
		{
			AssertEquals("A, B, C, D, E", sessionalData.Lookups.RefundTypeList.CodesAsString);
		}
		public void TestYesNoList()
		{
			AssertEquals("N, Y", sessionalData.Lookups.YesNoList.CodesAsString);
		}

		public void TestCustomsDisbursementBillNumberList()
		{
			CreateKREntryCustomsBillsView(StatementHeaderTypeList.Codes.CustomsDisbursementBill, "2234567890123456789", StatementHeaderPaymentStatusList.Codes.PYC);
			CreateKREntryCustomsBillsView(StatementHeaderTypeList.Codes.Invoice, "1234567890123456789", StatementHeaderPaymentStatusList.Codes.PYC);

			AssertEquals(2, sessionalData.Lookups.CustomsDisbursementBillNumberList.Count);
			Assert(sessionalData.Lookups.CustomsDisbursementBillNumberList.ContainsCode("2234567890123456789"));
			Assert(sessionalData.Lookups.CustomsDisbursementBillNumberList.ContainsCode("1234567890123456789"));

			void CreateKREntryCustomsBillsView(string type, string billNumber, string paymentStatus)
			{
				var view = Factory.New<KREntryCustomsBillsView>();
				view.KEB_StatementType = type;
				view.KEB_CustomsDisbursementBillNumber = billNumber;
				view.KEB_PaymentStatus = paymentStatus;
				view.KEB_BranchPK = entry.Declaration.JE_GB;
				view.KEB_GC = entry.Declaration.JE_GC;
				view.KEB_ImportEntryNum = entry.EntryNumber;
			}
		}

		protected override void SetUp()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var amendmentSessionData = new AmendmentSessionalDataCollection(instruction).AddNew();
			amendmentSessionData.CSI_Code = DutyTaxCorrectionCodeList.Codes.C;
			sessionalData = new RefundSessionalDataCollection(instruction, amendmentSessionData.PK).AddNew();
			sessionalData.CSI_CSI_SupportingInfo = amendmentSessionData.PK;

			entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = sessionalData.Parent.PK;
			entry.EntryNumber = "1234520000045M";
		}
		RefundSessionalData sessionalData;
		CusEntryHeader entry;
	}
}
