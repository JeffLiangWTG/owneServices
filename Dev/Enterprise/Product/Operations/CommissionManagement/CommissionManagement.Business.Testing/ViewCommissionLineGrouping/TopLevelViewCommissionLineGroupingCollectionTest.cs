using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.CommissionManagement.Business.Testing
{
	[TestedType(typeof(TopLevelViewCommissionLineGroupingCollection))]
	public class TopLevelViewCommissionLineGroupingCollectionTest : NonPersistentBusinessObjectCollectionTestCase<TopLevelViewCommissionLineGroupingCollection>
	{
		protected override TopLevelViewCommissionLineGroupingCollection GetCollectionToTest()
		{
			var viewCommissionLines = new ViewCommissionLineCollection(Factory);
			var collection = new TopLevelViewCommissionLineGroupingCollection(viewCommissionLines, null);
			collection.Init();
			return collection;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var line1 = Factory.New<ViewCommissionLine>();
			var line2 = Factory.New<ViewCommissionLine>();
			var grouping = new ViewCommissionLineGrouping(Factory);
			grouping.Init(new[] { line1, line2 });
			return grouping;
		}
	}
}
