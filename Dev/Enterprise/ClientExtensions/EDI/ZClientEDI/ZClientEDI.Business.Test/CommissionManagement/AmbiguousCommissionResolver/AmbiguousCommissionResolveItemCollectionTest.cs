using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.CommissionManagement.Business.Test
{
	[TestedType(typeof(AmbiguousCommissionResolveItemCollection))]
	class AmbiguousCommissionResolveItemCollectionTest : NonPersistentBusinessObjectCollectionTestCase<AmbiguousCommissionResolveItemCollection>
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

		protected override AmbiguousCommissionResolveItemCollection GetCollectionToTest()
		{
			return new AmbiguousCommissionResolveItemCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new AmbiguousCommissionResolveItem(Factory.New<AccAmbiguousCommission>());
		}

		#endregion
	}
}
