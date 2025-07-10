using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business
{
	public class CompletionCustomsOfficeLookups : CusCodeDataLookups
	{
		public CompletionCustomsOfficeLookups(CompletionCustomsOffice parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList CY_CodeList => Factory.GetCachedValue("CompletionCustomsOfficeLookups|CY_CodeList", () =>
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, Res.GetString("0752FD14-B64F-4DF9-BE59-2F440C957A25", "Customs Office"));
			return result;
		});

		public CustomsOfficeCodeCollection OfficeCodeList
		{
			get
			{
				var customsOfficeCodes = CustomsOfficeCodeCollection.LocalCountryOnlyCustomsOfficesWithRequiredRoles(Factory, Core.Constants.CountryCodes.Germany, ZString.Empty);
				customsOfficeCodes.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Universal.Constants.ZZRefCusCodeListFilters.CountryOrGrouping, "Property", new ZString(Core.Constants.CountryCodes.Germany)));
				return customsOfficeCodes;
			}
		}
	}
}
