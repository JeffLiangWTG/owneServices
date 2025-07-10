using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CH.Business.Testing;

public static class UniversalEventTestDataHelper
{
	public static ZString CreateUniversalInterchangeXml(string eventType = Events.InterchangeAcknowledgedCode, string messageType = null, string responseType = null, string reason = null, string responseMessage = null)
	{
		return $@"<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <Header>
    <SenderID>SenderID_0</SenderID>
    <RecipientID>RecipientID_0</RecipientID>
  </Header>
  <Body>
    {CreateUniversalEventXml(eventType, messageType, responseType, reason, responseMessage)}
  </Body>
</UniversalInterchange>";
	}

	public static ZString CreateUniversalEventXml(string eventType = Events.InterchangeAcknowledgedCode, string messageType = null, string responseType = null, string reason = null, string responseMessage = null)
	{
		return CreateUniversalEventXml(eventType, messageType, true, responseType, reason, responseMessage);
	}

	public static ZString CreateUniversalEventXmlWithoutContextCollection(string eventType = Events.InterchangeAcknowledgedCode, string messageType = MessageTypeCodeList.Codes.PassarNcts)
	{
		return CreateUniversalEventXml(eventType, messageType, false, null, null, null);
	}

	static ZString CreateUniversalEventXml(string eventType, string messageType, bool hasContextCollection, string responseType, string reason, string responseMessage)
	{
		StringBuilder builder = new StringBuilder();
		builder.Append($@"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""1.1"">
			<Event>
				<EventTime>2022-11-08T07:08:36</EventTime>
				<EventType>{eventType}</EventType>
				<EventParameters>
					<MessageType>{messageType}</MessageType>
					<Type>{responseType}</Type>
					<Reason>{reason}</Reason>
				</EventParameters>");

		if (hasContextCollection)
		{
			builder.Append($@"<ContextCollection>
				<Context>
					<Type>ResponseMessage</Type>
					<Value><![CDATA[{responseMessage}]]></Value>
				</Context>
			</ContextCollection>");
		}

		builder.Append($@"</Event></UniversalEvent>");

		return builder.ToString();
	}

	public static ZString ParseBodyFromUniversalInterchange(string xml) => XDocument.Parse(xml).Root.XPathSelectElement("//*[local-name()='UniversalInterchange']//*[local-name()='Body']").FirstNode.ToString();
}
