using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IN.Business;

public class DeclarationMessageSendingObjectLookups : ZLookups
{
	public DeclarationMessageSendingObjectLookups(DeclarationMessageSendingObject parent) : base(parent)
	{
	}

	public CodeDescriptionPairList MessageTypes => Factory.GetCachedValue<DeclarationMessageTypeList>();
}
