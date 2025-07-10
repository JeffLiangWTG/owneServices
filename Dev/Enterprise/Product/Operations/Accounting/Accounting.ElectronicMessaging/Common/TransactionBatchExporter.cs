using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Printing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.DataTransfer.Universal;
using Enterprise.Accounting.Export.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Common
{
	public class TransactionBatchExporter
	{
		public TransactionBatchExporter(BatchExportDataAccess dataAccess,
			PopulateOptionalXUTFieldsSetting optionalXUTFieldsSetting = null)
		{
			DataAccess = dataAccess;
			PopulateOptionalXUTFields = optionalXUTFieldsSetting ?? new PopulateOptionalXUTFieldsSetting();
		}

		public TransactionBatch CreateTransactionBatch(AccEInvoicingBatch eInvoicingBatch, string nameSpace = null)
		{
			var writerStrategy = CreateWriterStrategy();
			var transactions = GetInvoicingBasesToExport(eInvoicingBatch);
			if (transactions.Length > 0)
			{
				transactions = transactions
								.OrderBy(x => x.AH_PostDate.Date).ThenBy(x => x.AH_TransactionNum).ThenBy(x => x.AH_Ledger).ThenBy(x => x.AH_TransactionType)
								.ToArray();
			}
			var transactionBatch = new TransactionBatch(writerStrategy);

			var transactionExporter = new TransactionExporter(DataAccess);
			if (nameSpace != null)
			{
				transactionBatch.DataContext = transactionExporter.GetContextDataObject(eInvoicingBatch.Company?.GC_Code, eInvoicingBatch.AIB_BatchNumber, nameSpace);
			}

			foreach (var invoicingBase in transactions)
			{
				var transactionInfo = ConvertToUniversalTransaction(transactionExporter, writerStrategy, eInvoicingBatch, invoicingBase);
				if (transactionInfo != null)
				{
					transactionBatch.TransactionCollection.Add(transactionInfo);
				}
			}

			return transactionBatch;
		}

		protected virtual TransactionInfo ConvertToUniversalTransaction(TransactionExporter transactionExporter
			, AccountingTransactionDataObjectWriterStrategy writerStrategy
			, AccEInvoicingBatch eInvoicingBatch
			, InvoicingBase invoicingBase)
		{
			var validLedgers = new[] { LedgerTypes.AccountsReceivable, LedgerTypes.AccountsPayable };
			if (!validLedgers.Contains(invoicingBase.AH_Ledger.ToString()))
			{
				return null;
			}

			if (AccountingConfigurationRegistry.Instance.GenerateARInvoiceAttachmentForEReporting.Value
				&& invoicingBase.AH_Ledger.ToString() == LedgerTypes.AccountsReceivable)
			{
				invoicingBase.Factory.ServiceContainer.AddService(new ResetEDocStatusService());
				var generateARInvoiceToEdocsProcessor = new GenerateARInvoiceToEdocsProcessor(invoicingBase);
				generateARInvoiceToEdocsProcessor.Process(new NotificationBuffer(), new CancellationToken());
				invoicingBase.Logs?.AddNew(Events.ItemDocumentJobFinalised, string.Format(CultureInfo.InvariantCulture, (NoResString)"AR {0} {1}.pdf", invoicingBase.AH_TransactionType, invoicingBase.AH_TransactionNum));
			}

			var transactionInfo = CreateTransactionInfoInstance(writerStrategy);
			transactionInfo.DataContext = GetContextDataObjectForEachTransaction(eInvoicingBatch.Company?.GC_Code, invoicingBase);

			transactionExporter.PopulateUniversalTransaction(writerStrategy, invoicingBase.Company.GC_Code, invoicingBase.PK.ToGuid(), transactionInfo);

			PopulateOptionalXUTCollections(invoicingBase, transactionInfo);

			return transactionInfo;
		}

		static void PopulateOptionalXUTCollections(
			InvoicingBase invoicingBase,
			TransactionInfo transactionInfo)
		{
			var writeManager = new DataWritingManager(new ActionInfo(null, invoicingBase));
			var helper = new CreateUniversalShipmentHelper(writeManager);

			helper.PopulatePublishedEDocs(invoicingBase, transactionInfo);

			helper.PopulateLineConsols(invoicingBase, transactionInfo);
			helper.PopulateLineJobs(invoicingBase, transactionInfo);
		}

		protected virtual TransactionInfo CreateTransactionInfoInstance(IDataObjectWriterStrategy writerStrategy) => new TransactionInfo(writerStrategy);

		internal IDataContextDataObject GetContextDataObjectForEachTransaction(
			string companyCode, InvoicingBase invoice)
		{
			CompanyRow companyRow = DataAccess.LoadCompany(companyCode);
			return GetContextDataObjectForEachTransaction(companyRow, invoice);
		}

		internal static IDataContextDataObject GetContextDataObjectForEachTransaction(
			CompanyRow companyRow, InvoicingBase invoice)
		{
			IDataContextDataObject context = DataContextFactory.New();
			context.SetCompanyAndDataProviderDetails(companyRow);
			context.AddDataSource(DataContextType.AccountingInvoice, FormattableString.Invariant($"{invoice.AH_Ledger} {invoice.AH_TransactionType} {invoice.AH_TransactionNum}")); // This is an internal reference number for data context. Not displayed in UI
			return context;
		}

		AccountingTransactionDataObjectWriterStrategy CreateWriterStrategy()
		{
			var disallowedFields = PopulateOptionalXUTFields.GetFieldsToExcludeFromXUT().ToArray();

			var writerStrategy = new AccountingTransactionDataObjectWriterStrategy(
				accountingStrategy: new FieldListDisallowedAccountingTransactionWriterStrategy(disallowedFields),
				context: "eInvoicing"
			);
			return writerStrategy;
		}

		#region Implementation

		protected virtual InvoicingBase[] GetInvoicingBasesToExport(AccEInvoicingBatch source)
		{
			if (AccountingConfigurationRegistry.Instance.AllowSendingEInvoicingBatchWithError.Value)
			{
				return source.GetInvoicesWithStatus(EInvoicingPivotState.Batched, EInvoicingPivotState.BatchedWithError);
			}
			else
			{
				return source.GetInvoicesWithStatus(EInvoicingPivotState.Batched);
			}
		}

		readonly BatchExportDataAccess DataAccess;
		readonly PopulateOptionalXUTFieldsSetting PopulateOptionalXUTFields;

		#endregion
	}

	public class PopulateOptionalXUTFieldsSetting : IEquatable<PopulateOptionalXUTFieldsSetting>
	{
		public PopulateOptionalXUTFieldsSetting()
			: this(false, false, false)
		{
		}

		public PopulateOptionalXUTFieldsSetting(
			bool populateAttachedDocuments = false,
			bool populateShipments = false,
			bool populateAuthorizationDetails = false)
		{
			PopulateAttachedDocuments = populateAttachedDocuments;
			PopulateShipments = populateShipments;
			PopulateAuthorizationDetails = populateAuthorizationDetails;
		}

		public static PopulateOptionalXUTFieldsSetting AllTrue() => new PopulateOptionalXUTFieldsSetting(populateAttachedDocuments: true, populateShipments: true, populateAuthorizationDetails: true);

		public override string ToString()
		{
			if (!PopulateAttachedDocuments
			 && !PopulateShipments
			 && !PopulateAuthorizationDetails)
			{
				return (NoResString)"Not populate ShipmentCollection or AttachedDocumentCollection";
			}

			return $"{(PopulateAttachedDocuments ? "AttachedDocumentCollection; " : "")}"
				 + $"{(PopulateShipments ? "ShipmentCollection; " : "")}"
				 + $"{(PopulateAuthorizationDetails ? "AuthorizationDetails; " : "")}";
		}

		public bool Equals(PopulateOptionalXUTFieldsSetting other)
		{
			return other != null
				&& this.PopulateAttachedDocuments == other.PopulateAttachedDocuments
				&& this.PopulateShipments == other.PopulateShipments
				&& this.PopulateAuthorizationDetails == other.PopulateAuthorizationDetails;
		}

		public override bool Equals(object obj)
		{
			return obj is PopulateOptionalXUTFieldsSetting other
				&& this.Equals(other);
		}

		public override int GetHashCode()
		{
			return (PopulateAttachedDocuments, PopulateShipments, PopulateAuthorizationDetails).GetHashCode();
		}

		public bool PopulateAttachedDocuments { get; }
		public bool PopulateShipments { get; }
		public bool PopulateAuthorizationDetails { get; }

		public IEnumerable<string> GetFieldsToExcludeFromXUT()
		{
			if (!PopulateAttachedDocuments)
			{
				yield return nameof(TransactionInfo.AttachedDocumentCollection);
			}
			if (!PopulateShipments)
			{
				yield return nameof(TransactionInfo.ShipmentCollection);
			}
			if (!PopulateAuthorizationDetails)
			{
				// This will also Disallow AuthorizationDetailCollection from OriginalReference -> TransactionInfo.OriginalReference.AuthorizationDetailCollection
				yield return nameof(TransactionInfo.AuthorizationDetailCollection);
			}
		}
	}
}
