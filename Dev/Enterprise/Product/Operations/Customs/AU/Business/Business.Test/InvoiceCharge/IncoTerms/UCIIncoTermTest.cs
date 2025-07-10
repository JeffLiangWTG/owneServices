using Enterprise.Customs.Business;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class UCIIncoTermTest : UnpackedIncoTermTest
	{
		public override void TestITOTIncoterm()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = "LEG";
			var invoice = declaration.Invoices.AddNew();

			AssertEquals("No charges attached, ITOT should be itself", IncotermToTest, IncoTermAndChargeFactory.ITOTIncoTerm(IncotermToTest, invoice));

			BaseJobComInvHeaderCharge oNS = invoice.Charges.AddNew(AUChargeCodeList.Codes.OverseasInsurance, 100m, JobDeclaration.LocalCurrencyConstantCode);
			AssertEquals("With OFT and PAC, ITOT Incoterm should be UCF", Core.Constants.IncoTerms.UnpackedCostAndFreight, IncoTermAndChargeFactory.ITOTIncoTerm(IncotermToTest, invoice));

			oNS.J7_IsIncludedInITOT = true;
			AssertEquals("With OFT in lines, ITOT Incoterm should be UCI", Core.Constants.IncoTerms.UnpackedCostInsuranceAndFreight, IncoTermAndChargeFactory.ITOTIncoTerm(IncotermToTest, invoice));
		}

		public override void TestMissingMandatoryCharges()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = "LEG";
			var invoice = declaration.Invoices.AddNew();

			var result = IncoTermAndChargeFactory.MissingMandatoryCharges(IncotermToTest, invoice);
			AssertEquals("Missing charge", 3, result.Length);

			invoice.Charges.AddNew(AUChargeCodeList.Codes.PackingCost, 100m, JobDeclaration.LocalCurrencyConstantCode);
			result = IncoTermAndChargeFactory.MissingMandatoryCharges(IncotermToTest, invoice);
			AssertEquals("Missing Charge", 2, result.Length);

			invoice.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 100m, JobDeclaration.LocalCurrencyConstantCode);
			result = IncoTermAndChargeFactory.MissingMandatoryCharges(IncotermToTest, invoice);
			AssertEquals("Missing Charge", 1, result.Length);

			invoice.Charges.AddNew(AUChargeCodeList.Codes.OverseasInsurance, 100m, JobDeclaration.LocalCurrencyConstantCode);
			result = IncoTermAndChargeFactory.MissingMandatoryCharges(IncotermToTest, invoice);
			AssertEquals("No missing Charge", 0, result.Length);
		}

		protected override string IncotermToTest => Core.Constants.IncoTerms.UnpackedCostInsuranceAndFreight;

		protected override bool ExpectedValueForCanThisIncoTermHaveThisCharge(ICustomsChargeCode chargeCode)
		{
			return base.ExpectedValueForCanThisIncoTermHaveThisCharge(chargeCode)
				|| chargeCode.Code == AUChargeCodeList.Codes.ExWorks || chargeCode.Code == AUChargeCodeList.Codes.OverseasFreight
				|| chargeCode.Code == AUChargeCodeList.Codes.ForeignInlandFreight || chargeCode.Code == AUChargeCodeList.Codes.OverseasInsurance;
		}

		protected override bool ExpectedValueForThisChargeMandatory(ICustomsChargeCode chargeCode)
			=> base.ExpectedValueForThisChargeMandatory(chargeCode) || chargeCode.Code == AUChargeCodeList.Codes.OverseasFreight || chargeCode.Code == AUChargeCodeList.Codes.OverseasInsurance;
	}
}
