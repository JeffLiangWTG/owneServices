using System;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;

namespace Enterprise.Client.EDI.Billing
{
	public class ABMCustomsUsage : EServicesSystemUsage
	{
		public ABMCustomsUsage(BusinessObjectFactory factory, IUsingParty user, ZDateTime periodStart)
			: this("", factory, user, periodStart)
		{
		}

		public ABMCustomsUsage(ZString subCode, BusinessObjectFactory factory, IUsingParty user, ZDateTime periodStart)
			: base(subCode, factory, user, periodStart)
		{
			PriceHeaderCode = BillingConstants.BillingSystem.ABMCustoms;
		}

		public override ZString SystemCode
		{
			get { return BillingConstants.BillingSystem.ABMCustoms; }
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
					priceHeader = standardPricesCompany.PriceHeaderForDate(midMonth, PriceHeaderCode);
				}

				return priceHeader;
			}
		}

		#region Properties

		public bool IsCustoms
		{
			get { return SubCode == ABMCustomsTransactionTypes.Codes.Customs; }
		}

		public bool IsPortCommunity
		{
			get { return SubCode == ABMCustomsTransactionTypes.Codes.PortCommunity; }
		}

		public bool IsFiscalRep
		{
			get { return SubCode == ABMCustomsTransactionTypes.Codes.FiscalRep; }
		}

		public ZString Jurisdiction { get { return Reference1; } }

		public ZString Department { get { return Reference3; } }

		public ZString Provider { get { return Reference2; } }

		#endregion

		#region Summary Sections

		public override SummarySection[] GetGeneralSummarySections()
		{
			SummarySection result = new SummarySection(Factory);
			SummaryLine summaryLine = result.Lines.AddNew();
			summaryLine.MainDescription = HasPriceItem ? PriceItem.L7_DescriptionLocalized.Trim() : GetTransactionDescription(SubCode);
			summaryLine.AdditionalDescription = Jurisdiction;
			summaryLine.PurchasedCount = Department;
			summaryLine.UnitCount = Provider;
			summaryLine.TotalUnitCount = ((int)UnitCount).ToString(CultureInfo.InvariantCulture);
			summaryLine.UnitPrice = UnitPrice.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture);
			summaryLine.Amount = Amount.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture);

			SummaryLine summaryHeader = result.Header;
			summaryHeader.MainDescription = string.Format(CultureInfo.InvariantCulture, "{0} Usage", GetTransactionDescription(SubCode));
			summaryHeader.AdditionalDescription = "Jurisdiction";
			summaryHeader.PurchasedCount = "Department";
			summaryHeader.UnitCount = IsPortCommunity ? "Provider" : "";
			summaryHeader.TotalUnitCount = "Count";
			summaryHeader.UnitPrice = "Price";
			summaryHeader.Amount = "Total";
			summaryHeader.TotalAmount = Amount.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture);
			summaryHeader.ClientCompanyDescription = ClientCompanyDescription;

			return new SummarySection[] { result };
		}

		ZString GetTransactionDescription(string code)
		{
			var transactionTypes = Factory.GetCachedValue("ABMCustomsUsage.ABMCustomsTransactionTypes", () => { return new ABMCustomsTransactionTypes(); });
			return "ABM " + transactionTypes.GetDescriptionFromCode(code);
		}

		#endregion
	}
}

