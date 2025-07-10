using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Module
{
	public class ComponentChangeLogsFilter : ModuleGuidForeignCollectionFilter
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter strip name")]
		public ComponentChangeLogsFilter(BusinessObjectFactory factory)
			: base("Component Change Logs",
				  ModuleIDs.ViewComponentChangeLog, ProcessHeaderSchema.PK, ViewComponentChangeLogSchema.CCL_ParentId, () => new ViewComponentChangeLogCollection(factory), typeof(ProcessHeader))
		{
		}

		protected ComponentChangeLogsFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
		}

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new ComponentChangeLogsFilterValidation(this);
		}

		class ComponentChangeLogsFilterValidation : ModuleGuidFilterValidation
		{
			public ComponentChangeLogsFilterValidation(ComponentChangeLogsFilter parent)
				: base(parent)
			{
				this.parent = parent;
			}

			readonly ComponentChangeLogsFilter parent;

			protected override void CheckSelectedFiltersDescription()
			{
				if (parent.SelectedFilters.ActiveModuleFilters.Select(f => f.Description).Contains(ViewComponentChangeLog.ModuleFilterConstants.TransferType))
				{
					parent.SelectedFiltersDescriptionInfo.AddWarning(Res.GetString("e6bb4a46-0acd-4aa7-b161-984b7ad38dd2", "Using 'Transfer Type' in Component Change Logs has been deprecated for performance reasons. Consider using 'Last Transfer Type' for workflows instead."));
				}
			}
		}
	}
}
