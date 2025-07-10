using System;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;

namespace Enterprise.Client.EDI.Billing
{
	public class AirlineMessagingUsage : EServicesSystemUsage
	{
		public AirlineMessagingUsage(BusinessObjectFactory factory, IUsingParty user, ZDateTime periodStart)
			: base(factory, user, periodStart)
		{
		}

		public AirlineMessagingUsage(ZString subCode, BusinessObjectFactory factory, IUsingParty user, ZDateTime periodStart)
			: base(factory, user, periodStart)
		{
			SubCode = subCode;
		}

		public override ZString SystemCode
		{
			get { return BillingConstants.BillingSystem.AirlineMessaging; }
		}

		#region Properties

		public bool IsRemitUsage
		{
			get { return ChargeTypeCode == "P" && UnitPrice < 0; }
		}

		public bool IsChargeableTraxonUsage
		{
			get { return ChargeTypeCode == "P" && UnitPrice >= 0; }
		}

		public bool IsNonChargeableTraxonUsage
		{
			get { return ChargeTypeCode == "N"; }
		}

		public bool IsFWBUsage
		{
			get { return SubCode.StartsWith("W", StringComparison.OrdinalIgnoreCase); }
		}

		public bool IsFHLUsage
		{
			get { return SubCode.StartsWith("H", StringComparison.OrdinalIgnoreCase); }
		}

		public bool IsFSUUsage
		{
			get { return SubCode.StartsWith("S", StringComparison.OrdinalIgnoreCase); }
		}

		public ZString ProviderName
		{
			get
			{
				var result = ZString.Empty;
				var providerCode = ProviderCode;
				if (providerCode == "1") { result = "BT"; }
				if (providerCode == "2") { result = "Delta"; }
				if (providerCode == "3") { result = "CCSJ"; }
				if (providerCode == "4") { result = "CCN"; }
				if (providerCode == "5") { result = "Descartes"; }
				if (providerCode == "6") { result = "GLSHK"; }
				if (providerCode == "A" || providerCode == "X" || providerCode == "E" || providerCode == "R") { result = "Traxon"; }
				if (providerCode == "U") { result = "Other Provider"; }
				return result;
			}
		}

		public ZString ProviderChargeInfo
		{
			get
			{
				var result = ZString.Empty;
				var providerCode = ProviderCode;
				if (providerCode == "X") { result = " (CX/LY/AI/5X/US)"; }
				if (providerCode == "E") { result = " (EDP Service)"; }
				if (providerCode == "R") { result = " (RCF Service)"; }
				return result;
			}
		}

		public ZString ProviderCodeForDiscount
		{
			get
			{
				var result = ZString.Empty;
				var providerCode = SubCode.SubstringSafe(1, 1);
				if (providerCode == "A" || providerCode == "X")
				{
					result = "ADX";
				}
				else
				{
					result = "AD" + providerCode;
				}
				return result;
			}
		}

		ZString ProviderCode
		{
			get { return SubCode.SubstringSafe(1, 1); }
		}

		ZString ChargeTypeCode
		{
			get { return SubCode.SubstringSafe(2, 1); }
		}

		#endregion

		#region Summary Sections

		public override SummarySection[] GetGeneralSummarySections()
		{
			if (Amount != 0 || IsNonChargeableTraxonUsage)
			{
				SummarySection result = new SummarySection(Factory);
				SummaryLine summaryLine = result.Lines.AddNew();
				summaryLine.MainDescription = PriceItem != null ? PriceItem.L7_DescriptionLocalized : ZString.Empty;
				summaryLine.UnitPrice = UnitPrice.ToString(BillingConstants.AmountFourDecimalFormat, CultureInfo.InvariantCulture);
				summaryLine.UnitCount = ((int)UnitCount).ToString(CultureInfo.InvariantCulture);
				summaryLine.Amount = Amount.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture);

				SummaryLine summaryHeader = result.Header;
				summaryHeader.MainDescription = "Airline Messaging Usage";
				if (IsRemitUsage) { summaryHeader.MainDescription += " (Remitable)"; }
				else if (IsChargeableTraxonUsage) { summaryHeader.MainDescription += " (Chargeable)"; }
				else if (IsNonChargeableTraxonUsage) { summaryHeader.MainDescription += " (Non-Chargeable)"; }
				else if (!ProviderName.IsEmpty) { summaryHeader.MainDescription += " - " + ProviderName; }

				summaryHeader.UnitPrice = "Price";
				summaryHeader.UnitCount = "Message Count";
				summaryHeader.Amount = "Total";
				summaryHeader.TotalAmount = Amount.ToString(BillingConstants.AmountFourDecimalFormat, CultureInfo.InvariantCulture);
				summaryHeader.ClientCompanyDescription = ClientCompanyDescription;

				return new SummarySection[] { result };
			}
			else
			{
				return Array.Empty<SummarySection>();
			}
		}

		#endregion
	}
}

