using CargoWise.Types;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSRES;
using Enterprise.Edifact.D99B.Segments;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class IMDRInfoProviderTest : D99BCUSRESInfoProviderTest
	{
		public override void TestDocumentName()
		{
			IMDRInfoProvider provider = new IMDRInfoProvider(IMDR);
			AssertEquals("Document name", "FID", provider.DocumentName);
		}

		public void TestAQISContainerCharge()
		{
			CUSRESMessage iMDR = new CUSRESMessage();
			IMDRInfoProvider provider = new IMDRInfoProvider(iMDR);
			SetMonetaryAmounts(iMDR);
			AssertEquals(100m, provider.AQISContainerCharge);
		}

		public void TestAQISProcessingCharge()
		{
			CUSRESMessage iMDR = new CUSRESMessage();
			IMDRInfoProvider provider = new IMDRInfoProvider(iMDR);
			SetMonetaryAmounts(iMDR);
			AssertEquals(110m, provider.AQISProcessingCharge);
		}

		public void TestDeclarationProcessingCharge()
		{
			CUSRESMessage iMDR = new CUSRESMessage();
			IMDRInfoProvider provider = new IMDRInfoProvider(iMDR);
			SetMonetaryAmounts(iMDR);
			AssertEquals(120m, provider.DeclarationProcessingCharge);
		}

		public void TestTotalOtherCharges()
		{
			CUSRESMessage iMDR = new CUSRESMessage();
			IMDRInfoProvider provider = new IMDRInfoProvider(iMDR);
			SetMonetaryAmounts(iMDR);
			AssertEquals(130m, provider.TotalOtherCharges);
		}

		public void TestTotalPayableAdmin()
		{
			CUSRESMessage iMDR = new CUSRESMessage();
			IMDRInfoProvider provider = new IMDRInfoProvider(iMDR);
			SetMonetaryAmounts(iMDR);
			AssertEquals(140m, provider.TotalPayableAdmin);
		}

		public void TestTotalWoodLevy()
		{
			CUSRESMessage iMDR = new CUSRESMessage();
			IMDRInfoProvider provider = new IMDRInfoProvider(iMDR);
			SetMonetaryAmounts(iMDR);
			AssertEquals(150m, provider.TotalWoodLevy);
		}

		public void TestTotalTILV()
		{
			CUSRESMessage iMDR = new CUSRESMessage();
			IMDRInfoProvider provider = new IMDRInfoProvider(iMDR);
			SetMonetaryAmounts(iMDR);
			AssertEquals(123.45m, provider.TotalTILV);
		}

		public void TestTotalPayable()
		{
			CUSRESMessage iMDR = new CUSRESMessage();
			IMDRInfoProvider provider = new IMDRInfoProvider(iMDR);
			SetMonetaryAmounts(iMDR);
			AssertEquals(160m, provider.TotalPayable);
		}

		public void TestAQISServicePayment()
		{
			CUSRESMessage iMDR = new CUSRESMessage();
			IMDRInfoProvider provider = new IMDRInfoProvider(iMDR);
			SetMonetaryAmounts(iMDR);
			AssertEquals(170m, provider.AQISServicePayment);
		}

		public void TestTotalPayableDuty()
		{
			CUSRESMessage iMDR = new CUSRESMessage();
			IMDRInfoProvider provider = new IMDRInfoProvider(iMDR);
			SetMonetaryAmounts(iMDR);
			AssertEquals(180m, provider.TotalPayableDuty);
		}

		public void TestTotalPayableWET()
		{
			CUSRESMessage iMDR = new CUSRESMessage();
			IMDRInfoProvider provider = new IMDRInfoProvider(iMDR);
			SetMonetaryAmounts(iMDR);
			AssertEquals(181m, provider.TotalPayableWET);
		}

		public void TestTotalPayableGST()
		{
			CUSRESMessage iMDR = new CUSRESMessage();
			IMDRInfoProvider provider = new IMDRInfoProvider(iMDR);
			SetMonetaryAmounts(iMDR);
			AssertEquals(182m, provider.TotalPayableGST);
		}

		public void TestTotalPayableLCT()
		{
			CUSRESMessage iMDR = new CUSRESMessage();
			IMDRInfoProvider provider = new IMDRInfoProvider(iMDR);
			SetMonetaryAmounts(iMDR);
			AssertEquals(183m, provider.TotalPayableLCT);
		}

		public void TestTotalDeferredGST()
		{
			CUSRESMessage iMDR = new CUSRESMessage();
			IMDRInfoProvider provider = new IMDRInfoProvider(iMDR);
			SetMonetaryAmounts(iMDR);
			AssertEquals(184m, provider.TotalDeferredGST);
		}

		public void TestTotalSecurityConcession()
		{
			var iMDR = new CUSRESMessage();
			var provider = new IMDRInfoProvider(iMDR);
			SetMonetaryAmounts(iMDR);
			AssertEquals(185m, provider.TotalSecurityConcession);
		}

		public void TestTotalSecurityLiability()
		{
			var iMDR = new CUSRESMessage();
			var provider = new IMDRInfoProvider(iMDR);
			SetMonetaryAmounts(iMDR);
			AssertEquals(186m, provider.TotalSecurityLiability);
		}

		void SetMonetaryAmounts(CUSRESMessage iMDR)
		{
			SetMonetaryAmount(iMDR, "35", 100m);//AQIS Container charge
			SetMonetaryAmount(iMDR, "26", 110m);//AQIS Processing Charge
			SetMonetaryAmount(iMDR, "23", 120m);//Declaration Processing Charge
			SetMonetaryAmount(iMDR, "304", 130m);//Total Other Charge
			SetMonetaryAmount(iMDR, "7", 140m);//Total Payable Admin
			SetMonetaryAmount(iMDR, "58", 150m);//Total Wood Levy
			SetMonetaryAmount(iMDR, "68", 123.45m);//Total TILV
			SetMonetaryAmount(iMDR, "128", 160m);//Total Payable
			SetMonetaryAmount(iMDR, "206", 170m);//AQIS Service Payment Amount
			SetMonetaryAmount(iMDR, "9", 180m);//Total Payable Duty
			SetMonetaryAmount(iMDR, "149", 181m);//Total Payable WET
			SetMonetaryAmount(iMDR, "369", 182m);//Total Payable GST
			SetMonetaryAmount(iMDR, "371", 183m);//Total Payable LCT
			SetMonetaryAmount(iMDR, "210", 184m);//Total Deferred GST
			SetMonetaryAmount(iMDR, "292", 185m);//Total Security Concession
			SetMonetaryAmount(iMDR, "Z01", 186m);//Total Security Liability
		}

		void SetMonetaryAmount(CUSRESMessage iMDR, string monetaryAmountType, ZDecimal amount)
		{
			SegmentGroup5 group5 = iMDR.Group5.InstantiateAChildAndAddItToChildrenCollection();
			MOASegment mOA = group5.MOA.InstantiateAChildAndAddItToChildrenCollection();
			mOA.MonetaryAmount.MonetaryAmountTypeCodeQualifier = MonetaryAmountTypeCodeQualifierList.GetFromString(monetaryAmountType);
			mOA.MonetaryAmount.MonetaryAmountValue = amount.ToString(2);
		}

		protected override D99BCUSRESInfoProvider GetInfoProvider(CUSRESMessage cUSRES)
		{
			return new IMDRInfoProvider(IMDR);
		}

		protected override CUSRESMessage GetCUSRESMessage()
		{
			return IMDR;
		}

		CUSRESMessage IMDR
		{
			get
			{
				if (fIMDR == null)
				{
					fIMDR = new CUSRESMessage();
				}
				return fIMDR;
			}
		}
		CUSRESMessage fIMDR;
	}
}
