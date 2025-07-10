using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;

namespace Enterprise.Client.EDI.Billing.Hosting
{
	public class HostingDataAccessUsage : HostingUsage
	{
		public HostingDataAccessUsage(BusinessObjectFactory factory, IUsingParty user, ZDateTime periodStart, ZString code, ZString subCode, ZInt rawUnitCount)
			: base(factory, user, periodStart, code, subCode, rawUnitCount)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007:CustomizableDataTranslationRule", Justification = "Baseline")]
		public override SummarySection[] GetGeneralSummarySections()
		{
			ZString mainDescription = SystemDescription + " Usage";

			var result = new SummarySection(Factory);
			var summaryLine = result.Lines.AddNew();
			summaryLine.MainDescription = PriceItem?.L7_Description.Trim() ?? mainDescription;
			summaryLine.UnitPrice = UnitPrice.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture);
			summaryLine.UnitCount = UnitCount.ToString();
			summaryLine.Amount = Amount.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture);

			var summaryHeader = result.Header;
			summaryHeader.MainDescription = mainDescription;
			summaryHeader.UnitPrice = "Price";
			summaryHeader.UnitCount = "Units";
			summaryHeader.Amount = "Total";
			summaryHeader.TotalAmount = Amount.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture);

			return new [] { result };
		}
	}
}

