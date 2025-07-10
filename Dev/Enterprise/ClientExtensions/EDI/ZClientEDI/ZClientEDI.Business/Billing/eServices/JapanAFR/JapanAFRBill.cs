using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;

namespace Enterprise.Client.EDI.Billing
{
	public class JapanAFRBill : TransactionSystemBillEx
	{
		public JapanAFRBill(BusinessObjectFactory factory)
			: base(BillingConstants.BillingSystem.JapanAFR, factory)
		{
		}

		#region Summary Sections

		public override SummarySection[] GetGeneralSummarySections(ZGuid organisationPK)
		{
			var organisationUsages = SystemUsages.Cast<JapanAFRUsage>().Where(x => x.OrganisationPK == organisationPK).ToArray();
			if (organisationUsages.Length == 0)
			{
				return System.Array.Empty<SummarySection>();
			}

			return new SummarySection[] { BuildSingleSummarySection(organisationUsages, false) };
		}

		#endregion

		#region Group Summary Sections

		public override SummarySection[] GetGroupSummarySections()
		{
			var result = base.GetGroupSummarySections();
			foreach (var section in result)
			{
				section.Header.MainDescription = "Pre Departure Sea Manifest Filing (AFR) Group Summary";
			}
			return result;
		}

		#endregion

		#region Invoice

		protected override ZString GetInvoiceDescription(PriceItemUsage systemUsage, int unitCount)
		{
			return ZString.Format("Pre Departure Sea Manifest Filing - {0} Japan AFR Transactions at {1} {2} per Transaction",
				unitCount,
				systemUsage.CurrencyCode,
				systemUsage.PriceItem.L7_Price.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture));
		}

		protected override ZString GetInvoiceDiscountDescription()
		{
			return "Pre Departure Sea Manifest Filing - " + base.GetInvoiceDiscountDescription();
		}

		#endregion
	}
}

