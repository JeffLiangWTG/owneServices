using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.ZArchitecture.Modules;
using Ledgers = Enterprise.ZArchitecture.Core.LedgerTypes;
using TransactionTypes = Enterprise.ZArchitecture.Core.TransactionTypes;

namespace Enterprise.Accounting.GUI
{
	public class TransactionHeaderControllerIDLookup
	{
		public ControllerID GetControllerID(TransactionHeader transaction)
		{
			if (transaction == null)
			{
				throw new ArgumentException("Transaction cannot be null");
			}

			return GetControllerID(transaction.AH_Ledger, transaction.AH_TransactionType);
		}

		public ControllerID GetControllerID(ZString ledger, ZString transactionType)
		{
			ZString key = GetKey(ledger, transactionType);
			return Controllers.ContainsKey(key) ? Controllers[key] : null;
		}

		Dictionary<string, ControllerID> Controllers
		{
			get
			{
				if (fControllers == null)
				{
					fControllers = new Dictionary<string, ControllerID>();
					AddAccountsReceivableControllerIDs(fControllers);
					AddAccountsPayableControllerIDs(fControllers);
				}

				return fControllers;
			}
		}
		Dictionary<string, ControllerID> fControllers;

		void AddAccountsPayableControllerIDs(Dictionary<string, ControllerID> dictionary)
		{
			AddToDictionary(Ledgers.AccountsPayable, TransactionTypes.AdjustmentNote, ControllerIDs.APAdjustmentNote, dictionary);
			AddToDictionary(Ledgers.AccountsPayable, TransactionTypes.Contra, ControllerIDs.APContra, dictionary);
			AddToDictionary(Ledgers.AccountsPayable, TransactionTypes.CreditNote, ControllerIDs.APCreditNote, dictionary);
			AddToDictionary(Ledgers.AccountsPayable, TransactionTypes.Discount, ControllerIDs.APDiscount, dictionary);
			AddToDictionary(Ledgers.AccountsPayable, TransactionTypes.ExchangeDifference, ControllerIDs.APExchangeDifference, dictionary);
			AddToDictionary(Ledgers.AccountsPayable, TransactionTypes.Invoice, ControllerIDs.APInvoice, dictionary);
			AddToDictionary(Ledgers.AccountsPayable, TransactionTypes.Journal, ControllerIDs.APJournal, dictionary);
			AddToDictionary(Ledgers.AccountsPayable, TransactionTypes.Overpayment, ControllerIDs.APOverpayment, dictionary);
			AddToDictionary(Ledgers.AccountsPayable, TransactionTypes.Payment, ControllerIDs.ZAPPayment, dictionary);
			AddToDictionary(Ledgers.AccountsPayable, TransactionTypes.Receipt, ControllerIDs.ZAPReceipt, dictionary);
			AddToDictionary(Ledgers.AccountsPayable, TransactionTypes.Transfer, ControllerIDs.APTransfer, dictionary);
		}

		void AddAccountsReceivableControllerIDs(Dictionary<string, ControllerID> dictionary)
		{
			AddToDictionary(Ledgers.AccountsReceivable, TransactionTypes.AdjustmentNote, ControllerIDs.ARAdjustmentNote, dictionary);
			AddToDictionary(Ledgers.AccountsReceivable, TransactionTypes.Contra, ControllerIDs.ARContra, dictionary);
			AddToDictionary(Ledgers.AccountsReceivable, TransactionTypes.CreditNote, ControllerIDs.ARCreditNote, dictionary);
			AddToDictionary(Ledgers.AccountsReceivable, TransactionTypes.Discount, ControllerIDs.ARDiscount, dictionary);
			AddToDictionary(Ledgers.AccountsReceivable, TransactionTypes.ExchangeDifference, ControllerIDs.ARExchangeDifference, dictionary);
			AddToDictionary(Ledgers.AccountsReceivable, TransactionTypes.Invoice, ControllerIDs.ARInvoice, dictionary);
			AddToDictionary(Ledgers.AccountsReceivable, TransactionTypes.Journal, ControllerIDs.ARJournal, dictionary);
			AddToDictionary(Ledgers.AccountsReceivable, TransactionTypes.Overpayment, ControllerIDs.AROverpayment, dictionary);
			AddToDictionary(Ledgers.AccountsReceivable, TransactionTypes.Payment, ControllerIDs.ZARPayment, dictionary);
			AddToDictionary(Ledgers.AccountsReceivable, TransactionTypes.Receipt, ControllerIDs.ZARReceipt, dictionary);
			AddToDictionary(Ledgers.AccountsReceivable, TransactionTypes.Transfer, ControllerIDs.ARTransfer, dictionary);
		}

		void AddToDictionary(ZString ledger, ZString transactionType, ControllerID iD, Dictionary<string, ControllerID> dictionary)
		{
			dictionary.Add(GetKey(ledger, transactionType), iD);
		}

		ZString GetKey(ZString ledger, ZString transactionType)
		{
			return ledger + "|" + transactionType;
		}
	}
}
