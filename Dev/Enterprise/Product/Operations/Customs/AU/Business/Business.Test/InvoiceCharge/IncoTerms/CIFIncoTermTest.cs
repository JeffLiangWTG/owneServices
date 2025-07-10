using Enterprise.Customs.Common;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CIFIncoTermTest : Common.Testing.BaseCIFIncoTermTest
	{
		public override void TestITOTIncoterm()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = "LEG";
			var invoice = declaration.Invoices.AddNew();

			AssertEquals("No charges attached, ITOT should be itself", Core.Constants.IncoTerms.CostInsuranceAndFreight, IncoTermAndChargeFactory.ITOTIncoTerm(IncotermToTest, invoice));

			var oNS = invoice.Charges.AddNew(AUChargeCodeList.Codes.OverseasInsurance, 100m, JobDeclaration.LocalCurrencyConstantCode);
			var oFT = invoice.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 100m, JobDeclaration.LocalCurrencyConstantCode);
			var pAC = invoice.Charges.AddNew(AUChargeCodeList.Codes.PackingCost, 100m, JobDeclaration.LocalCurrencyConstantCode);

			AssertEquals("With ONS, OFT and PAC, ITOT Incoterm should be UFB", Core.Constants.IncoTerms.UnpackedFreeOnBoard, IncoTermAndChargeFactory.ITOTIncoTerm(IncotermToTest, invoice));

			pAC.J7_IsIncludedInITOT = true;
			AssertEquals("With PAC in lines, ITOT Incoterm should be FOB", Core.Constants.IncoTerms.FreeOnBoard, IncoTermAndChargeFactory.ITOTIncoTerm(IncotermToTest, invoice));

			oFT.J7_IsIncludedInITOT = true;
			AssertEquals("With OFT in lines, ITOT Incoterm should be CFR", Core.Constants.IncoTerms.CostFreightWithAmpersand, IncoTermAndChargeFactory.ITOTIncoTerm(IncotermToTest, invoice));

			oNS.J7_IsIncludedInITOT = true;
			AssertEquals("With OFT & ONS in lines, ITOT Incoterm should be CIF", Core.Constants.IncoTerms.CostInsuranceAndFreight, IncoTermAndChargeFactory.ITOTIncoTerm(IncotermToTest, invoice));

			oFT.J7_IsIncludedInITOT = false;
			oNS.J7_IsIncludedInITOT = true;
			AssertEquals("ITOT incoterm", Core.Constants.IncoTerms.CostAndInsurance, IncoTermAndChargeFactory.ITOTIncoTerm(IncotermToTest, invoice));

			pAC.J7_IsIncludedInITOT = false;
			oFT.J7_IsIncludedInITOT = true;
			oNS.J7_IsIncludedInITOT = false;
			AssertEquals("ITOT incoterm", Core.Constants.IncoTerms.UnpackedCostAndFreight, IncoTermAndChargeFactory.ITOTIncoTerm(IncotermToTest, invoice));

			pAC.J7_IsIncludedInITOT = false;
			oFT.J7_IsIncludedInITOT = true;
			oNS.J7_IsIncludedInITOT = true;
			AssertEquals("ITOT incoterm", Core.Constants.IncoTerms.UnpackedCostInsuranceAndFreight, IncoTermAndChargeFactory.ITOTIncoTerm(IncotermToTest, invoice));
		}

		protected override string CountryContext() => JobDeclaration.AUEdifice;

		protected override bool ExpectedValueForCanThisIncoTermHaveThisCharge(ICustomsChargeCode charge)
		{
			return charge.IsIncoTermNeutral
				|| charge.Code == CustomsChargeTypeList.Codes.ExWorks
				|| charge.Code == CustomsChargeTypeList.Codes.PackingCost
				|| charge.Code == CustomsChargeTypeList.Codes.ForeignInlandFreight
				|| charge.Code == CustomsChargeTypeList.Codes.OverseasFreight
				|| charge.Code == CustomsChargeTypeList.Codes.OverseasInsurance;
		}
	}
}
