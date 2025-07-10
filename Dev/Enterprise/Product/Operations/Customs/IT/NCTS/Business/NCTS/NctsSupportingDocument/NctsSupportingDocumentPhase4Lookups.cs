using Enterprise.Customs.IT.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.NCTS.Business;

sealed class NctsSupportingDocumentPhase4Lookups : EU.NCTS.Business.NctsSupportingDocumentPhase4Lookups
{
	public NctsSupportingDocumentPhase4Lookups(EU.NCTS.Business.NctsSupportingDocument parent) : base(parent)
	{
	}

	public override CodeDescriptionPairList StatusList => Factory.GetCachedValue<AvailabilityTypeList>();
}
