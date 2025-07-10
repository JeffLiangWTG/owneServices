using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common.DataValidation;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;

namespace Enterprise.Accounting.ElectronicMessaging.KoreaSouth
{
	class KoreaSouthEInvoiceXmlWriter : TransactionBatchToPayloadWriterBase
	{
		public KoreaSouthEInvoiceXmlWriter(TaxInvoiceBuilder taxInvoiceBuilder)
		{
			TaxInvoiceBuilder = taxInvoiceBuilder;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Element name")]
		protected override void WritePayloadToStream(TransactionBatch transactionBatch, Stream stream, ZString messageType, AccEInvoicingBatch accBatch, INotifications notifications, INotifications warnings)
		{
			if (transactionBatch?.TransactionCollection == null)
			{
				throw new ArgumentNullException(nameof(transactionBatch), "Transaction batch or collection can't be null.");
			}

			if (transactionBatch?.TransactionCollection?.Count < 1)
			{
				throw new ArgumentException("Transaction batch should at least have one transaction.");
			}

			XStreamingElement eInvoiceInfo;
			switch (messageType)
			{
				case KoreaSouthEInvoiceAPICommandList.Codes.GenerateInvoiceRequest:
					eInvoiceInfo = new XStreamingElement("TaxInvoiceSet");
					foreach (var transaction in transactionBatch.TransactionCollection)
					{
						var taxInvoice = TaxInvoiceBuilder.BuildTaxInvoice(transaction, accBatch, notifications);
						if (taxInvoice != null)
						{
							eInvoiceInfo.Add(taxInvoice);
						}
					}
					break;
				case KoreaSouthEInvoiceAPICommandList.Codes.QueryInvoiceRequest:
					eInvoiceInfo = new XStreamingElement("Empty");
					break;
				default:
					throw new ArgumentException("Invalid Message Type.");
			}

			var writerSettings = new XmlWriterSettings { NamespaceHandling = NamespaceHandling.OmitDuplicates, };

			using (var writer = XmlWriter.Create(stream, writerSettings))
			{
				writer.WriteStartDocument();
				eInvoiceInfo.WriteTo(writer);
				writer.WriteEndDocument();
				writer.Flush();
			}
		}

		protected override IPayloadValidation GetPayloadValidation(ZString messageType)
		{
			switch (messageType)
			{
				case KoreaSouthEInvoiceAPICommandList.Codes.GenerateInvoiceRequest:
					return new GenerateInvoiceRequestPayloadValidation();
				case KoreaSouthEInvoiceAPICommandList.Codes.QueryInvoiceRequest:
					return new QueryInvoiceRequestPayloadValidation();
				default:
					return null;
			}
		}

		TaxInvoiceBuilder TaxInvoiceBuilder { get; }
	}
}
