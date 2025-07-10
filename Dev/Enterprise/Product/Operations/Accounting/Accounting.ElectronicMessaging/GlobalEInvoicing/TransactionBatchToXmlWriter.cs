using System;
using System.IO;
using System.Xml;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common.DataValidation;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;

namespace Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing
{
	public abstract class TransactionBatchToXmlWriter : TransactionBatchToPayloadWriterBase
	{
		protected virtual XmlWriterSettings Settings()
		{
			return new XmlWriterSettings
			{
				NamespaceHandling = NamespaceHandling.OmitDuplicates,
			};
		}

		protected abstract void WriteDocumentBody(XmlWriter writer, TransactionInfo transactionInfo, ZString messageType, AccEInvoicingBatch accBatch, INotifications notifications, INotifications warnings);

		protected override void WritePayloadToStream(TransactionBatch transactionBatch, Stream stream, ZString messageType,
			AccEInvoicingBatch accBatch, INotifications notifications, INotifications warnings)
		{
			if (transactionBatch?.TransactionCollection == null)
			{
				throw new ArgumentNullException(nameof(transactionBatch), "Transaction batch or collection can't be null.");
			}

			if (transactionBatch?.TransactionCollection?.Count != 1)
			{
				throw new ArgumentException("Each transaction batch can have only one transaction.");
			}

			WriteXmlToStreamCore(transactionBatch.TransactionCollection[0], stream, messageType, accBatch, notifications, warnings);
		}

		protected virtual void WriteXmlToStreamCore(TransactionInfo transactionInfo, Stream stream, ZString messageType, AccEInvoicingBatch accBatch, INotifications notifications, INotifications warnings)
		{
			using (var writer = XmlWriter.Create(stream, Settings()))
			{
				writer.WriteStartDocument();
				WriteDocumentBody(writer, transactionInfo, messageType, accBatch, notifications, warnings);
				writer.WriteEndDocument();
				writer.Flush();
			}
		}

		protected override IPayloadValidation GetPayloadValidation(ZString messageType) => XsdResourceNames != null ? new XsdValidation(XsdResourceNames) : null;

		protected virtual ZString[] XsdResourceNames => null;
	}
}
