using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public class TypeValidation : ValidationProvider
	{
		public static string InvalidTypeMessage
		{
			get { return Res.GetString("5ed8f4dc-2084-466e-a380-4d1222ad5838", "Enter a valid {0:G}."); }
		}

		#region Check Valid ZBlob

		public const int ZBlobMaxBytesBeforeError = 1048576; // 1MB

		public const int ZBlobMaxBytesBeforeWarning = 102400; // 100K

		/// <summary>
		/// Sets an error if the ZBlob is larger than 1MB, or a warning if the ZBlob is larger than 100K.
		/// </summary>
		/// <param name="propertyInfo">The PropertyInfo to validate.</param>
		public static void CheckValidZBlobSize(ZPropertyInfo propertyInfo)
		{
			byte[] data = (byte[])((IZTypeInternals)propertyInfo.Value).GetValueForLogicalDataLayer(false);
			int numberOfBytes = data.Length;

			if (numberOfBytes > ZBlobMaxBytesBeforeError)
			{
				propertyInfo.AddError(Res.GetString("dced84a1-c6ab-4995-93cb-6eb8ddd67766", "This note is too large to store in the database. Reduce its size by removing any large images or files.\r\n\r\n   - Current Size:  {0} KB\r\n   - Maximum Size:  {1} KB",
					(numberOfBytes / 1024).ToString(),
					(ZBlobMaxBytesBeforeError / 1024).ToString()) + "\r\n\r\n");
			}
			else if (numberOfBytes > ZBlobMaxBytesBeforeWarning)
			{
				propertyInfo.AddWarning(Res.GetString("79d0a3db-ce26-46e0-b359-9ecdda27d8a1", "There is a large amount of data in this note. Consider reducing its size by removing any large images or files."));
			}
		}

		#endregion

		#region Check Valid Guid

		/// <summary>
		/// Sets an error if the Guid is not valid.
		/// </summary>
		/// <param name="propertyInfo">The PropertyInfo to validate.</param>
		public static void CheckValidGuid(ZPropertyInfo propertyInfo)
		{
			CheckValidGuid(propertyInfo, "");
		}

		public static void CheckValidGuid(ZPropertyInfo propertyInfo, ZString description)
		{
			CheckForMissingRecord(propertyInfo, description);
			if (!propertyInfo.HasErrors())
			{
				CheckValid(propertyInfo, description);
			}
		}

		public static string GetHumanReadablePropertyName(ZPropertyInfo propertyInfo, string fallBackName)
		{
			if (string.IsNullOrEmpty(fallBackName))
			{
				throw new ArgumentNullException(nameof(fallBackName));
			}
			return propertyInfo.HasHumanReadableName ? propertyInfo.HumanReadableName.ToString() : fallBackName;
		}

		public static string GetHumanReadablePropertyName(ZPropertyInfo propertyInfo)
		{
			return GetHumanReadablePropertyName(propertyInfo, Res.GetString("70726eea-98da-40cb-afc5-4f595447863d", "selection"));
		}

		static void CheckForMissingRecord(ZPropertyInfo propertyInfo, ZString description)
		{
			if (((ZGuid)propertyInfo.Value).IsMissing)
			{
				if (description.IsEmpty)
				{
					description = GetHumanReadablePropertyName(propertyInfo);
				}

				string error = Res.GetString("2a72d221-9a8b-44a0-9374-441ed251d36c", "The selected {0} is no longer valid. Please choose a new {0} from the list.", description);
				propertyInfo.AddError(error);
			}
		}

		#endregion

		#region Check Valid Geography

		/// <summary>
		/// Sets an error if the Geography is not valid.
		/// </summary>
		/// <param name="propertyInfo">The PropertyInfo to validate.</param>
		public static void CheckValidGeography(ZPropertyInfo propertyInfo)
		{
			CheckValidGeography(propertyInfo, "");
		}

		public static void CheckValidGeography(ZPropertyInfo propertyInfo, ZString description)
		{
			var value = (ZGeography)propertyInfo.Value;
			if (!CheckIsValidGeographyValue(value))
			{
				if (description.IsEmpty)
				{
					description = GetHumanReadablePropertyName(propertyInfo, Res.GetString("4cce05d4-e63f-4fe8-8c40-8651ac69d967", "geography value"));
				}

				string error = Res.GetString("5075ddfe-edd1-4306-b63c-0e2c13f7cae6", "The {0} you set is not valid.");
				propertyInfo.AddError(error);
			}
		}

		internal static bool CheckIsValidGeographyValue(ZGeography value)
		{
			return value.IsValid && value.STIsValid();
		}

		#endregion

		#region Check Valid ZDateTime

		/// <summary>
		/// Sets an error if the DateTime is not valid.
		/// </summary>
		/// <param name="propertyInfo">The PropertyInfo to validate.</param>
		public static void CheckValidZDateTimeAndRange(ZPropertyInfo propertyInfo)
		{
			CheckValidZDateTimeAndRange(propertyInfo, "");
		}

		public static void CheckValidZDateTimeAndRange(ZPropertyInfo propertyInfo, ZString description)
		{
			CheckValid(propertyInfo, description);
			CheckValidZDateTimeRange(propertyInfo);
		}

		public static void CheckValidZDateTimeAndRange(ZPropertyInfo propertyInfo, bool needCheckFutureYear, bool needCheckPastYear)
		{
			CheckValidZDateTimeAndRange(propertyInfo, needCheckFutureYear, needCheckPastYear, "");
		}

		public static void CheckValidZDateTimeAndRange(ZPropertyInfo propertyInfo, bool needCheckFutureYear, bool needCheckPastYear, ZString description)
		{
			CheckValid(propertyInfo, description);
			CheckValidZDateTimeRange(propertyInfo, needCheckFutureYear, needCheckPastYear);
		}

		public static void CheckValidZDateTimeWithoutRange(ZPropertyInfo propertyInfo)
		{
			CheckValidZDateTimeWithoutRange(propertyInfo, "");
		}

		public static void CheckValidZDateTimeWithoutRange(ZPropertyInfo propertyInfo, ZString description)
		{
			CheckValid(propertyInfo, description);
		}

		public static void CheckValidSmallDateTime(ZPropertyInfo propertyInfo)
		{
			var propertyValue = propertyInfo.Value;
			ZDateTime propertyDate = (propertyValue is ZDateTime)
				? (ZDateTime)propertyValue : (ZDate)propertyValue;
			if (propertyDate > ZDateTime.MaxSmallDateTimeValue)
			{
				propertyInfo.AddError(ErrorForSmallDateTimeFuture(propertyDate));
			}
			else if (propertyDate < ZDateTime.MinSmallDateTimeValue)
			{
				propertyInfo.AddError(ErrorForSmallDateTimePast(propertyDate));
			}

			CheckValid(propertyInfo, "");
		}

		public static bool IsInSmallDateTimeRange(ZPropertyInfo propertyInfo)
		{
			var propertyDate = (ZDateTime)propertyInfo.Value;
			if (propertyDate > ZDateTime.MaxSmallDateTimeValue || propertyDate < ZDateTime.MinSmallDateTimeValue)
			{
				return false;
			}
			return true;
		}

		public static void CheckValidZDateTimeRange(ZPropertyInfo propertyInfo)
		{
			if (propertyInfo.PropertyDescriptor.Attributes.OfType<IDurationBasedDateConverter>().Any())
			{
				return;
			}

			new DateRangeValidation().Validate(propertyInfo);
		}

		public static void CheckValidZDateTimeRange(ZPropertyInfo propertyInfo, bool needCheckFutureYear, bool needCheckPastYear = true)
		{
			new DateRangeValidation().Validate(propertyInfo, needCheckFutureYear, needCheckPastYear);
		}

		public static void CheckValidZDateTimeRange(ZPropertyInfo propertyInfo, TypeValidationLimits limits)
		{
			new DateRangeValidation(limits).Validate(propertyInfo);
		}

		public static bool IsWithinValidZDateTimeRangeWithoutError(ZDateTime dateTime)
		{
			return new DateRangeValidation().IsWithinValidZDateTimeRangeWithoutError(dateTime);
		}

		#endregion

		#region Check Valid ZDateTimeOffset

		/// <summary>
		/// Sets an error if the DateTimeOffset is not valid.
		/// </summary>
		/// <param name="propertyInfo">The PropertyInfo to validate.</param>
		public static void CheckValidZDateTimeOffsetAndRange(ZPropertyInfo propertyInfo)
		{
			CheckValidZDateTimeOffsetAndRange(propertyInfo, "");
		}

		public static void CheckValidZDateTimeOffsetAndRange(ZPropertyInfo propertyInfo, ZString description)
		{
			CheckValid(propertyInfo, description);
			CheckValidZDateTimeOffsetRange(propertyInfo);
		}

		public static void CheckValidZDateTimeOffsetWithoutRange(ZPropertyInfo propertyInfo)
		{
			CheckValidZDateTimeOffsetWithoutRange(propertyInfo, "");
		}

		public static void CheckValidZDateTimeOffsetWithoutRange(ZPropertyInfo propertyInfo, ZString description)
		{
			CheckValid(propertyInfo, description);
		}

		public static void CheckValidZDateTimeOffsetRange(ZPropertyInfo propertyInfo)
		{
			new DateRangeValidation().Validate(propertyInfo);
		}

		public static void CheckValidZDateTimeOffsetRange(ZPropertyInfo propertyInfo, TypeValidationLimits limits)
		{
			new DateRangeValidation(limits).Validate(propertyInfo);
		}

		public static bool IsWithinValidZDateTimeOffsetRangeWithoutError(ZDateTimeOffset dateTimeOffset)
		{
			return new DateRangeValidation().IsWithinValidZDateTimeOffsetRangeWithoutError(dateTimeOffset);
		}

		#endregion

		#region Check Valid ZDate

		/// <summary>
		/// Sets an error if the Date is not valid.
		/// </summary>
		/// <param name="propertyInfo">The PropertyInfo to validate.</param>
		public static void CheckValidZDateAndRange(ZPropertyInfo propertyInfo)
		{
			CheckValidZDateAndRange(propertyInfo, "");
		}

		public static void CheckValidZDateAndRange(ZPropertyInfo propertyInfo, ZString description)
		{
			CheckValid(propertyInfo, description);
			CheckValidZDateRange(propertyInfo);
		}

		public static void CheckValidZDateWithoutRange(ZPropertyInfo propertyInfo)
		{
			CheckValidZDateWithoutRange(propertyInfo, "");
		}

		public static void CheckValidZDateWithoutRange(ZPropertyInfo propertyInfo, ZString description)
		{
			CheckValid(propertyInfo, description);
		}

		public static void CheckValidZDateRange(ZPropertyInfo propertyInfo)
		{
			new DateRangeValidation().Validate(propertyInfo);
		}

		public static void CheckValidZDateRange(ZPropertyInfo propertyInfo, TypeValidationLimits limits)
		{
			new DateRangeValidation(limits).Validate(propertyInfo);
		}

		public static bool IsWithinValidZDateRangeWithoutError(ZDate dateTime)
		{
			return new DateRangeValidation().IsWithinValidZDateTimeRangeWithoutError(dateTime);
		}

		#endregion

		#region Check Valid Time

		public static void CheckValidZTime(ZPropertyInfo propertyInfo)
		{
			CheckValid(propertyInfo, "");
		}

		#endregion

		#region Check Valid Decimal

		/// <summary>
		/// Sets an error if the Decimal does not meet the related column's database precision requirements.
		/// </summary>
		/// <param name="propertyInfo">The PropertyInfo to validate.</param>
		/// <param name="databasePrecision">The decimal's precision in the Database.</param>
		/// <param name="databaseScale">The decimal's scale in the Database.</param>
		public static void CheckValidDecimal(ZPropertyInfo propertyInfo, int databasePrecision, int databaseScale)
		{
			var propertyValue = propertyInfo.Value;
			if (!(propertyValue is ZDecimal))
			{
				throw new Exception("Cannot validate the precision for '" + propertyInfo.Name + "' <" + propertyValue.GetType() + "> as it is not a ZDecimal!");
			}

			var value = (ZDecimal)propertyValue;
			if (!value.IsWithinSqlPrecisionAndScale(databasePrecision, databaseScale))
			{
				string errorField = GetHumanReadablePropertyName(propertyInfo);
				string error = Res.GetString("e3f9486f-3a7e-4777-af14-14cd4a07e56b", "The number {0} is too large, the maximum value allowed for {1} is {2}.",
					value.ToString("n" + GetNumberOfDecimals(value)), errorField, GetFormattedMaxDecimal(databasePrecision, databaseScale));
				propertyInfo.AddError(error);
			}
		}

		static string GetFormattedMaxDecimal(int databasePrecision, int databaseScale)
		{
			string result = GetMaxIntegralPart(databasePrecision, databaseScale).ToString("n0");

			if (databaseScale > 0)
			{
				result += "." + new string('9', databaseScale);
			}

			return result;
		}

		static decimal GetMaxIntegralPart(int databasePrecision, int databaseScale)
		{
			return (decimal)Math.Pow(10, databasePrecision - databaseScale) - 1;
		}

		static int GetNumberOfDecimals(decimal value)
		{
			int result = 0;

			string valueString = value.ToString();
			int decimalIndex = valueString.IndexOf(".");

			if (decimalIndex != -1)
			{
				result = valueString.Substring(decimalIndex + 1).Length;
			}

			return result;
		}

		#endregion

		#region Check Valid Money

		/// <summary>
		/// Sets an error if the Money does not meet the related column's database precision requirements.
		/// </summary>
		/// <param name="propertyInfo">The PropertyInfo to validate.</param>
		/// <param name="databasePrecision">The decimal's precision in the Database.</param>
		/// <param name="databaseScale">The decimal's scale in the Database.</param>
		public static void CheckValidMoney(ZPropertyInfo propertyInfo, int databasePrecision, int databaseScale)
		{
			var propertyValue = propertyInfo.Value;
			if (!(propertyValue is ZDecimal))
			{
				throw new Exception("Cannot validate the precision for '" + propertyInfo.Name + "' <" + propertyValue.GetType() + "> as it is not a ZDecimal!");
			}

			ZDecimal value = (ZDecimal)propertyValue;

			if (!IsValidMoney(value))
			{
				string errorField = GetHumanReadablePropertyName(propertyInfo);
				string error = Res.GetString("F82EE1F1-B8CB-4560-AB1A-84B6C958FBC2", "The number {0} is too large, the value's range of {1} is between {2} and {3}.",
					value.ToString("n" + GetNumberOfDecimals(value)), errorField, MinMoney, MaxMoney);
				propertyInfo.AddError(error);
				return;
			}

			CheckValidDecimal(propertyInfo, databasePrecision, databaseScale);
		}

		internal const decimal MaxMoney = 922337203685477.5807M;
		internal const decimal MinMoney = -922337203685477.5808M;

		public static bool IsValidMoney(ZDecimal value)
		{
			if (value >= MinMoney && value <= MaxMoney)
			{
				return true;
			}

			return false;
		}

		#endregion

		#region Implementation

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "We really want a 4 character year")]
		public static string ErrorForSmallDateTimeFuture(ZDateTime dateTime)
		{
			return Res.GetString("b0c64d29-2fb0-479f-9264-ebc10179808c", "The date '{0}' is later than '{1}', the limit for this field.",
				dateTime.ToString("dd-MMM-yyyy"), ZDateTime.MaxSmallDateTimeValue.ToString("dd-MMM-yyyy")); // We really want a 4 character year
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "We really want a 4 character year")]
		public static string ErrorForSmallDateTimePast(ZDateTime dateTime)
		{
			return Res.GetString("8edcc1cc-16ac-441f-8d39-033ac4bb8dba", "The date '{0}' is earlier than '{1}', the limit for this field.",
				dateTime.ToString("dd-MMM-yyyy"), ZDateTime.MinSmallDateTimeValue.ToString("dd-MMM-yyyy")); // We really want a 4 character year
		}

		static void CheckValid(ZPropertyInfo propertyInfo, ZString description)
		{
			var value = propertyInfo.Value;

			if (!((propertyInfo.IsNullable && value.IsEmpty) || value.IsValid))
			{
				if (description.IsEmpty)
				{
					description = GetHumanReadablePropertyName(propertyInfo);
				}

				string error = String.Format(InvalidTypeMessage, description);
				propertyInfo.AddError(error);
			}
		}

		#endregion
	}
}
