using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(OutturnRespPartyIDCodeDescriptionPairListRegistryItem))]
	sealed class OutturnRespPartyIDCodeDescriptionPairListRegistryItemTest : StronglyTypedRegistryItemTestCase<ReadOnlyCodeDescriptionPairList>
	{
		public void TestConstructor()
		{
			OutturnRespPartyIDCodeDescriptionPairListRegistryItem listRegistryItem = new OutturnRespPartyIDCodeDescriptionPairListRegistryItem("Name", (NoResString)"Caption", (NoResString)"Hint", 5,
				new CodeDescriptionPairListEditorInfo(), RegistryStorageFlags.Branch,
				RegistryOptions.Default, new ReadOnlyCodeDescriptionPairList(), false, null);

			AssertEquals("Name", "Name", listRegistryItem.Name);
			AssertEquals("Caption", "Caption", listRegistryItem.Caption);
			AssertEquals("Hint", "Hint", listRegistryItem.Hint);
			AssertEquals("Registry Data Type", typeof(OutturnRespPartyIDCodeDescriptionPairListRegistryDataType), listRegistryItem.DataType.GetType());
			AssertEquals("Code Max Length", 5, ((OutturnRespPartyIDCodeDescriptionPairListRegistryDataType)listRegistryItem.DataType).CodeMaxLength);
			AssertEquals("Editor Info", typeof(CodeDescriptionPairListEditorInfo), listRegistryItem.EditorInfo.GetType());
			AssertEquals("Registry Storage Flag", RegistryStorageFlags.Branch, listRegistryItem.Storage);
			AssertEquals("Registry Options", RegistryOptions.Default, listRegistryItem.Options);
			AssertEquals("Default Value", typeof(CodeDescriptionPairList), listRegistryItem.DefaultValue.GetType());
			AssertNull("Categories", listRegistryItem.Categories);
		}

		protected override StronglyTypedRegistryItem<ReadOnlyCodeDescriptionPairList, ReadOnlyCodeDescriptionPairList> GetNewRegistryItem()
		{
			return new OutturnRespPartyIDCodeDescriptionPairListRegistryItem("", null, null, 3, new CodeDescriptionPairListEditorInfo(), RegistryStorageFlags.System, RegistryOptions.Default, new CodeDescriptionPairList(), false, null);
		}
	}
}
