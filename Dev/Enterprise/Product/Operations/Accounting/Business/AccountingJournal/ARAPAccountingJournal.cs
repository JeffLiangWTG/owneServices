using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.DocumentEngineIntegration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business
{
	public class ARAPAccountingJournal : AccountingJournal, ISourceIdentifierProvider
	{
		public ARAPAccountingJournal(TransactionHeader transaction, ReadOnlyBusinessObjectFactory factory, AccReportingBook reportingBook = null, DataTable generalLedgerTransactionData = null)
			: base(transaction, factory, reportingBook, generalLedgerTransactionData)
		{
			PopulateJournalLines();
			PopulateTaxDetails();
		}

		void PopulateTaxDetails()
		{
			if (GeneralLedgerTransactionData != null && GeneralLedgerTransactionData.Rows.Count != 0)
			{
				var rows = GeneralLedgerTransactionData.Select($"TransactionHeaderID = '{transaction.PK.ToGuid()}' and GLType = 'TGM'");
				TaxDetails = AccountingJournalTaxDetailForReportingBook.Create(Factory, ObjectFactory.Get<ITaxProcessor>().GetTaxDetailsForAccountingJournal(Factory, transaction.PK), reportingBook, rows);
			}
			else
			{
				var useGeneralLedgerData = AccountingConfigurationRegistry.Instance.GenerateAndStoreJournalEntriesForPostedAccountingTransactions.Value;
				TaxDetails = AccountingJournalTaxDetail.Create(Factory, ObjectFactory.Get<ITaxProcessor>().GetTaxDetailsForAccountingJournal(Factory, transaction.PK, useGeneralLedgerData));
			}
		}

		public override Dictionary<ZString, ZString> ApplicableOptionalFields
		{
			get
			{
				if (applicableOptionalFields_innervalue == null)
				{
					applicableOptionalFields_innervalue = new Dictionary<ZString, ZString>();

					if (transaction.AH_Ledger == LedgerTypes.AccountsReceivable && !new ZString[] { TransactionTypes.Contra, TransactionTypes.Transfer }.Contains(transaction.AH_TransactionType))
					{
						applicableOptionalFields_innervalue.Add(DebtorText, transaction.AH_OH.IsEmpty ? ZString.Empty : Factory.Load<OrgHeader>(transaction.AH_OH).OH_Code);
					}

					if (transaction.AH_Ledger == LedgerTypes.AccountsPayable && !new ZString[] { TransactionTypes.Contra, TransactionTypes.Transfer }.Contains(transaction.AH_TransactionType))
					{
						applicableOptionalFields_innervalue.Add(CreditorText, transaction.AH_OH.IsEmpty ? ZString.Empty : Factory.Load<OrgHeader>(transaction.AH_OH).OH_Code);
					}

					if ((transaction.AH_Ledger == LedgerTypes.AccountsPayable || transaction.AH_Ledger == LedgerTypes.AccountsReceivable) && transaction.AH_TransactionType == TransactionTypes.Contra)
					{
						AddOptionalFieldsForCTRTransactionWhenARAPLedger();
					}

					if ((transaction.AH_Ledger == LedgerTypes.AccountsPayable || transaction.AH_Ledger == LedgerTypes.AccountsReceivable) && transaction.AH_TransactionType == TransactionTypes.Transfer)
					{
						AddOptionalFieldsForTRFTransactionWhenARAPLedger();
					}

					if ((transaction.AH_Ledger == LedgerTypes.AccountsReceivable || transaction.AH_Ledger == LedgerTypes.AccountsPayable)
						 && (transaction.AH_TransactionType == TransactionTypes.Receipt || transaction.AH_TransactionType == TransactionTypes.Payment))
					{
						applicableOptionalFields_innervalue.Add(BankCodeText, transaction.AH_AB.IsEmpty ? ZString.Empty : Factory.Load<AccBankAccount>(transaction.AH_AB).AB_BankAbbreviation);
					}

					if ((transaction.AH_Ledger == LedgerTypes.AccountsReceivable || transaction.AH_Ledger == LedgerTypes.AccountsPayable)
					 && (transaction.AH_TransactionType == TransactionTypes.Invoice || transaction.AH_TransactionType == TransactionTypes.CreditNote || transaction.AH_TransactionType == TransactionTypes.AdjustmentNote))
					{
						applicableOptionalFields_innervalue.Add(StatusText, GetJournalStatus());

						if (!transaction.AH_GB_TaxBranch.IsEmpty)
						{
							applicableOptionalFields_innervalue.Add(TaxBranchText, transaction.TaxBranch.GB_Code);
						}
					}
				}

				return applicableOptionalFields_innervalue;
			}
		}
		Dictionary<ZString, ZString> applicableOptionalFields_innervalue;

		public override bool GroupByLinesWhilePrinting
		{
			get
			{
				if (transaction.AH_TransactionType == TransactionTypes.Contra || transaction.AH_TransactionType == TransactionTypes.Transfer)
				{
					return false;
				}
				else
				{
					return base.GroupByLinesWhilePrinting;
				}
			}
		}

		protected override IEnumerable<ZString> ValidLedgerTypes
		{
			get
			{
				return new ZString[]
				{
					LedgerTypes.AccountsReceivable,
					LedgerTypes.AccountsPayable
				};
			}
		}

		protected override IEnumerable<ZString> ValidTransactionTypes
		{
			get
			{
				return new ZString[]
				{
					TransactionTypes.Invoice,
					TransactionTypes.CreditNote,
					TransactionTypes.AdjustmentNote,
					TransactionTypes.ExchangeDifference,
					TransactionTypes.Discount,
					TransactionTypes.Overpayment,
					TransactionTypes.Payment,
					TransactionTypes.Receipt,
					TransactionTypes.Contra,
					TransactionTypes.Journal,
					TransactionTypes.Transfer
				};
			}
		}

		ZGuid ISourceIdentifierProvider.SourceIdentifier => transaction.PK;

		void AddOptionalFieldsForTRFTransactionWhenARAPLedger()
		{
			ZQuery transferRowFilter = new ZQuery(AccTransactionHeaderSchema.AH_Ledger, transaction.AH_Ledger);
			transferRowFilter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Transfer);
			transferRowFilter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionNum, transaction.AH_TransactionNum);
			transferRowFilter.AddToFilter(AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, transaction.PK);
			var relatedTransactions = new TransactionHeaderCollection(Factory, transferRowFilter);
			relatedTransactions.Load();
			if (relatedTransactions.Count == 1 && relatedTransactions[0] is TransferRow)
			{
				bool isFromTransaction = (transaction.AH_TransactionCount == AccTransactionHeader.TransactionCountConstants.BankTransferFromRow) || (transaction.AH_TransactionCount == AccTransactionHeader.TransactionCountConstants.BankTransferFromRowWhenReversing);
				var fromTransaction = isFromTransaction ? transaction : relatedTransactions[0];
				var toTransaction = isFromTransaction ? relatedTransactions[0] : transaction;

				var fromText = transaction.AH_Ledger == LedgerTypes.AccountsPayable ? Res.GetString("9228baea-94ef-44a2-9349-30ab881a2f65", "FROM CREDITOR") : Res.GetString("782a7ce7-3ea0-4a65-9f3a-842a268e0f58", "FROM DEBTOR");
				var toText = transaction.AH_Ledger == LedgerTypes.AccountsPayable ? Res.GetString("c4c7b646-b982-4495-ab41-510647e0afe0", "TO CREDITOR") : Res.GetString("f1102f13-5835-41e5-80cd-be7c1e9bcd6c", "TO DEBTOR");

				applicableOptionalFields_innervalue.Add(fromText, fromTransaction.Header.OH_Code);
				applicableOptionalFields_innervalue.Add(toText, toTransaction.Header.OH_Code);
			}
		}

		void AddOptionalFieldsForCTRTransactionWhenARAPLedger()
		{
			ZQuery contraRowFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Contra);
			contraRowFilter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionNum, transaction.AH_TransactionNum);
			contraRowFilter.AddToFilter(AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, transaction.PK);
			var relatedTransactions = new TransactionHeaderCollection(Factory, contraRowFilter);
			relatedTransactions.Load();
			if (relatedTransactions.Count == 1 && relatedTransactions[0] is ContraRow)
			{
				var debtorTransaction = transaction.AH_Ledger == LedgerTypes.AccountsReceivable ? transaction : relatedTransactions[0];
				var creditorTransaction = transaction.AH_Ledger == LedgerTypes.AccountsReceivable ? relatedTransactions[0] : transaction;

				applicableOptionalFields_innervalue.Add(DebtorText, debtorTransaction.Header.OH_Code);
				applicableOptionalFields_innervalue.Add(CreditorText, creditorTransaction.Header.OH_Code);
			}
		}
	}
}
