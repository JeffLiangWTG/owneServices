using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Client.EDI.Billing.Business
{
	/// <summary>
	/// SystemBill for usage corresponding to a single price item on the org price list.
	/// </summary>
	public class PriceItemBill : SystemBill
	{
		public PriceItemBill(ZString systemCode, BusinessObjectFactory factory)
			: this(factory)
		{
			this.SystemCode = systemCode;
		}

		public PriceItemBill(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Summary Sections

		public override SummarySection[] GetGeneralSummarySections(ZGuid organisationPK)
		{
			return BuildGeneralSummarySections(organisationPK, true);
		}

		#endregion

		#region Validation

		protected override void ValidateUnitPrice(BusinessObject notificationOwner)
		{
			if (SystemUsages.Count > 0)
			{
				foreach (var periodGroup in SystemUsages.GroupBy(x => x.PeriodStart))
				{
					ZDecimal expectedUnitPrice = periodGroup.Max(x => x.UnitPrice);
					if (expectedUnitPrice != 0m && periodGroup.Any(x => x.UnitPrice != expectedUnitPrice && x.UnitPrice != 0m))
					{
						notificationOwner.AddRowError(BillingSystemDescription + ": Different unit prices exist in affiliate organisations within the paying entity for period " + periodGroup.First().PeriodStartAsText + ".");
					}
				}

				ValidatePriceNonZero(notificationOwner);
			}
		}

		protected void ValidatePriceNonZero(BusinessObject notificationOwner)
		{
			foreach (SystemUsage systemUsage in SystemUsages)
			{
				PriceItemUsage itemUsage = systemUsage as PriceItemUsage;
				if (itemUsage != null)
				{
					if (!itemUsage.HasPriceItem)
					{
						notificationOwner.AddRowError(string.Format(CultureInfo.InvariantCulture, "{0}: No price with valid fee type found for {1}, price code {2}", BillingSystemDescription, itemUsage.Organisation.OH_Code, itemUsage.PriceItemCode));
					}
					else if (itemUsage.UnitPrice == 0m)
					{
						notificationOwner.AddRowWarning(string.Format(CultureInfo.InvariantCulture, "{0}: Zero price for {1}, price code {2}", BillingSystemDescription, itemUsage.Organisation.OH_Code, itemUsage.PriceItemCode));
					}
				}
			}
		}

		protected void ValidateUnitPricePerSubCode(BusinessObject notificationOwner)
		{
			if (SystemUsages.Count > 0)
			{
				foreach (var periodGroup in SystemUsages.GroupBy(x => x.PeriodStart))
				{
					foreach (var subCodeGroup in periodGroup.GroupBy(x => x.SubCode))
					{
						ZDecimal expectedUnitPrice = subCodeGroup.Max(x => x.UnitPrice);
						if (expectedUnitPrice != 0m && subCodeGroup.Any(x => x.UnitPrice != expectedUnitPrice && x.UnitPrice != 0m))
						{
							notificationOwner.AddRowError(BillingSystemDescription + ", Code " + subCodeGroup.Key + ": Different unit prices exist in affiliate organisations within the paying entity for period " + subCodeGroup.First().PeriodStartAsText + ".");
						}
					}
				}

				ValidatePriceNonZero(notificationOwner);
			}
		}

		#endregion
	}
}

