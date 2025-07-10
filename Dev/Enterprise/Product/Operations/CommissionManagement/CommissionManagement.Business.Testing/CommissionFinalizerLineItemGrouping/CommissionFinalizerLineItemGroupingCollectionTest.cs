using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.CommissionManagement.Business.Testing
{
	[TestedType(typeof(CommissionFinalizerLineItemGroupingCollection))]
	internal class CommissionFinalizerLineItemGroupingCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CommissionFinalizerLineItemGroupingCollection>
	{
		#region Implementation

		protected override CommissionFinalizerLineItemGroupingCollection GetCollectionToTest()
		{
			return new CommissionFinalizerLineItemGroupingCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var grouping = new CommissionFinalizerLineItemGrouping(Factory);
			grouping.Init(new[] { new CommissionFinalizerLineItem(Factory.New<ViewCommissionLine>()) });
			return grouping;
		}

		#endregion
	}
}
