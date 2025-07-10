using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Client.EDI.Billing.Business;

namespace Enterprise.Client.EDI.Billing
{
	[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
	public class PortMessagingBill : TransactionalWithSubCodeSystemBill
	{
		public PortMessagingBill(BusinessObjectFactory factory)
			: base(BillingConstants.BillingSystem.PortMessaging, factory)
		{
		}

		#region Charge Codes

		public override ZString GetAmountChargeCodeName(SystemUsage usage)
		{
			return GetChargeCodeName((PortMessagingUsage)usage);
		}

		internal ZString GetChargeCodeName(PortMessagingUsage usage)
		{
			var result = EDIDataRegistry.Instance.TransactionChargeCodes.Value.GetDescriptionFromCode(usage.SubCode);
			if (string.IsNullOrEmpty(result))
			{
				result = base.GetAmountChargeCodeName(usage);
			}
			return result;
		}

		public override ZString GetDiscountChargeCodeName(SystemUsage usage)
		{
			var result = usage != null ? EDIDataRegistry.Instance.TransactionDiscountChargeCodes.Value.GetDescriptionFromCode(usage.SubCode) : null;
			if (string.IsNullOrEmpty(result))
			{
				result = base.GetDiscountChargeCodeName(usage);
			}
			return result;
		}

		#endregion

		#region Summary Section

		protected override PriceItemUsage[] GetGeneralSummarySectionsUsages(ZGuid organisationPK)
		{
			var result = base.GetGeneralSummarySectionsUsages(organisationPK);
			Array.Sort(result, new PortMessagingUsageComparer());
			return result;
		}

		#endregion

		#region Invoice

		[SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
		protected override void CreateInvoiceLinesCore(List<BillLine> lines, ZDateTime dateForExchangeRate, ARInvoice invoice)
		{
			SystemUsages.Sort(new PortMessagingUsageComparer());
			base.CreateInvoiceLinesCore(lines, dateForExchangeRate, invoice);
		}

		protected override ZString GetInvoiceDescription(PriceItemUsage systemUsage, int unitCount)
		{
			var usage = systemUsage as PortMessagingUsage;
			var caption = PortMessagingBillingSystem.GetUsageCaptions().FirstOrDefault(x => string.Equals(x.UsageCode, usage.SubCode, StringComparison.OrdinalIgnoreCase));
			var desc = caption?.UsageDescription ?? usage.SubCode;
			return ZString.Format("Forwarding Port Messaging - {0} {1} at {2} {3} per Transaction",
				unitCount,
				desc,
				usage.CurrencyCode,
				usage.PriceItem.L7_Price.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture));
		}

		#endregion

		class PortMessagingUsageComparer : IComparer<SystemUsage>
		{
			public int Compare(SystemUsage x, SystemUsage y)
			{
				return x.SubCode.CompareTo(y.SubCode);
			}
		}
	}
}

