using Enterprise.Customs.Common;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class FOBIncoTermTest : Common.Testing.BaseFOBIncoTermTest
	{
		public override void TestITOTIncoterm()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = "LEG";
			var invoice = declaration.Invoices.AddNew();

			AssertEquals("No charges attached, ITOT should be itself", Core.Constants.IncoTerms.FreeOnBoard, IncoTermAndChargeFactory.ITOTIncoTerm(IncotermToTest, invoice));

			var pAC = invoice.Charges.AddNew(AUChargeCodeList.Codes.PackingCost, 100m, JobDeclaration.LocalCurrencyConstantCode);
			AssertEquals("With PAC, ITOT Incoterm should be UFB", Core.Constants.IncoTerms.UnpackedFreeOnBoard, IncoTermAndChargeFactory.ITOTIncoTerm(IncotermToTest, invoice));

			pAC.J7_IsIncludedInITOT = true;
			AssertEquals("With PAC in lines, ITOT Incoterm should be FOB", Core.Constants.IncoTerms.FreeOnBoard, IncoTermAndChargeFactory.ITOTIncoTerm(IncotermToTest, invoice));
		}

		protected override string CountryContext() => JobDeclaration.AUEdifice;

		protected override bool ExpectedValueForCanThisIncoTermHaveThisCharge(ICustomsChargeCode charge)
		{
			return charge.IsIncoTermNeutral
				|| charge.Code == CustomsChargeTypeList.Codes.ExWorks
				|| charge.Code == CustomsChargeTypeList.Codes.PackingCost
				|| charge.Code == CustomsChargeTypeList.Codes.ForeignInlandFreight;
		}
	}
}
