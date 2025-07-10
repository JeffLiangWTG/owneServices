using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;

namespace Enterprise.Client.EDI.Billing.Fee
{
	public class FeeSystemUsage : SystemUsage
	{
		public FeeSystemUsage(BusinessObjectFactory factory, IUsingParty user, ZDateTime periodStart, ClientLicenceFee[] fees, bool isRemitTo = false)
			: base(factory, user, periodStart)
		{
			this.fees = fees;
			this.isRemitTo = isRemitTo;
		}

		readonly ClientLicenceFee[] fees;
		readonly bool isRemitTo;

		public override ZString SystemCode
		{
			get { return BillingConstants.BillingSystem.Fee; }
		}

		#region Fees

		public IEnumerable<ClientLicenceFee> Fees
		{
			get { return fees; }
		}

		#endregion

		#region IsRemitTo

		public bool IsRemitTo
		{
			get { return isRemitTo; }
		}

		#endregion

		#region Amount

		protected override ZDecimal AmountCore
		{
			get { return !isRemitTo ? Fees.Sum(x => x.L8_Amount) : Fees.Sum(x => -x.L8_Amount); }
		}

		protected override ZDecimal AmountExemptProcessingFeeCore
		{
			get
			{
				var feeTypes = EDIDataRegistry.Instance.LicenceFeeTypes.Value;
				return Fees.Sum(x => feeTypes.GetBoolFromCode(x.L8_Type) ? x.L8_Amount : ZDecimal.Zero);
			}
		}

		#endregion

		#region CurrencyCode

		public override ZString CurrencyCode
		{
			get
			{
				return fees != null && fees.Length > 0
					? fees[0].L8_RX_NKCurrency
					: ZString.Empty;
			}
		}

		#endregion

		#region Summary Sections

		public override SummarySection[] GetGeneralSummarySections()
		{
			SummarySection result = new SummarySection(Factory);

			SummaryLine summaryHeader = result.Header;
			summaryHeader.MainDescription = "Product Fees";
			if (isRemitTo) { summaryHeader.MainDescription += " (Remitable)"; }
			summaryHeader.Amount = "Amount";
			summaryHeader.TotalAmount = Amount.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture);

			foreach (ClientLicenceFee licenceFee in Fees)
			{
				SummaryLine summaryLine = result.Lines.AddNew();
				summaryLine.MainDescription = !isRemitTo ? licenceFee.L8_DescriptionMultilingual.ToString() : "Remit: " + licenceFee.L8_DescriptionMultilingual;
				summaryLine.Amount = !isRemitTo
					? licenceFee.L8_Amount.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture)
					: (-licenceFee.L8_Amount).ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture);
			}

			return new SummarySection[] { result };
		}

		#endregion
	}
}

