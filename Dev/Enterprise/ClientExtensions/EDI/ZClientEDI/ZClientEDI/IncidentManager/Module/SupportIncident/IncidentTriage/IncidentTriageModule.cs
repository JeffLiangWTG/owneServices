using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.IncidentManager.Module
{
	public class IncidentTriageModule : ZFilterGridModule, IOperationalActionSupportable
	{
		public IncidentTriageModule()
		{
			Plugins.Add(ControllerIDs.OperationalActions);
		}

		#region Implementation

		public override ModuleIdentifier ID => Modules.ClientModuleRegistration.IncidentTriage;

		public override SecurityCheckpoint SecurityCheckpoint => EDISecurityCheckpoints.CustomerServiceIncidentTriage;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.AlwaysAllow;

		public override bool SupportsWorkflow => true;

		OperationalActionSupporter IOperationalActionSupportable.OperationalActionSupporter => new IncidentTriageActionSupporter();

		public override string WorkflowType
		{
			get { return IncidentTriageConstants.WorkflowDescriptorInformation.Code; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return new IncidentTriageController();
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new IncidentTriageFilterBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new IncidentTriageFilterControl(GridCollection, (IncidentTriageFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new IncidentTriageCollection(Factory);
		}

		#endregion
	}
}
