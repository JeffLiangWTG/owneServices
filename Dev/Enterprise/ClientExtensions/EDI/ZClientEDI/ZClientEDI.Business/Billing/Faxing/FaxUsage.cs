using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing.Fax
{
	public class FaxUsage : PriceItemUsage
	{
		public FaxUsage(BusinessObjectFactory factory, IUsingParty user, ZDateTime periodStart)
			: base(factory, user, periodStart, BillingConstants.BillingSystem.Fax, true)
		{
			SummaryHeaderDescription = "Faxing Service";
			UnitCountDescription = "Pages";
		}

		#region Properties

		public ZInt PageCount
		{
			get { return TransactionCount; }
			set { TransactionCount = value; }
		}

		public ZDecimal PageRate
		{
			get { return GetFaxPrices(CurrencyCode); }
		}

		public override ZDecimal UnitPrice
		{
			get { return PageRate; }
		}

		public override ZString CurrencyCode
		{
			get { return InvoiceDelivery != null ? InvoiceDelivery.L9_RX_NKInvoiceCurrency : ZString.Empty; }
		}

		public bool IsExchangeRatePricing
		{
			get { return PriceItem != null; }
		}

		#endregion

		#region Method

		ZDecimal GetFaxPrices(ZString currencyCode)
		{
			ZDecimal result = ZDecimal.Zero;
			if (!currencyCode.IsEmpty)
			{
				if (IsExchangeRatePricing)
				{
					ClientFaxPrice faxPrice = MonthlyFaxPrice(currencyCode);
					if (faxPrice != null)
					{
						result = faxPrice.CFP_PageRate;
					}
				}
				else
				{
					var prices = EDIDataRegistry.Instance.FaxPrices;
					if (prices != null)
					{
						var faxPrice = prices.Cast<FaxPrice>().FirstOrDefault(s => s.Code == currencyCode);
						if (faxPrice != null)
						{
							result = faxPrice.Price;
						}
					}
				}
			}
			return result;
		}

		protected override bool PriceItemIsCorrectFeeType(ClientLicencePriceItem priceItem)
		{
			return priceItem != null && priceItem.IsPerPage;
		}

		ClientFaxPrice MonthlyFaxPrice(ZString currencyCode)
		{
			ZQuery query = new ZQuery(ClientFaxPriceSchema.CFP_Month, (ZByte)PeriodStart.Month);
			query.AddToFilter(ClientFaxPriceSchema.CFP_Year, (ZShort)PeriodStart.Year);
			query.AddToFilter(ClientFaxPriceSchema.CFP_RX_NKCurrencyCode, currencyCode);
			return Factory.LoadTop1<ClientFaxPrice>(query);
		}

		#endregion
	}
}

