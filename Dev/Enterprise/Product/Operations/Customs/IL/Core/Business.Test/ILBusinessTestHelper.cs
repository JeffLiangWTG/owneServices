using System.Xml.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Testing;
using static Enterprise.Customs.IL.Business.Constants;

namespace Enterprise.Customs.IL.Business.Testing
{
	public static class ILBusinessTestHelper
	{
		public static TestEdiMessage CreateMessage(BusinessObjectFactory factory, string messageType)
		{
			var message = factory.New<TestEdiMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.ILCustoms;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_MessageText = "<xml><Book>Test</Book></xml>";
			message.EM_MessageType = messageType;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_Status = EDIMessage.Status.Queued;

			return message;
		}

		public static string GetEmbeddedResourcePath(string fileName) => "Enterprise.Customs.IL.Business.Testing.TestFiles." + fileName;

		public static string GetMessageResponseHeader(string messageBodyText)
		{
			var doc = Extensions.TryParseXML(messageBodyText);
			var ns = XNamespace.Get("http://www.w3.org/2003/05/soap-envelope");
			var headerElement = doc.Root.Element(ns + Unpacker.ResponseHeader);

			var ns0 = XNamespace.Get("http://MalamTeam.Inf.ESB.Schemas.ResponseHeader");
			var responseHeaderElement = headerElement?.Element(ns0 + Unpacker.ResponseHeaderInner);
			var responseHeaderContent = (ZString)responseHeaderElement?.ToString();

			return responseHeaderContent;
		}

		public static string GetMessageBody(string messageBodyText)
		{
			var doc = Extensions.TryParseXML(messageBodyText);
			var ns = XNamespace.Get("http://www.w3.org/2003/05/soap-envelope");

			var bodyElement = doc.Root.Element(ns + Unpacker.ResponseBody);
			var bodyContent = (ZString)bodyElement?.LastNode?.ToString();

			return bodyContent;
		}
	}
}
