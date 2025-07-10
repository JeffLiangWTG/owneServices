using System.Linq;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(DropDownCodeDescriptionBoolRegistryItem))]
	sealed class DropDownCodeDescriptionBoolRegistryItemTest : StronglyTypedRegistryItemTestCase<DropDownCodeDescriptionBoolCollection, DropDownCodeDescriptionBoolCollection>
	{
		public void TestConstructor()
		{
			var collection = new DropDownCodeDescriptionBoolCollection(16, null, null);
			collection.AddSystemDefined("MANAGERSECURITY1", (NoResString)"DDM", true);
			collection.AddSystemDefined("MANAGERSECURITY1", (NoResString)"PLD", false);
			AssertEquals(2, collection.Count);
			AssertEquals(false, ((DropDownCodeDescriptionBool)collection.First())?.BoolInfo.ReadOnly);

			var item = new DropDownCodeDescriptionBoolRegistryItem("ManagerSecurityMapping", (NoResString)"Category1", (NoResString)"Manager Security Mapping", (NoResString)"Hint1",
				RegistryStorageFlags.System, (NoResString)"Enabled", collection, 16, null, null);
			AssertItem(item, "ManagerSecurityMapping", "Category1", "Manager Security Mapping", "Hint1", RegistryStorageFlags.System, "Enabled", collection);
		}

		void AssertItem(DropDownCodeDescriptionBoolRegistryItem registryItem, string expectedName, string expectedCategory, string expectedCaption, string expectedHint, RegistryStorageFlags expectedStorage, string expectedBoolColumnCaption, DropDownCodeDescriptionBoolCollection expectedDefaultValue)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Name", expectedName, registryItem.Name);
				AssertEquals("Category", expectedCategory, registryItem.Category);
				AssertEquals("Caption", expectedCaption, registryItem.Caption);
				AssertEquals("Hint", expectedHint, registryItem.Hint);
				AssertEquals("Storage", expectedStorage, registryItem.Storage);
				AssertEquals("EditorInfo.BoolColumnCaption", expectedBoolColumnCaption, ((DropDownCodeDescriptionBoolRegistryEditorInfo)registryItem.EditorInfo).BoolColumnCaption);

				if (expectedDefaultValue != null)
				{
					for (int i = 0; i < expectedDefaultValue.Count; i++)
					{
						AssertEquals(expectedDefaultValue[i].Code, registryItem.DefaultValue[i].Code);
						AssertEquals(expectedDefaultValue[i].Description, registryItem.DefaultValue[i].Description);
						AssertEquals(expectedDefaultValue[i].Bool, registryItem.DefaultValue[i].Bool);
					}
				}
			});
		}

		protected override StronglyTypedRegistryItem<DropDownCodeDescriptionBoolCollection, DropDownCodeDescriptionBoolCollection> GetNewRegistryItem()
		{
			return new DropDownCodeDescriptionBoolRegistryItem("", null, null, null, RegistryStorageFlags.System, (NoResString)"Enabled", new DropDownCodeDescriptionBoolCollection(), 16, null, null);
		}
	}
}
