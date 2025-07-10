using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.MX.Business.Testing
{
	class GlbExternalPasswordLookups_MXLTest : BusinessObjectLookupsTestCase
	{
		public void TestCustomsEnclosureList()
		{
			var lookups = new GlbExternalPasswordLookups_MXL(Factory.New<GlbExternalPassword_MXL>());
			var listFilterObj = lookups.CustomsFacilities.FilterBusinessObjectDefaults;
			AssertEquals(2, listFilterObj.Count);
		}
	}
}
