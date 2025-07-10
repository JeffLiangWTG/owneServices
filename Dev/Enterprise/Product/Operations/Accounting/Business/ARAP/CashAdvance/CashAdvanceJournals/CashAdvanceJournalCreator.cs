using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.CashAdvance
{
	internal abstract class CashAdvanceJournalCreator
	{
		/// <summary>
		/// Creates a Journal for a cash advance request in the same factory
		/// If journal generation is successful, NewJournalCreated event will be fired.
		/// If pre-condiiton for generating a journal is not met, then JournalCreationFailed event will be fired.
		/// </summary>
		/// <param name="requestHeader">Cash Advance for which journal will be created</param>
		internal Journal.Journal Generate(CashAdvanceRequestHeader requestHeader, Journal.Journal overpayJournal = null)
		{
			Argument.NotNull(requestHeader, nameof(requestHeader));

			OnJournalCreationBegin(requestHeader);

			var (hasError, errorMessage) = RunPreJournalCreationValidation(requestHeader);
			if (hasError)
			{
				OnNewJournalCreationFailed(requestHeader, errorMessage);
				return null;
			}

			var factory = requestHeader.Factory;
			Journal.Journal journal = GetJournalInstance(factory, requestHeader.CAH_Ledger);
			if (journal != null)
			{
				journal.DebitCreditSign = GetDebitCreditSign(requestHeader.CAH_Ledger);
				journal.AH_OH = requestHeader.CAH_OH_Organization;
				journal.AH_GC = requestHeader.CAH_GC_Company;
				journal.AH_AG = GetJournalGLAccount(requestHeader.CAH_Ledger);
				journal.AH_RX_NKTransactionCurrency = requestHeader.CAH_RX_NKTransactionCurrency;
				journal.AH_Desc = GetJournalDescription(requestHeader);
				journal.AH_ExchangeRate = requestHeader.CAH_ExchangeRate;
				((IMatching)journal).OSPartialPaymentAmount = GetOSAmount(requestHeader);
				if (overpayJournal == null)
				{
					journal.AH_LocalExTaxAmount = GetLocalAmount(requestHeader);
					journal.AH_OSExTaxAmount = GetOSAmount(requestHeader);
				}
				else
				{
					journal.AH_LocalExTaxAmount = GetLocalAmount(requestHeader) - overpayJournal.AH_LocalExTaxAmount;
					journal.AH_OSExTaxAmount = GetOSAmount(requestHeader) - overpayJournal.AH_OSExTaxAmount;
				}
				journal.AH_PostDate = GetPostDate(requestHeader);
				journal.AH_InvoiceDate = GetInvoiceDate(requestHeader);
				journal.AH_TransactionCreatedByMatching = false;

				OnNewJournalCreated(requestHeader, journal);
			}

			OnJournalCreationEnd(requestHeader);

			return journal;
		}

		protected Journal.Journal GetJournalInstance(BusinessObjectFactory factory, ZString ledger)
		{
			Journal.Journal journal = null;
			if (ledger == LedgerTypes.AccountsReceivable)
			{
				journal = factory.New<Journal.ARJournal>();
			}
			else if (ledger == LedgerTypes.AccountsPayable)
			{
				journal = factory.New<Journal.APJournal>();
			}
			journal.AH_TransactionCategory = GetJournalCategory(ledger);
			return journal;
		}

		protected abstract ZString GetJournalCategory(ZString ledger);

		protected abstract Guid GetJournalGLAccount(ZString ledger);

		protected abstract string GetDebitCreditSign(ZString ledger);

		protected abstract ZString GetJournalDescription(CashAdvanceRequestHeader requestHeader);

		protected abstract ZDateTime GetPostDate(CashAdvanceRequestHeader requestHeader);

		protected abstract ZDateTime GetInvoiceDate(CashAdvanceRequestHeader requestHeader);

		protected abstract ZDecimal GetLocalAmount(CashAdvanceRequestHeader requestHeader);

		protected abstract ZDecimal GetOSAmount(CashAdvanceRequestHeader requestHeader);

		(bool HasError, ZString ErrorMessage) RunPreJournalCreationValidation(CashAdvanceRequestHeader requestHeader)
		{
			var errorMessageBuilder = new ZStringBuilder();
			RunPreJournalCreationValidationCore(requestHeader, errorMessageBuilder);
			return errorMessageBuilder.IsEmpty ? (false, string.Empty) : (true, errorMessageBuilder.ToStringWithNewLineBetweenAppends());
		}

		protected virtual void RunPreJournalCreationValidationCore(CashAdvanceRequestHeader requestHeader, ZStringBuilder errorMessageBuilder)
		{
			var glAccount = GetJournalGLAccount(requestHeader.CAH_Ledger);
			if (Guid.Empty == glAccount)
			{
				var ledgerText = requestHeader.CAH_Ledger == LedgerTypes.AccountsReceivable
					? Res.GetString("718a950d-2a78-4f68-acb9-d68fab15dcb4", "Receivables")
					: Res.GetString("5a6f8070-2582-4043-89ca-01cfad358486", "Payables");
				var registryPath = requestHeader.CAH_Ledger == LedgerTypes.AccountsReceivable ? AccountingConfigurationRegistry.Instance.CashAdvanceClearingAccount.GetLocation() : AccountingConfigurationRegistry.Instance.PayablesCashAdvanceClearingAccount.GetLocation();
				errorMessageBuilder.Append(Res.GetString("649217a4-c71b-4cf4-ac85-ca2a363b7c07", @"This transaction cannot be matched.
On matching this transaction, a journal to the {0} Advance Payment Clearing Account will be created, which requires a Advance Payment Clearing Account to be recorded in the {1} registry.
Please ensure this registry has an account recorded and then match the transaction.", ledgerText, registryPath));
			}
		}

		protected virtual void OnJournalCreationBegin(CashAdvanceRequestHeader requestHeader)
		{ }

		protected virtual void OnJournalCreationEnd(CashAdvanceRequestHeader requestHeader)
		{ }

		internal bool IsGenerateRequired(CashAdvanceRequestHeader requestHeader)
		{
			return IsGenerateRequiredCore(requestHeader);
		}

		protected virtual bool IsGenerateRequiredCore(CashAdvanceRequestHeader requestHeader) => true;

		#region NewJournalCreated

		void OnNewJournalCreated(CashAdvanceRequestHeader requestHeader, Journal.Journal journal)
		{
			OnNewJournalCreatedCore(requestHeader, journal);
			RaiseNewJournalCreatedEvent(requestHeader, journal);
		}

		protected virtual void OnNewJournalCreatedCore(CashAdvanceRequestHeader requestHeader, Journal.Journal journal)
		{
		}

		void RaiseNewJournalCreatedEvent(CashAdvanceRequestHeader requestHeader, Journal.Journal journal)
		{
			NewJournalCreated?.Invoke(requestHeader, journal);
		}

		internal event Action<CashAdvanceRequestHeader, Journal.Journal> NewJournalCreated;

		#endregion

		#region JournalCreationFailed

		void OnNewJournalCreationFailed(CashAdvanceRequestHeader requestHeader, string errorMessage)
		{
			OnNewJournalCreationFailedCore(requestHeader, errorMessage);
			RaiseNewJournalCreationFailedEvent(requestHeader, errorMessage);
		}

		protected virtual void OnNewJournalCreationFailedCore(CashAdvanceRequestHeader requestHeader, string errorMessage)
		{
		}

		void RaiseNewJournalCreationFailedEvent(CashAdvanceRequestHeader requestHeader, string errorMessage)
		{
			JournalCreationFailed?.Invoke(requestHeader, errorMessage);
		}

		internal event Action<CashAdvanceRequestHeader, string> JournalCreationFailed;

		#endregion
	}
}
