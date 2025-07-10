using System;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CodeDescriptionWithThreeGroupsRegistryItem))]
	sealed class CodeDescriptionWithThreeGroupsRegistryItemTest : StronglyTypedRegistryItemTestCase<CodeDescriptionWithThreeGroupsCollection, CodeDescriptionWithThreeGroupsCollection>
	{
		public void TestConstructor()
		{
			var groups = new CodeDescriptionPairList();
			var groups2 = new CodeDescriptionPairList();
			var groups3 = new CodeDescriptionPairList();
			SetupGroupLookups(groups, groups2, groups3);

			var list1 = new CodeDescriptionWithThreeGroupsCollection(groups, groups2, groups3, 17);
			var emptyList = new CodeDescriptionWithThreeGroupsCollection(17);

			list1.Add("ABC", "CDE", "CD2", "CD3");

			RegistryItemImplWithDynamicDefaultValue.DefaultValueGetter valueGetterEmpty = (companyPK, branchPK, departmentPK) => emptyList;
			RegistryItemImplWithDynamicDefaultValue.DefaultValueGetter valueGetterList1 = (companyPK, branchPK, departmentPK) => list1;

			var item1 = new CodeDescriptionWithThreeGroupsRegistryItem(
				"Name1", (NoResString)"Category1", (NoResString)"Caption1", (NoResString)"Hint1", 3, RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport, //emptyList,
				new CodeDescriptionWithThreeGroupsRegistryEditorInfo((NoResString)"Group1", (NoResString)"Group2", (NoResString)"Group3", (NoResString)"ExtraDescription", (NoResString)"MainDescription", true, false),
				valueGetterEmpty,
				new ReadOnlyCodeDescriptionPairList(),
				new ReadOnlyCodeDescriptionPairList(),
				new ReadOnlyCodeDescriptionPairList());

			var item2 = new CodeDescriptionWithThreeGroupsRegistryItem(
				"Name2", (NoResString)"Category2", (NoResString)"Caption2", (NoResString)"Hint2", 3, RegistryStorageFlags.Company, RegistryOptions.IsOnlyForSupport,
				new CodeDescriptionWithThreeGroupsRegistryEditorInfo((NoResString)"Group1", (NoResString)"Group2", (NoResString)"Group3", (NoResString)"ExtraDescription", (NoResString)"MainDescription", true, false),
				valueGetterList1,
				new ReadOnlyCodeDescriptionPairList(),
				new ReadOnlyCodeDescriptionPairList(),
				new ReadOnlyCodeDescriptionPairList());

			AssertItem(item1, "Name1", "Category1", "Caption1", "Hint1", RegistryStorageFlags.System, "Group1", "Group2", "Group3", "ExtraDescription", "", "", "", emptyList);
			AssertItem(item2, "Name2", "Category2", "Caption2", "Hint2", RegistryStorageFlags.Company, "Group1", "Group2", "Group3", "ExtraDescription", "ABC", "CD1", "EF2", list1);
		}

		public void TestGetValueShouldReturnValueWithCorrectCodeMaxLengthGroupLookupsDefaultGroupsForNewChild()
		{
			var groups = new CodeDescriptionPairList();
			var groups2 = new CodeDescriptionPairList();
			var groups3 = new CodeDescriptionPairList();
			SetupGroupLookups(groups, groups2, groups3);

			var defaultValue = new CodeDescriptionWithThreeGroupsCollection(groups, groups2, groups3, 17)
			{
				{ "DEF", "ABC", "CD1", "EF2" }
			};
			defaultValue.CodeMaxLength = 17;
			AssertContainsExactElementsInAnyOrder("The default value should have expected GroupLookup.", groups, defaultValue.GroupLookup);
			AssertContainsExactElementsInAnyOrder("The default value should have expected Group2Lookup.", groups2, defaultValue.Group2Lookup);
			AssertContainsExactElementsInAnyOrder("The default value should have expected Group3Lookup.", groups3, defaultValue.Group3Lookup);
			AssertEquals("The default value should have expected DefaultGroupForNewChild.", "ABC", defaultValue.GroupLookup.DefaultCode);
			AssertEquals("The default value should have expected DefaultGroup2ForNewChild.", "CD1", defaultValue.Group2Lookup.DefaultCode);
			AssertEquals("The default value should have expected DefaultGroup3ForNewChild.", "EF2", defaultValue.Group3Lookup.DefaultCode);

			var newValue = new CodeDescriptionWithThreeGroupsCollection(groups, groups2, groups3, 17)
			{
				{ "NEW", "CDE", "AB1", "AB2" }
			};

			RegistryItemImplWithDynamicDefaultValue.DefaultValueGetter valueGetterDefaultValue = (companyPK, branchPK, departmentPK) => defaultValue;

			var codeDescriptionWithThreeGroupsRegistryItem = new CodeDescriptionWithThreeGroupsRegistryItem(
				"Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", 17, RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.IsOnlyForSupport,
				new CodeDescriptionWithThreeGroupsRegistryEditorInfo((NoResString)"Group1", (NoResString)"Group2", (NoResString)"Group3", (NoResString)"ExtraDescription", (NoResString)"MainDescription", null, true, false, false),
				valueGetterDefaultValue,
				new ReadOnlyCodeDescriptionPairList(),
				new ReadOnlyCodeDescriptionPairList(),
				new ReadOnlyCodeDescriptionPairList());
			codeDescriptionWithThreeGroupsRegistryItem.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, newValue);

			var value = codeDescriptionWithThreeGroupsRegistryItem.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);

			AssertEquals("The returned value from GetValue() should have the same CodeMaxLength as the default value's.", defaultValue.CodeMaxLength, value.CodeMaxLength);
			AssertContainsExactElementsInAnyOrder("The returned value from GetValue() should have the same GroupLookup as the default value's.", defaultValue.GroupLookup, value.GroupLookup);
			AssertContainsExactElementsInAnyOrder("The returned value from GetValue() should have the same Group2Lookup as the default value's.", defaultValue.Group2Lookup, value.Group2Lookup);
			AssertContainsExactElementsInAnyOrder("The returned value from GetValue() should have the same Group3Lookup as the default value's.", defaultValue.Group3Lookup, value.Group3Lookup);
			AssertEquals("The returned value from GetValue() should have the same DefaultGroupForNewChild as the default value's.", defaultValue.GroupLookup.DefaultCode, value.GroupLookup.DefaultCode);
			AssertEquals("The returned value from GetValue() should have the same DefaultGroup2ForNewChild as the default value's.", defaultValue.Group2Lookup.DefaultCode, value.Group2Lookup.DefaultCode);
			AssertEquals("The returned value from GetValue() should have the same DefaultGroup3ForNewChild as the default value's.", defaultValue.Group3Lookup.DefaultCode, value.Group3Lookup.DefaultCode);
		}

		protected override StronglyTypedRegistryItem<CodeDescriptionWithThreeGroupsCollection, CodeDescriptionWithThreeGroupsCollection> GetNewRegistryItem()
		{
			RegistryItemImplWithDynamicDefaultValue.DefaultValueGetter valueGetterDefaultValue = (companyPK, branchPK, departmentPK) => new CodeDescriptionWithThreeGroupsCollection(8);

			return new CodeDescriptionWithThreeGroupsRegistryItem(
				"", null, null, null, 8, RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport,
				new CodeDescriptionWithThreeGroupsRegistryEditorInfo((NoResString)"", (NoResString)"", (NoResString)"", (NoResString)"", (NoResString)"", null, true, false, false),
				valueGetterDefaultValue,
				new ReadOnlyCodeDescriptionPairList(),
				new ReadOnlyCodeDescriptionPairList(),
				new ReadOnlyCodeDescriptionPairList());
		}

		void AssertItem(CodeDescriptionWithThreeGroupsRegistryItem item, string expectedName, string expectedCategory, string expectedCaption, string expectedHint,
			RegistryStorageFlags expectedStorage, string expectedGroupColumnCaption, string expectedGroup2ColumnCaption, string expectedGroup3ColumnCaption, string expectedExtraDescriptionCaption,
			string expectedDefaultGroupColumnValue, string expectedDefaultGroup2ColumnValue, string expectedDefaultGroup3ColumnValue, CodeDescriptionWithThreeGroupsCollection expectedDefaultValue)
		{
			AssertEquals("Name", expectedName, item.Name);
			AssertEquals("Category", expectedCategory, item.Category);
			AssertEquals("Caption", expectedCaption, item.Caption);
			AssertEquals("Hint", expectedHint, item.Hint);
			AssertEquals("Storage", expectedStorage, item.Storage);
			AssertEquals("EditorInfo.GroupColumnCaption", expectedGroupColumnCaption, ((CodeDescriptionWithThreeGroupsRegistryEditorInfo)item.EditorInfo).GroupColumnCaption);
			AssertEquals("EditorInfo.Group2ColumnCaption", expectedGroup2ColumnCaption, ((CodeDescriptionWithThreeGroupsRegistryEditorInfo)item.EditorInfo).Group2ColumnCaption);
			AssertEquals("EditorInfo.Group3ColumnCaption", expectedGroup3ColumnCaption, ((CodeDescriptionWithThreeGroupsRegistryEditorInfo)item.EditorInfo).Group3ColumnCaption);
			AssertEquals("EditorInfo.ExtraDescriptionColumnCaption", expectedExtraDescriptionCaption, ((CodeDescriptionWithThreeGroupsRegistryEditorInfo)item.EditorInfo).ExtraDescriptionColumnCaption);

			CodeDescriptionWithThreeGroupsCollection defaultValue = item.DefaultValue;
			AssertEquals("DefaultValue.Count", expectedDefaultValue.Count, defaultValue.Count);

			for (int i = 0; i < expectedDefaultValue.Count; i++)
			{
				AssertEquals(string.Format("DefaultValue[{0}].Code", i), expectedDefaultValue[i].Code, defaultValue[i].Code);
				AssertEquals(string.Format("DefaultValue[{0}].MainDescription", i), expectedDefaultValue[i].MainDescription, defaultValue[i].MainDescription);
			}

			AssertEquals("DefaultValue.AddNew().Group", expectedDefaultGroupColumnValue, defaultValue.AddNew().GroupLookup?.DefaultCode ?? string.Empty);
			AssertEquals("DefaultValue.AddNew().Group2", expectedDefaultGroup2ColumnValue, defaultValue.AddNew().Group2Lookup?.DefaultCode ?? string.Empty);
			AssertEquals("DefaultValue.AddNew().Group3", expectedDefaultGroup3ColumnValue, defaultValue.AddNew().Group3Lookup?.DefaultCode ?? string.Empty);
		}

		void SetupGroupLookups(CodeDescriptionPairList groups, CodeDescriptionPairList groups2, CodeDescriptionPairList groups3)
		{
			groups.AddPair("ABC", "Group 1");
			groups.AddPair("CDE", "Group 2");
			groups.DefaultCode = "ABC";
			groups2.AddPair("AB1", "Group 11");
			groups2.AddPair("CD1", "Group 21");
			groups2.AddPair("EF1", "Group 31");
			groups2.DefaultCode = "CD1";
			groups3.AddPair("AB2", "Group 12");
			groups3.AddPair("CD2", "Group 22");
			groups3.AddPair("EF2", "Group 32");
			groups3.DefaultCode = "EF2";
		}
	}
}
