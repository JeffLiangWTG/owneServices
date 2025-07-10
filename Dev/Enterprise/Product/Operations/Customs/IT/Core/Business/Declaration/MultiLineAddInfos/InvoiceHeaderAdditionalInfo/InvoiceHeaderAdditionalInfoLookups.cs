using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Business.Declaration;

public class InvoiceHeaderAdditionalInfoLookups : EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoLookups
{
	public InvoiceHeaderAdditionalInfoLookups(InvoiceHeaderAdditionalInfo parent) : base(parent)
	{
	}

	public override CodeDescriptionPairList SubTypeList => Factory.GetCachedValue<EU.Business.AdditionalInfoSubTypeList>();

	protected override ZBool OmitLevelAttribute => true;
}
