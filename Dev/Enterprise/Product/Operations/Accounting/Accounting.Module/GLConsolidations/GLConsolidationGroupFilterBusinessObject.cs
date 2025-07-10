using Enterprise.Accounting.Business.GeneralLedger.GLConsolidations;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module
{
	public class GLConsolidationGroupFilterBusinessObject : AccountingFilterStripBusinessObject
	{
		public GLConsolidationGroupFilterBusinessObject()
			: base()
		{
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();

			var codeFilter = filters.AddTextFilter("Code", AccConsolidationGroupSchema.YR_Code);
			codeFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|GLConsolidationGroupsFilter|Code", "Code");

			var descriptionFilter = filters.AddTextFilter("Description", AccConsolidationGroupSchema.YR_Description);
			descriptionFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|GLConsolidationGroupsFilter|Description", "Description");

			var parentGroupFilter = filters.AddGuidFilter("Parent Group", ModuleIDs.GLConsolidationGroups, AccConsolidationGroupSchema.YR_YR_ConsolidationGroup, ConsolidationGroups);
			parentGroupFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|GLConsolidationGroupsFilter|ParentGroup", "Parent Group");

			var lastProcessedDateFilter = filters.AddDateFilter("Last Processed Date", AccConsolidationGroupSchema.YR_HighWatermark, true);
			lastProcessedDateFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|GLConsolidationGroupsFilter|LastProcessedDate", "Last Processed Date");

			return filters;
		}

		public AccConsolidationGroupCollection ConsolidationGroups
		{
			get { return new AccConsolidationGroupCollection(Factory); }
		}
	}
}