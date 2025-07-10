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
	public class IncidentManagementGroupModule : ZFilterGridModule
	{
		#region Implementation
		public override ModuleIdentifier ID => Modules.ClientModuleRegistration.IncidentManagementGroup;

		public override SecurityCheckpoint SecurityCheckpoint => EDISecurityCheckpoints.CustomerServiceIncidentManagementGroup;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.AlwaysAllow;

		public override bool SupportsWorkflow => true;

		public override string WorkflowType
		{
			get { return IncidentManagementGroupConstants.WorkflowDescriptorInformation.Code; }
		}

		public override bool AllowDelete
		{
			get { return false; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return new IncidentManagementGroupController();
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new IncidentManagementGroupFilterBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new IncidentManagementGroupFilterControl(GridCollection, (IncidentManagementGroupFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new IncidentManagementGroupCollection(Factory);
		}
		#endregion
	}
}
