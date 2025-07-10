using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.Accounting.GUI;
using Enterprise.Accounting.GUI.ARAP.Invoicing;
using Enterprise.Environment;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module.TransactionApproval
{
	public class APInvoiceApprovalController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public APInvoiceApprovalController()
		{
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.APInvoiceApproval; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.APInvoiceApproval; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(APInvoiceChargesApprovalRequest); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new APInvoiceChargesApprovalBulkForm(new APInvoiceChargesApprovalBulk(businessEntity.Factory, new InteractiveSecurityOverrideProvider(), (APInvoiceChargesApprovalRequest)businessEntity), TransactionApprovalFormModes.View);
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.APInvoiceApproval_Cancel; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.APInvoiceApproval_Edit; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.APInvoiceApproval_New; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.APInvoiceApproval; }
		}
	}
}
