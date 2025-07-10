using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	public abstract class RegistryItemSetTestCaseWithFactory<T> : RegistryItemSetTestCase<T> where T : RegistryItemSet
	{
		#region Generic

		[DeveloperOnlyTest]
		public void TestNotCachedOptionAndCwOnlyOptionsNotUsedTogether()
		{
			var itemsThatFailed = new List<string>();
			foreach (IRegistryItem item in this.AllItems)
			{
				var isSomeKindOfCwOnlyFlag = item.HasOption(RegistryOptions.IsOnlyForCargoWise)
					|| item.HasOption(RegistryOptions.IsOnlyEditableBySupportIfHosted)
					|| item.HasOption(RegistryOptions.IsOnlyForDevelopers)
					|| item.HasOption(RegistryOptions.IsOnlyForSupport);

				if (isSomeKindOfCwOnlyFlag && item.HasOption(RegistryOptions.NotCached) && !item.HasOption(RegistryOptions.IsReadOnly))
				{
					var riw = item as RegistryItemWrapper;
					if (riw != null && !riw.IsExcludedFromCwOnlyNonCachedTest)
					{
						itemsThatFailed.Add(item.Name);
					}
				}
			}
			Assert(@"(Dev only) At least one of your registry items used a forbidden combination of RegistryOptions and was not excluded from this test. 
You have made an item that is marked as NotCached but is only editable by some kind of CW person (e.g. IsOnlyForSupport, IsOnlyForDeveloper, etc). 
That combination looks a bit stupid: ordinary users cannot change it (no need to cache), while CW users can change it (but they can restart the process controller). 
If this combination really is valid (e.g. the process controller itself changes values that users need to see instantly) then you can exclude it from this test by 
setting IsExcludedFromCwOnlyNonCachedTest to true. See [\Enterprise\Product\Operations\Customs\GB\Enterprise.Customs.GB.Registry\Business\GBCustomsDataRegistry.cs] for an example. 
The items that failed were:
" + new ZStringBuilder(itemsThatFailed).ToStringWithNewLineBetweenAppends(), itemsThatFailed.Count == 0);
		}

		protected void TestGenericRegistryItem(IRegistryItem item, string expectedName, string expectedCategory, string expectedCaption, string expectedHint, RegistryStorageFlags expectedStorage)
		{
			TestGenericRegistryItem(item, expectedName, expectedCategory, expectedCaption, expectedHint, expectedStorage, RegistryOptions.Default);
		}

		protected void TestGenericRegistryItem(IRegistryItem item, string expectedName, string expectedCategory, string expectedCaption, string expectedHint, RegistryStorageFlags expectedStorage, object expectedDefaultValue)
		{
			TestGenericRegistryItem(item, expectedName, expectedCategory, expectedCaption, expectedHint, expectedStorage, RegistryOptions.Default, expectedDefaultValue);
		}

		protected void TestGenericRegistryItem(IRegistryItem item, string expectedName, string expectedCategory, string expectedCaption, string expectedHint, RegistryStorageFlags expectedStorage, RegistryOptions expectedOptions)
		{
			AssertEquals("Name", expectedName, item.Name);
			AssertEquals("Category", expectedCategory, item.Category);
			AssertEquals("Caption", expectedCaption, item.Caption);
			AssertEquals("Hint", expectedHint, item.Hint);
			AssertEquals("Storage", expectedStorage, item.Storage);
			AssertEquals("Options", expectedOptions, item.Options);
		}

		protected void TestGenericRegistryItem(IRegistryItem item, string expectedName, string expectedCategory, string expectedCaption, string expectedHint, RegistryStorageFlags expectedStorage, RegistryOptions expectedOptions, object expectedDefaultValue)
		{
			TestGenericRegistryItem(item, expectedName, expectedCategory, expectedCaption, expectedHint, expectedStorage, expectedOptions);
			AssertEquals("DefaultValue", expectedDefaultValue, item.DefaultValue);
		}

		protected void TestGenericRegistryItem(IRegistryItem item, string expectedName, string[] expectedCategories, string expectedCaption, string expectedHint, RegistryStorageFlags expectedStorage, RegistryOptions expectedOptions, object expectedDefaultValue)
		{
			TestGenericRegistryItem(item, expectedName, expectedCategories[0], expectedCaption, expectedHint, expectedStorage, expectedOptions, expectedDefaultValue);
			AssertEquals("Categories.Count", expectedCategories.Length, item.Categories.Length);
			for (var i = 1; i < expectedCategories.Length; i++)
			{
				AssertEquals("Category", expectedCategories[i], item.Categories[i]);
			}
		}

		#endregion

		#region CodeSelectionCollectionRegistryItem

		protected void TestRegistryItem(CodeSelectionCollectionRegistryItem item, string expectedName, string expectedCategory, string expectedCaption, string expectedHint, RegistryStorageFlags expectedStorage, CodeDescriptionPairListProvider expectedCodesProvider, RegistryOptions expectedOptions, CodeSelectionCollection expectedDefaultValue)
		{
			var expectedList = expectedCodesProvider.CodeDescriptionPairList;

			TestGenericRegistryItem(item, expectedName, expectedCategory, expectedCaption, expectedHint, expectedStorage, expectedOptions);

			var dataType = (CodeSelectionCollectionRegistryDataType)item.DataType;
			var list = dataType.DefaultValue.Codes;
			AssertEquals("list.Count", expectedList.Count, list.Count);

			foreach (CodeDescriptionPair pair in list)
			{
				Assert(expectedList.ContainsCode(pair.Code));
			}

			AssertEquals("DefaultValue.Count", expectedDefaultValue.Count, item.DefaultValue.Count);

			var expectedDefaultCodes = new List<ZString>();
			foreach (CodeSelection codeSelection in expectedDefaultValue)
			{
				expectedDefaultCodes.Add(codeSelection.Code);
			}

			foreach (CodeSelection codeSelection in item.DefaultValue)
			{
				AssertCollectionContains("expectedDefaultCodes", codeSelection.Code, expectedDefaultCodes);
			}
		}

		#endregion

		#region BooleanRegistryItem

		protected void TestRegistryItem(BooleanRegistryItem item, string expectedName, string expectedCategory, string expectedCaption, string expectedHint, RegistryStorageFlags expectedStorage, bool expectedDefaultValue)
		{
			TestRegistryItem(item, expectedName, expectedCategory, expectedCaption, expectedHint, expectedStorage, RegistryOptions.Default, expectedDefaultValue);
		}

		protected void TestRegistryItem(BooleanRegistryItem item, string expectedName, string expectedCategory, string expectedCaption, string expectedHint, RegistryStorageFlags expectedStorage, RegistryOptions expectedOptions, bool expectedDefaultValue)
		{
			TestGenericRegistryItem(item, expectedName, expectedCategory, expectedCaption, expectedHint, expectedStorage, expectedOptions, expectedDefaultValue);
			SetValue(item, !expectedDefaultValue);
			if ((expectedOptions & RegistryOptions.CannotCallParameterlessValueGetter) == 0)
			{
				AssertEquals("Value", !expectedDefaultValue, item.Value);
			}
		}

		#endregion

		#region ChargeCodeListRegistryItem

		protected void TestRegistryItem(ChargeCodeListRegistryItem item, string expectedName, string expectedCategory, string expectedCaption, string expectedHint, RegistryStorageFlags expectedStorage, RegistryOptions expectedOptions, string expectedDefaultChargeCode, RegistryFindBoxFilter expectedFindBoxFilter)
		{
			TestGenericRegistryItem(item, expectedName, expectedCategory, expectedCaption, expectedHint, expectedStorage, expectedOptions, expectedDefaultChargeCode);
			AssertEquals("ExpectedFindBoxFilter", expectedFindBoxFilter, RegistryFindBoxFilter.MrgDsbOrMjaChargeCode);

			Guid new1 = Guid.NewGuid();
			Guid new2 = Guid.NewGuid();
			SetValue(item, new1 + "," + new2);
			AssertEquals("Value", new1 + "," + new2, item.Value);
		}

		#endregion

		#region CodeDescriptionBoolRegistryItem

		protected void TestRegistryItem(CodeDescriptionBoolRegistryItem item, string expectedName, string expectedCategory, string expectedCaption, string expectedHint, RegistryStorageFlags expectedStorage, RegistryOptions expectedOptions, string expectedBoolColumnCaption, CodeDescriptionBoolCollection newValue)
		{
			TestRegistryItem(item, expectedName, expectedCategory, expectedCaption, expectedHint, expectedStorage, expectedOptions, expectedBoolColumnCaption, false, 0, newValue, null);
		}

		protected void TestRegistryItem(CodeDescriptionBoolRegistryItem item, string expectedName, string expectedCategory, string expectedCaption, string expectedHint, RegistryStorageFlags expectedStorage, RegistryOptions expectedOptions, string expectedBoolColumnCaption)
		{
			TestRegistryItem(item, expectedName, expectedCategory, expectedCaption, expectedHint, expectedStorage, expectedOptions, expectedBoolColumnCaption, false, 0, new CodeDescriptionBoolCollection(), null);
		}

		protected void TestRegistryItem(CodeDescriptionBoolRegistryItem item, string expectedName, string expectedCategory, string expectedCaption, string expectedHint, RegistryStorageFlags expectedStorage, string expectedBoolColumnCaption, bool expectedDefaultBoolColumnValue, int expectedDefaultValueCount, params ICodeDescription[] expectedDefaultValueElements)
		{
			TestRegistryItem(item, expectedName, expectedCategory, expectedCaption, expectedHint, expectedStorage, RegistryOptions.Default, expectedBoolColumnCaption, expectedDefaultBoolColumnValue, expectedDefaultValueCount, new CodeDescriptionBoolCollection(), expectedDefaultValueElements);
		}

		protected void TestRegistryItem(CodeDescriptionBoolRegistryItem item, string expectedName, string expectedCategory, string expectedCaption, string expectedHint, RegistryStorageFlags expectedStorage, RegistryOptions expectedOptions, string expectedBoolColumnCaption, bool expectedDefaultBoolColumnValue, int expectedDefaultValueCount, params ICodeDescription[] expectedDefaultValueElements)
		{
			TestRegistryItem(item, expectedName, expectedCategory, expectedCaption, expectedHint, expectedStorage, expectedOptions, expectedBoolColumnCaption, expectedDefaultBoolColumnValue, expectedDefaultValueCount, new CodeDescriptionBoolCollection(), expectedDefaultValueElements);
		}

		protected void TestRegistryItem(CodeDescriptionBoolRegistryItem item, string expectedName, string expectedCategory, string expectedCaption, string expectedHint, RegistryStorageFlags expectedStorage, RegistryOptions expectedOptions, string expectedBoolColumnCaption, bool expectedDefaultBoolColumnValue, int expectedDefaultValueCount, CodeDescriptionBoolCollection newValue, params ICodeDescription[] expectedDefaultValueElements)
		{
			TestGenericRegistryItem(item, expectedName, expectedCategory, expectedCaption, expectedHint, expectedStorage, expectedOptions);
			AssertEquals("EditorInfo.BoolColumnCaption", expectedBoolColumnCaption, ((CodeDescriptionBoolRegistryEditorInfo)item.EditorInfo).BoolColumnCaption);

			CodeDescriptionBoolCollection defaultValue = item.DefaultValue;
			AssertEquals("DefaultValue.Count", expectedDefaultValueCount, defaultValue.Count);
			AssertEquals("Default Bool Column Value", expectedDefaultBoolColumnValue, ((CodeDescriptionBoolCollection)defaultValue.Clone(null, null)).AddNew().Bool);

			if (expectedDefaultValueElements != null)
			{
				foreach (ICodeDescription expectedElement in expectedDefaultValueElements)
				{
					AssertEquals("GetDescriptionFromCode(" + expectedElement.Code + ")", expectedElement.Description, defaultValue.GetDescriptionFromCode(expectedElement.Code));
				}
			}

			CodeDescriptionBool element1 = newValue.AddNew();
			CodeDescriptionBool element2 = newValue.AddNew();

			element1.Code = "C1";
			element1.Description = (NoResString)"D1";
			element1.Bool = true;
			element2.Code = "C2";
			element2.Description = (NoResString)"D2";
			element2.Bool = false;

			SetValue(item, newValue);
			CodeDescriptionBoolCollection actualNewValue = item.Value;

			AssertEquals("Value.Count", 2, actualNewValue.Count);
			AssertEquals("Value[0].Code", "C1", actualNewValue[0].Code);
			AssertEquals("Value[0].Description", "D1", actualNewValue[0].Description);
			AssertEquals("Value[0].Bool", true, actualNewValue[0].Bool);
			AssertEquals("Value[1].Code", "C2", actualNewValue[1].Code);
			AssertEquals("Value[1].Description", "D2", actualNewValue[1].Description);
			AssertEquals("Value[1].Bool", false, actualNewValue[1].Bool);
		}

		#endregion

		#region ParentAndChildCodeDescriptionBoolRegistryItem

		protected void TestRegistryItem(ParentAndChildCodeDescriptionBoolRegistryItem item, string expectedName, string expectedCategory, string expectedCaption, string expectedHint, RegistryStorageFlags expectedStorage, ParentCodeDescriptionBoolCollection expectedParentDefaultList, ParentAndChildCodeDescriptionBoolRegistryEditorInfo expectedEditorInfo)
		{
			TestGenericRegistryItem(item, expectedName, expectedCategory, expectedCaption, expectedHint, expectedStorage);

			ParentAndChildCodeDescriptionBoolRegistryEditorInfo actualEditorInfo = (ParentAndChildCodeDescriptionBoolRegistryEditorInfo)item.EditorInfo;

			AssertEquals("ParentListCaption", expectedEditorInfo.ParentListCaption, actualEditorInfo.ParentListCaption);
			AssertEquals("IsParentListReadOnly", expectedEditorInfo.IsParentListReadOnly, actualEditorInfo.IsParentListReadOnly);
			AssertEquals("ParentListEditorInfo.IsBoolColumnVisible", expectedEditorInfo.ParentListEditorInfo.IsBoolColumnVisible, actualEditorInfo.ParentListEditorInfo.IsBoolColumnVisible);
			AssertEquals("ParentListEditorInfo.IsBoolColumnVisibleCondition", expectedEditorInfo.ParentListEditorInfo.IsBoolColumnVisibleCondition, actualEditorInfo.ParentListEditorInfo.IsBoolColumnVisibleCondition);
			AssertEquals("ParentListEditorInfo.IsOnlyBoolColumnEditable", expectedEditorInfo.ParentListEditorInfo.IsOnlyBoolColumnEditable, actualEditorInfo.ParentListEditorInfo.IsOnlyBoolColumnEditable);

			AssertEquals("ChildListCaption", expectedEditorInfo.ChildListCaption, actualEditorInfo.ChildListCaption);
			AssertEquals("ChildListEditorInfo.IsBoolColumnVisible", expectedEditorInfo.ChildListEditorInfo.IsBoolColumnVisible, actualEditorInfo.ChildListEditorInfo.IsBoolColumnVisible);
			AssertEquals("ChildListEditorInfo.IsBoolColumnVisibleCondition", expectedEditorInfo.ChildListEditorInfo.IsBoolColumnVisibleCondition, actualEditorInfo.ChildListEditorInfo.IsBoolColumnVisibleCondition);
			AssertEquals("ChildListEditorInfo.IsOnlyBoolColumnEditable", expectedEditorInfo.ChildListEditorInfo.IsOnlyBoolColumnEditable, actualEditorInfo.ChildListEditorInfo.IsOnlyBoolColumnEditable);

			if (expectedParentDefaultList != null)
			{
				foreach (ParentCodeDescriptionBool expectedElement in expectedParentDefaultList)
				{
					AssertEquals("Default Element Description", expectedElement.Description, item.DefaultValue.GetDescriptionFromCode(expectedElement.Code));
					AssertEquals("Default Element Bool", expectedElement.Bool, item.DefaultValue.GetBoolFromCode(expectedElement.Code));
					foreach (CodeDescriptionBool expectedElementChild in expectedElement.ChildList)
					{
						ICodeDescriptionBoolList actualChildList = item.DefaultValue.GetChildList(expectedElement.Code);
						AssertEquals("Default Element Child Description", expectedElementChild.Description, actualChildList.GetDescriptionFromCode(expectedElementChild.Code));
						AssertEquals("Default Element Child Bool", expectedElementChild.Bool, actualChildList.GetBoolFromCode(expectedElementChild.Code));
					}
				}
			}

			ParentCodeDescriptionBoolCollection newValue = new ParentCodeDescriptionBoolCollection();

			ParentCodeDescriptionBool parentElement1 = newValue.AddNew();
			ParentCodeDescriptionBool parentElement2 = newValue.AddNew();

			parentElement1.Code = "P1c";
			parentElement1.Description = (NoResString)"P1d";
			parentElement1.Bool = true;
			CodeDescriptionBool parent1Child1 = parentElement1.ChildList.AddNew();
			parent1Child1.Code = "C1c";
			parent1Child1.Description = (NoResString)"C1d";
			parent1Child1.Bool = false;
			CodeDescriptionBool parent1Child2 = parentElement1.ChildList.AddNew();
			parent1Child2.Code = "C2c";
			parent1Child2.Description = (NoResString)"C2d";
			parent1Child2.Bool = true;

			parentElement2.Code = "P2c";
			parentElement2.Description = (NoResString)"P2d";
			parentElement2.Bool = false;
			CodeDescriptionBool parent2Child1 = parentElement2.ChildList.AddNew();
			parent2Child1.Code = "C3c";
			parent2Child1.Description = (NoResString)"C3d";
			parent2Child1.Bool = true;

			SetValue(item, newValue);
			ParentCodeDescriptionBoolCollection actualNewValue = (ParentCodeDescriptionBoolCollection)item.Value;

			AssertEquals("Value.Count", 2, actualNewValue.Count);

			AssertEquals("Value[0].Code", "P1c", actualNewValue[0].Code);
			AssertEquals("Value[0].Description", "P1d", actualNewValue[0].Description);
			AssertEquals("Value[0].Bool", true, actualNewValue[0].Bool);
			AssertEquals("Value[0].ChildList.Count", 2, actualNewValue[0].ChildList.Count);
			AssertEquals("Value[0].ChildList[0].Code", "C1c", actualNewValue[0].ChildList[0].Code);
			AssertEquals("Value[0].ChildList[0].Description", "C1d", actualNewValue[0].ChildList[0].Description);
			AssertEquals("Value[0].ChildList[0].Bool", false, actualNewValue[0].ChildList[0].Bool);
			AssertEquals("Value[0].ChildList[1].Code", "C2c", actualNewValue[0].ChildList[1].Code);
			AssertEquals("Value[0].ChildList[1].Description", "C2d", actualNewValue[0].ChildList[1].Description);
			AssertEquals("Value[0].ChildList[1].Bool", true, actualNewValue[0].ChildList[1].Bool);

			AssertEquals("Value[1].Code", "P2c", actualNewValue[1].Code);
			AssertEquals("Value[1].Description", "P2d", actualNewValue[1].Description);
			AssertEquals("Value[1].Bool", false, actualNewValue[1].Bool);
			AssertEquals("Value[1].ChildList.Count", 1, actualNewValue[1].ChildList.Count);
			AssertEquals("Value[1].ChildList[0].Code", "C3c", actualNewValue[1].ChildList[0].Code);
			AssertEquals("Value[1].ChildList[0].Description", "C3d", actualNewValue[1].ChildList[0].Description);
			AssertEquals("Value[1].ChildList[0].Bool", true, actualNewValue[1].ChildList[0].Bool);
		}

		#endregion

		#region CodeDescriptionPairListRegistryItem

		protected void TestRegistryItem(CodeDescriptionPairListRegistryItem item, string expectedName, string expectedCategory, string expectedCaption, string expectedHint, RegistryStorageFlags expectedStorage, int expectedDefaultValueCount, params ICodeDescription[] expectedDefaultValueElements)
		{
			TestRegistryItem(item, expectedName, expectedCategory, expectedCaption, expectedHint, expectedStorage, 3, expectedDefaultValueCount, expectedDefaultValueElements);
		}

		protected void TestRegistryItem(CodeDescriptionPairListRegistryItem item, string expectedName, string expectedCategory, string expectedCaption, string expectedHint, RegistryStorageFlags expectedStorage, RegistryOptions expectedOptions, int expectedDefaultValueCount, params ICodeDescription[] expectedDefaultValueElements)
		{
			TestRegistryItem(item, expectedName, expectedCategory, expectedCaption, expectedHint, expectedStorage, expectedOptions, 3, expectedDefaultValueCount, expectedDefaultValueElements);
		}

		protected void TestRegistryItem(CodeDescriptionPairListRegistryItem item, string expectedName, string expectedCategory, string expectedCaption, string expectedHint, RegistryStorageFlags expectedStorage, int expectedCodeMaxLength, int expectedDefaultValueCount, params ICodeDescription[] expectedDefaultValueElements)
		{
			TestRegistryItem(item, expectedName, expectedCategory, expectedCaption, expectedHint, expectedStorage, RegistryOptions.Default, expectedCodeMaxLength, expectedDefaultValueCount, expectedDefaultValueElements);
		}

		protected void TestRegistryItem(CodeDescriptionPairListRegistryItem item, string expectedName, string expectedCategory, string expectedCaption, string expectedHint, RegistryStorageFlags expectedStorage, RegistryOptions expectedOptions, int expectedCodeMaxLength, int expectedDefaultValueCount, params ICodeDescription[] expectedDefaultValueElements)
		{
			TestGenericRegistryItem(item, expectedName, expectedCategory, expectedCaption, expectedHint, expectedStorage, expectedOptions);
			AssertEquals("DataType.CodeMaxLength", expectedCodeMaxLength, ((CodeDescriptionPairListRegistryDataType)item.DataType).CodeMaxLength);

			ReadOnlyCodeDescriptionPairList defaultValue = item.DefaultValue;
			AssertEquals("DefaultValue.Count", expectedDefaultValueCount, defaultValue.Count);

			foreach (ICodeDescription expectedElement in expectedDefaultValueElements)
			{
				AssertEquals("GetDescriptionFromCode(" + expectedElement.Code + ")", expectedElement.Description, defaultValue.GetDescriptionFromCode(expectedElement.Code));
			}

			CodeDescriptionPairList newValue = new CodeDescriptionPairList();

			newValue.AddPair("C1", "D1");
			newValue.AddPair("C2", "D2");
			newValue.AddPair("C3", "D3");

			SetValue(item, newValue);
			ReadOnlyCodeDescriptionPairList actualNewValue = item.Value;

			AssertEquals("Value.Count", 3, actualNewValue.Count);
			AssertEquals("Value[0].Code", "C1", actualNewValue[0].Code);
			AssertEquals("Value[0].Description", "D1", actualNewValue[0].Description);
			AssertEquals("Value[1].Code", "C2", actualNewValue[1].Code);
			AssertEquals("Value[1].Description", "D2", actualNewValue[1].Description);
			AssertEquals("Value[2].Code", "C3", actualNewValue[2].Code);
			AssertEquals("Value[2].Description", "D3", actualNewValue[2].Description);
		}

		#endregion

		#region CodeDescriptionPairListWithDefaultCodeRegistryItem

		protected void TestRegistryItemWithDefaultCode(CodeDescriptionPairListWithDefaultCodeRegistryItem item, string expectedName, string expectedCategory, string expectedCaption, string expectedHint, RegistryStorageFlags expectedStorage)
		{
			AssertEquals("Name", expectedName, item.Name);
			AssertEquals("Category", expectedCategory, item.Category);
			AssertEquals("Caption", expectedCaption, item.Caption);
			AssertEquals("Hint", expectedHint, item.Hint);
			AssertEquals("Storage", expectedStorage, item.Storage);
		}

		protected void TestRegistryItemWithDefaultCode(CodeDescriptionPairListWithDefaultCodeRegistryItem item, string expectedName, string expectedCategory, string expectedCaption, string expectedHint, RegistryStorageFlags expectedStorage, int expectedCodeMaxLength, int expectedDefaultValueCount)
		{
			TestRegistryItemWithDefaultCode(item, expectedName, expectedCategory, expectedCaption, expectedHint, expectedStorage);
			AssertEquals("DefaultValue.Count", expectedDefaultValueCount, item.Value.Count);

			foreach (ICodeDescription element in (ICodeDescriptionPairList)item.Value)
			{
				AssertEquals("GetDescriptionFromCode(" + element.Code + ")", element.Description, item.Value.GetDescriptionFromCode(element.Code));
			}

			SystemDefinableCodeDescriptionBoolCollection newValue = new SystemDefinableCodeDescriptionBoolCollection();
			SystemDefinableCodeDescriptionBool value1 = new SystemDefinableCodeDescriptionBool();
			value1.Code = "C1";
			value1.Description = (NoResString)"D1";
			SystemDefinableCodeDescriptionBool value2 = new SystemDefinableCodeDescriptionBool();
			value2.Code = "C2";
			value2.Description = (NoResString)"D2";
			SystemDefinableCodeDescriptionBool value3 = new SystemDefinableCodeDescriptionBool();
			value3.Code = "C3";
			value3.Description = (NoResString)"D3";

			newValue.Add(value1);
			newValue.Add(value2);
			newValue.Add(value3);
			newValue.SetDefaultCode("C2", true);

			SetValue(item, newValue);

			AssertEquals("Default code", "C2", item.Value.DefaultCode);
		}

		#endregion

		#region CodeDescriptionPairListWithDefaultCodeAndExtraBoolRegistryItem

		protected void TestRegistryItem(CodeDescriptionPairListWithDefaultCodeAndExtraBoolRegistryItem item, string expectedName, string expectedCategory, string expectedCaption, string expectedHint, RegistryStorageFlags expectedStorage, RegistryOptions expectedOptions, CodeDescriptionPairListWithDefaultCodeAndExtraBoolRegistryDataType expectedDefaultValue, bool expectedAllowNew)
		{
			TestGenericRegistryItem(item, expectedName, expectedCategory, expectedCaption, expectedHint, expectedStorage, expectedOptions);
			var expected = expectedDefaultValue.SystemDefinedList.Cast<ICodeDescription>();
			var collection = (SystemDefinableCodeDescriptionBoolWithExtraBoolCollection)item.DefaultValue;
			AssertContainsExactElementsInAnyOrder("Default collection", expected, collection);
			AssertEquals("AllowNew", expectedAllowNew, collection.AllowNew);
		}

		#endregion

		#region MilestoneEventUpdatesRegistryItem

		protected void TestRegistryItem(IMilestoneEventUpdatesRegistryItem item, string expectedName, string expectedCategory, string expectedCaption, string expectedHint, RegistryStorageFlags expectedStorage, MilestoneEventUpdatesCollection expectedDefaultValue)
		{
			TestGenericRegistryItem(item, expectedName, expectedCategory, expectedCaption, expectedHint, expectedStorage);
			MilestoneEventUpdatesCollection defaultValue = item.DefaultValue as MilestoneEventUpdatesCollection;
			AssertEquals("Default value collection count", expectedDefaultValue.Count, defaultValue.Count);
			foreach (MilestoneEventUpdates eventUpdate in expectedDefaultValue)
			{
				AssertContains(eventUpdate, defaultValue);
			}
		}

		void AssertContains(MilestoneEventUpdates expectedEventUpdate, MilestoneEventUpdatesCollection testCollecton)
		{
			bool eventFound = false;
			foreach (MilestoneEventUpdates testEventUpdate in testCollecton)
			{
				if (testEventUpdate.EventType == expectedEventUpdate.EventType)
				{
					foreach (string expectedWebParty in expectedEventUpdate.WebPartyTypes)
					{
						AssertEquals("Web Part Type: " + expectedWebParty, expectedEventUpdate.GetValue(expectedWebParty), testEventUpdate.GetValue(expectedWebParty));
					}
					eventFound = true;
					break;
				}
			}
			if (!eventFound)
			{
				AssertEquals("Settings for default milestone event " + expectedEventUpdate.EventType + " could not be found", false);
			}
		}

		#endregion

		#region EventVisibilityRegistryItem

		protected void TestRegistryItem(IEventVisibilityRegistryItem item, string expectedName, string expectedCategory, string expectedCaption, string expectedHint, RegistryStorageFlags expectedStorage, EventVisibilityCollection expectedDefaultValue)
		{
			TestGenericRegistryItem(item, expectedName, expectedCategory, expectedCaption, expectedHint, expectedStorage);
			var defaultValue = item.DefaultValue as EventVisibilityCollection;
			AssertEquals("Default value collection count", expectedDefaultValue.Count, defaultValue.Count);
			foreach (EventVisibility expectedEvent in expectedDefaultValue)
			{
				var contains = false;
				foreach (EventVisibility actualEvent in defaultValue)
				{
					if (expectedEvent.EventCode == actualEvent.EventCode)
					{
						contains = true;
						break;
					}
				}
				Assert(string.Format("Event code '{0}' was expected in the default values", expectedEvent), contains);
			}
		}

		#endregion

		#region CodePairListRegistryItem

		protected void TestRegistryItem(CodePairRegistryItem item, string expectedName, string expectedCategory, string expectedCaption, string expectedHint, RegistryStorageFlags expectedStorage, ReadOnlyCodeDescriptionPairList expectedLookUpList, string expectedDefaultValue)
		{
			TestGenericRegistryItem(item, expectedName, expectedCategory, expectedCaption, expectedHint, expectedStorage, expectedDefaultValue);
			CodePairRegistryDataType dataType = (CodePairRegistryDataType)item.DataType;

			AssertEquals("DataType.LookUpList.Count", expectedLookUpList.Count, dataType.LookUpList.Count);

			foreach (ICodeDescription element in expectedLookUpList)
			{
				AssertEquals("GetDescriptionFromCode(" + element.Code + ")", element.Description, dataType.LookUpList.GetDescriptionFromCode(element.Code));
			}

			SetValue(item, expectedLookUpList[0].Code);
			AssertEquals("Value", expectedLookUpList[0].Code, item.Value);
		}

		protected void TestRegistryItem(FilterLayoutCodePairRegistryItem item, string expectedName, string expectedCategory, string expectedCaption, string expectedHint, RegistryStorageFlags expectedStorage, RegistryOptions options)
		{
			TestGenericRegistryItem(item, expectedName, expectedCategory, expectedCaption, expectedHint, expectedStorage, options);
		}

		protected void TestRegistryItem(CodePairRegistryItem item,
			string expectedName,
			string expectedCategory,
			string expectedCaption,
			string expectedHint,
			RegistryStorageFlags expectedStorage,
			RegistryOptions expectedOptions,
			ReadOnlyCodeDescriptionPairList expectedLookUpList,
			string expectedDefaultValue)
		{
			TestGenericRegistryItem(item, expectedName, expectedCategory, expectedCaption, expectedHint, expectedStorage, expectedOptions, expectedDefaultValue);
			CodePairRegistryDataType dataType = (CodePairRegistryDataType)item.DataType;

			string message = string.Format("Testing {0} item, ", expectedName);

			Assert(message + " expected lookUpList shouldn't be empty", expectedLookUpList.Count > 0);
			AssertEquals(message + "DataType.LookUpList.Count", expectedLookUpList.Count, dataType.LookUpList.Count);

			foreach (ICodeDescription element in expectedLookUpList)
			{
				AssertEquals(message + "GetDescriptionFromCode(" + element.Code + ")", element.Description, dataType.LookUpList.GetDescriptionFromCode(element.Code));
			}

			SetValue(item, expectedLookUpList[0].Code);
			if ((expectedOptions & RegistryOptions.CannotCallParameterlessValueGetter) == 0)
			{
				AssertEquals(message + "Value", expectedLookUpList[0].Code, item.Value);
			}
		}

		#endregion

		#region DecimalRegistryItem

		protected void TestRegistryItem(DecimalRegistryItem item, string expectedName, string expectedCategory, string expectedCaption, string expectedHint, RegistryStorageFlags expectedStorage, decimal expectedDefaultValue)
		{
			TestRegistryItem(item, expectedName, expectedCategory, expectedCaption, expectedHint, expectedStorage, RegistryOptions.Default, expectedDefaultValue);
		}

		protected void TestRegistryItem(DecimalRegistryItem item, string expectedName, string expectedCategory, string expectedCaption, string expectedHint, RegistryStorageFlags expectedStorage, RegistryOptions expectedOptions, decimal expectedDefaultValue)
		{
			TestGenericRegistryItem(item, expectedName, expectedCategory, expectedCaption, expectedHint, expectedStorage, expectedOptions, expectedDefaultValue);
			SetValue(item, 887766m);
			AssertEquals("Value", 887766m, item.Value);
		}

		protected void TestRegistryItem(DecimalRegistryItem item, string expectedName, string expectedCategory, string expectedCaption, string expectedHint, RegistryStorageFlags expectedStorage, RegistryOptions expectedOptions, decimal expectedDefaultValue, decimal expectedMinValue, decimal expectedMaxValue)
		{
			TestGenericRegistryItem(item, expectedName, expectedCategory, expectedCaption, expectedHint, expectedStorage, expectedOptions, expectedDefaultValue);
			DecimalRegistryDataType dataType = (DecimalRegistryDataType)item.DataType;
			AssertEquals("Minimum value", expectedMinValue, (decimal)dataType.LowerBound);
			AssertEquals("Maximum value", expectedMaxValue, (decimal)dataType.UpperBound);
			var boundValue = Math.Min((decimal)dataType.UpperBound, 15m);
			SetValue(item, boundValue);
			if ((expectedOptions & RegistryOptions.CannotCallParameterlessValueGetter) == 0)
			{
				AssertEquals("Value", boundValue, item.Value);
			}
		}

		#endregion

		#region GuidRegistryItem

		protected void TestRegistryItem(GuidRegistryItem item, string expectedName, string expectedCategory, string expectedCaption, string expectedHint, RegistryStorageFlags expectedStorage, RegistryFindBoxCollection expectedFindBoxCollection, Guid expectedDefaultValue)
		{
			TestRegistryItem(item, expectedName, expectedCategory, expectedCaption, expectedHint, expectedStorage, RegistryOptions.IsValueMandatory, expectedFindBoxCollection, expectedDefaultValue);
		}

		protected void TestRegistryItem(GuidRegistryItem item, string expectedName, string[] expectedCategories, string expectedCaption, string expectedHint, RegistryStorageFlags expectedStorage, RegistryFindBoxCollection expectedFindBoxCollection, Guid expectedDefaultValue)
		{
			TestGenericRegistryItem(item, expectedName, expectedCategories, expectedCaption, expectedHint, expectedStorage, RegistryOptions.IsValueMandatory, expectedDefaultValue);
			AssertGuidRegistryItem(item, RegistryOptions.IsValueMandatory, expectedFindBoxCollection);
		}

		protected void TestRegistryItem(GuidRegistryItem item, string expectedName, string expectedCategory, string expectedCaption, string expectedHint, RegistryStorageFlags expectedStorage, RegistryOptions expectedOptions, RegistryFindBoxCollection expectedFindBoxCollection, Guid expectedDefaultValue)
		{
			TestGenericRegistryItem(item, expectedName, expectedCategory, expectedCaption, expectedHint, expectedStorage, expectedOptions, expectedDefaultValue);
			AssertGuidRegistryItem(item, expectedOptions, expectedFindBoxCollection);
		}

		void AssertGuidRegistryItem(GuidRegistryItem item, RegistryOptions expectedOptions, RegistryFindBoxCollection expectedFindBoxCollection)
		{
			var editorInfo = (GuidFindBoxRegistryEditorInfo)item.EditorInfo;
			AssertEquals("EditorInfo.FindBoxCollection", expectedFindBoxCollection, editorInfo.FindBoxCollection);

			var newGuid = Guid.NewGuid();
			SetValue(item, newGuid);
			if ((expectedOptions & RegistryOptions.CannotCallParameterlessValueGetter) == 0)
			{
				AssertEquals("Value", newGuid, item.Value);
			}
		}

		#endregion

		#region GuidArrayRegistryItem

		protected void TestRegistryItem(GuidArrayRegistryItem item, string expectedName, string expectedCategory, string expectedCaption, string expectedHint, RegistryStorageFlags expectedStorage, Type expectedEditorInfoType, Guid[] expectedDefaultValue)
		{
			TestRegistryItem(item, expectedName, expectedCategory, expectedCaption, expectedHint, expectedStorage, RegistryOptions.Default, expectedEditorInfoType, expectedDefaultValue);
		}

		protected void TestRegistryItem(GuidArrayRegistryItem item, string expectedName, string expectedCategory, string expectedCaption, string expectedHint, RegistryStorageFlags expectedStorage, RegistryOptions expectedOptions, Type expectedEditorInfoType, Guid[] expectedDefaultValue)
		{
			TestGenericRegistryItem(item, expectedName, expectedCategory, expectedCaption, expectedHint, expectedStorage, expectedOptions);
			AssertEquals("EditorInfo.GetType()", expectedEditorInfoType, item.EditorInfo.GetType());
			AssertArrayEquals(item.DefaultValue, expectedDefaultValue, "DefaultValue");

			Guid newGuid1 = Guid.NewGuid();
			Guid newGuid2 = Guid.NewGuid();
			Guid[] newValue = new Guid[] { newGuid1, newGuid2 };
			SetValue(item, newValue);
			AssertArrayEquals(item.Value, newValue, "DefaultValue");
		}

		#endregion

		#region ImageRegistryItem

		protected void TestRegistryItem(ImageRegistryItem item, string expectedName, string expectedCategory, string expectedCaption, string expectedHint, RegistryStorageFlags expectedStorage, RegistryOptions expectedOptions)
		{
			TestGenericRegistryItem(item, expectedName, expectedCategory, expectedCaption, expectedHint, expectedStorage, expectedOptions, null);

			using (Image image = new Bitmap(1, 2))
			{
				SetValue(item, image);
				using (Image value = item.Value)
				{
					AssertEquals("Value", true, Utilities.IsImageEqual(image, item.Value));
				}
			}
		}

		#endregion

		#region BinaryRegistryItem

		protected void TestRegistryItem(BinaryRegistryItem item, string expectedName, string expectedCategory, string expectedCaption, string expectedHint, RegistryStorageFlags expectedStorage, RegistryOptions expectedOptions, byte[] defaultValue)
		{
			TestGenericRegistryItem(item, expectedName, expectedCategory, expectedCaption, expectedHint, expectedStorage, expectedOptions);
			AssertArrayEquals(defaultValue, item.DefaultValue, "DefaultValue");
		}

		#endregion

		#region IntRegistryItem

		protected void TestRegistryItem(IntRegistryItem item, string expectedName, string expectedCategory, string expectedCaption, string expectedHint, RegistryStorageFlags expectedStorage, int expectedDefaultValue)
		{
			TestRegistryItem(item, expectedName, expectedCategory, expectedCaption, expectedHint, expectedStorage, RegistryOptions.Default, expectedDefaultValue);
		}

		protected void TestRegistryItem(IntRegistryItem item, string expectedName, string expectedCategory, string expectedCaption, string expectedHint, RegistryStorageFlags expectedStorage, RegistryOptions expectedOptions, int expectedDefaultValue)
		{
			TestGenericRegistryItem(item, expectedName, expectedCategory, expectedCaption, expectedHint, expectedStorage, expectedOptions, expectedDefaultValue);
			SetValue(item, 1875);
			if ((expectedOptions & RegistryOptions.CannotCallParameterlessValueGetter) == 0)
			{
				AssertEquals("Value", 1875, item.Value);
			}
		}

		protected void TestRegistryItem(IntRegistryItem item, string expectedName, string expectedCategory, string expectedCaption, string expectedHint, RegistryStorageFlags expectedStorage, RegistryOptions expectedOptions, int expectedDefaultValue, int expectedMinValue, int expectedMaxValue)
		{
			TestGenericRegistryItem(item, expectedName, expectedCategory, expectedCaption, expectedHint, expectedStorage, expectedOptions, expectedDefaultValue);
			Assert("DataType must be IntRegistryDataType", item.DataType is IntRegistryDataType);
			IntRegistryDataType dataType = (IntRegistryDataType)item.DataType;
			AssertEquals("Minimum value", expectedMinValue, (int)dataType.LowerBound);
			AssertEquals("Maximum value", expectedMaxValue, (int)dataType.UpperBound);
			int randValue = new Random().Next(expectedMinValue, expectedMaxValue);
			SetValue(item, randValue);
			if ((expectedOptions & RegistryOptions.CannotCallParameterlessValueGetter) == 0)
			{
				AssertEquals("Value", randValue, item.Value);
			}
		}

		#endregion

		#region LinkRegistryItem

		public void TestRegistryItem(LinkRegistryItem item, string expectedCategory, string expectedCaption, string expectedHint, ModuleIdentifier expectedModuleID)
		{
			IRegistryItem registryItem = item;

			AssertEquals("Category", expectedCategory, registryItem.Category);
			AssertEquals("Caption", expectedCaption, registryItem.Caption);
			AssertEquals("Hint", expectedHint, registryItem.Hint);
			AssertEquals("ModuleID", expectedModuleID, item.ModuleID);
		}

		#endregion

		#region StringRegistryItem

		protected void TestRegistryItem(StringRegistryItem item, string expectedName, string expectedCategory, string expectedCaption, string expectedHint, RegistryStorageFlags expectedStorage, TextEditorType expectedTextEditorType, string expectedDefaultValue)
		{
			TestRegistryItem(item, expectedName, expectedCategory, expectedCaption, expectedHint, expectedStorage, RegistryOptions.Default, expectedTextEditorType, expectedDefaultValue);
		}

		protected void TestRegistryItem(StringRegistryItem item, string expectedName, string expectedCategory, string expectedCaption, string expectedHint, RegistryStorageFlags expectedStorage, TextEditorType expectedTextEditorType, RegistryOptions expectedOptions, string expectedDefaultValue)
		{
			TestRegistryItem(item, expectedName, expectedCategory, expectedCaption, expectedHint, expectedStorage, expectedOptions, expectedTextEditorType, expectedDefaultValue);
		}

		protected void TestRegistryItem(StringRegistryItem item, string expectedName, string expectedCategory, string expectedCaption, string expectedHint, RegistryStorageFlags expectedStorage, RegistryOptions expectedOptions, TextEditorType expectedTextEditorType, string expectedDefaultValue, string testValueToSetAndRead = "This is the new value.")
		{
			TestGenericRegistryItem(item, expectedName, expectedCategory, expectedCaption, expectedHint, expectedStorage, expectedOptions, expectedDefaultValue);
			AssertEquals("EditorInfo.EditorType", expectedTextEditorType, ((TextRegistryEditorInfo)item.EditorInfo).EditorType);
			SetValue(item, testValueToSetAndRead);
			if ((expectedOptions & RegistryOptions.CannotCallParameterlessValueGetter) == 0)
			{
				AssertEquals("Value", testValueToSetAndRead, item.Value);
			}
		}

		protected void TestStringRegistryItem(StringRegistryItem item, string expectedName, string expectedCategory, string expectedCaption, string expectedHint, RegistryStorageFlags expectedStorage, TextEditorType expectedTextEditorType, RegistryOptions expectedOptions, string expectedDefaultValue, CharacterCase expectedCharacterCase, string testValueToSetAndRead = "This is the new value.")
		{
			TestRegistryItem(item, expectedName, expectedCategory, expectedCaption, expectedHint, expectedStorage, expectedOptions, expectedTextEditorType, expectedDefaultValue, testValueToSetAndRead);
			AssertEquals(expectedCharacterCase, ((StringRegistryDataType)item.DataType).CharacterCase);
		}

		#endregion

		#region StringArrayRegistryItem

		protected void TestRegistryItem(StringArrayRegistryItem item, string expectedName, string expectedCategory, string expectedCaption, string expectedHint, RegistryStorageFlags expectedStorage)
		{
			TestRegistryItem(item, expectedName, expectedCategory, expectedCaption, expectedHint, expectedStorage, RegistryOptions.Default);
		}

		protected void TestRegistryItem(StringArrayRegistryItem item, string expectedName, string expectedCategory, string expectedCaption, string expectedHint, RegistryStorageFlags expectedStorage, RegistryOptions expectedOptions)
		{
			TestRegistryItem(item, expectedName, expectedCategory, expectedCaption, expectedHint, expectedStorage, expectedOptions, Enumerable.Empty<string>());
		}

		protected void TestRegistryItem(StringArrayRegistryItem item, string expectedName, string expectedCategory, string expectedCaption, string expectedHint, RegistryStorageFlags expectedStorage, IEnumerable<string> expectedDefaultValue)
		{
			TestRegistryItem(item, expectedName, expectedCategory, expectedCaption, expectedHint, expectedStorage, RegistryOptions.Default, expectedDefaultValue);
		}
		protected void TestRegistryItem(StringArrayRegistryItem item, string expectedName, string expectedCategory, string expectedCaption, string expectedHint, RegistryStorageFlags expectedStorage, RegistryOptions expectedOptions, IEnumerable<string> expectedDefaultValue, Type expectedEditorInfoType = null)
		{
			TestGenericRegistryItem(item, expectedName, expectedCategory, expectedCaption, expectedHint, expectedStorage, expectedOptions);
			AssertEquals("EditorInfo", expectedEditorInfoType, item.EditorInfo?.GetType());
			AssertContainsExactElementsInAnyOrder(expectedDefaultValue, item.DefaultValue);

			var newValue = new[] { "string1", "string2" };
			SetValue(item, newValue);
			AssertArrayEquals(item.Value, newValue, "Set value");
		}

		#endregion

		#region MultilingualStringRegistryItem

		protected void TestMultilingualRegistryItem(MultilingualStringRegistryItem item, string expectedName, string expectedCategory, string expectedCaption, string expectedHint, RegistryStorageFlags expectedStorage, TextEditorType expectedTextEditorType, string expectedDefaultValue)
		{
			TestMultilingualRegistryItem(item, expectedName, expectedCategory, expectedCaption, expectedHint, expectedStorage, RegistryOptions.Default, expectedTextEditorType, expectedDefaultValue);
		}

		protected void TestMultilingualRegistryItem(MultilingualStringRegistryItem item, string expectedName, string expectedCategory, string expectedCaption, string expectedHint, RegistryStorageFlags expectedStorage, TextEditorType expectedTextEditorType, RegistryOptions expectedOptions, string expectedDefaultValue)
		{
			TestMultilingualRegistryItem(item, expectedName, expectedCategory, expectedCaption, expectedHint, expectedStorage, expectedOptions, expectedTextEditorType, expectedDefaultValue);
		}

		protected void TestMultilingualRegistryItem(MultilingualStringRegistryItem item, string expectedName, string expectedCategory, string expectedCaption, string expectedHint, RegistryStorageFlags expectedStorage, RegistryOptions expectedOptions, TextEditorType expectedTextEditorType, string expectedDefaultValue, string testValueToSetAndRead = "This is the new value.")
		{
			TestGenericRegistryItem(item, expectedName, expectedCategory, expectedCaption, expectedHint, expectedStorage, expectedOptions, expectedDefaultValue);
			AssertEquals("EditorInfo.EditorType", expectedTextEditorType, ((TextRegistryEditorInfo)item.EditorInfo).EditorType);
			SetValue(item, testValueToSetAndRead);
			if ((expectedOptions & RegistryOptions.CannotCallParameterlessValueGetter) == 0)
			{
				AssertEquals("Value", testValueToSetAndRead, item.Value);
			}
		}

		protected void TestMultilingualStringRegistryItem(MultilingualStringRegistryItem item, string expectedName, string expectedCategory, string expectedCaption, string expectedHint, RegistryStorageFlags expectedStorage, TextEditorType expectedTextEditorType, RegistryOptions expectedOptions, string expectedDefaultValue, CharacterCase expectedCharacterCase)
		{
			TestMultilingualRegistryItem(item, expectedName, expectedCategory, expectedCaption, expectedHint, expectedStorage, expectedOptions, expectedTextEditorType, expectedDefaultValue);
			AssertEquals(expectedCharacterCase, ((StringRegistryDataType)item.DataType).CharacterCase);
		}

		protected void TestRegistryItem(MultilingualStringRegistryItem item, string expectedName, string expectedCategory, string expectedCaption, string expectedHint, RegistryStorageFlags expectedStorage, TextEditorType expectedTextEditorType, string expectedDefaultValue)
		{
			TestRegistryItem(item, expectedName, expectedCategory, expectedCaption, expectedHint, expectedStorage, RegistryOptions.Default, expectedTextEditorType, expectedDefaultValue);
		}

		protected void TestRegistryItem(MultilingualStringRegistryItem item, string expectedName, string expectedCategory, string expectedCaption, string expectedHint, RegistryStorageFlags expectedStorage, TextEditorType expectedTextEditorType, RegistryOptions expectedOptions, string expectedDefaultValue)
		{
			TestRegistryItem(item, expectedName, expectedCategory, expectedCaption, expectedHint, expectedStorage, expectedOptions, expectedTextEditorType, expectedDefaultValue);
		}

		protected void TestRegistryItem(MultilingualStringRegistryItem item, string expectedName, string expectedCategory, string expectedCaption, string expectedHint, RegistryStorageFlags expectedStorage, RegistryOptions expectedOptions, TextEditorType expectedTextEditorType, string expectedDefaultValue, string testValueToSetAndRead = "This is the new value.")
		{
			TestGenericRegistryItem(item, expectedName, expectedCategory, expectedCaption, expectedHint, expectedStorage, expectedOptions, expectedDefaultValue);
			AssertEquals("EditorInfo.EditorType", expectedTextEditorType, ((TextRegistryEditorInfo)item.EditorInfo).EditorType);
			AssertEquals("Should be TextRegistryEditorInfo", typeof(TextRegistryEditorInfo), item.EditorInfo.GetType());
			SetValue(item, testValueToSetAndRead);
			if ((expectedOptions & RegistryOptions.CannotCallParameterlessValueGetter) == 0)
			{
				AssertEquals("Value", testValueToSetAndRead, item.Value);
			}
		}

		protected void TestStringRegistryItem(MultilingualStringRegistryItem item, string expectedName, string expectedCategory, string expectedCaption, string expectedHint, RegistryStorageFlags expectedStorage, TextEditorType expectedTextEditorType, RegistryOptions expectedOptions, string expectedDefaultValue, CharacterCase expectedCharacterCase, string testValueToSetAndRead = "This is the new value.")
		{
			TestRegistryItem(item, expectedName, expectedCategory, expectedCaption, expectedHint, expectedStorage, expectedOptions, expectedTextEditorType, expectedDefaultValue, testValueToSetAndRead);
			AssertEquals(expectedCharacterCase, ((StringRegistryDataType)item.DataType).CharacterCase);
		}

		#endregion

		#region CodeDescriptionBoolRegistryItem

		protected void TestRegistryItem(CodeDescriptionBoolRegistryItem item, string expectedName, string expectedCategory, string expectedCaption, string expectedHint, RegistryStorageFlags expectedStorage)
		{
			AssertEquals("Name", expectedName, item.Name);
			AssertEquals("Category", expectedCategory, item.Category);
			AssertEquals("Caption", expectedCaption, item.Caption);
			AssertEquals("Hint", expectedHint, item.Hint);
			AssertEquals("Flgs", expectedStorage, item.Storage);
		}

		protected void TestRegistryItem(CodeDescriptionBoolRegistryItem item, string expectedName, string expectedCategory, string expectedCaption, string expectedHint, RegistryStorageFlags expectedStorage, RegistryOptions expectedOptions)
		{
			TestRegistryItem(item, expectedName, expectedCategory, expectedCaption, expectedHint, expectedStorage);
			AssertEquals("Options", expectedOptions, item.Options);
		}

		#endregion

		#region BillCustomisationRegistryItem

		protected void TestRegistryItem(BillCustomisationRegistryItem item, string expectedName, string expectedCategory, string expectedCaption, string expectedHint, RegistryStorageFlags expectedStorage, Action<BillCustomisationRegistryDataType> additionalAssertions)
		{
			TestRegistryItem(item, expectedName, expectedCategory, expectedCaption, expectedHint, expectedStorage, RegistryOptions.Default, additionalAssertions);
		}

		protected void TestRegistryItem(BillCustomisationRegistryItem item, string expectedName, string expectedCategory, string expectedCaption, string expectedHint, RegistryStorageFlags expectedStorage, RegistryOptions expectedOptions, Action<BillCustomisationRegistryDataType> additionalAssertions)
		{
			TestGenericRegistryItem(item, expectedName, expectedCategory, expectedCaption, expectedHint, expectedStorage, expectedOptions);
			var defaultValue = item.DataType as BillCustomisationRegistryDataType;
			additionalAssertions(defaultValue);
		}

		#endregion

		#region DateTimeRegistryItem

		protected void TestRegistryItem(DateTimeRegistryItem item, string expectedName, string expectedCategory, string expectedCaption, string expectedHint, RegistryStorageFlags expectedStorage, RegistryOptions expectedOptions, DateTime expectedDefaultValue)
		{
			var testDateTime = ZDateTime.UtcNow.ToDateTime();
			TestGenericRegistryItem(item, expectedName, expectedCategory, expectedCaption, expectedHint, expectedStorage, expectedOptions, expectedDefaultValue);
			SetValue(item, testDateTime);
			AssertEquals("Value", testDateTime, item.Value);
		}

		protected void TestRegistryItem(DateTimeRegistryItem item, string expectedName, string expectedCategory, string expectedCaption, string expectedHint, ZDateTimePickerFormat expectedDateTimePickerFormat, RegistryStorageFlags expectedStorage, RegistryOptions expectedOptions, DateTime expectedDefaultValue)
		{
			TestRegistryItem(item, expectedName, expectedCategory, expectedCaption, expectedHint, expectedStorage, expectedOptions, expectedDefaultValue);
			AssertEquals("EditorInfo.EditorType", expectedDateTimePickerFormat, ((DateTimeRegistryEditorInfo)item.EditorInfo).DateTimeFormat);
		}

		#endregion

		#region SystemToSystemTrustRegistryItem

		protected void TestRegistryItem(SystemToSystemTrustRegistryItem item, string expectedName, string expectedCategory, string expectedCaption, string expectedHint, RegistryStorageFlags expectedStorage, RegistryOptions expectedOptions)
		{
			AssertEquals("Name", expectedName, item.Name);
			AssertEquals("Category", expectedCategory, item.Category);
			AssertEquals("Caption", expectedCaption, item.Caption);
			AssertEquals("Hint", expectedHint, item.Hint);
			AssertEquals("Flgs", expectedStorage, item.Storage);
			AssertEquals("Opts", expectedOptions, item.Options);
		}

		#endregion

		#region Implementation

		protected BusinessObjectFactory Factory
		{
			get
			{
				if (factory == null)
				{
					factory = new BusinessObjectFactory();
				}
				return factory;
			}
		}

		void AssertArrayEquals(Array actualArray, Array expectedArray, string name)
		{
			AssertEquals(name + ".Length", expectedArray.Length, actualArray.Length);
			for (int i = 0; i < expectedArray.Length; i++)
			{
				AssertEquals(name + "[" + i + "]", expectedArray.GetValue(i), actualArray.GetValue(i));
			}
		}

		void SetValue(IRegistryItem item, object value)
		{
			RegistryTester.SetValue(item, value);
		}

		BusinessObjectFactory factory;

		#endregion

		#region Assertions

		protected void AssertItemDefaultValueAndNewValue<U>(string itemName, IRegistryItem item, U expectedDefaultValue, U newValue)
		{
			AssertItemDefaultValueAndNewValue(itemName, item, expectedDefaultValue, newValue, Guid.Empty, Guid.Empty, Guid.Empty);
		}

		protected void AssertItemDefaultValueAndNewValue<U>(string itemName, IRegistryItem item, U expectedDefaultValue, U newValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			AssertEquals("Default value for " + itemName, expectedDefaultValue, item.DefaultValue);
			SetAndAssertItemValue(itemName, item, newValue, companyPK, branchPK, departmentPK);
		}

		protected void SetAndAssertItemValue<U>(string itemName, IRegistryItem item, U newValue)
		{
			SetAndAssertItemValue(itemName, item, newValue, Guid.Empty, Guid.Empty, Guid.Empty);
		}

		protected void SetAndAssertItemValue<U>(string itemName, IRegistryItem item, U newValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			item.SetValue(companyPK, branchPK, departmentPK, newValue);
			AssertEquals(itemName, newValue, item.Value);
		}

		#endregion
	}
}
