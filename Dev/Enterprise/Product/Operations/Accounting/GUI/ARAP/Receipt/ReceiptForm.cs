using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.GUI.OrgCollectionCalls;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GUI.ARAP
{
#if DEBUG

	public interface IAccessReceiptFormInternalsForTest
	{
		Core.Forms.ZPostOrCancelButton CloseButton { get; }
		void OnApplyButtonClick(object sender, EventArgs e);
		Core.Forms.ZPostOrCancelButton PostButton { get; }
		ZTextBox ChequeNoTextBox { get; }
		IButton fApplyButton { get; }
		ZGuidFindBox OrganisationGuidFindBox { get; }
	}

#endif

	public partial class ReceiptForm : AccountingZForm
#if DEBUG
, IAccessReceiptFormInternalsForTest
#endif
	{
		public ReceiptForm(Receipt receiptBizO)
			: base(receiptBizO)
		{
			SetupPostingButtons();
			AH_NumberOfSupportingDocumentsCalcEdit.Visible = receiptBizO.AH_NumberOfSupportingDocumentsVisible_ReadOnly;
			DisplayModeChanged += ReceiptForm_DisplayModeChanged;

			PlugIns.Add(ControllerIDs.eDocsPlugIn);

			if (ARReceiptBizO != null)
			{
				ARReceiptBizO.On_DisplayCollectionNotesForm += ARReceiptBizO_On_DisplayCollectionNotesForm;
				PlugIns.Add(ControllerIDs.LinkedeNettEDIMessage);
			}

			if (receiptBizO != null)
			{
				receiptBizO.HasChangesChanged += ReceiptBizO_HasChangesChanged;
			}

			WorkflowTabPage.Initialize(receiptBizO);
			FormLoadedWithArgs += (s, e) => InitializeWorkflowTabPage(e.Args);
		}

		void InitializeWorkflowTabPage(IEnumerable<string> args)
		{
			if (args != null && args.FirstOrDefault() != ModuleIDs.APTransaction.ToString() && args.FirstOrDefault() != ModuleIDs.ARTransaction.ToString())
			{
				WorkflowTabPage.TabVisible = false;
			}
		}

		protected virtual void SetupPostingButtons()
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, null, CloseButton, PostWithoutMatchingButton);
			ZFormPostingButtonsStrategy.SetupPosting(this, null, CloseButton, ReceiptDetailButton);
		}

		public override string FormCaption
		{
			get { return ReceiptBizO.DefaultDescription; }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		#region ValidateAndSave Override

		protected override ContinueWithSave ValidateAndSave()
		{
			ContinueWithSave baseResult = ContinueWithSave.No;

			bool shouldPromptForPrintReceipt = false;
			if (!ReceiptBizO.IsInDatabase)
			{
				ReceiptBizO.RunPreSaveValidation();
				if (ReceiptBizO.HasErrors || !ReceiptBizO.IsPostWithMatching)
				{
					ReceiptBizO.IsPostWithMatching = false;

					if (ReceiptBizO.IsInMatchingContext &&
						ReceiptBizO.MatchingBaseObject != null)
					{
						ReceiptBizO.MatchingBaseObject.MoveAllFromMatchToUnmatch();
					}

					if (ReceiptBizO.HasErrors)
					{
						return base.ValidateAndSave();
					}
				}

				shouldPromptForPrintReceipt = true;
			}

			if (!ReceiptBizO.IsPostWithMatching)
			{
				ReceiptBizO.UseReceiptValidation = true;
				baseResult = base.ValidateAndSave();

				if (shouldPromptForPrintReceipt && baseResult == ContinueWithSave.Yes)
				{
					ReceiptPrintPrompt();
				}
			}
			else
			{
				ReceiptBizO.UseReceiptValidation = false;
				ShowMatchingForm();
			}

			return baseResult;
		}

		protected void ShowMatchingForm()
		{
			NewMatchGroupForm matchingForm = new NewMatchGroupForm(ReceiptBizO.MatchingBaseObject);
			matchingForm.HideMatchAndContinueButtonForReceiptPayment();
			ReceiptBizO.IsTopLevel = false;
			matchingForm.Closing += new CancelEventHandler(MatchingForm_Closing);
			matchingForm.FormClosed += MatchingForm_Closed;
			ZFormModaliser.Show(matchingForm, this);
		}

		#endregion

		protected override bool ShowAuditTab => true;

		#region Implementation

		#region ARReceiptBizO_On_DisplayCollectionNotesForm

		void ARReceiptBizO_On_DisplayCollectionNotesForm()
		{
			ZString message = Res.GetString("19ed906b-c09b-48bd-9a07-216ba75a2a2c", "There is an open Collection Call for this A/R Client.") + "\r\n\r\n";
			message += Res.GetString("f6443cdf-dbf5-4818-959e-995168f43ab7", "Do you want to view the Collection Calls for this A/R Client?");

			if (Globals.IsTest || Globals.Message.Show(message, Res.GetString("b56d58fb-bb21-4e6c-9f5e-30139539cbbc", "Collection Calls"), MessageBoxButtons.YesNo, DialogResult.Yes) == DialogResult.Yes)
			{
				BusinessObjectFactory newFactory = new BusinessObjectFactory();
				OrgHeader orgHeaderBiz = newFactory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, ARReceiptBizO.Header.PK));
				ZFormModaliser.ShowDialogAndDispose(new CollectionNotesForm(orgHeaderBiz));
			}
		}

		#endregion

		#region Event Handlers

		void ReceiptDetailButton_Click(object sender, EventArgs e)
		{
			ReceiptBizO.IsPostWithMatching = true;
		}

		void WorkflowTabPage_InitializeTab(object sender, EventArgs e)
		{
			WorkflowTabPage.SuspendLayout();
			WorkflowTabPage.ResumeLayout(false);
			WorkflowTabPage.PerformLayout();
		}

		#endregion

		void ReceiptBizO_HasChangesChanged(object sender, EventArgs e)
		{
			SetApplyButtonText();
		}

		void SetApplyButtonText()
		{
			var hasChangesOnlyInChildren = BusinessEntity != null && BusinessEntity.HasChanges && !BusinessEntity.HasChangesNotIncludingChildren;

			if (BusinessEntity != null && BusinessEntity.IsInDatabaseIncludingChildren && !hasChangesOnlyInChildren)
			{
				PostWithoutMatchingButton.Text = ZFormPostingButtonsStrategy.NewButtonText(this).Text;
				PostWithoutMatchingButton.IsCaptionOverridden = true;
				ReceiptDetailButton.Visible = false;
			}
			else if (DisplayMode == ODisplayMode.Delete)
			{
				PostWithoutMatchingButton.Text = ZFormPostingButtonsStrategy.DeleteButtonText(this).Text;
				PostWithoutMatchingButton.IsCaptionOverridden = true;
				ReceiptDetailButton.Visible = false;
			}
			else if (ReceiptBizO.IsInDatabase && hasChangesOnlyInChildren)
			{
				PostWithoutMatchingButton.Text = ZFormPostingButtonsStrategy.ApplyButtonText(this).Text;
				PostWithoutMatchingButton.IsCaptionOverridden = true;
			}
		}

		#region BusinessEntity as ...

		ARReceipt ARReceiptBizO
		{
			get { return BusinessEntity as ARReceipt; }
		}

		Receipt ReceiptBizO
		{
			get { return BusinessEntity as Receipt; }
		}

		#endregion

		#region OnLoad

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (DisplayMode == ODisplayMode.Delete)
			{
				CloseButton.Text = ZFormPostingButtonsStrategy.CancelButtonText(this).Text;
			}

			SetApplyButtonText();

			UnmatchDateEdit.Visible = ReceiptBizO.UnmatchingData.WasUnmatched;
			if (UnmatchDateEdit.Visible != AH_NumberOfSupportingDocumentsCalcEdit.Visible)
			{
				if (UnmatchDateEdit.Visible)
				{
					ControlDpiScalingHelper.SetLeft(ref UnmatchDateEdit, ReceiptNoTextBox.Left, false);
				}
				else if (AH_NumberOfSupportingDocumentsCalcEdit.Visible)
				{
					ControlDpiScalingHelper.SetLeft(ref AH_NumberOfSupportingDocumentsCalcEdit, ReceiptNoTextBox.Left, false);
				}
			}
		}

		#endregion

		#region DisplayModeChanged

		void ReceiptForm_DisplayModeChanged(object sender, DisplayModeChangedEventArgs e)
		{
			if (e.ToMode == ODisplayMode.Delete)
			{
				ReceiptDetailButton.FlatStyle = FlatStyle.Standard;
				ReceiptDetailButton.ForeColor = Color.White;
				ReceiptDetailButton.BackColor = Color.Crimson;
			}
			else if (e.ToMode == ODisplayMode.ReadOnly)
			{
				ReceiptDetailButton.Enabled = false;
			}
			DocManagerReadOnlyOverrideHelper.TrySetReadOnlyOverride(BusinessEntity, e.ToMode);
		}

		#endregion

		#region MatchingForm_Closing

#if DEBUG
		internal
#endif
		void MatchingForm_Closing(object sender, CancelEventArgs e)
		{
			if (!e.Cancel)
			{
				OnClosingMatchingForm((NewMatchGroupForm)sender);
			}
		}

		void MatchingForm_Closed(object sender, FormClosedEventArgs e)
		{
			if (ReceiptBizO.MatchingBaseObject.Corrupted)
			{
				DisplayMode = ODisplayMode.ReadOnly;
				base.SetReadOnlyIncludingChildren();
				ReceiptDetailButton.Enabled = false;
				PostWithoutMatchingButton.Enabled = false;
				Globals.Message.ShowWarning(Res.GetString("4B1C61B8-6E86-4FB9-BE3F-0B7F337DC905", "An error has occurred. Your unsaved work must be re-entered.\r\n\r\nPlease close the form in which you were working and re-enter the data."));
			}
		}

		void OnClosingMatchingForm(NewMatchGroupForm newMatchGroupForm)
		{
			if (ReceiptBizO != null)
			{
				ReceiptBizO.IsPostWithMatching = false;
				ReceiptBizO.IsTopLevel = true;
				if (newMatchGroupForm.SaveFactoryResult == SaveFactoryFlag.OK)
				{
					DisplayMode = ODisplayMode.Browse;
					ReceiptPrintPrompt();
				}
				else
				{
					ReceiptBizO.UseReceiptValidation = true;
					((IMatching)ReceiptBizO).ChequeOrReference_ReadOnly = false;
				}
			}
		}

		#endregion

		void ReceiptPrintPrompt()
		{
			if (ARReceiptBizO != null)
			{
				if (AccountingConfigurationRegistry.Instance.AccountingReceiptPrintPrompting.Value)
				{
					if (Globals.Message.Show(Res.GetString("a3498391-87c9-40a3-b6ec-87c885c3edcd", "This option can be turned off via the following Registry: Accounting > Receivable Defaults > Default Settings > Receipt Print Prompting.\r\n\r\nDo you want to print receipt now?"), Res.GetString("4649121b-31a2-4921-8504-03c52c7c4196", "Print Receipt"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
					{
						ReceiptPrint printHandler = new ReceiptPrint();
						printHandler.PrintReceiptMatchingReport(ARReceiptBizO, AccountingUtils.AccountingDocumentTitles.ReceiptJournal);
					}
				}
			}
		}

		#endregion

		#region IDisposable Members

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (ReceiptBizO != null)
				{
					ReceiptBizO.HasChangesChanged -= ReceiptBizO_HasChangesChanged;
				}
				if (ARReceiptBizO != null)
				{
					ARReceiptBizO.On_DisplayCollectionNotesForm -= ARReceiptBizO_On_DisplayCollectionNotesForm;
				}
			}
			base.Dispose(disposing);
		}

		#endregion

		#region IAccessReceiptFormInternalsForTest Members
#if DEBUG
		Core.Forms.ZPostOrCancelButton IAccessReceiptFormInternalsForTest.CloseButton
		{
			get { return this.CloseButton; }
		}

		void IAccessReceiptFormInternalsForTest.OnApplyButtonClick(object sender, EventArgs e)
		{
			this.OnApplyButtonClick(sender, e);
		}

		Core.Forms.ZPostOrCancelButton IAccessReceiptFormInternalsForTest.PostButton
		{
			get { return this.ReceiptDetailButton; }
		}

		ZTextBox IAccessReceiptFormInternalsForTest.ChequeNoTextBox
		{
			get { return this.ChequeNoTextBox; }
		}

		IButton IAccessReceiptFormInternalsForTest.fApplyButton
		{
			get { return this.fApplyButton; }
		}

		ZGuidFindBox IAccessReceiptFormInternalsForTest.OrganisationGuidFindBox
		{
			get { return this.OrganisationGuidFindBox; }
		}

#endif
		#endregion
	}
}

