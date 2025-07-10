using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class UniversalPriceSystemUsage : PriceItemUsage
	{
		public UniversalPriceSystemUsage(
			BusinessObjectFactory factory,
			string systemCode,
			string priceHeaderCode,
			IUsingParty user,
			ZDateTime periodStart,
			string subCode = "",
			int unitCount = 0,
			string unitCountDescription = "Units",
			string currencyCode = "",
			int includedUnitCount = 0,
			string includedUnitCountDescription = "",
			string totalUnitCountDescription = "",
			bool isTransactional = false)
			: base(factory, user, periodStart, systemCode, isTransactional)
		{
			this.systemCode = systemCode;
			this.currencyCode = currencyCode;
			PriceHeaderCode = priceHeaderCode;
			UnitCountDescription = unitCountDescription;
			SubCode = subCode;
			TotalUnitCount = unitCount;
			this.includedUnitCount = includedUnitCount;
			IncludedUnitCountDescription = includedUnitCountDescription;
			TotalUnitCountDescription = totalUnitCountDescription;
		}

		public override ZDecimal UnitPrice
		{
			get
			{
				if (!isUnitPriceCalculated)
				{
					CalculateUnitPriceAndCurrency();
				}
				return unitPrice;
			}
		}
		ZDecimal unitPrice;
		bool isUnitPriceCalculated;

		public override ZInt IncludedUnitCount => includedUnitCount;
		int includedUnitCount;

		public void AddIncludedUnitCount(int n) => includedUnitCount += n;

		void CalculateUnitPriceAndCurrency()
		{
			isUnitPriceCalculated = true;
			unitPrice = 0m;

			var item = PriceItem;
			if (item != null)
			{
				if (currencyCode.IsEmpty)
				{
					if (!item.L7_RX_NKCurrency.IsEmpty)
					{
						currencyCode = item.L7_RX_NKCurrency;
						unitPrice = item.L7_Price;
					}
					else if (item.CurrencyRates.Count == 0 && !item.Parent.L6_HasExchangeRates)
					{
						currencyCode = item.Parent.L6_RX_NKCurrency;
						unitPrice = item.L7_Price;
					}
				}
				else if (item.L7_RX_NKCurrency == currencyCode)
				{
					unitPrice = item.L7_Price;
				}
				else if (item.L7_RX_NKCurrency.IsEmpty)
				{
					var rate = item.CurrencyRates.FirstOrDefault(x => x.PIR_RX_NKCurrency == currencyCode);
					if (rate != null)
					{
						unitPrice = rate.PIR_Price;
					}
					else if (item.Parent.L6_RX_NKCurrency == currencyCode)
					{
						unitPrice = item.L7_Price;
					}
				}
			}
		}

		public override ZString CurrencyCode
		{
			get
			{
				if (currencyCode.IsEmpty && !isUnitPriceCalculated)
				{
					CalculateUnitPriceAndCurrency();
				}
				return currencyCode;
			}
		}
		ZString currencyCode;

		public void ApplyCurrencyCode(string currency)
		{
			if (currencyCode != currency)
			{
				currencyCode = currency;
				isUnitPriceCalculated = false;
			}
		}

		#region System Code

		public override ZString SystemCode => systemCode;
		readonly ZString systemCode;

		#endregion

		public override ZString PriceItemCode => SubCode.IsEmpty ? SystemCode : SubCode;

		public override void CalculateAmount()
		{
			base.CalculateAmount();

			if (HasLicenceUnits && HasPriceItem)
			{
				LicenceUnitsAmount = Utilities.Round(UnitCount * PriceItemLicenceUnits, 1);
			}
		}

		public override SummarySection[] GetGeneralSummarySections()
		{
			var section = BuildGeneralSummarySectionWithPriceItemDescription(includeLicenceUnits: true);
			return new[] { section };
		}

		public override ClientLicencePriceHeader PriceHeader
		{
			get
			{
				if (priceHeader == null)
				{
					var standardPricesCompany = LicenceCompany.StandardPricesCompany;
					if (standardPricesCompany != null)
					{
						var midMonth = new DateTime(PeriodStart.Year, PeriodStart.Month, 15);
						priceHeader = standardPricesCompany.PriceHeaderForDate(midMonth, PriceHeaderCode);
					}
				}

				return priceHeader;
			}
		}

		ClientLicencePriceHeader priceHeader;
	}
}

