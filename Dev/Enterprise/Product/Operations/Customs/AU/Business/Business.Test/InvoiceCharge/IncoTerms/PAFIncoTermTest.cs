using Enterprise.Customs.Business;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class PAFIncoTermTest : Common.Testing.IncoTermTest
	{
		public override void TestITOTIncoterm()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = "LEG";
			var invoice = declaration.Invoices.AddNew();

			AssertEquals("No charges attached, ITOT should be itself", Core.Constants.IncoTerms.PackedAtFactory, IncoTermAndChargeFactory.ITOTIncoTerm(IncotermToTest, invoice));

			BaseJobComInvHeaderCharge pAC = invoice.Charges.AddNew(AUChargeCodeList.Codes.PackingCost, 100m, JobDeclaration.LocalCurrencyConstantCode);
			AssertEquals("ITOT incoterm ", Core.Constants.IncoTerms.UnpackedAtFactory, IncoTermAndChargeFactory.ITOTIncoTerm(IncotermToTest, invoice));

			pAC.J7_IsIncludedInITOT = true;
			AssertEquals("ITOT incoterm", Core.Constants.IncoTerms.PackedAtFactory, IncoTermAndChargeFactory.ITOTIncoTerm(IncotermToTest, invoice));
		}

		public override void TestMissingMandatoryCharges()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = "LEG";
			var invoice = declaration.Invoices.AddNew();

			var result = IncoTermAndChargeFactory.MissingMandatoryCharges(IncotermToTest, invoice);
			AssertEquals("One missing charge", 1, result.Length);

			invoice.Charges.AddNew(AUChargeCodeList.Codes.ForeignInlandFreight, 100m, JobDeclaration.LocalCurrencyConstantCode);
			result = IncoTermAndChargeFactory.MissingMandatoryCharges(IncotermToTest, invoice);
			AssertEquals("No missing Charge", 0, result.Length);
		}

		protected override string IncotermToTest => Core.Constants.IncoTerms.PackedAtFactory;

		protected override bool ExpectedValueForCanThisIncoTermHaveThisCharge(ICustomsChargeCode charge)
			=> charge.IsIncoTermNeutral || charge.Code == AUChargeCodeList.Codes.PackingCost || charge.Code == AUChargeCodeList.Codes.ExWorks;

		protected override bool ExpectedValueForThisChargeMandatory(ICustomsChargeCode charge) => charge.Code == AUChargeCodeList.Codes.ForeignInlandFreight;

		protected override bool ExpectedValueForThisChargeRecommeded(ICustomsChargeCode charge)
			=> base.ExpectedValueForThisChargeRecommeded(charge) || ExpectedValueForThisChargeMandatory(charge);

		protected override string CountryContext() => JobDeclaration.AUEdifice;
	}
}
