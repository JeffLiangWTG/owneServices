using Enterprise.Customs.IE.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.Business
{
	public class AISUCC5InboundEDIMessageLookups : EDIMessageLookups
	{
		public AISUCC5InboundEDIMessageLookups(AISUCC5InboundEDIMessage parent)
			: base(parent)
		{
		}

		public override CodeDescriptionPairList MessageTypeList => Factory.GetCachedValue<AISInterchangeTypeList>();
	}
}
