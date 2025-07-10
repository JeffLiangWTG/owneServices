using System.IO;
using System.Xml.Linq;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.EU.H7.Business
{
	public static class EUH7IncomingMessageImporter
	{
		public static void Import(Stream fileStream, AsycudaManifestHeader manifestHeader)
		{
			var inboundEDIMessageApplicationCode = manifestHeader.ApplicationBusinessProvider.InboundEDIMessageApplicationCode;
			using (var reader = new StreamReader(fileStream))
			{
				var messageBody = reader.ReadToEnd();

				var xmlMessage = XDocument.Parse(messageBody);
				var messageType = xmlMessage.Root.Name.LocalName.Substring(2, 3);

				var newEdiMessage = manifestHeader.Factory.New<EDIMessage>();
				newEdiMessage.EM_ApplicationCode = inboundEDIMessageApplicationCode;
				newEdiMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
				newEdiMessage.EM_Status = EDIMessage.Status.Queued;
				newEdiMessage.EM_MessageType = messageType;
				newEdiMessage.EM_MessageText = messageBody;
				newEdiMessage.EM_LinkedObject = manifestHeader;
			}

			manifestHeader.Factory.Save();
		}
	}
}
