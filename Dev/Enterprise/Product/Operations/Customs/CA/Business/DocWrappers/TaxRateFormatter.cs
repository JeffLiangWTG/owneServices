using CargoWise.Types;
using Enterprise.Customs.Common.CA;

namespace Enterprise.Customs.CA.Business
{
	public static class TaxRateFormatter
	{
		public static ZString Format(ZDecimal rate, ZString rateType)
		{
			if (rate.IsEmpty)
			{
				return ZString.Empty;
			}
			switch (rateType)
			{
				case RateTypes.Codes.AdValorem:
					return rate.ToString("#,###.0");
				default:
					return rate.ToString("#,###.00###");
			}
		}
	}
}
