using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	internal class ICS2OutboundEDIMessageLookups : EDIMessageLookups
	{
		public ICS2OutboundEDIMessageLookups(ICS2OutboundEDIMessage parent)
		: base(parent)
		{
		}

		public override CodeDescriptionPairList MessageTypeList => Factory.GetCachedValue<MessageTypes>();
	}
}
