using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.CashAdvance
{
	public class CashAdvanceRequestUpdateVisitor : ICashAdvanceRequestProcessingByInvoiceVisitor, ICashAdvanceRequestProcessingByJournalVisitor
	{
		public void Visit(ARInvoice arInvoice) => VisitInternal(arInvoice);

		public void Visit(APInvoice apInvoice) => VisitInternal(apInvoice);

		void VisitInternal(Invoice invoice)
		{
			if (invoice != null && invoice.Lines.Any() && !invoice.IsReversed)
			{
				var carInfoByInvoice = new CashAdvanceRequestInfoByInvoice(invoice);
				var carRequirementLines = carInfoByInvoice.CashAdvanceRequirements;

				if (carRequirementLines?.Any() ?? false)
				{
					//Pending Lines
					var pendingLines = carRequirementLines.Where(l => l.IsPending).ToArray();
					foreach (var pendingLine in pendingLines)
					{
						pendingLine.RemoveCashAdvanceRequirement();
					}

					//Requested Lines
					var requestedLines = carRequirementLines.Where(l => l.IsRequested).ToArray();
					foreach (var requestedLine in requestedLines)
					{
						requestedLine.Cancel();
					}

					var cashAdvanceRequests = carInfoByInvoice.CashAdvanceRequestHeaders;
					if (cashAdvanceRequests?.Any() ?? false)
					{
						//Paid Lines
						var postedLines = carRequirementLines.Where(l => l.IsPaid).ToArray();
						foreach (var postedLine in postedLines)
						{
							postedLine.MarkAsInvoiced(invoice);
						}

						if (postedLines.Any() &&
							postedLines.First().IsFunctionalityEnabled &&
							postedLines.First().IsManualSettingOfCashAdvanceRequestStatusToPaidAllowed)
						{
							if (Math.Abs(invoice.AH_OSTotal) <= Math.Abs(postedLines.Sum(x => x.OSPaidAmount)))
							{
								invoice.AH_OutstandingAmount = 0;
								invoice.AH_FullyPaidDate = ZDateTime.Today;
							}
						}
					}

					var overpaidJournasForCAH = CreateOverpaidJournalBasedOnInvoice(invoice, carInfoByInvoice);

					CreateCAIJournalAndMatchWithInvoice(invoice, carInfoByInvoice, overpaidJournasForCAH);
				}
			}
		}

		void CreateCAIJournalAndMatchWithInvoice(Invoice invoice, CashAdvanceRequestInfoByInvoice carInfoByInvoice, Dictionary<CashAdvanceRequestHeader, Journal.Journal> overpaidJournalsForCAH)
		{
			if (invoice is IInvoiceAssociatedToCashAdvanceRequest cahUpdater &&
					carInfoByInvoice.CashAdvanceRequestHeaders.All(cah => (cah.CAH_Status == CashAdvanceStatusCodes.RequestHeader.Invoiced || cah.CAH_Status == CashAdvanceStatusCodes.RequestHeader.PartiallyInvoiced) &&
																			cah.CanCashAdvanceRelatedJournalBeCreated))
			{
				var invoiceLines = invoice.Lines.Cast<InvoicingLineBase>();
				var cahAdvanceMatchDetails = new Dictionary<ZGuid, CashAdvanceMatchingTransactionDetail>();
				var creator = new CashAdvanceInvoicedJournalCreator(invoice);
				var context = new CashAdvanceJournalContext(creator
															, newJournalCreatedEventHandler: AddToCashAdvanceMatchDetails
															, journalCreationFailedEventHandler: (pcah, err) => throw new CannotGenerateCashAdvanceJournalException(pcah, err));
				foreach (var cah in carInfoByInvoice.CashAdvanceRequestHeaders)
				{
					Journal.Journal overpayJournal = null;
					overpaidJournalsForCAH.TryGetValue(cah, out overpayJournal);
					context.CreateJournal(cah, overpayJournal);
				}

				foreach (var invoiceLine in invoiceLines)
				{
					if (invoiceLine.RelatedJobCharge != null)
					{
						var charge = invoiceLine.RelatedJobCharge;
						AccCashAdvanceRequestLine requestLine = null;

						if (invoice.AH_Ledger == LedgerTypes.AccountsReceivable && (charge?.ARCashAdvanceRequestLine?.IsInvoiced ?? false))
						{
							requestLine = invoiceLine.RelatedJobCharge.ARCashAdvanceRequestLine;
						}
						else if (invoice.AH_Ledger == LedgerTypes.AccountsPayable && (charge?.APCashAdvanceRequestLine?.IsInvoiced ?? false))
						{
							requestLine = invoiceLine.RelatedJobCharge.APCashAdvanceRequestLine;
						}

						if (CanBeAddedToMapping(charge, requestLine))
						{
							var cahPK = requestLine.RequestHeader.PK;
							cahAdvanceMatchDetails[cahPK].InvoiceLineToRequestLineMapping[invoiceLine] = (charge, requestLine);
						}
					}
				}

				cahUpdater.SetCashAdvanceMatchDetails(cahAdvanceMatchDetails.Values.ToList());

				void AddToCashAdvanceMatchDetails(CashAdvanceRequestHeader header, Journal.Journal caiJournal)
				{
					var matchDetail = new CashAdvanceMatchingTransactionDetail(header, caiJournal);
					cahAdvanceMatchDetails.Add(header.PK, matchDetail);
				}

				bool CanBeAddedToMapping(BaseCharge charge, AccCashAdvanceRequestLine requestLine)
				{
					if (charge == null || requestLine == null)
					{
						return false;
					}

					var requestHeader = requestLine.RequestHeader;
					return cahAdvanceMatchDetails.ContainsKey(requestHeader.PK) &&
							cahAdvanceMatchDetails[requestHeader.PK].RequestHeader.Lines.OfType<AccCashAdvanceRequestLine>()
																						.Any(cal => cal.PK == requestLine.PK);
				}
			}
		}

		Dictionary<CashAdvanceRequestHeader, Journal.Journal> CreateOverpaidJournalBasedOnInvoice(Invoice invoice, CashAdvanceRequestInfoByInvoice carInfoByInvoice)
		{
			var overpaidJournalsForCAH = new Dictionary<CashAdvanceRequestHeader, Journal.Journal>();

			var carRequirementLines = carInfoByInvoice.CashAdvanceRequirements;
			var totalCashAdvancePaidAmount = carRequirementLines.Where(l => l.IsInvoiced).Sum(l => l.OSPaidAmount);
			if (Math.Abs(totalCashAdvancePaidAmount) > Math.Abs(invoice.AH_OSTotalAmount))
			{
				var creator = new CashAdvanceOverpaidJournalCreator(invoice, carRequirementLines);
				var context = new CashAdvanceJournalContext(creator
															, newJournalCreatedEventHandler: null
															, journalCreationFailedEventHandler: (pcah, err) => throw new CannotGenerateCashAdvanceJournalException(pcah, err));
				foreach (var cah in carInfoByInvoice.CashAdvanceRequestHeaders)
				{
					var journal = context.CreateJournal(cah);
					overpaidJournalsForCAH.Add(cah, journal);
				}
			}
			return overpaidJournalsForCAH;
		}

		public void Visit(Journal.APJournal journal) => VisitInternal(journal);

		public void Visit(Journal.ARJournal journal) => VisitInternal(journal);

		void VisitInternal(Journal.Journal journal)
		{
			if (!journal.IsInDatabase &&
				journal.IsCashAdvanceJournal &&
				journal.CashAdvanceRequestHeader != null &&
				journal.CashAdvanceRequestHeader.CanBeMarkedAsPaid &&
				journal.AH_OSExTaxAmount == journal.CashAdvanceRequestHeader.CAH_OSOutstandingAmount &&
				journal.AH_LocalExTaxAmount == journal.CashAdvanceRequestHeader.CAH_LocalOutstandingAmount)
			{
				journal.CashAdvanceRequestHeader.MarkAsPaid(updatedViaMatchingJournal: true);
			}
		}
	}
}
