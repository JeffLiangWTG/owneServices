using System.Globalization;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;

namespace Enterprise.Accounting.ElectronicMessaging.Hungary
{
	public class VatRateAggregateWithTypeInfo : VatRateAggregate
	{
		public VatRateAggregateWithTypeInfo(VATRateKey key)
		{
			Key = key;
		}

		public VATRateKey Key { get; }
	}

	public class VatRateAggregate
	{
		public string NetAmountOSFormatted => NetAmountOS.ToString("F2", CultureInfo.InvariantCulture);    // decimal format string as required by XSD
		public string NetAmountHUFFormatted => NetAmountHUF.ToString("F2", CultureInfo.InvariantCulture);    // decimal format string as required by XSD
		public string VatAmountOSFormatted => VatAmountOS.ToString("F2", CultureInfo.InvariantCulture);    // decimal format string as required by XSD
		public string VatAmountHUFFormatted => VatAmountHUF.ToString("F2", CultureInfo.InvariantCulture);    // decimal format string as required by XSD
		public string TotalAmountOSFormatted => TotalAmountOS.ToString("F2", CultureInfo.InvariantCulture);    // decimal format string as required by XSD
		public string TotalAmountHUFFormatted => TotalAmountHUF.ToString("F2", CultureInfo.InvariantCulture);    // decimal format string as required by XSD

		public decimal NetAmountOS { get; private set; }
		public decimal NetAmountHUF { get; private set; }
		public decimal VatAmountOS { get; private set; }
		public decimal VatAmountHUF { get; private set; }
		public decimal TotalAmountOS => NetAmountOS + VatAmountOS;
		public decimal TotalAmountHUF => NetAmountHUF + VatAmountHUF;

		public void Add(PostingJournal line)
		{
			NetAmountOS += line.OSAmount.GetValueOrDefault();
			NetAmountHUF += line.LocalAmount.GetValueOrDefault();
			VatAmountOS += line.OSGSTVATAmount.GetValueOrDefault();
			VatAmountHUF += line.LocalGSTVATAmount.GetValueOrDefault();
		}
	}

	public class VATRateKey
	{
		public VATRateTagType VATRateType { get; set; }
		public string VATRateValue { get; set; }
		public VATExemptionInfo VATExemption { get; set; }

		public bool Match(VATRateKey obj)
		{
			return obj != null &&
					VATRateType == obj.VATRateType &&
					VATRateValue == obj.VATRateValue &&
					VATExemption?.Case == obj.VATExemption?.Case &&
					VATExemption?.Reason == obj.VATExemption?.Reason;
		}
	}

	public class VATExemptionInfo
	{
		public string Case { get; set; }
		public string Reason { get; set; }
	}

	public enum VATRateTagType
	{
		None, VatPercentage, VatExemption, VatOutOfScope, VatDomesticReverseCharge
	}
}
