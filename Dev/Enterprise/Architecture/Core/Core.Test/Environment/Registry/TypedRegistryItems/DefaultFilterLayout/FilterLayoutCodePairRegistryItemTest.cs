using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(FilterLayoutCodePairRegistryItem))]
	sealed class FilterLayoutCodePairRegistryItemTest : StronglyTypedRegistryItemTestCase<string>
	{
		public void TestConstructor()
		{
			FilterLayoutCodePairRegistryItem item = new FilterLayoutCodePairRegistryItem("Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", true, true, new ComboBoxFilterLayoutRegistryEditorInfo("TrackingShipments"), RegistryStorageFlags.Company, RegistryOptions.Default, string.Empty, false);
			AssertEquals("Name", "Name", item.Name);
			AssertEquals("Category", "Category", item.Category);
			AssertEquals("Caption", "Caption", item.Caption);
			AssertEquals("Storage", RegistryStorageFlags.Company, item.Storage);
			AssertEquals("DefaultValue", string.Empty, item.DefaultValue);
		}

		protected override StronglyTypedRegistryItem<string, string> GetNewRegistryItem()
		{
			return new FilterLayoutCodePairRegistryItem("", null, null, null, false, false, new ComboBoxFilterLayoutRegistryEditorInfo("TrackingShipments"), RegistryStorageFlags.Company, RegistryOptions.Default, string.Empty, false);
		}
	}
}
