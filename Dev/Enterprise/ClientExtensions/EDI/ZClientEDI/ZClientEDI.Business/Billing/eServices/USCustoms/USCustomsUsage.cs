using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;

namespace Enterprise.Client.EDI.Billing
{
	public class USCustomsUsage : EServicesSystemUsage
	{
		public USCustomsUsage(BusinessObjectFactory factory, IUsingParty user, ZDateTime periodStart)
			: this("", factory, user, periodStart)
		{
		}

		public USCustomsUsage(ZString subCode, BusinessObjectFactory factory, IUsingParty user, ZDateTime periodStart)
			: base(subCode, factory, user, periodStart)
		{
			PriceHeaderCode = BillingConstants.BillingSystem.ODM;
		}

		public override ZString SystemCode
		{
			get { return BillingConstants.BillingSystem.USCustoms; }
		}

		public override ZString PriceItemCode
		{
			get { return BillingConstants.BillingSystem.USCustoms; }
		}

		#region Summary Sections

		public override SummarySection[] GetGeneralSummarySections()
		{
			SummarySection result = new SummarySection(Factory);
			SummaryLine summaryLine = result.Lines.AddNew();
			summaryLine.MainDescription = USCustomsBill.TransactionCategoryDescription;
			summaryLine.UnitCount = ((int)TransactionCount).ToString(CultureInfo.InvariantCulture);
			summaryLine.UnitPrice = UnitPrice.ToString(BillingConstants.AmountFourDecimalFormat, CultureInfo.InvariantCulture);
			summaryLine.Amount = Amount.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture);

			SummaryLine summaryHeader = result.Header;
			summaryHeader.MainDescription = "USC Service Bureau";
			summaryHeader.UnitCount = "Transaction Count";
			summaryHeader.UnitPrice = "Price";
			summaryHeader.Amount = "Total";
			summaryHeader.TotalAmount = Amount.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture);
			summaryHeader.ClientCompanyDescription = ClientCompanyDescription;

			if (HasMinimumFee)
			{
				summaryHeader.TotalUnitCount = "Minimum Count";
				summaryLine.TotalUnitCount = UnitCount.ToString();
			}

			return new SummarySection[] { result };
		}

		public override ZInt UnitCount
		{
			get { return HasMinimumFee ? PriceItem.L7_UnitBreak : TransactionCount; }
		}

		public bool HasMinimumFee
		{
			get { return PriceItem != null && PriceItem.L7_UnitBreak > TransactionCount; }
		}

		#endregion
	}
}
