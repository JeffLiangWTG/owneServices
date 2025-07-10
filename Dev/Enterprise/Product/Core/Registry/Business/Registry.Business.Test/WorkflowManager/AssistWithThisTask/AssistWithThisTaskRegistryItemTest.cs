using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(AssistWithThisTaskRegistryItem))]
	sealed class AssistWithThisTaskRegistryItemTest : StronglyTypedRegistryItemTestCase<CategorisedAssistWithThisTaskSettingCollection>
	{
		public void TestProperties()
		{
			AssertNotNull(Item.DefaultValue);
			AssertEquals(typeof(AssistWithThisTaskRegistryDataType), Item.DataType.GetType());
		}

		protected override StronglyTypedRegistryItem<CategorisedAssistWithThisTaskSettingCollection, CategorisedAssistWithThisTaskSettingCollection> GetNewRegistryItem()
		{
			return new AssistWithThisTaskRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}
	}
}
