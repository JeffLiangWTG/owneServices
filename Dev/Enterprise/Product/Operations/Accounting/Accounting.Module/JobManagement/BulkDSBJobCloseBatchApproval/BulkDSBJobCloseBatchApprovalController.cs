using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.GUI.JobManagement;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class BulkDSBJobCloseBatchApprovalController : ZController
	{
		public override ControllerID ID => ControllerIDs.BulkDSBJobCloseBatchApproval;
		public override ModuleIdentifier ModuleID => ModuleIDs.BulkDSBJobCloseBatchApproval;

		public override Type TypeOfTopLevelBusinessObject => typeof(DsbJobCloseBatch);

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.None;
		protected override SecurityCheckpoint CheckPointForView => Env.Security.DisbursementJobCloseBatchView;
		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.None;
		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.None;

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			var form = new BulkDSBJobCloseBatchApprovalForm((DsbJobCloseBatch)businessEntity);
			return form;
		}

		public IZForm GetApprovalOrCancelForm(IBusiness sourceEntity, BulkDSBJobCloseBatchApprovalFormModes mode)
		{
			var businessEntity = GetLoadedBusinessEntityInLocalFactory(sourceEntity);
			var form = new BulkDSBJobCloseBatchApprovalForm((DsbJobCloseBatch)businessEntity, mode);
			return form;
		}
	}
}
