using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Xml;
using UniversalTransactionBatch = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionBatch;

namespace Enterprise.Accounting.ElectronicMessaging.Turkey
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("will be used in the Turkey XML Mapping functionality")]
	[XmlSerializerAssembly("Enterprise.Accounting.ElectronicMessaging.XmlSerializers")]
	public class EInvoiceXmlWriter
	{
		/// <summary>
		/// Generate Turkey E-Invoice XML when given a universal transaction batch
		/// </summary>
		/// <param name="transactionBatch">Universal Transaction Batch representing EInvoice Batch</param>
		/// <param name="stream">Into which we write the XML, this can be a file stream or in memory stream passed by the consumer of this method.</param>
		/// <param name="notifications">To notify about validation problems with the generated XML</param>

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant String")]
		const string ns3 = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant String")]
		const string ns4 = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";

		public void WriteXmlToStream(UniversalTransactionBatch transactionBatch, GlbCompany company, Stream stream) //, INotifications notifications
		{
			if (transactionBatch?.TransactionCollection?.Count != 1)
			{
				throw new ArgumentException("EInvoiceXmlWriter will only accept one transaction per transaction batch.");
			}

			var eInvoiceInfo = new InvoiceInfoTag(transactionBatch.TransactionCollection.First(), company).BuildInvoiceInfo();

			var settings = new XmlWriterSettings
			{
				ConformanceLevel = ConformanceLevel.Document,
				OmitXmlDeclaration = false,
				NamespaceHandling = NamespaceHandling.OmitDuplicates,
				Encoding = Encoding.UTF8,
				NewLineHandling = NewLineHandling.Replace,
				Indent = true,
				IndentChars = "  ",
				NewLineChars = "\r\n",
			};

			XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
			ns.Add("ns3", ns3);
			ns.Add("ns4", ns4);
			var serializer = ZXmlSerializer.New(eInvoiceInfo.GetType());

			using (XmlWriter writer = XmlWriter.Create(stream, settings))
			{
				writer.WriteStartDocument();
				writer.WriteStartElement("ns0", "invoices", "http://tempuri.org/");
				serializer.Serialize(writer, eInvoiceInfo, ns);
				writer.WriteEndDocument();
				writer.Flush();
			}
		}
	}
}
