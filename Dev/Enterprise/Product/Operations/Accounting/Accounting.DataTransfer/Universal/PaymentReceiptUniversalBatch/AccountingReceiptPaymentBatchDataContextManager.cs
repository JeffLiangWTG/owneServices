using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using UniversalTransactionBatch = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionBatch;

namespace Enterprise.Accounting.DataTransfer.Universal
{
	public abstract class AccountingReceiptPaymentBatchDataContextManager<T> : DataContextManager<T>, ITransactionBatchDataContextManager
		where T : BusinessObject
	{
		public override string DefaultOutputDirectory => null;

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new ZQuery { IsNoResultQuery = true };
		}

		#region ITransactionBatchDataContextManager

		public bool ManagesTransactionBatches
		{
			get { return true; }
		}

		public IKeysResult GetKeysForBlockingParallelImport(IEDIMessage message, ITopLevelDataObject dataObject, IXmlSessionTracker logger, IUniversalObjectFactory factory)
		{
			return new UniversalKeysResult(Enumerable.Empty<(string KeyValue, string KeySource)>());
		}

		public abstract ITopLevelDataObjectWriter GetTransactionBatchDataObjectWriter(IDataWritingManager writeManager);

		public bool UseIncomingTransactionBatchData(IEDIMessage message, ITopLevelDataObject dataObject, IXmlSessionTracker logger, IUniversalObjectFactory factory)
		{
			var result = false;

			var universalTransactionBatch = (UniversalTransactionBatch)dataObject;
			var transactionCollection = universalTransactionBatch.TransactionCollection;

			var dataTargetType = transactionCollection?.FirstOrDefault()?.DataContext?.DataTargetCollection?.FirstOrDefault()?.Type.GetValueOrDefault() ?? ZString.Empty;
			if (DataContextType.ToString() == dataTargetType)
			{
				var transactionBatchImporter = new TransactionBatchImporter(universalTransactionBatch, logger, (UniversalObjectFactory)factory);
				result = transactionBatchImporter.ImportTransactionBatch();
			}

			return result;
		}

		#endregion

	}
}
