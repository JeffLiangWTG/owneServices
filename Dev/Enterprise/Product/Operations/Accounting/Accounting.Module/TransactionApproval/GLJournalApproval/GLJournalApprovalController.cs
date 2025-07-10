using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.Accounting.GUI;
using Enterprise.Accounting.GUI.GeneralLedger.GLJournals;
using Enterprise.Environment;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module.TransactionApproval
{
	public class GLJournalApprovalController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public GLJournalApprovalController()
		{
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.GLJournalApproval; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.GLJournalApproval; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(GLJournalApprovalRequest); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new GLJournalApprovalBulkForm(new GLJournalApprovalBulk(businessEntity.Factory, new InteractiveSecurityOverrideProvider(), (GLJournalApprovalRequest)businessEntity), TransactionApprovalFormModes.View);
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.GLJournalApprovalEdit; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.GLJournalApproval; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.GLJournalApproval; }
		}
	}
}

