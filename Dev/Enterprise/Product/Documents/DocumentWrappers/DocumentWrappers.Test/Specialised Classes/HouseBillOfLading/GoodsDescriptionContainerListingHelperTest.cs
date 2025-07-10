using CargoWise.EntityFramework.Testing;

namespace Enterprise.DocumentWrappers.Freight.Testing
{
	sealed class GoodsDescriptionContainerListingHelperTest : TestCaseWithFactory
	{
		public void TestContainerTypeAndCountList()
		{
			Assert("empty list", Helper.ContainerCountList.Count == 0);
			Assert("empty list", Helper.ContainerTypeList.Count == 0);

			Helper.AddContainer("", "40GP", 1);
			Assert("number of container counts", Helper.ContainerCountList.Count == 1);
			Assert("number of container type counts", Helper.ContainerTypeList.Count == 1);

			Helper.AddContainer("ABC123", "40GP", 1);
			Assert("number of container counts", Helper.ContainerCountList.Count == 1);
			Assert("number of container type counts", Helper.ContainerTypeList.Count == 1);
			Assert("should be 2", Helper.ContainerCountList[0].StartsWith("2"));
			Assert("should contain the container type", Helper.ContainerTypeList[0].StartsWith("x 40GP CONTAINER"));

			// adding the same container shouldnt affect our total
			Helper.AddContainer("ABC123", "40GP", 1);
			Assert("number of container counts", Helper.ContainerCountList.Count == 1);
			Assert("number of container type counts", Helper.ContainerTypeList.Count == 1);
			Assert("should be 2", Helper.ContainerCountList[0].StartsWith("2"));
			Assert("should contain the container type", Helper.ContainerTypeList[0].StartsWith("x 40GP CONTAINER"));

			Helper.AddContainer("ABC987", "40GP", 1); // add another container, and it should!
			Assert("number of container counts", Helper.ContainerCountList.Count == 1);
			Assert("number of container type counts", Helper.ContainerTypeList.Count == 1);
			Assert("should be 3", Helper.ContainerCountList[0].StartsWith("3"));

			Helper.AddContainer("", "20FR", 4);
			Assert("number of container counts", Helper.ContainerCountList.Count == 2);
			Assert("number of container type counts", Helper.ContainerTypeList.Count == 2);
			Assert("should be 3", Helper.ContainerCountList[0].StartsWith("3"));
			Assert("should contain the container type", Helper.ContainerTypeList[0].StartsWith("x 40GP CONTAINER"));
			Assert("should be 4", Helper.ContainerCountList[1].StartsWith("4"));
			Assert("should contain the container type", Helper.ContainerTypeList[1].StartsWith("x 20FR CONTAINER"));
		}

		public void TestContainerListingHelper()
		{
			AssertEquals("", Helper.ContainersListForDocument);

			Helper.AddContainer("", "40GP", 1);
			AssertEquals("1 x 40GP CONTAINER\n", Helper.ContainersListForDocument);

			Helper.AddContainer("ABC123", "40GP", 1);
			AssertEquals("2 x 40GP CONTAINER\n", Helper.ContainersListForDocument);

			// adding the same container shouldnt affect our total
			Helper.AddContainer("ABC123", "40GP", 1);
			AssertEquals("2 x 40GP CONTAINER\n", Helper.ContainersListForDocument);

			Helper.AddContainer("ABC987", "40GP", 1); // add another container, and it should!
			AssertEquals("3 x 40GP CONTAINER\n", Helper.ContainersListForDocument);

			Helper.AddContainer("", "40GP", 4);
			AssertEquals("7 x 40GP CONTAINER\n", Helper.ContainersListForDocument);

			Helper.AddContainer("", "20FR", 4);
			AssertEquals("7 x 40GP CONTAINER\n4 x 20FR CONTAINER\n", Helper.ContainersListForDocument);

			Helper.AddContainer("", "20FR", 4);
			AssertEquals("7 x 40GP CONTAINER\n8 x 20FR CONTAINER\n", Helper.ContainersListForDocument);
		}

		GoodsDescriptionContainerListingHelper Helper
		{
			get { return helper ?? (helper = new GoodsDescriptionContainerListingHelper()); }
		}
		GoodsDescriptionContainerListingHelper helper;
	}
}
