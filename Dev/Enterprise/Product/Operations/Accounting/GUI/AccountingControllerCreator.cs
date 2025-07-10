using CargoWise.Application;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.GUI
{
	public static class AccountingControllerCreator
	{
		public static ZController GetNewController(AccTransactionHeader header)
		{
			return GetNewController(header, null);
		}

		public static ZController GetNewController(AccTransactionHeader header, ModuleIdentifier callingModuleID)
		{
			string ledger = "";
			string transactionType = "";
			if (header != null)
			{
				ledger = header.AH_Ledger;
				transactionType = header.AH_TransactionType;
			}
			return GetNewController(transactionType, ledger, callingModuleID);
		}

		public static ZController GetNewController(string transactionType, string ledger)
		{
			return GetNewController(transactionType, ledger, null);
		}

		public static ZController GetNewController(string transactionType, string ledger, ModuleIdentifier callingModuleID)
		{
			var controllerIDProvider = ObjectFactory.Get<IAccountingControllerIdDecider>();
			var controllerID = controllerIDProvider.GetControllerID(transactionType, ledger, callingModuleID);
			if (controllerID == null)
			{
				return null;
			}

			return ZControllerFactory.Create(controllerID);
		}
	}

	public class AccountingControllerIdDecider : IAccountingControllerIdDecider
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502: Avoid excessive complexity")]
		public ControllerID GetControllerID(string transactionType, string ledger, ModuleIdentifier callingModuleID)
		{
			switch (ledger)
			{
				case LedgerTypes.AccountsPayable:
					{
						switch (transactionType)
						{
							case TransactionTypes.Invoice:
								if (callingModuleID == ModuleIDs.APInvoiceApproval)
								{
									return ControllerIDs.APInvoiceNewForApproval;
								}
								else
								{
									return ControllerIDs.APInvoice;
								}
							case TransactionTypes.CreditNote:
								if (callingModuleID == ModuleIDs.APInvoiceApproval)
								{
									return ControllerIDs.APCreditNoteNewForApproval;
								}
								else
								{
									return ControllerIDs.APCreditNote;
								}
							case TransactionTypes.AdjustmentNote:
								return ControllerIDs.APAdjustmentNote;
							case TransactionTypes.Journal:
								if (callingModuleID == ModuleIDs.ZAPMatching)
								{
									return ControllerIDs.APBankFeeJournal;
								}
								else
								{
									return ControllerIDs.APJournal;
								}
							case TransactionTypes.Contra:
								return ControllerIDs.APContra;
							case TransactionTypes.Transfer:
								return ControllerIDs.APTransfer;
							case TransactionTypes.Receipt:
								if (callingModuleID == ModuleIDs.CashbookTransaction)
								{
									return ControllerIDs.CashBookAPReceipt;
								}
								else
								{
									return ControllerIDs.ZAPReceipt;
								}
							case TransactionTypes.Payment:
								if (callingModuleID == ModuleIDs.CashbookTransaction)
								{
									return ControllerIDs.CashBookAPPayment;
								}
								else
								{
									return ControllerIDs.ZAPPayment;
								}
							case TransactionTypes.Discount:
								return ControllerIDs.APDiscount;
							case TransactionTypes.ExchangeDifference:
								return ControllerIDs.APExchangeDifference;
							case TransactionTypes.Overpayment:
								return ControllerIDs.APOverpayment;
							default:
								return null;
						}
					}
				case LedgerTypes.AccountsReceivable:
					{
						switch (transactionType)
						{
							case TransactionTypes.Invoice:
								if (callingModuleID == ModuleIDs.UnapprovedIntercompanyTransaction)
								{
									return ControllerIDs.ARInvoiceForInterCompanyTransaction;
								}
								else
								{
									return ControllerIDs.ARInvoice;
								}
							case TransactionTypes.CreditNote:
								if (callingModuleID == ModuleIDs.UnapprovedIntercompanyTransaction)
								{
									return ControllerIDs.ARCreditNoteForInterCompanyTransaction;
								}
								else
								{
									return ControllerIDs.ARCreditNote;
								}
							case TransactionTypes.AdjustmentNote:
								return ControllerIDs.ARAdjustmentNote;
							case TransactionTypes.Journal:
								if (callingModuleID == ModuleIDs.ZARMatching)
								{
									return ControllerIDs.ARBankFeeJournal;
								}
								else
								{
									return ControllerIDs.ARJournal;
								}
							case TransactionTypes.Contra:
								return ControllerIDs.ARContra;
							case TransactionTypes.Transfer:
								return ControllerIDs.ARTransfer;
							case TransactionTypes.Receipt:
								if (callingModuleID == ModuleIDs.CashbookTransaction)
								{
									return ControllerIDs.CashBookARReceipt;
								}
								else
								{
									return ControllerIDs.ZARReceipt;
								}
							case TransactionTypes.Payment:
								if (callingModuleID == ModuleIDs.CashbookTransaction)
								{
									return ControllerIDs.CashBookARPayment;
								}
								else
								{
									return ControllerIDs.ZARPayment;
								}
							case TransactionTypes.Discount:
								return ControllerIDs.ARDiscount;
							case TransactionTypes.ExchangeDifference:
								return ControllerIDs.ARExchangeDifference;
							case TransactionTypes.Overpayment:
								return ControllerIDs.AROverpayment;
							default:
								return null;
						}
					}
				case LedgerTypes.CashBook:
					{
						switch (transactionType)
						{
							case TransactionTypes.DirectPayment:
								return ControllerIDs.DirectPayment;
							case TransactionTypes.DirectReceipt:
								return ControllerIDs.DirectReceipt;
							case TransactionTypes.OpeningPayment:
								return ControllerIDs.OpeningPayment;
							case TransactionTypes.OpeningReceipt:
								return ControllerIDs.OpeningReceipt;
							case TransactionTypes.Transfer:
								return ControllerIDs.BankTransfer;
							case TransactionTypes.ExchangeDifference:
								return ControllerIDs.BankCurrencyAdjustment;
							default:
								return null;
						}
					}
				case LedgerTypes.UnapprovedPayableTransactions:
					{
						switch (transactionType)
						{
							case TransactionTypes.UAInvoice:
								return ControllerIDs.UAInvoice;
							case TransactionTypes.UACreditNote:
								return ControllerIDs.UACreditNote;
							default:
								return null;
						}
					}
				case LedgerTypes.JobCosting:
					{
						switch (transactionType)
						{
							case TransactionTypes.Journal:
								return ControllerIDs.JCCostingJournal;
							case TransactionTypes.JobRevenueJournal:
								return ControllerIDs.JobRevenueJournal;
							default:
								return null;
						}
					}
				case LedgerTypes.IncompleteTransactions:
					{
						switch (transactionType)
						{
							case TransactionTypes.IncompleteInvoice:
								if (callingModuleID == ModuleIDs.APInvoiceApproval)
								{
									return ControllerIDs.APInvoiceLinkedToApproval;
								}
								else
								{
									return ControllerIDs.APIncompleteInvoice;
								}
							case TransactionTypes.IncompleteCreditNote:
								if (callingModuleID == ModuleIDs.APInvoiceApproval)
								{
									return ControllerIDs.APCreditNoteLinkedToApproval;
								}
								else
								{
									return ControllerIDs.APIncompleteCreditNote;
								}
							case TransactionTypes.IncompleteAdjustmentNote:
								return ControllerIDs.APIncompleteAdjustmentNote;
							default:
								return null;
						}
					}
				case LedgerTypes.TransactionsPendingAllocation:
					return ControllerIDs.TransactionsPendingAllocation;
				default:
					return null;
			}
		}
	}
}
