using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.AWB.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Testing
{
	class CcsukCusAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestSpecialHandlingCodes()
		{
			var mawb = Factory.New<CusMAWB>();
			var communityHandlingCode = mawb.CommunityHandlingCodes.AddNew().Data;
			var lookups = new CcsukCusAddInfoLookups(communityHandlingCode);
			AssertSame("Cached", lookups.SpecialHandlingCodes, Factory.GetCachedValue<AWBSpecialHandlingCodeDescriptionPairList>());
		}
	}
}
