using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class ForeignInlandFreightTest : Common.Testing.ForeignInlandFreightTest
	{
		protected override string GetCountryContext() => JobDeclaration.AUEdifice;

		protected override bool ExpectedGetCalculatedIncludedInITOT(ZString incoterm, bool userEnteredIsDutiable)
		{
			return incoterm != Core.Constants.IncoTerms.ExWorks &&
					incoterm != Core.Constants.IncoTerms.PackedAtFactory &&
					incoterm != Core.Constants.IncoTerms.UnpackedAtFactory &&
					incoterm != EdificeIncoTermAndCustomsChargeFactory.ErrorIncoTermCode;
		}
	}
}
