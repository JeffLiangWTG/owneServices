using System;
using System.Linq;
using CargoWise.ResourceStrings.Cache;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CodeDescriptionBoolRegistryItem))]
	sealed class CodeDescriptionBoolRegistryItemTest : StronglyTypedRegistryItemTestCase<CodeDescriptionBoolCollection, CodeDescriptionBoolCollection>
	{
		public void TestConstructor()
		{
			CodeDescriptionPairList list1 = new CodeDescriptionPairList();
			CodeDescriptionPairList list2 = new CodeDescriptionPairList();
			CodeDescriptionPairList list3 = new CodeDescriptionPairList();
			CodeDescriptionPairList list4 = new CodeDescriptionPairList();
			ReadOnlyCodeDescriptionPairList emptyList = new ReadOnlyCodeDescriptionPairList();

			list1.AddPair("ABC", "ABC Description");
			list2.AddPair("123", "123 Description");
			list2.AddPair("456", "456 Description");
			list3.AddPair("XYZ", "XYZ Description");
			list4.AddPair("789", "789 Description");

			CodeDescriptionBoolRegistryItem item1 = new CodeDescriptionBoolRegistryItem("Name1", (NoResString)"Category1", (NoResString)"Caption1", (NoResString)"Hint1", RegistryStorageFlags.System, (NoResString)"Bool1");
			CodeDescriptionBoolRegistryItem item2 = new CodeDescriptionBoolRegistryItem("Name2", (NoResString)"Category2", (NoResString)"Caption2", (NoResString)"Hint2", RegistryStorageFlags.Company, (NoResString)"Bool2", true);
			CodeDescriptionBoolRegistryItem item3 = new CodeDescriptionBoolRegistryItem("Name3", (NoResString)"Category3", (NoResString)"Caption3", (NoResString)"Hint3", RegistryStorageFlags.Branch, (NoResString)"Bool3", list1);
			CodeDescriptionBoolRegistryItem item4 = new CodeDescriptionBoolRegistryItem("Name4", (NoResString)"Category4", (NoResString)"Caption4", (NoResString)"Hint4", RegistryStorageFlags.All, (NoResString)"Bool4", true, list2);
			CodeDescriptionBoolRegistryItem item5 = new CodeDescriptionBoolRegistryItem("Name5", (NoResString)"Category5", (NoResString)"Caption5", (NoResString)"Hint5", RegistryStorageFlags.SystemDepartment, new CodeDescriptionBoolRegistryEditorInfo((NoResString)"Bool5"));
			CodeDescriptionBoolRegistryItem item6 = new CodeDescriptionBoolRegistryItem("Name6", (NoResString)"Category6", (NoResString)"Caption6", (NoResString)"Hint6", RegistryStorageFlags.CompanyDepartment, new CodeDescriptionBoolRegistryEditorInfo((NoResString)"Bool6"), false, list1);
			CodeDescriptionBoolRegistryItem item7 = new CodeDescriptionBoolRegistryItem("Name7", (NoResString)"Category7", (NoResString)"Caption7", (NoResString)"Hint7", RegistryStorageFlags.BranchDepartment, (NoResString)"Bool7", new CodeDescriptionBoolCollection(list3, false));
			CodeDescriptionBoolRegistryItem item8 = new CodeDescriptionBoolRegistryItem("Name8", (NoResString)"Category8", (NoResString)"Caption8", (NoResString)"Hint8", RegistryStorageFlags.System | RegistryStorageFlags.Company, new CodeDescriptionBoolRegistryEditorInfo((NoResString)"Bool8"), new CodeDescriptionBoolCollection(list4, true));

			AssertItem(item1, "Name1", "Category1", "Caption1", "Hint1", RegistryStorageFlags.System, "Bool1", false, emptyList);
			AssertItem(item2, "Name2", "Category2", "Caption2", "Hint2", RegistryStorageFlags.Company, "Bool2", true, emptyList);
			AssertItem(item3, "Name3", "Category3", "Caption3", "Hint3", RegistryStorageFlags.Branch, "Bool3", false, list1);
			AssertItem(item4, "Name4", "Category4", "Caption4", "Hint4", RegistryStorageFlags.All, "Bool4", true, list2);
			AssertItem(item5, "Name5", "Category5", "Caption5", "Hint5", RegistryStorageFlags.SystemDepartment, "Bool5", false, emptyList);
			AssertItem(item6, "Name6", "Category6", "Caption6", "Hint6", RegistryStorageFlags.CompanyDepartment, "Bool6", false, list1);
			AssertItem(item7, "Name7", "Category7", "Caption7", "Hint7", RegistryStorageFlags.BranchDepartment, "Bool7", false, list3);
			AssertItem(item8, "Name8", "Category8", "Caption8", "Hint8", RegistryStorageFlags.System | RegistryStorageFlags.Company, "Bool8", true, list4);
		}

		public void TestIncludeMissingDefaults()
		{
			CodeDescriptionBoolCollection list1 = new CodeDescriptionBoolCollection();
			CodeDescriptionBoolCollection list2 = new CodeDescriptionBoolCollection();
			list1.Add("AAA", (NoResString)"A Desc", true);
			list1.Add("BBB", (NoResString)"B Desc", true);

			list2.Add("BBB", (NoResString)"B Desc", false);
			list2.Add("CCC", (NoResString)"C Desc", true);

			CodeDescriptionBoolRegistryItem item1 = new CodeDescriptionBoolRegistryItem("Name1", (NoResString)"Category1", (NoResString)"Caption1", (NoResString)"Hint1",
				RegistryStorageFlags.System | RegistryStorageFlags.Company, (NoResString)"Bool1", list1);
			item1.IncludeMissingDefaults = true;
			item1.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, list2);

			CodeDescriptionBoolCollection expectedList = new CodeDescriptionBoolCollection();
			expectedList.AddRange(list2);
			expectedList.Add(list1[0]);
			AssertEqualList(expectedList, item1.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			AssertEqualList(list1, item1.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEqualList(list2, item1.Value);

			CodeDescriptionBoolRegistryItem item2 = new CodeDescriptionBoolRegistryItem("Name2", (NoResString)"Category2", (NoResString)"Caption2", (NoResString)"Hint2",
				RegistryStorageFlags.System | RegistryStorageFlags.Company, (NoResString)"Bool2", list1);
			item2.IncludeMissingDefaults = true;
			item2.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list2);
			AssertEqualList(list1, item2.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			AssertEqualList(expectedList, item2.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEqualList(list2, item2.Value);
		}

		public void TestRemoveNonDefaults()
		{
			CodeDescriptionBoolCollection list1 = new CodeDescriptionBoolCollection();
			CodeDescriptionBoolCollection list2 = new CodeDescriptionBoolCollection();
			list1.Add("AAA", (NoResString)"A Desc", true);
			list1.Add("BBB", (NoResString)"B Desc", true);

			list2.Add("BBB", (NoResString)"B Desc", false);
			list2.Add("CCC", (NoResString)"C Desc", true);

			CodeDescriptionBoolRegistryItem item1 = new CodeDescriptionBoolRegistryItem("Name1", (NoResString)"Category1", (NoResString)"Caption1", (NoResString)"Hint1",
				RegistryStorageFlags.System | RegistryStorageFlags.Company, (NoResString)"Bool1", list1);
			item1.RemoveNonDefaults = true;
			item1.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, list2);

			CodeDescriptionBoolCollection expectedList = new CodeDescriptionBoolCollection();
			expectedList.Add(list2[0]);
			AssertEqualList(expectedList, item1.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			AssertEqualList(list1, item1.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEqualList(list2, item1.Value);

			CodeDescriptionBoolRegistryItem item2 = new CodeDescriptionBoolRegistryItem("Name2", (NoResString)"Category2", (NoResString)"Caption2", (NoResString)"Hint2",
				RegistryStorageFlags.System | RegistryStorageFlags.Company, (NoResString)"Bool2", list1);
			item2.RemoveNonDefaults = true;
			item2.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list2);
			AssertEqualList(list1, item2.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			AssertEqualList(expectedList, item2.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEqualList(list2, item2.Value);
		}

		public void TestGetValueShouldReturnValueWithCorrectCodeMaxLengthAndDefaultBoolForNewChild()
		{
			var defaultValue = new CodeDescriptionBoolCollection(100) { { "DEFAULT", (NoResString)"Default Description", true } };
			var newValue = new CodeDescriptionBoolCollection { { "NEW", (NoResString)"New Description", true } };
			CodeDescriptionBoolRegistryItem codeDescriptionBoolRegistryItem = new CodeDescriptionBoolRegistryItem("Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint2", RegistryStorageFlags.System | RegistryStorageFlags.Company, (NoResString)"Bool", defaultValue);
			codeDescriptionBoolRegistryItem.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, newValue);

			var value = codeDescriptionBoolRegistryItem.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);

			AssertEquals("The returned value from GetValue() should have the same CodeMaxLength as the default value's.", defaultValue.CodeMaxLength, value.CodeMaxLength);
			AssertEquals("The returned value from GetValue() should have the same DefaultBoolForNewChild as the default value's.", defaultValue.DefaultBoolForNewChild, value.DefaultBoolForNewChild);
		}

		public void TestTranslatable()
		{
			using (var chsMockData = Res.GetLanguageInstance(Core.SharedConstants.Languages.ChineseSimplified).UseMockData())
			{
				var list = new CodeDescriptionPairList();
				list.AddPair("0", ResString._GetMultilingualString(12, "0", "Zero"));
				list.AddPair("1", ResString._GetMultilingualString(12, "1", "One"));

				chsMockData.Put("0", new ResourceStringData("0", "零"));
				chsMockData.Put("1", new ResourceStringData("1", "一"));

				var registryItem = new CodeDescriptionBoolRegistryItem("Name", null, null, null, RegistryStorageFlags.All, null, list);
				AssertEquals("Zero", registryItem.Value[0].Description.ToString(Core.SharedConstants.Languages.EnglishAmerican));
				AssertEquals("零", registryItem.Value[0].Description.ToString(Core.SharedConstants.Languages.ChineseSimplified));
				AssertEquals("One", registryItem.Value[1].Description.ToString(Core.SharedConstants.Languages.EnglishAmerican));
				AssertEquals("一", registryItem.Value[1].Description.ToString(Core.SharedConstants.Languages.ChineseSimplified));

				var newList = new CodeDescriptionPairList();
				newList.AddPair("2", "Two");
				registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CodeDescriptionBoolCollection(newList));
				var key = ((ResourceString)registryItem.Value[0].Description).ResourceKey;
				chsMockData.Put(key, new ResourceStringData(key, "二"));
				AssertEquals("Two", registryItem.Value[0].Description.ToString(Core.SharedConstants.Languages.EnglishAmerican));
				AssertEquals("二", registryItem.Value[0].Description.ToString(Core.SharedConstants.Languages.ChineseSimplified));

				var captionSource = (ITranslatableRegistryItemCaptionSource)registryItem;
				AssertEquals(12, captionSource.Asmid);
				AssertContainsExactElementsInAnyOrder(new MultilingualString[] { ResString._GetMultilingualString(12, "0", "Zero"), ResString._GetMultilingualString(12, "1", "One") }, captionSource.DefaultStrings);
				AssertContainsExactElementsInAnyOrder(new string[] { "Zero", "One" }, captionSource.GetCaptions(new CodeDescriptionBoolCollection(list)));
				AssertContainsExactElementsInAnyOrder(new string[] { "Zero", "One" }, new TranslatableRegistryItemValueCaptionSource(captionSource, new CodeDescriptionBoolCollection(list)).GetRuntimeCaptions().Select(res => res.ToString()));
				AssertContainsExactElementsInAnyOrder(new string[] { "Two" }, new TranslatableRegistryItemValueCaptionSource(captionSource, new CodeDescriptionBoolCollection(newList)).GetRuntimeCaptions().Select(res => res.ToString()));
				AssertEquals("0", CustomizableDataResourceStrings.GetMultilingualString(captionSource, null, "Zero").ResourceKey);
				AssertEquals("1", CustomizableDataResourceStrings.GetMultilingualString(captionSource, null, "One").ResourceKey);
				AssertEquals(CustomizableDataResourceStrings.GetCustomizableDataKey("R!Name", "Two"), CustomizableDataResourceStrings.GetMultilingualString(captionSource, null, "Two").ResourceKey);
			}
		}

		public void TestBuildLogReference()
		{
			CodeDescriptionBoolCollection list1 = new CodeDescriptionBoolCollection();
			CodeDescriptionBoolCollection list2 = new CodeDescriptionBoolCollection();
			list1.Add("AAA", (NoResString)"A Desc", false);
			list1.Add("BBB", (NoResString)"B Desc", true);
			list1.Add("CCC", (NoResString)"C Desc", true);
			list1.Add("DDD", (NoResString)"D Desc", true);

			list2.Add("AAA", (NoResString)"A Desc", true);
			list2.Add("BBB", (NoResString)"B Desc", true);
			list2.Add("CCC", (NoResString)"C Desc", false);
			list2.Add("EEE", (NoResString)"E Desc", false);

			var item1 = new CodeDescriptionBoolRegistryItem("Name1", (NoResString)"Category1", (NoResString)"Caption1", (NoResString)"Hint1",
				RegistryStorageFlags.System | RegistryStorageFlags.Company, (NoResString)"Bool1", list1);
			var args = new RegistryItemWrapper.BuildLogReferenceArgs(item1, list1, list2);
			AssertEquals("A Desc Allow\r\nC Desc Disallow", CodeDescriptionBoolRegistryItem.BuildLogReference(args, "Allow", "Disallow"));
		}

		void AssertEqualList(ICodeDescriptionBoolList list1, ICodeDescriptionBoolList list2)
		{
			AssertEquals("count", list1.Count, list2.Count);
			for (int i = 0; i < list1.Count; ++i)
			{
				AssertEquals("code " + i, list1[i].Code, list2[i].Code);
				AssertEquals("desc " + i, list1[i].Description, list2[i].Description);
				AssertEquals("bool " + i, list1[i].Bool, list2[i].Bool);
			}
		}

		protected override StronglyTypedRegistryItem<CodeDescriptionBoolCollection, CodeDescriptionBoolCollection> GetNewRegistryItem()
		{
			return new CodeDescriptionBoolRegistryItem("", null, null, null, RegistryStorageFlags.System, (NoResString)"");
		}

		void AssertItem(CodeDescriptionBoolRegistryItem item, string expectedName, string expectedCategory, string expectedCaption, string expectedHint, RegistryStorageFlags expectedStorage, string expectedBoolColumnCaption, bool expectedDefaultBoolColumnValue, ReadOnlyCodeDescriptionPairList expectedDefaultValue)
		{
			AssertEquals("Name", expectedName, item.Name);
			AssertEquals("Category", expectedCategory, item.Category);
			AssertEquals("Caption", expectedCaption, item.Caption);
			AssertEquals("Hint", expectedHint, item.Hint);
			AssertEquals("Storage", expectedStorage, item.Storage);
			AssertEquals("EditorInfo.BoolColumnCaption", expectedBoolColumnCaption, ((CodeDescriptionBoolRegistryEditorInfo)item.EditorInfo).BoolColumnCaption);

			CodeDescriptionBoolCollection defaultValue = item.DefaultValue;
			AssertEquals("DefaultValue.Count", expectedDefaultValue.Count, defaultValue.Count);

			for (int i = 0; i < expectedDefaultValue.Count; i++)
			{
				AssertEquals(string.Format("DefaultValue[{0}].Code", i), expectedDefaultValue[i].Code, defaultValue[i].Code);
				AssertEquals(string.Format("DefaultValue[{0}].Description", i), expectedDefaultValue[i].Description, defaultValue[i].Description);
			}

			AssertEquals("DefaultValue.AddNew().Bool", expectedDefaultBoolColumnValue, defaultValue.AddNew().Bool);
		}
	}
}
