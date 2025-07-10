using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ResString = Enterprise.ZArchitecture.GUI.ResString;

namespace Enterprise.ZArchitecture.Business.Internal.Testing
{
	[TestedType(typeof(FilterStrip))]
	public class FilterStripTest : NonPersistentBusinessObjectTestCase
	{
		#region event fire

		public void TestFilterDescriptionHooksupEvent()
		{
			var filter = new ModuleNumberRangeFilter("descdesc", delegate { return new ZQuery(); });
			ModuleFilters.AddCustomFilter(filter);
			Strip.ModuleFilterChanged += new EventHandler(Strip_ModuleFilterChanged);
			Strip.FilterDescription = filter.Description;
			filter.Decimals = 4;
			AssertEquals(true, eventFired);
		}

		void Strip_ModuleFilterChanged(object sender, EventArgs e)
		{
			eventFired = true;
		}

		bool eventFired;

		#endregion

		#region TestCurrentModuleFilter

		public void TestCurrentModuleFilter()
		{
			Strip.FilterDescription = TextFilter.Description;
			AssertEquals(TextFilter, Strip.CurrentModuleFilter);

			Strip.FilterDescription = TextFilterAlwaysApplied.Description;
			AssertEquals(TextFilterAlwaysApplied, Strip.CurrentModuleFilter);

			Strip.FilterDescription = TextFilterAlwaysAppliedAndHidden.Description;
			AssertNull("ModuleFilter.Visibility = AlwaysAppliedAndHidden, FilterStrip.CurrentModuleFilter should be null.", Strip.CurrentModuleFilter);
		}

		#endregion

		#region TestAlwaysVisibleFiltersHaveReadOnlyDescriptions

		public void TestAlwaysVisibleFiltersHaveReadOnlyDescriptions()
		{
			Strip.FilterDescription = TextFilter.Description;
			AssertEquals("Non AlwaysVisible filter shold not be readonly", false, Strip.FilterDescriptionInfo.ReadOnly);

			Strip.FilterDescription = TextFilterAlwaysVisible.Description;
			AssertEquals("AlwaysVisible filter should be readonly", true, Strip.FilterDescriptionInfo.ReadOnly);
		}

		#endregion

		#region TestFilterDescriptionListPropertyName

		public void TestFilterDescriptionListPropertyName()
		{
			var info = Strip.GetType().GetProperty(FilterStrip.Schema.FilterDescriptionList);
			AssertEquals(Strip.FilterDescriptionList, (CodeDescriptionPairList)info.GetValue(Strip, null));
		}

		#endregion

		#region TestIsTextFilterWithList

		public void TestIsTextFilterWithList()
		{
			Strip.FilterDescription = TextFilter.Description;
			AssertEquals(false, Strip.IsTextFilterWithList);

			Strip.FilterDescription = TextFilterWithList.Description;
			AssertEquals(true, Strip.IsTextFilterWithList);
		}

		#endregion

		#region TestDefaultOrCategoryIsNone

		public void TestDefaultOrCategoryIsNone()
		{
			AssertEquals(FilterOrCategory.None, Strip.OrCategory);
		}

		#endregion

		#region TestSettingOrCategoryFiresChangeEvent

		public void TestSettingOrCategoryFiresChangeEvent()
		{
			Strip.OrCategoryChanged += delegate
			{
				Assert(true);
			};

			Strip.OrCategory = FilterOrCategory.Red;
		}

		#endregion

		#region TestSettingGroupName

		public void TestSettingGroupName()
		{
			Strip.GroupName = "New Group";
			AssertEquals("", TextFilter.GroupName);

			Strip.FilterDescription = "Text Filter";
			Strip.GroupName = "New Group";
			AssertEquals("New Group", TextFilter.GroupName);
		}

		#endregion

		#region TestSettingGroupOrCategory

		public void TestSettingGroupOrCategory()
		{
			Strip.GroupOrCategory = FilterOrCategory.Cyan;
			AssertEquals(FilterOrCategory.None, TextFilter.GroupOrCategory);

			Strip.FilterDescription = "Text Filter";
			Strip.GroupOrCategory = FilterOrCategory.Grey;
			AssertEquals(FilterOrCategory.Grey, TextFilter.GroupOrCategory);
		}

		#endregion

		#region TestFilterDescription

		[ExpectNoExceptions]
		public void TestFilterDescriptionWhenModuleFiltersNullDoesNotBlowUp()
		{
			new FilterStrip(null).FilterDescription = "test";
			new FilterStrip(null).FilterDescriptionLocalized = "test";
		}

		public void TestFilterDescriptionWithEmptyString()
		{
			Strip.FilterDescription = "Text Filter";
			AssertEquals("Precondition", "Text Filter", Strip.FilterDescription);
			AssertEquals("Precondition", "Text Filter", Strip.FilterDescriptionLocalized);

			Strip.FilterDescription = "";
			AssertEquals("Text Filter", FilterStrip.SelectFilterDescriptionText, Strip.FilterDescription);
			AssertEquals("Text Filter", FilterStrip.SelectFilterDescriptionText, Strip.FilterDescriptionLocalized);
		}

		public void TestFilterDescriptionCreatesDuplicateModuleFilterWhenExistingDescriptionIsActive()
		{
			var strips = new FilterStripCollection(ModuleFilters);

			var strip1 = strips.AddNew("");
			var strip2 = strips.AddNew("");

			strip1.FilterDescription = "Text Filter";
			strip1.CurrentModuleFilter.IsActive = false;
			strip1.OrCategory = FilterOrCategory.Green;
			strip1.GroupOrCategory = FilterOrCategory.Green;
			strip1.GroupName = "abc";

			strip2.FilterDescription = "Text Filter";
			AssertEquals("Filter was not active and should *not* have been duplicated", strip1.CurrentModuleFilter, strip2.CurrentModuleFilter);

			strip2.FilterDescription = "";
			strip1.CurrentModuleFilter.IsActive = true;
			strip2.OrCategory = FilterOrCategory.Red;
			strip2.GroupOrCategory = FilterOrCategory.Red;
			strip2.GroupName = "abc";
			strip2.FilterDescription = "Text Filter";

			AssertNotEquals("Filter is already active and should have been duplicated", strip1.CurrentModuleFilter, strip2.CurrentModuleFilter);
			AssertEquals("Duplicated filter should auto-set the OR category.", FilterOrCategory.Green, strip2.OrCategory);
			AssertEquals("Duplicated filter should auto-set the OR category.", FilterOrCategory.Green, strip2.CurrentModuleFilter.OrCategory);
			AssertEquals("Duplicated filter should auto-set the Group OR category.", FilterOrCategory.Green, strip2.GroupOrCategory);
		}

		public void TestFilterDescriptionRetrievesDuplicateFiltersWhenSpecified()
		{
			ModuleFilter duplicate = new ModuleTextFilter("Text Filter", DummyBizoSchema.Z0_Description);
			duplicate.IsDuplicateDefault = true;
			var clonedDuplicate = ModuleFilters.AddNewDuplicateFilter(duplicate);

			var strips = new FilterStripCollection(ModuleFilters);

			var strip1 = strips.AddNew("");
			var strip2 = strips.AddNew("");

			strip1.IncludeDuplicatesWhenGettingModuleFilter = true;
			strip1.FilterDescription = "Text Filter (1)";
			strip2.FilterDescription = "Text Filter (1)";

			AssertEquals("Strip 1 was searching defaults and should have found the filter", strip1.CurrentModuleFilter, clonedDuplicate);
			AssertNotEquals("Strip 2 was not searching defaults and should not have found the filter", strip2.CurrentModuleFilter, clonedDuplicate);
		}

		public void TestFilterDescriptionChangesWithoutChangingModuleFilters()
		{
			var strips = new FilterStripCollection(ModuleFilters);

			var strip1 = strips.AddNew("");

			strip1.FilterDescription = "Text Filter AlwaysApplied";
			var desiredFilter = strip1.CurrentModuleFilter;

			using (strip1.SuspendTriggeringChangeOfModuleFilterOnSettingDescription())
			{
				strip1.FilterDescription = "Text Filter";
			}

			AssertEquals("Filter Strip Description should be overriden", "Text Filter", strip1.FilterDescription);
			AssertEquals("Filter Strip Module should match original", desiredFilter, strip1.CurrentModuleFilter);
			AssertEquals("Filter Strip Module should be original type", "Text Filter AlwaysApplied", strip1.CurrentModuleFilter.Description);
		}

		public void TestFilterDescription_SetStripAndOrGroups_ToSameValuesInSameGroup()
		{
			var strips = new FilterStripCollection(ModuleFilters);

			var strip1 = strips.AddNew("");
			strip1.GroupName = "111";
			strip1.FilterDescription = "Text Filter";
			strip1.CurrentModuleFilter.IsActive = true;
			strip1.OrCategory = FilterOrCategory.Red;
			strip1.GroupOrCategory = FilterOrCategory.Red;

			var strip2 = strips.AddNew("");
			strip2.GroupName = "222";
			strip2.FilterDescription = "Text Filter";
			strip2.CurrentModuleFilter.IsActive = true;
			strip2.OrCategory = FilterOrCategory.Green;
			strip2.GroupOrCategory = FilterOrCategory.GreenYellow;

			var strip3 = strips.AddNew("");
			strip3.GroupName = "222";
			strip3.FilterDescription = "Text Filter";
			strip3.CurrentModuleFilter.IsActive = true;

			AssertEquals(FilterOrCategory.Green, strip3.OrCategory);
			AssertEquals(FilterOrCategory.Green, strip3.CurrentModuleFilter.OrCategory);
			AssertEquals(FilterOrCategory.GreenYellow, strip3.GroupOrCategory);
			AssertEquals(FilterOrCategory.GreenYellow, strip3.CurrentModuleFilter.GroupOrCategory);
		}

		public void TestFilterDescription_DoesNotSetGroupsToModuleFilterGroups_WhenExistingDescriptionIsInDifferentCategory()
		{
			var strips = new FilterStripCollection(ModuleFilters);

			var strip1 = strips.AddNew("");
			strip1.FilterDescription = "Text Filter";
			strip1.CurrentModuleFilter.IsActive = true;
			strip1.GroupName = "abc";
			strip1.OrCategory = FilterOrCategory.Green;
			strip1.GroupOrCategory = FilterOrCategory.Green;

			var strip2 = strips.AddNew("");
			strip2.GroupName = "xyz";
			strip2.OrCategory = FilterOrCategory.Red;
			strip2.GroupOrCategory = FilterOrCategory.Red;
			strip2.FilterDescription = "Text Filter";

			var strip3 = strips.AddNew("");
			strip3.GroupName = "ABC";
			strip3.OrCategory = FilterOrCategory.Red;
			strip3.GroupOrCategory = FilterOrCategory.Red;
			strip3.FilterDescription = "Text Filter";

			AssertEquals("Duplicated filter should not auto-set the OR category.", FilterOrCategory.Red, strip2.OrCategory);
			AssertEquals("Duplicated filter should not auto-set the Group OR category.", FilterOrCategory.Red, strip2.GroupOrCategory);
			AssertEquals("Duplicated filter should not auto-set the OR category.", FilterOrCategory.Red, strip2.CurrentModuleFilter.OrCategory);
			AssertEquals("Duplicated filter should not auto-set the OR category.", FilterOrCategory.Red, strip3.OrCategory);
			AssertEquals("Duplicated filter should not auto-set the Group OR category.", FilterOrCategory.Red, strip3.GroupOrCategory);
			AssertEquals("Duplicated filter should not auto-set the OR category.", FilterOrCategory.Red, strip3.CurrentModuleFilter.OrCategory);
		}

		public void TestAddingNewFilter_CopyGroupOrCatefory_FromFirstFilterInGroup()
		{
			var strips = new FilterStripCollection(ModuleFilters);

			var strip1 = strips.AddNew("");
			strip1.FilterDescription = "Text Filter";
			strip1.CurrentModuleFilter.IsActive = true;
			strip1.GroupName = "abc";
			strip1.OrCategory = FilterOrCategory.Green;
			strip1.GroupOrCategory = FilterOrCategory.Green;

			var strip2 = strips.AddNew("");
			strip2.FilterDescription = "Text Filter with List";
			strip2.GroupName = "abc";
			strip2.CurrentModuleFilter.IsActive = true;

			AssertEquals("New Filter should auto-set the Group Or Category", FilterOrCategory.Green, strip2.GroupOrCategory);
			AssertEquals("New Filter should not auto-set the Or Category", FilterOrCategory.None, strip2.OrCategory);
		}

		public void TestModuleFilterOrCategoryTakesPrecedenceWhenReadOnly()
		{
			ModuleFilters["Text Filter"].OrCategory = FilterOrCategory.AliceBlue;
			ModuleFilters["Text Filter"].IsOrCategoryReadOnly = true;
			ModuleFilters["Text Filter"].GroupOrCategory = FilterOrCategory.DarkRed;
			ModuleFilters["Text Filter"].IsGroupOrCategoryReadOnly = false;

			ModuleFilters["Text Filter AlwaysApplied"].OrCategory = FilterOrCategory.MediumVioletRed;
			ModuleFilters["Text Filter AlwaysApplied"].IsOrCategoryReadOnly = false;
			ModuleFilters["Text Filter AlwaysApplied"].GroupOrCategory = FilterOrCategory.YellowGreen;
			ModuleFilters["Text Filter AlwaysApplied"].IsGroupOrCategoryReadOnly = true;

			var strips = new FilterStripCollection(ModuleFilters);
			var strip1 = strips.AddNew("");
			strip1.OrCategory = FilterOrCategory.BlanchedAlmond;
			strip1.FilterDescription = "Text Filter";

			var strip2 = strips.AddNew("");
			strip2.FilterDescription = "Text Filter AlwaysApplied";

			AssertEquals(FilterOrCategory.AliceBlue, ModuleFilters["Text Filter"].OrCategory);
			AssertEquals(FilterOrCategory.None, ModuleFilters["Text Filter"].GroupOrCategory);
			AssertEquals(FilterOrCategory.None, ModuleFilters["Text Filter AlwaysApplied"].OrCategory);
			AssertEquals(FilterOrCategory.YellowGreen, ModuleFilters["Text Filter AlwaysApplied"].GroupOrCategory);

			//when a ModuleFilter has readonly properties these properties 
			//should be reflected in the FilterStrip containing the ModuleFilter
			AssertEquals(FilterOrCategory.AliceBlue, strip1.OrCategory);
			AssertEquals(FilterOrCategory.None, strip1.GroupOrCategory);
			AssertEquals(FilterOrCategory.None, strip2.OrCategory);
			AssertEquals(FilterOrCategory.YellowGreen, strip2.GroupOrCategory);
		}

		public void TestFilterDescription_ShouldNotDisableValidStripWhenItsFilterDescriptionIsTheSameAsAnInvalidStripDescription()
		{
			var textStrip1 = new FilterStrip(ModuleFilters);
			textStrip1.FilterDescription = "Text Filter";

			var invalidStripWithConflictingDescription = new FilterStrip(ModuleFilters);
			invalidStripWithConflictingDescription.FilterDescription = "Text Filter (1)";

			var testStrip2 = new FilterStrip(ModuleFilters);
			testStrip2.FilterDescription = "Text Filter";

			AssertEquals("Precondition", "Text Filter (1)", testStrip2.CurrentModuleFilterDescription);

			invalidStripWithConflictingDescription.FilterDescription = "";
			AssertNotNull("Should not have disabled testStrip2", ModuleFilters[testStrip2.CurrentModuleFilterDescription]);
			AssertEquals("Should not have disabled testStrip2", false, ModuleFilters[testStrip2.CurrentModuleFilterDescription].IsDeleted);
		}

		public void TestFilterDescription_ShouldOnlyDuplicateModuleFilterIfStripDescriptionMatchesTheOriginalModuleFilterDescription()
		{
			var textStrip1 = new FilterStrip(ModuleFilters);
			textStrip1.FilterDescription = "Text Filter";

			var testStrip2 = new FilterStrip(ModuleFilters);
			testStrip2.FilterDescription = "Text Filter";

			AssertNotNull("Should have duplicated ModuleFilter as the strips FilterDescription matches TextFilter", testStrip2.CurrentModuleFilter);

			AssertEquals("Precondition", "Text Filter (1)", testStrip2.CurrentModuleFilterDescription);
			var invalidStripWithConflictingDescription = new FilterStrip(ModuleFilters);
			invalidStripWithConflictingDescription.FilterDescription = "Text Filter (1)";

			AssertNull("Should not have duplcated ModuleFilter as the strips FilterDescription does not match TextFilter", invalidStripWithConflictingDescription.CurrentModuleFilter);
		}

		#endregion

		#region TestFilterDescriptionLocalized

		public void TestFilterDescriptionLocalized()
		{
			using (var cache = Res.UseMockData())
			{
				var strip = new FilterStrip(ModuleFilters);

				var filterDescriptionLocalizedList = strip.FilterDescriptionLocalizedList;

				Assert("There should be some filters to have correct test", filterDescriptionLocalizedList.Count > 0);
				AssertEquals(strip.FilterDescriptionList.Count, filterDescriptionLocalizedList.Count);

				foreach (ICodeDescription codeDescription in strip.FilterDescriptionList)
				{
					var filter = ModuleFilters[codeDescription.Description];
					if (filter != null)
					{
						strip.FilterDescription = codeDescription.Description;
						AssertEquals(codeDescription.Description, strip.FilterDescriptionLocalized);
						AssertEquals(codeDescription.Description, filterDescriptionLocalizedList.GetDescriptionFromCode(codeDescription.Code));

						filter.MultilingualDescription = (NoResString)(filter.Description + "-ABC");
					}
				}

				filterDescriptionLocalizedList = strip.FilterDescriptionLocalizedList;

				AssertEquals(strip.FilterDescriptionList.Count, filterDescriptionLocalizedList.Count);

				foreach (ICodeDescription codeDescription in strip.FilterDescriptionList)
				{
					var filter = ModuleFilters[codeDescription.Description];
					if (filter != null)
					{
						strip.FilterDescription = codeDescription.Description;
						var localizedDescription = codeDescription.Description + "-ABC";
						AssertEquals(localizedDescription, strip.FilterDescriptionLocalized);
						AssertEquals(codeDescription.Description, filterDescriptionLocalizedList.GetDescriptionFromCode(localizedDescription));
						Assert("MultilingualDescription should be used directly as the localized list code", ReferenceEquals(filter.MultilingualDescription, ((CodeDescriptionPair)filterDescriptionLocalizedList[localizedDescription]).MultilingualCode));
					}
				}
			}
		}

		public void TestFilterDescriptionLocalized_InvalidStripWithNumberSuffix_WithMultilingualDescription()
		{
			TextFilter.MultilingualDescription = (NoResString)"Text Filter (Localized)";

			var textStrip1 = new FilterStrip(ModuleFilters);
			textStrip1.FilterDescription = "Text Filter";
			AssertEquals("Text Filter (Localized)", textStrip1.FilterDescriptionLocalized);

			var textStrip2 = new FilterStrip(ModuleFilters);
			textStrip2.FilterDescription = "Text Filter";
			AssertEquals("Text Filter (Localized)", textStrip2.FilterDescriptionLocalized);

			var invalidStripWithNumberSuffix = new FilterStrip(ModuleFilters);
			invalidStripWithNumberSuffix.FilterDescription = "Text Filter (1)";
			AssertEquals("Should not localize FilterDescription as its not a valid filter", "Text Filter (1)", invalidStripWithNumberSuffix.FilterDescriptionLocalized);
		}

		public void TestFilterDescriptionLocalized_InvalidStripWithNumberSuffix_WithNoMultilingualDescription()
		{
			TextFilter.MultilingualDescription = null;

			var textStrip1 = new FilterStrip(ModuleFilters);
			textStrip1.FilterDescription = "Text Filter";
			AssertEquals("Should be FilterDescription when no localized description available", "Text Filter", textStrip1.FilterDescriptionLocalized);

			var textStrip2 = new FilterStrip(ModuleFilters);
			textStrip2.FilterDescription = "Text Filter";
			AssertEquals("Should be FilterDescription when no localized description", "Text Filter", textStrip2.FilterDescriptionLocalized);

			var invalidStripWithNumberSuffix = new FilterStrip(ModuleFilters);
			invalidStripWithNumberSuffix.FilterDescription = "Text Filter (1)";
			AssertEquals("Should be FilterDescription when no localized description available", "Text Filter (1)", invalidStripWithNumberSuffix.FilterDescriptionLocalized);
		}

		public void TestSelectedFilterDoesNotMatchCategoryText()
		{
			using (Res.TemporarilySwitchLanguage(Enterprise.Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				var key = ((ResourceString)TextFilter.Category.Description).ResourceKey;
				mockRes.Put(key, new ResourceStringData(key, "文本搜索"));

				var strip = new FilterStrip(ModuleFilters);
				TextFilter.MultilingualDescription = TextFilter.Category.Description;
				strip.FilterDescriptionLocalized = "文本搜索";
				AssertEquals("Text Filter", strip.FilterDescription);
				strip.FilterDescriptionLocalized = "Text Filter";
				AssertEquals("Text Filter", strip.FilterDescription);
			}
		}

		#endregion

		#region TestUniqueIDReturnsTranslated

		public void TestUniqueIDReturnsTranslated()
		{
			TextFilter.MultilingualDescription = ResString.GetMultilingualString("Foo", "Grill");

			using (Res.TemporarilySwitchLanguage(Enterprise.Core.SharedConstants.Languages.French))
			using (var mock = Res.UseMockData())
			{
				mock.Put("Foo", new ResourceStringData("Foo", "Le Grill"));

				var strip = new FilterStrip(ModuleFilters);
				strip.FilterDescription = TextFilter.Description;

				AssertEquals("Le Grill", ((ILayoutDetailTreeNode)strip).UniqueID);
			}
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new FilterStrip(new ModuleFilterCollection());
		}

		protected override void SetUp()
		{
			base.SetUp();

			TextFilter = new ModuleTextFilter("Text Filter", DummyBizoSchema.Z0_Description);
			TextFilterAlwaysApplied = new ModuleTextFilter("Text Filter AlwaysApplied", DummyBizoSchema.Z0_Description);
			TextFilterAlwaysApplied.Visibility = FilterVisibility.AlwaysApplied;
			TextFilterAlwaysAppliedAndHidden = new ModuleTextFilter("Text Filter AlwaysAppliedAndHidden", DummyBizoSchema.Z0_Description);
			TextFilterAlwaysAppliedAndHidden.Visibility = FilterVisibility.AlwaysAppliedAndHidden;
			TextFilterAlwaysVisible = new ModuleTextFilter("Text Filter AlwaysVisible", DummyBizoSchema.Z0_Description);
			TextFilterAlwaysVisible.Visibility = FilterVisibility.AlwaysVisible;

			TextFilterWithList = new ModuleTextFilter("Text Filter with List", DummyBizoSchema.Z0_Description, new DummyBusinessObjectCollection(Factory));

			ModuleFilters = new ModuleFilterCollection();
			ModuleFilters.AddCustomFilter(TextFilter);
			ModuleFilters.AddCustomFilter(TextFilterAlwaysApplied);
			ModuleFilters.AddCustomFilter(TextFilterAlwaysAppliedAndHidden);
			ModuleFilters.AddCustomFilter(TextFilterAlwaysVisible);
			ModuleFilters.AddCustomFilter(TextFilterWithList);

			Strip = new FilterStrip(ModuleFilters);
		}

		FilterStrip Strip;
		ModuleFilterCollection ModuleFilters;
		ModuleTextFilter TextFilter;
		ModuleTextFilter TextFilterWithList;
		ModuleTextFilter TextFilterAlwaysApplied;
		ModuleTextFilter TextFilterAlwaysAppliedAndHidden;
		ModuleTextFilter TextFilterAlwaysVisible;

		#endregion
	}
}
