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
	public class IncidentDiagnosticCriteriaModule : ZFilterGridModule, IOperationalActionSupportable
	{
		public IncidentDiagnosticCriteriaModule()
		{
			Plugins.Add(ControllerIDs.OperationalActions);
		}

		#region Implementation

		public override ModuleIdentifier ID => Modules.ClientModuleRegistration.IncidentDiagnosticCriteria;

		public override SecurityCheckpoint SecurityCheckpoint => EDISecurityCheckpoints.CustomerServiceIncidentDiagnosticCriteria;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.AlwaysAllow;

		public override bool SupportsWorkflow => true;

		OperationalActionSupporter IOperationalActionSupportable.OperationalActionSupporter => new IncidentDiagnosticCriteriaActionSupporter();

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return new IncidentDiagnosticCriteriaController();
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new IncidentDiagnosticCriteriaFilterBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new IncidentDiagnosticCriteriaFilterControl(GridCollection, (IncidentDiagnosticCriteriaFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new IncidentDiagnosticCriteriaCollection(Factory);
		}

		#endregion
	}
}
