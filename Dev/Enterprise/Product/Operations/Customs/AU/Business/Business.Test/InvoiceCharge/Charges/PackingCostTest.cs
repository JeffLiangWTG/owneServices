using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class PackingCostTest : Common.Testing.PackingCostTest
	{
		protected override string GetCountryContext() => JobDeclaration.AUEdifice;

		protected override ICustomsChargeCode GetChargeCodeToTest() => EdificeIncoTermAndCustomsChargeFactory.EdificePackingCost;

		protected override bool ExpectedGetCalculatedIncludedInITOT(ZString incoterm, bool userEnteredIsDutiable)
		{
			return base.ExpectedGetCalculatedIncludedInITOT(incoterm, userEnteredIsDutiable) &&
				incoterm != Core.Constants.IncoTerms.UnpackedAtFactory &&
				incoterm != Core.Constants.IncoTerms.UnpackedFreeOnBoard &&
				incoterm != Core.Constants.IncoTerms.UnpackedCostAndFreight &&
				incoterm != Core.Constants.IncoTerms.UnpackedCostInsuranceAndFreight;
		}
	}
}
