using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;

namespace Enterprise.Client.EDI.Billing
{
	public class PremiumServiceUsage : PriceItemUsage
	{
		public PremiumServiceUsage(BusinessObjectFactory factory, IUsingParty user, ZDateTime periodStart, ClientPremiumService[] services)
			: base(factory, user, periodStart, "", false)
		{
			this.services = services;
			UnitCountDescription = "Units";
			FeeTypeUnitCount = 1;
			serviceUnits = services.Sum(x => x.CPS_Units);
		}

		readonly ClientPremiumService[] services;
		readonly int serviceUnits;

		public override ZString SystemCode
		{
			get { return BillingConstants.BillingSystem.Service; }
		}

		public override ClientLicencePriceHeader PriceHeader
		{
			get
			{
				if (UseGlobalPrice)
				{
					ClientLicencePriceHeader priceHeader = null;
					var standardPricesCompany = LicenceCompany.StandardPricesCompany;
					if (standardPricesCompany != null)
					{
						var midMonth = new DateTime(PeriodStart.Year, PeriodStart.Month, 15);
						priceHeader = standardPricesCompany.PriceHeaderForDate(midMonth, services[0].CPS_PriceHeaderCode);
					}
					return priceHeader;
				}
				else
				{
					return base.PriceHeader;
				}
			}
		}

		public override ZString PriceItemCode
		{
			get { return services != null && services.Length > 0 ? services[0].CPS_Type : ZString.Empty; }
		}

		internal ZString PriceItemCodeForTest
		{
			get { return PriceItemCode; }
		}

		public override ZInt UnitCount
		{
			get { return serviceUnits * FeeTypeUnitCount; }
		}

		public override ZDecimal UnitPrice
		{
			get
			{
				ZDecimal price = 0m;

				if (UseGlobalPrice)
				{
					var mainPriceHeader = base.PriceHeader;
					var globalPriceHeader = PriceHeader;
					if (mainPriceHeader != null && globalPriceHeader != null)
					{
						var item = PriceItem;
						if (item != null)
						{
							var rate = PriceItem.CurrencyRates.FirstOrDefault(x => x.PIR_RX_NKCurrency == mainPriceHeader.L6_RX_NKCurrency);
							if (rate != null)
							{
								price = rate.PIR_Price;
							}
							else if ((!item.L7_RX_NKCurrency.IsEmpty && item.L7_RX_NKCurrency == mainPriceHeader.L6_RX_NKCurrency)
								|| (item.L7_RX_NKCurrency.IsEmpty && mainPriceHeader.L6_RX_NKCurrency == globalPriceHeader.L6_RX_NKCurrency))
							{
								price = item.L7_Price;
							}
						}
					}
				}
				else
				{
					price = base.UnitPrice;
				}
				return price;
			}
		}

		public override ZString CurrencyCode
		{
			get
			{
				if (UseGlobalPrice)
				{
					return base.PriceHeader?.L6_RX_NKCurrency ?? "";
				}
				else
				{
					return base.CurrencyCode;
				}
			}
		}

		public bool UseGlobalPrice
		{
			get { return services.Length > 0 && !services[0].CPS_PriceHeaderCode.IsEmpty; }
		}

		/// <summary>
		/// The unit count of the fee type. For example, if the fee type is CoreUsers then this will be the number of core users.
		/// For fee types like PerDevice this will be 1.
		/// </summary>
		public ZInt FeeTypeUnitCount { get; set; }

		public ZGuid DatabasePK
		{
			get { return services[0].CPS_LD; }
		}

		protected override ClientInvoiceDelivery GetInvoiceDeliveryCore()
		{
			if (UseGlobalPrice)
			{
				return LicCompany != null ? LicCompany.InvoiceDeliveries.FindByServerAndSystem(ServerCode, BillingConstants.PriceHeaderType.LDaaS) : null;
			}
			else
			{
				return base.GetInvoiceDeliveryCore();
			}
		}

		#region Service

		public IEnumerable<ClientPremiumService> Services
		{
			get { return services; }
		}

		#endregion

		#region Summary Sections

		public override SummarySection[] GetGeneralSummarySections()
		{
			var result = BuildGeneralSummarySectionWithPriceItemDescription();
			return new SummarySection[] { result };
		}

		#endregion
	}
}

