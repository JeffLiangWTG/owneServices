using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class ICS2InboundEDIMessageLookups : EDIMessageLookups
	{
		public ICS2InboundEDIMessageLookups(ICS2InboundEDIMessage parent)
			: base(parent)
		{
		}

		public override CodeDescriptionPairList MessageTypeList => Factory.GetCachedValue<MessageTypes>();
	}
}
