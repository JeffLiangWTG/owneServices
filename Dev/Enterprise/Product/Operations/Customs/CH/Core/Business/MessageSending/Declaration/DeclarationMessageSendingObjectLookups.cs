using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.Business;

public class DeclarationMessageSendingObjectLookups : ZLookups
{
	public DeclarationMessageSendingObjectLookups(BusinessObject parent) : base(parent)
	{
	}

	public virtual CodeDescriptionPairList MessageTypeList => new CodeDescriptionPairList();

	public virtual CodeDescriptionPairList CorrectionReasonList => new CodeDescriptionPairList();
}
