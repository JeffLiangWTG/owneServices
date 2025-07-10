using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.DocumentScanning.GUI;
using Enterprise.DocumentScanning.PlugIn;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.GUI.GeneralLedger.GLJournals
{
	public partial class GLJournalApprovalBulkForm : GLJournalApprovalBulkFormForDesigner
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for tgljournalFormhe designer", true)]
		public GLJournalApprovalBulkForm()
		{
		}

		public GLJournalApprovalBulkForm(GLJournalApprovalBulk bo, TransactionApprovalFormModes actionMode)
			: base(bo, actionMode)
		{
			if (ActionMode == TransactionApprovalFormModes.SetDescription)
			{
				EDocsTabPage.TabVisible = false;  // If User click EDocs tab during SetDescription mode, it will create a new DocumentFactory and conflict with our relink eDocs logic
			}
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			EDocsTabPage.RunWhenBindingOrFirstShown(new EventHandler(this.InitializeEDocsTab));
		}

		public override bool IsReasonDescriptionReadOnly
		{
			get { return base.IsReasonDescriptionReadOnly && ActionMode != TransactionApprovalFormModes.Reject; }
		}

		#region eDoc tab

		void InitializeEDocsTab(object sender, EventArgs e)
		{
			var currentRequest = TopGrid.ListManager != null ? (TopGrid.ListManager.GetCurrent() as GLJournalApprovalRequest) : ((GLJournalApprovalBulk)BusinessEntity).Approvals[0];
			eDocPlugIn = new eDocsPlugIn(currentRequest); //eDocsPlugIn always bind to the current selected approval in the grid, this is ok as we disable the refresh and Add eDocs button below.
			this.edocUserControl = new eDocsUserControl(eDocPlugIn);
			this.EDocsTabPage.SuspendLayout();
			this.EDocsTabPage.Controls.Add(this.edocUserControl);

			// 
			// edocsUserControl
			// 
			this.edocUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.edocUserControl, "Approvals.StorageMain");   // expose a StorageMain type property here.
			this.edocUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.edocUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.edocUserControl.Name = "edocsUserControl";
			this.edocUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(845, 149, true);
			this.edocUserControl.TabIndex = 0;
			this.edocUserControl.SetReadOnly(true); // disable the refresh and Add eDocs button.
			this.EDocsTabPage.ResumeLayout(true);
		}
#if DEBUG
		public
#endif
		eDocsUserControl edocUserControl;
#if DEBUG
		public
#endif
		eDocsPlugIn eDocPlugIn;

		#endregion

		#region Posting

		protected override bool IsPostingSupported => true;

		protected override bool IsSaveButtonHidden => base.IsSaveButtonHidden || !Environment.Env.Security.GLJournalApprovalPost.IsAllowed;

		protected override void SetPostContext(BusinessObjectFactory newFactory)
		{
			newFactory.SetContext(GLJournalApprovalRequest.Context.Posting);
		}

		protected override BusinessObject GetRequestParent(GLJournalApprovalRequest reloadedApproval, out ZString errorMessage)
		{
			errorMessage = ZString.Empty;
			return reloadedApproval.GetLinkedJournal().journal;
		}

		protected override ControllerID GetControllerIDForEditing(BusinessObject requestParent, out BusinessObject objectToEdit)
		{
			objectToEdit = requestParent;
			return ControllerIDs.GLJournalLinkedToApproval;
		}

		protected override void PostApprovalsAndRemovePosted(TransactionApprovalBulk<GLJournal, GLJournalApprovalRequest, GLJournalApprovalRequestDetails> approvalBulk)
		{
			((GLJournalApprovalBulk)approvalBulk).PostApprovalsAndRemovePosted(new GLJournalFormApprovalGUIProvider());
		}

		#endregion
	}

#if DEBUG
	// This ZForm serves as base class only. It's not abstract so that it can be open in designer tool. Hence it has TestExcludeZWinFormsAllHaveFormBashers attribute applied.
	[TestExcludeZWinFormsAllHaveFormBashers]
#endif
	public class GLJournalApprovalBulkFormForDesigner : TransactionApprovalBulkForm<GLJournal, GLJournalApprovalRequest, GLJournalApprovalRequestDetails>
	{
		public GLJournalApprovalBulkFormForDesigner()
		{
		}

		public GLJournalApprovalBulkFormForDesigner(GLJournalApprovalBulk bo, TransactionApprovalFormModes actionMode)
			: base(bo, actionMode)
		{
		}
	}
}
