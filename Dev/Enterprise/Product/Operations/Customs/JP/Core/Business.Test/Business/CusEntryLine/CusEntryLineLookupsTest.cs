using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(CusEntryLineLookups))]
	sealed class CusEntryLineLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestPriceCheckTypeList()
		{
			AssertContainsExactElementsInAnyOrder(Factory.GetCachedValue<PriceCheckTypeList>(), cusEntryLineLookups.PriceCheckTypeList);
		}

		protected override void SetUp()
		{
			cusEntryLineLookups = Factory.New<CusEntryLine>().Lookups;
		}

		CusEntryLineLookups cusEntryLineLookups;
	}
}
