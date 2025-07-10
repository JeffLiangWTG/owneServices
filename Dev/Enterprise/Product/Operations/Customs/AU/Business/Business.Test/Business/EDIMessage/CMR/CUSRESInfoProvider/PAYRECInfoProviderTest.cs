using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSRES;
using Enterprise.Edifact.D99B.Segments;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class PAYRECInfoProviderTest : D99BCUSRESInfoProviderTest
	{
		public void TestBankDetails()
		{
			CMRPAYRECMessage message = Factory.New<CMRPAYRECMessage>();
			message.EM_MessageText = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::PAYREC+26H1 FG9F 2F6F:1+11'DTM+138:20051108:102'NAD+MR+AAA374M::95'NAD+CM+66015286036::95'NAD+IM+12090995252::95'NAD+VT+AA33HF::95'NAD+COQ+242200::215'NAD+AO+323232::215++TEST ACCOUNT NAME:MORE ACCOUNT NAME'RFF+ABO:B00148665/1/SYD1::1'RFF+ABQ:CATE TEST PARTS TRAN'RFF+ADU:B00148665/1 CATE TES'RFF+ABT:AAAA9XNCG'RFF+RA:AAAA9XNFH'TAX+3'MOA+7:0000000000000.00'TAX+3'MOA+23:0000000000030.10'TAX+3'MOA+9:0000000000825.00'TAX+3'MOA+58:0000000000000.00'TAX+3'MOA+149:0000000000000.00'TAX+3'MOA+369:0000000001765.50'TAX+3'MOA+371:0000000000000.00'TAX+3'MOA+26:0000000000006.50'TAX+3'MOA+206:0000000000000.00'TAX+3'MOA+304:0000000000000.00'TAX+3'MOA+128:0000000002627.10'UNT+37+000001'";
			AssertNotNull("CUSRES Edifact Message", message.CUSRESEdifactMessageForTestOnly);

			PAYRECInfoProvider infoProvider = new PAYRECInfoProvider(message.CUSRESEdifactMessageForTestOnly);
			AssertEquals("BSB number", "242200", infoProvider.BSBNumber);
			AssertEquals("Account Number", "323232", infoProvider.BankAccountNumber);
			AssertEquals("Account Name", "TEST ACCOUNT NAME MORE ACCOUNT NAME", infoProvider.BankAccountName);
		}

		public void TestTotalPayableAdmin()
		{
			CUSRESMessage pAYREC = new CUSRESMessage();
			PAYRECInfoProvider infoProvider = new PAYRECInfoProvider(pAYREC);
			SetUpPayrec(pAYREC, "7");
			AssertEquals(125.30m, infoProvider.TotalPayableAdmin);
		}

		public void TestDeclarationProcessingCharge()
		{
			CUSRESMessage pAYREC = new CUSRESMessage();
			PAYRECInfoProvider infoProvider = new PAYRECInfoProvider(pAYREC);
			SetUpPayrec(pAYREC, "23");
			AssertEquals(125.30m, infoProvider.DeclarationProcessingCharge);
		}

		public void TestTotalPayableDuty()
		{
			CUSRESMessage pAYREC = new CUSRESMessage();
			PAYRECInfoProvider infoProvider = new PAYRECInfoProvider(pAYREC);
			SetUpPayrec(pAYREC, "9");
			AssertEquals(125.30m, infoProvider.TotalPayableDuty);
		}

		public void TestTotalWoodLevy()
		{
			CUSRESMessage pAYREC = new CUSRESMessage();
			PAYRECInfoProvider infoProvider = new PAYRECInfoProvider(pAYREC);
			SetUpPayrec(pAYREC, "58");
			AssertEquals(125.30m, infoProvider.TotalWoodLevy);
		}

		public void TestTotalTILV()
		{
			CUSRESMessage pAYREC = new CUSRESMessage();
			PAYRECInfoProvider provider = new PAYRECInfoProvider(pAYREC);
			SetUpPayrec(pAYREC, "68");
			AssertEquals(125.30m, provider.TotalTILV);
		}

		public void TestTotalPayableWET()
		{
			CUSRESMessage pAYREC = new CUSRESMessage();
			PAYRECInfoProvider infoProvider = new PAYRECInfoProvider(pAYREC);
			SetUpPayrec(pAYREC, "149");
			AssertEquals(125.30m, infoProvider.TotalPayableWET);
		}

		public void TestTotalPayableGST()
		{
			CUSRESMessage pAYREC = new CUSRESMessage();
			PAYRECInfoProvider infoProvider = new PAYRECInfoProvider(pAYREC);
			SetUpPayrec(pAYREC, "369");
			AssertEquals(125.30m, infoProvider.TotalPayableGST);
		}

		public void TestTotalPayableLCT()
		{
			CUSRESMessage pAYREC = new CUSRESMessage();
			PAYRECInfoProvider infoProvider = new PAYRECInfoProvider(pAYREC);
			SetUpPayrec(pAYREC, "371");
			AssertEquals(125.30m, infoProvider.TotalPayableLCT);
		}

		public void TestAQISProcessingCharge()
		{
			CUSRESMessage pAYREC = new CUSRESMessage();
			PAYRECInfoProvider infoProvider = new PAYRECInfoProvider(pAYREC);
			SetUpPayrec(pAYREC, "26");
			AssertEquals(125.30m, infoProvider.AQISProcessingCharge);
		}

		public void TestAQISServicePaymentCharge()
		{
			CUSRESMessage pAYREC = new CUSRESMessage();
			PAYRECInfoProvider infoProvider = new PAYRECInfoProvider(pAYREC);
			SetUpPayrec(pAYREC, "206");
			AssertEquals(125.30m, infoProvider.AQISServicePayment);
		}

		public void TestTotalOtherCharges()
		{
			CUSRESMessage pAYREC = new CUSRESMessage();
			PAYRECInfoProvider infoProvider = new PAYRECInfoProvider(pAYREC);
			SetUpPayrec(pAYREC, "304");
			AssertEquals(125.30m, infoProvider.TotalOtherCharges);
		}

		public void TestTotalPayable()
		{
			CUSRESMessage pAYREC = new CUSRESMessage();
			PAYRECInfoProvider infoProvider = new PAYRECInfoProvider(pAYREC);
			SetUpPayrec(pAYREC, "128");
			AssertEquals(125.30m, infoProvider.TotalPayable);
		}

		void SetUpPayrec(CUSRESMessage pAYREC, string monetaryAmountType)
		{
			SegmentGroup5 group5 = pAYREC.Group5.InstantiateAChildAndAddItToChildrenCollection();
			MOASegment mOA = group5.MOA.InstantiateAChildAndAddItToChildrenCollection();
			mOA.MonetaryAmount.MonetaryAmountTypeCodeQualifier = MonetaryAmountTypeCodeQualifierList.GetFromString(monetaryAmountType);
			mOA.MonetaryAmount.MonetaryAmountValue = "125.30";
		}

		public override void TestDocumentName()
		{
			CUSRESMessage pAYREC = new CUSRESMessage();
			PAYRECInfoProvider infoProvider = new PAYRECInfoProvider(pAYREC);
			AssertEquals("Document name", "PAYREC", infoProvider.DocumentName);
		}

		public void TestEFTRunNumber()
		{
			CUSRESMessage pAYREC = new CUSRESMessage();
			PAYRECInfoProvider infoProvider = new PAYRECInfoProvider(pAYREC);

			SegmentGroup3 group3 = pAYREC.Group3.InstantiateAChildAndAddItToChildrenCollection();
			RFFSegment rFF = group3.RFF.InstantiateAChildAndAddItToChildrenCollection();
			rFF.Reference.ReferenceFunctionCodeQualifier = ReferenceFunctionCodeQualifierList.BanksCommonTransactionReferenceNumber;
			rFF.Reference.ReferenceIdentifier = "BBBXXXYY1";
			AssertEquals("EFTRunNumber", "BBBXXXYY1", infoProvider.EFTRunNumber);
		}

		public void TestICSReceiptNumber()
		{
			CUSRESMessage pAYREC = new CUSRESMessage();
			PAYRECInfoProvider infoProvider = new PAYRECInfoProvider(pAYREC);

			SegmentGroup3 group3 = pAYREC.Group3.InstantiateAChildAndAddItToChildrenCollection();
			RFFSegment rFF = group3.RFF.InstantiateAChildAndAddItToChildrenCollection();
			rFF.Reference.ReferenceFunctionCodeQualifier = ReferenceFunctionCodeQualifierList.RemittanceAdviceNumber;
			rFF.Reference.ReferenceIdentifier = "111222333";
			AssertEquals("ICSReceiptNumber", "111222333", infoProvider.ICSReceiptNumber);
		}

		protected override CUSRESMessage GetCUSRESMessage()
		{
			return new CUSRESMessage();
		}

		protected override D99BCUSRESInfoProvider GetInfoProvider(CUSRESMessage cUSRES)
		{
			return new PAYRECInfoProvider(cUSRES);
		}
	}
}
