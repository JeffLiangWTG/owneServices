using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IN.Business;

public class EDIMessageLookups : Messaging.Business.EDIMessageLookups
{
	public EDIMessageLookups(EDIMessage parent) : base(parent)
	{
	}

	public override CodeDescriptionPairList MessageTypeList => Factory.GetCachedValue<EDIMessageTypeList>();
}
