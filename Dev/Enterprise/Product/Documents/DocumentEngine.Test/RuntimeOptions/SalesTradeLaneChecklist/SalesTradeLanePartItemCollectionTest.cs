using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	[TestedType(typeof(SalesTradeLanePartItemCollection))]
	sealed class SalesTradeLanePartItemCollectionTest : NonPersistentBusinessObjectCollectionTestCase<SalesTradeLanePartItemCollection>
	{
		public void TestJsonConverter()
		{
			var collection = new SalesTradeLanePartItemCollection();
			collection.Add(new SalesTradeLanePartItem("XXX", "XXX", "XXX") { Include = true });

			var result = JsonConverterHelper.Serialize(collection);
			var deserialisedCollection = JsonConverterHelper.Deserialize<SalesTradeLanePartItemCollection>(result);

			AssertEquals(true, deserialisedCollection.HasIncludedItems);
		}

		#region Add / Remove

		public void TestAllowAdd()
		{
			var collection = new SalesTradeLanePartItemCollection();
			AssertEquals(false, collection.AllowNew);
		}

		public void TestAllowRemove()
		{
			var collection = new SalesTradeLanePartItemCollection();
			AssertEquals(false, collection.AllowRemove);
		}

		#endregion

		#region Included Items

		public void TestHasIncludedItems()
		{
			var collection = new SalesTradeLanePartItemCollection();
			var item = new SalesTradeLanePartItem("XXX", "XXX", "XXX");
			collection.Add(item);

			item.Include = false;
			AssertEquals(false, collection.HasIncludedItems);

			item.Include = true;
			AssertEquals(true, collection.HasIncludedItems);
		}

		#endregion

		#region Overrides

		protected override SalesTradeLanePartItemCollection GetCollectionToTest()
		{
			return new SalesTradeLanePartItemCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new SalesTradeLanePartItem("Mode", "AIR", "Air");
		}

		#endregion
	}
}
