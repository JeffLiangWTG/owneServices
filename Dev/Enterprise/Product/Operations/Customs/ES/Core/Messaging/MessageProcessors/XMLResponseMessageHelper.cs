using System.Collections.Generic;
using System.IO;
using System.Xml;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageProcessors
{
	public static class XMLResponseMessageHelper
	{
		public static (ZString nodeInnerXmlString, ZString nodeValue) GetNode(ZString messageText, ZString nodeName)
		{
			var nodeInnerXml = ZString.Empty;
			var nodeValue = ZString.Empty;
			if (XmlUtils.IsValidXml(messageText, out var xmlDocument))
			{
				var node = xmlDocument.GetElementsByTagName(nodeName, "*");
				if (node != null && node.Count == 1)
				{
					nodeInnerXml = node[0].InnerXml;
					nodeValue = node[0].InnerText;
				}
			}
			return (nodeInnerXml, nodeValue);
		}

		public static List<ZString> GetMultipleNodes(ZString messageText, ZString nodeName)
		{
			var nodesXml = new List<ZString>();
			if (XmlUtils.IsValidXml(messageText, out var xmlDocument))
			{
				var nodes = xmlDocument.GetElementsByTagName(nodeName);
				foreach (XmlNode node in nodes)
				{
					nodesXml.Add(node.OuterXml);
				}
			}
			return nodesXml;
		}

		public static string GetAttributeValue(ZString messageText, ZString nodeName, string attributeName)
		{
			var attributeValue = ZString.Empty;
			if (XmlUtils.IsValidXml(messageText, out var xmlDocument))
			{
				var node = xmlDocument.GetElementsByTagName(nodeName);
				if (node != null && node.Count == 1 && node[0].Attributes != null)
				{
					var attr = node[0].Attributes[attributeName];
					if (attr != null)
					{
						attributeValue = attr.Value;
					}
				}
			}

			return attributeValue;
		}

		public static StringReader GetXmlBody(TextReader textReader)
		{
			var textString = textReader.ReadToEnd();
			textString = textString.Replace("\"\"", "\" \"");
			if (textString.Contains(BodyText))
			{
				var (nodeInnerXmlString, _) = GetNode(textString, BodyText);
				return new StringReader(nodeInnerXmlString);
			}
			else
			{
				return new StringReader(textString);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		const string BodyText = "Body";
	}
}
