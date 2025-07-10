using System;
using System.IO;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common.DataValidation;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;

namespace Enterprise.Accounting.ElectronicMessaging.China
{
	public class ChinaPayloadWriter : TransactionBatchToPayloadWriterBase
	{
		protected override void WritePayloadToStream(TransactionBatch transactionBatch, Stream stream, ZString messageType, AccEInvoicingBatch accBatch, INotifications notifications, INotifications warnings)
		{
			Argument.NotNull(transactionBatch, nameof(transactionBatch));
			Argument.NotNull(stream, nameof(stream));
			Argument.NotNull(accBatch, nameof(accBatch));
			Argument.NotNull(notifications, nameof(notifications));
			Argument.NotNull(warnings, nameof(warnings));

			var transactionHeader = accBatch.TransactionPivots[0]?.ParentTransactionHeader;
			if (transactionHeader == null)
			{
				return;
			}

			IChinaPayloadCreator payloadCreator;
			switch (messageType)
			{
				case ChinaEInvoiceAPICommandList.Codes.GenerateInvoiceRequest:
					payloadCreator = new ChinaEInvoicePayloadCreator(transactionHeader);
					break;
				case ChinaEInvoiceAPICommandList.Codes.RequestDocumentForInvoice:
					payloadCreator = new ChinaSearchEInvoicePayloadCreator(transactionHeader);
					break;
				default:
					throw new ArgumentException("Unknown message type: " + messageType);
			}

			WritePayloadToStreamCore(payloadCreator, stream);
		}

		void WritePayloadToStreamCore(IChinaPayloadCreator creator, Stream stream)
		{
			var payloadAsJson = creator.CreatePayloadAsJson();
			var payloadAsJsonBytes = MessageEncoding.UTF8WithoutBOM.GetBytes(payloadAsJson);
			stream.Write(payloadAsJsonBytes, 0, payloadAsJsonBytes.Length);
		}

		protected override IPayloadValidation GetPayloadValidation(ZString messageType)
		{
			var schemaResourceName = GetSchemaResourceName(messageType);
			return schemaResourceName.IsNullOrEmpty() ? null : new JsonValidation2(schemaResourceName);
		}

		string GetSchemaResourceName(ZString messageType)
		{
			switch (messageType)
			{
				case ChinaEInvoiceAPICommandList.Codes.GenerateInvoiceRequest:
					return GENSchemaResourceName;
				case ChinaEInvoiceAPICommandList.Codes.RequestDocumentForInvoice:
					return RDNSchemaResourceName;
				default:
					return string.Empty;
			}
		}

		const string GENSchemaResourceName = "Enterprise.Accounting.ElectronicMessaging.China.EInvoice.SendInvoice.ChinaEInvoiceSchema.json";
		const string RDNSchemaResourceName = "Enterprise.Accounting.ElectronicMessaging.China.EInvoice.SearchInvoice.ChinaSearchEInvoiceSchema.json";
	}
}
