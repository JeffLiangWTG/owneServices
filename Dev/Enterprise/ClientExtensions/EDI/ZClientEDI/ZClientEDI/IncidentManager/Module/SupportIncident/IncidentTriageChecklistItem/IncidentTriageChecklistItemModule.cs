using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.IncidentManager.Module
{
	public class IncidentTriageChecklistItemModule : ZFilterGridModule
	{
		#region Implementation
		public override ModuleIdentifier ID => Modules.ClientModuleRegistration.IncidentTriageChecklistItem;

		public override SecurityCheckpoint SecurityCheckpoint => EDISecurityCheckpoints.CustomerServiceIncidentTriageChecklists;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.AlwaysAllow;

		public override bool SupportsWorkflow => true;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return new IncidentTriageChecklistItemController();
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new IncidentTriageChecklistItemFilterBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new IncidentTriageChecklistsItemFilterControl(GridCollection, (IncidentTriageChecklistItemFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new IncidentTriageChecklistItemCollection(Factory);
		}
		#endregion
	}
}
