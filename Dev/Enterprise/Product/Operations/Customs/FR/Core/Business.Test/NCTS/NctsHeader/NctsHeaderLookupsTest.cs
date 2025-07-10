using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.EU;

namespace Enterprise.Customs.FR.Business.NCTS.Testing
{
	class NctsHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestDetailedStatusCodeList()
		{
			AssertSame(Factory.GetCachedValue<NctsDetailedStatusList>(), lookups.DetailedStatusCodeList);
		}

		public void TestNctsTransitStatusList()
		{
			AssertSame(Factory.GetCachedValue<NctsTransitStatusList>(), lookups.NctsTransitStatusList);
		}

		public void TestNctsMessageStatusList()
		{
			nctsHeader.BH_ApplicationCode = "NCT";
			AssertSame(Factory.GetCachedValue<FrNctsMessageStatusList>(), lookups.NctsMessageStatusList);
			nctsHeader.BH_ApplicationCode = "NC5";
			AssertSame(Factory.GetCachedValue<LogicalStatusList>(), lookups.NctsMessageStatusList);
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			lookups = new NctsHeaderLookups(nctsHeader);
		}
		NctsHeader nctsHeader;
		NctsHeaderLookups lookups;
	}
}
