using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Manifest.Business
{
	class IcsOfficeCodeLookups : EuOfficeCodeLookups
	{
		public IcsOfficeCodeLookups(EuOfficeCode officeCode) : base(officeCode)
		{
		}

		public override CodeDescriptionPairList CY_CodeList => Factory.GetCachedValue<OfficeCodes_ICS>();
	}
}
