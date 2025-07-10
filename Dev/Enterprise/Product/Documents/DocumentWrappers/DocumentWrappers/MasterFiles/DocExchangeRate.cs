using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;

namespace Enterprise.DocumentWrappers
{
	public class DocExchangeRate : DocBaseWrapper
	{
		public static DocExchangeRate New(ExchangeRate exchangeRate, BusinessObjectFactory factory)
		{
			return exchangeRate == null ? null : new DocExchangeRate(exchangeRate, factory);
		}

		DocExchangeRate(ExchangeRate exchangeRate, BusinessObjectFactory factory)
			: base(exchangeRate, factory) { }

		public DocCurrency Currency
		{
			get { return DocCurrency.New(Factory, Rate.RateCurrency); }
		}

		public ZDecimal BuyRate
		{
			get { return Rate.JF_BaseRate; }
		}
		public ZDecimal LocalClientSellRate
		{
			get { return Rate.JF_SellRate; }
		}
		public ZDecimal AgentsSellRate
		{
			get
			{
				return Rate.JF_SellRate;
			}
		}

		public ZString OrgType => Rate.JF_OrgType.IsEmpty ? new ZString("ALL") : Rate.JF_OrgType;

		#region Implementation

		ExchangeRate Rate
		{
			get { return (ExchangeRate)WrappedObject; }
		}

		#endregion
	}
}
