using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Base.Matching
{
	public class DynamicTransactionCreatorClearingJournal : DynamicTransactionCreator
	{
		public DynamicTransactionCreatorClearingJournal(IMatchingCollection transactions)
			: base(transactions.Factory)
		{
			Transactions = transactions;
		}

		readonly IMatchingCollection Transactions;

		public static bool IsJournalShouldBeCreated
		{
			get { return AccountingConfigurationRegistry.Instance.ClearingJournalConfiguration.Value != AccountingConstants.ClearingJournalConfigurationTypes.Standard.Code; }
		}

		public static string IsInvoiceCanBePartlyPaid(InvoicingBase invoice)
		{
			string message = "";

			string clearingJournalConfiguration = AccountingConfigurationRegistry.Instance.ClearingJournalConfiguration.Value;
			if ((clearingJournalConfiguration == AccountingConstants.ClearingJournalConfigurationTypes.LineBranchPerHeader.Code ||
				 clearingJournalConfiguration == AccountingConstants.ClearingJournalConfigurationTypes.LineBranchPerMatching.Code)
				&& ((IMatching)invoice).LocalPartialPaymentAmount != invoice.AH_LocalTotal
				&& !invoice.Lines.Any(line => ((InvoicingLineBase)line).TransLinePaysTotalAmount != 0M)
				&& !invoice.Lines.Any(line => ((ILineMatching)line).LocalPaidAmount != 0M))
			{
				ZGuid firstBranchPK = invoice.Lines[0].AL_GB;
				if (!invoice.Lines.All(line => ((InvoicingLineBase)line).AL_GB == firstBranchPK))
				{
					message = Res.GetString("86A7B95C-451C-4A85-8CC2-FAFD2201A4BD", "This transaction with different branches in lines is being partly paid without matching lines. Clearing Journal Configuration is set to create journals by line branches. To create Clearing Journal lines the transaction paid amount will be divided proportionally to transaction line amounts.");
				}
			}

			return message;
		}

		public override IMatchingCollection CreateTransactions()
		{
			IMatchingCollection newTransactions = new IMatchingCollection(Factory);

			if (IsJournalShouldBeCreated)
			{
				string clearingJournalConfiguration = AccountingConfigurationRegistry.Instance.ClearingJournalConfiguration.Value;
				Func<IMatching, string> getDescription = transaction =>
						transaction.Ledger + " " + transaction.TransactionType +
						(transaction.TransactionCategory.IsEmpty ? "" : " " + transaction.TransactionCategory) +
						(transaction.TransactionNumber.IsEmpty ? " " + TransactionNumberPlaceholder : " " + transaction.TransactionNumber) +
						(transaction.Ledger == LedgerTypes.AccountsReceivable && !transaction.ConsolidatedRef.IsEmpty ? " " + transaction.ConsolidatedRef : "");
				var journalData =
						from IMatching transaction in Transactions
						select new
						{
							Transaction = transaction,
							Ledger = transaction.Ledger,
							Organisation = transaction.Organisation,
							Branch = transaction.BranchGuid,
							LocalPartialPaymentAmount = transaction.LocalPartialPaymentAmount,
							Description = getDescription(transaction)
						};

				if (clearingJournalConfiguration == AccountingConstants.ClearingJournalConfigurationTypes.LineBranchPerHeader.Code ||
					clearingJournalConfiguration == AccountingConstants.ClearingJournalConfigurationTypes.LineBranchPerMatching.Code)
				{
					int localDecimals = GlbCompany.CurrentCompany.LocalCurrency.Decimals;
					Func<InvoicingBase, InvoicingLineBase, ZDecimal> getApportionedAmount = (invoice, line) =>
						Utilities.Round((line.AL_LineAmount + line.AL_GSTVAT) * ((IMatching)invoice).LocalPartialPaymentAmount / invoice.AH_LocalTotal, localDecimals);

					var preprocessedTransactions =
						(from transaction in Transactions
						 where transaction is InvoicingBase
						 let invoice = (InvoicingBase)transaction
						 let firstInvoiceLine = invoice.Lines[0]
						 let onlyOneBranchInvoice = invoice.Lines.All(line => ((InvoicingLineBase)line).AL_GB == firstInvoiceLine.AL_GB)
						 let isPaidByLines = invoice.Lines.Any(line => ((ILineMatching)line).LocalPaidAmount != 0M)
						 let isFullyPaid = ((IMatching)invoice).LocalPartialPaymentAmount == invoice.AH_LocalTotal
						 let needAmountApportionment = !onlyOneBranchInvoice && !isFullyPaid && !isPaidByLines
						 let maxLine = !needAmountApportionment ? null :
											(from InvoicingLineBase lineForMaxCalculation in invoice.Lines
											 where Math.Abs(lineForMaxCalculation.AL_LineAmount + lineForMaxCalculation.AL_GSTVAT) ==
													invoice.Lines.Max(currentLine => Math.Abs(((InvoicingLineBase)currentLine).AL_LineAmount + ((InvoicingLineBase)currentLine).AL_GSTVAT))
											 select lineForMaxCalculation).First()
						 let apportionDifference = !needAmountApportionment ? 0M :
								((IMatching)invoice).LocalPartialPaymentAmount - invoice.Lines.Sum(line => getApportionedAmount(invoice, (InvoicingLineBase)line))
						 select new
						 {
							 Transaction = invoice,
							 Lines = from InvoicingLineBase invoiceLine in invoice.Lines
									 select new
									 {
										 InvoicePK = invoiceLine.AL_AH,
										 BranchPK = invoiceLine.AL_GB,
										 TotalLocalAmount = invoiceLine.AL_LineAmount + invoiceLine.AL_GSTVAT,
										 LocalPaidAmount = ((ILineMatching)invoiceLine).LocalPaidAmount,
										 ApportionedAmount = !needAmountApportionment ? 0M :
												getApportionedAmount(invoice, invoiceLine) + (maxLine.PK == invoiceLine.PK ? apportionDifference : 0M)
									 },
							 OnlyOneBranchInvoice = onlyOneBranchInvoice,
							 IsPaidByLines = isPaidByLines,
							 IsFullyPaid = isFullyPaid
						 }).ToArray();

					var journalDataForInvoicesOnly =
						from transaction in preprocessedTransactions
						from line in transaction.Lines
						group line by new { InvoicePK = line.InvoicePK, Branch = line.BranchPK } into lineGroup
						let firstLinePreprocessedHeader = (from preprocessedTransaction in preprocessedTransactions where preprocessedTransaction.Transaction.PK == lineGroup.Key.InvoicePK select preprocessedTransaction).First()
						let firstLineIMatching = (IMatching)firstLinePreprocessedHeader.Transaction
						select new
						{
							Transaction = firstLineIMatching,
							Ledger = firstLineIMatching.Ledger,
							Organisation = firstLineIMatching.Organisation,
							Branch = lineGroup.Key.Branch,
							LocalPartialPaymentAmount = (ZDecimal)(firstLinePreprocessedHeader.OnlyOneBranchInvoice ? (decimal)firstLineIMatching.LocalPartialPaymentAmount :
								firstLinePreprocessedHeader.IsFullyPaid ? lineGroup.Sum(line => line.TotalLocalAmount) :
								firstLinePreprocessedHeader.IsPaidByLines ? lineGroup.Sum(line => line.LocalPaidAmount) :
																			lineGroup.Sum(line => line.ApportionedAmount)),
							Description = getDescription(firstLineIMatching)
						};

					if (clearingJournalConfiguration == AccountingConstants.ClearingJournalConfigurationTypes.LineBranchPerHeader.Code)
					{
						journalData = journalDataForInvoicesOnly.Concat(journalData.Where(journal => !(journal.Transaction is InvoicingBase)));
					}
					else if (clearingJournalConfiguration == AccountingConstants.ClearingJournalConfigurationTypes.LineBranchPerMatching.Code)
					{
						string journalDescription = Res.GetString("ABAC493C-CC7B-4B29-A650-04D69C8F6ABA", "MATCHING CLEARING JNL");
						journalData =
							from IMatching transaction in Transactions
							where !(transaction is InvoicingBase)
							group transaction by new { Branch = transaction.BranchGuid, Ledger = transaction.Ledger, Organisation = transaction.Organisation } into transactionGroup
							select new
							{
								Transaction = transactionGroup.First(),
								Ledger = transactionGroup.Key.Ledger,
								Organisation = transactionGroup.Key.Organisation,
								Branch = transactionGroup.Key.Branch,
								LocalPartialPaymentAmount = (ZDecimal)transactionGroup.Sum(transaction => transaction.LocalPartialPaymentAmount),
								Description = ""
							};
						journalData = journalDataForInvoicesOnly.Concat(journalData);
						journalData =
							from journal in journalData
							group journal by new { Branch = journal.Branch, Ledger = journal.Ledger, Organisation = journal.Organisation } into journalGroup
							select new
							{
								Transaction = journalGroup.First().Transaction,
								Ledger = journalGroup.Key.Ledger,
								Organisation = journalGroup.Key.Organisation,
								Branch = journalGroup.Key.Branch,
								LocalPartialPaymentAmount = (ZDecimal)journalGroup.Sum(journal => journal.LocalPartialPaymentAmount),
								Description = journalGroup.Key.Ledger + " " + journalDescription
							};
					}
				}

				foreach (var journal in journalData)
				{
					if (!journal.LocalPartialPaymentAmount.IsEmpty)
					{
						Journal dynamicJournal = (Journal)Factory.New(journal.Ledger == LedgerTypes.AccountsPayable ? typeof(APJournal) : typeof(ARJournal));
						dynamicJournal.IsAutoGenerated = true;
						dynamicJournal.AH_TransactionCategory = Constants.TransactionCategory.Codes.Clearing;
						dynamicJournal.AH_AG = AccountingConfigurationRegistry.Instance.ClearingJournalClearingAccount.Value;
						dynamicJournal.AH_OH = journal.Organisation;
						dynamicJournal.AH_GB = journal.Branch;
						dynamicJournal.AH_Desc = journal.Description;
						dynamicJournal.DebitCreditSign = journal.LocalPartialPaymentAmount > 0 ? DebitCreditDataEntry.DR : DebitCreditDataEntry.CR;
						dynamicJournal.AH_OSExTaxAmount = Math.Abs(journal.LocalPartialPaymentAmount);
						dynamicJournal.AH_TransactionCreatedByMatching = true;

						dynamicJournal.HookAH_TransactionNum_ValueChangedToCompleteDescription(journal.Transaction as TransactionHeader);

						newTransactions.Add(dynamicJournal);
					}
				}
			}

			return newTransactions;
		}

		public const string TransactionNumberPlaceholder = "{TransactionNumber}";
	}
}