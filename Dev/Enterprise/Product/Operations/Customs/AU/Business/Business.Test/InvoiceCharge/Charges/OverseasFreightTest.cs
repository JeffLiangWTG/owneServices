using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class OverseasFreightTest : Common.Testing.OverseasFreightTest
	{
		protected override string GetCountryContext() => JobDeclaration.AUEdifice;

		protected override Common.ICustomsChargeCode GetChargeCodeToTest() => EdificeIncoTermAndCustomsChargeFactory.EdificeOverseasFreight;

		protected override bool ExpectedGetCalculatedIncludedInITOT(ZString incoterm, bool userEnteredIsDutiable)
		{
			return incoterm != Core.Constants.IncoTerms.ExWorks &&
				incoterm != Core.Constants.IncoTerms.PackedAtFactory &&
				incoterm != Core.Constants.IncoTerms.FreeOnBoard &&
				incoterm != Core.Constants.IncoTerms.UnpackedAtFactory &&
				incoterm != Core.Constants.IncoTerms.UnpackedFreeOnBoard &&
				incoterm != Core.Constants.IncoTerms.CostAndInsurance &&
				incoterm != "XXX";
		}
	}
}
