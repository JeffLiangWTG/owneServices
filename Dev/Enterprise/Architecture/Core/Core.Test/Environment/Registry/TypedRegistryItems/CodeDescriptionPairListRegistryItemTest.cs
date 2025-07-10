using System;
using System.Linq;
using CargoWise.ResourceStrings.Cache;
using CargoWiseOne.ResourceStrings;
using CargoWiseOne.ResourceStrings.Testing;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using ResString = Enterprise.ZArchitecture.Core.SourceGenerated.ResString;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(ReadOnlyCodeDescriptionPairList))]
	sealed class CodeDescriptionPairListRegistryItemTest : StronglyTypedRegistryItemTestCase<ReadOnlyCodeDescriptionPairList>
	{
		public void TestConstructor()
		{
			CodeDescriptionPairListRegistryItem registryItem = new CodeDescriptionPairListRegistryItem("Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", 5, RegistryStorageFlags.All);

			AssertEquals("Name", "Name", registryItem.Name);
			AssertEquals("Category", "Category", registryItem.Category);
			AssertEquals("Caption", "Caption", registryItem.Caption);
			AssertEquals("Hint", "Hint", registryItem.Hint);
			AssertEquals("DataType.CodeMaxLength", 5, ((CodeDescriptionPairListRegistryDataType)registryItem.DataType).CodeMaxLength);
			AssertEquals("Storage", RegistryStorageFlags.All, registryItem.Storage);
			AssertEquals("Options", RegistryOptions.Default, registryItem.Options);
			AssertEquals("DefaultValue.Count", 0, registryItem.DefaultValue.Count);

			CodeDescriptionPairListEditorInfo editorInfo = (CodeDescriptionPairListEditorInfo)registryItem.EditorInfo;
			AssertEquals("EditorInfo.ShowCodeColumn", true, editorInfo.ShowCodeColumn);
			AssertEquals("EditorInfo.ShowDescriptionColumn", true, editorInfo.ShowDescriptionColumn);
			AssertEquals("EditorInfo.DescriptionFieldCasing", CodeDescriptionPairListEditorInfo.CharacterCasing.Normal, editorInfo.DescriptionFieldCasing);
		}

		public void TestConstructorWithMultipleCategories()
		{
			CodeDescriptionPairListRegistryItem registryItem = new CodeDescriptionPairListRegistryItem("Name", (NoResString)"Caption", (NoResString)"Hint", 3, RegistryStorageFlags.All, (NoResString)"Category1", (NoResString)"Category2");

			AssertEquals("Name", "Name", registryItem.Name);
			AssertEquals("Categories.Length", 2, registryItem.Categories.Length);
			AssertEquals("Categories[0]", "Category1", registryItem.Categories[0]);
			AssertEquals("Categories[1]", "Category2", registryItem.Categories[1]);
			AssertEquals("Caption", "Caption", registryItem.Caption);
			AssertEquals("Hint", "Hint", registryItem.Hint);
			AssertEquals("DataType.CodeMaxLength", 3, ((CodeDescriptionPairListRegistryDataType)registryItem.DataType).CodeMaxLength);
			AssertEquals("Storage", RegistryStorageFlags.All, registryItem.Storage);
			AssertEquals("Options", RegistryOptions.Default, registryItem.Options);
			AssertEquals("DefaultValue.Count", 0, registryItem.DefaultValue.Count);

			CodeDescriptionPairListEditorInfo editorInfo = (CodeDescriptionPairListEditorInfo)registryItem.EditorInfo;
			AssertEquals("EditorInfo.ShowCodeColumn", true, editorInfo.ShowCodeColumn);
			AssertEquals("EditorInfo.ShowDescriptionColumn", true, editorInfo.ShowDescriptionColumn);
			AssertEquals("EditorInfo.DescriptionFieldCasing", CodeDescriptionPairListEditorInfo.CharacterCasing.Normal, editorInfo.DescriptionFieldCasing);
		}

		public void TestConstructorWithMultipleCategoriesAndDefaultValue()
		{
			CodeDescriptionPairList testList = new CodeDescriptionPairList();
			testList.AddPair("tst1", ResString.GetMultilingualString("tst1", "Test 1"));
			testList.AddPair("tst2", ResString.GetMultilingualString("tst2", "Test 2"));
			CodeDescriptionPairListRegistryItem registryItem = new CodeDescriptionPairListRegistryItem("Name", (NoResString)"Caption", (NoResString)"Hint", 3, RegistryStorageFlags.All, testList, (NoResString)"Category1", (NoResString)"Category2");

			AssertEquals("Name", "Name", registryItem.Name);
			AssertEquals("Categories.Length", 2, registryItem.Categories.Length);
			AssertEquals("Categories[0]", "Category1", registryItem.Categories[0]);
			AssertEquals("Categories[1]", "Category2", registryItem.Categories[1]);
			AssertEquals("Caption", "Caption", registryItem.Caption);
			AssertEquals("Hint", "Hint", registryItem.Hint);
			AssertEquals("DataType.CodeMaxLength", 3, ((CodeDescriptionPairListRegistryDataType)registryItem.DataType).CodeMaxLength);
			AssertEquals("Storage", RegistryStorageFlags.All, registryItem.Storage);
			AssertEquals("Options", RegistryOptions.Default, registryItem.Options);
			AssertEquals("DefaultValue.Count", 2, registryItem.DefaultValue.Count);
			AssertEquals("DefaultValue[0]", "tst1", registryItem.DefaultValue[0].Code);
			AssertEquals("DefaultValue[0]", "Test 1", registryItem.DefaultValue[0].Description);
			AssertEquals("DefaultValue[1]", "tst2", registryItem.DefaultValue[1].Code);
			AssertEquals("DefaultValue[1]", "Test 2", registryItem.DefaultValue[1].Description);

			CodeDescriptionPairListEditorInfo editorInfo = (CodeDescriptionPairListEditorInfo)registryItem.EditorInfo;
			AssertEquals("EditorInfo.ShowCodeColumn", true, editorInfo.ShowCodeColumn);
			AssertEquals("EditorInfo.ShowDescriptionColumn", true, editorInfo.ShowDescriptionColumn);
			AssertEquals("EditorInfo.DescriptionFieldCasing", CodeDescriptionPairListEditorInfo.CharacterCasing.Normal, editorInfo.DescriptionFieldCasing);
		}

		public void TestValueIsReadOnlyCodeDescriptionPairList()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.AddPair("ABC", ResString.GetMultilingualString("ABC", "ABC Description"));

			CodeDescriptionPairListRegistryItem registryItem = new CodeDescriptionPairListRegistryItem("CodeDescriptionPairListRegistryItem", null, null, null, 3, RegistryStorageFlags.All, list);

			ReadOnlyCodeDescriptionPairList defaultValue = registryItem.DefaultValue;
			AssertEquals("DefaultValue.Count", 1, defaultValue.Count);
			AssertEquals("DefaultValue.GetDescriptionFromCode(\"ABC\")", "ABC Description", defaultValue.GetDescriptionFromCode("ABC"));

			list.Clear();
			list.AddPair("123", ResString.GetMultilingualString("123", "123 Description"));

			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			AssertEquals("Value.Count", 1, registryItem.Value.Count);
			AssertEquals("Value.GetDescriptionFromCode(\"123\")", "123 Description", registryItem.Value.GetDescriptionFromCode("123"));
		}

		public void TestConstructorWithInner()
		{
			RegistryItemImpl registryItem = new RegistryItemImpl("Name", (NoResString)"Caption", (NoResString)"Hint",
				new CodeDescriptionPairListRegistryDataType(6), new CodeDescriptionPairListEditorInfo(), RegistryStorageFlags.Branch,
				RegistryOptions.Default, new ReadOnlyCodeDescriptionPairList(), false, null);

			CodeDescriptionPairListRegistryItem listRegistryItem = new CodeDescriptionPairListRegistryItem(registryItem, false, new ReadOnlyCodeDescriptionPairList());

			AssertEquals("Name", "Name", listRegistryItem.Name);
			AssertEquals("Caption", "Caption", listRegistryItem.Caption);
			AssertEquals("Hint", "Hint", listRegistryItem.Hint);
			AssertEquals("Registry Data Type", typeof(CodeDescriptionPairListRegistryDataType), listRegistryItem.DataType.GetType());
			AssertEquals("Code Max Length", 6, ((CodeDescriptionPairListRegistryDataType)listRegistryItem.DataType).CodeMaxLength);
			AssertEquals("Editor Info", typeof(CodeDescriptionPairListEditorInfo), listRegistryItem.EditorInfo.GetType());
			AssertEquals("Registry Storage Flag", RegistryStorageFlags.Branch, listRegistryItem.Storage);
			AssertEquals("Registry Options", RegistryOptions.Default, listRegistryItem.Options);
			AssertEquals("Default Value", typeof(ReadOnlyCodeDescriptionPairList), listRegistryItem.DefaultValue.GetType());
			AssertNull("Categories", listRegistryItem.Categories);
		}

		public void TestIsLocalizable()
		{
			using (IMockResourceStringCache chsMockData = Res.GetLanguageInstance(Enterprise.Core.SharedConstants.Languages.ChineseSimplified).UseMockData())
			{
				var list = new CodeDescriptionPairList();
				list.AddPair("ABC", ResString.GetMultilingualString("ABC", "ABC Description"));
				chsMockData.Put("ABC", new ResourceStringData("ABC", "ABC说明"));

				var registryItem = new CodeDescriptionPairListRegistryItem("CodeDescriptionPairListRegistryItem", null, null, null, 3, RegistryStorageFlags.All, list);
				AssertEquals("ABC Description", registryItem.Value.GetMultilingualDescriptionFromCode("ABC").ToString(Enterprise.Core.SharedConstants.Languages.EnglishAmerican));
				AssertEquals("ABC说明", registryItem.Value.GetMultilingualDescriptionFromCode("ABC").ToString(Enterprise.Core.SharedConstants.Languages.ChineseSimplified));

				list = new CodeDescriptionPairList();
				list.AddPair("XYZ", "Another Description");
				registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

				string key = ((ResourceString)registryItem.Value.GetMultilingualDescriptionFromCode("XYZ")).ResourceKey;
				chsMockData.Put(key, new ResourceStringData(key, "另一个说明"));
				AssertEquals("Another Description", registryItem.Value.GetMultilingualDescriptionFromCode("XYZ").ToString(Enterprise.Core.SharedConstants.Languages.EnglishAmerican));
				AssertEquals("另一个说明", registryItem.Value.GetMultilingualDescriptionFromCode("XYZ").ToString(Enterprise.Core.SharedConstants.Languages.ChineseSimplified));
			}
		}

		public void TestCaptionSource()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair("1", ResString._GetMultilingualString(55, "1", "One"));
			list.AddPair("2", ResString._GetMultilingualString(55, "2", "Two"));
			var registryItem = new CodeDescriptionPairListRegistryItem("CodeDescriptionPairListRegistryItem", null, null, null, 3, RegistryStorageFlags.All, list);
			var captionSource = (ITranslatableRegistryItemCaptionSource)registryItem;
			AssertEquals(55, captionSource.Asmid);
			AssertContainsExactElementsInAnyOrder(new MultilingualString[] { ResString._GetMultilingualString(55, "1", "One"), ResString._GetMultilingualString(55, "2", "Two") }, captionSource.DefaultStrings);
			AssertContainsExactElementsInAnyOrder(new string[] { "One", "Two" }, captionSource.GetCaptions(list));
			AssertContainsExactElementsInAnyOrder(new string[] { "One", "Two" }, new TranslatableRegistryItemValueCaptionSource(captionSource, list).GetRuntimeCaptions().Select(res => res.ToString()));
			AssertEquals("1", CustomizableDataResourceStrings.GetMultilingualString(captionSource, null, "One").ResourceKey);
			AssertEquals("2", CustomizableDataResourceStrings.GetMultilingualString(captionSource, null, "Two").ResourceKey);
			AssertEquals(CustomizableDataResourceStrings.GetCustomizableDataKey("R!CodeDescriptionPairListRegistryItem", "Three"), CustomizableDataResourceStrings.GetMultilingualString(captionSource, null, "Three").ResourceKey);
		}

		public void TestMultilingualCodes()
		{
			using (IMockResourceStringCache chsMockData = Res.GetLanguageInstance(Enterprise.Core.SharedConstants.Languages.ChineseSimplified).UseMockData())
			{
				var list = new CodeDescriptionPairList();
				list.AddPair(ResString._GetMultilingualString(55, "1", "One"));
				list.AddPair(ResString._GetMultilingualString(55, "2", "Two"));
				chsMockData.Put("1", new ResourceStringData("1", "一"));
				chsMockData.Put("2", new ResourceStringData("2", "二"));

				var registryItem = new CodeDescriptionPairListRegistryItem("CodeDescriptionPairListRegistryItem", null, null, null, 256, RegistryStorageFlags.All, list);
				AssertEquals("One", registryItem.Value[0].Code);
				AssertEquals("一", ((CodeDescriptionPair)registryItem.Value[0]).MultilingualCode.ToString(Enterprise.Core.SharedConstants.Languages.ChineseSimplified));

				var captionSource = (ITranslatableRegistryItemCaptionSource)registryItem;
				AssertEquals(55, captionSource.Asmid);
				AssertContainsExactElementsInAnyOrder(new MultilingualString[] { ResString._GetMultilingualString(55, "1", "One"), ResString._GetMultilingualString(55, "2", "Two") }, captionSource.DefaultStrings);
				AssertContainsExactElementsInAnyOrder(new string[] { "One", "Two" }, captionSource.GetCaptions(list));
				AssertContainsExactElementsInAnyOrder(new string[] { "One", "Two" }, new TranslatableRegistryItemValueCaptionSource(captionSource, list).GetRuntimeCaptions().Select(res => res.ToString()));
				AssertEquals("1", CustomizableDataResourceStrings.GetMultilingualString(captionSource, null, "One").ResourceKey);
				AssertEquals("2", CustomizableDataResourceStrings.GetMultilingualString(captionSource, null, "Two").ResourceKey);
				AssertEquals(CustomizableDataResourceStrings.GetCustomizableDataKey("R!CodeDescriptionPairListRegistryItem", "Three"), CustomizableDataResourceStrings.GetMultilingualString(captionSource, null, "Three").ResourceKey);

				list = new CodeDescriptionPairList();
				list.AddPair("Three");
				registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

				string key = ((ResourceString)((CodeDescriptionPair)registryItem.Value[0]).MultilingualCode).ResourceKey;
				chsMockData.Put(key, new ResourceStringData(key, "三"));
				AssertEquals("Three", registryItem.Value[0].Code);
				AssertEquals("三", ((CodeDescriptionPair)registryItem.Value[0]).MultilingualCode.ToString(Enterprise.Core.SharedConstants.Languages.ChineseSimplified));
			}
		}

		[ExpectNoExceptions]
		public void TestAddingDuplicatedKeyWithDifferentResString()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair("1", ResString._GetMultilingualString(55, "1", "One"));
			list.AddPair("2", ResString._GetMultilingualString(55, "2", "One"));
			var registryItem = new CodeDescriptionPairListRegistryItem("CodeDescriptionPairListRegistryItem", null, null, null, 3, RegistryStorageFlags.All, list);
			var captionSource = (IRegistryItemCaptionSource)registryItem;
			captionSource.GetCaptions(list);
		}

		protected override StronglyTypedRegistryItem<ReadOnlyCodeDescriptionPairList, ReadOnlyCodeDescriptionPairList> GetNewRegistryItem()
		{
			return new CodeDescriptionPairListRegistryItem("", (NoResString)"", (NoResString)"", 3, RegistryStorageFlags.System);
		}
	}
}
