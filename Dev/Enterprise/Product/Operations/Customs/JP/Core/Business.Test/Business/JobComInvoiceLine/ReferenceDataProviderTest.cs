using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(ReferenceDataProvider))]
	sealed class ReferenceDataProviderTest : TestCaseWithFactory
	{
		public void TestGetTradeControlOrderAppendixList_Cachenable()
		{
			TestDataCoreHelper.PrepareTradeControlOrderAppendixList(Factory);

			var isExport = true;
			var list = (CodeDescriptionPairList)ReferenceDataProvider.GetTradeControlOrderAppendixList(Factory, isExport);
			var expectedList = Factory.GetCachedValue($"JP.TradeControlOrderAppendixList-{ZDateTime.Today.ToISO8601ShortDateString()}-{isExport}", () => new CodeDescriptionPairList());
			AssertContainsExactElementsInAnyOrder(expectedList, list);

			isExport = false;
			list = (CodeDescriptionPairList)ReferenceDataProvider.GetTradeControlOrderAppendixList(Factory, isExport);
			expectedList = Factory.GetCachedValue($"JP.TradeControlOrderAppendixList-{ZDateTime.Today.ToISO8601ShortDateString()}-{isExport}", () => new CodeDescriptionPairList());
			AssertContainsExactElementsInAnyOrder(expectedList, list);
		}
	}
}
