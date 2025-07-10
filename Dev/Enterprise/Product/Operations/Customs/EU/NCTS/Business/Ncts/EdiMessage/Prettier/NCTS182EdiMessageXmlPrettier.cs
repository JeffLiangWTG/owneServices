using Enterprise.Messaging.Business;

namespace Enterprise.Customs.EU.NCTS.Business;

public class NCTS182EdiMessageXmlPrettier : NctsEdiMessagePrettier<NCTS182EdiMessageParsingXMLPrettyDataProvider>
{
	public NCTS182EdiMessageXmlPrettier(EDIMessage message) : base(message, new NCTS182EdiMessageParsingXMLPrettyDataProvider(message))
	{ }
}
