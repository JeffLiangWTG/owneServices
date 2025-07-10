using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class ETHeaderPrincipalTraderWrapperTest : SADEmptyTraderWrapperTest
{
	protected override void AdditionalPropertiesCheck()
	{
		CombineAssertions(nameof(ETHeaderPrincipalTraderWrapper), () =>
		 {
			 var wrapper = new ETHeaderPrincipalTraderWrapper();
			 AssertEquals(ZString.Empty, wrapper.TraderGuaranteeTaxIdentificationNumber);
			 AssertEquals(ZString.Empty, wrapper.TIRHolderIdentification);
			 AssertEquals(ZString.Empty, wrapper.RepresentativeGuaranteeTaxIdentificationNumber);
			 AssertEquals(ZString.Empty, wrapper.RepresentativeName);
			 AssertEquals(ZString.Empty, wrapper.RepresentativeType);
		 });
	}
}
