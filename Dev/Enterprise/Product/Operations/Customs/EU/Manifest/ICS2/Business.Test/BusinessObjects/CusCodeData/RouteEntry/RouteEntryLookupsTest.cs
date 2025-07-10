using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	public class RouteEntryLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCountryCodeList()
		{
			var parent = Factory.New<RouteEntry>();
			var lookups = new RouteEntryLookups(parent);
			AssertType<RefCountryCollection>(lookups.CountryCodeList);
		}
	}
}
