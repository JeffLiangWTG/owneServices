using CargoWise.EntityFramework;
using Enterprise.CustomerService.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.CustomerService.Module
{
	public class IncidentApprovalModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.ServiceRequest; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.ServiceRequest);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new IncidentApprovalFilterControl(GridCollection, (IncidentApprovalFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new IncidentApprovalCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new IncidentApprovalFilterBusinessObject();
		}

		#region Licence checkpoints

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		#endregion

		#region Security

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.IncidentApproval; }
		}

		#endregion Security checkpoints

		public override bool AllowNew
		{
			get { return false; }
		}
	}
}
