using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Registry.Business.Testing
{
	[TestedType(typeof(ShippingAgentRegistryItem))]
	class ShippingAgentRegistryItemTest : StronglyTypedRegistryItemTestCase<ShippingAgentObject>
	{
		public void TestConstructor()
		{
			AssertEquals("Test", Item.Name);
			AssertEquals("Category", Item.Category);
			AssertEquals("Caption", Item.Caption);
			AssertEquals("Hint", Item.Hint);
			AssertType<ShippingAgentRegistryDataType>(Item.DataType);
			AssertEquals(RegistryStorageFlags.Company, Item.Storage);
			AssertEquals(RegistryOptions.Default, Item.Options);
			AssertType<ShippingAgentObject>(Item.DefaultValue);
		}

		protected override StronglyTypedRegistryItem<ShippingAgentObject, ShippingAgentObject> GetNewRegistryItem() => new ShippingAgentRegistryItem("Test", "Category", "Caption", "Hint");
	}
}
