namespace Enterprise.Client.EDI.Registry.Business.Testing
{
	using CargoWise.EntityFramework.Testing;

	public class MyAccountHostingSiteLandingPageUrlLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestProductTypes()
		{
			AssertNotNull(new MyAccountHostingSiteLandingPageUrlLookups(null).ProductTypes);
		}
	}
}
