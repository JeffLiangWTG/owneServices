using System.Collections.Generic;
using System.Globalization;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.CashAdvance;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.JobInvoicing.InvoiceLineToChargeMatching;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public static class TransactionAllocationConverter
	{
		public enum Context
		{
			InConversion
		}

		public static (InvoicingBase Invoice, string ErrorMessage) ConvertUnallocatedToAP(TransactionPendingAllocation transactionPendingAllocation)
		{
			if (ObjectFactory.Get<ICountryComplianceFactory>().GetICountryComplianceInfoBase(transactionPendingAllocation.Company.GC_RN_NKCountryCode) is IEnableTransactionsPendingAllocationAllocateAsReceivable countryCompliance &&
				!countryCompliance.IsComplianceSubTypeNotEligibleForAllocateAsReceivable(transactionPendingAllocation.AH_ComplianceSubType))
			{
				return (null, Res.GetString("2C754421-DEC3-4185-B300-9BCDF59CB4BA", "You cannot allocate Return AP Invoice as Payable. Selected invoice must be allocated as Receivable. Transaction number: {0}.", transactionPendingAllocation.AH_TransactionNum));
			}

			var isInvoicePendingAllocation = IsInvoicePendingAllocation(transactionPendingAllocation.AH_TransactionType);

			var newFactory = new BusinessObjectFactory();
			using var context = newFactory.SetTempContext(BusinessContext.AllocatingTransaction);

			InvoicingBase result = isInvoicePendingAllocation ? newFactory.Load<APInvoice>(transactionPendingAllocation.PK) : newFactory.Load<APCreditNote>(transactionPendingAllocation.PK);

			PAToAPTransactionLineMonitor.Create(result);

			return ConvertUnallocatedCore(result, transactionPendingAllocation, isInvoicePendingAllocation ? TransactionTypes.Invoice : TransactionTypes.CreditNote, LedgerTypes.AccountsPayable);
		}

		public static (InvoicingBase Invoice, string ErrorMessage) ConvertUnallocatedToAR(TransactionPendingAllocation transactionPendingAllocation)
		{
			if (!IsInvoicePendingAllocation(transactionPendingAllocation.AH_TransactionType))
			{
				return (null, Res.GetString("4D5EB762-4AD0-417B-AC91-EB01ED9BA6E2", "Credit Note Pending Allocation transactions are not eligible to allocations as Receivable Invoice. Transaction number: {0}.", transactionPendingAllocation.AH_TransactionNum));
			}

			var result = new BusinessObjectFactory().Load<ARCreditNote>(transactionPendingAllocation.PK);
			if (result != null)
			{
				if (CountryComplianceFactory.GetCountryComplianceInfo(result.Company.GC_RN_NKCountryCode) is IEnableTransactionsPendingAllocationAllocateAsReceivable countryCompliance)
				{
					result.AH_ComplianceSubType = countryCompliance.GetEligibleComplianceSubTypeForAllocateAsReceivable(result.AH_ComplianceSubType);
					result.Lines.SetReadOnlyIncludingChildren(false);
				}
				result.AllocationApprovalRequest.PostingDetails.IsCrossLedgerImportFromXML = true;
			}

			return ConvertUnallocatedCore(result, transactionPendingAllocation, TransactionTypes.CreditNote, LedgerTypes.AccountsReceivable);
		}

		static bool IsInvoicePendingAllocation(string transactionType) => transactionType == TransactionTypes.InvoicePendingAllocation;

		static (InvoicingBase Invoice, string ErrorMessage) ConvertUnallocatedCore(InvoicingBase result, TransactionPendingAllocation transactionPendingAllocation, string transactionType, string ledger)
		{
			if (result == null)
			{
				return (null, Res.GetString("30b60379-5743-4f69-8cdf-5c658f4b488a", "The transaction {0} cannot be allocated because another user has deleted the record.", transactionPendingAllocation.AH_TransactionNum));
			}

			var multiplier = IsInvoicePendingAllocation(transactionPendingAllocation.AH_TransactionType) ? 1 : -1;
			result.AH_TransactionType = transactionType;
			result.AH_Ledger = ledger;
			result.ExpectedInvoiceExclTaxTotal = multiplier * transactionPendingAllocation.AH_OSExTaxAmount;
			result.ExpectedInvoiceTaxTotal = multiplier * transactionPendingAllocation.AH_OSTaxAmount;
			result.ExpectedInvoiceTotal = multiplier * transactionPendingAllocation.AH_OSTotalAmount;

			if ((ZString)result.AH_LedgerInfo.OriginalValue != LedgerTypes.TransactionsPendingAllocation)
			{
				return (null, Res.GetString("772A439A-4C6B-43ef-BB9C-271E6C6F6E62", "Cannot allocate transaction that has been allocated."));
			}

			if (transactionPendingAllocation.AH_Desc == transactionPendingAllocation.DefaultDescription)
			{
				result.AH_Desc = result.DefaultDescription;
			}

			result.Lines.UpdateHeaderAmounts();
			result.SubmittedFromInvoicingForm = true;
			result.ValidateExpectedInvoiceTotal = true;

			if (transactionPendingAllocation.AllocationApprovalRequest != null
				&& transactionPendingAllocation.AllocationApprovalRequest.PostingDetails.UniversalTransaction != null
				&& !transactionPendingAllocation.AllocationApprovalRequest.PostingDetails.UniversalTransaction.PostDate.IsEmpty)
			{
				result.AH_PostDate = transactionPendingAllocation.AllocationApprovalRequest.PostingDetails.UniversalTransaction.PostDate;
			}
			else if (!result.AllowBackPosting)
			{
				result.AH_PostDate = ZDateTime.Now;
			}

			if (result.ExportedBatchSequence != null)
			{
				result.ExportedBatchSequence.Delete();
			}

			var isLocalCurrencyTransaction = result.AH_RX_NKTransactionCurrency == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			result.AH_PostedToEFT = result.IsAPTransaction && !isLocalCurrencyTransaction && AccountingConfigurationRegistry.Instance.UseJobExchangeRateDefault.Value;

			try
			{
				AddLinesFromLinkedRequestWithUniversalXML(result);
			}
			catch (CannotGenerateCashAdvanceJournalException ex)
			{
				return (null, ex.Message);
			}

			result.ValidateAndFixTaxBranch();

			result.SetContext(Context.InConversion);

			return (result, null);
		}

		static void AddLinesFromLinkedRequestWithUniversalXML(InvoicingBase transaction)
		{
			if (transaction.AllocationApprovalRequest != null && !transaction.AllocationApprovalRequest.PostingDetails.SourceXML.IsEmpty)
			{
				var requestPostingDetails = transaction.AllocationApprovalRequest.PostingDetails;
				bool isProcessed = false;

				if (AccountingConfigurationRegistry.Instance.EnableAutoAccrualMatching.Value)
				{
					var matchingResult = InvoiceLineToChargeMatchingGenerator.GetMatchingResult(requestPostingDetails.UniversalTransaction);
					if (matchingResult != null && matchingResult.Outcome == MatchingOutcome.FullyMatched)
					{
						var bestSuggestions = matchingResult.GetBestMatchingSuggestions();

						var importedConsolCostPks = new HashSet<ZGuid>();
						foreach (var suggestion in bestSuggestions)
						{
							foreach (var recordedCharge in suggestion.ChargeGroup.GetCharges())
							{
								if (recordedCharge.E6_PK.IsEmpty)
								{
									InvoicingLineBase line = (InvoicingLineBase)transaction.Lines.AddNew();

									var charge = transaction.Factory.Load<Charge>(recordedCharge.JR_PK);
									transaction.ImportJobChargesIntoInvoice(new List<Charge>() { charge }, line, false);
								}
								else
								{
									if (!importedConsolCostPks.Contains(recordedCharge.E6_PK))
									{
										InvoicingLineBase line = (InvoicingLineBase)transaction.Lines.AddNew();

										var consolCost = transaction.Factory.Load<JobConsolCost>(recordedCharge.E6_PK);
										transaction.ImportSingleCost(consolCost, line);

										importedConsolCostPks.Add(recordedCharge.E6_PK);
									}
								}
							}
						}

						transaction.Logs.AddNew(Events.AutoMatchDone, string.Format(CultureInfo.InvariantCulture, "{0}|{1}|Auto Matching Run", transaction.AH_Ledger, transaction.AH_TransactionType));
						isProcessed = true;
					}
				}

				if (!isProcessed)
				{
					var importer = ObjectFactory.Get<ITransactionImporter>();
					importer.ImportTransactionLines(requestPostingDetails.SourceXML, transaction, requestPostingDetails.IsCrossLedgerImportFromXML);
				}
			}
		}
	}
}
