using CargoWise.Types;
using Enterprise.Customs.Common.KR;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.KR.Business
{
	public static class ExchangeRateTypeDecider
	{
		public static ExchangeRateType GetExchangeRateType(ZString messageType)
		{
			var result = ExchangeRateType.Customs;
			switch (messageType)
			{
				case KRJobMessageTypeList.Codes.Export:
				case KRJobMessageTypeList.Codes.LocalExport:
					result = ExchangeRateType.CustomsSecondary;
					break;
			}
			return result;
		}
	}
}
