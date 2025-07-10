using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;

public class CusTempStorageRegHeaderLookups	: AutoCusTempStorageRegHeaderLookups
{
	public CusTempStorageRegHeaderLookups(AutoCusTempStorageRegHeader parent) : base(parent)
	{
	}

	public virtual CodeDescriptionPairList PreviousReferenceTypeList
	{
		get
		{
			return RefCusCodeListTypes.GetCachedListMatchAnyAttributes(Factory, Country, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfPNTS, ZDateTime.Today, new[] { new KeyValuePair<ZString, ZString>(RefCusCodeListAttributeTypes.Codes.Purpose, Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.Declaration) });
		}
	}

	static ZString Country => GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

	public virtual CodeDescriptionPairList StatusList => new ();

	public virtual CustomsOfficeCodeCollection CustomsOfficeList => CustomsOfficeCodeCollection.GetCachedCollection(Factory, Country, ZDateTime.Today);

	public CodeDescriptionPairList PackTypeList => Universal.RefCusCodeListTypes.GetCachedList(Factory,
		Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations,
		Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
		ZDateTime.Today);
}
