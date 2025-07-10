using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.CommissionManagement.Business.Testing
{
	[TestedType(typeof(ViewCommissionLineGroupingCollection))]
	public class ViewCommissionLineGroupingCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ViewCommissionLineGroupingCollection>
	{
		protected override ViewCommissionLineGroupingCollection GetCollectionToTest()
		{
			return new ViewCommissionLineGroupingCollection(Factory);
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
