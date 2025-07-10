using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Common.DataValidation
{
	public abstract class BaseEInvoicingDataValidator
	{
		protected BaseEInvoicingDataValidator(GlbCompany company, bool shouldSendErrorNotificationEmail = false)
		{
			CurrentCompany = company;
			ShouldSendErrorNotificationEmail = shouldSendErrorNotificationEmail;
		}

		protected GlbCompany CurrentCompany { get; }

		/// <summary>
		/// When true, an error notification email is sent to members of EInvoicingErrorNotificationGroup for every validation error.
		/// </summary>
		public bool ShouldSendErrorNotificationEmail { get; }

#if DEBUG
		public
#else
        protected
#endif
		BusinessObjectFactory Factory = new BusinessObjectFactory();

		public void Run(ILogger logger)
		{
			logger.Log(LogType.Debug, "Data Validation started for the batched transactions.");
			RunCore(logger);
			logger.Log(LogType.Debug, "Data Validation completed for the batched transactions.");
		}

		/// <summary>
		/// Inheriting classes should call ValidateBatchedTransactions() from this method.
		/// </summary>
		protected abstract void RunCore(ILogger logger);

		/// <summary>
		/// Inheriting classes should override this method and apply any validation logic required for a transaction.
		/// </summary>
		/// <param name="transaction">The transaction to be validated.</param>
		/// <returns>A collection of validation errors; a null or empty collection should be returned if no errors are detected.</returns>
		public virtual IReadOnlyCollection<ZString> ValidateTransaction(InvoicingBase transaction, AccEInvoicingTransactionPivot pivot) => null;

		protected virtual void AddPreValidationFetchHints(IReadOnlyCollection<InvoicingBase> transactions)
		{
		}

		#region Implementation

		protected void ValidateBatchedTransactions(ILogger logger)
		{
			var transactionsAndPivots = LoadBatchedTransactionsAndPivots();
			if (transactionsAndPivots.Count == 0)
			{
				return;
			}
			logger.Log(LogType.Debug, FormattableString.Invariant($"Found {transactionsAndPivots.Count:N0} batched transactions to validate"));

			var errors = new List<IncorrectTransactionDetails>();
			foreach (var (transaction, pivot) in transactionsAndPivots)
			{
				ValidateTransactionAndUpdatePivotStatus(transaction, pivot, errors, logger);
			}

			if (errors.Count > 0)
			{
				Factory.Save();
				foreach (var error in errors)
				{
					logger.Log(LogType.Warning, FormattableString.Invariant($"[{error.UniqueIdentifier}] : Failed to send {error.BizoName} due to validation error: \r\n  {error.GetAllErrorsAsString("\r\n  ")}"));
				}
			}
		}

		IReadOnlyCollection<(InvoicingBase, AccEInvoicingTransactionPivot)> LoadBatchedTransactionsAndPivots()
		{
			var pivotFilter = new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_Status, EInvoicingPivotState.Batched);
			pivotFilter.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_GC, CurrentCompany.PK);
			var pivots = Factory.Load<AccEInvoicingTransactionPivot>(pivotFilter);
			var invoices = Factory.Load<InvoicingBase>(new ZQuery(AccTransactionHeaderSchema.PK, pivots.Select(p => p.AIP_ParentID).ToArray()));

			AddPreValidationFetchHints(invoices);

			var pivotLookupByInvoicePk = pivots.ToDictionary(x => x.AIP_ParentID, x => x);
			var result = new List<(InvoicingBase, AccEInvoicingTransactionPivot)>(invoices.Length);
			foreach (var invoice in invoices)
			{
				if (pivotLookupByInvoicePk.TryGetValue(invoice.PK, out var pivot))
				{
					result.Add((invoice, pivot));
				}
			}

			return result;
		}

		void ValidateTransactionAndUpdatePivotStatus(InvoicingBase transaction, AccEInvoicingTransactionPivot pivot, ICollection<IncorrectTransactionDetails> errors, ILogger logger)
		{
			var validationErrors = ValidateTransaction(transaction, pivot);

			if (validationErrors != null && validationErrors.Count > 0)
			{
				var validationResult = IncorrectTransactionDetails.FromBizo(transaction, validationErrors);
				errors.Add(validationResult);

				UpdatePivotStatus(pivot, validationResult);
				SendErrorNotificationEmail(transaction, validationResult, logger);
			}
		}

		void UpdatePivotStatus(AccEInvoicingTransactionPivot pivot, IncorrectTransactionDetails errors)
		{
			pivot.AIP_Status = EInvoicingPivotState.BatchedWithError;
			pivot.AIP_ErrorDescription = errors.GetAllErrorsAsString(" ");

			if (!AccountingConfigurationRegistry.Instance.AllowSendingEInvoicingBatchWithError.Value)
			{
				pivot.Batch.RemovePivotAndDiscardBatchIfEmpty(pivot);
			}
		}

		void SendErrorNotificationEmail(InvoicingBase transaction, IncorrectTransactionDetails errors, ILogger logger)
		{
			if (ShouldSendErrorNotificationEmail)
			{
				new GEIEmailNotificationCreator(null, transaction, errors.Errors, logger).SendEmail();
			}
		}

		#endregion
	}
}
