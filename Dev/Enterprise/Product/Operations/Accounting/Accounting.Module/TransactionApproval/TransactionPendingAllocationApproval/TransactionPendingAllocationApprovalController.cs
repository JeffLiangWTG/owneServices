using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.GUI;
using Enterprise.Accounting.GUI.ARAP.Invoicing;
using Enterprise.Environment;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module.TransactionApproval
{
	public class TransactionPendingAllocationApprovalController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public TransactionPendingAllocationApprovalController()
		{
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.TransactionsPendingAllocationApproval; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.TransactionsPendingAllocationApproval; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(TransactionPendingAllocationApprovalRequest); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new TransactionPendingAllocationApprovalBulkForm(new TransactionPendingAllocationApprovalBulk(businessEntity.Factory, new InteractiveSecurityOverrideProvider(), (TransactionPendingAllocationApprovalRequest)businessEntity), TransactionApprovalFormModes.View);
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
			get { return Env.Security.TransactionsPendingAllocationApproval; }
		}
	}
}
