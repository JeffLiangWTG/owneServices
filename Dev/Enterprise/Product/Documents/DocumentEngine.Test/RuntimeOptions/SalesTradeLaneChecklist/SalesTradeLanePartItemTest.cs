using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	[TestedType(typeof(SalesTradeLanePartItem))]
	sealed class SalesTradeLanePartItemTest : NonPersistentBusinessObjectTestCase
	{
		#region SubItems

		public void TestSetIncludeToTrueIfAnyIncludeAnySubItems()
		{
			var item = new SalesTradeLanePartItem("XXX", "XXX", "XXX");
			var subItem = new SalesTradeLanePartItem("YYY", "YYY", "YYY");
			item.SubItemsCollection.Add(subItem);

			AssertEquals("Precondition", false, item.Include);

			subItem.Include = true;

			AssertEquals(true, item.Include);
		}

		#endregion

		#region JsonConverter

		public void TestJsonConverter()
		{
			var item = new SalesTradeLanePartItem("AAA", "BBB", "CCC");
			item.Include = true;
			var subItem = new SalesTradeLanePartItem("YYY", "YYY", "YYY");
			subItem.Include = true;
			item.SubItemsCollection.Add(subItem);

			var result = JsonConverterHelper.Serialize(item);
			var deserialisedField = JsonConverterHelper.Deserialize<SalesTradeLanePartItem>(result);

			AssertEquals("BBB", deserialisedField.Code);
			AssertEquals("CCC", deserialisedField.Description);
			AssertEquals(true, deserialisedField.Include);
			AssertEquals(1, deserialisedField.SubItemsCollection.Count);
			AssertEquals("YYY", deserialisedField.SubItemsCollection.Cast<SalesTradeLanePartItem>().Single().Code);
		}

		#endregion

		#region Overrides

		protected override BusinessObject GetNewBusinessObject()
		{
			return new SalesTradeLanePartItem("Mode", "AIR", "Air");
		}

		#endregion
	}
}
