using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	[TestedType(typeof(PermitTypePartItemCollection))]
	sealed class PermitTypePartItemCollectionTest : NonPersistentBusinessObjectCollectionTestCase<PermitTypePartItemCollection>
	{
		public void TestJsonConverter()
		{
			var collection = new PermitTypePartItemCollection();
			collection.Add(new PermitTypePartItem("XXX", "XXX", "XXX") { Include = true });

			var result = JsonConverterHelper.Serialize(collection);
			var deserialisedCollection = JsonConverterHelper.Deserialize<PermitTypePartItemCollection>(result);

			AssertEquals(true, deserialisedCollection.HasIncludedItems);
		}

		#region Add / Remove

		public void TestAllowAdd()
		{
			var collection = new PermitTypePartItemCollection();
			AssertEquals(false, collection.AllowNew);
		}

		public void TestAllowRemove()
		{
			var collection = new PermitTypePartItemCollection();
			AssertEquals(false, collection.AllowRemove);
		}

		#endregion

		#region Included Items

		public void TestHasIncludedItems()
		{
			var collection = new PermitTypePartItemCollection();
			var item = new PermitTypePartItem("XXX", "XXX", "XXX");
			collection.Add(item);

			item.Include = false;
			AssertEquals(false, collection.HasIncludedItems);

			item.Include = true;
			AssertEquals(true, collection.HasIncludedItems);
		}

		#endregion

		#region Overrides

		protected override PermitTypePartItemCollection GetCollectionToTest()
		{
			return new PermitTypePartItemCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new PermitTypePartItem("Mode", "AIR", "Air");
		}

		#endregion
	}
}
