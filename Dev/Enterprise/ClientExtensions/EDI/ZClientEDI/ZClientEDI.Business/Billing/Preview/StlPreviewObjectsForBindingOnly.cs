using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Client.EDI.Billing.Business.Preview
{
	public class StlBillForBindingOnly : NonPersistentBusinessObject, IObsoleteValidation
	{
		public StlBillForBindingOnly(StlBill stlBill)
		{
			if (stlBill != null)
			{
				foreach (StlMonthlyUsage monthlyUsage in stlBill.MonthlyUsagesForBindingOnly)
				{
					MonthlyUsagesForBindingOnly.Add(new StlMonthlyUsageForBindingOnly(monthlyUsage));
				}
			}
		}

		public StlMonthlyUsageForBindingOnlyCollection MonthlyUsagesForBindingOnly
		{
			get { return monthlyUsages ?? (monthlyUsages = new StlMonthlyUsageForBindingOnlyCollection()); }
		}
		StlMonthlyUsageForBindingOnlyCollection monthlyUsages;
	}

	public class UsageLineForBindingOnly : NonPersistentBusinessObject, IObsoleteValidation
	{
		public UsageLineForBindingOnly(UsageLine usageLine)
		{
			if (usageLine != null)
			{
				Sequence = usageLine.Sequence;
				PeriodStart = usageLine.PeriodStart;
				PriceItemCode = usageLine.PriceItemCode;
				UnitCount = usageLine.UnitCount;
				Price = usageLine.Price;
				PriceCurrency = usageLine.PriceCurrency;
				PreDiscountAmount = usageLine.PreDiscountAmount;
				DiscountedPrice = usageLine.DiscountedPrice;
				DiscountAmount = usageLine.DiscountAmount;
				PostDiscountAmount = usageLine.PostDiscountAmount;
				LicenceUnits = usageLine.LicenceUnits;
				LocalCurrency = usageLine.LocalCurrency;
				LocalPreDiscountAmount = usageLine.LocalPreDiscountAmount;
				LocalDiscountAmount = usageLine.LocalDiscountAmount;
				LocalPostDiscountAmount = usageLine.LocalPostDiscountAmount;
				EnterpriseCode = usageLine.EnterpriseCode;
				CompanyCode = usageLine.CompanyCode;
				ServerCode = usageLine.ServerCode;
				PriceItemDesc = usageLine.PriceItemDesc;
			}
		}

		public ZInt Sequence { get; private set; }
		public ZDateTime PeriodStart { get; private set; }
		public ZString PriceItemCode { get; private set; }
		public ZDecimal UnitCount { get; private set; }
		public ZDecimal Price { get; private set; }
		public ZString PriceCurrency { get; private set; }
		public ZDecimal PreDiscountAmount { get; private set; }
		public ZDecimal DiscountedPrice { get; private set; }
		public ZDecimal DiscountAmount { get; private set; }
		public ZDecimal PostDiscountAmount { get; private set; }
		public ZDecimal LicenceUnits { get; private set; }
		public ZString LocalCurrency { get; private set; }
		public ZDecimal LocalPreDiscountAmount { get; private set; }
		public ZDecimal LocalDiscountAmount { get; private set; }
		public ZDecimal LocalPostDiscountAmount { get; private set; }
		public ZString EnterpriseCode { get; private set; }
		public ZString CompanyCode { get; private set; }
		public ZString ServerCode { get; private set; }
		public ZString PriceItemDesc { get; private set; }
	}

	public class UsageLineForBindingOnlyCollection : NonPersistentBusinessObjectCollection<UsageLineForBindingOnly>
	{
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new UsageLineForBindingOnly(null);
		}
	}

	public class StlMonthlyUsageForBindingOnly : NonPersistentBusinessObject, IObsoleteValidation
	{
		public ZString ServerCode { get; private set; }
		public ZString EnterpriseCode { get; private set; }
		public ZDecimal TotalLicenceUnits { get; private set; }
		public ZString PriceListVersion { get; private set; }
		public ZBool IsSiteLive { get; private set; }
		public ZDateTime SiteLiveDate { get; private set; }

		public StlMonthlyUsageForBindingOnly(StlMonthlyUsage stlMonthlyUsage)
		{
			if (stlMonthlyUsage != null)
			{
				ServerCode = stlMonthlyUsage.ServerCode;
				EnterpriseCode = stlMonthlyUsage.EnterpriseCode;
				TotalLicenceUnits = stlMonthlyUsage.TotalLicenceUnits;
				PriceListVersion = stlMonthlyUsage.PriceListVersion;
				IsSiteLive = stlMonthlyUsage.IsSiteLive;
				SiteLiveDate = stlMonthlyUsage.SiteLiveDate;

				foreach (UsageLine usageLine in stlMonthlyUsage.UsageLinesForBindingOnly)
				{
					UsageLinesForBindingOnly.Add(new UsageLineForBindingOnly(usageLine));
				}
			}
		}

		public UsageLineForBindingOnlyCollection UsageLinesForBindingOnly
		{
			get { return usageLines ?? (usageLines = new UsageLineForBindingOnlyCollection()); }
		}

		UsageLineForBindingOnlyCollection usageLines;
	}

	public class StlMonthlyUsageForBindingOnlyCollection : NonPersistentBusinessObjectCollection<StlMonthlyUsageForBindingOnly>
	{
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new StlMonthlyUsageForBindingOnly(null);
		}
	}
}
