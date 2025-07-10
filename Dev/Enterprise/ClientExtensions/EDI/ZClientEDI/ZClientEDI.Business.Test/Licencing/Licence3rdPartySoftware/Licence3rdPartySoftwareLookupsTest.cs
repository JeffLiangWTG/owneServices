using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business.Test
{
	internal class Licence3rdPartySoftwareLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestListsTest()
		{
			Licence3rdPartySoftware header = Factory.New<Licence3rdPartySoftware>();
			AssertNotNull("OS Type List should not be null", header.Lookups.OSType);
			Assert("OS software list should have more then 0 items", header.Lookups.OSType.Count > 0);

			AssertNotNull("Licence type list should not be null", header.Lookups.LicenceType);
			Assert("Licence type list should have more then 0 items", header.Lookups.LicenceType.Count > 0);

			AssertNotNull("Clients list should not be null", header.Lookups.Clients);
			AssertEquals("Clients.GetType()", typeof(OrganisationsFindBoxCollection), header.Lookups.Clients.GetType());
		}
	}
}