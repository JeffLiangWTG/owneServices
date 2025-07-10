using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsContainerLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTypeOfServiceList()
		{
			CombineAssertions(() =>
			{
				var list = lookups.TypeOfServiceList;
				AssertSame("Cached", list, lookups.TypeOfServiceList);
				AssertEquals("CodesAsString", "DE, IN, SE, TS", list.CodesAsString);
			});
		}

		public void TestCargoIdTypeList()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Codes from list", "CNT, NCT", lookups.CargoIdTypeList.CodesAsString);
				AssertSame("Cached", lookups.CargoIdTypeList, lookups.CargoIdTypeList);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			lookups = new NctsContainerLookups(Factory.New<NctsContainer>());
		}
		NctsContainerLookups lookups;
	}
}
