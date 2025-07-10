using Enterprise.Customs.GB.EMCS.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.EMCS.Business
{
	public class EMCSInboundEDIMessageLookups : EDIMessageLookups
	{
		public EMCSInboundEDIMessageLookups(EMCSInboundEDIMessage parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList MessageTypeList => Factory.GetCachedValue<EMCSGBIncomingMessageTypeList>();
	}
}
