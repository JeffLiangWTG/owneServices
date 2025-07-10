using System;
using System.Globalization;

namespace CargoWise.Common
{
	public static class DefaultCulture
	{
		/// <summary>
		/// Default culture for the application, which is set to Australian English (en-AU)
		/// </summary>
		public static CultureInfo Instance
		{
			get
			{
				if (defaultCulture == null)
				{
					defaultCulture = new CultureInfo("en-AU", false);

					SetupDefaultCulture();
				}

				return defaultCulture;
			}
		}

		static void SetupDefaultCulture()
		{
			// Setting the below because these could have been changed by the user in Regional Settings
			defaultCulture.NumberFormat.NumberDecimalSeparator = ".";
			defaultCulture.NumberFormat.CurrencyDecimalSeparator = defaultCulture.NumberFormat.NumberDecimalSeparator;
			defaultCulture.NumberFormat.PercentDecimalSeparator = defaultCulture.NumberFormat.NumberDecimalSeparator;

			defaultCulture.NumberFormat.NumberGroupSeparator = ",";
			defaultCulture.NumberFormat.CurrencyGroupSeparator = defaultCulture.NumberFormat.NumberGroupSeparator;
			defaultCulture.NumberFormat.PercentGroupSeparator = defaultCulture.NumberFormat.NumberGroupSeparator;

			defaultCulture.NumberFormat.CurrencySymbol = "$";

			defaultCulture.NumberFormat.NegativeSign = "-";
			defaultCulture.NumberFormat.PositiveSign = "+";

			defaultCulture.DateTimeFormat.AbbreviatedMonthNames = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec", "" };
			defaultCulture.DateTimeFormat.AbbreviatedMonthGenitiveNames = defaultCulture.DateTimeFormat.AbbreviatedMonthNames;
			defaultCulture.DateTimeFormat.AMDesignator = defaultCulture.DateTimeFormat.AMDesignator.ToUpper();
			defaultCulture.DateTimeFormat.PMDesignator = defaultCulture.DateTimeFormat.PMDesignator.ToUpper();
			defaultCulture.DateTimeFormat.ShortDatePattern = "d/MM/yyyy";
		}

		[ThreadStatic]
		static CultureInfo defaultCulture;
	}
}
