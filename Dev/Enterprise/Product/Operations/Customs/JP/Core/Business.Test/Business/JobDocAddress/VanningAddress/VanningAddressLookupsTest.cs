using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(VanningAddressLookups))]
	sealed class VanningAddressLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestGovRegNumTypes()
		{
			CombineAssertions(() =>
			{
				var lookups = Factory.New<VanningAddress>().Lookups;

				AssertContainsExactElementsInAnyOrder(new[] { "CCP", "LPC", "CIE" }, lookups.GovRegNumTypes.GetAllCodes());
				AssertSame("Should have been cached", lookups.GovRegNumTypes, lookups.GovRegNumTypes);
			});
		}

		public void TestState_List()
		{
			var lookups = Factory.New<VanningAddress>().Lookups;
			AssertEquals("Code list count", new OrgCodeLists().State_List(lookups.Parent.Country).Count, lookups.State_List.Count);
		}
	}
}
