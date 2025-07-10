using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.CH.Business;

public class NotifyCustomsOfficeLookups : CusCodeDataLookups
{
	public NotifyCustomsOfficeLookups(AutoCusCodeData parent) : base(parent)
	{
	}

	protected new NotifyCustomsOffice Parent => (NotifyCustomsOffice)base.Parent;

	public ZZRefCusCodeListCombinedCollection NotifyCustomsOfficeList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Switzerland, UniversalReferenceConstants.RefCusCodeList.EdecTypes.CustomsOffice, Parent.EffectiveAssessmentDate);
}
