using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[WrapperTypeName("ExchangeRate")]
	public abstract class ExchangeRateWrapper : GenericWrapper
	{
		protected ExchangeRateWrapper(BusinessObject businessObjectToWrap, BusinessObjectFactory factory)
			: base(businessObjectToWrap, factory)
		{
		}

		#region Wrapper Properties

		public CurrencyWrapper Currency
		{
			get { return currency ?? (currency = GetCurrency()); }
		}

		#endregion

		#region IZType Properties

		public ZDecimal BuyRate
		{
			get { return GetBuyRate(); }
		}

		public ZDecimal SellRate
		{
			get { return GetSellRate(); }
		}

		public ZDecimal SellRateAgent
		{
			get { return GetSellRateAgent(); }
		}

		#endregion

		#region Implementation

		CurrencyWrapper currency;

		protected abstract CurrencyWrapper GetCurrency();

		protected abstract ZDecimal GetBuyRate();

		protected abstract ZDecimal GetSellRate();

		protected abstract ZDecimal GetSellRateAgent();

		#endregion
	}
}
