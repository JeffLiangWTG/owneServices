using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.FeatureControl.Module
{
	public class FeatureControlFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			filters.AddTextFilter("Code", FeatureControlHeaderSchema.FCM_FeatureControlCode);
			filters.AddTextFilter("Description", FeatureControlHeaderSchema.FCM_Description);
			filters.AddGuidFilter("Release Group", ModuleIDs.GlbGroup, FeatureControlHeaderSchema.FCM_GG_ReleaseGroup, new GlbGroupCollection(Factory, true));
			return filters;
		}
	}
}
