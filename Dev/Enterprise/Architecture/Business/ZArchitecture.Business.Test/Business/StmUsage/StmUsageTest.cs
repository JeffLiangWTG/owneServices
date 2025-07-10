using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(StmUsage))]
	sealed class StmUsageTest : EnterpriseBusinessObjectTestCase
	{
		protected override bool IsDeleteSupported()
		{
			return false;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var result = factory.New<StmUsage>();
			result.XW_EndTimeUtc = result.XW_StartTimeUtc.AddSeconds(1);
			return result;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var result = (StmUsage)Factory.NewWithValidTestData(GetExpectedBusinessObjectType(), TestBusinessObjectKind.MinimumRequiredToSave | TestBusinessObjectKind.PopulateDependentCollections);
			result.XW_EndTimeUtc = result.XW_StartTimeUtc.AddSeconds(1);
			return result;
		}
	}
}
