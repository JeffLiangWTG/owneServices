using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Module
{
	public class ProcessHeaderLinkFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			filters.AddGuidFilter(ProcessHeaderLink.ModuleFilterConstants.HeaderFrom, ModuleIDs.ProcessHeader, ProcessHeaderLinkSchema.FP_FH_HeaderFrom, () => new ProcessHeaderCollection(Factory)).MultilingualDescription = ResString.GetMultilingualString("BufferManagement|Module|ProcessHeaderLink|From Workflow", "From Workflow");
			filters.AddGuidFilter(ProcessHeaderLink.ModuleFilterConstants.HeaderTo, ModuleIDs.ProcessHeader, ProcessHeaderLinkSchema.FP_FH_HeaderTo, () => new ProcessHeaderCollection(Factory)).MultilingualDescription = ResString.GetMultilingualString("BufferManagement|Module|ProcessHeaderLink|To Workflow", "To Workflow");

			filters.AddTextFilter(ProcessHeaderLink.ModuleFilterConstants.LinkType, ProcessHeaderLinkSchema.FP_LinkType, () => new ProcessHeaderLinkTypeList()).MultilingualDescription = ResString.GetMultilingualString("BufferManagement|Module|ProcessHeaderLink|Link Type", "Link Type");

			return filters;
		}
	}
}
