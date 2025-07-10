using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;

namespace Enterprise.Client.EDI.Billing
{
	public class NZCustomsUsage : EServicesSystemUsage
	{
		public NZCustomsUsage(BusinessObjectFactory factory, IUsingParty user, ZDateTime periodStart)
			: this("", factory, user, periodStart)
		{
		}

		public NZCustomsUsage(ZString subCode, BusinessObjectFactory factory, IUsingParty user, ZDateTime periodStart)
			: base(subCode, factory, user, periodStart)
		{
			PriceHeaderCode = BillingConstants.BillingSystem.ODM;
		}

		public override ZString SystemCode
		{
			get { return BillingConstants.BillingSystem.NZCustoms; }
		}

		#region Properties

		public bool IsJobUsage
		{
			get { return !IsMessageUsage; }
		}

		public bool IsMessageUsage
		{
			get { return SubCode == "NZC"; }
		}

		#endregion

		#region Summary Sections

		public override SummarySection[] GetGeneralSummarySections()
		{
			SummarySection result = new SummarySection(Factory);
			SummaryLine summaryLine = result.Lines.AddNew();
			summaryLine.MainDescription = PriceItem != null ? PriceItem.L7_DescriptionLocalized : ZString.Empty;
			summaryLine.UnitPrice = UnitPrice.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture);
			summaryLine.UnitCount = ((int)UnitCount).ToString(CultureInfo.InvariantCulture);
			summaryLine.Amount = Amount.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture);

			SummaryLine summaryHeader = result.Header;
			summaryHeader.MainDescription = "NZ Customs";
			if (IsJobUsage) { summaryHeader.MainDescription += " Jobs"; }
			else if (IsMessageUsage) { summaryHeader.MainDescription += " Messages"; }

			summaryHeader.UnitPrice = "Price";
			summaryHeader.UnitCount = "Message Count";
			summaryHeader.Amount = "Total";
			summaryHeader.TotalAmount = Amount.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture);
			summaryHeader.ClientCompanyDescription = ClientCompanyDescription;

			return new SummarySection[] { result };
		}

		#endregion
	}
}

