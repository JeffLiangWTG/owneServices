using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Printing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public partial class PeriodicInvoicingForm : AccountingZForm, IAllowTabBackwardBetweenSomeOfMyChildren
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public PeriodicInvoicingForm()
		{
		}

		public PeriodicInvoicingForm(PeriodicInvoice periodicInvoice)
			: base(periodicInvoice)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, PostAndCloseButton, CloseButton, PostButton);
		}

		new PeriodicInvoice BusinessEntity
		{
			get { return base.BusinessEntity as PeriodicInvoice; }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			this.periodicInvoiceControl.BackColor = BackColor;

			if (!AccountingConfigurationRegistry.Instance.EnableMiscInvoiceInPeriodicInvoice.Value)
			{
				periodicInvoiceControl.TabControl.Controls.Remove(periodicInvoiceControl.MiscInvoicesTabPage);
			}

			if (BusinessEntity != null && !BusinessEntity.IsInDatabase)
			{
				PreviewInvoiceMenuItem = ZFormMenuStrategy.AddActionsMenuItem(this, ResString.GetMultilingualString("Accounting|PeriodicInvoicingForm|PreviewInvoiceMenuName", "Preview Invoice"), PreviewInvoice_Click);
			}

			if (!AccountingMasterFilesUtils.IsTaxBranchApplicable)
			{
				TaxBranchFindBox.Visible = false;
			}

			SellReferenceTextBox.AllowOverlap(jobTypeSelectionControl1);
		}

		#region IAllowTabBackwardBetweenSomeOfMyChildren Members

		bool IAllowTabBackwardBetweenSomeOfMyChildren.AllowTabBackward(Control control, Control previousControl)
		{
			return control == DateEdit && previousControl == PostDateEdit;
		}

		#endregion

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			periodicInvoiceControl.PreparePeriodicInvoiceControl();
		}

		JobTypeSelectionControl jobTypeSelectionControl1;
		ZArchitecture.ZTextBox SellReferenceTextBox;
		protected ZCheckedListBox JobTypeCheckedListBox;

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			return (base.ShowPreSaveDialogs() == ContinueWithSave.No) ? ContinueWithSave.No : CreatePeriodicInvoiceAndCheckSecurityRights();
		}

		ContinueWithSave CreatePeriodicInvoiceAndCheckSecurityRights()
		{
			var result = ContinueWithSave.No;

			if (BusinessEntity.MiscInvoices.Any())
			{
				var createResult = BusinessEntity.CreateTransactions();
				HookInvoiceOnNegativeComplianceFailedToCreate(BusinessEntity.PostManager.Poster.PostedInvoices);
				return createResult ? ContinueWithSave.Yes : ContinueWithSave.No;
			}
			else
			{
				// Note: validation is disabled in periodicInvoiceInNewFactory because it has already run in RunPreSaveValidation
				var periodicInvoiceInNewFactory = BusinessEntity.PreparePeriodicInvoiceInNewFactoryForAuthorizationCheck();
				periodicInvoiceInNewFactory.PostManager.OnCriticalPostError += PeriodicInvoiceErrorHandleHelper.OnCriticalPostError;
				if (periodicInvoiceInNewFactory.CreateTransactions())
				{
					var periodicCreditNoteInNewFactory = periodicInvoiceInNewFactory.PostManager.Poster.PostedInvoices.Cast<InvoicingBase>().FirstOrDefault(x => x.AH_TransactionType == TransactionTypes.CreditNote);
					var securityProvider = new InvoicingSecurityOverrideProvider(periodicCreditNoteInNewFactory);
					if (CheckLevelSecurityRightsForPeriodicCreditNote(periodicCreditNoteInNewFactory, securityProvider))
					{
						try
						{
							BusinessEntity.PostManager.OnCriticalPostError += PeriodicInvoiceErrorHandleHelper.OnCriticalPostError;
							BusinessEntity.CreateTransactions(); // create the "real" one now
							var postedInvoices = BusinessEntity.PostManager.Poster.PostedInvoices;
							HookInvoiceOnNegativeComplianceFailedToCreate(postedInvoices);
							var creditNotesApproved = postedInvoices.OfType<ARCreditNote>();
							if (creditNotesApproved.Any())
							{
								List<ZGuid> getApprovingDetails(ARCreditNote creditNote)
								{
									return creditNote.EnforceTwoApproversWhenPostingARCredit || creditNote.EnforceSequentialApproversWhenPostingARCredit
										? securityProvider.UserPKsForTwoCredentialLogin ?? new List<ZGuid>()
										: new List<ZGuid> { securityProvider.UserSecurityOverride?.UserPK ?? ZGuid.Empty };
								}

								creditNotesApproved.ForEach(x => x.ApprovingUserPKList = getApprovingDetails(x));
							}
							result = ContinueWithSave.Yes;
						}
						finally
						{
							periodicInvoiceInNewFactory.PostManager.OnCriticalPostError -= PeriodicInvoiceErrorHandleHelper.OnCriticalPostError;
						}
					}
				}
				return result;
			}
		}

		void HookInvoiceOnNegativeComplianceFailedToCreate(InvoicingBaseCollection transactions)
		{
			foreach (InvoicingBase invoicingBase in transactions)
			{
				if (invoicingBase is ARInvoice || invoicingBase is ARCreditNote)
				{
					invoicingBase.OnNegativeCompliancesFailedToCreate += new EventHandler(NegativeCompliancesFailedToCreate);
				}
			}
		}

		void NegativeCompliancesFailedToCreate(object sender, EventArgs e)
		{
			Globals.Message.ShowWarning(AccountingConstants.GetComplianceDocumentNegativeMessage());
		}

		bool CheckLevelSecurityRightsForPeriodicCreditNote(InvoicingBase periodicCreditNote, InvoicingSecurityOverrideProvider securityProvider)
		{
			var result = true;
			if (periodicCreditNote != null)
			{
				if (!new InvoicingPreSaveHelper().PreSaveActionsForPeriodicInvoice(periodicCreditNote, securityProvider, UpdateSecurityProviderMode))
				{
					result = false;
					Globals.Message.ShowError(Res.GetString("3A83E00F-2C7D-4A8B-B08B-DC2DB28A4E7C", "You do not have security rights to post a credit note for the required amount. User with a higher Credit/Adjustment Note Approval Level can post this transaction."));
				}
			}
			return result;
		}

		void UpdateSecurityProviderMode(InvoicingBase periodicCreditNote)
		{
			var invoicingSecurityOverrideProvider = periodicCreditNote.SecurityOverrideProvider as InvoicingSecurityOverrideProvider;
			if (invoicingSecurityOverrideProvider != null)
			{
				invoicingSecurityOverrideProvider.RequiresTwoApprovers = periodicCreditNote.TransactionsWithUniqueBranchDepartmentForAuthorisationCalculation.Any(x => x.EnforceTwoApproversWhenPostingARCredit);
				invoicingSecurityOverrideProvider.RequiresSequentialApprovals = periodicCreditNote.TransactionsWithUniqueBranchDepartmentForAuthorisationCalculation.Any(x => x.EnforceSequentialApproversWhenPostingARCredit);
			}
		}

		void PreviewInvoice_Click(object sender, EventArgs e)
		{
			var helper = new JobInvoicingSecurityHelper(Env.Security.NewReceivablesPeriodicInvoice, false);
			var allowedToPreviewInvoice = helper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.PreviewOnly);
			var allowedToPreviewAndDeliverInvoice = helper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.PreviewAndDeliver);

			if (!allowedToPreviewInvoice)
			{
				helper.ShowError(SecurityCore.PreviewOnly);
				return;
			}

			if (!BusinessEntity.HasErrors)
			{
				var previewErrors = BusinessEntity.PreviewPeriodicInvoiceAndReturnErrors(!allowedToPreviewAndDeliverInvoice, PeriodicInvoiceErrorHandleHelper.OnCriticalPostError);

				if (previewErrors.Any())
				{
					Globals.Message.ShowError(Res.GetString("3ab38061-df35-4df5-b7c7-dae24e9e02da",
						"This invoice can't be previewed due to the following:\r\nSince this invoice screen was opened, one of the billing lines included in this invoice has been changed.\r\n{0}\r\nYou will need to cancel this screen and begin the periodic invoice process again.",
						string.Join("\r\n", previewErrors)));
				}
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("A92F30CF-A41A-4c9d-A12A-BD6B7C6EBF6E", "Invoice cannot be previewed until errors are rectified."));
			}
		}

#if DEBUG
		PeriodicInvoicePostManager LastPostManagerForTestOnly;
#endif

		protected override ContinueWithSave ValidateAndSave()
		{
			ContinueWithSave result = ContinueWithSave.No;
			if (!LastSaveSuccessful)
			{
				Globals.Message.ShowError(Res.GetString("85f5a526-5c0b-43bf-8d8c-809e4222eefd",
				@"The saving process has encountered an unrecoverable error. 
Please retry the action after closing and reopening the form."));
			}
			else
			{
				try
				{
					BusinessEntity.ResetPostManager();
					BusinessEntity.PostManager.OnCriticalPostError += new EventHandler<CriticalPostingErrorEventArgs>(PeriodicInvoiceErrorHandleHelper.OnCriticalPostError);
					result = base.ValidateAndSave();
					if (BusinessEntity.PostManager.CancelPosting)
					{
						result = ContinueWithSave.No;
					}
#if DEBUG
					if (Globals.IsTest)
					{
						LastPostManagerForTestOnly = BusinessEntity.PostManager;
					}
#endif
				}
				finally
				{
					BusinessEntity.PostManager.OnCriticalPostError -= new EventHandler<CriticalPostingErrorEventArgs>(PeriodicInvoiceErrorHandleHelper.OnCriticalPostError);
				}
			}

			if (result == ContinueWithSave.Yes)
			{
				SetReadOnlyIncludingChildren();
				PromptForPrintInvoices();
			}

			return result;
		}

		protected override void HandleSaveException(Exception e)
		{
			if (e is ComplianceSequenceRelatedException complianceSequenceEx)
			{
				Globals.Message.ShowError(complianceSequenceEx.UserFriendlyMessage, Res.GetString("473A81F5-3C66-4F19-B742-EEB6FBF466C0", "Compliance sequence error"));
			}
			else
			{
				base.HandleSaveException(e);
			}
		}

		void PromptForPrintInvoices()
		{
			InvoicePrinter printer = null;

			foreach (InvoicingBase postedInvoice in BusinessEntity.PostManager.Poster.PostedInvoices)
			{
				(printer ?? (printer = new InvoicePrinter())).PrintTransaction(postedInvoice, this, InvoicePrintContext.DontCare);
			}
		}

		protected override void SetReadOnlyIncludingChildren()
		{
			base.SetReadOnlyIncludingChildren();

			JobTypeCheckedListBox.Enabled = false;
		}

		internal MenuItem PreviewInvoiceMenuItem;
	}
}

