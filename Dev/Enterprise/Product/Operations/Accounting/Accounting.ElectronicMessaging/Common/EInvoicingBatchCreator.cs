using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Accounting.ElectronicMessaging.Registry;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.ElectronicMessaging.Common
{
	/// <summary>
	/// Creates EInvoicing Batches with the parameters provided.
	/// </summary>
	public sealed class EInvoicingBatchCreator : EInvoicingBatchCreatorBase
	{
		public EInvoicingBatchCreator(GlbCompany company, int maximumBatchSize = 1)
			: base(company)
		{
			MaximumBatchSize = CalculateMaximumBatchSizeWithArchitecturalLimitation(maximumBatchSize);
		}

		protected override DynamicBusinessObjectCollection GetAllTransactionsForCompany(string[] actionTypes = null, string[] actionTypesJoiningTransactionHeader = null, bool queryJoinWithHeadersOnly = false)
		{
			return base.GetAllTransactionsForCompany(actionTypes, actionTypesJoiningTransactionHeader, true);
		}

		internal static int CalculateMaximumBatchSizeWithArchitecturalLimitation(int intendedMaximumBatchSize)
		{
			if (intendedMaximumBatchSize > AccountingElectronicMessagingRegistry.MaximumBatchSizeSupportedInXUEProcessor)
			{
				var message = FormattableString.Invariant($"Due to a limitation when importing XUE messages, batches greater than {AccountingElectronicMessagingRegistry.MaximumBatchSizeSupportedInXUEProcessor} are not supported. Requested maximum batch size {intendedMaximumBatchSize} is reduced to {AccountingElectronicMessagingRegistry.MaximumBatchSizeSupportedInXUEProcessor}."); // Developer error report
				ErrorReporter.ReportOnce("EInvoicingBatchCreator_BatchSizeExceedsArchitecturalLimitation", message);  // Developer error report
			}
			return Math.Min(AccountingElectronicMessagingRegistry.MaximumBatchSizeSupportedInXUEProcessor, intendedMaximumBatchSize);
		}

		public int MaximumBatchSize { get; }

		public static int MaximumBatchSizeFromRegistryOrCountryDefault(GlbCompany company, int countryDefault)
		{
			Argument.NotNull(company, nameof(company));

			var registryValue = AccountingElectronicMessagingRegistry.Instance.EInvoicingBatchSize.GetFallBackValueAtAllLevels(company.PK.ToGuid(), Guid.Empty, Guid.Empty);
			return registryValue == 0
					? countryDefault
					: registryValue;
		}

		protected override IEnumerable<QueuedPivotPKs> GroupTransactionForBatching(DynamicBusinessObjectCollection transactions)
		{
			if (transactions == null || transactions.Count == 0)
			{
				yield break;
			}
			var groupedTransactions = GroupTransactionByUniqueIdentifierForBatching(transactions);
			var oneBatch = new QueuedPivotPKs();
			foreach (DynamicBusinessObject t in groupedTransactions)
			{
				oneBatch.Add(t);

				if (oneBatch.Items.Count >= MaximumBatchSize)
				{
					yield return oneBatch;
					oneBatch = new QueuedPivotPKs();
				}
			}

			if (oneBatch.Items.Count > 0)
			{
				yield return oneBatch;
			}
		}
	}
}
