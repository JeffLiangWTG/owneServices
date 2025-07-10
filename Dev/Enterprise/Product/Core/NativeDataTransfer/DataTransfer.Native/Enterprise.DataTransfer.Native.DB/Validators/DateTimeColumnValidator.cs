using System;
using CargoWise.Types;

namespace Enterprise.DataTransfer.Native.DB.Validators
{
	public class DateTimeColumnValidator : IColumnValidator
	{
		readonly string columnDataType;
		readonly bool allowNullOrEmpty;
		readonly ZDateTime minDateTimeValue;
		readonly ZDateTime maxDateTimeValue;

		public DateTimeColumnValidator(string dataType, bool nullable, ZDateTime minAcceptableDateTime, ZDateTime maxAcceptableDateTime)
		{
			allowNullOrEmpty = nullable;
			columnDataType = dataType;
			minDateTimeValue = minAcceptableDateTime;
			maxDateTimeValue = maxAcceptableDateTime;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "We don't have locale information for Native XML import, so keep errors in English")]
		public ValidationResult Validate(object value)
		{
			var success = false;
			var error = string.Empty;

			if (allowNullOrEmpty && IsNullValueOrEmptyString(value))
			{
				success = true;
			}
			else if (!allowNullOrEmpty && IsNullValueOrEmptyString(value))
			{
				success = false;
				error = "Field cannot be empty/null";
			}
			else if (value is string s)
			{
				if (ZDateTime.TryParseISO8601Date(s, out var zDt))
				{
					var result = ValidateZDateTime(zDt);
					success = result.Success;
					error = result.Error;
				}
				else
				{
					success = false;
					error = $"'{s}' could not be converted to type {columnDataType}";
				}
			}
			else if (value is ZDateTime zDt)
			{
				var result = ValidateZDateTime(zDt);
				success = result.Success;
				error = result.Error;
			}
			else if (value is DateTime dt)
			{
				var result = ValidateZDateTime(dt);
				success = result.Success;
				error = result.Error;
			}

			return new ValidationResult(success, error);
		}

		ValidationResult ValidateZDateTime(ZDateTime date)
		{
			var error = string.Empty;

			var success = date.IsValidSmallDateTime;
			if (!success)
			{
				var isDateType = columnDataType == DbDataType.Date;
				var minDateTimeStr = isDateType ? minDateTimeValue.ToDateTime().ToShortDateString() : minDateTimeValue.ToISO8601String();
				var maxDatetimeStr = isDateType ? maxDateTimeValue.ToDateTime().ToShortDateString() : maxDateTimeValue.ToISO8601String();
				var valueStr = isDateType ? date.ToISO8601ShortDateString() : date.ToISO8601String();

				error = $"'{valueStr}' is not within the range for {columnDataType} ({minDateTimeStr} - {maxDatetimeStr})"; // We don't have locale information for Native XML import, so keep errors in English
			}

			return new ValidationResult(success, error);
		}

		bool IsNullValueOrEmptyString(object value)
		{
			return value == null ||
				DBNull.Value.Equals(value) ||
				(value is string s && string.IsNullOrEmpty(s));
		}
	}
}
