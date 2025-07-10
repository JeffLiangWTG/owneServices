using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.GUI.JobManagement;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class BulkDSBJobCloseBatchApprovalModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.BulkDSBJobCloseBatchApproval;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.DisbursementJobCloseBatchView;

		public override bool AllowNew => false;
		public override bool AllowEdit => false;
		public override bool AllowDelete => false;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.BulkDSBJobCloseBatchApproval);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new DsbJobCloseBatchCollection(Factory);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new BulkDSBJobCloseBatchApprovalFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new BulkDSBJobCloseBatchApprovalFilterBusinessObject();
		}
		protected override MenuItem[] GetNewActionMenuItems()
		{
			List<MenuItem> menuItems = new List<MenuItem>(base.GetNewActionMenuItems());
			menuItems.Add(new ZMenuItem(ApprovalText, ApprovalBatch));
			menuItems.Add(new ZMenuItem(CancelText, CancelBatch));
			return menuItems.ToArray();
		}

		#region Event Handler

		public IZForm ApprovalOrCancelBatch(bool isApproval)
		{
			IZForm formToShow = null;
			var caption = isApproval ? ApprovalText : CancelText;
			var batch = Grid.SelectedElements?.Length == 1 ? Grid.SelectedElements[0] as DsbJobCloseBatch : null;
			var mode =  isApproval ? BulkDSBJobCloseBatchApprovalFormModes.Approve : BulkDSBJobCloseBatchApprovalFormModes.Cancel;
			var securityCheckpoint = isApproval ? Env.Security.DisbursementJobCloseBatchApprove : Env.Security.DisbursementJobCloseBatchCancel;

			if (!securityCheckpoint.IsAllowed)
			{
				securityCheckpoint.ShowError();
			}
			else if (Grid.SelectedElements.Length != 1)
			{
				Globals.Message.ShowError(SelectOneRecordText, caption);
			}
			else if (batch.JBB_BatchStatus != AccountingConstants.DsbJobBatchStatus.RequireApproval)
			{
				Globals.Message.ShowError(CanNotApprovalOrCancelText, caption);
			}
			else
			{
				var controller = ZControllerFactory.Create(ControllerIDs.BulkDSBJobCloseBatchApproval) as BulkDSBJobCloseBatchApprovalController;
				formToShow = controller.GetApprovalOrCancelForm(Grid.SelectedElements[0], mode);
				ZFormModaliser.Show((Form)formToShow, Grid.FindForm());
			}

			return formToShow;
		}

		protected void ApprovalBatch(object sender, EventArgs e)
		{
			ApprovalOrCancelBatch(true);
		}

		protected void CancelBatch(object sender, EventArgs e)
		{
			ApprovalOrCancelBatch(false);
		}

		#endregion

		#region Text 

		public ResourceString ApprovalText = ResString.GetMultilingualString("6acd0090-3afe-4f0a-bab1-14046baceb9f", "Approve");
		public ResourceString CancelText = ResString.GetMultilingualString("6658c5a4-d8f7-4bfc-9df1-e5dfceb313ef", "Cancel");
		public ResourceString SelectOneRecordText = ResString.GetMultilingualString("b52de36a-6547-4bca-b582-6c38eb916778", "Please select one record");
		public ResourceString CanNotApprovalOrCancelText = ResString.GetMultilingualString("ee150c56-ed6a-4f5e-b911-3ea21012be26", "You can only Approval/Cancel record with status REQ");

		#endregion
	}
}
