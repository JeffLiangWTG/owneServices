using CargoWise.EntityFramework.Testing;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	internal sealed class OperationalActionLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestEvents()
		{
			OperationalAction action = Factory.New<OperationalAction>();
			AssertNotNull("Events", action.Lookups.Events);
			Assert("Events should be loaded.", action.Lookups.Events.Count > 0);
		}
	}
}
