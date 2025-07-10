using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.CashAdvance
{
	internal static class CashAdvanceMatchingHandler
	{
		internal static void MatchWithInvoice(Invoice matchingInvoice, List<CashAdvanceMatchingTransactionDetail> cashAdvanceMatchingDetails)
		{
			//Validate first
			var errorMessage = Validate();
			if (!string.IsNullOrEmpty(errorMessage))
			{
				throw new CannotMatchCashAdvanceJournalWithInvoiceException(errorMessage);
			}

			//Collect all transactions for matching
			var transactionsForMatching = new Dictionary<BusinessObject, ZDecimal>();
			transactionsForMatching.Add(matchingInvoice, 0);
			cashAdvanceMatchingDetails.Select(d => d.CashAdvanceInvoicedJournal)
									.ForEach(j =>  transactionsForMatching.Add(j, 0));

			//Build MatchingBizO
			var matchingBizO = GetMatchingBizOInstance();
			matchingBizO.PrimaryOrganization = matchingInvoice.Header.PK;
			var maxPostDate = transactionsForMatching.Keys.Select(t => ((TransactionHeader)t).AH_PostDate.Date).Max();
			matchingBizO.MatchDate = maxPostDate;

			//Update Invoice OSPartialPaymentAmount
			var multiplier = Math.Sign((matchingInvoice as IMatching)?.OSOutstandingAmount ?? ZDecimal.Zero);
			var canMatchAtLineLevel = true;
			foreach (var matchingTransactionDetail in cashAdvanceMatchingDetails)
			{
				canMatchAtLineLevel = !matchingTransactionDetail.InvoiceLineToRequestLineMapping.Any(kvp => kvp.Key.AL_OSAmount < kvp.Value.CashAdvanceRequestLine.CAL_OSAmount);
				if (!canMatchAtLineLevel)
				{
					break;
				}
			}

			if (canMatchAtLineLevel)
			{
				matchingBizO.MoveFromUnmatchToMatch(transactionsForMatching);
				UpdateInvoicePaidAmountByLine();
			}
			else
			{
				UpdateInvoicePaidAmountAtHeader();
				matchingBizO.MoveFromUnmatchToMatch(transactionsForMatching);
			}

			//Create EXX transaction
			var exxTransaction = CreateBalancingEXXIfRequires();
			if (exxTransaction != null)
			{
				matchingBizO.AddMiscellaneousTransaction(exxTransaction);
			}

			if (matchingBizO.HasErrors)
			{
				var errorMessageBuilder = new ZStringBuilder(matchingBizO.GetErrors().Select(n => n.Message));
				var fullErrorMessage = Res.GetString("c01b978e-9926-4c2c-9ca5-c2353408f682", "Matching failed for transaction {0} with following validation errors -\r\n{1}"
													, matchingInvoice.AH_TransactionNum
													, errorMessageBuilder.ToStringWithNewLineBetweenAppends());
				throw new CannotMatchCashAdvanceJournalWithInvoiceException(fullErrorMessage);
			}

			//Now do the Matching
			var matchingResult = false;
			if (matchingBizO.Balance == 0m && !matchingBizO.HasErrors)
			{
				matchingBizO.DoNotSaveFactoryOnMatching = true;
				matchingResult = matchingBizO.MatchAndClearTransactions();
			}

			if (!matchingResult)
			{
				if (exxTransaction != null)
				{
					matchingBizO.DeleteMiscTransaction(exxTransaction);
				}
				throw new CannotMatchCashAdvanceJournalWithInvoiceException(Res.GetString("f6dd22f7-9bcd-4f12-8540-6f1992a1b9f1", "Matching failed for transaction {0}.", matchingInvoice.AH_TransactionNum));
			}

			#region Inner functions

			void UpdateInvoicePaidAmountByLine()
			{
				var mediator = new InvoicingBasePayLineMediator(matchingBizO, matchingInvoice);
				if (mediator.Lines.Any())
				{
					foreach (ILineMatching mline in mediator.Lines)
					{
						mline.PaidAmount = 0M;
					}

					foreach (var matchingTransactionDetail in cashAdvanceMatchingDetails)
					{
						foreach (var mapping in matchingTransactionDetail.InvoiceLineToRequestLineMapping)
						{
							if (mapping.Key is ILineMatching matchingLine)
							{
								matchingLine.PaidAmount = multiplier * mapping.Value.CashAdvanceRequestLine.CAL_OSAmount;
							}
						}
					}
					mediator.ConveyData();
				}
			}

			void UpdateInvoicePaidAmountAtHeader()
			{
				var osPaidAmountWithoutMultiplier = cashAdvanceMatchingDetails.Sum(d => d.CashAdvanceInvoicedJournal.AH_OSExTaxAmount);
				var localPaidAmountWithoutMultiplier = cashAdvanceMatchingDetails.Sum(d => d.CashAdvanceInvoicedJournal.AH_LocalExTaxAmount);
				if (matchingInvoice is ISupportMatchingOfMyLines matchingOfMyLines)
				{
					matchingOfMyLines.LineTotalPaidAmount = multiplier * osPaidAmountWithoutMultiplier;
					matchingOfMyLines.LineTotalLocalPaidAmount = multiplier * localPaidAmountWithoutMultiplier;
				}
				transactionsForMatching[matchingInvoice] = multiplier * osPaidAmountWithoutMultiplier;
			}

			TransactionHeader CreateBalancingEXXIfRequires()
			{
				var invoiceAmount = ((IMatching)matchingInvoice).OSPartialPaymentAmount;
				var journalAmount = cashAdvanceMatchingDetails.Select(d => d.CashAdvanceInvoicedJournal)
										.Cast<IMatching>()
										.Sum(j => j.OSPartialPaymentAmount);

				if (matchingBizO.Balance != 0 && (invoiceAmount + journalAmount == 0))
				{
					return matchingBizO.GetMiscellaneousTransaction(TransactionTypes.ExchangeDifference);
				}
				return null;
			}

			MatchingBase GetMatchingBizOInstance()
			{
				if (matchingInvoice.AH_Ledger == LedgerTypes.AccountsReceivable)
				{
					return new ARMatchingBase(matchingInvoice.Factory, false);
				}
				else if (matchingInvoice.AH_Ledger == LedgerTypes.AccountsPayable)
				{
					return new APMatchingBase(matchingInvoice.Factory, false);
				}
				else
				{
					throw new InvalidOperationException(FormattableString.Invariant($"Cannot match Advance Payment for ledger {matchingInvoice.AH_Ledger}"));
				}
			}

			string Validate()
			{
				var overpaidCAHInfo = new List<string>();
				var errorMessageBuilder = new ZStringBuilder();
				var totalInvoicedCashAdvanceLineAmount = 0M;
				foreach (var matchingTransactionDetail in cashAdvanceMatchingDetails)
				{
					if (matchingInvoice.AH_RX_NKTransactionCurrency != matchingTransactionDetail.CashAdvanceInvoicedJournal.AH_RX_NKTransactionCurrency)
					{
						errorMessageBuilder.AppendLine(GetCurrencyMismatchErrorMessage(matchingTransactionDetail.RequestHeader
																					, matchingTransactionDetail.CashAdvanceInvoicedJournal
																					, matchingInvoice));
					}
					totalInvoicedCashAdvanceLineAmount += matchingTransactionDetail.CashAdvanceInvoicedJournal.AH_OSTotal;
					if (matchingTransactionDetail.InvoiceLineToRequestLineMapping.Any(kvp => kvp.Key.AL_OSAmount != kvp.Value.CashAdvanceRequestLine.CAL_OSPaidAmount))
					{
						overpaidCAHInfo.Add(FormattableString.Invariant($"{matchingTransactionDetail.RequestHeader.Organization.OH_Code} | {matchingTransactionDetail.RequestHeader.Job.JH_JobNum} | {matchingTransactionDetail.RequestHeader.CAH_RequestReferenceNumber} | {matchingTransactionDetail.RequestHeader.CAH_OSAmount} | {matchingTransactionDetail.RequestHeader.CAH_RX_NKTransactionCurrency}"));
					}
				}

				if (totalInvoicedCashAdvanceLineAmount > matchingInvoice.AH_OSTotal && matchingInvoice.AH_Ledger == LedgerTypes.AccountsReceivable)
				{
					errorMessageBuilder.AppendLine(GetOverPaidErrorMessageForAR(matchingInvoice.AH_OSTotal, totalInvoicedCashAdvanceLineAmount, matchingInvoice.AH_RX_NKTransactionCurrency));
					if (overpaidCAHInfo.Any())
					{
						errorMessageBuilder.AppendLine(string.Join("\r\n", overpaidCAHInfo));
					}
				}
				else if (totalInvoicedCashAdvanceLineAmount > -1 * matchingInvoice.AH_OSTotal && matchingInvoice.AH_Ledger == LedgerTypes.AccountsPayable)
				{
					errorMessageBuilder.AppendLine(GetOverPaidErrorMessageForAP(matchingInvoice.AH_OSTotal, totalInvoicedCashAdvanceLineAmount, matchingInvoice.AH_RX_NKTransactionCurrency));
					if (overpaidCAHInfo.Any())
					{
						errorMessageBuilder.AppendLine(string.Join("\r\n", overpaidCAHInfo));
					}
				}

				return errorMessageBuilder.ToStringWithNewLineBetweenAppends();
			}

			#endregion
		}

		internal static void CreateCashAdvancePaidOrReceivedMatchingJournals(ZDateTime matchDate
																		, CashAdvanceRequestHeader[] selectedCAHs
																		, Action<CashAdvanceRequestHeader, Journal.Journal> newJournalCreatedEventHandler
																		, Action<CashAdvanceRequestHeader, string> journalCreationFailedEventHandler)
		{
			var context = new CashAdvanceJournalContext(new CashAdvanceReceiptOrPaymentJournalCreator(matchDate)
																	, newJournalCreatedEventHandler: newJournalCreatedEventHandler
																	, journalCreationFailedEventHandler: journalCreationFailedEventHandler);
			foreach (var cah in selectedCAHs)
			{
				if (cah.CanMatchingJournalBeCreated)
				{
					if (context != null)
					{
						context.CreateJournal(cah);
					}
				}
			}
		}

		static string GetOverPaidErrorMessageForAP(ZDecimal invoiceAmount, ZDecimal totalInvoicedCashAdvanceLineAmount, ZString currency)
		{
			return Res.GetString("8be340fe-d8e7-4c57-b2eb-19579ec25f8e", "Invoice Total [{0} {1}] is less than the Advance Payment paid [{0} {2}]. Unable to apply Advance Payment to the invoice. Please correct the cost amount, or cancel and re-create the Advance Payment to post this invoice"
																	, currency
																	, invoiceAmount
																	, totalInvoicedCashAdvanceLineAmount);
		}

		static string GetOverPaidErrorMessageForAR(ZDecimal invoiceAmount, ZDecimal totalInvoicedCashAdvanceLineAmount, ZString currency)
		{
			return Res.GetString("c75ccd78-e199-400e-999f-fea5013ab8bf", "Invoice Total [{0} {1}] is less than the Advance Payment received [{0} {2}]. Unable to apply Advance Payment to the invoice. Please correct the sell amount, or cancel and re-create the Advance Payment to post this invoice"
																	, currency
																	, invoiceAmount
																	, totalInvoicedCashAdvanceLineAmount);
		}

		static string GetCurrencyMismatchErrorMessage(CashAdvanceRequestHeader cashAdvanceRequest, Journal.Journal cashAdvanceJournal, InvoicingBase invoice)
		{
			return Res.GetString("2a074b75-2ef2-4d48-89e4-a0a0cf4938b4", "Advance Payment {0} cannot be invoiced as journal currency is {1} whereas invoice currency is {1}."
																	, cashAdvanceRequest.CAH_RequestReferenceNumber
																	, cashAdvanceJournal.AH_RX_NKTransactionCurrency
																	, invoice.AH_RX_NKTransactionCurrency);
		}
	}
}
