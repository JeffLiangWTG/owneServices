using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Common;
using Enterprise.Accounting.ElectronicMessaging.efatura.uyumsoft.com.tr;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Accounting.ElectronicMessaging.Turkey
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("will be used in the Turkey XML Mapping functionality")]
	[XmlSerializerAssembly("Enterprise.Accounting.ElectronicMessaging.XmlSerializers")]
	public class RCNXmlWriter
	{
		/// <summary>
		/// Generate Turkey E-Invoice XML when given a universal transaction batch
		/// </summary>
		/// <param name="invoiceId">GUID represents Turkish AR e-Archive invoice to cancel</param>
		/// <param name="cancelDate">Cancellation date of the Turkish AR e-Archive invoice</param>
		/// <param name="stream">Into which we write the XML, this can be a file stream or in memory stream passed by the consumer of this method.</param>
		/// <param name="notifications">To notify about validation problems with the generated XML</param>
		public void WriteXmlToStream(string invoiceId, DateTime cancelDate, Stream stream)
		{
			var eArchiveCancelInvoiceContext = new CancelEArchiveInvoice();

			eArchiveCancelInvoiceContext.request = new EArchiveCancelInvoiceContext()
			{
				InvoiceId = invoiceId,
				CancelDate = cancelDate.Date
			};

			var settings = new XmlWriterSettings
			{
				ConformanceLevel = ConformanceLevel.Document,
				OmitXmlDeclaration = false,
				NamespaceHandling = NamespaceHandling.Default,
				Encoding = Encoding.UTF8,
				NewLineHandling = NewLineHandling.Replace,
				Indent = true,
				IndentChars = "  ",
				NewLineChars = "\r\n",
			};

			XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
			ns.Add("", "http://tempuri.org/");
			var serializer = ZXmlSerializer.New(eArchiveCancelInvoiceContext.GetType());

			var doc = new XmlDocument();

			using (var tmpStream = new MemoryStream())
			{
				using (XmlWriter writer = XmlWriter.Create(tmpStream, settings))
				{
					writer.WriteStartDocument();
					serializer.Serialize(writer, eArchiveCancelInvoiceContext, ns);
					writer.WriteEndDocument();
					writer.Flush();
				}

				tmpStream.Position = 0;
				using (var reader = new StreamReader(tmpStream, true))
				{
					var xml = reader.ReadToEnd();
					doc.LoadXml(xml);
				}
			}

			var contextNode = doc.DocumentElement?.GetElementsByTagName(nameof(eArchiveCancelInvoiceContext.request))?.Item(0);
			var attribute = contextNode?.Attributes.GetNamedItem(nameof(EArchiveCancelInvoiceContext.CancelDate));
			if (attribute == null)
			{
				var message = string.Format(CultureInfo.InvariantCulture, (NoResString)"Xml is not created properly, missing attribute {0}.", nameof(EArchiveCancelInvoiceContext.CancelDate)); // Developer Notification Key
				ErrorReporter.ReportOnce("EArchiveCancelInvoiceContextCancelDateMissing", message);
			}
			else
			{
				attribute.Value = cancelDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
			}

			using (XmlWriter writer = XmlWriter.Create(stream, settings))
			{
				doc.WriteContentTo(writer);
			}
		}
	}
}
