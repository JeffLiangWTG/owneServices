using CargoWise.Types;
using Enterprise.CustomerService.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class ProjectModuleListBuilder : ModuleListBuilder
	{
		protected override bool ShouldIncludeAllModule(ModuleListType moduleListType, ZString product, ZString productArea)
		{
			return true;
		}
	}
}

