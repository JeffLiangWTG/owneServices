using System;
using CargoWise.EntityFramework;
using Enterprise.CommissionManagement.Business;
using Enterprise.CommissionManagement.GUI;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.CommissionManagement.Module
{
	public class CommissionApprovalRequestController : ZController
	{
		#region Standard Overrides

		public override ControllerID ID
		{
			get { return ControllerIDs.CommissionApprovalRequest; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.CommissionApprovalRequest; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(AccCommissionApprovalRequest); }
		}

		#endregion

		#region Form

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			var approvalRequest = (AccCommissionApprovalRequest)businessEntity;
			return new CommissionApprovalRequestForm(approvalRequest);
		}

		public override IZForm ShowNewForm()
		{
			throw new ControllerShowNewFormNotSupportedException("Does not support New functionality");
		}

		public override IZForm ShowDeleteForm(BusinessObject sourceEntity)
		{
			throw new ControllerShowDeleteFormNotSupportedException("Does not support Delete functionality");
		}

		#endregion

		#region Urls

		public override bool MakeUrlsOnlyOpenableForCurrentCompany
		{
			get { return OrganisationsDataRegistry.Instance.OnlyShowCommissionsForCurrentLoginCompany.Value; }
		}

		#endregion

		#region Security

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { throw new NotSupportedException(); }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.CommissionAuthorizationLevel2.IsAllowed ? Env.Security.CommissionAuthorizationLevel2 : Env.Security.CommissionAuthorizationLevel1; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { throw new NotSupportedException(); }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.CommissionApprovalRequest; }
		}

		#endregion
	}
}
