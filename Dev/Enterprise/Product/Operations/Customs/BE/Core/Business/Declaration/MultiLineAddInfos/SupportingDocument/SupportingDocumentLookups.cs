using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BE.Business.Declaration;

public class SupportingDocumentLookups : EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentLookups
{
	public SupportingDocumentLookups(SupportingDocument parent) : base(parent)
	{
	}

	public CodeDescriptionPairList BinaryIntValuesList => new BinaryIntValuesList();

	protected new SupportingDocument Parent => (SupportingDocument)base.Parent;

	public override CodeDescriptionPairList UnitOfQuantityList => CachedCustomsUQList;

	CodeDescriptionPairList CachedCustomsUQList => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Belgium, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, ZDateTime.Today);
}
