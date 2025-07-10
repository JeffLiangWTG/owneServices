using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Module
{
	public class BMFilterRuleFilterBusinessObject : ProcessHeaderFilterBusinessObject, IBMFilterRuleFilterBusinessObject
	{
		public BMFilterRuleFilterBusinessObject()
			: this(false)
		{
		}

		public BMFilterRuleFilterBusinessObject(bool isIndexFilter)
		{
			QueryObjectType = typeof(ProcessHeader);
			if (isIndexFilter)
			{
				SetGlowFiltersIfAllowed();
			}
			SearchType = isIndexFilter ? SearchType.Index : SearchType.Sql;
			((IFilterStripBusinessObjectInternals)this).LayoutContext = ModuleIDs.BMFilterRule.Name;
		}

		public string FilterControlIdentifier { get; set; }

		protected override void OnModuleFiltersReset()
		{
			base.OnModuleFiltersReset();
			AddAuditFiltersIfRequired();
		}

		protected override void AddAuditFiltersIfRequiredCore()
		{
			if (SearchType != SearchType.Index && !this.Any(f => f.FilterColumn == ProcessHeaderSchema.FH_SystemCreateTimeUtc))
			{
				ModuleAuditFilterProvider.AddAuditFilters(ModuleFilters, ProcessHeaderSchema.Instance, Factory, typeof(ProcessHeader));
			}
		}

		protected override StmModuleFilter[] GetLayoutsForUserDefinedFilters()
		{
			return UserDefinedFilterHelperBusiness.GetLayoutsForUserDefinedFilters(Factory, LayoutsHelper, ModuleIDs.ProcessHeader.Name, includePublishedOnly: true, SearchType);
		}

		protected override bool IsActiveStatusFilterAlwaysApplied()
		{
			return this.GetContexts<BufferManagementBusinessContext>().FirstOrDefault() != BufferManagementBusinessContext.IgnoreDefaultFilters;
		}

		public override bool IsGlowIndexSearchAllowed =>
			isGlowIndexSearchAllowed &&
			(base.IsGlowIndexSearchAllowed || BMSRegistry.Instance.UseGlowIndexingForTagRuleFilters.Value);

		#region IBMFilterRuleFilterBusinessObject Members

		IEnumerable<ZString> IBMFilterRuleFilterBusinessObject.ActiveFilterIdentifiers => ActiveModuleFilters.Select(f => f.OriginalCode);

		IEnumerable<ZString> LoadActiveFilterIdentifiersIncludingUserDefined(HashSet<ZString> currentFilters)
		{
			var userDefinedFiltersAndOthers = ActiveModuleFilters.Split(f => f is ModuleUserDefinedFilter);

			foreach (var filter in userDefinedFiltersAndOthers.NonMatchingSet)
			{
				var filterCode = filter.OriginalCode;

				if (!currentFilters.Contains(filterCode))
				{
					currentFilters.Add(filterCode);
					yield return filterCode;
				}
			}

			foreach (var filter in userDefinedFiltersAndOthers.MatchingSet.Cast<ModuleUserDefinedFilter>())
			{
				var stmModuleFilter = Factory.Load<StmModuleFilter>(filter.LayoutPK);
				var bo = RelatedModuleFiltersHelper.GetNewFilterBusinessObject(stmModuleFilter);

				if (!(bo is BMFilterRuleFilterBusinessObject filterBO) || !bo.LoadFilterRuleLayout(stmModuleFilter))
				{
					continue;
				}

				foreach (var filterIdentiFier in filterBO.LoadActiveFilterIdentifiersIncludingUserDefined(currentFilters))
				{
					yield return filterIdentiFier;
				}
			}
		}

		IEnumerable<ZString> IBMFilterRuleFilterBusinessObject.ActiveFilterIdentifiersIncludingUserDefined => LoadActiveFilterIdentifiersIncludingUserDefined(new HashSet<ZString>());

		#endregion
	}
}
