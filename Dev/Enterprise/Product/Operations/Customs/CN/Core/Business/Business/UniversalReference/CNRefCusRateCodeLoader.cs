using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.CN.Business
{
	public static class CNRefCusRateCodeLoader
	{
		public static IEnumerable<ZString> GetRateCodesByRateType(BusinessObjectFactory factory, ZString rateType)
		{
			return factory.GetCachedValue("CNGetRateCodesByRateType_" + rateType, () =>
			{
				return CusRefRateCodeView.Loader.LoadByRateType(factory, Core.Constants.CountryCodes.China, rateType).Select(x => x.ZY1_RateCode);
			});
		}
	}
}
