using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Business.Declaration;

public class SupportingDocumentLookups : EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentLookups
{
	public SupportingDocumentLookups(SupportingDocument parent)
		: base(parent)
	{
	}

	protected new SupportingDocument Parent => (SupportingDocument)base.Parent;

	public override CodeDescriptionPairList StatusList => Factory.GetCachedValue<AvailabilityTypeList>();

	public override CodeDescriptionPairList UnitOfQuantityList => UniversalReferenceHelper.GetEuropeanUnionEUNCustomsUQCodeList(Factory);
}
