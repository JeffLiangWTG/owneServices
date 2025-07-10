namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	public class CusContainerLookupsTest : Customs.Business.Testing.CusContainerLookupsTest
	{
		public void TestContainer()
		{
			CusContainer parent = Factory.New<CusContainer>();
			AssertEquals(parent.Lookups.Container, parent);
		}

		#region Implementation

		protected override Customs.Business.CusContainerLookups GetCusContainerLookups()
		{
			return new CusContainerLookups(Factory.New<CusContainer>());
		}

		#endregion
	}
}
