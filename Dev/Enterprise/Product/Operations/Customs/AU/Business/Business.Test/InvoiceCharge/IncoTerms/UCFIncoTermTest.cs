using Enterprise.Customs.Business;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class UCFIncoTermTest : UnpackedIncoTermTest
	{
		public override void TestITOTIncoterm()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = "LEG";
			var invoice = declaration.Invoices.AddNew();

			AssertEquals("No charges attached, ITOT should be itself", IncotermToTest, IncoTermAndChargeFactory.ITOTIncoTerm(IncotermToTest, invoice));

			BaseJobComInvHeaderCharge oFT = invoice.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 100m, JobDeclaration.LocalCurrencyConstantCode);
			AssertEquals("With OFT and PAC, ITOT Incoterm should be UFB", Core.Constants.IncoTerms.UnpackedFreeOnBoard, IncoTermAndChargeFactory.ITOTIncoTerm(IncotermToTest, invoice));

			oFT.J7_IsIncludedInITOT = true;
			AssertEquals("With OFT in lines, ITOT Incoterm should be UCF", Core.Constants.IncoTerms.UnpackedCostAndFreight, IncoTermAndChargeFactory.ITOTIncoTerm(IncotermToTest, invoice));
		}

		public override void TestMissingMandatoryCharges()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = "LEG";
			var invoice = declaration.Invoices.AddNew();

			var result = IncoTermAndChargeFactory.MissingMandatoryCharges(IncotermToTest, invoice);
			AssertEquals("Two missing charge", 2, result.Length);

			invoice.Charges.AddNew(AUChargeCodeList.Codes.PackingCost, 100m, JobDeclaration.LocalCurrencyConstantCode);
			result = IncoTermAndChargeFactory.MissingMandatoryCharges(IncotermToTest, invoice);
			AssertEquals("One missing Charge", 1, result.Length);

			invoice.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 100m, JobDeclaration.LocalCurrencyConstantCode);
			result = IncoTermAndChargeFactory.MissingMandatoryCharges(IncotermToTest, invoice);
			AssertEquals("No missing Charge", 0, result.Length);
		}

		protected override string IncotermToTest => Core.Constants.IncoTerms.UnpackedCostAndFreight;

		protected override bool ExpectedValueForCanThisIncoTermHaveThisCharge(ICustomsChargeCode charge)
			=> base.ExpectedValueForCanThisIncoTermHaveThisCharge(charge) || charge.Code == AUChargeCodeList.Codes.ExWorks || charge.Code == AUChargeCodeList.Codes.OverseasFreight || charge.Code == AUChargeCodeList.Codes.ForeignInlandFreight;

		protected override bool ExpectedValueForThisChargeMandatory(ICustomsChargeCode charge)
			=> base.ExpectedValueForThisChargeMandatory(charge) || charge.Code == AUChargeCodeList.Codes.OverseasFreight;
	}
}
