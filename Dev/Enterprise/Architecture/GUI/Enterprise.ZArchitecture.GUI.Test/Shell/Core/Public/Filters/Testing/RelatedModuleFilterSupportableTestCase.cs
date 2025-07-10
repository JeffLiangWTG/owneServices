using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	[TestsSubclassesOf(typeof(IRelatedModuleFilterSupportable), null, ExcludeClientDlls = false)]
	public abstract class RelatedModuleFilterSupportableTestCase<T> : TestCaseWithFactory
			where T : BusinessObject, IRelatedModuleFilterSupportable
	{
		public void TestChangeFilters_ShouldDeleteOldFilters()
		{
			var businessObject = GetNewBusinessObject();

			foreach (var testSet in GetFilterRules(businessObject))
			{
				var filter = testSet.FilterGetter();
				AssertEquals(false, filter.IsDeleted);

				filter.Delete();

				var newFilter = testSet.FilterGetter();

				AssertEquals(newFilter, testSet.FilterGetter());
				AssertEquals(false, newFilter.IsDeleted);
			}
		}

		public virtual void TestDeleteBusinessObject_ShouldDeleteFilters()
		{
			var initialFilterCount = Factory.GetDatabaseCount(typeof(StmModuleFilter));
			var businessObject = GetNewBusinessObject();
			var filters = GetFilterRules(businessObject).Select(x => x.FilterGetter()).ToArray();

			foreach (var filter in filters)
			{
				AssertEquals(false, filter.IsDeleted);
			}

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var loadedBusinessObject = newFactory.Load<T>(businessObject.PK);

			loadedBusinessObject.Delete();
			newFactory.Save();

			foreach (var filter in filters)
			{
				var filterInDatabase = newFactory.Load<StmModuleFilter>(filter.PK);
				AssertNull("Filters should be deleted along with the related business object, and yet...", filterInDatabase);
			}

			AssertEquals("The filter count should be the same as before the rules were created", initialFilterCount, Factory.GetDatabaseCount(typeof(StmModuleFilter)));
		}

		public void TestClone_ShouldCreateNewDuplicatedFilterStrips()
		{
			var businessObject = GetNewBusinessObject();

			if (!businessObject.SupportsClone())
			{
				Assert(true);
				return;
			}

			var filterRules = GetFilterRules(businessObject).ToArray();

			var cloneWithNoFilters = (T)businessObject.Clone();
			var cloneWithNoFiltersRules = GetFilterRules(cloneWithNoFilters).ToArray();

			foreach (var testSet in filterRules)
			{
				var clonedTestSet = cloneWithNoFiltersRules.Single(x => x.FilterName == testSet.FilterName);
				RelatedModuleFilterTestHelper.AssertModuleFilterDeepClone(testSet.FilterGetter(), clonedTestSet.FilterGetter(), cloneWithNoFilters);
			}

			foreach (var testSet in filterRules)
			{
				var filter = testSet.FilterGetter();
				filter.S9_FilterData = new ZBlob(new byte[] { 1, 2, 3, 4, 5 });
				var userData = filter.GetOrCreateLayoutUserData(new EmptyLayoutsHelper());
				userData.S0_FilterDataValues = new ZBlob(new byte[] { 1, 2, 3, 4, 5 });
			}

			var cloneWithFilters = (T)businessObject.Clone();
			var cloneWithFiltersRules = GetFilterRules(cloneWithFilters).ToArray();

			foreach (var testSet in filterRules)
			{
				var clonedTestSet = cloneWithFiltersRules.Single(x => x.FilterName == testSet.FilterName);
				RelatedModuleFilterTestHelper.AssertModuleFilterDeepClone(testSet.FilterGetter(), clonedTestSet.FilterGetter(), cloneWithFilters);
			}
		}

		public void TestHumanReadableName_ShouldBeOverridden()
		{
			var businessObject = GetNewBusinessObject();
			AssertNotEquals("HumanReadableName needs to be something meaningful as this identifier will be used to help users find object that have certain filters. See BMBoardSection.cs for a good example.", businessObject.GetType().Name, businessObject.HumanReadableName);
		}

		public void TestFilterBusinessObjects_HaveQueryObjectTypeSpecified()
		{
			var businessObject = GetNewBusinessObject();

			foreach (var filterSet in GetFilterRules(businessObject))
			{
				var filter = filterSet.FilterGetter();
				var filterBizo = (FilterStripBusinessObject)RelatedModuleFiltersHelper.GetNewFilterBusinessObject(filter);

				if (filterBizo.GetShouldAddUserDefinedFilters)
				{
					AssertNotNull("QueryObjectType needs to be set explicitly for FilterStripBusinessObjects that are used for filter rules, because they are created outside the context of a module and this property will not be otherwise set.", filterBizo.QueryObjectType);
				}
			}

			Assert(true);
		}

		public void TestFilterBusinessObjects_HaveLayoutContextSpecified()
		{
			var businessObject = GetNewBusinessObject();

			foreach (var filterSet in GetFilterRules(businessObject))
			{
				var filter = filterSet.FilterGetter();
				var filterBizo = (FilterStripBusinessObject)RelatedModuleFiltersHelper.GetNewFilterBusinessObject(filter);

				if (filterBizo.GetShouldAddUserDefinedFilters)
				{
					var layoutContext = ((IFilterStripBusinessObjectInternals)filterBizo).LayoutContext;
					AssertNotNullOrEmpty("LayoutContext should be set", layoutContext);
				}
			}

			Assert(true);
		}

		public void TestValidateBusinessObject_ShouldCheckForNestedUnpublishedUserDefinedFilters()
		{
			var businessObject = GetNewBusinessObject();
			FilterStripsTestHelper.SaveFilterLayout(ModuleIDs.GlbStaff, "Nonpublished", false, false, true);

			foreach (var filterSet in GetFilterRules(businessObject))
			{
				var layout = filterSet.FilterGetter();
				AssertNoErrors(layout);

				var moduleId = ModuleIDs.AllIncludingClientModules.First(x => x.Name == layout.S9_ModuleID);
				var filterBizo = (FilterStripBusinessObject)RelatedModuleFiltersHelper.GetNewFilterBusinessObject(moduleId);

				var filter = (IModuleFilterWithSelectedFilters)filterBizo.AddFilterStrip<ModuleFilter>(FilterDescriptionForValidationTest);
				filter = filter ?? filterBizo.AddNkFilterStrip("Staff");
				AssertNotNull("The module you're testing doesn't have a Creating User or Staff filter... SAD! Please override GetFilterDescriptionForValidationTest and provide a filter for GlbStaff that allows the 'filters match' type operators. If there is no filter for GlbStaff, you'll have to refactor this test to allow other modules.", filter);

				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch;
				filter.SelectedFilters.AddFilterStrip<ModuleUserDefinedFilter>("[USR]Nonpublished");
				filterBizo.WriteFilterStripsToXml(layout, filterBizo.FilterStrips, new EmptyLayoutsHelper());

				ValidateBusinessObject(businessObject);

				AssertHasRowError(layout, string.Format(CultureInfo.InvariantCulture, @"Filter has errors.
The selected filters have one or more errors.
The following non-published user-defined filter has been included in the selected filters of this filter strip:

{0} -> Nonpublished

Filter rules cannot contain non-published user-defined filters.", filter.MultilingualDescription));
			}
		}

		public void TestExpectedFilterStripBusinessObjectType_ShouldBeUsedForEverything()
		{
			var businessObject = GetNewBusinessObject();

			foreach (var filterSet in GetFilterRules(businessObject))
			{
				var moduleId = ModuleIDs.AllIncludingClientModules.First(x => x.Name == filterSet.FilterGetter().S9_ModuleID);
				var filterBizo = RelatedModuleFiltersHelper.GetNewFilterBusinessObject(moduleId);

				AssertEquals(filterSet.ExpectedTypeOfFilterStripBusinessObject, filterBizo.GetType().Name);
			}
		}

		protected virtual T GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData<T>();
		}

		protected abstract IEnumerable<FilterRuleTestSet> GetFilterRules(T businessObject);

		protected virtual string OverriddenFilterBizoObjectFactoryReference => null;

		protected abstract void ValidateBusinessObject(T businessObject);

		protected virtual string FilterDescriptionForValidationTest => "Creating User";
	}
}
