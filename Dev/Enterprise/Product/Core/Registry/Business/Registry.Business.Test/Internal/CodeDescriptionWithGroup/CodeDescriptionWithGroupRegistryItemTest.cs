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
	[TestedType(typeof(CodeDescriptionWithGroupRegistryItem))]
	public class CodeDescriptionWithGroupRegistryItemTest : StronglyTypedRegistryItemTestCase<CodeDescriptionWithGroupCollection, CodeDescriptionWithGroupCollection>
	{
		public void TestConstructor()
		{
			var groups = new CodeDescriptionPairList();
			groups.AddPair("ABC", "Group 1");
			groups.AddPair("CDE", "Group 2");
			groups.AddPair("EFG", "Group 3");

			var list1 = new CodeDescriptionWithGroupCollection(groups, "ABC");
			var list2 = new CodeDescriptionWithGroupCollection(groups, "CDE");
			var list3 = new CodeDescriptionWithGroupCollection(groups, "EFG");
			var list4 = new CodeDescriptionWithGroupCollection(groups, "");
			var emptyList = new CodeDescriptionWithGroupCollection();

			list1.Add("ABC", (NoResString)"ABC Description", "CDE");
			list2.Add("123", (NoResString)"123 Description", "ABC");
			list2.Add("456", (NoResString)"456 Description", "CDE");
			list3.Add("XYZ", (NoResString)"XYZ Description", "EFG");
			list4.Add("789", (NoResString)"789 Description", "ABC");

			var item1 = new CodeDescriptionWithGroupRegistryItem("Name1", (NoResString)"Category1", (NoResString)"Caption1", (NoResString)"Hint1", RegistryStorageFlags.System, (NoResString)"Group1");
			var item2 = new CodeDescriptionWithGroupRegistryItem("Name2", (NoResString)"Category2", (NoResString)"Caption2", (NoResString)"Hint2", RegistryStorageFlags.Company, (NoResString)"Group2", "ABC");
			var item3 = new CodeDescriptionWithGroupRegistryItem("Name3", (NoResString)"Category3", (NoResString)"Caption3", (NoResString)"Hint3", RegistryStorageFlags.Branch, (NoResString)"Group3", list1);
			var item4 = new CodeDescriptionWithGroupRegistryItem("Name4", (NoResString)"Category4", (NoResString)"Caption4", (NoResString)"Hint4", RegistryStorageFlags.All, (NoResString)"Group4", list2);
			var item5 = new CodeDescriptionWithGroupRegistryItem("Name5", (NoResString)"Category5", (NoResString)"Caption5", (NoResString)"Hint5", RegistryStorageFlags.SystemDepartment, new CodeDescriptionWithGroupRegistryEditorInfo((NoResString)"Group5"));
			var item6 = new CodeDescriptionWithGroupRegistryItem("Name6", (NoResString)"Category6", (NoResString)"Caption6", (NoResString)"Hint6", RegistryStorageFlags.CompanyDepartment, new CodeDescriptionWithGroupRegistryEditorInfo((NoResString)"Group6"), list3);
			var item7 = new CodeDescriptionWithGroupRegistryItem("Name7", (NoResString)"Category7", (NoResString)"Caption7", (NoResString)"Hint7", RegistryStorageFlags.BranchDepartment, (NoResString)"Group7", new CodeDescriptionWithGroupCollection(groups, "ABC"));
			var item8 = new CodeDescriptionWithGroupRegistryItem("Name8", (NoResString)"Category8", (NoResString)"Caption8", (NoResString)"Hint8", RegistryStorageFlags.System | RegistryStorageFlags.Company, new CodeDescriptionWithGroupRegistryEditorInfo((NoResString)"Group8"), list4);

			AssertItem(item1, "Name1", "Category1", "Caption1", "Hint1", RegistryStorageFlags.System, "Group1", "", emptyList);
			AssertItem(item2, "Name2", "Category2", "Caption2", "Hint2", RegistryStorageFlags.Company, "Group2", "ABC", emptyList);
			AssertItem(item3, "Name3", "Category3", "Caption3", "Hint3", RegistryStorageFlags.Branch, "Group3", "ABC", list1);
			AssertItem(item4, "Name4", "Category4", "Caption4", "Hint4", RegistryStorageFlags.All, "Group4", "CDE", list2);
			AssertItem(item5, "Name5", "Category5", "Caption5", "Hint5", RegistryStorageFlags.SystemDepartment, "Group5", "", emptyList);
			AssertItem(item6, "Name6", "Category6", "Caption6", "Hint6", RegistryStorageFlags.CompanyDepartment, "Group6", "EFG", list3);
			AssertItem(item7, "Name7", "Category7", "Caption7", "Hint7", RegistryStorageFlags.BranchDepartment, "Group7", "ABC", emptyList);
			AssertItem(item8, "Name8", "Category8", "Caption8", "Hint8", RegistryStorageFlags.System | RegistryStorageFlags.Company, "Group8", "", list4);
		}

		public void TestIncludeMissingDefaults()
		{
			var groups = new CodeDescriptionPairList();
			groups.AddPair("ABC", "Group 1");
			groups.AddPair("CDE", "Group 2");

			var list1 = new CodeDescriptionWithGroupCollection(groups, "ABC", 5);
			var list2 = new CodeDescriptionWithGroupCollection(groups, "CDE", 5);

			list1.Add("AAA", (NoResString)"A Desc", "ABC");
			list1.Add("BBBBB", (NoResString)"B Desc", "ABC");

			list2.Add("BBBBB", (NoResString)"B Desc", "ABC");
			list2.Add("CCC", (NoResString)"C Desc", "CDE");

			var item1 = new CodeDescriptionWithGroupRegistryItem("Name1", (NoResString)"Category1", (NoResString)"Caption1", (NoResString)"Hint1",
				RegistryStorageFlags.System | RegistryStorageFlags.Company, (NoResString)"Group1", list1);
			item1.IncludeMissingDefaults = true;
			item1.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, list2);

			var expectedList = new CodeDescriptionWithGroupCollection(groups, "ABC", 5);
			expectedList.Add((CodeDescriptionWithGroup)list2[0].Clone(null, null));
			expectedList.Add((CodeDescriptionWithGroup)list2[1].Clone(null, null));
			expectedList.Add((CodeDescriptionWithGroup)list1[0].Clone(null, null));
			AssertEqualList(expectedList, item1.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			AssertEqualList(list1, item1.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEqualList(list2, item1.Value);
			AssertEquals(5, item1.DefaultValue.CodeMaxLength);

			var item2 = new CodeDescriptionWithGroupRegistryItem("Name2", (NoResString)"Category2", (NoResString)"Caption2", (NoResString)"Hint2",
				RegistryStorageFlags.System | RegistryStorageFlags.Company, new CodeDescriptionWithGroupRegistryEditorInfo((NoResString)"Group2"), list1, true, false);
			item2.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list2);
			AssertEqualList(list1, item2.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			AssertEqualList(expectedList, item2.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEqualList(list2, item2.Value);
		}

		public void TestRemoveNonDefaults()
		{
			var groups = new CodeDescriptionPairList();
			groups.AddPair("ABC", "Group 1");
			groups.AddPair("CDE", "Group 2");

			var list1 = new CodeDescriptionWithGroupCollection(groups, "ABC", 7);
			var list2 = new CodeDescriptionWithGroupCollection(groups, "CDE", 7);

			list1.Add("AAA1234", (NoResString)"A Desc", "ABC");
			list1.Add("BBB", (NoResString)"B Desc", "ABC");

			list2.Add("BBB", (NoResString)"B Desc", "ABC");
			list2.Add("CCC", (NoResString)"C Desc", "CDE");

			var item1 = new CodeDescriptionWithGroupRegistryItem("Name1", (NoResString)"Category1", (NoResString)"Caption1", (NoResString)"Hint1",
				RegistryStorageFlags.System | RegistryStorageFlags.Company, (NoResString)"Bool1", list1);
			item1.RemoveNonDefaults = true;
			item1.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, list2);

			var expectedList = new CodeDescriptionWithGroupCollection(groups, "ABC");
			expectedList.Add((CodeDescriptionWithGroup)list2[0].Clone(null, null));
			AssertEqualList(expectedList, item1.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			AssertEqualList(list1, item1.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEqualList(list2, item1.Value);
			AssertEquals(7, item1.DefaultValue.CodeMaxLength);

			var item2 = new CodeDescriptionWithGroupRegistryItem("Name2", (NoResString)"Category2", (NoResString)"Caption2", (NoResString)"Hint2",
				RegistryStorageFlags.System | RegistryStorageFlags.Company, new CodeDescriptionWithGroupRegistryEditorInfo((NoResString)"Group5"), list1, false, true);
			item2.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list2);
			AssertEqualList(list1, item2.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			AssertEqualList(expectedList, item2.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEqualList(list2, item2.Value);
		}

		public void TestGetValueShouldReturnValueWithCorrectCodeMaxLengthGroupLookupDefaultGroupForNewChild()
		{
			var groups = new CodeDescriptionPairList();
			groups.AddPair("ABC", "Group 1");
			groups.AddPair("CDE", "Group 2");

			var defaultValue = new CodeDescriptionWithGroupCollection(groups, "ABC", 11) { { "DEF", (NoResString)"Default Description", "ABC" } };
			AssertContainsExactElementsInAnyOrder("The default value should have expected GroupLookup.", groups, defaultValue.GroupLookup);
			AssertEquals("The default value should have expected DefaultGroupForNewChild.", "ABC", defaultValue.DefaultGroupForNewChild);

			var newValue = new CodeDescriptionWithGroupCollection(groups, "ABC") { { "NEW", (NoResString)"New Description", "CDE" } };
			var codeDescriptionWithGroupRegistryItem = new CodeDescriptionWithGroupRegistryItem("Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint2", RegistryStorageFlags.System | RegistryStorageFlags.Company, (NoResString)"Group", defaultValue);
			codeDescriptionWithGroupRegistryItem.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, newValue);

			var value = codeDescriptionWithGroupRegistryItem.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);

			AssertEquals("The returned value from GetValue() should have the same CodeMaxLength as the default value's.", defaultValue.CodeMaxLength, value.CodeMaxLength);
			AssertContainsExactElementsInAnyOrder("The returned value from GetValue() should have the same GroupLookup as the default value's.", defaultValue.GroupLookup, value.GroupLookup);
			AssertEquals("The returned value from GetValue() should have the same DefaultGroupForNewChild as the default value's.", defaultValue.DefaultGroupForNewChild, value.DefaultGroupForNewChild);
		}

		public void TestTranslatable()
		{
			using (var chsMockData = Res.GetLanguageInstance(Core.SharedConstants.Languages.ChineseSimplified).UseMockData())
			{
				var groups = new CodeDescriptionPairList();
				groups.AddPair("ABC", "Group 1");
				groups.AddPair("CDE", "Group 2");

				var list = new CodeDescriptionWithGroupCollection(groups, "ABC");
				list.Add("0", ResString._GetMultilingualString(12, "0", "Zero"), "ABC");
				list.Add("1", ResString._GetMultilingualString(12, "1", "One"), "CDE");

				chsMockData.Put("0", new ResourceStringData("0", "零"));
				chsMockData.Put("1", new ResourceStringData("1", "一"));

				var registryItem = new CodeDescriptionWithGroupRegistryItem("Name", null, null, null, RegistryStorageFlags.All, (NoResString)"Group", list);
				AssertEquals("Zero", registryItem.Value[0].Description.ToString(Core.SharedConstants.Languages.EnglishAmerican));
				AssertEquals("零", registryItem.Value[0].Description.ToString(Core.SharedConstants.Languages.ChineseSimplified));
				AssertEquals("One", registryItem.Value[1].Description.ToString(Core.SharedConstants.Languages.EnglishAmerican));
				AssertEquals("一", registryItem.Value[1].Description.ToString(Core.SharedConstants.Languages.ChineseSimplified));

				var newList = new CodeDescriptionWithGroupCollection(groups, "ABC");
				newList.Add("2", (NoResString)"Two", "ABC");
				registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CodeDescriptionWithGroupCollection(newList));
				var key = ((ResourceString)registryItem.Value[0].Description).ResourceKey;
				chsMockData.Put(key, new ResourceStringData(key, "二"));
				AssertEquals("Two", registryItem.Value[0].Description.ToString(Core.SharedConstants.Languages.EnglishAmerican));
				AssertEquals("二", registryItem.Value[0].Description.ToString(Core.SharedConstants.Languages.ChineseSimplified));

				var captionSource = (ITranslatableRegistryItemCaptionSource)registryItem;
				AssertEquals(12, captionSource.Asmid);
				AssertContainsExactElementsInAnyOrder(new MultilingualString[] { ResString._GetMultilingualString(12, "0", "Zero"), ResString._GetMultilingualString(12, "1", "One") }, captionSource.DefaultStrings);
				AssertContainsExactElementsInAnyOrder(new string[] { "Zero", "One" }, captionSource.GetCaptions(new CodeDescriptionWithGroupCollection(list)));
				AssertContainsExactElementsInAnyOrder(new string[] { "Zero", "One" }, new TranslatableRegistryItemValueCaptionSource(captionSource, new CodeDescriptionWithGroupCollection(list)).GetRuntimeCaptions().Select(res => res.ToString()));
				AssertContainsExactElementsInAnyOrder(new string[] { "Two" }, new TranslatableRegistryItemValueCaptionSource(captionSource, new CodeDescriptionWithGroupCollection(newList)).GetRuntimeCaptions().Select(res => res.ToString()));
				AssertEquals("0", CustomizableDataResourceStrings.GetMultilingualString(captionSource, null, "Zero").ResourceKey);
				AssertEquals("1", CustomizableDataResourceStrings.GetMultilingualString(captionSource, null, "One").ResourceKey);
				AssertEquals(CustomizableDataResourceStrings.GetCustomizableDataKey("R!Name", "Two"), CustomizableDataResourceStrings.GetMultilingualString(captionSource, null, "Two").ResourceKey);
			}
		}

		void AssertEqualList(ICodeDescriptionWithGroupList list1, ICodeDescriptionWithGroupList list2)
		{
			AssertEquals("count", list1.Count, list2.Count);
			for (int i = 0; i < list1.Count; ++i)
			{
				AssertEquals("code " + i, list1[i].Code, list2[i].Code);
				AssertEquals("desc " + i, list1[i].Description, list2[i].Description);
				AssertEquals("bool " + i, list1[i].Group, list2[i].Group);
			}
		}

		protected override StronglyTypedRegistryItem<CodeDescriptionWithGroupCollection, CodeDescriptionWithGroupCollection> GetNewRegistryItem()
		{
			return new CodeDescriptionWithGroupRegistryItem("", null, null, null, RegistryStorageFlags.System, (NoResString)"");
		}

		void AssertItem(CodeDescriptionWithGroupRegistryItem item, string expectedName, string expectedCategory, string expectedCaption, string expectedHint, RegistryStorageFlags expectedStorage, string expectedGroupColumnCaption, string expectedDefaultGroupColumnValue, CodeDescriptionWithGroupCollection expectedDefaultValue)
		{
			AssertEquals("Name", expectedName, item.Name);
			AssertEquals("Category", expectedCategory, item.Category);
			AssertEquals("Caption", expectedCaption, item.Caption);
			AssertEquals("Hint", expectedHint, item.Hint);
			AssertEquals("Storage", expectedStorage, item.Storage);
			AssertEquals("EditorInfo.GroupColumnCaption", expectedGroupColumnCaption, ((CodeDescriptionWithGroupRegistryEditorInfo)item.EditorInfo).GroupColumnCaption);

			CodeDescriptionWithGroupCollection defaultValue = item.DefaultValue;
			AssertEquals("DefaultValue.Count", expectedDefaultValue.Count, defaultValue.Count);

			for (int i = 0; i < expectedDefaultValue.Count; i++)
			{
				AssertEquals(string.Format("DefaultValue[{0}].Code", i), expectedDefaultValue[i].Code, defaultValue[i].Code);
				AssertEquals(string.Format("DefaultValue[{0}].Description", i), expectedDefaultValue[i].Description, defaultValue[i].Description);
			}

			AssertEquals("DefaultValue.AddNew().Bool", expectedDefaultGroupColumnValue, defaultValue.AddNew().Group);
		}
	}
}
