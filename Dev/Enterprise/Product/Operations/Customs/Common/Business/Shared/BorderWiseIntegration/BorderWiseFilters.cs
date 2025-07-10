using System;

namespace Enterprise.Customs.Common
{
	public sealed class BorderWiseFilters
	{
		public BorderWiseFilters(string tariffWithStatCode = null)
		{
			if (tariffWithStatCode != null)
			{
				var components = tariffWithStatCode.Split(' ');

				if (components.Length > 0)
				{
					Tariff = components[0];
				}
				if (components.Length > 1)
				{
					Stat = components[1];
				}
			}
		}

		public AdditionalDataForBorderWise AdditionalData { get; set; }

		public bool DataForVFP { get; set; }
		public bool DataForFPD { get; set; }

		public string ImpExp
		{
			get { return impExp ?? string.Empty; }
			set { impExp = value?.Trim(); }
		}
		string impExp;

		public string CountryCodeOverride
		{
			get { return countryCodeOverride ?? string.Empty; }
			set { countryCodeOverride = value?.Trim(); }
		}
		string countryCodeOverride;

		public string DateForDutyRate
		{
			get { return dateForDutyRate ?? string.Empty; }
			set { dateForDutyRate = value?.Trim(); }
		}
		string dateForDutyRate;

		public string Tariff
		{
			get { return tariff ?? string.Empty; }
			set { tariff = value?.Trim(); }
		}
		string tariff;

		public string Stat
		{
			get { return stat ?? string.Empty; }
			set { stat = value?.Trim(); }
		}
		string stat;

		public string TreatmentCode
		{
			get { return treatmentCode ?? string.Empty; }
			set { treatmentCode = value?.Trim(); }
		}
		string treatmentCode;

		public string TariffConcessionOrder
		{
			get { return tariffConcessionOrder ?? string.Empty; }
			set { tariffConcessionOrder = value?.Trim(); }
		}
		string tariffConcessionOrder;

		public string CustomsUQ
		{
			get { return customsUQ ?? string.Empty; }
			set { customsUQ = value; }
		}
		string customsUQ;

		public Uri ReturnUri { get; set; }

		public bool AddCountryParameter { get; set; } = true;

		public string RequestId { get; set; }

		public string FocusedCommodity { get; set; }
	}
}
