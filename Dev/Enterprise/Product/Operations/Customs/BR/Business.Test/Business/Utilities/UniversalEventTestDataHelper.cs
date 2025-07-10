using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.BR.Business.Testing
{
	public static class UniversalEventTestDataHelper
	{
		public static ZString CreateUniversalInterchangeXml(string eventType = Events.InterchangeAcknowledgedCode, string messageType = null, string responseType = null, string reason = null, string responseMessage = null)
		{
			return $@"<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <Header>
	<SenderID>BRCustoms</SenderID>
	<RecipientID>HYEDBRSAO</RecipientID>
  </Header>
  <Body>
	{CreateUniversalEventXml(eventType, messageType, responseType, reason, responseMessage)}
  </Body>
</UniversalInterchange>";
		}

		public static ZString CreateUniversalEventXml(string eventType = Events.InterchangeAcknowledgedCode, string messageType = null, string responseType = null, string reason = null, string responseMessage = null)
		{
			return $@"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Event>
    <EventTime>2022-11-08T07:08:36</EventTime>
    <EventType>{eventType}</EventType>
    <EventParameters>
      <MessageType>{messageType}</MessageType>
      <Type>{responseType}</Type>
      <Reason>{reason}</Reason>
    </EventParameters>
    <ContextCollection>
      <Context>
        <Type>ResponseMessage</Type>
        <Value><![CDATA[{responseMessage}]]></Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>";
		}
	}
}
