using System.Linq;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CodeDescriptionBoolWithSingleTrueRegistryItem))]
	sealed class CodeDescriptionBoolWithSingleTrueRegistryItemTest : StronglyTypedRegistryItemTestCase<CodeDescriptionBoolCollection, CodeDescriptionBoolCollection>
	{
		public void TestConstructor()
		{
			var collection = new CodeDescriptionBoolWithSingleTrueCollection();
			collection.AddSystemDefined("AGA", (NoResString)"Agar", true);
			collection.AddSystemDefined("BOB", (NoResString)"Bobby", false);
			AssertEquals(2, collection.Count);
			AssertEquals(false, ((CodeDescriptionBoolWithSingleTrue)collection.First())?.BoolInfo.ReadOnly);

			var item = new CodeDescriptionBoolWithSingleTrueRegistryItem("Name1", (NoResString)"Category1", (NoResString)"Caption1", (NoResString)"Hint1", RegistryStorageFlags.BranchDepartment, RegistryOptions.IsOnlyForSupport, new CodeDescriptionBoolRegistryEditorInfo((NoResString)"Bool1"), collection);
			AssertItem(item, "Name1", "Category1", "Caption1", "Hint1", RegistryStorageFlags.BranchDepartment, RegistryOptions.IsOnlyForSupport, "Bool1", collection);
		}

		void AssertItem(CodeDescriptionBoolWithSingleTrueRegistryItem registryItem, string expectedName, string expectedCategory, string expectedCaption, string expectedHint, RegistryStorageFlags expectedStorage, RegistryOptions expectedOptions, string expectedBoolColumnCaption, CodeDescriptionBoolWithSingleTrueCollection expectedDefaultValue)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Name", expectedName, registryItem.Name);
				AssertEquals("Category", expectedCategory, registryItem.Category);
				AssertEquals("Caption", expectedCaption, registryItem.Caption);
				AssertEquals("Hint", expectedHint, registryItem.Hint);
				AssertEquals("Storage", expectedStorage, registryItem.Storage);
				AssertEquals("Options", expectedOptions, registryItem.Options);
				AssertEquals("EditorInfo.BoolColumnCaption", expectedBoolColumnCaption, ((CodeDescriptionBoolRegistryEditorInfo)registryItem.EditorInfo).BoolColumnCaption);

				if (expectedDefaultValue != null)
				{
					for (int i = 0; i < expectedDefaultValue.Count; i++)
					{
						AssertEquals(expectedDefaultValue[i].Code, registryItem.Value[i].Code);
						AssertEquals(expectedDefaultValue[i].Description, registryItem.Value[i].Description);
						AssertEquals(expectedDefaultValue[i].Bool, registryItem.Value[i].Bool);
					}
				}
			});
		}

		protected override StronglyTypedRegistryItem<CodeDescriptionBoolCollection, CodeDescriptionBoolCollection> GetNewRegistryItem()
		{
			return new CodeDescriptionBoolWithSingleTrueRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport, new CodeDescriptionBoolRegistryEditorInfo((NoResString)""), new CodeDescriptionBoolWithSingleTrueCollection());
		}
	}
}
