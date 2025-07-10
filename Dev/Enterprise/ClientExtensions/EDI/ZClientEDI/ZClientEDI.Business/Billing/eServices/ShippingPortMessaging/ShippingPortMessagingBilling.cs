using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;

namespace Enterprise.Client.EDI.Billing
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
	public class ShippingPortMessagingBill : TransactionalWithSubCodeSystemBill
	{
		public ShippingPortMessagingBill(BusinessObjectFactory factory)
			: base(BillingConstants.BillingSystem.ShippingPortMessaging, factory)
		{
		}

		#region Invoice

		public override ZString GetAmountChargeCodeName(SystemUsage systemUsage)
		{
			var usage = systemUsage as ShippingPortMessagingUsage;
			return EDIDataRegistry.Instance.TransactionChargeCodes.Value.GetDescriptionFromCode(usage.SubCode);
		}

		protected override ZString GetInvoiceDescription(PriceItemUsage systemUsage, int unitCount)
		{
			var priceItem = systemUsage.PriceItem;
			return ZString.Format("{0} - {1} Transactions at {2} {3} per Transaction",
				priceItem.L7_DescriptionLocalized,
				unitCount,
				systemUsage.CurrencyCode,
				priceItem.L7_Price.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture));
		}

		#endregion
	}
}

