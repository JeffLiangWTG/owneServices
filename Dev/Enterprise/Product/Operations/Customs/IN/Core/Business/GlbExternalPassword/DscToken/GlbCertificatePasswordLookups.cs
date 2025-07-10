using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;
using CusCodeAttributes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeList.Attributes;
using CusCodeTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes;

namespace Enterprise.Customs.IN.Business;

public class GlbCertificatePasswordLookups : MasterFiles.Business.GlbExternalPasswordLookups
{
	public GlbCertificatePasswordLookups(GlbCertificatePassword parent) : base(parent)
	{
	}

	public CodeDescriptionPairList CertificateAuthorities
		=> RefCusCodeListTypes.GetCachedList(Factory, CountryCodes.India, CusCodeTypes.IndiaCertificateAuthority, ZDateTime.Today);

	public CodeDescriptionPairList Chipsets
		=> Factory.GetCachedValue($"Enterprise.Customs.IN.Business.GlbDigitalSignatureCertificateTokenDetailsLookups.Chipsets.{ZDateTime.Today}", GetChipsets);

	CodeDescriptionPairList GetChipsets()
	{
		var collection = ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, CountryCodes.India, CusCodeTypes.IndiaChipsetManufacturer, ZDateTime.Today);
		collection.Load();
		var result = new CodeDescriptionPairList();
		foreach (ZZRefCusCodeListCombined item in collection)
		{
			var description = item.GetAttribute(CusCodeAttributes.IndiaChipsetdll);
			result.AddPair(item.ZZD_Code, description);
		}
		return result;
	}
}
