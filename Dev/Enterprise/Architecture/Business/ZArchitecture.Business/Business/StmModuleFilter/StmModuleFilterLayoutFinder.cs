using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business
{
	class StmModuleFilterLayoutFinder : IFilterModuleLayoutFinder
	{
		public CodeDescriptionPairList GetListOfModuleFilterLayouts(string moduleName)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			ZQuery filterLayoutsQuery = new ZQuery();
			filterLayoutsQuery.AddToFilter(StmModuleFilterSchema.S9_IsPublished, ZBool.True);
			filterLayoutsQuery.AddToFilter(StmModuleFilterSchema.S9_ModuleID, moduleName);
			StmModuleFilter[] filters = factory.Load<StmModuleFilter>(filterLayoutsQuery);

			CodeDescriptionPairList result = new CodeDescriptionPairList();
			foreach (StmModuleFilter filterLayout in filters)
			{
				result.AddPair(filterLayout.S9_FilterNameMultilingual);
			}

			return result;
		}
	}
}
