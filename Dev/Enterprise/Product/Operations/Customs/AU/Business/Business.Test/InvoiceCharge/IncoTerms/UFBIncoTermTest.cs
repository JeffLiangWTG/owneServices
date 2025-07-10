using Enterprise.Customs.Common;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class UFBIncoTermTest : UnpackedIncoTermTest
	{
		public override void TestITOTIncoterm()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = "LEG";
			var invoice = declaration.Invoices.AddNew();

			AssertEquals("No charges attached, ITOT should be itself", IncotermToTest, IncoTermAndChargeFactory.ITOTIncoTerm(IncotermToTest, invoice));

			var fift = invoice.Charges.AddNew(AUChargeCodeList.Codes.ForeignInlandFreight, 100m, JobDeclaration.LocalCurrencyConstantCode);
			AssertEquals("With FIFT, ITOT Incoterm should be UAF", Core.Constants.IncoTerms.UnpackedAtFactory, IncoTermAndChargeFactory.ITOTIncoTerm(IncotermToTest, invoice));

			fift.J7_IsIncludedInITOT = true;
			AssertEquals("With FIFT in lines, ITOT Incoterm should be UFB", Core.Constants.IncoTerms.UnpackedFreeOnBoard, IncoTermAndChargeFactory.ITOTIncoTerm(IncotermToTest, invoice));
		}

		public override void TestMissingMandatoryCharges()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = "LEG";
			var invoice = declaration.Invoices.AddNew();

			var result = IncoTermAndChargeFactory.MissingMandatoryCharges(IncotermToTest, invoice);
			AssertEquals("Missing charge", 1, result.Length);

			invoice.Charges.AddNew(AUChargeCodeList.Codes.PackingCost, 100m, JobDeclaration.LocalCurrencyConstantCode);
			result = IncoTermAndChargeFactory.MissingMandatoryCharges(IncotermToTest, invoice);
			AssertEquals("Missing Charge", 0, result.Length);
		}

		protected override string IncotermToTest => Core.Constants.IncoTerms.UnpackedFreeOnBoard;

		protected override bool ExpectedValueForCanThisIncoTermHaveThisCharge(ICustomsChargeCode chargeCode)
			=> base.ExpectedValueForCanThisIncoTermHaveThisCharge(chargeCode) || chargeCode.Code == AUChargeCodeList.Codes.ExWorks || chargeCode.Code == AUChargeCodeList.Codes.ForeignInlandFreight;
	}
}
