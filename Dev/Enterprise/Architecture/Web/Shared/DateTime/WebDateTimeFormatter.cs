using System;
using System.Globalization;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.Web.Shared
{
	public class WebDateTimeFormatter : IDateInputControl
	{
		#region Static

		public static string GetFormattedDate(ZDateTimeOffset value, ZDateTimePickerFormat dateTimeFormat) => GetFormattedDate(value.ToZDateTime(), dateTimeFormat);

		public static string GetFormattedDate(ZDateTime value, ZDateTimePickerFormat dateTimeFormat)
		{
			if (value.IsValid)
			{
				if (WebDateTimeParser.IsValidCultureForDatesParsingAndDisplay(WebDateFormatCulture))
				{
					return value.ToString(GetDateFormatString(dateTimeFormat), WebDateFormatCulture);
				}
				return value.ToString(GetDateFormatString(dateTimeFormat), WebDateTimeParser.DefaultCulture);
			}
			return string.Empty;
		}

		public static ZDateTime GetParsedDate(string value, ZDateTimePickerFormat dateTimeFormat)
		{
			if (!string.IsNullOrEmpty(value))
			{
				ZDateTime result = WebDateTimeParser.ParseDate(value);
				if (!result.IsValid)
				{
					var dateEditCore = ObjectFactory.Get<IDateEditCore>(nameof(IDateEditCore), new WebDateTimeFormatter(dateTimeFormat));
					result = dateEditCore.StringToDate(value.ToUpper());
					if (!result.IsValid)
					{
						ZDateTime.TryParseExact(value, out result, GetDateFormatString(dateTimeFormat));
						if (!result.IsValid)
						{
							ZDateTime.TryParseExact(value, out result, DateTimeFormatStrings.GetLocalizedFormatString(GetDateFormatString(dateTimeFormat)));
						}
					}
				}
				return result;
			}
			return ZDateTime.Empty;
		}

		public static CultureInfo WebDateFormatCulture
		{
			get
			{
				CultureInfo result = WebDateTimeParser.DefaultCulture;

				if (WebDataRegistry.Instance.DateFormat.Value == WebRegistryDateFormats.WebUserLocale)
				{
					result = WebDateTimeParser.WebUserCulture;
				}
				try
				{
					if (result.DateTimeFormat == null)
					{
						throw new NotSupportedException();
					}
				}
				catch (NotSupportedException) { result = WebDateTimeParser.DefaultCulture; }
				return result;
			}
		}

		public static string GetDateFormatString(ZDateTimePickerFormat dateTimeFormat)
		{
			switch (dateTimeFormat)
			{
				case ZDateTimePickerFormat.Long:
					return ZDateTime.LongTimeFormat;
				case ZDateTimePickerFormat.Time:
					return ZDateTime.ShortTimeFormat;
			}
			return ZDateTime.ShortDateFormat;
		}

		#endregion

		#region Constructors

		public WebDateTimeFormatter(string formatString)
			: base()
		{
			Argument.NotNullOrEmpty(formatString, "FormatString");
			this.formatString = formatString;
		}

		public WebDateTimeFormatter(ZDateTimePickerFormat dateTimeFormat)
			: base()
		{
			Argument.NotNull(dateTimeFormat, "DateTimeFormat");
			this.formatString = GetDateFormatString(dateTimeFormat);
		}

		#endregion

		#region IDateInputControl

		public int AutoCompleteMonthThreshold
		{
			get { return 0; }
		}

		public bool AutoCompleteYear
		{
			get { return true; }
		}

		public string FormatString
		{
			get { return formatString; }
		}

		readonly string formatString;

		#endregion

	}
}
