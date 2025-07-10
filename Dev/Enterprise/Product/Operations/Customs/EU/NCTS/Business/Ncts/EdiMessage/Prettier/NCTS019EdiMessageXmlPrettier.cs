using Enterprise.Messaging.Business;

namespace Enterprise.Customs.EU.NCTS.Business;

public class NCTS019EdiMessageXmlPrettier : NctsEdiMessagePrettier<NCTS019EdiMessageParsingXMLPrettyDataProvider>
{
	public NCTS019EdiMessageXmlPrettier(EDIMessage message) : base(message, new NCTS019EdiMessageParsingXMLPrettyDataProvider(message))
	{ }
}
