using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;

namespace Enterprise.Client.EDI.Billing.Fee
{
	public class FeeRawUsage : SystemRawUsage
	{
		public FeeRawUsage(BillingLoadRawUsageContext context)
			: base(context)
		{
		}

		public override ZString SystemCode
		{
			get { return BillingConstants.BillingSystem.Fee; }
		}

		public override SummarySection[] GetRawUsageSummarySections()
		{
			SummarySection result = new SummarySection(Factory);

			EDIOrgHeader organisation = Factory.Load<EDIOrgHeader>(context.OrganisationPK);
			if (organisation != null && organisation.LicCompany != null)
			{
				IEnumerable<ClientLicenceFee> matchedFees = organisation.LicCompany.Fees.GetMatched(PeriodStart);

				if (matchedFees.Any())
				{
					result.Header.MainDescription = "Product Fees";
					result.Header.Amount = "Amount";
					result.Header.TotalAmount = matchedFees.Sum(x => x.L8_Amount).ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture);
					result.Header.TotalDescription = string.Format(CultureInfo.CurrentCulture, "{0} ({1})", "Total", matchedFees.First().L8_RX_NKCurrency);

					foreach (ClientLicenceFee fee in matchedFees)
					{
						SummaryLine summaryLine = result.Lines.AddNew();
						summaryLine.MainDescription = fee.L8_DescriptionMultilingual;
						summaryLine.Amount = fee.L8_Amount.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture);
					}
				}
			}

			return new SummarySection[] { result };
		}
	}
}

