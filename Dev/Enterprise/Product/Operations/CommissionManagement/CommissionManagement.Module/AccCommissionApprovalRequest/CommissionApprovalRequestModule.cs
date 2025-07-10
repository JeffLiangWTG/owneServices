using CargoWise.EntityFramework;
using Enterprise.CommissionManagement.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.CommissionManagement.Module
{
	public class CommissionApprovalRequestModule : ZFilterGridModule
	{
		public CommissionApprovalRequestModule()
		{
		}

		#region ID

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.CommissionApprovalRequest; }
		}

		#endregion

		#region Allowed Actions

		public override bool AllowDelete
		{
			get { return false; }
		}

		public override bool AllowNew
		{
			get { return false; }
		}

		#endregion

		#region GridCollection

		new AccCommissionApprovalRequestCollection GridCollection
		{
			get { return (AccCommissionApprovalRequestCollection)base.GridCollection; }
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new AccCommissionApprovalRequestCollection(Factory);
		}

		#endregion

		#region FilterBusinessObject

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new CommissionApprovalRequestFilterBusinessObject();
		}

		#endregion

		#region FilterControl

		protected override IFilterControl GetNewFilterControl()
		{
			return new CommissionApprovalRequestFilterControl(GridCollection, (CommissionApprovalRequestFilterBusinessObject)FilterBusinessObject);
		}

		#endregion

		#region Controller

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.CommissionApprovalRequest);
		}

		#endregion

		#region Security

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.CommissionApprovalRequest; }
		}

		#endregion

		#region Licence

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.CommissionManager; }
		}

		#endregion
	}
}
