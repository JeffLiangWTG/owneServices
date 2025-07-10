using System.IO;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common.DataValidation;
using UniversalTransactionBatch = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionBatch;

namespace Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing
{
	public interface ITransactionBatchToPayloadWriter
	{
		void WritePayloadToStream(UniversalTransactionBatch transactionBatch, Stream stream, ZString messageType, AccEInvoicingBatch accBatch, INotifications notifications, INotifications warnings);
		IPayloadValidation GetPayloadValidation(ZString messageType);
	}

	public abstract class TransactionBatchToPayloadWriterBase : ITransactionBatchToPayloadWriter
	{
		void ITransactionBatchToPayloadWriter.WritePayloadToStream(UniversalTransactionBatch transactionBatch, Stream stream, ZString messageType, AccEInvoicingBatch accBatch, INotifications notifications, INotifications warnings)
			=> WritePayloadToStream(transactionBatch, stream, messageType, accBatch, notifications, warnings);

		IPayloadValidation ITransactionBatchToPayloadWriter.GetPayloadValidation(ZString messageType) => GetPayloadValidation(messageType);

		protected abstract void WritePayloadToStream(UniversalTransactionBatch transactionBatch, Stream stream, ZString messageType, AccEInvoicingBatch accBatch, INotifications notifications, INotifications warnings);

		protected virtual IPayloadValidation GetPayloadValidation(ZString messageType) => null;
	}
}
