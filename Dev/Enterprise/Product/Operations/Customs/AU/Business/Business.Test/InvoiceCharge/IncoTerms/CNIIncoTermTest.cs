using Enterprise.Customs.Common;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CNIIncoTermTest : Common.Testing.IncoTermTest
	{
		public override void TestITOTIncoterm()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = "LEG";
			var invoice = declaration.Invoices.AddNew();

			AssertEquals("No charges attached, ITOT should be itself", Core.Constants.IncoTerms.CostAndInsurance, IncoTermAndChargeFactory.ITOTIncoTerm(IncotermToTest, invoice));

			var oNS = invoice.Charges.AddNew(AUChargeCodeList.Codes.OverseasInsurance, 100m, JobDeclaration.LocalCurrencyConstantCode);
			var pAC = invoice.Charges.AddNew(AUChargeCodeList.Codes.PackingCost, 100m, JobDeclaration.LocalCurrencyConstantCode);

			AssertEquals("With ONS and PAC, ITOT Incoterm should be UFB", Core.Constants.IncoTerms.UnpackedFreeOnBoard, IncoTermAndChargeFactory.ITOTIncoTerm(IncotermToTest, invoice));

			pAC.J7_IsIncludedInITOT = true;
			AssertEquals("With PAC in lines, ITOT Incoterm should be FOB", Core.Constants.IncoTerms.FreeOnBoard, IncoTermAndChargeFactory.ITOTIncoTerm(IncotermToTest, invoice));

			oNS.J7_IsIncludedInITOT = true;
			AssertEquals("With OFT in lines, ITOT Incoterm should be C&I", Core.Constants.IncoTerms.CostAndInsurance, IncoTermAndChargeFactory.ITOTIncoTerm(IncotermToTest, invoice));
		}

		public override void TestMissingMandatoryCharges()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = "LEG";
			var invoice = declaration.Invoices.AddNew();

			var result = IncoTermAndChargeFactory.MissingMandatoryCharges(IncotermToTest, invoice);
			AssertEquals("One missing charge", 1, result.Length);

			invoice.Charges.AddNew(AUChargeCodeList.Codes.OverseasInsurance, 100m, JobDeclaration.LocalCurrencyConstantCode);
			result = IncoTermAndChargeFactory.MissingMandatoryCharges(IncotermToTest, invoice);
			AssertEquals("No missing Charge", 0, result.Length);
		}

		protected override string CountryContext() => JobDeclaration.AUEdifice;

		protected override string IncotermToTest => Core.Constants.IncoTerms.CostAndInsurance;

		protected override bool ExpectedValueForCanThisIncoTermHaveThisCharge(ICustomsChargeCode charge)
		{
			return charge.IsIncoTermNeutral
				|| charge.Code == AUChargeCodeList.Codes.OverseasInsurance
				|| charge.Code == AUChargeCodeList.Codes.PackingCost
				|| charge.Code == AUChargeCodeList.Codes.ForeignInlandFreight
				|| charge.Code == AUChargeCodeList.Codes.ExWorks;
		}

		protected override bool ExpectedValueForThisChargeMandatory(ICustomsChargeCode chargeCode) => chargeCode.Code == AUChargeCodeList.Codes.OverseasInsurance;
	}
}
