using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;

namespace Enterprise.Client.EDI.Billing
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
	public class NZCustomsBill : TransactionalWithSubCodeSystemBill
	{
		public NZCustomsBill(BusinessObjectFactory factory)
			: base(BillingConstants.BillingSystem.NZCustoms, factory)
		{
		}

		#region Charge Codes

		public override ZString GetAmountChargeCodeName(SystemUsage usage)
		{
			return GetChargeCodeName((NZCustomsUsage)usage);
		}

		ZString JobChargeCodeName
		{
			get { return EDIDataRegistry.Instance.NZCustomsJobChargeCode.Value; }
		}

		ZString MessageChargeCodeName
		{
			get { return EDIDataRegistry.Instance.NZCustomsMessageChargeCode.Value; }
		}

		#endregion

		#region Summary Sections

		protected override IEnumerable<Predicate<PriceItemUsage>> GetGeneralSummarySectionsUsageFilters()
		{
			yield return (u => ((NZCustomsUsage)u).IsJobUsage);
			yield return (u => ((NZCustomsUsage)u).IsMessageUsage);
		}

		#endregion

		#region Invoice

		ZString GetChargeCodeName(NZCustomsUsage usage)
		{
			if (usage.IsJobUsage) { return JobChargeCodeName; }
			if (usage.IsMessageUsage) { return MessageChargeCodeName; }
			else
			{
				return base.GetAmountChargeCodeName(usage);
			}
		}

		protected override ZString GetInvoiceDescription(PriceItemUsage systemUsage, int unitCount)
		{
			var usage = systemUsage as NZCustomsUsage;
			return ZString.Format("{0} {1} at {2} {3} per {4}",
				unitCount,
				usage.IsMessageUsage ? "NZ Customs Messages" : "NZ Customs CUS" + usage.SubCode + " Jobs",
				usage.CurrencyCode,
				usage.PriceItem.L7_Price.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture),
				usage.IsMessageUsage ? "Message" : "Job");
		}

		#endregion
	}
}

