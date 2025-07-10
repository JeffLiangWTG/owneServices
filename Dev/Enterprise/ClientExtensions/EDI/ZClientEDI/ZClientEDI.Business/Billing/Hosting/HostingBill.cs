using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Client.EDI.Billing.Business;

namespace Enterprise.Client.EDI.Billing.Hosting
{
	public class HostingBill : SystemBill, IDiscountable
	{
		public HostingBill(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Charge Codes

		public override ZString GetAmountChargeCodeName(SystemUsage usage)
		{
			return ChargeCodeFromUsage((HostingUsage)usage);
		}

		public override ZString GetDiscountChargeCodeName(SystemUsage usage)
		{
			var result = ((HostingUsage)usage).PriceItem?.L7_DiscountChargeCode ?? "";
			if (result.IsEmpty)
			{
				result = EDIDataRegistry.Instance.HostingDiscountChargeCode.Value;
			}
			return result;
		}

		#endregion

		#region Calculate Group Amounts

		protected override void CalculateGroupAmounts()
		{
			CalculateDiscountsAndSurcharges();

			Amount = DiscountCalculations.Values.Sum(x => x.Amount);
			DiscountAmount = DiscountCalculations.Values.Sum(x => x.DiscountAmount);
			SurchargeAmount = -SurchargeCalculations.Values.Sum(x => x.DiscountAmount);
		}

		void CalculateDiscountsAndSurcharges()
		{
			DiscountCalculations.Clear();
			DiscountCalculationsToSystemUsages.Clear();
			SurchargeCalculations.Clear();
			List<ClientLicenceBillingDiscount> allDiscounts = new List<ClientLicenceBillingDiscount>();
			ClientLicenceBillingDiscountCollection localDiscounts = Discounts;

			foreach (var usagesGroupedByMonth in SystemUsages.GroupBy(x => x.PeriodStart))
			{
				var firstDayOfMonth = usagesGroupedByMonth.Key;
				var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
				var firstUsage = usagesGroupedByMonth.First();
				var prices = firstUsage.PriceHeader;
				allDiscounts.Clear();
				if (prices != null)
				{
					allDiscounts.AddRange(prices.Discounts);
				}
				if (localDiscounts != null)
				{
					allDiscounts.AddRange(localDiscounts);
				}

				amountToDiscount = usagesGroupedByMonth.Sum(x => x.Amount);

				DiscountCalculation discountCalculation = ClientLicenceBillingDiscountCollection.CalculateTransactionalDiscount(allDiscounts, this, 1, amountToDiscount, firstDayOfMonth, lastDayOfMonth);
				DiscountCalculations.Add(firstDayOfMonth, discountCalculation);
				DiscountCalculationsToSystemUsages.Add(discountCalculation, usagesGroupedByMonth.ToArray());

				DiscountCalculation surchargeCalculation = ClientLicenceBillingDiscountCollection.CalculateTransactionalSurcharge(allDiscounts, this, discountCalculation.Amount, firstDayOfMonth, lastDayOfMonth);
				SurchargeCalculations.Add(firstDayOfMonth, surchargeCalculation);
			}
		}

		internal Dictionary<DiscountCalculation, SystemUsage[]> DiscountCalculationsToSystemUsages
		{
			get { return discountCalculationsToSystemUsages ?? (discountCalculationsToSystemUsages = new Dictionary<DiscountCalculation, SystemUsage[]>()); }
		}
		Dictionary<DiscountCalculation, SystemUsage[]> discountCalculationsToSystemUsages;

		protected Dictionary<ZDateTime, DiscountCalculation> DiscountCalculations
		{
			get { return discountCalculations ?? (discountCalculations = new Dictionary<ZDateTime, DiscountCalculation>()); }
		}
		Dictionary<ZDateTime, DiscountCalculation> discountCalculations;

		protected Dictionary<ZDateTime, DiscountCalculation> SurchargeCalculations
		{
			get { return surchargeCalculations ?? (surchargeCalculations = new Dictionary<ZDateTime, DiscountCalculation>()); }
		}
		Dictionary<ZDateTime, DiscountCalculation> surchargeCalculations;

		protected override IEnumerable<DiscountDetailedInfo> DiscountDetailedInfos => DiscountCalculations.Values.SelectMany(x => x.Details);
		protected override IEnumerable<DiscountDetailedInfo> SurchargeDetailedInfos => SurchargeCalculations.Values.SelectMany(x => x.Details);

		#endregion

		#region IDiscountable Members

		ZString IDiscountable.SystemCode
		{
			get { return SystemCode; }
		}

		ZDecimal IDiscountable.AmountToDiscount
		{
			get { return amountToDiscount; }
		}
		ZDecimal amountToDiscount;

		#endregion

		protected override void CreateInvoiceLinesCore(List<BillLine> lines, ZDateTime dateForExchangeRate, ARInvoice invoice)
		{
			foreach (HostingUsage usage in SystemUsages)
			{
				string lineDescription = usage.GetInvoiceLineDescription();

				lines.Add(new BillLine(usage.Amount, new TaxGroup(usage.InvoiceDelivery), CurrencyCode, ChargeCodeFromUsage(usage), lineDescription, lines.Count, SystemCode));
			}

			if (DiscountAmount != 0)
			{
				var lastLine = lines[lines.Count - 1];
				lines.Add(new BillLine(-DiscountAmount, lastLine.Tax, lastLine.CurrencyCode, EDIDataRegistry.Instance.HostingDiscountChargeCode.Value, BillingSystemDescription + " Discount", lines.Count, SystemCode, ZGuid.Empty));
			}

			if (SurchargeAmount != 0)
			{
				var lastLine = lines[lines.Count - 1];
				lines.Add(new BillLine(SurchargeAmount, lastLine.Tax, lastLine.CurrencyCode, EDIDataRegistry.Instance.HostingDiscountChargeCode.Value, BillingSystemDescription + " Surcharge", lines.Count, SystemCode, ZGuid.Empty));
			}
		}

		internal static ZString ChargeCodeFromUsage(HostingUsage usage)
		{
			var result = ZString.Empty;

			if (usage.SystemCode == BillingConstants.BillingSystem.HostingStorage)
			{
				switch (usage.SubCode)
				{
					case BillingConstants.Hosting.DataStorageCode:
						result = EDIDataRegistry.Instance.HostingDataStorageChargeCode.Value;
						break;

					case BillingConstants.Hosting.eDocsStorageCode:
						result = EDIDataRegistry.Instance.HostingDocsStorageChargeCode.Value;
						break;

					case BillingConstants.Hosting.UltraFastStorageCode:
						result = EDIDataRegistry.Instance.HostingUltraFastStorageChargeCode.Value;
						break;

					case BillingConstants.Hosting.NonProductionStorageCode:
						result = EDIDataRegistry.Instance.HostingNonProductionStorageChargeCode.Value;
						break;

					default:
						break;
				}
			}
			else if (usage.SystemCode == BillingConstants.BillingSystem.HostingRemoteDevices)
			{
				switch (usage.SubCode)
				{
					case BillingConstants.Hosting.RemoteDevicesCode:
						result = EDIDataRegistry.Instance.HostingRemoteDevicesChargeCode.Value;
						break;

					case BillingConstants.Hosting.PrintServersCode:
						result = EDIDataRegistry.Instance.HostingPrintServersChargeCode.Value;
						break;

					default:
						break;
				}
			}
			else
			{
				result = usage.PriceItem?.L7_ChargeCode ?? "";
			}

			return result;
		}

		#region Validation

		protected override void ValidateUnitPrice(BusinessObject notificationOwner)
		{
			foreach (HostingUsage usage in SystemUsages)
			{
				if (usage.PriceItem == null)
				{
					notificationOwner.AddRowError(string.Format(CultureInfo.InvariantCulture, "{0}: No price found for org {1} for usage code {2}", SystemDescription, usage.Organisation.OH_Code, usage.SubCode));
				}
				else if ((
						SystemCode == BillingConstants.BillingSystem.HostingStorage
						&& usage.PriceItem.L7_FeeType != BillingConstants.FeeType.Per10GBPerMonthMin1GB
						&& usage.PriceItem.L7_FeeType != BillingConstants.FeeType.PerGBPerMonth
					) || (
						SystemCode == BillingConstants.BillingSystem.HostingRemoteDevices
						&& usage.PriceItem.L7_FeeType != BillingConstants.FeeType.PerDevicePerMonth
					))
				{
					notificationOwner.AddRowError(string.Format(CultureInfo.InvariantCulture, "{0}: Invalid fee basis for org {1}, usage code {2}: {3}", SystemDescription, usage.Organisation.OH_Code, usage.SubCode, usage.PriceItem.L7_FeeType));
				}
			}

			foreach (var usageGroup in SystemUsages.Cast<HostingUsage>().GroupBy(x => x.SubCode))
			{
				var usage = usageGroup.First();
				if (ChargeCodeFromUsage(usage).IsEmpty)
				{
					notificationOwner.AddRowError(string.Format(CultureInfo.InvariantCulture, "{0}: No charge code found for price code {1}", SystemDescription, usage.SubCode));
				}
			}
		}

		protected override void ValidateAllCore(BusinessObject notificationOwner)
		{
			base.ValidateAllCore(notificationOwner);

			foreach (HostingUsage usage in SystemUsages)
			{
				var ownerLicence = usage.User.UsageOwnerLicence;
				if (ownerLicence != null && ownerLicence.LA_AgreedLiveDate.IsEmpty)
				{
					notificationOwner.AddRowWarning(string.Format(CultureInfo.InvariantCulture, "Blank site-live usage from {0} (Server {1}) is included.",
						ownerLicence.Company.Header.OH_Code,
						usage.User.ServerCode));
				}
			}
		}

		#endregion

		#region Summary Sections

		public override SummarySection[] GetGroupSummarySections()
		{
			return System.Array.Empty<SummarySection>();
		}

		public override SummarySection[] GetGeneralSummarySections(ZGuid organisationPK)
		{
			return System.Array.Empty<SummarySection>();
		}

		public override SummarySection[] GetDiscountSummarySections()
		{
			return GetMultiPeriodDiscountSummarySections(DiscountCalculations);
		}

		public override SummarySection[] GetSurchargeSummarySections()
		{
			return GetMultiPeriodDiscountSummarySections(SurchargeCalculations);
		}

		#endregion
	}
}

