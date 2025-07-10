using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.EU.Business.WorldCustomsOrganisation.Testing
{
	class WCOExtensionsTest : TestCaseWithFactory
	{
		public void TestGetLatestMessageForComparison()
		{
			var entry = Factory.New<CusEntryHeader>();
			var ediMessage = entry.Messages.AddNew();
			ediMessage.EM_MessageText = @"<MetaData xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""urn:wco:datamodel:WCO:DocumentMetaData-DMS:2""></MetaData>";
			ediMessage.EM_Status = EDIMessageStatusList.Codes.Queued;
			ediMessage.EM_MessageType = WCOEDIMessageTypeList.Codes.NewDeclaration;
			var messageText = entry.GetLatestMessageForComparison()?.EM_MessageText ?? string.Empty;
			AssertEquals(ediMessage.EM_MessageText, messageText);

			ediMessage.EM_MessageType = WCOEDIMessageTypeList.Codes.DMSNewDeclaration;
			messageText = entry.GetLatestMessageForComparison()?.EM_MessageText ?? string.Empty;
			AssertEquals(ediMessage.EM_MessageText, messageText);

			ediMessage.EM_MessageType = WCOEDIMessageTypeList.Codes.NewAmendment;
			messageText = entry.GetLatestMessageForComparison()?.EM_MessageText ?? string.Empty;
			AssertEquals(ediMessage.EM_MessageText, messageText);

			ediMessage.EM_Status = EDIMessageStatusList.Codes.Discarded;
			messageText = entry.GetLatestMessageForComparison()?.EM_MessageText ?? string.Empty;
			AssertNullOrEmpty(messageText);
		}

		public void TestRemoveAllNamespaces()
		{
			XNamespace aw = "http://www.adventure-works.com";
			XElement rootXml = new XElement(aw + "Root",
				new XAttribute(XNamespace.Xmlns + "aw", "http://www.adventure-works.com"),
				new XAttribute(aw + "Att", "content")
			);

			AssertEquals(aw, rootXml.GetNamespaceOfPrefix("aw"));
			rootXml.RemoveAllNamespaces();
			AssertNotEquals(aw, rootXml.GetNamespaceOfPrefix("aw"));
		}

		public void TestSequence()
		{
			var xmlDoc = XDocument.Parse("<Root><Element><SequenceNumeric>1</SequenceNumeric></Element><Element><SequenceNumeric>2</SequenceNumeric></Element><Element><SequenceNumeric>3</SequenceNumeric></Element></Root>");

			AssertEquals("1", GetSequence(xmlDoc.XPathSelectElement("/Root/Element[1]/SequenceNumeric"), "Element"));
			AssertEquals("2", GetSequence(xmlDoc.XPathSelectElement("/Root/Element[2]/SequenceNumeric"), "Element"));
			AssertEquals("3", GetSequence(xmlDoc.XPathSelectElement("/Root/Element[3]/SequenceNumeric"), "Element"));
		}

		string GetSequence(XElement selectedNode, string localName)
		{
			var elementList = new List<XElement>(new XElement[] { selectedNode });
			var e = selectedNode;

			while (e.Parent != null)
			{
				e = e.Parent;
				if (e.NodeType == XmlNodeType.Document)
				{
					break;
				}
				elementList.Add(e);
			}
			return string.Join("/*", elementList.Where(x => x.Name.LocalName == localName).Select(x => x.Sequence()).Reverse());
		}
	}
}
