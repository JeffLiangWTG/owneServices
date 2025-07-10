using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Manifest.Business.Testing
{
	[TestedType(typeof(TemporaryLandingInfoLookups))]
	sealed class TemporaryLandingInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCodeList()
		{
			var parent = Factory.New<TemporaryLandingInfo>();
			var lookups = new TemporaryLandingInfoLookups(parent);
			var codeList = lookups.CodeList;
			var list = Factory.GetCachedValue<TemporaryLandingReasonList>();
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("List", list, codeList);
				AssertEquals("Count", 4, codeList.Count);
			});
		}

		public void TestBondedTransportList()
		{
			var parent = Factory.New<TemporaryLandingInfo>();
			var lookups = new TemporaryLandingInfoLookups(parent);
			var codeList = lookups.BondedTransportList;
			var list = Factory.GetCachedValue<BondedTransportCodeList>();
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("List", list, codeList);
				AssertEquals("Count", 6, codeList.Count);
			});
		}
	}
}
