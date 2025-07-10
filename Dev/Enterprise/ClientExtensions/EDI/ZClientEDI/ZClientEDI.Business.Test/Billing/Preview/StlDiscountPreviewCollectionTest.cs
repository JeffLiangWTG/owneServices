using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(StlDiscountPreviewCollection))]
	class StlDiscountPreviewCollectionTest : NonPersistentBusinessObjectCollectionTestCase<StlDiscountPreviewCollection>
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

		protected override StlDiscountPreviewCollection GetCollectionToTest()
		{
			return new StlDiscountPreviewCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new StlDiscountPreview();
		}

		#endregion
	}
}
