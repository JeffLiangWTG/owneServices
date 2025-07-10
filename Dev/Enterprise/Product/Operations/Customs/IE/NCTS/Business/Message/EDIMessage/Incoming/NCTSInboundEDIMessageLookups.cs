using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class NCTSInboundEDIMessageLookups : EDIMessageLookups
	{
		public NCTSInboundEDIMessageLookups(NCTSInboundEDIMessage parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList MessageTypeList => Factory.GetCachedValue<NCTSIncomingMessageTypeList>();
	}
}
