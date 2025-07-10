using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Module
{
	public class ViewComponentChangeLogFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();

			result.AddGuidFilter(ViewComponentChangeLog.ModuleFilterConstants.FromComponent, ModuleIDs.BMComponent, ViewComponentChangeLogSchema.CCL_FC_ComponentFrom, () => new BMComponentCollection(Factory)).MultilingualDescription = ResString.GetMultilingualString("ViewComponentChangeLogFilterBusinessObject|FromComponent", "From Component");
			result.AddGuidFilter(ViewComponentChangeLog.ModuleFilterConstants.ToComponent, ModuleIDs.BMComponent, ViewComponentChangeLogSchema.CCL_FC_ComponentTo, () => new BMComponentCollection(Factory)).MultilingualDescription = ResString.GetMultilingualString("ViewComponentChangeLogFilterBusinessObject|ToComponent", "To Component");
			result.AddGuidFilter(ViewComponentChangeLog.ModuleFilterConstants.Workflow, ModuleIDs.ProcessHeader, ViewComponentChangeLogSchema.CCL_ParentId, () => new ProcessHeaderCollection(Factory, new ZQuery(ProcessHeaderSchema.FH_FH_ParentHeader, SQLComparisonOperator.NotEqual, null))).MultilingualDescription = ResString.GetMultilingualString("ViewComponentChangeLogFilterBusinessObject|Workflow", "Workflow");

			var transferTimeFilter = result.AddDateFilter(ViewComponentChangeLog.ModuleFilterConstants.TransferTime, ViewComponentChangeLogSchema.CCL_TransferTimeUtc, convertFromLocalToUTC: true);

			transferTimeFilter.MultilingualDescription = ResString.GetMultilingualString("ViewComponentChangeLogFilterBusinessObject|TransferTime", "Transfer Time");
			transferTimeFilter.Visibility = FilterVisibility.AlwaysVisible;
			transferTimeFilter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.ThisWeek;

			result.AddTextFilter(ViewComponentChangeLog.ModuleFilterConstants.TransferType, ViewComponentChangeLogSchema.CCL_TransferType, () => new TransferTypeList()).MultilingualDescription = ResString.GetMultilingualString("ViewComponentChangeLogFilterBusinessObject|TransferType", "Transfer Type");
			result.AddTextFilter(ViewComponentChangeLog.ModuleFilterConstants.CCRStatus, ViewComponentChangeLogSchema.CCL_CcrStatus, () => new ConstraintStatusList()).MultilingualDescription = ResString.GetMultilingualString("ViewComponentChangeLogFilterBusinessObject|CCRStatus", "CCR Status");
			result.AddTextFilter(ViewComponentChangeLog.ModuleFilterConstants.WorkflowStatus, ViewComponentChangeLogSchema.CCL_WorkflowStatus, () => new WorkflowStatusList()).MultilingualDescription = ResString.GetMultilingualString("ViewComponentChangeLogFilterBusinessObject|WorkflowStatus", "Workflow Status");
			result.AddTextFilter(ViewComponentChangeLog.ModuleFilterConstants.DeferralReason, ViewComponentChangeLogSchema.CCL_DeferReason, () => BMSRegistry.Instance.DeferralReasons.Value).MultilingualDescription = ResString.GetMultilingualString("ViewComponentChangeLogFilterBusinessObject|DeferralReason", "Deferral Reason");

			result.AddNkFilter(ViewComponentChangeLog.ModuleFilterConstants.User, ViewComponentChangeLogSchema.CCL_GS_NKUser, ModuleIDs.GlbStaff, new GlbStaffCollection(Factory)).MultilingualDescription = ResString.GetMultilingualString("ViewComponentChangeLogFilterBusinessObject|User", "User");

			result.AddNumberRangeFilter(ViewComponentChangeLog.ModuleFilterConstants.BufferPenetration, ViewComponentChangeLogSchema.CCL_BufferPenetrationPercent).MultilingualDescription = ResString.GetMultilingualString("ViewComponentChangeLogFilterBusinessObject|BufferPenetration", "Buffer Penetration");
			result.AddNumberRangeFilter(ViewComponentChangeLog.ModuleFilterConstants.BufferZone, ViewComponentChangeLogSchema.CCL_BufferZone).MultilingualDescription = ResString.GetMultilingualString("ViewComponentChangeLogFilterBusinessObject|BufferZone", "Buffer Zone");

			return result;
		}
	}
}
