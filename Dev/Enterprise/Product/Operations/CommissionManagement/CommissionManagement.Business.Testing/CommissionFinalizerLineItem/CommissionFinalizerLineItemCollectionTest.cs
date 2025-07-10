using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.CommissionManagement.Business.Testing
{
	[TestedType(typeof(CommissionFinalizerLineItemCollection))]
	public class CommissionFinalizerLineItemCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CommissionFinalizerLineItemCollection>
	{
		#region Allowed Actions

		public void TestAllowNew()
		{
			var collection = GetCollectionToTest();
			AssertEquals(false, collection.AllowNew);
		}

		public void TestAllowRemove()
		{
			var collection = GetCollectionToTest();
			AssertEquals(false, collection.AllowRemove);
		}

		#endregion

		#region Overrides

		protected override CommissionFinalizerLineItemCollection GetCollectionToTest()
		{
			return new CommissionFinalizerLineItemCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var commissionLine = Factory.New<ViewCommissionLine>();
			return new CommissionFinalizerLineItem(commissionLine);
		}

		#endregion
	}
}
