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
	public class InvestigationItemModule : ZFilterGridModule, IOperationalActionSupportable
	{
		public InvestigationItemModule()
		{
			Plugins.Add(ControllerIDs.OperationalActions);
		}

		#region Implementation

		public override ModuleIdentifier ID => Modules.ClientModuleRegistration.InvestigationItem;

		public override SecurityCheckpoint SecurityCheckpoint => EDISecurityCheckpoints.CustomerServiceInvestigationItem;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.AlwaysAllow;

		public override bool SupportsWorkflow => true;

		OperationalActionSupporter IOperationalActionSupportable.OperationalActionSupporter => new InvestigationItemActionSupporter();

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return new InvestigationItemController();
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new InvestigationItemFilterBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new InvestigationItemFilterControl(GridCollection, (InvestigationItemFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new InvestigationItemCollection(Factory);
		}

		#endregion
	}
}
