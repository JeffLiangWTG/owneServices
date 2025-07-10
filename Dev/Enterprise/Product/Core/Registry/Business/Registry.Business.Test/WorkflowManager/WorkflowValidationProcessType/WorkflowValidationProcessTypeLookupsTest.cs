using CargoWise.EntityFramework.Testing;

namespace Enterprise.Registry.Business.Testing
{
	sealed class WorkflowValidationProcessTypeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestProcessTypeList()
		{
			var lookups = new WorkflowValidationProcessTypeLookups(null);

			AssertNotNull(lookups.ProcessTypeList);
			AssertEquals(true, lookups.ProcessTypeList.Count > 0);
		}
	}
}
