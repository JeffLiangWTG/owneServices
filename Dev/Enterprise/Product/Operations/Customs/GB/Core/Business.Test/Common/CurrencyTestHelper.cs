using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Business.Testing
{
	public class CurrencyTestHelper
	{
		public CurrencyTestHelper(BusinessObjectFactory factory)
		{
			Factory = factory;
		}

		protected readonly BusinessObjectFactory Factory;

		public void SetExchangeRate(string currency, ZDecimal rate, ZDateTime effectiveDate)
		{
			var refCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, currency);
			var filter = new ZQuery(RefExchangeRateSchema.RE_RX_NKExCurrency, SQLComparisonOperator.Equal, refCurrency.RX_Code);
			filter.AddToFilter(JoinCondition.And, RefExchangeRateSchema.RE_GC, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.PK);
			filter.AddToFilter(JoinCondition.And, RefExchangeRateSchema.RE_StartDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, effectiveDate);
			filter.AddToFilter(JoinCondition.And, RefExchangeRateSchema.RE_ExpiryDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, effectiveDate);
			filter.AddToFilter(JoinCondition.And, RefExchangeRateSchema.RE_ExRateType, SQLComparisonOperator.Equal, "CUS");

			var exchangeRate = Factory.LoadTop1<RefExchangeRate>(filter);
			if (exchangeRate == null)
			{
				exchangeRate = Factory.New<RefExchangeRate>();
				exchangeRate.RE_RX_NKExCurrency = refCurrency.RX_Code;
				exchangeRate.RE_GC = GlbCompany.CurrentCompany.PK;
				exchangeRate.RE_StartDate = effectiveDate;
				exchangeRate.RE_ExpiryDate = effectiveDate.AddDays(1);
				exchangeRate.RE_ExRateType = "CUS";
			}
			exchangeRate.RE_SellRate = rate;
		}
	}
}
