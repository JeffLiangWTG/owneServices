using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(ModuleNkFilter))]
	public class ModuleNkFilterTest : ModuleFilterWithSelectedFiltersTestCase<ModuleNkFilter, ZString>
	{
		#region TestPropertyValidation

		public void TestPropertyValidation()
		{
			var errorText = "You cant hug your children with atomic arms!";
			Filter.PropertyValidation = null;
			Filter.Property = ZString.Empty;

			Filter.Validation.ValidateProperty();
			AssertNoError(Filter.PropertyInfo, errorText);

			Filter.PropertyValidation = delegate(ZPropertyInfo info)
			{
				if (info.Value.IsEmpty)
				{
					info.AddError(errorText);
				}
			};

			Filter.Validation.ValidateProperty();
			AssertHasError(Filter.PropertyInfo, errorText);
			AssertEquals(1, Filter.PropertyInfo.GetErrors().Count());
		}

		#endregion

		#region TestIsExpensiveQuery

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		#endregion

		#region TestModuleID

		public void TestModuleID()
		{
			var collection = new DummyBusinessObjectCollection(Factory);
			var filter1 = new ModuleNkFilter("moo", DummyBizoSchema.Z0_Code, ModuleIDs.Organisation, collection);
			var filter2 = new ModuleNkFilter("moo", SomeDelegate, ModuleIDs.Organisation, collection);

			AssertEquals(ModuleIDs.Organisation, filter1.ModuleId);
			AssertEquals(ModuleIDs.Organisation, filter2.ModuleId);
		}

		ZQuery SomeDelegate(ZString nK)
		{
			return null;
		}

		#endregion

		#region TestComparisonOperators

		public void TestDefaultComparisonOperatorIsExact()
		{
			AssertEquals(ModuleTextBaseFilter.ComparisonConstants.Exact, Filter.ComparisonOperator);
		}

		public void TestAllowedComparisonOperators()
		{
			var allowedComparisonOperators = Filter.AllowedComparisonOperators;
			AssertEquals(6, allowedComparisonOperators.Count);
			AssertCollectionContains(ModuleTextBaseFilter.ComparisonConstants.Exact, allowedComparisonOperators);
			AssertCollectionContains(ModuleTextBaseFilter.ComparisonConstants.NotEqual, allowedComparisonOperators);
			AssertCollectionContains(ModuleTextBaseFilter.ComparisonConstants.IsBlank, allowedComparisonOperators);
			AssertCollectionContains(ModuleTextBaseFilter.ComparisonConstants.IsNotBlank, allowedComparisonOperators);
			AssertCollectionContains(ModuleTextBaseFilter.ComparisonConstants.FiltersMatch, allowedComparisonOperators);
		}

		#endregion

		#region TestCurrentUserComparisonOperator

		public void TestNonStaffFilter_ShouldNotHaveCurrentUserOperator()
		{
			Assert("CurrentUser operator should not be allowed when the ModuleId is not GlbStaff, but it was.", !Filter.AllowedComparisonOperators.Contains(CurrentUserComparisonOperator));
		}

		public void TestStaffFilter_ShouldHaveCurrentUserOperator()
		{
			Assert("CurrentUser operator should be allowed when the ModuleId is GlbStaff, but it wasn't.", GlbStaffFilter.AllowedComparisonOperators.Contains(CurrentUserComparisonOperator));
		}

		public void TestStaffFilter_ShouldSetCorrectPropertyCode()
		{
			SetFilterToCurrentUser();
			AssertEquals("Filter code should match the current user's code, but it didn't.", Env.CurrentUser.Initials, GlbStaffFilter.Property);
		}

		public void TestStaffFilter_ShouldHaveReadOnlyProperty()
		{
			GlbStaffFilter.Property = "A";
			AssertEquals("The default comparison operator is used, so the property should be able to be set, but it wasn't.", "A", GlbStaffFilter.Property);
			GlbStaffFilter.Property = "B";
			AssertEquals("The default comparison operator is used, so the property should be able to be set, but it wasn't.", "B", GlbStaffFilter.Property);

			SetFilterToCurrentUser();
			var property = GlbStaffFilter.Property;
			GlbStaffFilter.Property = "This won't be set.";
			AssertEquals("Property is read-only when in current user mode, so the property value should not change, but it did.", property, GlbStaffFilter.Property);
			Assert("Property is read-only when in current user mode, so the Property_ReadOnly property should be true and it wasn't.", GlbStaffFilter.IsPropertyReadOnly());
		}

		public void TestStaffFilter_ShouldUseCurrentUserWhenSavedLayoutLoaded()
		{
			var stripBizo = new DummyFilterStripBusinessObject();
			stripBizo.AddModuleFilterForTest(GlbStaffFilter);
			var strip = stripBizo.FilterStrips.AddNew(GlbStaffFilter.Description);

			var filter = (ModuleNkFilter)stripBizo[GlbStaffFilter.Description];
			SetFilterToCurrentUser(GlbStaffFilter);
			var oldUserCode = Env.CurrentUser.Initials;
			AssertEquals("Filter's property should be set to the logged in user's code.", oldUserCode, filter.Property);

			var savedFilter = stripBizo.SaveLayout("savedFilter", true);
			AssertNotNull("Filter layout was not saved successfully", savedFilter);

			using (CurrentUserChanger.SwitchToNewUserTemporarily(User.PostMasterUserName))
			{
				var newUserCode = Env.CurrentUser.Initials;
				AssertNotEquals("If a new user has been logged in, the user code should be different.", newUserCode, oldUserCode);

				strip.Delete();
				stripBizo.LoadLayout(savedFilter, false);
				var loadedFilter = (ModuleNkFilter)stripBizo[GlbStaffFilter.Description];
				AssertEquals("Loaded layout should have loaded the filter with the current user comparison operator, but it didn't.", CurrentUserComparisonOperator, loadedFilter.ComparisonOperator);
				AssertEquals("Loaded layout should have loaded the filter with its property set to the new logged in user's code, but it wasn't.", newUserCode, loadedFilter.Property);
			}
		}

		public void TestStaffFilter_ShouldCreateCorrectQuery()
		{
			var bizO1 = Factory.NewWithValidTestData<DummyBusinessObject>();
			bizO1.Z0_Code = Env.CurrentUser.Initials;
			bizO1.Z0_NVarCharMax = ZGuid.NewZGuid().ToString();
			var bizO2 = Factory.NewWithValidTestData<DummyBusinessObject>();
			bizO2.Z0_Code = bizO1.Z0_Code == "ZZZ" ? "YYY" : "ZZZ";
			bizO2.Z0_NVarCharMax = bizO1.Z0_NVarCharMax;

			var varcharFilter = new ModuleTextFilter("String data", DummyBizoSchema.Z0_NVarCharMax) { Property = bizO1.Z0_NVarCharMax, IsActive = true };
			var filterBiz0 = new DummyFilterStripBusinessObject();
			filterBiz0.AddModuleFilterForTest(GlbStaffFilter);
			filterBiz0.AddModuleFilterForTest(varcharFilter);
			GlbStaffFilter.IsActive = true;

			SetFilterToCurrentUser();
			var result = Factory.Load<DummyBusinessObject>(filterBiz0.Filter);
			AssertEquals("Filter query should have returned the one and only business object that was saved by the current user, but didn't.", 1, result.Length);
			Assert("Filter should have returned the correct business object only, but it didn't.", result.All(x => x.PK == bizO1.PK));

			using (CurrentUserChanger.SwitchToNewUserTemporarily(User.PostMasterUserName))
			{
				AssertNotEquals("The PostMaster's code cannot be the random code that I chose for the second business object, otherwise this test sort of falls apart.", bizO2.Z0_Code, Env.CurrentUser.Initials);
				GlbStaffFilter.CachedQuery_ForTest = null; // force a reevaluation of the query... it's okay becuase this kind of artificial user-swapping isn't really possible when using the product.
				result = Factory.Load<DummyBusinessObject>(filterBiz0.Filter);
				AssertEquals("The business object was saved by a different user, so the filter should not find any, but it did.", 0, result.Length);
			}
		}

		#endregion

		#region TestFiltersMatchComparisonOperator

		public void TestFiltersMatch_WhenDelegateFiltersMatch_ThenFilterSupportsFiltersMatch()
		{
			ModuleNkFilter filter = new ModuleNkFilter("description", DummyFiltersMatchQuery, ModuleIDs.SalesEnquiry, new StmNoteNonDependentCollection(Factory));
			AssertEquals("When provided with a filters match subquery, the filter is expected to support the filters match comparison operator.", true, filter.SupportsFiltersMatchComparisonOperator);
		}

		ZQuery DummyFiltersMatchQuery(ZDBOnlySubQuery filtersMatchQuery, SQLComparisonOperator comparisonOperator, ZString nK)
		{
			return new ZQuery();
		}

		#endregion

		#region Implementation

		protected override ModuleNkFilter GetNewModuleFilter()
		{
			var list = new StmNoteNonDependentCollection(Factory);
			return new ModuleNkFilter("moo", DummyBizoSchema.Z0_Code, ModuleIDs.SalesEnquiry, list);
		}

		GlbStaffFilterForTest GetNewGlbStaffModuleFilter()
		{
			var list = new StmNoteNonDependentCollection(Factory);
			return new GlbStaffFilterForTest("moo", DummyBizoSchema.Z0_Code, ModuleIDs.GlbStaff, list);
		}

		GlbStaffFilterForTest GlbStaffFilter
		{
			get { return glbStaffFilter ?? (glbStaffFilter = GetNewGlbStaffModuleFilter()); }
		}
		GlbStaffFilterForTest glbStaffFilter;

		static void SetFilterToCurrentUser(ModuleNkFilter filter)
		{
			filter.ComparisonOperator = CurrentUserComparisonOperator;
		}

		void SetFilterToCurrentUser()
		{
			SetFilterToCurrentUser(GlbStaffFilter);
		}

		IEnvironment Env
		{
			get { return EnvProxy.Instance; }
		}

		const string CurrentUserComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.CurrentUser;

		public class GlbStaffFilterForTest : ModuleNkFilter
		{
			public GlbStaffFilterForTest(ZString description, SchemaStringColumn nkFilterColumn, ModuleIdentifier iD, IBusinessObjectCollection list)
				: base(description, nkFilterColumn, iD, list)
			{
			}

			public bool IsPropertyReadOnly()
			{
				return Property_ReadOnly;
			}
		}

		protected override FilterCategory ExpectedDefaultCategory
		{
			get { return FilterCategories.Other; }
		}

		protected override string FilterDescriptionForFiltersMatchTest => "DummyNk";

		#endregion
	}
}
