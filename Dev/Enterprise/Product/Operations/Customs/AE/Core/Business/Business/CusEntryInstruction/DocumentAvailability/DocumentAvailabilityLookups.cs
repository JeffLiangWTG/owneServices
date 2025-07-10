using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AE.Business;

public class DocumentAvailabilityLookups : CusSupportingInfoLookups
{
	public DocumentAvailabilityLookups(DocumentAvailability parent)
		: base(parent)
	{
	}

	public new DocumentAvailability Parent => (DocumentAvailability)base.Parent;

	public CodeDescriptionPairList DocumentTypeList => Factory.GetCachedValue<DocumentTypeList>();

	public CodeDescriptionPairList AvailabilityStatusList => Factory.GetCachedValue<AvailabilityStatusList>();

	public CodeDescriptionPairList ReasonCodeList => Factory.GetCachedValue<ReasonCodeList>();
}
