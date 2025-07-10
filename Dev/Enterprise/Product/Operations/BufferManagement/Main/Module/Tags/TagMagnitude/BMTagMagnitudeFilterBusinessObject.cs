using Enterprise.BufferManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Module
{
	public class BMTagMagnitudeFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			AddTextFilters(filters);
			return filters;
		}

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Code", TagMagnitudeSchema.TGM_Code).MultilingualDescription = ResString.GetMultilingualString("BufferManagement|TagMagnitudeFilterBusinessObject|Code", "Code");
			filters.AddTextFilter("Description", TagMagnitudeSchema.TGM_Description).MultilingualDescription = ResString.GetMultilingualString("BufferManagement|TagMagnitudeFilterBusinessObject|Description", "Description");
			filters.AddGuidFilter("Tag Group", ModuleIDs.BMTagDefinition, TagMagnitudeSchema.TGM_TGD_Tag, () => new TagDefinitionCollection(Factory)).MultilingualDescription = ResString.GetMultilingualString("BufferManagement|TagMagnitudeFilterBusinessObject|TagGroup", "Tag Group");
			filters.AddGuidFilter("Owner Group", ModuleIDs.GlbGroup, TagMagnitudeSchema.TGM_GG_OwnerGroup, new GlbGroupActiveBusinessObjectCollection(Factory)).MultilingualDescription = ResString.GetMultilingualString("BufferManagement|TagMagnitudeFilterBusinessObject|OwnerGroup", "Owner Group");
		}
	}
}
