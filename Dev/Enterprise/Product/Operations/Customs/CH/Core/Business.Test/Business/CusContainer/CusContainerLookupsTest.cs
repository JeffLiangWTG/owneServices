namespace Enterprise.Customs.CH.Business.Testing;

class CusContainerLookupsTest : Customs.Business.Testing.CusContainerLookupsTest
{
	protected override Customs.Business.CusContainerLookups GetCusContainerLookups() => new CusContainerLookups(Factory.New<CusContainer>());
}
