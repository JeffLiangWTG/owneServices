using CargoWise.EntityFramework.Testing;
using MailManager;

namespace Enterprise.MailManager.Business.Testing
{
	sealed class MailDBItemsLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestStatusCodeList()
		{
			var lookups = new MailDBItemsLookups(Factory.New<MailItem>());
			AssertNotNull("lookups.StatusCodes", lookups.StatusCodes);
			AssertEquals("lookups.StatusCodes returned Type", typeof(StatusCodeList), lookups.StatusCodes.GetType());
		}

		public void TestDirectionList()
		{
			var lookups = new MailDBItemsLookups(Factory.New<MailItem>());
			AssertNotNull("lookups.Direction", lookups.Directions);
			AssertEquals("lookups.Directions returned Type", typeof(DirectionList), lookups.Directions.GetType());
		}
	}
}
