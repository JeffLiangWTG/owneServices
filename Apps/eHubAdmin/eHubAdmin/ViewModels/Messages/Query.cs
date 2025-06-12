extern alias Sys;
using System;
using System.Globalization;
using System.Web.Mvc;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using Sys::System.ComponentModel.DataAnnotations;

namespace eServices.eHubAdmin.ViewModels.Messages
{
	public class Query
	{
		public Query()
		{
			IsIncludingArchiveStaging = true;
			IsIncludingArchive = true;
			IsIncludingMessageBox = true;
			SearchType = "Advanced";
		}

		public string Role { get; set; }
		public string AnyRole { get; set; }
		public string DateFilterType { get; set; }
		public string FromPeriodType { get; set; }
		public string ToPeriodType { get; set; }
		[RegularExpression("^[0-9]*$", ErrorMessage = "Invalid value. Positive integer only.")]
		[Range(1, 100, ErrorMessage = "Out of range. Min is 1 and max is 100.")]
		public int? FromPeriodValue { get; set; }
		[RegularExpression("^[0-9]*$", ErrorMessage = "Invalid value. Positive integer only.")]
		[Range(0, 100, ErrorMessage = "Out of range. Min is 0 and max is 100.")]
		public int? ToPeriodValue { get; set; }
		public string SenderInbox { get; set; }
		public string RecipientInbox { get; set; }
		public string SenderOutbox { get; set; }
		public string RecipientOutbox { get; set; }

		public string To { get; set; }
		[DataType(DataType.Date)]
		[DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
		[Display(Name = "To")]
		public DateTime? ToDate { get; set; }
		[DataType(DataType.Time)]
		[DisplayFormat(DataFormatString = @"{0:hh\:mm}", ApplyFormatInEditMode = true)]
		public TimeSpan? ToTime { get; set; }

		public string From { get; set; }
		[DataType(DataType.Date)]
		[DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
		[Display(Name = "From")]
		public DateTime? FromDate { get; set; }
		[DataType(DataType.Time)]
		[DisplayFormat(DataFormatString = @"{0:hh\:mm}", ApplyFormatInEditMode = true)]
		public TimeSpan? FromTime { get; set; }

		public string TimeZoneIdIANA { get; set; } = "Australia/Sydney";

		public string DateRangeType { get; set; }

		public DateTime? Received { get; set; }

		public string Status { get; set; }

		private string applicationFull;
		[JsonIgnore]
		public string ApplicationFull
		{
			get { return applicationFull; }
			set
			{
				if (value.IsNullOrWhiteSpace()) return;
				ApplicationCode = value.Contains(" ") ? value.Substring(0, value.IndexOf(" ", StringComparison.Ordinal)) : value;
				applicationFull = value;
			}
		}

		[Display(Name = "Application Code"), MaxLength(3)]
		public string ApplicationCode { get; set; }

		public int? Refresh { get; set; }

		public string KeyType { get; set; }
		[RegularExpression(@"[a-fA-F0-9]{8}(?:-[a-fA-F0-9]{4}){3}-[a-fA-F0-9]{12}", ErrorMessage = "Key value is not a valid GUID")]
		public Guid? Key { get; set; }

		[Display(Name = "Archive Staging",
			Description =
				"There are too many messages yet to be archived and searching them would adversely affect the search performance and more than likely result in a timeout!")]
		public bool? IsIncludingArchiveStaging { get; set; }

		[Display(Name = "Archive")]
		public bool? IsIncludingArchive { get; set; }

		[Display(Name = "Message Box")]
		public bool? IsIncludingMessageBox { get; set; }

		public string SearchType { get; set; }

		internal void ConvertDateTimesToStrings()
		{
			To = null;
			if (ToDate.HasValue)
			{
				To = ToTime.HasValue ? ToDate.Value.Add(ToTime.Value).ToString("yyyyMMddHHmmss") : ToDate.Value.ToString("yyyyMMdd");
			}
			ToDate = null;
			ToTime = null;

			From = null;
			if (FromDate.HasValue)
			{
				From = FromTime.HasValue ? FromDate.Value.Add(FromTime.Value).ToString("yyyyMMddHHmmss") : FromDate.Value.ToString("yyyyMMdd");
			}
			FromDate = null;
			FromTime = null;
		}

		internal void ConvertStringsToDateTimes()
		{
			DateTime parsedDate;
			if (!string.IsNullOrWhiteSpace(To) && !ToDate.HasValue)
			{
				if (DateTime.TryParseExact(To, "yyyyMMdd", null, DateTimeStyles.None, out parsedDate))
				{
					ToDate = parsedDate.Date;
					ToTime = null;
				}
				else if (DateTime.TryParseExact(To, "yyyyMMddHHmmss", null, DateTimeStyles.None, out parsedDate))
				{
					ToDate = parsedDate.Date;
					ToTime = parsedDate.TimeOfDay;
				}
			}
			To = null;

			if (!string.IsNullOrWhiteSpace(From) && !FromDate.HasValue)
			{
				if (DateTime.TryParseExact(From, "yyyyMMdd", null, DateTimeStyles.None, out parsedDate))
				{
					FromDate = parsedDate.Date;
					FromTime = null;
				}
				else if (DateTime.TryParseExact(From, "yyyyMMddHHmmss", null, DateTimeStyles.None, out parsedDate))
				{
					FromDate = parsedDate.Date;
					FromTime = parsedDate.TimeOfDay;
				}
			}
			From = null;
		}

		internal DateTime GetPeriodDate(DateTime currentDate, int periodValue, string periodType)
		{
			switch (periodType)
			{
				case "Hour":
					periodValue *= 60;
					break;
				case "Day":
					periodValue *= 24 * 60;
					break;
				case "Week":
					periodValue *= 7 * 24 * 60;
					break;
				case "Month":
					periodValue *= 30 * 24 * 60;
					break;
			}

			var periodStartDate = currentDate.AddMinutes(-(periodValue));
			var periodStartDateTime = new DateTime(periodStartDate.Year, periodStartDate.Month, periodStartDate.Day, periodStartDate.Hour, periodStartDate.Minute, periodStartDate.Second);
			return periodStartDateTime;
		}

		internal void FixData()
		{
			if (!IsIncludingArchive.HasValue)
			{
				IsIncludingArchive = true;
			}
			if (!IsIncludingMessageBox.HasValue)
			{
				IsIncludingMessageBox = true;
			}
			if (!IsIncludingArchiveStaging.HasValue)
			{
				IsIncludingArchiveStaging = true;
			}

			if (Role == "Any")
			{
				SenderInbox = null;
				RecipientInbox = null;
				SenderOutbox = null;
				RecipientOutbox = null;
			}
			else
			{
				AnyRole = null;
			}
			Role = null;

			if (string.IsNullOrWhiteSpace(KeyType) || Key == null)
			{
				KeyType = null;
				Key = null;
			}

			ConvertDateTimesToStrings();

			if (From == null && To == null)
			{
				if (string.IsNullOrEmpty(DateFilterType))
					DateFilterType = "WithinPeriod";
				if (string.IsNullOrEmpty(FromPeriodType))
					FromPeriodType = "Hour";
				if (!FromPeriodValue.HasValue)
					FromPeriodValue = 1;
				if (string.IsNullOrEmpty(ToPeriodType))
					ToPeriodType = "Hour";
				if (!ToPeriodValue.HasValue)
					ToPeriodValue = 0;
			}
			else
			{
				DateFilterType = "WithinDates";
			}
		}
	}
}