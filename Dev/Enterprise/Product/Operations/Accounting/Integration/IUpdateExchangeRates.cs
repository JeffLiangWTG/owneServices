using System;
using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Integration
{
	public interface IUpdateExchangeRates
	{
		void UpdateExchangeRates(BusinessObject target, Converter<BusinessObject, IExchangeRateSource> sourceProvider, object actionLog);
	}
}
