using System;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;

namespace Enterprise.Client.EDI.Billing
{
	/// <summary>
	/// Usage of a single eHub client specific mapping by a single client.
	/// Contains just one ClientChargeableUsage record.
	/// </summary>
	public class ClientMappingUsage : EServicesSystemUsage
	{
		public ClientMappingUsage(ZString systemCode, BusinessObjectFactory factory, IUsingParty user, ZDateTime periodStart)
			: base(systemCode, "", factory, user, periodStart)
		{
			PriceHeaderCode = BillingConstants.PriceHeaderType.EHub;
		}

		public override bool ShowOnBillingSummary
		{
			get { return base.ShowOnBillingSummary || (UnitPrice != 0m && TransactionCount > 0); }
		}

		public override void SetPreviewOnly(ClientLicencePriceHeader previewPriceHeader)
		{
			// Preview does not override custom HUB prices
			if (!IsHubPriceHeaderCode)
			{
				base.SetPreviewOnly(previewPriceHeader);
			}
		}

		public bool IsHubPriceHeaderCode => PriceHeaderCode == BillingConstants.PriceHeaderType.EHub;

		protected override ClientLicencePriceItem GetPriceItemCore()
		{
			ClientLicencePriceItem result = null;
			foreach (var item in PriceHeader.LocalOrStandardItems.FindAllByCode(SystemCode))
			{
				if (string.Equals(item.L7_Ref4, SubCode, StringComparison.OrdinalIgnoreCase))
				{
					result = item;
					break;
				}
				else if (result == null && item.L7_Ref4.IsEmpty)
				{
					result = item;
				}
			}

			if (result == null && PriceHeaderCode == BillingConstants.PriceHeaderType.EHub)
			{
				PriceHeaderCode = BillingConstants.PriceHeaderType.ODM;
				ResetPriceHeader();
				result = PriceHeader?.LocalOrStandardItems.FindByCode(SystemCode);
			}

			return result;
		}

		public override ClientLicencePriceHeader PriceHeader
		{
			get
			{
				var result = base.PriceHeader;
				if (result == null && PriceHeaderCode == BillingConstants.PriceHeaderType.EHub)
				{
					PriceHeaderCode = BillingConstants.PriceHeaderType.ODM;
					result = base.PriceHeader;
				}

				return result;
			}
		}

		public override SummarySection[] GetGeneralSummarySections()
		{
			var mainDescription = !SummaryHeaderDescription.IsEmpty ? SummaryHeaderDescription : DefaultSummaryHeaderDescription;

			var result = new SummarySection(Factory);
			SummaryLine summaryLine = result.Lines.AddNew();
			summaryLine.MainDescription = PriceItem != null && PriceHeaderCode == BillingConstants.PriceHeaderType.EHub ? PriceItem.L7_DescriptionLocalized : SubCode;
			summaryLine.PurchasedCount = PriceItem != null ? PriceItem.L7_UnitBreak.ToString() : "0";
			summaryLine.UnitCount = TransactionCount.ToString();
			summaryLine.TotalUnitCount = UnitCount.ToString();
			summaryLine.UnitPrice = UnitPrice.ToString(BillingConstants.AmountFourDecimalFormat, CultureInfo.InvariantCulture);
			summaryLine.Amount = Amount.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture);

			SummaryLine summaryHeader = result.Header;
			summaryHeader.MainDescription = mainDescription;
			summaryHeader.UnitPrice = "Price";
			summaryHeader.PurchasedCount = "Included";
			summaryHeader.UnitCount = "Used";
			summaryHeader.TotalUnitCount = "Excess Usage";
			summaryHeader.Amount = "Total";
			summaryHeader.TotalAmount = Amount.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture);
			summaryHeader.ClientCompanyDescription = ClientCompanyDescription;

			return new SummarySection[] { result };
		}
	}
}

