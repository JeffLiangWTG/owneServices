using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ParentAndChildCodeDescriptionBoolRegistryItem))]
	sealed class ParentAndChildCodeDescriptionBoolRegistryItemTest : StronglyTypedRegistryItemTestCase<IParentCodeDescriptionBoolList, ParentCodeDescriptionBoolCollection>
	{
		public void TestEditorInfoAndDefaultValue()
		{
			ParentAndChildCodeDescriptionBoolRegistryEditorInfo editorInfo = new ParentAndChildCodeDescriptionBoolRegistryEditorInfo((NoResString)"", (NoResString)"", null, null);
			ParentCodeDescriptionBoolCollection collection = new ParentCodeDescriptionBoolCollection();
			collection.AddNew().Code = "DON";
			ParentAndChildCodeDescriptionBoolRegistryItem item = new ParentAndChildCodeDescriptionBoolRegistryItem("", null, null, null, editorInfo, RegistryStorageFlags.System, collection);
			AssertEquals("EditorInfo", editorInfo, item.EditorInfo);
			AssertEquals("DefaultValue.ContainsCode(\"DON\")", true, ((ParentCodeDescriptionBoolCollection)item.DefaultValue).ContainsCode("DON"));
		}

		protected override StronglyTypedRegistryItem<IParentCodeDescriptionBoolList, ParentCodeDescriptionBoolCollection> GetNewRegistryItem()
		{
			CodeDescriptionBoolRegistryEditorInfo editorInfo = new CodeDescriptionBoolRegistryEditorInfo(null);
			ParentAndChildCodeDescriptionBoolRegistryEditorInfo parentEditorInfo = new ParentAndChildCodeDescriptionBoolRegistryEditorInfo((NoResString)"", (NoResString)"", editorInfo, editorInfo);
			return new ParentAndChildCodeDescriptionBoolRegistryItem("", null, null, null, parentEditorInfo, RegistryStorageFlags.System);
		}
	}
}
