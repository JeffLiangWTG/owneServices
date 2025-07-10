using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing.Posting;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	/// <summary>
	/// If there is more than one invoice posted for the shipment per debtor, every invoice comes with /A, /B, etc at the end.
	/// </summary>
	public static partial class InvoiceLiteralNumberGenerator
	{
		/// <summary>
		/// Generates number for the Job invoice
		/// </summary>
		/// <param name="currentJob">Job the invoice/credit note belongs to</param>
		/// <returns>Next invoice/Credit note number for a Job AR Invoice/Credit Note</returns>
		public static string GetNextAndUpdateUniqueJobARInvoiceNumber(InvoicingBase invoicingBase, IPostingJob currentJob)
		{
			var result = string.Empty;
			if (currentJob != null)
			{
				var query = GetInvoiceFilter(invoicingBase, currentJob);
				var queryFromCache = new ZQuery(query);
				queryFromCache.FetchOnlyFromLocalCache = true;

				var transactions = invoicingBase.Factory.Load<AccTransactionHeader>(query);
				var transactionsFromCache = invoicingBase.Factory.Load<AccTransactionHeader>(queryFromCache);

				var currentJobPK = currentJob.PK;
				var dbResultForOthers = transactions.Where(x => x.AH_JH != currentJobPK).Select(y => y.AH_ConsolidatedInvoiceRef);
				var dbResultForJob = transactions.Where(x => x.AH_JH == currentJobPK).Select(y => y.AH_ConsolidatedInvoiceRef);
				var cacheResultForOther = transactionsFromCache.Where(x => x.AH_JH != currentJobPK).Select(y => y.AH_ConsolidatedInvoiceRef);
				var cacheResultForJob = transactionsFromCache.Where(x => x.AH_JH == currentJobPK).Select(y => y.AH_ConsolidatedInvoiceRef);

				result = GetNextConsolInvoiceNumber(invoicingBase.Factory, dbResultForJob.Union(cacheResultForJob), dbResultForOthers.Union(cacheResultForOther), currentJob.JobNumber);
			}
			return result;
		}

		static ZQuery GetInvoiceFilter(InvoicingBase invoicingBase, IPostingJob currentJob)
		{
			var query = new ZQuery();
			query.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable);
			query.AddToFilter(AccTransactionHeaderSchema.AH_GC, invoicingBase.AH_GC);
			var consolidatedInvoiceRefSubQuery = new ZQuery();
			consolidatedInvoiceRefSubQuery.AddToFilter(AccTransactionHeaderSchema.AH_ConsolidatedInvoiceRef, currentJob.JobNumber);
			consolidatedInvoiceRefSubQuery.AddToFilter(JoinCondition.Or, AccTransactionHeaderSchema.AH_ConsolidatedInvoiceRef, SQLComparisonOperator.StartsWith, currentJob.JobNumber + CommonUtils.SuffixSeparator);
			query.AddToFilter(consolidatedInvoiceRefSubQuery);
			query.AddOptionRecompileConditionally = true;
			return query;
		}

		/// <summary>
		/// Generates number for consol invoice
		/// </summary>
		/// <param name="factory">Business object factory used for posting</param>
		/// <param name="consolNumber">Unique Consol number</param>
		/// <param name="transactionHeaderPK">Invoice/Credit note that the number is generated for</param>
		/// <returns>Next invoice/Credit note number for a Consol AR Invoice/Credit Note</returns>
		public static string GetNextConsolARInvoiceNumber(BusinessObjectFactory factory, string consolNumber, ZGuid transactionHeaderPK)
		{
			return GetNextConsolInvoiceNumber(factory, GetExistingInvoiceNumbers(factory, consolNumber, AccTransactionHeaderSchema.AH_ConsolidatedInvoiceRef,
				transactionHeaderPK, AccTransactionHeaderSchema.AH_ConsolidatedInvoiceRef), consolNumber);
		}

		public static string GetNextLiteralAPInvoiceNumberForCASS(BusinessObjectFactory factory, InvoicingBase apInvoiceOrCreditNote, string invoiceNumber)
		{
			return GetNextConsolInvoiceNumber(factory
				, existingInvoiceNumbersForJob: GetExistingAPTransactionNumsInUseForCASS(
					factory,
					invoiceNumber,
					apInvoiceOrCreditNote.PK,
					apInvoiceOrCreditNote.AH_OH,
					apInvoiceOrCreditNote.AH_TransactionType)
				, invoiceNumber: invoiceNumber
				, reportError: false);
		}

		public static string GetNextLiteralInvoiceNumber_WithSuffix(BusinessObjectFactory factory, InvoicingBase invoice, ZString invoiceNumber, string suffix)
		{
			int suffixLength = 4;
			string candidateInvoiceNumber = invoiceNumber.SubstringSafe(0, AccTransactionHeaderSchema.AH_TransactionNum.MaxLength - suffixLength) + suffix;
			var exstingInvoiceNumbers = GetExistingInvoiceNumbers(factory, candidateInvoiceNumber, AccTransactionHeaderSchema.AH_TransactionNum, invoice.PK, invoice.AH_Ledger, invoice.AH_TransactionType, invoice.AH_OH, AccTransactionHeaderSchema.AH_TransactionNum);
			string result = GetNextConsolInvoiceNumber(factory, exstingInvoiceNumbers, candidateInvoiceNumber);

			while (result.Length > AccTransactionHeaderSchema.AH_TransactionNum.MaxLength && suffixLength < AccTransactionHeaderSchema.AH_TransactionNum.MaxLength)
			{
				suffixLength++;
				candidateInvoiceNumber = invoiceNumber.SubstringSafe(0, AccTransactionHeaderSchema.AH_TransactionNum.MaxLength - suffixLength) + suffix;
				exstingInvoiceNumbers = GetExistingInvoiceNumbers(factory, candidateInvoiceNumber, AccTransactionHeaderSchema.AH_TransactionNum, invoice.PK, invoice.AH_Ledger, invoice.AH_TransactionType, invoice.AH_OH, AccTransactionHeaderSchema.AH_TransactionNum);
				result = GetNextConsolInvoiceNumber(factory, exstingInvoiceNumbers, candidateInvoiceNumber);
			}
			return result;
		}

		public static ZString GetNextProfitShareAPInvoiceNumberForConsol(BusinessObjectFactory factory, ZString consolNumber)
		{
			string prefix = "PS";
			var invoiceNumbers = GetExistingInvoiceNumbers(factory, prefix + " " + consolNumber, AccTransactionHeaderSchema.AH_TransactionNum, AccTransactionHeaderSchema.AH_TransactionNum);

			if (!invoiceNumbers.Any())
			{
				return prefix + " " + consolNumber;
			}
			else
			{
				int indexOfExistingSuffix = 0;

				if (consolNumber.Contains(CommonUtils.SuffixSeparator))
				{
					indexOfExistingSuffix = CommonUtils.GetNumberRepresentation(consolNumber.SubstringSafe(consolNumber.IndexOf(CommonUtils.SuffixSeparator) + 1));
				}

				string nextSuffixToTry = GetLetterRepresentation(1 + indexOfExistingSuffix);

				string consolNumberWithoutExistingSuffix = consolNumber;
				if (consolNumber.Contains(CommonUtils.SuffixSeparator))
				{
					consolNumberWithoutExistingSuffix = consolNumber.SubstringSafe(0, consolNumber.IndexOf(CommonUtils.SuffixSeparator));
				}

				return GetNextProfitShareAPInvoiceNumberForConsol(factory, consolNumberWithoutExistingSuffix + nextSuffixToTry);
			}
		}

		public static string GetAutoAPInvoiceNumberForCASS(ZString currencyCode) => FormattableString.Invariant($"CASS{currencyCode}{ZDateTime.Now.ToString("yyMMdd", CultureInfo.InvariantCulture)}");

		#region Implementation

		/// <summary>
		/// Converts numbers like 1, 2, 3, etc to A, B, C, etc.
		/// if number greater than 26, then we should get AA, AB, AC...BA, BB, BC, etc.
		/// </summary>
		/// <param name="number">Number to convert</param>
		/// <returns>String representation of the number</returns>
		static string GetLetterRepresentation(int number)
		{
			if (number > 0)
			{
				return CommonUtils.SuffixSeparator + CommonUtils.GetLetterRepresentation(number);
			}
			else
			{
				return "";
			}
		}

		/// <remarks>Keep this filter in synch with the code in InvoiceBaseValidation.CheckAH_TransactionNum. If you chang this logic you should check
		/// if that validation also needs to change.</remarks>
		static IEnumerable<ZString> GetExistingAPTransactionNumsInUseForCASS(BusinessObjectFactory factory, string invoiceNumber, ZGuid transactionHeaderPK, ZGuid organisation,
			ZString transactionType)
		{
			var result = new List<ZString>();

			var filterForTransactions = new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, SQLComparisonOperator.StartsWith, invoiceNumber);
			filterForTransactions.AddToFilter(AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, transactionHeaderPK);
			filterForTransactions.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);

			switch (transactionType)
			{
				case TransactionTypes.Invoice:
					filterForTransactions.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType,
						new ZString[] {
						TransactionTypes.Invoice,
						TransactionTypes.IncompleteInvoice,
						TransactionTypes.InvoicePendingAllocation,
						TransactionTypes.UAInvoice
					});
					break;
				case TransactionTypes.CreditNote:
					filterForTransactions.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType,
						new ZString[] {
						TransactionTypes.CreditNote,
						TransactionTypes.IncompleteCreditNote,
						TransactionTypes.CreditNotePendingAllocation,
						TransactionTypes.UACreditNote
					});
					break;
				default:
					return Array.Empty<ZString>();
			}

			filterForTransactions.AddToFilter(AccTransactionHeaderSchema.AH_Ledger,
				new ZString[] {
					LedgerTypes.AccountsPayable,
					LedgerTypes.IncompleteTransactions,
					LedgerTypes.TransactionsPendingAllocation,
					LedgerTypes.UnapprovedPayableTransactions });

			filterForTransactions.AddToFilter(AccTransactionHeaderSchema.AH_OH, organisation);

			var transactionNumbers = factory.Load<AccTransactionHeader>(filterForTransactions).Select(l => l.AH_TransactionNum);
			result.AddRange(transactionNumbers);

			ZQuery filterForJobInvocing = new ZQuery(JobChargeSchema.JR_APInvoiceNum, SQLComparisonOperator.StartsWith, invoiceNumber);
			filterForJobInvocing.AddToFilter(JobChargeSchema.JR_OH_CostAccount, organisation);
			filterForJobInvocing.AddToFilter(JobChargeSchema.JR_LocalCostAmt, SQLComparisonOperator.LessThan, 0);
			filterForJobInvocing.AddToFilter(AccountingUtils.GenerateCompanyFilter(JobChargeSchema.JR_GB, factory));

			var jobInvoiceNumbers = factory.Load<JobCharge>(filterForJobInvocing).Select(c => c.JR_APInvoiceNum);
			result.AddRange(jobInvoiceNumbers);

			return result;
		}

		static IEnumerable<ZString> GetExistingInvoiceNumbers(BusinessObjectFactory factory, string consolNumber, SchemaColumn columnToFilter, SchemaStringColumn columnWithInvoiceNumber)
		{
			return GetExistingInvoiceNumbers(factory, consolNumber, columnToFilter, ZGuid.Empty, null, null, ZGuid.Empty, columnWithInvoiceNumber);
		}

		static IEnumerable<ZString> GetExistingInvoiceNumbers(BusinessObjectFactory factory, string consolNumber, SchemaColumn columnToFilter, ZGuid transactionHeaderPK, SchemaStringColumn columnWithInvoiceNumber)
		{
			return GetExistingInvoiceNumbers(factory, consolNumber, columnToFilter, transactionHeaderPK, null, null, ZGuid.Empty, columnWithInvoiceNumber);
		}

		/// <remarks>Keep this filter in synch with the code in InvoiceBaseValidation.CheckAH_TransactionNum. If you chang this logic you should check
		/// if that validation also needs to change.</remarks>
		static IEnumerable<ZString> GetExistingInvoiceNumbers(BusinessObjectFactory factory, string consolNumber, SchemaColumn columnToFilter, ZGuid transactionHeaderPK,
			string ledger, string transactionType, ZGuid organisation, SchemaStringColumn columnWithInvoiceNumber)
		{
			var consolNumberFilter = new ZQuery(columnToFilter, SQLComparisonOperator.StartsWith, consolNumber + CommonUtils.SuffixSeparator);
			consolNumberFilter.AddToFilter(JoinCondition.Or, columnToFilter, consolNumber);
			var filter = new ZQuery(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
			filter.AddToFilter(AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, transactionHeaderPK);
			filter.AddToFilter(consolNumberFilter);
			if (ledger != null)
			{
				filter.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, ledger);
			}
			if (transactionType != null)
			{
				filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, transactionType);
			}
			if (!organisation.IsEmpty)
			{
				filter.AddToFilter(AccTransactionHeaderSchema.AH_OH, organisation);
			}

			return factory.Load<AccTransactionHeader>(filter).Select(x => (ZString)x[columnWithInvoiceNumber]);
		}

		static IEnumerable<int> ConvertInvoiceNumbersSuffixesToInts(IEnumerable<ZString> invoiceNumbers, string baseInvoiceNumber, bool reportErrors)
		{
			foreach (ZString existingNumber in invoiceNumbers)
			{
				if (!existingNumber.StartsWith(baseInvoiceNumber))
				{
					if (reportErrors)
					{
						ErrorReporter.ReportOnce("GetNextConsolInvoiceNumber", string.Format("Existing number '{0}' doesn't start with expected '{1}'.", existingNumber, baseInvoiceNumber));
					}

					continue;
				}
				var suffix = existingNumber.Substring(baseInvoiceNumber.Length, existingNumber.Length - baseInvoiceNumber.Length);
				var errorMessage = CommonUtils.ValidaeSuffixForGetNumberRepresentation(suffix);
				if (!string.IsNullOrEmpty(errorMessage))
				{
					if (reportErrors)
					{
						ErrorReporter.ReportOnce("GetNextConsolInvoiceNumber", string.Format("Number to add suffix for '{0}'. Existing number '{1}'. Suffix: '{2}'. Error: {3}", baseInvoiceNumber, existingNumber, suffix, errorMessage));
					}

					continue;
				}
				yield return CommonUtils.GetNumberRepresentation(suffix);
			}
		}

		static string GetNextConsolInvoiceNumber(BusinessObjectFactory factory, IEnumerable<ZString> existingInvoiceNumbersForJob, string invoiceNumber, bool reportError = true)
		{
			return GetNextConsolInvoiceNumber(factory, existingInvoiceNumbersForJob, Array.Empty<ZString>(), invoiceNumber, reportError);
		}

		static string GetNextConsolInvoiceNumber(BusinessObjectFactory factory, IEnumerable<ZString> existingInvoiceNumbersForJob, IEnumerable<ZString> existingInvoiceNumbersToSkip, string invoiceNumber, bool reportError = true)
		{
			string result = invoiceNumber;

			var intsForInvoiceNumbersForJob = ConvertInvoiceNumbersSuffixesToInts(existingInvoiceNumbersForJob, invoiceNumber, reportError);
			int nextNumber = intsForInvoiceNumbersForJob.Any() ? intsForInvoiceNumbersForJob.Max() + 1 : 0;
			var intsForInvoiceNumbersToSkip = ConvertInvoiceNumbersSuffixesToInts(existingInvoiceNumbersToSkip, invoiceNumber, false);

			foreach (int value in intsForInvoiceNumbersToSkip.OrderBy(i => i))
			{
				if (nextNumber == value)
				{
					nextNumber++;
				}
			}

			if (nextNumber >= AccountingConfigurationRegistry.Instance.MaximumNumberOfInvoicesAllowedOnJob.Value)
			{
				factory.SetContext(BusinessContext.MaximumJobInvoiceNumberError);
			}
			else
			{
				result += GetLetterRepresentation(nextNumber);
			}

			return result;
		}

		#endregion
	}
}
