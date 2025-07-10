using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	public class DummyFilterStripBusinessObject : FilterStripBusinessObject
	{
		public DummyFilterStripBusinessObject()
		{
			((IFilterStripBusinessObjectInternals)this).LayoutContext = LayoutsTestDataHelper.TestModuleID;
		}

		public DummyFilterStripBusinessObject(BusinessObjectFactory factory)
			: base(factory)
		{
			((IFilterStripBusinessObjectInternals)this).LayoutContext = LayoutsTestDataHelper.TestModuleID;
		}

		public const string FilterThatOverridesAllOthers = "Filter that overrides all other filters";

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();
			foreach (var filter in UserAddedModuleFilters)
			{
				result.AddCustomFilter(filter);
			}

			return result;
		}

		public void AddModuleFilterForTest(ModuleFilter filter)
		{
			UserAddedModuleFilters.Add(filter);

			// so that next access will get module filters again
			typeof(FilterStripBusinessObject).GetField("fModuleFilters", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(this, null);
		}

		public virtual void AddUserSavedModuleFilterForTest(StmModuleFilter filter)
		{
			Layouts.Add(filter);

			// so that next access will get module filters again
			typeof(FilterStripBusinessObject).GetField("fModuleFilters", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(this, null);
		}

		public override ModuleFilterCollection ModuleFilters
		{
			get
			{
				if (QueryObjectType == null)
				{
					SetQueryObjectTypeFromAdditionalHelpers();
				}

				return base.ModuleFilters;
			}
		}

		void SetQueryObjectTypeFromAdditionalHelpers()
		{
			var firstAdditionalHelper = GetCustomFilterStripsHelpersCore().FirstOrDefault(x => x.BusinessObjectType != null);

			if (firstAdditionalHelper != null)
			{
				QueryObjectType = firstAdditionalHelper.BusinessObjectType;
			}
		}

		#region LayoutsHelper

		public void SetLayoutsToNull()
		{
			// so that next access will get user filters again
			GetType().BaseType.GetField("fLayouts", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(this, null);
		}

		public DummyFilterStripLayoutsHelper LayoutsHelperAsDummy
		{
			get { return (DummyFilterStripLayoutsHelper)LayoutsHelper; }
			set { LayoutsHelper = value; }
		}

		protected override FilterStripLayoutsHelper GetNewLayoutsHelper()
		{
			var result = new DummyFilterStripLayoutsHelper();
			result.CurrentUserPkForTest = EnvProxy.Instance.CurrentUser.PK;
			result.CurrentUserTablePrefixForTest = GlbStaffSchema.Constants.Prefix;

			return result;
		}

		#endregion

		protected override ModuleFilter GetModuleFilterThatOverridesAllOtherFiltersCore()
		{
			return new ModuleTextFilter(FilterThatOverridesAllOthers, DummyBizoSchema.Z0_Code);
		}

		readonly List<ModuleFilter> UserAddedModuleFilters = new List<ModuleFilter>();

		public new void AddDateTimeRange(ZQuery query, DateComparisonOperator comparisonOperator, JoinCondition joinCondition, SchemaDateTimeColumn column, ZDateTime lowerEmptyOk, ZDateTime upperEmptyOk, bool convertFromLocalToUtc = false)
		{
			base.AddDateTimeRange(query, comparisonOperator, joinCondition, column, lowerEmptyOk, upperEmptyOk, convertFromLocalToUtc);
		}

		protected override FilterVisibility IsSystemDefinedStatusFilterVisibility => IsSystemDefinedStatusFilterVisibilityOverride ?? base.IsSystemDefinedStatusFilterVisibility;
		public FilterVisibility? IsSystemDefinedStatusFilterVisibilityOverride;
	}
}
