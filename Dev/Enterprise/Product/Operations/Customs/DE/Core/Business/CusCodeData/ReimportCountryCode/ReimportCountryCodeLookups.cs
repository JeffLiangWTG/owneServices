using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business
{
	public class ReimportCountryCodeLookups : CusCodeDataLookups
	{
		public ReimportCountryCodeLookups(ReimportCountryCode officeCode)
			: base(officeCode)
		{
		}

		public override CodeDescriptionPairList CY_CodeList
		{
			get
			{
				return Factory.GetCachedValue("ReimportCountryCodeLookups.CY_CodeList", () =>
				{
					var result = new CodeDescriptionPairList();
					var refCodes = ZZRefCusCodeListCombined.Loader.Load(Factory, Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.DEReimportCountry, ZDate.Today);

					refCodes.OrderBy(x => x.ZZD_Code).ForEach(combined => result.AddPair(combined.ZZD_Code, combined.ZZD_Description));
					return result;
				});
			}
		}
	}
}
