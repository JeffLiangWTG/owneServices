using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business
{
	public class NationalFeeTypeCodeList : CodeDescriptionPairList
	{
		public NationalFeeTypeCodeList(BusinessObjectFactory factory, TariffView tariff = null, ZDateTime? assessmentDate = null)
		{
			AddRange(GetNationalFeeTypeCodeList(factory, tariff, assessmentDate));
		}

		CodeDescriptionPairList GetNationalFeeTypeCodeList(BusinessObjectFactory factory, TariffView tariff, ZDateTime? assessmentDate)
		{
			var cacheKey = "FR.NationalFeeTypeCodeList";
			if (tariff != null && assessmentDate != null)
			{
				cacheKey = $"{cacheKey}_{tariff.PK}_{assessmentDate}";
			}
			return factory.GetCachedValue(cacheKey, () =>
			{
				var result = new CodeDescriptionPairList();
				result.AddRange(CusRefRateCodeView.Loader.Load(factory, Core.Constants.CountryCodes.France, null, false));
				if (tariff != null && assessmentDate != null)
				{
					var categories = tariff.GetEffectiveVATApplicabilities((ZDateTime)assessmentDate).Select(x => x.ZX5_VATCategory).Distinct().Where(x => !x.IsEmpty);
					foreach (var category in categories)
					{
						result.AddPairIfNotExist(category, category);
					}
				}
				result.Sort();
				return result;
			});
		}
	}
}
