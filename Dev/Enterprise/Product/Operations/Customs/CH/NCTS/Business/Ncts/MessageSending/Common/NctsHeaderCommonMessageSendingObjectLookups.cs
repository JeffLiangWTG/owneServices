using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NctsHeaderCommonMessageSendingObjectLookups : NctsHeaderMessageSendingObjectLookups
{
	public NctsHeaderCommonMessageSendingObjectLookups(NctsHeaderCommonMessageSendingObject parent) : base(parent)
	{
	}

	public new CodeDescriptionPairList MessageTypeList => new CodeDescriptionPairList();
}
