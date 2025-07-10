using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusSCAPivotValidationPerformanceTest : TestCaseWithFactory
	{
		public void TestCheckCV_AssociatedContainer_PerformanceWorked()
		{
			try
			{
				TypeDecider.AddSubstitution(typeof(CusSCAContainer), typeof(CusSCAContainerTest.CusSCAContainerForTesting));

				var oceanBill = Factory.New<CusSCAOceanBill>();
				var houseBill = oceanBill.HouseBills.AddNew();
				var container = Factory.New<CusSCAContainerTest.CusSCAContainerForTesting>();
				oceanBill.Containers.Add(container);
				container.CN_ContainerMode = "FCL";
				var pivot1 = houseBill.Pivot.AddNew();
				pivot1.CV_CN = container.PK;
				var pivot2 = houseBill.Pivot.AddNew();
				pivot2.CV_CN = container.PK;

				AssertSame("To make sure the Pivots reference to the same Container.", pivot1.Container, pivot2.Container);

				//Cached properties are triggered when setting CV_CN, so Multiple...Calculated == 2.
				container.MultipleHouseBillsCalculated = 0;
				container.MultipleConsigneesCalculated = 0;

				//Last setting of CV_CN would trigger cache, and nothing in the factory changed thereafter, hence without validating the cache Multiple...Calculate remains 0.
				Factory.InvalidateCachedProperties();

				pivot1.Validation.ValidateCV_AssociatedContainer();
				pivot2.Validation.ValidateCV_AssociatedContainer();

				CombineAssertions(
					"IsAssociatedWithMultipleHouseBills and IsAssociatedWithMultipleConsignees are expected to be calculated only once.",
					() =>
					{
						AssertEquals(1, container.MultipleHouseBillsCalculated);
						AssertEquals(1, container.MultipleConsigneesCalculated);
					});
			}
			finally
			{
				TypeDecider.RemoveSubstitution(typeof(CusSCAContainer));
			}
		}
	}
}
