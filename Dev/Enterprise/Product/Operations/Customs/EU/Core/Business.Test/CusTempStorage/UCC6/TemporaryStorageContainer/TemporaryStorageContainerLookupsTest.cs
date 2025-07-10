using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	public class TemporaryStorageContainerLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestEmptyFullIndicatorList()
		{
			var container = Factory.New<TemporaryStorageContainer>();
			var lookups = container.Lookups;

			AssertContainsExactElementsInAnyOrder("Empty Full indicator list Codes.", new[] { "A", "B" }, lookups.EmptyFullIndicatorList.GetAllCodesZString());
			AssertEquals("Code A description should be equal EmptyFullIndicatorList.Descriptions.Empty.", EmptyFullIndicatorList.Descriptions.Empty, lookups.EmptyFullIndicatorList.GetDescriptionFromCode("A"));
			AssertEquals("Code B description should be equal EmptyFullIndicatorList.Descriptions.NotEmpty.", EmptyFullIndicatorList.Descriptions.NotEmpty, lookups.EmptyFullIndicatorList.GetDescriptionFromCode("B"));
		}
	}
}
