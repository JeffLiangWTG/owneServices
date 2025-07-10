using Enterprise.Customs.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class LegalActInfoLookups : CusSupportingInfoLookups
	{
		public LegalActInfoLookups(LegalActInfo parent) : base(parent)
		{
		}

		public new LegalActInfo Parent => base.Parent as LegalActInfo;

		public CodeDescriptionPairList AdditionalTaxTypeList => Factory.GetCachedValue<AdditionalTaxTypeList>();

		public CodeDescriptionPairList ExTariffLegalActList => BRRefCusCodeListTypes.GetExTariffLegalActList(Factory);

		public CodeDescriptionPairList LegalActIssuingAuthorityList => BRRefCusCodeListTypes.GetLegalActIssuingAuthorityList(Factory);
	}
}
