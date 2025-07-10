using Enterprise.Customs.IE.EMCS.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.EMCS.Business
{
	public class EMCSOutboundEDIMessageLookups : EDIMessageLookups
	{
		public EMCSOutboundEDIMessageLookups(EMCSOutboundEDIMessage parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList MessageTypeList => Factory.GetCachedValue<EMCSOutgoingMessageTypeList>();
	}
}
