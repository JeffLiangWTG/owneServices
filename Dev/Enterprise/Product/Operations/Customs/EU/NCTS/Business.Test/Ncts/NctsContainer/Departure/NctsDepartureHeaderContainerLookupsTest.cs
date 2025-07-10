using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsDepartureHeaderContainerLookupsTest : BusinessObjectLookupsTestCase
	{
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
			var container = Factory.New<NctsDepartureHeaderContainer>();
			lookups = container.Lookups;
		}

		NctsDepartureHeaderContainerLookups lookups;
	}
}
