using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using ResString = Enterprise.ZArchitecture.GUI.UserControls.ResString;

namespace Enterprise.ZArchitecture.GUI
{
	public class ZStmModuleFilterFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			filters.AddFiltersForTranslatableText(FilterDescriptions.Name.EnglishText, StmModuleFilterSchema.S9_FilterName, typeof(StmModuleFilter), FilterDescriptions.Name);
			filters.AddTextFilter(FilterDescriptions.Type.EnglishText, StmModuleFilterSchema.S9_FilterType, new StmModuleFilterTypes()).MultilingualDescription = FilterDescriptions.Type;
			filters.AddFlagFilter(FilterDescriptions.Published.EnglishText, FilterDescriptions.Published, StmModuleFilterSchema.S9_IsPublished, ModuleFilterSubGroup.Default).MultilingualDescription = FilterDescriptions.Published;
			filters.AddTextFilter(FilterDescriptions.Module.EnglishText, StmModuleFilterSchema.S9_ModuleID).MultilingualDescription = FilterDescriptions.Module;

			return filters;
		}

		static class FilterDescriptions
		{
			public static ResourceString Name => ResString.GetMultilingualString("ZStmModuleFilterFilterBusinessObject.Name", "Filter Name");
			public static ResourceString Type => ResString.GetMultilingualString("ZStmModuleFilterFilterBusinessObject.Type", "Filter Type");
			public static ResourceString Published => ResString.GetMultilingualString("ZStmModuleFilterFilterBusinessObject.Published", "Published");
			public static ResourceString Module => ResString.GetMultilingualString("ZStmModuleFilterFilterBusinessObject.Module", "Module");
		}
	}
}
