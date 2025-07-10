using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.Types;
using Newtonsoft.Json;

namespace Enterprise.Accounting.Business
{
	public class ElectronicProcessingChargeFeatureControlModel
	{
		public EnableDisbursementLicenseFeeModel EnableDisbursementLicenseFee { get; set; }

		public List<ElectronicProcessingChargeCurrencyModel> ElectronicProcessingChargeCurrency { get; set; }

		public List<ElectronicProcessingChargeConfigurationModel> ElectronicProcessingChargeConfiguration { get; set; }
	}

	public class EnableDisbursementLicenseFeeModel
	{
		public bool EnableSystemLevel { get; set; }

		public List<CompanySecurityModel> CompanyLevel { get; set; }
	}

	public class ElectronicProcessingChargeCurrencyModel
	{
		public string CurrencyCode { get; set; }

		readonly string[] acceptedShortDateFormats = { ZDateTime.ShortDateFormat, "yyyy-MM-dd", "dd-MM-yyyy", "dd-MM-yy" };

		[JsonProperty(nameof(ValidFromDate))]
		string validFromDateString;

		[JsonIgnore]
		public DateTime? ValidFromDate
		{
			get
			{
				return DateTime.TryParseExact(validFromDateString, acceptedShortDateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result) ? result : null;
			}
			set
			{
				validFromDateString = value?.ToString(ZDateTime.ShortDateFormat);
			}
		}
	}

	public class ElectronicProcessingChargeConfigurationModel
	{
		public string JobType { get; set; }

		readonly string[] acceptedShortDateFormats = { ZDateTime.ShortDateFormat, "yyyy-MM-dd", "dd-MM-yyyy", "dd-MM-yy" };

		[JsonProperty(nameof(StartDate))]
		string startDateString;

		[JsonIgnore]
		public DateTime? StartDate
		{
			get
			{
				return DateTime.TryParseExact(startDateString, acceptedShortDateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result) ? result : null;
			}
			set
			{
				startDateString = value?.ToString(ZDateTime.ShortDateFormat);
			}
		}

		[JsonProperty(nameof(EndDate))]
		string endDateString;

		[JsonIgnore]
		public DateTime? EndDate
		{
			get
			{
				return DateTime.TryParseExact(endDateString, acceptedShortDateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result) ? result : null;
			}
			set
			{
				endDateString = value?.ToString(ZDateTime.ShortDateFormat);
			}
		}
	}
}
