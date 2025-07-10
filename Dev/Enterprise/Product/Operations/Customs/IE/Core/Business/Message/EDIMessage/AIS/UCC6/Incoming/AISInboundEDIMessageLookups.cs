using Enterprise.Customs.IE.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.Business
{
	public class AISInboundEDIMessageLookups : EDIMessageLookups
	{
		public AISInboundEDIMessageLookups(AISInboundEDIMessage parent)
			: base(parent)
		{
		}

		public override CodeDescriptionPairList MessageTypeList => Factory.GetCachedValue<AISInterchangeTypeList>();
	}
}
