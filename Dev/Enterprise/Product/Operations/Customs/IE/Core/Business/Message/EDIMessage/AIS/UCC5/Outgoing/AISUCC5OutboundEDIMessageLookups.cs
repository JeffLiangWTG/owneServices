using Enterprise.Customs.IE.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.Business
{
	public class AISUCC5OutboundEDIMessageLookups : EDIMessageLookups
	{
		public AISUCC5OutboundEDIMessageLookups(AISUCC5OutboundEDIMessage parent)
			: base(parent)
		{
		}

		public override CodeDescriptionPairList MessageTypeList => Factory.GetCachedValue<AISInterchangeTypeList>();
	}
}
