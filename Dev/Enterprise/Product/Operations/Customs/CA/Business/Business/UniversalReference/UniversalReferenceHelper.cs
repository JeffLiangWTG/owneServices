using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CA.Business
{
	class UniversalReferenceHelper
	{
		public static CodeDescriptionPairList GetRefCusRateCodePairList(BusinessObjectFactory factory, ZString rateType, ZDateTime effectiveDate, string tariffCode = null, bool isAddNO = true)
		{
			return factory.GetCachedValue(string.Format("RefCusRateCode_{0}_{1}_{2}", rateType, effectiveDate.ToISO8601ShortDateString(), tariffCode ?? ZString.Empty), () =>
			{
				var result = new CodeDescriptionPairList();
				var rateCodes = RefCusRateCode.Loader.LoadByRateType(factory, Core.Constants.CountryCodes.Canada, rateType, SQLComparisonOperator.Equal);
				rateCodes.ForEach(x =>
				{
					result.AddPairIfNotExist(x.ZY1_RateCode, x.ZY1_Description);
				});

				if (rateType == DutyAndTaxTypes.Codes.ExciseTax)
				{
					var tariffView = new TariffView.Loader(factory).LoadMostRecentCachedTariff(Core.Constants.CountryCodes.Canada, Constants.TariffTypes.HarmonizedSystem, tariffCode ?? ZString.Empty, effectiveDate);
					if (tariffView != null)
					{
						var exciseTaxRates = GetExciseTaxRates(factory, effectiveDate, tariffCode ?? ZString.Empty);
						if (exciseTaxRates.Any())
						{
							var newResult = new CodeDescriptionPairList();
							foreach (CodeDescriptionPair pair in result)
							{
								if (exciseTaxRates.Any(x => x.RateCode == pair.Code))
								{
									newResult.AddPair(pair.Code, pair.Description);
								}
							}
							result = newResult;
						}
					}

					if (isAddNO)
					{
						result.AddPair(DutyAndTaxTypes.Constant.NO, Res.GetString("dc366399-619b-4e39-9e00-1a37de700a50", "EXCISE TAX IS NOT APPLIABLE TO THIS LINE"));
					}
				}

				result.Sort();
				return result;
			});
		}

		public static IEnumerable<RateView> GetExciseTaxRates(BusinessObjectFactory factory, ZDateTime effectiveDate, string tariffCode)
		{
			return factory.GetCachedValue(string.Format("CAExciseTaxRates_{0}_{1}", effectiveDate.ToISO8601ShortDateString(), tariffCode), () =>
			{
				var tariffView = new TariffView.Loader(factory).LoadMostRecentCachedTariff(Core.Constants.CountryCodes.Canada, Constants.TariffTypes.HarmonizedSystem, tariffCode, effectiveDate);
				if (tariffView != null)
				{
					return tariffView.Rates.Where(d => d.ZZ2_StartDate <= effectiveDate && d.ZZ2_EndDate >= effectiveDate && d.ZZ2_ZZR_RateTypeCode == DutyAndTaxTypes.Codes.ExciseTax);
				}
				return Enumerable.Empty<RateView>();
			});
		}
	}
}
