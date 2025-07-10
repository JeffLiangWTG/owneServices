using System;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;

namespace Enterprise.Client.EDI.Billing
{
	public class GlobalContainerTrackingUsage : EServicesSystemUsage
	{
		public GlobalContainerTrackingUsage(BusinessObjectFactory factory, IUsingParty user, ZDateTime periodStart)
			: base("", factory, user, periodStart)
		{
			PriceHeaderCode = BillingConstants.BillingSystem.GlobalContainerTracking;
		}

		public ZInt CompanyUsageCount { get; set; }

		public ZInt DatabaseUsageCount { get; set; }

		public ZDecimal TransactionPrice { get; set; }

		public ZString TransactionDescription { get; set; }

		public override ZString SystemCode
		{
			get { return BillingConstants.BillingSystem.GlobalContainerTracking; }
		}

		public override ZString PriceItemCode
		{
			get { return BillingConstants.BillingSystem.GlobalContainerTracking; }
		}

		public override ClientLicencePriceHeader PriceHeader
		{
			get
			{
				ClientLicencePriceHeader priceHeader = null;

				var standardPricesCompany = LicenceCompany.StandardPricesCompany;
				if (standardPricesCompany != null)
				{
					var midMonth = new DateTime(PeriodStart.Year, PeriodStart.Month, 15);
					priceHeader = standardPricesCompany.PriceHeaderForDate(midMonth, SystemCode);
				}

				return priceHeader;
			}
		}

		protected override bool PriceItemIsCorrectFeeType(ClientLicencePriceItem priceItem)
		{
			return priceItem != null && priceItem.L7_FeeType == BillingConstants.FeeType.TransactionalOneVolumeBreak;
		}

		public override ZDecimal UnitPrice
		{
			get { return TransactionPrice; }
		}

		public ZDecimal UsageCountShare
		{
			get { return DatabaseUsageCount > 0 ? (ZDecimal)CompanyUsageCount / DatabaseUsageCount : 1; }
		}

		public override SummarySection[] GetGeneralSummarySections()
		{
			var section = base.GetGeneralSummarySections()[0];

			section.Lines[0].MainDescription = "    " + TransactionDescription;
			section.Lines[0].UnitCount = TransactionCount.ToString();

			if (UsageCountShare != 1)
			{
				section.Header.AdditionalDescription = "Usage Charge Share";
				section.Lines[0].AdditionalDescription = string.Format(CultureInfo.InvariantCulture, "{0} / {1}", CompanyUsageCount, DatabaseUsageCount);
			}

			return new SummarySection[] { section };
		}
	}
}

