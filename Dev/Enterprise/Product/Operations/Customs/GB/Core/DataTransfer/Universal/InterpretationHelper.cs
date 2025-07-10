using System.Collections.Specialized;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using CargoWise.Types;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.GB.DataTransfer.Universal
{
	static class InterpretationHelper
	{
		public static ZString RemoveAllNamespaces(ZString xmlDocument)
		{
			try
			{
				var xmlDocumentWithoutNamespaces = RemoveAllNamespaces(XElement.Parse(xmlDocument));
				return xmlDocumentWithoutNamespaces.ToString();
			}
			catch (XmlException)
			{
				return xmlDocument;
			}
		}

		public static HtmlTableCreator GetHtmlTableCreator(string width = "100%")
		{
			return new HtmlTableCreator(new NameValueCollection
			{
				{ "border", "1" },
				{ "cellpadding", "1" },
				{ "cellspacing", "0" },
				{ "width", width },
				{ "class", "table" }
			})
			{
				EnableHTMLEncoding = false
			};
		}

		public static void CreateTableOfErrorsFromNodeList(ZStringBuilder result, XmlNodeList errors, (string columnTitle, string xPath)[] columns)
		{
			foreach (XmlNode error in errors)
			{
				var tableCreator = GetHtmlTableCreator();
				tableCreator.WriteRow("<strong>Key</strong>", "<strong>Value</strong>");
				foreach (var (columnTitle, xPath) in columns)
				{
					tableCreator.WriteRow(columnTitle, error.SelectSingleNode(xPath)?.InnerText ?? string.Empty);
				}
				_ = result.Append($"<ul>{tableCreator.ToHtml()}</ul><br>");
			}
		}

		static XElement RemoveAllNamespaces(XElement xmlDocument)
		{
			if (!xmlDocument.HasElements)
			{
				var xElement = new XElement(xmlDocument.Name.LocalName);
				xElement.Value = xmlDocument.Value;

				foreach (XAttribute attribute in xmlDocument.Attributes())
				{
					xElement.Add(attribute);
				}

				return xElement;
			}
			return new XElement(xmlDocument.Name.LocalName, xmlDocument.Elements().Select(RemoveAllNamespaces));
		}
	}
}
