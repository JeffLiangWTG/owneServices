using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class RegistryItemImplWithDynamicDefaultValueTest : TestCase
	{
		public void TestEverything()
		{
			StringRegistryDataType dataType = new StringRegistryDataType();

			RegistryItemImplWithDynamicDefaultValue item = new RegistryItemImplWithDynamicDefaultValue("a", (NoResString)"b", (NoResString)"c", (NoResString)"d", dataType, RegistryStorageFlags.System, delegate
			{ return dataType.MaxLength.ToString(); });
			AssertDetails(item, "a", new string[] { "b" }, "c", "d", dataType, null, RegistryStorageFlags.System, RegistryOptions.Default, true);

			item = new RegistryItemImplWithDynamicDefaultValue("a", (NoResString)"b", (NoResString)"c", (NoResString)"d", dataType, RegistryStorageFlags.Company, RegistryOptions.IsReadOnly, delegate
			{ return dataType.MaxLength.ToString(); });
			AssertDetails(item, "a", new string[] { "b" }, "c", "d", dataType, null, RegistryStorageFlags.Company, RegistryOptions.IsReadOnly, true);

			item = new RegistryItemImplWithDynamicDefaultValue("a", new MultilingualString[] { (NoResString)"b", (NoResString)"bb" }, (NoResString)"c", (NoResString)"d", dataType, RegistryStorageFlags.System, delegate
			{ return dataType.MaxLength.ToString(); });
			AssertDetails(item, "a", new string[] { "b" }, "c", "d", dataType, null, RegistryStorageFlags.System, RegistryOptions.Default, true);

			CodeDescriptionPairListEditorInfo editorInfo = new CodeDescriptionPairListEditorInfo();
			item = new RegistryItemImplWithDynamicDefaultValue("a", new MultilingualString[] { (NoResString)"b", (NoResString)"bb" }, (NoResString)"c", (NoResString)"d", dataType, editorInfo, RegistryStorageFlags.System, delegate
			{ return dataType.MaxLength.ToString(); });
			AssertDetails(item, "a", new string[] { "b" }, "c", "d", dataType, editorInfo, RegistryStorageFlags.System, RegistryOptions.Default, true);

			item = new RegistryItemImplWithDynamicDefaultValue("a", new MultilingualString[] { (NoResString)"b", (NoResString)"bb" }, (NoResString)"c", (NoResString)"d", dataType, editorInfo, RegistryStorageFlags.System, RegistryOptions.IsReadOnly, delegate
			{ return dataType.MaxLength.ToString(); });
			AssertDetails(item, "a", new string[] { "b", "bb" }, "c", "d", dataType, editorInfo, RegistryStorageFlags.System, RegistryOptions.IsReadOnly, true);
		}

		void AssertDetails(RegistryItemImplWithDynamicDefaultValue item, string name, string[] categories, string caption, string hint, StringRegistryDataType dataType, IRegistryEditorInfo editorInfo, RegistryStorageFlags storageFlag, RegistryOptions options, bool useDefaultDefaultValue)
		{
			AssertEquals("Name", name, item.Name);
			for (int i = 0; i < categories.Length; i++)
			{
				AssertEquals("Category", categories[i], item.Categories[i]);
			}
			AssertEquals("Category", categories[0], item.Category);
			AssertEquals("Caption", caption, item.Caption);
			AssertEquals("Hint", hint, item.Hint);
			AssertEquals("DataType", dataType, item.DataType);
			AssertEquals("Storage", storageFlag, item.Storage);
			AssertEquals("Options", options, item.Options);
			if (editorInfo == null)
			{
				AssertNotNull("Editor Info", item.EditorInfo);
			}
			else
			{
				AssertEquals("Editor Info", editorInfo, item.EditorInfo);
			}

			dataType.MaxLength = 10;
			AssertEquals("DefaultValue", "10", item.DefaultValue);

			dataType.MaxLength = 20;
			AssertEquals("DefaultValue", "20", item.DefaultValue);
		}
	}
}
