using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(CustomsSummaryLineFetchStrategy))]
sealed class CustomsSummaryLineFetchStrategyTest : BusinessObjectFetchStrategyTestCase
{
	public void TestFetchForView()
	{
		var summaryLine1 = CustomsSummaryTestHelper.CreateSummaryLine(Factory);
		var summaryLine2 = CustomsSummaryTestHelper.CreateSummaryLine(Factory);
		TestFetchForView(summaryLine1, summaryLine2);
	}

	protected override IBusinessObjectCollection CreateCollectionToTest(BusinessObjectFactory factory)
	{
		return new CustomsSummaryLineCollection(factory);
	}
}
