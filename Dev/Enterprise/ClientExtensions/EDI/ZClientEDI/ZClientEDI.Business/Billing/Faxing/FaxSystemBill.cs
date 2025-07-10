using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Client.EDI.Billing.Business;

namespace Enterprise.Client.EDI.Billing.Fax
{
	public class FaxSystemBill : TransactionalSystemBill
	{
		public FaxSystemBill(BusinessObjectFactory factory)
			: base(BillingConstants.BillingSystem.Fax, factory)
		{
		}

		protected override void AddAllDiscounts(List<ClientLicenceBillingDiscount> discountsListToAddTo, SystemUsage firstUsage, ClientLicenceBillingDiscountCollection localDiscounts)
		{
			// discounts not applicable for fax system
		}

		protected override void CreateInvoiceLinesCore(List<BillLine> lines, ZDateTime dateForExchangeRate, ARInvoice invoice)
		{
			foreach (var usageTaxGroup in SystemUsages.GroupBy(x => new TaxGroup(x.InvoiceDelivery)))
			{
				int groupPageCount = usageTaxGroup.Sum(x => x.UnitCount);
				ZDecimal groupAmount = usageTaxGroup.Sum(x => x.Amount);
				string lineDescription = GetInvoiceLineDescription(usageTaxGroup.First(), groupPageCount);

				lines.Add(new BillLine(groupAmount, usageTaxGroup.Key, CurrencyCode, GetAmountChargeCodeName(usageTaxGroup.First()), lineDescription, lines.Count, SystemCode));
			}
		}

		string GetInvoiceLineDescription(SystemUsage usage, int groupPageCount)
		{
			bool isExchangeRatePricing = false;
			FaxUsage faxUsage = usage as FaxUsage;
			if (faxUsage != null)
			{
				isExchangeRatePricing = faxUsage.IsExchangeRatePricing;
			}

			return "Faxing Service\r\n"
				+ (isExchangeRatePricing ? "USD 0.16 is converted to your invoice currency at the xe.com rate on the 1st of the billing month\r\n" : "")
				+ groupPageCount.ToString(CultureInfo.InvariantCulture) + " @ " + usage.CurrencyCode + " " + usage.UnitPrice.ToString("F2", CultureInfo.InvariantCulture) + " per page";
		}

		#region Validation

		public const string NoPrice = "No fax page rate is defined for the invoice currency.";

		protected override void ValidateUnitPrice(BusinessObject notificationOwner)
		{
			if (SystemUsages.Count > 0)
			{
				var usage = (FaxUsage)SystemUsages[0];
				if (usage.PageRate == 0m)
				{
					notificationOwner.AddRowError(NoPrice);
				}
			}
		}

		#endregion
	}
}

