namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	class EMCSAddInfoCusContainerLookupsTest : EUEMCSAddInfoLookupsTest
	{
		public void TestEMCSDestinationTypeList()
		{
			var list = lookups.EMCSDestinationTypeList;
			CombineAssertions(() =>
			{
				AssertEquals("CodesAsString", "1, 2, 3, 4, 5", list.CodesAsString);
				AssertSame("Cached", list, lookups.EMCSDestinationTypeList);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<EMCSJobDeclaration>();
			var container = declaration.CusContainers.AddNew();
			var containerAddInfo = new EMCSAddInfoCusContainer(container.CO_AddInfoInfo);
			lookups = new EMCSAddInfoCusContainerLookups(containerAddInfo);
		}

		EMCSAddInfoCusContainerLookups lookups;
	}
}
