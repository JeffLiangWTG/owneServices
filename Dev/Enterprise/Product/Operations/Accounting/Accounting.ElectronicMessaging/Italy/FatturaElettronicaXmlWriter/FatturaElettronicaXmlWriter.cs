using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using UniversalTransactionBatch = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionBatch;

namespace Enterprise.Accounting.ElectronicMessaging.Italy
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("will be used in the Italy XML Mapping functionality")]
	public class FatturaElettronicaXmlWriter
	{
		/// <summary>
		/// Generate Italian Fattura Elettronica XML when given a universal transaction batch
		/// </summary>
		/// <param name="transactionBatch">Universal Transaction Batch representing EInvoice Batch</param>
		/// <param name="stream">Into which we write the XML, this can be a file stream or in memory stream passed by the consumer of this method.</param>
		/// <param name="errorNotifications">To notify about validation problems with the generated XML</param>
		/// <param name="warningNotifications">To notify about validation warnings with the generated XML</param>
		public void WriteXmlToStream(UniversalTransactionBatch transactionBatch, Stream stream, INotifications errorNotifications, INotifications warningNotifications = null)
		{
			if (transactionBatch?.TransactionCollection?.Count != 1)
			{
				throw new ArgumentException("FatturaElettronicaXmlWriter will only accept one transaction per transaction batch.");
			}

			var xml = BuildXml(transactionBatch.TransactionCollection.First(), errorNotifications, warningNotifications);

			var settings = new XmlWriterSettings
			{
				ConformanceLevel = ConformanceLevel.Document,
				OmitXmlDeclaration = false,
				NamespaceHandling = NamespaceHandling.OmitDuplicates,
				Encoding = Encoding.UTF8,
				NewLineHandling = NewLineHandling.Replace,
				NewLineChars = " "
			};

			using (var writer = XmlWriter.Create(stream, settings))
			{
				writer.WriteStartDocument();
				xml.WriteTo(writer);
				writer.WriteEndDocument();
				writer.Flush();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "hard-coded xml namespace")]
		XStreamingElement BuildXml(TransactionInfo transaction, INotifications errorNotifications, INotifications warningNotifications)
		{
			XNamespace p = "http://ivaservizi.agenziaentrate.gov.it/docs/xsd/fatture/v1.2";
			XNamespace ds = "http://www.w3.org/2000/09/xmldsig#";
			XNamespace xsi = "http://www.w3.org/2001/XMLSchema-instance";
			var xsdSchemaLocation = FatturaElettronicaDataHelper.ShouldUseNewSchema
				? "http://www.fatturapa.gov.it/export/fatturazione/sdi/fatturapa/v1.2/Schema_del_file_xml_FatturaPA_versione_1.2.1.xsd"
				: "http://www.fatturapa.gov.it/export/fatturazione/sdi/fatturapa/v1.2/Schema_del_file_xml_FatturaPA_versione_1.2.xsd";
			XNamespace schemaLocation = "http://ivaservizi.agenziaentrate.gov.it/docs/xsd/fatture/v1.2 " + xsdSchemaLocation;

			var orgHeader = transaction?.OrganizationAddress?.GetOrgHeader(factory);
			var recipientOrgCategory = orgHeader.GetCategory();
			var recipientOrgCountry = transaction?.OrganizationAddress?.Country?.Code ?? ZString.Empty;
			var formatoTransmissione = FatturaElettronicaDataHelper.GetTransmissionFormat(recipientOrgCategory, recipientOrgCountry);

			return new XStreamingElement(p + "FatturaElettronica",
				new XAttribute("versione", formatoTransmissione),
				new XAttribute(XNamespace.Xmlns + "p", p.NamespaceName),
				new XAttribute(XNamespace.Xmlns + "ds", ds.NamespaceName),
				new XAttribute(XNamespace.Xmlns + "xsi", xsi.NamespaceName),
				new XAttribute(xsi + "schemaLocation", schemaLocation.NamespaceName),
				BuildXmlForFatturaElettronicaHeader(transaction, recipientOrgCategory, orgHeader),
				BuildXmlForFatturaElettronicaBody(transaction, errorNotifications, warningNotifications)
				);
		}

		XStreamingElement BuildXmlForFatturaElettronicaHeader(TransactionInfo transaction, ZString recipientOrgCategory, OrgHeader orgHeader)
		{
			return new XStreamingElement("FatturaElettronicaHeader",
				new DatiTrasmissione().BuildXML(transaction, recipientOrgCategory),
				new CedentePrestatore().BuildXML(transaction, recipientOrgCategory, orgHeader),
				new CessionarioCommittente().BuildXML(transaction, recipientOrgCategory, orgHeader)
				);
		}

		XStreamingElement BuildXmlForFatturaElettronicaBody(TransactionInfo transaction, INotifications errorNotifications, INotifications warningNotifications)
		{
			return new XStreamingElement("FatturaElettronicaBody",
				new DatiGenerali(factory).BuildXML(transaction, errorNotifications, warningNotifications),
				new DatiBeniServizi().BuildXML(transaction),
				FatturaElettronicaDataHelper.IsPayable(transaction) ? null : new DatiPagamento().BuildXML(transaction),
				new Allegati().BuildXML(transaction)
				);
		}

		readonly BusinessObjectFactory factory = new BusinessObjectFactory();
	}
}
