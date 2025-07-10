using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.Module
{
	public class TagWithJobOrWorkflowFilter : ModuleGuidAppliedToSubCollectionFilter
	{
		public TagWithJobOrWorkflowFilter(ZString description, FilterCategory category, ModuleIdentifier moduleID, GetGuidQueryWithNotInandDropdownOption queryDelegate, IBusinessObjectCollection collection)
			: base(description, category, moduleID, queryDelegate, collection)
		{
		}

		protected override CodeDescriptionPairList CreateDropDownTypeNameList()
		{
			return new JobOrWorkflow();
		}
	}
}
