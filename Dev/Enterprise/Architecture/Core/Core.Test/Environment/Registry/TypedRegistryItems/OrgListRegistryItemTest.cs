using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using ResString = Enterprise.ZArchitecture.Core.SourceGenerated.ResString;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class OrgListRegistryItemTest : TestCase
	{
		public void TestConstructor()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.AddPair("ABC", ResString.GetMultilingualString("XYZ", "XYZ"));
			OrgListRegistryItem registryItem = new OrgListRegistryItem("Name", (NoResString)"Caption", (NoResString)"Hint", list);
			AssertConstructor(registryItem, "Name", "Caption", "Hint", RegistryStorageFlags.System);

			registryItem = new OrgListRegistryItem("Name", (NoResString)"Caption", (NoResString)"Hint", RegistryStorageFlags.Company, list);
			AssertConstructor(registryItem, "Name", "Caption", "Hint", RegistryStorageFlags.Company);
		}

		void AssertConstructor(OrgListRegistryItem registryItem, string name, string caption, string hint, RegistryStorageFlags storage)
		{
			AssertEquals(name, registryItem.Name);
			AssertEquals("Category", RawDataRegistry.Categories.Organizations_CodeLists, registryItem.Category);
			AssertEquals(caption, registryItem.Caption);
			AssertEquals(hint, registryItem.Hint);
			AssertEquals("DataType.CodeMaxLength", 3, ((CodeDescriptionPairListRegistryDataType)registryItem.DataType).CodeMaxLength);
			AssertEquals(storage, registryItem.Storage);
			AssertEquals("Options", RegistryOptions.Default, registryItem.Options);

			CodeDescriptionPairListEditorInfo editorInfo = (CodeDescriptionPairListEditorInfo)registryItem.EditorInfo;
			AssertEquals("EditorInfo.ShowCodeColumn", true, editorInfo.ShowCodeColumn);
			AssertEquals("EditorInfo.ShowDescriptionColumn", true, editorInfo.ShowDescriptionColumn);
			AssertEquals("EditorInfo.DescriptionFieldCasing", CodeDescriptionPairListEditorInfo.CharacterCasing.Normal, editorInfo.DescriptionFieldCasing);

			ReadOnlyCodeDescriptionPairList defaultValue = registryItem.DefaultValue;
			AssertEquals("DefaultValue.Count", 1, defaultValue.Count);
			AssertEquals("GetDescriptionFromCode(\"ABC\")", "XYZ", defaultValue.GetDescriptionFromCode("ABC"));
		}
	}
}
