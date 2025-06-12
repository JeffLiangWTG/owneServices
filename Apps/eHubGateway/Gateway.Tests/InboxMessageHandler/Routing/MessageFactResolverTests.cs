using System.IO;
using System.Text;
using System.Xml.XPath;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Gateway.Tests.InboxMessageHandler.Routing
{
	[TestClass]
	public class MessageFactResolverTests
	{
		[TestMethod]
		public void TestGetMessageType_NoNamespace_Root()
		{
			var message = "<GlobalElectronicInvoicing><Payload>Hello</Payload></GlobalElectronicInvoicing>";

			var result = ActOnGetMessageType(message);

			StringAssert.StartsWith("GlobalElectronicInvoicing", result);
		}

		[TestMethod]
		public void TestGetMessageType_NamespaceWithoutAlias_NamespaceHashRoot()
		{
			var message = "<GlobalElectronicInvoicing xmlns=\"http://cargowise.com/ehub/products/Accounting/GlobalElectronicInvoicing\"><Payload>Hello</Payload></GlobalElectronicInvoicing>";

			var result = ActOnGetMessageType(message);

			StringAssert.StartsWith("http://cargowise.com/ehub/products/Accounting/GlobalElectronicInvoicing#GlobalElectronicInvoicing", result);
		}

		[TestMethod]
		public void TestGetMessageType_NamespaceWithPrefix_NamespaceHashRoot()
		{
			var message = "<ns1:GlobalElectronicInvoicing xmlns:ns1=\"http://cargowise.com/ehub/products/Accounting/GlobalElectronicInvoicing\"><Payload>Hello</Payload></ns1:GlobalElectronicInvoicing>";

			var result = ActOnGetMessageType(message);

			StringAssert.StartsWith("http://cargowise.com/ehub/products/Accounting/GlobalElectronicInvoicing#GlobalElectronicInvoicing", result);
		}

		[TestMethod]
		public void TestGetMessageType_NoNamespaceWithOtherPrefix_Root()
		{
			var message = "<GlobalElectronicInvoicing xmlns:ns1=\"http://cargowise.com/ehub/products/Accounting/GlobalElectronicInvoicing\"><Payload>Hello</Payload></GlobalElectronicInvoicing>";

			var result = ActOnGetMessageType(message);

			StringAssert.StartsWith("GlobalElectronicInvoicing", result);
		}

		public string ActOnGetMessageType(string message)
		{
			var msgAsByteArray = Encoding.UTF8.GetBytes(message);
			XPathDocument xpDoc;

			using (var ms = new MemoryStream(msgAsByteArray))
			{
				xpDoc = new XPathDocument(ms);
			}

			return GetMessageType(xpDoc);
		}
		public string GetMessageType(XPathDocument xpDoc)
		{
			var xpNav = xpDoc.CreateNavigator();
			xpNav.MoveToRoot();
			xpNav.MoveToFollowing(XPathNodeType.Element);

			return string.IsNullOrWhiteSpace(xpNav.NamespaceURI)
				? xpNav.LocalName
				: $"{xpNav.NamespaceURI}#{xpNav.LocalName}";
		}
	}
}