using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	class NctsHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestNctsMessageStatusList()
		{
			var list = lookups.NctsMessageStatusList;
			Assert("Contains NCTS5ArrivalCustomsStatusList", list.ContainsCode(NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease));
			Assert("Contains NCTS5DepartureCustomsStatusList", list.ContainsCode(NCTS5DepartureCustomsStatusList.Codes.AmendmentRequested));
			AssertSame("cached", list, lookups.NctsMessageStatusList);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = NctsHeaderTest.GetNewBusinessObject(Factory);
			lookups = header.Lookups;
		}
		NctsHeader header;
		NctsHeaderLookups lookups;
	}
}
