using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.Business;

public class EComplaintMessageSendingObjectLookups : ZLookups
{
	public EComplaintMessageSendingObjectLookups(EComplaintMessageSendingObject parent) : base(parent)
	{
	}

	public CodeDescriptionPairList CorrectionReasonList => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Switzerland, UniversalReferenceConstants.RefCusCodeList.EdecTypes.CorrectionReason, ZDateTime.Today);
}
