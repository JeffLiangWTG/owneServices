using System;
using System.ComponentModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.DocumentScanning.GUI;
using Enterprise.DocumentScanning.PlugIn;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.GUI
{
	public enum CreditControlledDocumentsApprovalFormModes
	{
		SetDescription,
		Approve,
		Reject,
		Cancel,
		View
	}

	public partial class CreditControlledDocumentsApprovalForm : ZForm, IButtonPostTextOverride, IButtonApplyTextOverride
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public CreditControlledDocumentsApprovalForm()
		{
		}

		public CreditControlledDocumentsApprovalForm(CreditControlledDocumentsApprovalBulk bo, CreditControlledDocumentsApprovalFormModes actionMode)
			: base(bo)
		{
			ActionMode = actionMode;
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);
			DisplayMode = ODisplayMode.Edit;
			PostingButtonsUserControl.SaveButton.Visible = ActionMode == CreditControlledDocumentsApprovalFormModes.Approve && Env.Security.OnCreditHoldControllerApproveAllDocuments.IsAllowed;
			Approvals?.CreditControlledDocumentsApprovalsAllowedToProcess.ForEach(x => x.SetupEDocsFactoryToBeSavedWithApprovalFactory());
		}

		CreditControlledDocumentsApprovalBulk Approvals => BusinessEntity as CreditControlledDocumentsApprovalBulk;
		readonly CreditControlledDocumentsApprovalFormModes ActionMode;
		eDocsUserControl edocUserControl;
		eDocsPlugIn eDocPlugIn;

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			MissingResourceStringChecker.ExcludeFromTest(incoTermLabel);
			if (ActionMode == CreditControlledDocumentsApprovalFormModes.SetDescription)
			{
				RemoveCreditDetailsColumns();
				ReasonDescriptionTextBox.ReadOnly = false;
				creditTabPage.TabVisible = false;
				openJobButton.Visible = false;
			}
			else
			{
				ReasonDescriptionTextBox.ReadOnly = true;
				creditTabPage.TabVisible = true;
				openJobButton.Visible = true;
			}
			if (Approvals != null)
			{
				TopGridPanel.Visible = Approvals.CreditControlledDocumentsApprovalsAllowedToProcess.Count > 1;
				TopSingleRequestPanel.Visible = Approvals.CreditControlledDocumentsApprovalsAllowedToProcess.Count == 1;
			}
			orgInBreachGrid.ReadOnly = true;
			eDocsTabPage.RunWhenBindingOrFirstShown(new EventHandler(InitializeEDocsPlugin));
			creditTabPage.TabInitialized += CreditTabInitialized;
		}

		void RemoveCreditDetailsColumns()
		{
			var columnNamesToRemove = new[] {
				"OrgCreditLimit",
				"IsUsingSettlementGroupCreditLimit",
				"IsCreditOnHold",
				"OverCreditLimit",
				"StandardOverdueAmount",
				"DisbursementOverdueAmount",
				"StandardWIPsBilledToThisJob",
				"DisbursementWIPsBilledToThisJob"
			};
			orgInBreachGrid.RemoveFromAvailableColumns(columnNamesToRemove);
		}

		public override string FormVerb
		{
			get
			{
				string text = base.FormVerb;
				switch (ActionMode)
				{
					case CreditControlledDocumentsApprovalFormModes.Approve:
						text = Res.GetString("CreditControlledDocumentsApprovalForm|Approve", "Approve");
						break;
					case CreditControlledDocumentsApprovalFormModes.Reject:
						text = Res.GetString("CreditControlledDocumentsApprovalForm|Reject", "Reject");
						break;
					case CreditControlledDocumentsApprovalFormModes.Cancel:
						text = Res.GetString("CreditControlledDocumentsApprovalForm|Cancel", "Cancel");
						break;
					case CreditControlledDocumentsApprovalFormModes.SetDescription:
						text = Res.GetString("CreditControlledDocumentsApprovalForm|Request", "Request");
						break;
				}

				return text;
			}
		}

		protected override void OnApplyButtonClick(object sender, EventArgs e)
		{
			if (Approvals != null)
			{
				foreach (var approval in Approvals.CreditControlledDocumentsApprovalsAllowedToProcess)
				{
					approval.SetStatus(approval.XP_ApprovalStatus, true);
				}
				PostingButtonsUserControl.SaveAndCloseButton.PerformClick();
			}
		}

		protected override void ZForm_Closing(object sender, CancelEventArgs e)
		{
			//Do nothing to prevent asking user to save data
		}

		string IButtonPostTextOverride.PostButtonText
		{
			get { return ActionMode == CreditControlledDocumentsApprovalFormModes.Approve ? Res.GetString("CreditControlledDocumentsApprovalForm|Approve", "Approve") : Res.GetString("513bd902-0047-49e6-9502-2423089e9f37", "S&ave && Close"); }
		}

		string IButtonApplyTextOverride.ApplyButtonText
		{
			get
			{
				return ActionMode == CreditControlledDocumentsApprovalFormModes.Approve ? Res.GetString("0d53e805-c564-4921-a854-cebaabe48789", "Approve All Documents") : Res.GetString("787b39e4-c6b6-4239-b4a7-6ad8d76e9a73", "&Save");
			}
		}

		void InitializeEDocsPlugin(object sender, EventArgs e)
		{
			var currentAproval = (TopGrid.Visible && TopGrid.ListManager != null) ? (TopGrid.ListManager.GetCurrent() as CreditControlledDocumentsApproval) : Approvals.CreditControlledDocumentsApprovalsAllowedToProcess.First();
			eDocPlugIn = new eDocsPlugIn(currentAproval); //eDocsPlugIn always bind to the current selected approval in the grid
			edocUserControl = new eDocsUserControl(eDocPlugIn);
			eDocsTabPage.SuspendLayout();
			eDocsTabPage.Controls.Add(edocUserControl);

			// 
			// edocsUserControl
			// 
			this.eDocsTabPage.ResumeLayout(true);
			this.edocUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.edocUserControl, "CreditControlledDocumentsApprovalsAllowedToProcess.StorageMain");   // expose a StorageMain type property here.
			this.edocUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.edocUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.edocUserControl.Name = "edocsUserControl";
			this.edocUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(845, 149, true);
			this.edocUserControl.TabIndex = 0;
			this.eDocsTabPage.ResumeLayout(true);
		}

		void openJobButton_Click(object sender, EventArgs e)
		{
			if (Approvals != null)
			{
				var currentApproval = Approvals.CreditControlledDocumentsApprovalsAllowedToProcess.First();
				var bizObj = currentApproval.ParentBusinessObject;
				var controllerID = currentApproval.ControllerID;
				if (bizObj != null && controllerID != null)
				{
					ShowOperationalJob(bizObj, controllerID);
				}
				else
				{
					Globals.Message.ShowError(Res.GetString("3fd0f0c4-529b-4ab1-a3d4-b84fa5fc7bfe", "Cannot open corresponding operations job."));
				}
			}
		}

		protected virtual void ShowOperationalJob(BusinessObject bizObj, ControllerID controllerID)
		{
			LastShownJobForm = (ZForm)ZControllerFactory.Create(controllerID).ShowEditForm(bizObj);
		}

		ZForm LastShownJobForm;

		void CreditTabInitialized(object sender, EventArgs e)
		{
			MissingResourceStringChecker.ExcludeFromTest(SettlementGroupInfoLabel);
			var currentAproval = (TopGrid.Visible && TopGrid.ListManager != null) ? (TopGrid.ListManager.GetCurrent() as CreditControlledDocumentsApproval) : Approvals.CreditControlledDocumentsApprovalsAllowedToProcess.First();
			if (currentAproval.OrganisationsInBreach.Count > 0)
			{
				currentAproval.CreditStatusBizObj.OrganisationPK = currentAproval.OrganisationsInBreach.Cast<OrganisationInBreach>().First().OrganisationPK;
			}
		}
	}
}
