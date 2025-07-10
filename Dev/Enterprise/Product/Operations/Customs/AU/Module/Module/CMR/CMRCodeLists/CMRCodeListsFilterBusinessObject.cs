using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Module
{
	public class CMRCodeListsFilterBusinessObject : FilterStripBusinessObject
	{
		public CMRCodeListsFilterBusinessObject()
		{
			QueryObjectType = typeof(CMRCodeLists);
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();

			filters.AddTextFilter("Code Type", CMRCodeListsSchema.CI_CodeType);
			filters.AddTextFilter("Code", CMRCodeListsSchema.CI_Code);
			filters.AddTextFilter("Short Description", CMRCodeListsSchema.CI_Name);
			filters.AddTextFilter("Long Description", CMRCodeListsSchema.CI_Description);

			return filters;
		}
	}
}
