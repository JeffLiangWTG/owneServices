using Enterprise.Customs.Common;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class LISIncoTermTest : Common.Testing.BaseDDPIncoTermTest
	{
		public override void TestITOTIncoterm()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = "LEG";
			var invoice = declaration.Invoices.AddNew();

			AssertEquals("No charges attached, ITOT should be itself", Core.Constants.IncoTerms.LandedIntoStore, IncoTermAndChargeFactory.ITOTIncoTerm(IncotermToTest, invoice));

			var lCH = invoice.Charges.AddNew(AUChargeCodeList.Codes.LandingCharges, 100m, JobDeclaration.LocalCurrencyConstantCode);
			AssertEquals("With LCH, ITOT Incoterm should be CIF", Core.Constants.IncoTerms.CostInsuranceAndFreight, IncoTermAndChargeFactory.ITOTIncoTerm(IncotermToTest, invoice));

			lCH.J7_IsIncludedInITOT = true;
			AssertEquals("With LCH, ITOT Incoterm should be LIS", Core.Constants.IncoTerms.LandedIntoStore, IncoTermAndChargeFactory.ITOTIncoTerm(IncotermToTest, invoice));
		}

		protected override bool ExpectedValueForCanThisIncoTermHaveThisCharge(ICustomsChargeCode charge) => true;

		protected override string IncotermToTest => Core.Constants.IncoTerms.LandedIntoStore;

		protected override string CountryContext() => JobDeclaration.AUEdifice;
	}
}
