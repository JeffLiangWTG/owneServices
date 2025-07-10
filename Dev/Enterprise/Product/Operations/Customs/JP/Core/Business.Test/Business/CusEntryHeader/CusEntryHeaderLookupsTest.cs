using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.JP.Common;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(CusEntryHeaderLookups))]
	sealed class CusEntryHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCH_EntryStatusList()
		{
			AssertContainsExactElementsInAnyOrder(Factory.GetCachedValue<CustomsStatusList>(), cusEntryHeaderLookups.CH_EntryStatusList);
		}

		public void TestPhaseList()
		{
			AssertContainsExactElementsInAnyOrder(Factory.GetCachedValue<CustomsDeclarationPhases>(), cusEntryHeaderLookups.PhaseList);
		}

		protected override void SetUp()
		{
			cusEntryHeaderLookups = Factory.New<CusEntryHeader>().Lookups;
		}

		CusEntryHeaderLookups cusEntryHeaderLookups;
	}
}
