using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Business.MessageProcessors;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using EDIMessage = Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.CA.DataTransfer
{
	public class ARLTransactionBatchDataContextManager : DataContextManager<CusStatementHeader>, ITransactionBatchDataContextManager
	{
		#region TransactionBatchDataContextManager Members

		public ITopLevelDataObjectWriter GetTransactionBatchDataObjectWriter(IDataWritingManager writeManager)
		{
			return null;
		}

		public bool ManagesTransactionBatches
		{
			get { return true; }
		}

		public IKeysResult GetKeysForBlockingParallelImport(IEDIMessage message, ITopLevelDataObject dataObject, IXmlSessionTracker logger, IUniversalObjectFactory factory)
		{
			return InvokeOnProcessor(p => p.GetKeysForBlockingParallelImport(), () => KeysResult.NoMatch(), message, dataObject, logger);
		}

		public bool UseIncomingTransactionBatchData(IEDIMessage ediMessage, ITopLevelDataObject dataObject, IXmlSessionTracker logger, IUniversalObjectFactory factory)
		{
			return InvokeOnProcessor(processor => processor.Process(), () => false, ediMessage, dataObject, logger);
		}

		delegate T ProcessorAction<T>(ITransactionBatchMessageProcessor processor);

		T InvokeOnProcessor<T>(ProcessorAction<T> action, Func<T> provideDefault, IEDIMessage ediMessage, ITopLevelDataObject dataObject, IXmlSessionTracker logger)
		{
			var dataTarget = dataObject.GetMatchingDataTarget(DataContextType);
			if (dataTarget != null)
			{
				var transactionBatch = (TransactionBatch)dataObject;
				if (transactionBatch != null)
				{
					var processor = GetProcessor((EDIMessage)ediMessage, transactionBatch, logger);
					if (processor != null)
					{
						return action(processor);
					}
				}
			}

			return provideDefault();
		}

#if DEBUG
		protected virtual
#endif
		ITransactionBatchMessageProcessor GetProcessor(EDIMessage message, TransactionBatch transactionBatch, IXmlSessionTracker logger)
		{
			return TransactionBatchMessageProcessorProvider.GetProcessor(message, transactionBatch, logger);
		}

		public override DataContextType DataContextType
		{
			get { return DataContextType.CAAccountsReceivableLedger; }
		}

		public override ZString DataContextKey
		{
			get { return ZString.Empty; }
		}

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new ZQuery { IsNoResultQuery = true };
		}

		public override string DefaultOutputDirectory
		{
			get { return null; }
		}

		#endregion
	}
}
