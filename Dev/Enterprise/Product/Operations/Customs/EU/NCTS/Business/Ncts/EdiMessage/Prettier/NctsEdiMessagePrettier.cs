using Enterprise.Messaging.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsEdiMessageXmlPrettier : NctsEdiMessagePrettier<EdiMessageParsingXMLPrettyDataProvider>
	{
		public NctsEdiMessageXmlPrettier(EDIMessage message) : base(message, new EdiMessageParsingXMLPrettyDataProvider(message))
		{ }
	}
}
