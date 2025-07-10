using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.GUI.JobInvoicing;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.Interop.OutlookIntegration;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	[ZArchitecture.GUI.Testing.TestExcludeZWinFormsAllHaveFormBashers]
	public partial class ProfessionalServicesQuoteForm : ZForm
	{
		#region Constructor

		public ProfessionalServicesQuoteForm()
		{
		}

		public ProfessionalServicesQuoteForm(ProfessionalServicesQuote professionalServicesQuote)
			: base(professionalServicesQuote)
		{
			UpdateCompleteOrReopenTaskButtonText();
			WorkflowTabPage.Initialize(BusinessEntity);
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtons);
			professionalServicesQuote.IM_StatusInfo.ValueChanged += new EventHandler(IM_StatusInfo_ValueChanged);
			professionalServicesQuote.IM_OA_BranchAddressInfo.ValueChanged += new EventHandler(Client_ValueChanged);

			PlugIns.Add(ControllerIDs.JobInvoicing);
			PlugIns.Add(ControllerIDs.eDocsPlugIn);
			PlugIns.Add(ClientControllerRegistration.PSQRelatedOpportunities);

			SetupEmailSentLabel();
			SetupEmailMenuitems();

			ZPlugIn plugIn = PlugIns.GetPlugIn(ControllerIDs.JobInvoicing);
			if (plugIn.UserControl is JobInvoicingUserControl)
			{
				LocalClientControl = ((JobInvoicingUserControl)plugIn.UserControl).JobChargeUserControl.JH_OH_LocalChargesBoundOrgCard;
			}

			if (BusinessEntity.IM_InvoicingLocalClientHasInvoicingPreferencesNote)
			{
				if (LocalClientControl != null)
				{
					LocalClientControl.ForeColor = Color.Red;
				}
			}
			Job job = new Job.Loader(BusinessEntity).Load();
			if (job != null)
			{
				job.JH_OA_LocalChargesAddrInfo.ValueChanged += new EventHandler(Client_ValueChanged);
			}
			if (BusinessEntity.IM_ClientHasInvoicingPreferencesNote)
			{
				ClientLabel.ForeColor = Color.Red;
			}

			EmailButton.AllowOverlap(EmailMenuPanel);
		}

		void MainTabControl_Selecting(object sender, TabControlCancelEventArgs e)
		{
			if (e.TabPage == RelatedItemsTabPage && RelatedItemsTabPage.Controls.Count == 0)
			{
				InitializeRelatedItemsTabPage(RelatedItemsTabPage);
			}
		}

		void InitializeRelatedItemsTabPage(ZTabPage relatedItemsTabPage)
		{
			RelatedItemsUserControl = new EDIWorkTaskRelatedItemUserControl(IsViewOrDeleteMode);
			RelatedItemsUserControl.Dock = DockStyle.Fill;
			relatedItemsTabPage.Controls.Add(RelatedItemsUserControl);
		}

		protected EDIWorkTaskRelatedItemUserControl RelatedItemsUserControl;

		protected override void InitialiseForm()
		{
			base.InitializeComponent();
			base.InitialiseForm();
		}

		#endregion

		#region CompleteOrReopenTaskButton State

		enum CompleteOrReopenTaskButtonStates
		{
			ReopenTask,
			MarkAsFinished,
			CompleteTask
		}

		CompleteOrReopenTaskButtonStates CurrentCompleteOrReopenTaskButtonState
		{
			get { return (BusinessEntity.IsClosedOrCancelled) ? CompleteOrReopenTaskButtonStates.ReopenTask : CompleteOrReopenTaskButtonStates.CompleteTask; }
		}

		#endregion

		#region Update CompleteOrReopenTaskButton Text When Incident Status is Changed

		void IM_StatusInfo_ValueChanged(object sender, EventArgs e)
		{
			UpdateCompleteOrReopenTaskButtonText();
		}

		void UpdateCompleteOrReopenTaskButtonText()
		{
			switch (CurrentCompleteOrReopenTaskButtonState)
			{
				case (CompleteOrReopenTaskButtonStates.CompleteTask):
					CompleteOrReopenTaskButton.Text = "Complete Task";
					FormToolTip.SetToolTip(CompleteOrReopenTaskButton, "Close this task.");
					break;

				case (CompleteOrReopenTaskButtonStates.MarkAsFinished):
					CompleteOrReopenTaskButton.Text = "Mark as Finished";
					FormToolTip.SetToolTip(CompleteOrReopenTaskButton, "Mark this task as Finished - Deployed and Pending Verification.");
					break;

				case (CompleteOrReopenTaskButtonStates.ReopenTask):
					CompleteOrReopenTaskButton.Text = "Reopen Task";
					FormToolTip.SetToolTip(CompleteOrReopenTaskButton, "Reopen this task.");
					break;
			}
		}

		#endregion

		#region Completing, Cancelling or Reopening Tasks

		void CompleteOrReopenTaskButton_Click(object sender, EventArgs e)
		{
			HandleCompleteOrReopenTaskButtonClick();
		}

		void HandleCompleteOrReopenTaskButtonClick()
		{
			switch (CurrentCompleteOrReopenTaskButtonState)
			{
				case (CompleteOrReopenTaskButtonStates.CompleteTask):
					BusinessEntity.CloseIncident();
					break;

				case (CompleteOrReopenTaskButtonStates.MarkAsFinished):
					MarkIncidentAsFinished();
					break;

				case (CompleteOrReopenTaskButtonStates.ReopenTask):
					BusinessEntity.ReopenIncident();
					break;
			}
		}

		void MarkIncidentAsFinished()
		{
			BusinessEntity.MarkIncidentAsFixed("");
		}

		void CancelTaskButton_Click(object sender, EventArgs e)
		{
			CancelTask();
		}

		void CancelTask()
		{
			BusinessEntity.CancelIncident();
		}

		#endregion

		#region Email

		void SetupEmailMenuitems()
		{
			if (BusinessEntity.AllowOutlookEmailSending)
			{
				emailToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[]
				{
					this.correspondenceFromOutlookToolStripMenuItem
				});
			}
		}

		void correspondenceFromOutlookToolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				string filename = CargoWise.IO.Temp.GetTempFileNameWithExtension("eml");		// Outlook Express file extension
				System.IO.File.WriteAllText(filename, BusinessEntity.GetEmailInEmlFormat());
				FileOpener.Open(filename);

				BusinessEntity.NotifyMailSent();
			}
			catch (OutlookException ex)
			{
				Globals.Message.ShowWarning(ex.Message);
			}
			catch (ApplicationException ex)
			{
				Globals.Message.ShowWarning(ex.Message);
			}
		}

		void SetupEmailSentLabel()
		{
			EmailSentLabel.ForeColor = Color.FromArgb(0, 150, 0);
			EmailSentLabel.IsFontBold = true;
			EmailSentLabel.DataBindings.Add(new KBinding("IsVisibleForBinding", BusinessEntity, BusinessEntity.IM_IsEmailSentInfo.Name));
		}

		void EmailButton_Click(object sender, EventArgs e)
		{
			if (emailToolStripMenuItem.DropDown.Visible)
			{
				emailToolStripMenuItem.HideDropDown();
			}
			else
			{
				emailToolStripMenuItem.ShowDropDown();
			}
		}

		#endregion

		#region Editing the Contact

		void EditContactButton_Click(object sender, EventArgs e)
		{
			ShowEditContactForm();
		}

		void ShowEditContactForm()
		{
			if (BusinessEntity.Contact != null)
			{
				ZController controller = ZControllerFactory.Create(ControllerIDs.OrgContacts);
				controller.ShowEditForm(BusinessEntity.Contact);
#if DEBUG
				LastShownForm = controller.LastShownForm;
#endif
			}
		}
#if DEBUG
		internal IZForm LastShownForm;
#endif

#endregion

		#region Convert to Work Item

		void ConvertToWorkItemButton_Click(object sender, EventArgs e)
		{
			ConvertToWorkItem();
		}

		protected void ConvertToWorkItem()
		{
			if (RelatedItemsUserControl == null)
			{
				InitializeRelatedItemsTabPage(RelatedItemsTabPage);
			}

			if (RelatedItemsUserControl.CurrentDataItem == null)
			{
				RelatedItemsUserControl.SetDataBinding(BusinessEntity, "");
			}
			RelatedItemsUserControl.AddNewWorkItem();
		}

		#endregion

		#region ChangeClientLabelColor

		readonly ZOrgAddressControl LocalClientControl;

		void Client_ValueChanged(object sender, EventArgs e)
		{
			if (BusinessEntity != null)
			{
				if (BusinessEntity.IM_ClientHasInvoicingPreferencesNote)
				{
					ClientLabel.ForeColor = Color.Red;
				}
				else
				{
					ClientLabel.ForeColor = Color.Black;
				}
			}

				if (LocalClientControl != null && BusinessEntity != null)
				{
					if (BusinessEntity.IM_InvoicingLocalClientHasInvoicingPreferencesNote)
					{
						LocalClientControl.ForeColor = Color.Red;
					}
					else
					{
						LocalClientControl.ForeColor = Color.Black;
					}
				}
		}

		#endregion

		#region For Testing
#if DEBUG

		public ZTabControl TopLevelTabControl_Exposed => TopLevelTabControl;

#endif
		#endregion

		#region Implementation

		public override string FormCaption
		{
			get { return "Professional Services Quote"; }
		}

		public new ProfessionalServicesQuote BusinessEntity
		{
			get { return (ProfessionalServicesQuote)base.BusinessEntity; }
		}

		public override bool IsResizableByTabPageAllowed => true;

		#endregion
	}
}
