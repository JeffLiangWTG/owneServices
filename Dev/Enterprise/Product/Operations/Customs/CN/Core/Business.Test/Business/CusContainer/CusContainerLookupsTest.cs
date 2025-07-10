namespace Enterprise.Customs.CN.Business.Testing
{
	class CusContainerLookupsTest : Customs.Business.Testing.CusContainerLookupsTest
	{
		public void TestContainer()
		{
			var parent = Factory.New<CusContainer>();
			AssertEquals(parent.Lookups.Container, parent);
		}

		protected override Customs.Business.CusContainerLookups GetCusContainerLookups()
		{
			return new CusContainerLookups(Factory.New<CusContainer>());
		}
	}
}
