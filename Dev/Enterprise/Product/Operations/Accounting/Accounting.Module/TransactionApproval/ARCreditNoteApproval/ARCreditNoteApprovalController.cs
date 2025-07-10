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
	public class ARCreditNoteApprovalController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public ARCreditNoteApprovalController()
		{
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.ARCreditNoteApproval; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.ARCreditNoteApproval; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(ARCreditNoteApprovalRequest); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new ARCreditNoteApprovalBulkForm(new ARCreditNoteApprovalBulk(businessEntity.Factory, new InteractiveSecurityOverrideProvider(), (ARCreditNoteApprovalRequest)businessEntity), TransactionApprovalFormModes.View);
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.ARCreditNoteApproval; }
		}
	}
}
