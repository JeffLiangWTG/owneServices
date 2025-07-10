using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.PBN.Business;

public class PBNInboundEDIMessageLookups : EDIMessageLookups
{
	public PBNInboundEDIMessageLookups(AutoEDIMessage parent) : base(parent)
	{
	}

	public override CodeDescriptionPairList MessageTypeList => Factory.GetCachedValue<PBNMessageTypes>();
}
