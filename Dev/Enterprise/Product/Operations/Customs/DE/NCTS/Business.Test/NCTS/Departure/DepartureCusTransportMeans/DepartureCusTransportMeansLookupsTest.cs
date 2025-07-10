using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class DepartureCusTransportMeansLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestOfficeCodeList()
		{
			AssertSame("Same list as for MovementHeader", nctsHeader.MovementHeader.Lookups.OfficeCodeList, lookups.OfficeCodeList);
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			lookups = nctsHeader.MovementHeader.Lookups;
		}
		NctsHeader nctsHeader;
		NctsDepartureMovementHeaderLookups lookups;
	}
}
