using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsArrivalHeaderContainerLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCargoIdTypeList()
		{
			var list = lookups.CargoIdTypeList;
			CombineAssertions(() =>
			{
				AssertEquals("Codes from list", "CNT, NCT", list.CodesAsString);
				AssertSame("Cached", list, lookups.CargoIdTypeList);
			});
		}

		public void TestUnloadedStatesList()
		{
			var list = lookups.UnloadedStates;
			CombineAssertions(() =>
			{
				AssertEquals("Codes from list", "DEC, DIF, MIS", list.CodesAsString);
				AssertSame("Cached", list, lookups.UnloadedStates);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var container = Factory.New<NctsArrivalHeaderContainer>();
			lookups = container.Lookups;
		}

		NctsArrivalHeaderContainerLookups lookups;
	}
}
