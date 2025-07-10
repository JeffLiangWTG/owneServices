using System.Linq;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(OverrideImmuneCodeDescriptionBoolRegistryItem))]
	sealed class OverrideImmuneCodeDescriptionBoolRegistryItemTest : StronglyTypedRegistryItemTestCase<CodeDescriptionBoolCollection, CodeDescriptionBoolCollection>
	{
		public void TestConstructor()
		{
			ReadOnlyCodeDescriptionPairList emptyList = new ReadOnlyCodeDescriptionPairList();
			var collection = new OverrideImmuneCodeDescriptionBoolCollection();
			collection.AddSystemDefined("AGA", (NoResString)"Agar", true);
			AssertEquals(1, collection.Count);
			AssertEquals(true, ((OverrideImmuneCodeDescriptionBool)collection.First())?.BoolInfo.ReadOnly);

			var item = new OverrideImmuneCodeDescriptionBoolRegistryItem("Name1", (NoResString)"Category1", (NoResString)"Caption1", (NoResString)"Hint1", RegistryStorageFlags.BranchDepartment, new CodeDescriptionBoolRegistryEditorInfo((NoResString)"Bool1"), collection);

			AssertItem(item, "Name1", "Category1", "Caption1", "Hint1", RegistryStorageFlags.BranchDepartment, "Bool1", false, collection);
			var systemDefinedBool = ((OverrideImmuneCodeDescriptionBoolCollection)item.DefaultValue)[0];
			AssertEquals("CodeInfo.ReadOnly", true, systemDefinedBool.CodeInfo.ReadOnly);
			AssertEquals("DescriptionInfo.ReadOnly", true, systemDefinedBool.DescriptionInfo.ReadOnly);
			AssertEquals("BoolInfo.ReadOnly", true, systemDefinedBool.BoolInfo.ReadOnly);
		}

		void AssertItem(OverrideImmuneCodeDescriptionBoolRegistryItem item, string expectedName, string expectedCategory, string expectedCaption, string expectedHint, RegistryStorageFlags expectedStorage, string expectedBoolColumnCaption, bool expectedDefaultBoolColumnValue, OverrideImmuneCodeDescriptionBoolCollection expectedCollection)
		{
			AssertEquals("Name", expectedName, item.Name);
			AssertEquals("Category", expectedCategory, item.Category);
			AssertEquals("Caption", expectedCaption, item.Caption);
			AssertEquals("Hint", expectedHint, item.Hint);
			AssertEquals("Storage", expectedStorage, item.Storage);
			AssertEquals("EditorInfo.BoolColumnCaption", expectedBoolColumnCaption, ((CodeDescriptionBoolRegistryEditorInfo)item.EditorInfo).BoolColumnCaption);

			var defaultValue = (OverrideImmuneCodeDescriptionBoolCollection)item.DefaultValue;

			AssertEquals("DefaultValue.AddNew().Bool", expectedDefaultBoolColumnValue, defaultValue.AddNew().Bool);
		}

		protected override StronglyTypedRegistryItem<CodeDescriptionBoolCollection, CodeDescriptionBoolCollection> GetNewRegistryItem()
		{
			return new OverrideImmuneCodeDescriptionBoolRegistryItem("", null, null, null, RegistryStorageFlags.System, new CodeDescriptionBoolRegistryEditorInfo((NoResString)""), new OverrideImmuneCodeDescriptionBoolCollection());
		}
	}
}
