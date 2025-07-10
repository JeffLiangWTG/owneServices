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
using Enterprise.Accounting.Business.ARAP.EPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.GUI.ARAP.ReceiptPayment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using PaymentBase = Enterprise.Accounting.Business.ARAP.ReceiptPayment.Payment;

namespace Enterprise.Accounting.GUI.ARAP
{
	public partial class PaymentForm : AccountingZForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public PaymentForm()
		{
		}

		public PaymentForm(PaymentBase paymentBizO)
			: base(paymentBizO)
		{
			SetupPostingButtons();
			AH_NumberOfSupportingDocumentsCalcEdit.Visible = paymentBizO.AH_NumberOfSupportingDocumentsVisible_ReadOnly;
			PlugIns.Add(ControllerIDs.eDocsPlugIn);
			HookEvents();

			WorkflowTabPage.Initialize(Payment);
		}

		void InitializeWorkflowTabPage(IEnumerable<string> args)
		{
			if (args != null && args.FirstOrDefault() != ModuleIDs.APTransaction.ToString() && args.FirstOrDefault() != ModuleIDs.ARTransaction.ToString())
			{
				WorkflowTabPage.TabVisible = false;
			}
		}

		#region Hook and Unhook Events

		void HookEvents()
		{
			if (Payment != null)
			{
				Payment.HasChangesChanged += Payment_HasChangesChanged;
				if (Payment.RelatedPaymentApproval != null)
				{
					Payment.RelatedPaymentApproval.CurrencyCodeInfo.ValueChanged += (sender, e) => EnableOrDisableCheckEPayRateButton();
				}
				Payment.AH_ReceiptTypeInfo.ValueChanged += AH_ReceiptTypeInfo_ValueChanged;
			}
			DisplayModeChanged += PaymentForm_DisplayModeChanged;
			FormLoadedWithArgs += (s, e) => InitializeWorkflowTabPage(e.Args);
		}

		void UnhookEvents()
		{
			if (Payment != null)
			{
				Payment.HasChangesChanged -= Payment_HasChangesChanged;
				if (Payment.RelatedPaymentApproval != null)
				{
					Payment.RelatedPaymentApproval.CurrencyCodeInfo.ValueChanged -= (sender, e) => EnableOrDisableCheckEPayRateButton();
				}
				Payment.AH_ReceiptTypeInfo.ValueChanged -= AH_ReceiptTypeInfo_ValueChanged;
			}
			DisplayModeChanged -= PaymentForm_DisplayModeChanged;
		}

		#endregion

		#region Overrides

		#region FormCaption

		public override string FormCaption
		{
			get { return Payment.DefaultDescription; }
		}

		#endregion

		#region OnLoad

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (DisplayMode == ODisplayMode.ReadOnly && (!Payment.ChequeBookIsVisible))
			{
				HideChequeControl();
			}
			if (DisplayMode == ODisplayMode.Delete)
			{
				CloseButton.Text = ZFormPostingButtonsStrategy.CancelButtonText(this).Text;
			}

			SetApplyButtonText();

			UnmatchDateEdit.Visible = Payment != null && Payment.UnmatchingData.WasUnmatched;

			if (!DesignMode)
			{
				var allowEPayment = AccountingMasterFilesRegistry.Instance.EnableEPaymentFunctionality.Value.IsEPaymentEnabledForAnyProvider;
				EPaymentTabPage.TabVisible = allowEPayment;
				CheckEPayRateButton.Visible = allowEPayment;
				ProcessEPaymentButton.Visible = allowEPayment;
				if (CheckEPayRateButton.Visible)
				{
					EnableOrDisableCheckEPayRateButton();
				}
				AH_ReceiptTypeInfo_ValueChanged(this, null);
			}
		}

		void AH_ReceiptTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			if (Payment != null)
			{
				PaymentReasonDropEdit.Visible = Payment.IsEPayment;
				FundingBankAccountFindBox.Visible = Payment.IsEPayment;
				FundingCurrencyCodeFindBox.Visible = Payment.IsEPayment;
				UpdateAccountCaption(Payment);
			}
		}

		void UpdateAccountCaption(PaymentBase payment)
		{
			if (payment.IsEPayment)
			{
				BankAccountGuidFindBox.CaptionResourceString = Res.GetData("f17a9a96-2134-48cf-8d34-1ba18739d2b2", "E-Payment Account");
			}
			else
			{
				BankAccountGuidFindBox.CaptionResourceString = Res.GetData("51692eae-8540-4b05-8a28-8ae9a9759afa", "Bank Account");
			}
			BankAccountGuidFindBox.UpdateCaption();
		}

		void EnableOrDisableCheckEPayRateButton() => CheckEPayRateButton.Enabled = Payment?.RelatedPaymentApproval?.CanCreateQuotes ?? false;

		#endregion

		#region ShowPreSaveDialogs Override

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			ContinueWithSave baseResult = base.ShowPreSaveDialogs();

			if (baseResult == ContinueWithSave.Yes && !Payment.IsInDatabase)
			{
				if (!CalledFromMatchingFormClosing)
				{
					// Popup MatchingForm
					MatchingBase paymentMatcher = (Payment.MatchingBaseObject);
					fMatchingForm = new NewMatchGroupForm(paymentMatcher);
					fMatchingForm.HideMatchAndContinueButtonForReceiptPayment();
					fMatchingForm.Closing += new CancelEventHandler(fMatchingForm_Closing);
					ZFormModaliser.Show(fMatchingForm, this);
					SetCalledFromMatchingFormClosingTrue();
					baseResult = ContinueWithSave.No;
				}
				else // user made successful match - allow the save 
				{
					SetCalledFromMatchingFormClosingFalse();
				}
			}

			return baseResult;
		}

		#endregion

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				UnhookEvents();
			}

			base.Dispose(disposing);
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		protected override bool ShowAuditTab => true;

		#endregion

		#region Implementation

		PaymentBase Payment
		{
			get { return BusinessEntity as PaymentBase; }
		}

		#region Setup Posting Buttons

		protected virtual void SetupPostingButtons()
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, null, CloseButton, PaymentDetailButton);
		}

		ZPanel BottomPanel;
		protected Core.Forms.ZPostOrCancelButton PaymentDetailButton;
		protected Core.Forms.ZPostOrCancelButton CloseButton;
		ZDropEdit PaymentReasonDropEdit;

		#endregion

		#region Matching Form

		NewMatchGroupForm fMatchingForm;

		void fMatchingForm_Closing(object sender, CancelEventArgs e)
		{
			// Precondition: CalledFromMatchingFormClosing must have been set to True earlier in ValidateAndSave();

			if (fMatchingForm.SaveFactoryResult == SaveFactoryFlag.OK) // user clicked save in Matching Form, run ValidateAndSave on the PaymentForm for the second time
			{
				ContinueWithSave payFormResult = ValidateAndSave();

				if (payFormResult == ContinueWithSave.Yes) // set the DisplayMode and buttonText to 'New'
				{
					PrintManager.Print();

					this.SetReadOnlyIncludingChildren();
					this.DisplayMode = ZArchitecture.Core.ODisplayMode.Browse;

					this.fApplyButton.Enabled = true;
					this.fApplyButton.Visible = true;
					this.fApplyButton.Text = ZFormPostingButtonsStrategy.NewButtonText(this).Text;

					this.fCancelButton.Enabled = true;
				}
			}
			else // have to return to still unsaved Payment form
			{
				SetCalledFromMatchingFormClosingFalse();
				PaymentBase paymentBizO = BusinessEntity as PaymentBase;
				if (paymentBizO.AH_ChequeOrReferenceInfo.ReadOnly)
				{
					((IMatching)paymentBizO).ChequeOrReference_ReadOnly = false;
				}
				MatchingBase matchingBizO = fMatchingForm.BusinessEntity as MatchingBase;
				matchingBizO.DeleteCachedMiscTransactions();
				matchingBizO.RemoveAllInMatchedTransactions();
			}
		}

		#endregion

		#region MatchedByUser

		void SetCalledFromMatchingFormClosingTrue()
		{
			MatchedByUserSemaphore++;
		}

		void SetCalledFromMatchingFormClosingFalse()
		{
			MatchedByUserSemaphore--;
		}

		ZBool CalledFromMatchingFormClosing
		{
			get { return MatchedByUserSemaphore > 0; }
		}

		int MatchedByUserSemaphore;

		#endregion

		#region Payment Printing

#if DEBUG
		protected virtual
#endif
 PaymentPrintManager PrintManager
		{
			get
			{
				if (fPrintManager == null)
				{
					fPrintManager = new PaymentPrintManager(BusinessEntity.Identifier.ToGuid(), ZArchitecture.Core.TransactionTypes.Payment, BusinessEntity.Factory);
				}
				return fPrintManager;
			}
		}

		PaymentPrintManager fPrintManager;

		#endregion

		#region HideChequeControl

		void HideChequeControl()
		{
			ChequeBookGuidFindBox.Visible = false;
			ControlDpiScalingHelper.SetTop(ref ChequeNoTextBox, ChequeBookGuidFindBox.Top, false);

			ControlDpiScalingHelper.SetHeight(ref BankDetailsGroupBox, BankDetailsGroupBox.Height - (ChequeBookGuidFindBox.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(4)), false);
			ControlDpiScalingHelper.SetTop(ref PaymentAmountGroupBox, BankDetailsGroupBox.Top + BankDetailsGroupBox.Height, false);
			ControlDpiScalingHelper.SetHeight(ref TabControl, TabControl.Height - (ChequeBookGuidFindBox.Height), false);
		}

		#endregion

		#region DisplayModeChanged

		protected virtual void PaymentForm_DisplayModeChanged(object sender, DisplayModeChangedEventArgs e)
		{
			if (e.ToMode == ODisplayMode.Delete)
			{
				PaymentDetailButton.FlatStyle = FlatStyle.Standard;
				PaymentDetailButton.ForeColor = Color.White;
				PaymentDetailButton.BackColor = Color.Crimson;
			}
			DocManagerReadOnlyOverrideHelper.TrySetReadOnlyOverride(BusinessEntity, e.ToMode);
		}

		#endregion

		void Payment_HasChangesChanged(object sender, EventArgs e)
		{
			SetApplyButtonText();
		}

		void WorkflowTabPage_InitializeTab(object sender, EventArgs e)
		{
			WorkflowTabPage.SuspendLayout();
			WorkflowTabPage.ResumeLayout(false);
			WorkflowTabPage.PerformLayout();
		}

		void SetApplyButtonText()
		{
			if (fApplyButton != null)
			{
				string applyButtonText;
				if (BusinessEntity != null && BusinessEntity.IsInDatabaseIncludingChildren)
				{
					applyButtonText = Payment.HasChanges ? ZFormPostingButtonsStrategy.ApplyButtonText(this).Text : ZFormPostingButtonsStrategy.NewButtonText(this).Text;
				}
				else
				{
					applyButtonText = DisplayMode == ODisplayMode.Delete ? ZFormPostingButtonsStrategy.DeleteButtonText(this).Text : ZFormPostingButtonsStrategy.ApplyButtonText(this).Text;
				}
				fApplyButton.Text = applyButtonText;
			}
		}

		void CheckEPayRateButton_Click(object sender, EventArgs e)
		{
			var approval = Payment?.RelatedPaymentApproval;
			if (approval == null)
			{
				return;
			}

			try
			{
				if (!approval.HasChanges)
				{
					var (quote, errorMessage, quoteStatus) = approval.TryToCreateQuote(EPaymentProviderCodes.Codes.OFX);
					if (quoteStatus != EPaymentDealCreator.QuoteAcceptingStatus.NoErrors)
					{
						switch (quoteStatus)
						{
							case EPaymentDealCreator.QuoteAcceptingStatus.ValidationErrors:
								Globals.Message.ShowError(errorMessage);
								break;
							case EPaymentDealCreator.QuoteAcceptingStatus.QuoteRequested:
								var caption = Res.GetString("2f616c4d-5539-49f3-8850-7104b139fa98", "Quote already requested");
								var message = Res.GetString("1988db76-5213-45b9-a32c-9821cc4cd8b2", "E-Quote already requested. Response may take up to several minutes to be received. If you generate a new request, then the previous request will be discarded. Are you sure you want to proceed?");
								var dialogResult = Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
								if (dialogResult == DialogResult.Yes)
								{
									quote = approval.TryToCreateQuote(EPaymentProviderCodes.Codes.OFX, true).Quote;
								}
								break;
						}
					}
					if (quote != null)
					{
						approval.DiscardActiveFXQuotes(quote.QU_ProviderCode, discardOnlyIfPaymentDetailsAreChanged: false);
						quote.Factory.Save();
						Globals.Message.ShowInformation(Res.GetString("72696b1b-4275-4fd5-95aa-527fe833269a", "E-Quote request generated to service provider {0}. Response may take from a few moments up to several minutes to be received.", quote.QU_ProviderCode));
						BankAndEPaymentTabControl.SelectTab("EPaymentTabPage");
						RefreshButton_Click(this, null);
					}
				}
				else
				{
					Globals.Message.ShowInformation(Res.GetString("2f2999a2-e87e-4cd1-9a72-b60abd674260", "Please save your payment first."));
				}
			}
			catch (ZCannotSaveException ex)
			{
				Globals.Message.ShowError(ex.Message);
			}
		}

		void RefreshButton_Click(object sender, EventArgs e)
		{
			Payment?.RelatedPaymentApproval?.RefreshQuotes();
			Payment?.RelatedPaymentApproval?.RefreshCurrentDeal();
			Payment?.RefreshBindingIncludingChildren();
		}

		void AcceptQuoteButton_Click(object sender, EventArgs e)
		{
			if (Payment?.RelatedPaymentApproval != null)
			{
				PaymentApprovalEPaymentHelper.TryToAcceptQuote(Payment.RelatedPaymentApproval, EPaymentGrid.SelectedElements.Cast<EPaymentQuoteForDisplay>().Select(x => x.RealQuote).ToArray());
			}
		}

		void ProcessEPaymentButton_Click(object sender, EventArgs e)
		{
			if (Payment?.RelatedPaymentApproval != null)
			{
				if (PaymentApprovalEPaymentHelper.TryToCreateDeal(Payment?.RelatedPaymentApproval, () => CheckEPayRateButton_Click(this, EventArgs.Empty)))
				{
					BankAndEPaymentTabControl.SelectTab("EPaymentTabPage");
					RefreshButton_Click(sender, e);
				}
			}
		}

		#endregion
	}
}

