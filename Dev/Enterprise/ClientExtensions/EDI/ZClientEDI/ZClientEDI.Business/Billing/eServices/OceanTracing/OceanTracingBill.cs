using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;

namespace Enterprise.Client.EDI.Billing
{
	public class OceanTracingBill : TransactionSystemBillEx
	{
		public OceanTracingBill(ZString systemCode, BusinessObjectFactory factory)
			: base(systemCode, factory)
		{
		}

		protected override ZString GetInvoiceDescription(PriceItemUsage systemUsage, int unitCount)
		{
			return ZString.Format("{0} - {1} Transactions at {2} {3} per Transaction",
					systemUsage.HasPriceItem ? systemUsage.PriceItem.L7_DescriptionLocalized.Trim() : SystemDescription,
					unitCount,
					systemUsage.CurrencyCode,
					systemUsage.PriceItem.L7_Price.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture));
		}
	}
}

