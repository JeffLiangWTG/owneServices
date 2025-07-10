using System.Collections.Generic;
using Enterprise.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using UniversalTransactionBatch = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionBatch;

namespace Enterprise.UniversalDataBuss.Management.TransactionProcessing
{
	class UniversalTransactionBatchMessageProcessor : ITopLevelDataObjectProcessor
	{
		public MessageStatus ProcessDataObject(IEDIMessage message, ITopLevelDataObject dataObject, IUniversalObjectFactory factory, IXmlSessionTracker logger)
		{
			return ProcessTransaction(UseIncomingTransactionBatchData_Import, message, (UniversalTransactionBatch)dataObject, (UniversalObjectFactory)factory, (XmlSessionTracker)logger);
		}

		public MessageKeyProviderResult GetKeys(IEDIMessage message, ITopLevelDataObject dataObject, IUniversalObjectFactory factory, IXmlSessionTracker logger)
		{
			var keys = new HashSet<(string KeyValue, string KeySource)>();
			var result = ProcessTransaction(UseIncomingTransactionBatchData_GetKeys(keys), message, (UniversalTransactionBatch)dataObject, (UniversalObjectFactory)factory, (XmlSessionTracker)logger);

			if (result == MessageStatus.Processed)
			{
				return new MessageKeyProviderResult(keys);
			}
			else
			{
				return new MessageKeyProviderResult(result);
			}
		}

		public void ValidateDataObject(ITopLevelDataObject dataObject, IXmlSessionTracker logger)
		{
		}

		MessageStatus ProcessTransaction(UseIncomingTransactionBatchData invoke, IEDIMessage message, UniversalTransactionBatch dataObject, UniversalObjectFactory factory, XmlSessionTracker logger)
		{
			var result = MessageStatus.Discarded;
			var xmlSessionTracker = logger;

			foreach (var baseContextManager in DataContextManagersFactory.All)
			{
				var contextManager = baseContextManager as ITransactionBatchDataContextManager;
				if (contextManager != null && contextManager.ManagesTransactionBatches)
				{
					try
					{
						xmlSessionTracker.IndividualImportBegin(contextManager.DataContextType);
						if (invoke(message, dataObject, factory, contextManager, logger))
						{
							result = MessageStatus.Processed;
						}
					}
					finally
					{
						xmlSessionTracker.IndividualImportEnd();
					}
				}
			}

			if (result == MessageStatus.Discarded)
			{
				logger.LogWasNotUsedByModule(LogType.Information, Res.GetString("5ebbf90f-c961-4158-b2cc-cad70d3f83f4", "No Module used this Transaction Batch data."));
			}

			return result;
		}

		delegate bool UseIncomingTransactionBatchData(IEDIMessage message, UniversalTransactionBatch dataObject, UniversalObjectFactory factory, ITransactionBatchDataContextManager contextManager, XmlSessionTracker logger);

		bool UseIncomingTransactionBatchData_Import(IEDIMessage message, UniversalTransactionBatch dataObject, UniversalObjectFactory factory, ITransactionBatchDataContextManager contextManager, XmlSessionTracker logger)
		{
			return contextManager.UseIncomingTransactionBatchData(message, dataObject, logger, factory);
		}

		UseIncomingTransactionBatchData UseIncomingTransactionBatchData_GetKeys(HashSet<(string KeyValue, string KeySource)> keysInfo)
		{
			return (message, dataObject, factory, contextManager, logger) =>
			{
				var result = contextManager.GetKeysForBlockingParallelImport(message, dataObject, logger, factory);
				if (result.IsMatch)
				{
					foreach (var keyInfo in result.KeysInfo)
					{
						keysInfo.Add(keyInfo);
					}
				}

				return result.IsMatch;
			};
		}
	}
}
