using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.GUI.ARAP.AutoAllocationAndPrinting;
using Enterprise.Accounting.GUI.ARAP.ReceiptPayment;
using Enterprise.Accounting.GUI.ARAP.TransactionView;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.Accounting.Business.Base.Matching.CurrencySummaryRowCollection;

namespace Enterprise.Accounting.GUI.ARAP
{
	public partial class PaymentBatchForm : AccountingZForm, IButtonPostTextOverride, IButtonApplyTextOverride
	{
		ZPostOrCancelButton SaveAndCloseButton;
		ZPostOrCancelButton SaveButton;
		CargoWise.Windows.UI.KSplitContainer splitContainer1;
		CargoWise.Windows.UI.KSplitContainer splitContainer2;
		ZLabel AutoAllocateZLabel;
		ZLabel AutoPrintZLabel;
		ZTextBox CardSecurityCodeTextBox;
		ZCheckBox PostPaymentsAsPaymentApprovalsCheckBox;
		ZButton ApplyEXXButton;
		ZButton CheckExRateButton;
		ZButton AcceptQuotesButton;
		AccountingOnFormFilterControl QuoteFilterControl;
		ZPostOrCancelButton CancelPostingButton;
		ZCalcEdit CurrencySummaryTotalEPaymentFeesTextBox;
		ZLabel DisclaimerMessageLabel;
		ZButton LearnMoreButton;
		ZPictureBox ProviderLogoPictureBox;
		ZLabel ServiceProviderLabel;
		ZButton SyncRecipientsButton;
		ZCodeFindBox FundingCurrencyCodeFindBox;
		ZGuidFindBox FundingBankAccountFindBox;
		ZPostOrCancelButton SaveAsDraftButton;

		public PaymentBatchForm(APPaymentBatchPoster paymentBatchPosterBizObj)
			: base(paymentBatchPosterBizObj)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, SaveAndCloseButton, CancelPostingButton, SaveButton);
			HookEvents(paymentBatchPosterBizObj);

			QuoteInfoLabel.AllowOutsideOfParent();
		}

		#region Hook/Unhook Events

		void HookEvents(APPaymentBatchPoster paymentBatchPosterBizObj)
		{
			SaveAndCloseButton.EnabledChanged += SaveAndCloseButton_EnabledChanged;
			SaveAndCloseButton.VisibleChanged += SaveAndCloseButton_VisibleChanged;
			PaymentBatchGrid.ColourDeciding += PaymentBatchGrid_ColourDeciding;
			PaymentBatchGrid.CurrentCellChanged += PaymentBatchGrid_CurrentCellChanged;
			PaymentBatchGrid.ContextMenu.Popup += PaymentBatchGridMenu_Popup;
			PaymentBatchGrid.DoubleClick += HandleDoubleClickPaymentBatchGridTransaction;
			MatchTransactionsGrid.DoubleClick += ViewMatchGridTransaction;
			MatchTransactionsGrid.ContextMenu.Popup += MatchTransactionsGridMenu_Popup;
			paymentBatchPosterBizObj.OnSelectBankAccountFromDefault += PaymentBatchPosterBizObj_OnSelectBankAccountFromDefault;
			paymentBatchPosterBizObj.OnPaymentDeletedFromBatch += OnPaymentDeletedFromBatch;
			paymentBatchPosterBizObj.APB_PaymentTypeInfo.ValueChanged += PaymentTypeInfo_ValueChanged;
			paymentBatchPosterBizObj.CurrencySummary.SummaryRows.OnUpdatePaymentsExchangeRate += new EventHandler<UpdatePaymentsExchangeRateEventArgs>(CurrencySummaryRows_OnUpdatePaymentsExchangeRate);
			paymentBatchPosterBizObj.CurrencySummary.SummaryRows.OnUpdateExchangeRateOfPaymentsWithActiveDeals += new EventHandler<EventArgs>(CurrencySummaryRows_OnUpdateExchangeRateOfPaymentsWithActiveDeals);
			paymentBatchPosterBizObj.APB_ABInfo.ValueChanged += APB_ABInfo_ValueChanged;
		}

		void UnhookEvents()
		{
			SaveAndCloseButton.EnabledChanged -= SaveAndCloseButton_EnabledChanged;
			SaveAndCloseButton.VisibleChanged -= SaveAndCloseButton_VisibleChanged;

			PaymentBatchGrid.ColourDeciding -= PaymentBatchGrid_ColourDeciding;
			PaymentBatchGrid.CurrentCellChanged -= PaymentBatchGrid_CurrentCellChanged;
			PaymentBatchGrid.DoubleClick -= HandleDoubleClickPaymentBatchGridTransaction;
			if (PaymentBatchGrid.ContextMenu != null)
			{
				PaymentBatchGrid.ContextMenu.Popup -= PaymentBatchGridMenu_Popup;
			}
			MatchTransactionsGrid.DoubleClick -= ViewMatchGridTransaction;
			if (MatchTransactionsGrid.ContextMenu != null)
			{
				MatchTransactionsGrid.ContextMenu.Popup -= MatchTransactionsGridMenu_Popup;
			}
			if (PaymentBatchPoster != null)
			{
				PaymentBatchPoster.OnSelectBankAccountFromDefault -= PaymentBatchPosterBizObj_OnSelectBankAccountFromDefault;
				PaymentBatchPoster.OnPaymentDeletedFromBatch -= OnPaymentDeletedFromBatch;
				PaymentBatchPoster.APB_PaymentTypeInfo.ValueChanged -= PaymentTypeInfo_ValueChanged;
				PaymentBatchPoster.CurrencySummary.SummaryRows.OnUpdatePaymentsExchangeRate -= new EventHandler<UpdatePaymentsExchangeRateEventArgs>(CurrencySummaryRows_OnUpdatePaymentsExchangeRate);
				PaymentBatchPoster.CurrencySummary.SummaryRows.OnUpdateExchangeRateOfPaymentsWithActiveDeals -= new EventHandler<EventArgs>(CurrencySummaryRows_OnUpdateExchangeRateOfPaymentsWithActiveDeals);
				PaymentBatchPoster.APB_ABInfo.ValueChanged -= APB_ABInfo_ValueChanged;

				if (PaymentBatchGrid.ListManager != null && SelectedPayment != null && SelectedMatchingBase != null)
				{
					SelectedMatchingBase.OSOverpaymentAmountInfo.ValueChanged -= OSOverpaymentAmountInfo_ValueChanged;
					SelectedMatchingBase.ExchangeDifferenceAmountInfo.ValueChanged -= ExchangeDifferenceAmountInfo_ValueChanged;
					SelectedMatchingBase.DiscountAmountInfo.ValueChanged -= DiscountAmountInfo_ValueChanged;
				}
			}
		}

		#endregion

		#region Payment Printing

#if DEBUG
		protected virtual
#endif
		PaymentBatchPrintManager PrintManager
		{
			get
			{
				if (fPrintManager == null)
				{
					fPrintManager = new PaymentBatchPrintManager(PaymentCollectionForPrinting, FirstApproval, TransactionTypes.Payment, Factory);
				}
				return fPrintManager;
			}
		}

		PaymentBatchPrintManager fPrintManager;

		#endregion

		#region Event Handlers

		void APB_ABInfo_ValueChanged(object sender, EventArgs e)
		{
			var shouldDisplayAdditionalEPaymentControls = PaymentBatchPoster.BankAccount?.IsEPaymentAccount ?? false;
			var paymentProviderCode = PaymentBatchPoster.BankAccount?.AB_PaymentProvider ?? ZString.Empty;

			LearnMoreButton.Visible = shouldDisplayAdditionalEPaymentControls;
			DisclaimerMessageLabel.Visible = shouldDisplayAdditionalEPaymentControls;
			DisclaimerMessageLabel.Text = shouldDisplayAdditionalEPaymentControls ? PaymentApprovalEPaymentHelper.GetDisclaimerMessage(paymentProviderCode) : string.Empty;
			ServiceProviderLabel.Visible = shouldDisplayAdditionalEPaymentControls;
			ProviderLogoPictureBox.Visible = shouldDisplayAdditionalEPaymentControls;
			ProviderLogoPictureBox.Image = shouldDisplayAdditionalEPaymentControls ? EPaymentProviderLogoFinder.FindEPaymentProviderLogo(paymentProviderCode) : null;
		}

		void ProviderLogoPictureBox_Click(object sender, EventArgs e)
		{
			var paymentProviderCode = PaymentBatchPoster.BankAccount?.AB_PaymentProvider ?? ZString.Empty;
			EPaymentUrlLauncher.LaunchEPaymentProviderURL(paymentProviderCode);
		}

		void LearnMoreButton_Click(object sender, EventArgs e) => EPaymentUrlLauncher.LaunchEPaymentProductMarketingURL();

		void CurrencySummaryRows_OnUpdateExchangeRateOfPaymentsWithActiveDeals(object sender, EventArgs e)
		{
			Globals.Message.ShowError(PaymentApprovalBase.ActiveDealErrorTextForChangingPaymentDetails);
		}

		void CurrencySummaryRows_OnUpdatePaymentsExchangeRate(object sender, UpdatePaymentsExchangeRateEventArgs e)
		{
			var result = Globals.Message.Show(Res.GetString("0a2cac04-af2c-4ea9-8f12-1f937b6505bd", "Update all {0} payment(s) with exchange rate {1}?", e.Currency.RX_Code, Utilities.Round(e.ExchangeRate, Env.CurrentCompany.ExchangeRate.RateDecimals)),
					Res.GetString("6869e00f-f260-406a-9dc5-71cb43a90132", "Update payments exchange rate confirmation"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);

			e.Answer = result == DialogResult.Yes;
		}

		void PaymentTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			CardSecurityCodeTextBox.Visible = PaymentBatchPoster.APB_PaymentType == ReceiptTypes.eNettCreditCard;
			if (PaymentBatchPoster.APB_PaymentType == ReceiptTypes.EPayment)
			{
				BankAccountGuidFindBox.CaptionResourceString = Res.GetData("f17a9a96-2134-48cf-8d34-1ba18739d2b2", "E-Payment Account");
			}
			else
			{
				BankAccountGuidFindBox.CaptionResourceString = Res.GetData("51692eae-8540-4b05-8a28-8ae9a9759afa", "Bank Account");
			}
			BankAccountGuidFindBox.UpdateCaption();
			SetEPaymentFieldVisibility();
			if (previousAPB_PaymentType != PaymentBatchPoster.APB_PaymentType)
			{
				SetAvailableColumns();
				previousAPB_PaymentType = PaymentBatchPoster.APB_PaymentType;
			}
		}

		ZGuid PaymentBatchPosterBizObj_OnSelectBankAccountFromDefault(AccBankAccountCollection bankAccountCollection)
		{
			BankAccountSelectionObject bankAccountSelection = new BankAccountSelectionObject(bankAccountCollection);
			ZFormModaliser.ShowDialogAndDispose(new PaymentBatchBankSelectionForm(bankAccountSelection));
			return bankAccountSelection.SelectedBankAccount;
		}

		void SaveAndCloseButton_EnabledChanged(object sender, EventArgs e)
		{
			SaveAsDraftButton.Enabled = SaveAndCloseButton.Enabled;
		}

		void SaveAndCloseButton_VisibleChanged(object sender, EventArgs e)
		{
			SaveAsDraftButton.Visible = SaveAndCloseButton.Visible;
		}

		void PaymentBatchGrid_ColourDeciding(object sender, ColourDecidingEventArgs e)
		{
			e.Colour = GetColorForPayment((PaymentApprovalBase)e.ObjectAtRow);
		}

		void PaymentBatchGrid_CurrentCellChanged(object sender, EventArgs e)
		{
			if (!SuspendPaymentDetailsRefresh)
			{
				OnSelectedPaymentChanged();
			}
		}

		void OnPaymentDeletedFromBatch(object sender, EventArgs e)
		{
			var shouldKeepForm = true;
			if (!PaymentBatchPoster.ISPaymentBatchEmpty)
			{
				PaymentBatchGrid.Select(0);
			}
			else
			{
				shouldKeepForm = PaymentBatchPoster.IsInDatabase;
			}

			if (shouldKeepForm)
			{
				OnSelectedPaymentChanged();
				PaymentBatchPoster.RefreshBinding();
			}
			else
			{
				CloseAndShowEmptyPaymenyProcessing();
			}
		}

		void OverpaymentButton_Click(object sender, EventArgs e)
		{
			if (SelectedPayment == null)
			{
				return;
			}

			if (SelectedPayment.IsCancelledOrIsPosted)
			{
				Globals.Message.Show(Res.GetString("6D7D2459-644E-43B5-B56A-D680A22F7ABD", "You cannot create an Over Payment on this payment approval since it's status is {0}.",
					SelectedPayment.Lookups.AV_StatusList.GetDescriptionFromCode(SelectedPayment.AV_Status)));
			}
			else if (SelectedMatchingBase.IsOverPaymentAllowedInThisMatchingSession)
			{
				HandleMiscellaneousTransaction(ZArchitecture.Core.TransactionTypes.Overpayment);
			}
			else
			{
				Globals.Message.Show(string.Format((NoResString)"You cannot create an OverPayment in this matching session")); // developer only string
			}
		}

		void DiscountButton_Click(object sender, EventArgs e)
		{
			if (SelectedPayment == null)
			{
				return;
			}

			if (SelectedPayment.IsCancelledOrIsPosted)
			{
				Globals.Message.Show(Res.GetString("17969D92-1443-4A64-9D00-8DECBF4B3D6D", "You cannot create an Discount on this payment approval since it's status is {0}.",
					SelectedPayment.Lookups.AV_StatusList.GetDescriptionFromCode(SelectedPayment.AV_Status)));
			}
			else
			{
				HandleMiscellaneousTransaction(ZArchitecture.Core.TransactionTypes.Discount);
			}
		}

		void ExchangeDiffButton_Click(object sender, EventArgs e)
		{
			if (SelectedPayment == null)
			{
				return;
			}

			if (SelectedPayment.IsCancelledOrIsPosted)
			{
				Globals.Message.Show(Res.GetString("EF92CED7-5EB9-4C1F-89EB-A4A6DD3C5B8D", "You cannot create an Exchange Difference on this payment approval since it's status is {0}.",
					SelectedPayment.Lookups.AV_StatusList.GetDescriptionFromCode(SelectedPayment.AV_Status)));
			}
			else
			{
				HandleMiscellaneousTransaction(ZArchitecture.Core.TransactionTypes.ExchangeDifference);
			}
		}

		void ApplyEXXButton_Click(object sender, EventArgs e)
		{
			var notifications = new NotificationCollection();
			PaymentBatchPoster.ApplyExchangeGainLossToAllSingleForeignCurrencyPayments(notifications);
			PaymentBatchPoster.RefreshBinding();
			if (notifications.HasNotifications())
			{
				Globals.Message.ShowInformation(notifications.ToMessageListString());
			}
		}

		void SaveAsDraftButton_Click(object sender, EventArgs e)
		{
			TrySavePaymentsAsDraft();
		}

		bool TrySavePaymentsAsDraft()
		{
			var errorMessage = string.Empty;
			if (PaymentBatchPoster.ISPaymentBatchEmpty)
			{
				Globals.Message.ShowError(Res.GetString("7709D53C-B9F2-4EF4-A7AC-9CC574718C31", "Payment Batch cannot be saved as draft when there are no linked payment approvals."));
				return false;
			}
			else
			{
				foreach (PaymentApprovalBase payment in PaymentBatchPoster.PaymentApprovalCollection)
				{
					errorMessage = payment.CheckCanSaveAsDraft();
					if (!string.IsNullOrEmpty(errorMessage))
					{
						Globals.Message.ShowError(errorMessage);
						return false;
					}
				}
			}

			using (PaymentBatchPoster.Factory.SetTempContext(BusinessContext.SavingPaymentApprovalAsDraft))
			{
				return FireSaveButton() == ContinueWithSave.Yes;
			}
		}

		void CheckExRateButton_Click(object sender, EventArgs e)
		{
			if (PaymentBatchPoster.PaymentApprovalCollection.Any(x => !x.IsInDatabase) && !TrySavePaymentsAsDraft())
			{
				return;
			}

			if (PaymentBatchPoster.PaymentApprovalCollection.Cast<PaymentApprovalBase>().Any(x => x.HasActiveDeal))
			{
				Globals.Message.ShowError(PaymentApprovalBase.ActiveDealErrorTextForChangingPaymentDetails);
				return;
			}

			PaymentBatchEPaymentHelper.CheckExRate(PaymentBatchPoster, () =>
			{
				TabControlBankAndEPayment.SelectNextTabPage();
				QuoteFilterControl_PerformSearch(null, null);
				RefreshButton_Click(sender, e);
			});
		}

		void ProcessEPaymentsButton_Click(object sender, EventArgs e)
		{
			if (PaymentBatchEPaymentHelper.TryToCreateDeal(PaymentBatchPoster))
			{
				TabControlBankAndEPayment.SelectedTab = zTabPage2;
				PaymentBatchPoster.RefreshPaymentApprovalCollectionCurrentDeals();
			}
		}

		void AcceptQuotesButton_Click(object sender, EventArgs e)
		{
			PaymentBatchEPaymentHelper.AcceptQuotes(PaymentBatchPoster, () =>
			{
				Globals.Message.ShowInformation(Res.GetString("58c146ae-f3b4-4b32-9b10-2ce15e30bae9", "Quotes have been accepted. Payment Batch form will be reloaded to update details."));
				ReopenWithEditForm();
			});
		}

		void SyncRecipientsButton_Click(object sender, EventArgs e)
		{
			if (MatchEPaymentRecipientsHelper.RunPreSynchronizeCheck(PaymentBatchPoster.MatchEPaymentRecipients))
			{
				MatchEPaymentRecipientsHelper.CreateBeneficiaryRequest(PaymentBatchPoster.MatchEPaymentRecipients);
			}
		}

		void PrepareZPostOrCancelButtons()
		{
			SaveButton.Visible = true;
			SaveButton.Enabled = false;

			SaveAndCloseButton.Visible = true;
			SaveAndCloseButton.Enabled = false;

			SaveAsDraftButton.Visible = true;
			SaveAsDraftButton.Enabled = false;
		}

		void RefreshButton_Click(object sender, EventArgs e)
		{
			PaymentBatchPoster.EPaymentQuotesSummary.ReloadSummaries();
			PaymentBatchPoster.RefreshPaymentApprovalCollectionCurrentDeals();
			PaymentBatchPoster.MatchEPaymentRecipients.RefreshRequests();
			PaymentBatchPoster.MatchEPaymentRecipients.RefreshBinding();
			PaymentBatchPoster.ResetPaymentApprovalCollectionAccountDetails();
			PaymentBatchPoster.ReloadPaymentApprovalCollectionEPaymentBeneficiaries();
		}

		void EnableOrDisableCheckExRateButton()
		{
			CheckExRateButton.Enabled = PaymentBatchPoster.PaymentApprovalCollection.OfType<PaymentApprovalBase>().All(x => PaymentApprovalStatus.StatusesAllowingExchangeRateQuote.Contains<string>(x.AV_Status));
		}

		#endregion

		#region Match Transactions Grid Context Menu

		void MatchTransactionsGridMenu_Popup(object sender, EventArgs e)
		{
			// Remove the OVP, DSC and EXX Menu Items
			RemoveMatchTransactionGridMenuItems();

			// Add the items if Balance != 0, etc.
			if (SelectedPayment != null && !SelectedPayment.ReadOnly)
			{
				if (SelectedPayment != null && SelectedPayment.MatchedTransactionsBalanceWithPaymentAmount != 0)
				{
					if (SelectedMatchingBase.OverpaymentCurrent == null)
					{
						MatchTransactionsGrid.ContextMenu.MenuItems.Add(OVPMenuItem);
					}
					if (SelectedMatchingBase.DiscountCurrent == null)
					{
						MatchTransactionsGrid.ContextMenu.MenuItems.Add(DSCMenuItem);
					}
					if (SelectedMatchingBase.ExchangeDiffCurrent == null)
					{
						MatchTransactionsGrid.ContextMenu.MenuItems.Add(EXXMenuItem);
					}
				}
				// Delete Menu Item
				if (MatchTransactionsGrid.SelectedElements.Length == 1 && !PaymentBatchPoster.IsInDatabase)
				{
					if (!MatchTransactionsGrid.ContextMenu.MenuItems.Contains(DeleteMenuItem))
					{
						MatchTransactionsGrid.ContextMenu.MenuItems.Add(DeleteMenuItem);
					}
				}
			}

			// Viewing Transactions
			if (SelectedPayment != null && MatchTransactionsGrid.SelectedElements.Length == 1)
			{
				MatchTransactionsGrid.ContextMenu.MenuItems.Add(0, ViewMatchTransactionMenuItem);
			}
		}

		void RemoveMatchTransactionGridMenuItems()
		{
			if (MatchTransactionsGrid.ContextMenu.MenuItems.Contains(OVPMenuItem))
			{
				MatchTransactionsGrid.ContextMenu.MenuItems.Remove(OVPMenuItem);
			}
			if (MatchTransactionsGrid.ContextMenu.MenuItems.Contains(DSCMenuItem))
			{
				MatchTransactionsGrid.ContextMenu.MenuItems.Remove(DSCMenuItem);
			}
			if (MatchTransactionsGrid.ContextMenu.MenuItems.Contains(EXXMenuItem))
			{
				MatchTransactionsGrid.ContextMenu.MenuItems.Remove(EXXMenuItem);
			}
			if (MatchTransactionsGrid.ContextMenu.MenuItems.Contains(ViewMatchTransactionMenuItem))
			{
				MatchTransactionsGrid.ContextMenu.MenuItems.Remove(ViewMatchTransactionMenuItem);
			}
			if (PaymentBatchGrid.ContextMenu.MenuItems.ContainsKey((NoResString)"Delete")) // Hard-coded constant
			{
				PaymentBatchGrid.ContextMenu.MenuItems.RemoveByKey((NoResString)"Delete"); // Hard-coded constant
			}
			if (MatchTransactionsGrid.ContextMenu.MenuItems.Contains(DeleteMenuItem))
			{
				MatchTransactionsGrid.ContextMenu.MenuItems.Remove(DeleteMenuItem);
			}
		}

		#endregion

		#region Payment Batch Grid Context Menu

		void RemovePaymentBatchGridMenuItems()
		{
			if (PaymentBatchGrid.ContextMenu.MenuItems.ContainsKey((NoResString)"Delete")) // Hard-coded constant
			{
				PaymentBatchGrid.ContextMenu.MenuItems.RemoveByKey((NoResString)"Delete"); // Hard-coded constant
			}
			if (PaymentBatchGrid.ContextMenu.MenuItems.Contains(DeletePaymentMenuItem))
			{
				PaymentBatchGrid.ContextMenu.MenuItems.Remove(DeletePaymentMenuItem);
			}
			if (PaymentBatchGrid.ContextMenu.MenuItems.Contains(EditPaymentOrgDetailMenuItem))
			{
				PaymentBatchGrid.ContextMenu.MenuItems.Remove(EditPaymentOrgDetailMenuItem);
			}
			if (PaymentBatchGrid.ContextMenu.MenuItems.Contains(ViewPaymentTransactionMenuItem))
			{
				PaymentBatchGrid.ContextMenu.MenuItems.Remove(ViewPaymentTransactionMenuItem);
			}
		}

		void PaymentBatchGridMenu_Popup(object sender, EventArgs e)
		{
			RemovePaymentBatchGridMenuItems();

			if (SelectedPayment != null && !SelectedPayment.ReadOnly)
			{
				// Delete Menu Item
				if (!PaymentBatchGrid.ContextMenu.MenuItems.Contains(DeletePaymentMenuItem))
				{
					PaymentBatchGrid.ContextMenu.MenuItems.Add(DeletePaymentMenuItem);
				}

				if (!PaymentBatchGrid.ContextMenu.MenuItems.Contains(EditPaymentOrgDetailMenuItem))
				{
					PaymentBatchGrid.ContextMenu.MenuItems.Add(EditPaymentOrgDetailMenuItem);
				}
			}
			if (SelectedPayment != null && PaymentBatchGrid.SelectedElements.Length == 1)
			{
				PaymentBatchGrid.ContextMenu.MenuItems.Add(0, ViewPaymentTransactionMenuItem);
			}
		}

		#endregion

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (CheckExRateButton.Visible)
			{
				EnableOrDisableCheckExRateButton();
			}

			if (DisplayMode == ODisplayMode.ReadOnly)
			{
				CheckExRateButton.Enabled = false;
			}

			if (PaymentBatchPoster.IsInDatabase)
			{
				PostPaymentsAsPaymentApprovalsCheckBox.Visible = false;
			}

			SetupPaymentProcessingMenuItems(ActionsMenuItem);
			SetupPaymentProcessingMenuItems(PaymentBatchGrid.ContextMenu, true);
			APB_ABInfo_ValueChanged(this, null);
			PaymentBatchPoster.MatchEPaymentRecipients.RefreshRequests();
			SetAvailableColumns();
			previousAPB_PaymentType = PaymentBatchPoster.APB_PaymentType;
		}

		void SetEPaymentControlsVisibility()
		{
			var isVisible = AccountingMasterFilesRegistry.Instance.EnableEPaymentFunctionality.Value.IsEPaymentEnabledForAnyProvider;
			if (!isVisible)
			{
				string[] columnsToRemove = new[] { "DealStatusDescription", "DealSubmittedLocalTime", "DealLastResponseLocalTime", "DealErrorMessage", "DealProviderReference", "AV_EPaymentReasonCode" };

				foreach (ZGridColumnInfo columnStyle in PaymentBatchGrid.ColumnStyles.ToArray())
				{
					if (columnsToRemove.Contains(columnStyle.ColumnName))
					{
						PaymentBatchGrid.ColumnStyles.Remove(columnStyle);
					}
				}
			}
			CurrencySummaryTotalEPaymentTextBox.Visible = isVisible;
			CurrencySummaryTotalEPaymentFeesTextBox.Visible = isVisible;
			zTabPage2.TabVisible = isVisible;
			CheckExRateButton.Visible = isVisible;
			ProcessEPaymentsButton.Visible = isVisible;
		}

		void SetEPaymentFieldVisibility()
		{
			var isVisible = (PaymentBatchPoster.APB_PaymentType == ReceiptTypes.EPayment);
			CurrencySummaryTotalEPaymentTextBox.Visible = isVisible;
			CurrencySummaryTotalEPaymentFeesTextBox.Visible = isVisible;

			ChequeBookGuidFindBox.Visible = !isVisible;
			FundingBankAccountFindBox.Visible = isVisible;
			FundingCurrencyCodeFindBox.Visible = isVisible;
		}

		void SetAvailableColumns()
		{
			if (PaymentBatchPoster.APB_PaymentType == ReceiptTypes.EPayment)
			{
				PaymentBatchGrid.AddToAvailableColumns(bankAuditColumnNames);
				PaymentBatchGrid.AddToAvailableColumns(payeeDetailsColumns);
				PaymentBatchGrid.AddToAvailableColumns(ePaymentSyncDetailsColumns);
			}
			else if (PaymentBatchPoster.APB_PaymentType == ReceiptTypes.DirectDebit)
			{
				PaymentBatchGrid.AddToAvailableColumns(bankAuditColumnNames);
				PaymentBatchGrid.AddToAvailableColumns(payeeDetailsColumns);
				PaymentBatchGrid.RemoveFromAvailableColumns(ePaymentSyncDetailsColumns);
			}
			else
			{
				PaymentBatchGrid.RemoveFromAvailableColumns(bankAuditColumnNames);
				PaymentBatchGrid.RemoveFromAvailableColumns(payeeDetailsColumns);
				PaymentBatchGrid.RemoveFromAvailableColumns(ePaymentSyncDetailsColumns);
			}
		}

		string previousAPB_PaymentType = string.Empty;
		readonly string[] bankAuditColumnNames = new[] { "BankCreateUser", "BankCreateTimeLocal", "BankLastEditUser", "BankLastEditTimeLocal" };
		readonly string[] payeeDetailsColumns = new[] { "PayeeBankName", "AccountTitle", "PayeeBankAccountCountry", "PayeeBankBSB", "PayeeBankAccountNumber" };
		readonly string[] ePaymentSyncDetailsColumns = new[] { "EPaymentRecipientListLastUpdatedTimeLocal", "EPaymentBeneficiaryLastEditTimeLocal" };

		void SetupPaymentProcessingMenuItems(Menu parentMenuItem, bool createActionMenu = false)
		{
			if (PaymentBatchPoster.IsInDatabase && DisplayMode == ODisplayMode.Browse)
			{
				Menu.MenuItemCollection menuItems = parentMenuItem.MenuItems;

				if (createActionMenu)
				{
					var actionMenu = new ZMenuItem(ResString.GetMultilingualString("PaymentBatchForm|39CA4751-05CA-433B-8479-A87B3B72C59B", "Actio&ns"));
					parentMenuItem.MenuItems.Add(new ZMenuItem("-"));
					parentMenuItem.MenuItems.Add(actionMenu);
					menuItems = actionMenu.MenuItems;
				}

				if (PaymentProcessingGUIHelper.IsPaymentAuthorisationRequired())
				{
					var submitForApprovalMenuItem = new ZMenuItem(PaymentProcessingGUIHelper.SubmitForApprovalMenuText, new EventHandler(SubmitForApproval));
					menuItems.Add(submitForApprovalMenuItem);
				}
				else
				{
					var approveForPostingMenuItem = new ZMenuItem(PaymentProcessingGUIHelper.ApproveForPostingMenuText, new EventHandler(ApproveForPosting));
					menuItems.Add(approveForPostingMenuItem);
				}

				var mainAuthoriseMenuItem = new ZMenuItem(PaymentProcessingGUIHelper.AuthorisationMenuText);
				mainAuthoriseMenuItem.MenuItems.Add(new ZMenuItem(PaymentProcessingGUIHelper.AuthoriseMenuText, new EventHandler(AuthorisePaymentApprovals)));
				mainAuthoriseMenuItem.MenuItems.Add(new ZMenuItem(PaymentProcessingGUIHelper.UnAuthoriseMenuText, new EventHandler(UnAuthorisePaymentApprovals)));
				mainAuthoriseMenuItem.MenuItems.Add(new ZMenuItem(PaymentProcessingGUIHelper.RejectItemMenuText, new EventHandler(RejectPaymentApprovals)));
				menuItems.Add(mainAuthoriseMenuItem);

				var postMenuItem = new ZMenuItem(PaymentProcessingGUIHelper.PostApprovalsMenuText);
				postMenuItem.MenuItems.Add(new ZMenuItem(PaymentProcessingGUIHelper.PostItemMenuText, new EventHandler(PostPaymentApprovals)));
				postMenuItem.MenuItems.Add(new ZMenuItem(PaymentProcessingGUIHelper.AllocateChequeNoItemMenuText, new EventHandler(PopulateChequeNoForPaymentApprovals)));
				postMenuItem.MenuItems.Add(new ZMenuItem(PaymentProcessingGUIHelper.AllocateChequeNoAndPostItemMenuText, new EventHandler(PopulateChequeNoAndPostPaymentApprovals)));
				menuItems.Add(postMenuItem);

				var cancelMenuItem = new ZMenuItem(PaymentProcessingGUIHelper.CancelMenuText, new EventHandler(CancelPaymentApprovals));
				menuItems.Add(cancelMenuItem);
			}

			if (PaymentBatchPoster.IsInDatabase)
			{
				PostPaymentsAsPaymentApprovalsCheckBox.Visible = false;
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				UnhookEvents();
			}

			base.Dispose(disposing);
		}

		protected override void OnApplyButtonClick(object sender, EventArgs e)
		{
			base.OnApplyButtonClick(sender, e);

			if (PaymentBatchPoster.ISPaymentBatchEmpty)
			{
				CloseAndShowEmptyPaymenyProcessing();
			}
		}

		void UpdateGuiComponentsAfterSuccessfullySaved()
		{
			PaymentBatchPoster.SetReadOnly();

			if (PaymentBatchPoster.IsInDatabase)
			{
				this.zTabPage1.SetReadOnlyIncludingChildren();
				this.splitContainer1.Panel2.SetReadOnlyIncludingChildren();
				this.splitContainer2.Panel2.SetReadOnlyIncludingChildren();
				PrepareZPostOrCancelButtons();
			}

			EnableOrDisableCheckExRateButton();
			ProcessEPaymentsButton.Enabled = true;
			LearnMoreButton.Enabled = true;
		}

		#region Miscellaneous Transaction Related

		#region HandleMiscellaneousTransaction

		// Decides whether to pop-up the New form or View form
		void HandleMiscellaneousTransaction(ZString transactionType)
		{
			TransactionHeader miscTransaction = null;
			ZString readableTransactionType = ZString.Empty;
			switch (transactionType)
			{
				case ZArchitecture.Core.TransactionTypes.Overpayment:
					miscTransaction = SelectedMatchingBase.OverpaymentCurrent;
					readableTransactionType = Res.GetString("1415a7d3-5a4c-4bfd-ad1c-31acca4c4220", "Overpayment");
					break;
				case ZArchitecture.Core.TransactionTypes.ExchangeDifference:
					miscTransaction = SelectedMatchingBase.ExchangeDiffCurrent;
					readableTransactionType = Res.GetString("b6a350d9-1668-43bd-9989-dfec66ded6fd", "Exchange Difference");
					break;
				case ZArchitecture.Core.TransactionTypes.Discount:
					miscTransaction = SelectedMatchingBase.DiscountCurrent;
					readableTransactionType = Res.GetString("31d95d9a-fdeb-49e6-bddc-8005f741dc0f", "Discount");
					break;
				default:
					miscTransaction = null;
					break;
			}
			if (miscTransaction == null && SelectedPayment != null && SelectedPayment.MatchedTransactionsBalanceWithPaymentAmount != 0M)
			{
				var popupForm = ShowNewMiscTransactionForm(transactionType);
				BindPopupFormEventSafe(popupForm);
			}
			else if (miscTransaction != null)
			{
				using (new DisposableAction(() => miscTransaction.Factory.SetContext(BusinessContext.EditingPaymentApprovalBatch),
											() => miscTransaction.Factory.RemoveContext(BusinessContext.EditingPaymentApprovalBatch)))
				{
					var popupForm = ShowEditMiscTransactionForm(transactionType);
					BindPopupFormEventSafe(popupForm);
				}
			}
			else
			{
				string caption = Res.GetString("ad366d03-9dea-4f85-9936-9c8cdac4b274", "Match Transactions");
				string message = Res.GetString("11621db9-6a7a-4ae7-bb6d-b1e190cf0708", "{0} cannot be created here because the balance is zero.", readableTransactionType);
				Globals.Message.ShowInformation(message, caption);
			}
		}

		void PopupForm_Closed(object sender, EventArgs e)
		{
			PaymentBatchPoster.RefreshBinding();
		}

		void BindPopupFormEventSafe(IZForm popupForm)
		{
			popupForm.Closed -= PopupForm_Closed;
			popupForm.Closed += PopupForm_Closed;
		}

		#endregion

		#region ShowNewMiscTransactionForm

		IZForm ShowNewMiscTransactionForm(ZString transactionType)
		{
			IZForm formToReturn = null;
			TransactionHeader miscTrans = SelectedMatchingBase.GetMiscellaneousTransaction(transactionType);
			if (miscTrans != null)
			{
				miscTrans.BindableOSAmount = -(SelectedPayment?.MatchedTransactionsBalanceWithPaymentAmount) ?? 0;
				ZController miscTransController = AccountingControllerCreator.GetNewController(miscTrans);
				if (miscTransController != null)
				{
					TransactionHeaderCollection defaultMiscTrans = new TransactionHeaderCollection(Factory);
					defaultMiscTrans.Add(miscTrans);
					miscTransController.SetCollectionForDefaultsAndValidation(defaultMiscTrans);
					miscTransController.SetFormsModalTo(this);
					formToReturn = (TransactionViewForm)miscTransController.ShowNewForm();
				}
			}

			return formToReturn;
		}

		#endregion

		#region ShowEditMiscTransactionForm

		IZForm ShowEditMiscTransactionForm(ZString transactionType)
		{
			IZForm form = null;
			TransactionHeader header = null;

			switch (transactionType)
			{
				case ZArchitecture.Core.TransactionTypes.Overpayment:
					header = SelectedMatchingBase.OverpaymentCurrent;
					break;
				case ZArchitecture.Core.TransactionTypes.Discount:
					header = SelectedMatchingBase.DiscountCurrent;
					break;
				case ZArchitecture.Core.TransactionTypes.ExchangeDifference:
					header = SelectedMatchingBase.ExchangeDiffCurrent;
					break;
				default:
					break;
			}

			if (header != null)
			{
				ZController miscTransController = AccountingControllerCreator.GetNewController(header);
				if (miscTransController != null)
				{
					miscTransController.SetFormsModalTo(this);
					form = miscTransController.ShowEditForm(header);
				}
			}

			return form;
		}

		#endregion

		#region ViewPaymentTransactionMenuItem

		MenuItem ViewPaymentTransactionMenuItem
		{
			get
			{
				if (fViewPaymentTransactionMenuItem == null)
				{
					fViewPaymentTransactionMenuItem = new ZMenuItem(ResString.GetMultilingualString("73F91625-8FCD-4D34-A644-C9ED9AA28711", "View"), new EventHandler(HandleViewPaymentBatchGridTransaction));
				}

				return fViewPaymentTransactionMenuItem;
			}
		}

		MenuItem fViewPaymentTransactionMenuItem;

		#endregion

		#region Menu Item Handlers

		void EditPaymentOrganizationDetail(object sender, EventArgs e)
		{
			if (PaymentBatchGrid != null && PaymentBatchGrid.ListManager != null && PaymentBatchGrid.ListManager.Position >= 0)
			{
				ShowOrganisationFormToEdit();
			}
		}

		void AddOverpayment(object sender, EventArgs e)
		{
			if (SelectedMatchingBase.IsOverPaymentAllowedInThisMatchingSession)
			{
				HandleMiscellaneousTransaction(ZArchitecture.Core.TransactionTypes.Overpayment);
			}
			else
			{
				Globals.Message.Show(string.Format((NoResString)"You cannot create an OverPayment in this matching session")); // developer only string
			}
		}

		void AddExchangeDifference(object sender, EventArgs e)
		{
			HandleMiscellaneousTransaction(ZArchitecture.Core.TransactionTypes.ExchangeDifference);
		}

		void AddDiscount(object sender, EventArgs e)
		{
			HandleMiscellaneousTransaction(ZArchitecture.Core.TransactionTypes.Discount);
		}

		void DeleteTransaction(object sender, EventArgs e)
		{
			if (MatchTransactionsGrid.SelectedElements.Length == 1)
			{
				if (MatchTransactionsGrid.SelectedElements[0] == SelectedMatchingBase.DiscountCurrent ||
				MatchTransactionsGrid.SelectedElements[0] == SelectedMatchingBase.OverpaymentCurrent ||
				MatchTransactionsGrid.SelectedElements[0] == SelectedMatchingBase.ExchangeDiffCurrent)
				{
					SelectedMatchingBase.DeleteMiscTransaction((TransactionHeader)MatchTransactionsGrid.SelectedElements[0]);
				}
				else if (MatchTransactionsGrid.SelectedElements[0] is TransactionHeader)
				{
					PaymentBatchPoster.RemoveTransactionFromPayment((TransactionHeader)MatchTransactionsGrid.SelectedElements[0]);
				}
			}
		}

		#endregion

		#region Menu Item Properties
		MenuItem EditPaymentOrgDetailMenuItem
		{
			get
			{
				if (fEPODMenuItem == null)
				{
					fEPODMenuItem = new ZMenuItem(ResString.GetMultilingualString("Accounting.PaymentBatchPosting.EditPaymentOrganizationDetail", "Edit Payment Organization Detail"), new EventHandler(EditPaymentOrganizationDetail));
				}

				return fEPODMenuItem;
			}
		}

		MenuItem fEPODMenuItem;

		MenuItem OVPMenuItem
		{
			get
			{
				if (fOVPMenuItem == null)
				{
					fOVPMenuItem = new ZMenuItem(ResString.GetMultilingualString("Accounting.PaymentBatchPosting.Overpayment", "Add Overpayment"), new EventHandler(AddOverpayment));
				}

				return fOVPMenuItem;
			}
		}

		MenuItem fOVPMenuItem;

		MenuItem EXXMenuItem
		{
			get
			{
				if (fEXXMenuItem == null)
				{
					fEXXMenuItem = new ZMenuItem(ResString.GetMultilingualString("Accounting.PaymentBatchPosting.ExchangeDifference", "Add Exchange Difference"), new EventHandler(AddExchangeDifference));
				}

				return fEXXMenuItem;
			}
		}

		MenuItem fEXXMenuItem;

		MenuItem DSCMenuItem
		{
			get
			{
				if (fDSCMenuItem == null)
				{
					fDSCMenuItem = new ZMenuItem(ResString.GetMultilingualString("Accounting.PaymentBatchPosting.Discount", "Add Discount"), new EventHandler(AddDiscount));
				}

				return fDSCMenuItem;
			}
		}

		MenuItem fDSCMenuItem;

		MenuItem DeleteMenuItem
		{
			get
			{
				if (fDeleteMenuItem == null)
				{
					fDeleteMenuItem = new ZMenuItem(ResString.GetMultilingualString("MenuItem.Remove", "Remove"), new EventHandler(DeleteTransaction));
				}

				return fDeleteMenuItem;
			}
		}

		MenuItem fDeleteMenuItem;

		#region ViewMatchTransactionMenuItem

		MenuItem ViewMatchTransactionMenuItem
		{
			get
			{
				if (fViewMatchTransactionMenuItem == null)
				{
					fViewMatchTransactionMenuItem = new ZMenuItem(ResString.GetMultilingualString("5b46e2ab-6628-4a3a-b9a2-d2f14ddb157c", "View"), new EventHandler(ViewMatchGridTransaction));
				}

				return fViewMatchTransactionMenuItem;
			}
		}

		MenuItem fViewMatchTransactionMenuItem;

		#endregion

		#region Menu Item Handlers

		void ViewMatchGridTransaction(object sender, EventArgs e)
		{
			if (MatchTransactionsGrid.SelectedElements.Length == 1)
			{
				if (MatchTransactionsGrid.SelectedElements[0] == SelectedMatchingBase.DiscountCurrent)
				{
					HandleMiscellaneousTransaction(ZArchitecture.Core.TransactionTypes.Discount);
				}
				else if (MatchTransactionsGrid.SelectedElements[0] == SelectedMatchingBase.OverpaymentCurrent)
				{
					HandleMiscellaneousTransaction(ZArchitecture.Core.TransactionTypes.Overpayment);
				}
				else if (MatchTransactionsGrid.SelectedElements[0] == SelectedMatchingBase.ExchangeDiffCurrent)
				{
					HandleMiscellaneousTransaction(ZArchitecture.Core.TransactionTypes.ExchangeDifference);
				}
				else if (MatchTransactionsGrid.SelectedElements[0] is TransactionHeader)
				{
					TransactionHeader header = (TransactionHeader)MatchTransactionsGrid.SelectedElements[0];
					ZController newController = AccountingControllerCreator.GetNewController(header);
					newController.ShowViewForm(header);
				}
			}
		}

		void HandleViewPaymentBatchGridTransaction(object sender, EventArgs e)
		{
			ShowAPPaymentProcessingForm(true);
		}

		void HandleDoubleClickPaymentBatchGridTransaction(object sender, EventArgs e)
		{
			ShowAPPaymentProcessingForm();
		}

		IZForm ShowAPPaymentProcessingForm(bool isView = false)
		{
			IZForm result = null;
			if (PaymentBatchGrid.SelectedElements.Length == 1 && PaymentBatchGrid.SelectedElements[0] is PaymentApprovalBase payment)
			{
				if (PaymentBatchPoster.HasChanges)
				{
					var questionResult = Globals.Message.Show(
											Res.GetString("857b7ee1-c5fb-44b8-979d-1baeddd60ca3", "You must save this form first. Would you like to save now?"),
											Res.GetString("e09ec5c3-5b0b-4fd4-b544-34dc1fd5120d", "Cannot Edit"),
											MessageBoxButtons.YesNo,
											MessageBoxIcon.Question,
											DialogResult.Yes);

					var isSaveSuccessful = questionResult == DialogResult.Yes && this.FireSaveButton() == ContinueWithSave.Yes;
					if (isSaveSuccessful)
					{
						result = ShowAPPaymentProcessingFormCore(payment, isView);
					}
				}
				else
				{
					result = ShowAPPaymentProcessingFormCore(payment, isView);
				}
			}

			return result;
		}

		IZForm ShowAPPaymentProcessingFormCore(PaymentApprovalBase payment, bool isView)
		{
			IZForm result = null;
			var newController = ZControllerFactory.Create(ControllerIDs.APPaymentProcessing);
			if (isView)
			{
				result = newController.ShowViewForm(payment);
				ZFormModaliser.Show(result as Form, this);
			}
			else
			{
				result = newController.ShowEditForm(payment);
				ZFormModaliser.Show(result as Form, this);
			}

			if (result != null)
			{
				((ZForm)result).Saved += APPaymentProcessingForm_Saved;
			}

			return result;
		}

		void APPaymentProcessingForm_Saved(object sender, EventArgs e)
		{
			var savedForm = (ZForm)sender;
			savedForm.FormClosed -= SavedForm_FormClosed;
			savedForm.FormClosed += SavedForm_FormClosed;
		}

		void SavedForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			Globals.Message.Show(
				ResString.GetMultilingualString("78c2069f-c4a9-4ff4-a01c-5855a06061b0", "This form will be reloaded since the payment approval has been changed."),
				ResString.GetMultilingualString("87c672e3-a1fb-4a6c-8c98-0e2112505b06", "Information"),
				MessageBoxButtons.OK,
				MessageBoxIcon.Information);

			ReopenWithEditForm();
		}

		#endregion

		#endregion

		#region Misc Transactions Values Changed Handlers

		void UnBindMiscTransactionsAmountsChangedForPayment(PaymentApprovalBase payment)
		{
			if (payment != null)
			{
				payment.MatchingBaseObject.OSOverpaymentAmountInfo.ValueChanged -= OSOverpaymentAmountInfo_ValueChanged;
				payment.MatchingBaseObject.ExchangeDifferenceAmountInfo.ValueChanged -= ExchangeDifferenceAmountInfo_ValueChanged;
				payment.MatchingBaseObject.DiscountAmountInfo.ValueChanged -= DiscountAmountInfo_ValueChanged;
			}
		}

		void BindMiscTransactionsAmountsChangedForCurrentPayment()
		{
			if (SelectedMatchingBase != null)
			{
				SelectedMatchingBase.OSOverpaymentAmountInfo.ValueChanged += OSOverpaymentAmountInfo_ValueChanged;
				SelectedMatchingBase.ExchangeDifferenceAmountInfo.ValueChanged += ExchangeDifferenceAmountInfo_ValueChanged;
				SelectedMatchingBase.DiscountAmountInfo.ValueChanged += DiscountAmountInfo_ValueChanged;
			}
		}

		void DiscountAmountInfo_ValueChanged(object sender, EventArgs e)
		{
			RefreshSelectedPaymentData();
			if (SelectedMatchingBase != null && SelectedMatchingBase.DiscountCurrent != null)
			{
				SelectedMatchingBase.DiscountCurrent.HasChanges = true;
			}
		}

		void ExchangeDifferenceAmountInfo_ValueChanged(object sender, EventArgs e)
		{
			RefreshSelectedPaymentData();
			if (SelectedMatchingBase != null && SelectedMatchingBase.ExchangeDiffCurrent != null)
			{
				SelectedMatchingBase.ExchangeDiffCurrent.HasChanges = true;
			}
		}

		void OSOverpaymentAmountInfo_ValueChanged(object sender, EventArgs e)
		{
			RefreshSelectedPaymentData();
			if (SelectedMatchingBase != null && SelectedMatchingBase.OverpaymentCurrent != null)
			{
				SelectedMatchingBase.OverpaymentCurrent.HasChanges = true;
			}
		}

		void RefreshSelectedPaymentData()
		{
			PaymentBatchPoster.ResetPaymentMatchingCollectionFromCurrentPayment();
			SetButtonText(SelectedMatchingBase.DiscountCurrent, DiscountButton);
			SetButtonText(SelectedMatchingBase.ExchangeDiffCurrent, ExchangeDiffButton);
			SetButtonText(SelectedMatchingBase.OverpaymentCurrent, OverpaymentButton);
		}
		#endregion

		#endregion

		#region Payment Transactions Related

		#region DeletePaymentMenuItem

		MenuItem DeletePaymentMenuItem
		{
			get
			{
				if (fDeletePaymentMenuItem == null)
				{
					fDeletePaymentMenuItem = new ZMenuItem(ResString.GetMultilingualString("MenuItem.Remove", "Remove"), new EventHandler(DeletePaymentTransaction));
				}

				return fDeletePaymentMenuItem;
			}
		}

		MenuItem fDeletePaymentMenuItem;

		#endregion

		void DeletePaymentTransaction(object sender, EventArgs e)
		{
			using (new DisposableAction(() => SuspendPaymentDetailsRefresh = ZBool.True, () => SuspendPaymentDetailsRefresh = ZBool.False))
			{
				UnBindMiscTransactionsAmountsChangedForPayment(SelectedPayment);
				PaymentBatchPoster.RemovePaymentFromBatch(SelectedPayment);
			}
		}
		ZBool SuspendPaymentDetailsRefresh;

		#endregion

		#region PaymentProcessing Event Handler

		PaymentProcessingGUIHelper PaymentProcessingGUIHelper => paymentProcessingGUIHelper ?? (paymentProcessingGUIHelper = new PaymentProcessingGUIHelper());
		PaymentProcessingGUIHelper paymentProcessingGUIHelper;

		BusinessObject[] GetPaymentPaymentApprovals(object sender)
		{
			BusinessObject[] paymentApprovals = null;
			var senderMenuItem = sender as ZMenuItem;
			if (senderMenuItem != null)
			{
				paymentApprovals = senderMenuItem.ParentControl == this ? PaymentBatchPoster.PaymentApprovalCollection.ToArray() : PaymentBatchGrid.SelectedElements;
			}
			return paymentApprovals;
		}

		bool CheckBeforeProcessingPayments()
		{
			if (PaymentBatchPoster.HasChanges)
			{
				Globals.Message.Show(Res.GetString("4C9E1E5A-4AAF-4D5E-A576-D6E03968F305", "Please save this payment batch first."));
				return false;
			}
			else if (PaymentBatchPoster.PaymentApprovalCollection.OfType<PaymentApprovalBase>().Any(x => x.IsDiscrepancyWithPaymentBatch))
			{
				ValidateAll(ValidationType.Full);
				if (PaymentBatchPoster.HasErrors())
				{
					ShowErrorsDialog();
					return false;
				}
			}

			return true;
		}

		void SubmitForApproval(object sender, EventArgs e)
		{
			if (CheckBeforeProcessingPayments())
			{
				PaymentProcessingGUIHelper.SubmitForApproval(GetPaymentPaymentApprovals(sender));
			}
		}

		void ApproveForPosting(object sender, EventArgs e)
		{
			if (CheckBeforeProcessingPayments())
			{
				PaymentProcessingGUIHelper.ApproveForPosting(GetPaymentPaymentApprovals(sender));
			}
		}

		void AuthorisePaymentApprovals(object sender, EventArgs e)
		{
			if (CheckBeforeProcessingPayments())
			{
				PaymentProcessingGUIHelper.AuthorisePaymentApprovals(GetPaymentPaymentApprovals(sender));
			}
		}

		void UnAuthorisePaymentApprovals(object sender, EventArgs e)
		{
			if (CheckBeforeProcessingPayments())
			{
				PaymentProcessingGUIHelper.UnAuthorisePaymentApprovals(GetPaymentPaymentApprovals(sender));
			}
		}

		void RejectPaymentApprovals(object sender, EventArgs e)
		{
			if (CheckBeforeProcessingPayments())
			{
				PaymentProcessingGUIHelper.RejectPaymentApprovals(GetPaymentPaymentApprovals(sender));
			}
		}

		void PostPaymentApprovals(object sender, EventArgs e)
		{
			if (CheckBeforeProcessingPayments())
			{
				using (PaymentProcessingGUIHelper.RegisterAfterSavedAction(UpdateGuiComponentsAfterSuccessfullySaved))
				{
					PaymentProcessingGUIHelper.PostPaymentApprovals(GetPaymentPaymentApprovals(sender), PaymentBatchPoster.APB_BatchNumber);
				}
			}
		}

		void PopulateChequeNoForPaymentApprovals(object sender, EventArgs e)
		{
			if (CheckBeforeProcessingPayments())
			{
				PaymentProcessingGUIHelper.PopulateChequeNoForPaymentApprovals(GetPaymentPaymentApprovals(sender));
			}
		}

		void PopulateChequeNoAndPostPaymentApprovals(object sender, EventArgs e)
		{
			if (CheckBeforeProcessingPayments())
			{
				using (PaymentProcessingGUIHelper.RegisterAfterSavedAction(UpdateGuiComponentsAfterSuccessfullySaved))
				{
					PaymentProcessingGUIHelper.PopulateChequeNoAndPostPaymentApprovals(GetPaymentPaymentApprovals(sender));
				}
			}
		}

		void CancelPaymentApprovals(object sender, EventArgs e)
		{
			if (CheckBeforeProcessingPayments())
			{
				PaymentProcessingGUIHelper.CancelPaymentApprovals(GetPaymentPaymentApprovals(sender));
			}
		}

		#endregion

		#region Overrides

		#region FormVerb & FormCaption

		public override string FormVerb
		{
			get
			{
				string result;

				if (DisplayMode == ODisplayMode.ReadOnly)
				{
					result = FormVerbs.View;
				}
				else if (PaymentBatchPoster.IsInDatabase)
				{
					result = FormVerbs.Edit;
				}
				else
				{
					result = FormVerbs.New;
				}

				return result;
			}
		}

		public override string FormCaption
		{
			get
			{
				string result;

				if (PaymentBatchPoster.IsInDatabase)
				{
					result = Res.GetString("PaymentBatchForm|C4A5161B-9F9D-4D55-BF1A-6D94C6DA6407", "AP Payment Batch");
				}
				else
				{
					result = base.FormCaption;
				}

				return result;
			}
		}

		#endregion

		string IButtonPostTextOverride.PostButtonText
		{
			get
			{
				return PaymentBatchPoster.IsInDatabase ?
				  Res.GetString("PaymentBatchForm|CDC97A4E-9675-4C1F-8E5A-4E65365ADACD", "Save && Close") :
				  Res.GetString("PaymentBatchForm|F6D96C1A-C206-4c77-8D74-6C54C041CFC7", "P&ost && Close");
			}
		}

		string IButtonApplyTextOverride.ApplyButtonText
		{
			get
			{
				return PaymentBatchPoster.IsInDatabase ?
				  Res.GetString("PaymentBatchForm|AA4CDA09-5616-47DE-83BD-817BFB346FFF", "Save") :
				  Res.GetString("PaymentBatchForm|54EF3F05-9C53-4d4f-AE78-B5DDD91BF4C7", "&Post");
			}
		}

		protected override bool AllowNew
		{
			get { return false; }
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			OnSelectedPaymentChanged();
			((APPaymentBatchPoster)BusinessEntity).CheckForDefaultBankAccounts();
		}

		void UpdateDraftStatus()
		{
			foreach (PaymentApprovalBase payment in PaymentBatchPoster.PaymentApprovalCollection)
			{
				payment.UpdateDraftStatus();
			}
		}

		bool IsSavingPaymentApprovalAsDraft => PaymentBatchPoster.Factory.HasContext(BusinessContext.SavingPaymentApprovalAsDraft);

		bool IsPostPayment => !PaymentBatchPoster.PostPaymentsAsPaymentApprovals && !IsSavingPaymentApprovalAsDraft && !PaymentBatchPoster.IsInDatabase;

		protected override ContinueWithSave ValidateAndSave()
		{
			ContinueWithSave result;

			PaymentBatchPoster.RunPreSaveValidation();

			if (PaymentBatchPoster.HasErrors)
			{
				result = ContinueWithSave.No;
				ShowErrorsDialog();
			}
			else if (IsPostPayment)
			{
				result = MessageHelper.ShowMessageIfChequeBookUsesSamePrinterReturnsCancel(((APPaymentBatchPoster)BusinessEntity).ChequeBook) ? ContinueWithSave.No : ContinueWithSave.Yes;
			}
			else
			{
				result = ContinueWithSave.Yes;
			}

			if (result == ContinueWithSave.Yes && !PaymentBatchPoster.IsInDatabase && !IsSavingPaymentApprovalAsDraft && PaymentBatchPoster.APB_PaymentType == ReceiptTypes.EPayment)
			{
				result = PaymentProcessingGUIHelper.DisplayDealErrorMessagesBeforeBatchIsSaved(PaymentBatchPoster.PaymentApprovalCollection.ToArray<PaymentApprovalBase>(), PaymentBatchPoster.APB_PaymentType);
			}

			if (result == ContinueWithSave.Yes)
			{
				Cursor.Current = Cursors.WaitCursor;

				try
				{
					PaymentBatchPoster.MatchTransactions();
					UpdateDraftStatus();

					var isNewNonDraftPaymentBatch = !PaymentBatchPoster.IsInDatabase && !IsSavingPaymentApprovalAsDraft;

					if (base.ValidateAndSave() == ContinueWithSave.Yes)
					{
						UpdateGuiComponentsAfterSuccessfullySaved();

						if (isNewNonDraftPaymentBatch)
						{
							InitializePaymentCollectionForPrinting();
							if (PaymentCollectionForPrinting.Any() || FirstApproval != null)
							{
								PrintManager.Print();
							}
						}
					}
				}
				finally
				{
					Cursor.Current = Cursors.Arrow;
				}
			}

			return result;
		}

		AccountingMessageHelper MessageHelper
		{
			get
			{
				if (fMessageHelper == null)
				{
					fMessageHelper = new AccountingMessageHelper();
				}
				return fMessageHelper;
			}
		}
		AccountingMessageHelper fMessageHelper;

		protected override void SaveCore(ITransactionParticipant[] factories)
		{
			PaymentBatchPoster.UpdatePaymentApprovalStatus();

			if (PaymentBatchPoster.IsChequeNumberAutoAllocated && !PaymentBatchPoster.IsInDatabase)
			{
#if DEBUG
				if (Globals.IsTest && !Globals.GetIsUnitTestingProductionFunctionality())
				{
					Test_Allocator = new PaymentBatchChequeNumberAllocator.DummyPaymentBatchChequeNumberAllocator(PaymentBatchPoster.PaymentApprovalCollection, PaymentBatchPoster.PaymentApprovalCollection.Factory);
					Test_Allocator.SetChequeBookToInactiveOnSaving = Test_DeactivateChequeBookOnAllocation;
					base.SaveCore(Test_Allocator.GetFactoriesForTest());
				}
				else
				{
#endif
					var allocator = new PaymentBatchChequeNumberAllocator(PaymentBatchPoster.PaymentApprovalCollection, PaymentBatchPoster.Factory);
					base.SaveCore(allocator.GetFactoriesWithAllocationCodeToBeCalledOnSaving(factories));
#if DEBUG
				}
#endif
			}
			else
			{
				List<ITransactionParticipant> participants = new List<ITransactionParticipant>(factories);
				foreach (PaymentApprovalBase payment in PaymentBatchPoster.PaymentApprovalCollectionWithoutCancelledOrPosted)
				{
					if (payment.AV_PaymentType == ReceiptTypes.eNettCreditCard
						&& payment.BankAccount != null
						&& payment.BankAccount.IsCreditCardOrLinkedAccount)
					{
						participants.Add(new eNettPaymentTransactionParticipant(payment));
					}
				}
				base.SaveCore(participants.ToArray());
			}
		}

		protected override DialogResult ShowErrorsDialogCore(bool includeIgnoreOption)
		{
			if (!Globals.IsTest)
			{
				using (var msgBox = new ZErrorMessageBox(BusinessEntity, (NoResString)"selection", Res.GetString("85af39c0-5467-4496-becb-f858364c8770", "match"), Res.GetString("6bc95ae9-5fa7-4f51-82aa-9b8e89c010db", "matched"), includeIgnoreOption)) // Hard-coded constant
				{
					return ZFormModaliser.ShowMessageBoxWithoutDispose(msgBox);
				}
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("c4efc78d-e048-4bca-927c-1cdeb3324e89", "There are errors - can't save."), Res.GetString("660332a6-6ee3-4e23-90be-45caeae995ed", "Errors!"));
				return DialogResult.Abort;
			}
		}

		protected override void HandleSaveException(Exception e)
		{
			if (PaymentBatchPoster.IsChequeNumberAutoAllocated)
			{
				PaymentBatchPoster.AllocationOrPrintingFailed();
			}
			if (e is AllocationSaveException)
			{
				Globals.Message.ShowError(((AllocationSaveException)e).UserFriendlyMessage, Res.GetString("07decef5-3ac6-4a0a-bbe8-a74f6168b5c9", "Check Book Busy"));
			}
			else if (e is AllocationChequeBookException)
			{
				Globals.Message.ShowError(((AllocationChequeBookException)e).UserFriendlyMessage, Res.GetString("396bddae-0dc8-49c1-8391-b5485403da8d", "Check Book Full"));
			}
			else if (e is ENettProcessCreditCardException)
			{
				ENettProcessCreditCardException ex = e as ENettProcessCreditCardException;
				string message = Res.GetString("08e8f091-52ef-4715-ba9c-2f7e376bc572", "Failed to process credit card payments via ComPay.") + "\r\n";
				message += Res.GetString("4418c0cf-7c8b-484a-865e-4c679faebb30", "ComPay Error: ({0}) {1}", ex.eNettErrorCode, ex.eNettErrorMessage) + "\r\n\r\n";
				message += Res.GetString("ec1b306f-f503-4c0e-85f1-6dbdfd6b0d60", "Payment failed for the following creditors:") + "\r\n\r\n";

				foreach (PaymentApprovalBase payment in PaymentBatchPoster.PaymentApprovalCollection)
				{
					message += payment.Header.OH_Code + "\r\n";

					if (payment.NewPaymentMatchingObject != null && payment.NewPaymentMatchingObject.MatchLinks != null)
					{
						foreach (TransactionMatchLink matchLink in payment.NewPaymentMatchingObject.MatchLinks)
						{
							if (matchLink.MatchingTransaction is APInvoice)
							{
								message += "\t" + Res.GetString("0ad110a0-1ac2-4c26-9064-52b1ed3d2414", "Invoice Number: {0}", matchLink.MatchingTransaction.AH_TransactionNum) + "\r\n";
							}
						}
						payment.UndoCreateNewPayment();
					}
				}
				Globals.Message.ShowError(message);
			}
			else
			{
				base.HandleSaveException(e);
			}
		}
		protected override void ZForm_Closing(object sender, CancelEventArgs e)
		{
			if (!closeFormWithoutConfirmation)
			{
				base.ZForm_Closing(sender, e);
			}
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			SetEPaymentControlsVisibility();
		}

		#endregion

		#region Implementation

		AccPaymentApproval SelectedPaymentApproval
		{
			get
			{
				if (PaymentBatchGrid != null && PaymentBatchGrid.ListManager != null)
				{
					return PaymentBatchGrid.ListManager.GetCurrent() as AccPaymentApproval;
				}
				else
				{
					return null;
				}
			}
		}

		BusinessObjectFactory Factory => factory ?? (factory = BusinessEntity?.Factory ?? new BusinessObjectFactory());
		BusinessObjectFactory factory;

		void InitializePaymentCollectionForPrinting()
		{
			foreach (PaymentApprovalBase paymentApproval in PaymentBatchPoster.PaymentApprovalCollection)
			{
				if (paymentApproval.NewPayment != null)
				{
					PaymentCollectionForPrinting.Add(paymentApproval.NewPayment);
				}
			}
			PaymentCollectionForPrinting.Sort((x, y) => x.AH_ChequeOrReference.CompareTo(y.AH_ChequeOrReference));
		}

		List<TransactionHeader> PaymentCollectionForPrinting
		{
			get
			{
				if (fPaymentCollection == null)
				{
					fPaymentCollection = new List<TransactionHeader>();
				}
				return fPaymentCollection;
			}
		}
		List<TransactionHeader> fPaymentCollection;

		PaymentApprovalBase FirstApproval => PaymentBatchPoster.FirstApproval;

		Color GetColorForPayment(PaymentApprovalBase paymentApproval)
		{
			if ((paymentApproval.MatchedTransactionsBalanceWithPaymentAmount) != 0)
			{
				return Color.LightPink;
			}
			else
			{
				return Color.Empty;
			}
		}

		APPaymentBatchPoster PaymentBatchPoster
		{
			get
			{
				return BusinessEntity as APPaymentBatchPoster;
			}
		}

		void CloseAndShowEmptyPaymenyProcessing()
		{
			Globals.Message.Show(Res.GetString("b181d84c-d9cc-465b-b782-9610a88b6d13", "There are no payments left for posting. The form will be closed."));

			closeFormWithoutConfirmation = true;
			Close();
		}

		bool closeFormWithoutConfirmation;

		PaymentApprovalBase SelectedPayment
		{
			get
			{
				if (PaymentBatchPoster.PaymentApprovalCollection.Count < 1)
				{
					return null;
				}

				if (PaymentBatchGrid.CurrentRowIndex == -1 && PaymentBatchPoster.PaymentApprovalCollection.Count > 0)
				{
					PaymentBatchGrid.CurrentRowIndex = 0;
				}
				return PaymentBatchGrid.ListManager.List[PaymentBatchGrid.CurrentRowIndex] as PaymentApprovalBase;
			}
		}

		MatchingBase SelectedMatchingBase
		{
			get { return SelectedPayment?.MatchingBaseObject; }
		}

		PaymentApprovalBase fPreviousPayment;

		void OnSelectedPaymentChanged()
		{
			if (fPreviousPayment != null && !fPreviousPayment.IsDeleted)
			{
				UnBindMiscTransactionsAmountsChangedForPayment(fPreviousPayment);
			}
			fPreviousPayment = SelectedPayment;
			BindMiscTransactionsAmountsChangedForCurrentPayment();
			PaymentBatchPoster.SetPaymentDetails(fPreviousPayment);
			SetButtonText(SelectedMatchingBase?.DiscountCurrent, DiscountButton);
			SetButtonText(SelectedMatchingBase?.ExchangeDiffCurrent, ExchangeDiffButton);
			SetButtonText(SelectedMatchingBase?.OverpaymentCurrent, OverpaymentButton);

#if DEBUG
			PaymentDetailsRefreshed = ZBool.True;
#endif
		}

		ZOrganisationsForm ShowOrganisationFormToEdit()
		{
			AccPaymentApproval selectedApproval = SelectedPaymentApproval;
			if (selectedApproval != null)
			{
				ZController orgController = ZControllerFactory.Create(ControllerIDs.Organisation);
				OrgHeader orgsToEdit = Factory.Load(typeof(OrgHeader), selectedApproval.AV_OH) as OrgHeader;
				return orgController.ShowEditForm(orgsToEdit) as ZOrganisationsForm;
			}
			else
			{
				return null;
			}
		}

		void SetButtonText(TransactionHeader header, ZButton button)
		{
			if (header == null)
			{
				button.Text = Res.GetString("Accounting|PaymentBatchForm|ButtonNew", "New");
			}
			else
			{
				button.Text = Res.GetString("Accounting|PaymentBatchForm|ButtonEdit", "Edit");
			}
		}

		void ReopenWithEditForm()
		{
			ControllerID = ControllerIDs.PaymentBatch;
			ReloadForm();
		}

		protected override void OnBindingContextChanged(EventArgs e)
		{
			base.OnBindingContextChanged(e);
			if (QuoteFilterPanel != null)
			{
				SetupQuoteFilter();
			}
		}

		#region Quote Filter Grid

		void SetupQuoteFilter()
		{
			if (QuoteFilterControl != null)
			{
				QuoteFilterPanel.Controls.Remove(QuoteFilterControl);
				QuoteFilterControl.PerformSearch -= QuoteFilterControl_PerformSearch;
				QuoteFilterControl.FiltersCleared -= QuoteFilterControl_ClearButtonClicked;
			}

			if (PaymentBatchPoster != null)
			{
				QuoteFilterControl = new AccountingOnFormFilterControl(PaymentBatchPoster.FilteredEPaymentQuotes, PaymentBatchPoster.EPaymentQuotesFilterObject);
				QuoteFilterControl.Name = "QuoteDetailsFilterControl";
				QuoteFilterControl.SetMaxFilterStripPanelHeight(128);
				QuoteFilterControl.Size = QuoteFilterPanel.ClientSize;
				QuoteFilterControl.BackColor = BackColor;
				QuoteFilterControl.Dock = System.Windows.Forms.DockStyle.Fill;

				QuoteFilterPanel.Controls.Add(QuoteFilterControl);
				QuoteFilterControl.PerformSearch += QuoteFilterControl_PerformSearch;
				QuoteFilterControl.FiltersCleared += QuoteFilterControl_ClearButtonClicked;
				QuoteFilterControl.FilteredGrid.SizeChanged += QuoteFilteredGrid_BoundsChanged;
				QuoteFilterControl.FilteredGrid.LocationChanged += QuoteFilteredGrid_BoundsChanged;
				QuoteFilteredGrid_BoundsChanged(QuoteFilterControl.FilteredGrid, EventArgs.Empty);

				QuoteGridPanel.AllowOverlap(QuoteFilterControl);
			}
		}

		protected void QuoteFilterControl_PerformSearch(object sender, EventArgs e)
		{
			PaymentBatchPoster.FilteredEPaymentQuotes.Reload(true);
			PaymentBatchPoster.FilteredEPaymentQuotes.LoadWithMoreFiltering(PaymentBatchPoster.EPaymentQuotesFilterObject.Filter);
			QuoteInfoLabel.Text = QuoteInfoLabelText();
		}

		protected virtual void QuoteFilterControl_ClearButtonClicked(object sender, EventArgs e)
		{
			PaymentBatchPoster.FilteredEPaymentQuotes.RemoveAll();
		}

		protected virtual string QuoteInfoLabelText()
		{
			return Res.GetString("828e165f-600f-4670-b424-bf04b9962bc8", "Found {0} records that match your criteria.", PaymentBatchPoster.FilteredEPaymentQuotes.Count);
		}

		void QuoteFilteredGrid_BoundsChanged(object sender, EventArgs e)
		{
			QuoteGridPanel.Bounds = QuoteFilterControl.FilteredGrid.Bounds;
		}

		#endregion

		#endregion

		#region Test
#if DEBUG

		ZBool PaymentDetailsRefreshed;
		readonly ZBool Test_DeactivateChequeBookOnAllocation = ZBool.False;
		internal PaymentBatchChequeNumberAllocator.DummyPaymentBatchChequeNumberAllocator Test_Allocator;

#endif
		#endregion

	}
}

