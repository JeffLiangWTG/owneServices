using System;
using System.Linq;
using CargoWise.Application;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using UniversalTransaction = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionInfo;

namespace Enterprise.UniversalDataBuss.Management.TransactionProcessing
{
	class UniversalTransactionMessageProcessor : ITopLevelDataObjectProcessor
	{
		public MessageKeyProviderResult GetKeys(IEDIMessage message, ITopLevelDataObject dataObject, IUniversalObjectFactory factory, IXmlSessionTracker logger)
		{
			return new KeyProvider((XmlSessionTracker)logger).Execute(message, (UniversalTransaction)dataObject, (UniversalObjectFactory)factory);
		}

		public MessageStatus ProcessDataObject(IEDIMessage message, ITopLevelDataObject dataObject, IUniversalObjectFactory factory, IXmlSessionTracker logger)
		{
			var result = MessageStatus.Discarded;
			var xmlSessionTracker = (XmlSessionTracker)logger;

			var contextManager = DataContextManagersFactory.All.FirstOrDefault(x => (x as ITransactionDataContextManager)?.ManagesTransactions ?? false);
			if (contextManager != null)
			{
				try
				{
					xmlSessionTracker.IndividualImportBegin(contextManager.DataContextType);
					result = new MessageImporter(xmlSessionTracker).Execute(message, (UniversalTransaction)dataObject, (UniversalObjectFactory)factory);
				}
				catch (DataObjectReadFailureException exception)
				{
					result = MessageStatus.Rejected;
					logger.LogBoth(LogType.Error, exception.Message);
					logger.LogBoth(LogType.Information, Res.GetString("9d6a4fde-58e6-442d-b934-12f6f17f23a8", "No changes were made due to the above errors. Please fix the errors and try again."));
				}
				finally
				{
					xmlSessionTracker.IndividualImportEnd();
				}
			}

			return result;
		}

		public void ValidateDataObject(ITopLevelDataObject dataObject, IXmlSessionTracker logger)
		{
		}

		class KeyProvider : ThisOrchestratesWhichAccountingImporterIsUsed<MessageKeyProviderResult, IKeysResult>
		{
			public KeyProvider(XmlSessionTracker logger) : base(logger)
			{
			}

			protected override IKeysResult ImportTransaction(IEDIMessage message, UniversalTransaction dataObject, UniversalObjectFactory factory, ITransactionImporter importer)
			{
				return importer.GetKeysForBlockingParallelImport(message, dataObject, logger, factory);
			}

			protected override IKeysResult ImportPayableDraftInvoice(IEDIMessage message, UniversalTransaction dataObject, UniversalObjectFactory factory, IPayableDraftInvoiceImporter importer)
			{
				return importer.GetKeysForBlockingParallelImport(message, dataObject, logger, factory);
			}

			protected override IKeysResult ImportNetting(IEDIMessage message, UniversalTransaction dataObject, INettingTransactionImporter importer)
			{
				return importer.GetKeysForBlockingParallelImport(message, dataObject);
			}

			protected override MessageKeyProviderResult ToResult(IKeysResult result, MessageStatus failureStatus)
			{
				return result.IsMatch ? new MessageKeyProviderResult(result.KeysInfo) : new MessageKeyProviderResult(failureStatus);
			}
		}

		class MessageImporter : ThisOrchestratesWhichAccountingImporterIsUsed<MessageStatus, bool>
		{
			public MessageImporter(XmlSessionTracker logger) : base(logger)
			{
			}

			protected override bool ImportTransaction(IEDIMessage message, UniversalTransaction dataObject, UniversalObjectFactory factory, ITransactionImporter importer)
			{
				return importer.ImportTransaction(message, dataObject, logger, factory);
			}

			protected override bool ImportPayableDraftInvoice(IEDIMessage message, UniversalTransaction dataObject, UniversalObjectFactory factory, IPayableDraftInvoiceImporter importer)
			{
				return importer.ImportPayableDraftInvoice(message, dataObject, logger, factory);
			}

			protected override bool ImportNetting(IEDIMessage message, UniversalTransaction dataObject, INettingTransactionImporter importer)
			{
				return importer.ImportNettingTransaction(message, dataObject);
			}

			protected override MessageStatus ToResult(bool succeeded, MessageStatus failureStatus)
			{
				return succeeded ? MessageStatus.Processed : failureStatus;
			}
		}

		abstract class ThisOrchestratesWhichAccountingImporterIsUsed<T, TIntermediary>
		{
			protected ThisOrchestratesWhichAccountingImporterIsUsed(XmlSessionTracker logger)
			{
				this.logger = logger;
			}
			protected readonly XmlSessionTracker logger;

			protected abstract TIntermediary ImportTransaction(IEDIMessage message, UniversalTransaction dataObject, UniversalObjectFactory factory, ITransactionImporter importer);
			protected abstract TIntermediary ImportPayableDraftInvoice(IEDIMessage message, UniversalTransaction dataObject, UniversalObjectFactory factory, IPayableDraftInvoiceImporter importer);
			protected abstract TIntermediary ImportNetting(IEDIMessage message, UniversalTransaction dataObject, INettingTransactionImporter importer);
			protected abstract T ToResult(TIntermediary intermediary, MessageStatus failureStatus);

			public T Execute(IEDIMessage message, UniversalTransaction dataObject, UniversalObjectFactory factory)
			{
				if (IsNettingSystem(message.Branch.CompanyPK, dataObject))
				{
					var importer = (INettingTransactionImporter)Activator.CreateInstance(ObjectFactory.GetType("INettingTransactionImporter"), factory.BOFactory, logger);
					return ToResult(ImportNetting(message, dataObject, importer), MessageStatus.Rejected);
				}
				else
				{
					if (IsImportAPUniversalTransactionIntoPayableDraftInvoicesEnabled(message.Branch.CompanyPK, dataObject))
					{
						var importer = (IPayableDraftInvoiceImporter)Activator.CreateInstance(ObjectFactory.GetType("IPayableDraftInvoiceImporter"));
						return ToResult(ImportPayableDraftInvoice(message, dataObject, factory, importer), MessageStatus.Discarded);
					}
					else
					{
						var importer = (ITransactionImporter)Activator.CreateInstance(ObjectFactory.GetType("ITransactionImporter"));
						return ToResult(ImportTransaction(message, dataObject, factory, importer), MessageStatus.Discarded);
					}
				}
			}

			bool IsNettingSystem(Guid companyPk, UniversalTransaction dataObject)
			{
				if ((bool)ObjectFactory.Get<IAccounting>().Registry.IsNettingSystem.GetValueWithoutFallback(companyPk, Guid.Empty, Guid.Empty))
				{
					if (dataObject.DataContext != null && dataObject.DataContext.RecipientRoleCollection != null && dataObject.DataContext.RecipientRoleCollection.FirstOrDefault(x => x.Code.GetValueOrDefault() == RecipientRoleType.WNS) != null)
					{
						return true;
					}
				}

				return false;
			}

			bool IsImportAPUniversalTransactionIntoPayableDraftInvoicesEnabled(Guid companyPk, UniversalTransaction dataObject)
			{
				return LedgerTypes.AccountsPayable.Equals(dataObject.Ledger) &&
					(TransactionTypes.Invoice.Equals(dataObject.TransactionType.ToString()) || TransactionTypes.CreditNote.Equals(dataObject.TransactionType.ToString())) &&
					(bool)ObjectFactory.Get<IAccounting>().Registry.EnablePayablesInvoiceProcessingPortal.GetValueWithoutFallback(companyPk, Guid.Empty, Guid.Empty) &&
					(bool)ObjectFactory.Get<IAccounting>().Registry.EnableImportingUniversalTransactionIntoPayableDraftInvoices.GetValueWithoutFallback(companyPk, Guid.Empty, Guid.Empty);
			}
		}
	}
}
