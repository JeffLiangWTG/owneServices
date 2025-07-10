using System.Xml;
using System.Xml.XPath;
using CargoWise.Types;

namespace Enterprise.Customs.GB.CDS
{
	public abstract class InventoryLinkingResponse
	{
		readonly XmlDocument xmlDoc;

		protected InventoryLinkingResponse(ZString xml)
		{
			XML = xml;
			xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(XML);
		}

		protected abstract ZString RootNode { get; }

		protected ZString SelectSingleNode(ZString xpath) => xmlDoc.SelectSingleNode(xpath)?.InnerText ?? ZString.Empty;
		protected IXPathNavigable SelectSingleXmlNode(ZString xpath) => xmlDoc.SelectSingleNode(xpath);
		protected XmlNodeList SelectNodes(ZString xpath) => xmlDoc.SelectNodes(xpath);
		protected XmlNodeList SelectNodes(IXPathNavigable node, ZString xpath) => (node as XmlNode).SelectNodes(xpath);
		protected ZString GetStringFromNode(IXPathNavigable node, ZString xPath) => (ZString)((node as XmlNode).SelectSingleNode(xPath)?.InnerText ?? string.Empty);
		protected ZBool GetBoolFromNode(ZString xPath) => ZBool.ParseSafe(xmlDoc.SelectSingleNode(xPath)?.InnerText ?? "false", ZBool.False);
		protected ZBool GetBoolFromNode(IXPathNavigable node, ZString xPath) => ZBool.ParseSafe((node as XmlNode).SelectSingleNode(xPath)?.InnerText ?? "false", ZBool.False);
		protected ZDecimal GetDecimalFromNode(IXPathNavigable node, ZString xPath) => ZDecimal.ParseSafe((node as XmlNode).SelectSingleNode(xPath)?.InnerText, ZDecimal.Zero);

		public ZString MessageCode => SelectSingleNode(MessageCodeNodeXPath);
		public ZString MovementReference => SelectSingleNode(MovementReferenceNodeXPath);

		public ZString XML { get; }

		protected virtual ZString MessageCodeNodeXPath => System.FormattableString.Invariant($"//*[local-name()='{RootNode}']/*[local-name()='messageCode']");
		ZString MovementReferenceNodeXPath => System.FormattableString.Invariant($"//*[local-name()='{RootNode}']/*[local-name()='movementReference']");
	}
}
