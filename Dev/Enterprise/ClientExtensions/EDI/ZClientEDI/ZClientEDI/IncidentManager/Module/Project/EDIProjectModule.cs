using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.ProcessManagement.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.IncidentManager.Module
{
	public class EDIProjectModule : ProjectModule
	{
		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new EDIProjectFilterBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new EDIProjectFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new ProjectCollection(Factory);
		}

		protected override ModuleIdentifier GetRecentItemsModuleIDCore()
		{
			return ModuleIDs.Project;
		}
	}
}
