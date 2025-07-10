using System.IO;
using System.Linq;
using System.Xml;
using CargoWise.Types;
using Enterprise.DocumentEngine.ReportErrorManagement;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine
{
	public class XmlCreateDeliveryInfoStrategy : ReportDataExportStrategy
	{
		Stream xmlStream;
		XmlWriter xmlWriter;
		string rootElementName;
		string[] columns;
		Report xmlDeliveryReport;

		protected override void Initialize(Report report, DocDeliveryContact deliveryContact, DeliveryInstructions deliveryInstructions)
		{
			xmlStream = new MemoryStream();
			var xmlWriterSetings = new XmlWriterSettings { Indent = true, CheckCharacters = false };
			xmlWriter = XmlWriter.Create(xmlStream, xmlWriterSetings);
			rootElementName = report.Name.KeepAlphanumericCharactersXMLFormatting();
			xmlWriter.WriteStartDocument();
			xmlWriter.WriteStartElement(rootElementName);
			xmlDeliveryReport = report;
		}

		protected override void ExportColumnHeadings(string[] columns)
		{
			this.columns = columns
				.Select(column => ((ZString)column).KeepAlphanumericCharactersXMLFormatting().ToString())
				.ToArray();

			if (columns.Any(c => string.IsNullOrWhiteSpace(c)))
			{
				var message = string.Format((NoResString)"Column headings should not contain empty values (invalid characters for XML elements are removed) since they are used for XML element names. Final column headings for report {0} are: '{1}'",
					rootElementName, string.Join("', '", columns));

				xmlDeliveryReport.ErrorManager.Add(new ReportFilterValidationError(message, ReportProcessingErrorSeverity.WarningWithoutErrorReport));
				xmlDeliveryReport.ErrorManager.ReportErrors();
			}

			WriteSchema();
		}

		void WriteSchema()
		{
			const string schemaNamespace = "http://www.w3.org/2001/XMLSchema";
			xmlWriter.WriteStartElement("xs", "schema", schemaNamespace);
			xmlWriter.WriteAttributeString("xmlns", "");
			xmlWriter.WriteAttributeString("id", rootElementName);
			{
				xmlWriter.WriteStartElement("element", schemaNamespace);
				xmlWriter.WriteAttributeString("name", rootElementName + "Item");
				xmlWriter.WriteStartElement("complexType", schemaNamespace);
				xmlWriter.WriteStartElement("sequence", schemaNamespace);
				foreach (string column in columns)
				{
					if (string.IsNullOrWhiteSpace(column))
					{
						continue;
					}

					xmlWriter.WriteStartElement("element", schemaNamespace);
					xmlWriter.WriteAttributeString("name", XmlConvert.VerifyName(column));
					xmlWriter.WriteAttributeString("type", "xs:string");
					xmlWriter.WriteAttributeString("minOccurs", "1");
					xmlWriter.WriteAttributeString("maxOccurs", "1");
					xmlWriter.WriteEndElement();
				}
				xmlWriter.WriteEndElement(); // sequence
				xmlWriter.WriteEndElement(); // complexType
				xmlWriter.WriteEndElement(); // element
			}
			xmlWriter.WriteEndElement();
		}

		protected override void ExportDataRow(string[] values)
		{
			xmlWriter.WriteStartElement(rootElementName + "Item");
			if (columns != null)
			{
				for (int i = 0; i < columns.Length && i < values.Length; i++)
				{
					if (string.IsNullOrWhiteSpace(columns[i]))
					{
						continue;
					}

					xmlWriter.WriteStartElement(columns[i]);
					xmlWriter.WriteString(values[i]);
					xmlWriter.WriteEndElement();
				}
			}
			xmlWriter.WriteEndElement();
		}

		protected override string AttachmentType
		{
			get { return AttachmentTypeList.Codes.Xml; }
		}

		protected override Stream GenerateAttachment()
		{
			xmlWriter.WriteEndElement();
			xmlWriter.WriteEndDocument();
			xmlWriter.Flush();
			xmlStream.Position = 0;
			return xmlStream;
		}
	}
}
