using System.Globalization;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public class DateRangeValidation
	{
		public DateRangeValidation() : this(null) { }

		public DateRangeValidation(TypeValidationLimits limits)
		{
			this.Limits = limits ?? TypeValidationLimits.Default;
		}
		readonly TypeValidationLimits Limits;

		public static string ErrorForPastYear(string dateTimeString, int pastYearsBeforeError)
		{
			return Res.GetString("8a1be523-1b7c-490e-8e4b-48593ebf100b", "The date '{0}' is more than {1} years old and thus is not valid.", dateTimeString, pastYearsBeforeError);
		}

		public static string ErrorForFutureYear(string dateTimeString, int futureYearsBeforeError)
		{
			return Res.GetString("272eb316-ed25-4aed-9559-0efb2b7ce5e0", "The date '{0}' is more than {1} years from now and thus is not valid.", dateTimeString, futureYearsBeforeError);
		}

		public static string WarningForPastYear(string dateTimeString, int pastYearsBeforeWarning)
		{
			if (pastYearsBeforeWarning == 0)
			{
				return Res.GetString("8826c20f-bf26-4e2b-a972-116fee9a7cb7", "The date '{0}' should be in future.", dateTimeString);
			}
			else if (pastYearsBeforeWarning == 1)
			{
				return Res.GetString("3b112527-da9d-48c9-8dbe-7fa887dd63bf", "The date '{0}' is more than {1} year old.", dateTimeString, pastYearsBeforeWarning);
			}
			else
			{
				return Res.GetString("1ef68b1c-87c7-4b88-9987-d07ab0b3699b", "The date '{0}' is more than {1} years old.", dateTimeString, pastYearsBeforeWarning);
			}
		}

		public static string WarningForFutureYear(string dateTimeString, int futureYearsBeforeWarning)
		{
			if (futureYearsBeforeWarning > 1)
			{
				return Res.GetString("28845C22-D0A7-4444-8DA6-AED6720FA89B", "The date '{0}' is more than {1} years from now.", dateTimeString, futureYearsBeforeWarning);
			}
			else
			{
				return Res.GetString("2aef8432-ac89-44ff-bf23-edc82c4c4716", "The date '{0}' is more than {1} year from now.", dateTimeString, futureYearsBeforeWarning);
			}
		}

		public void Validate(ZPropertyInfo propertyInfo, bool needCheckFutureYear = true, bool needCheckPastYear = true)
		{
			if (!propertyInfo.BizObj.IsInDatabase || !propertyInfo.OriginalValue.Equals(propertyInfo.Value))
			{
				ValidateDateValueHasChanged(propertyInfo, needCheckFutureYear, needCheckPastYear);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "We really want a 4 character year")]
		public void ValidateDateValueHasChanged(ZPropertyInfo propertyInfo, bool needCheckFutureYear = true, bool needCheckPastYear = true)
		{
			var propertyValue = propertyInfo.Value;
			ZDateTime propertyDate = (propertyValue is ZDateTime)
				? (ZDateTime)propertyValue : (propertyValue is ZDateTimeOffset)
				? ((ZDateTimeOffset)propertyValue).ToZDateTime()
				: (ZDate)propertyValue;

			if (propertyDate.IsValid && !propertyDate.IsEmpty)
			{
				ZDateTime today = ZDateTime.Today;

				const string DATE_FORMAT = "dd-MMM-yyyy"; // We really want a 4 character year

				NotificationTypes notificationTypeForPastYear = GetNotificationTypeAfterCheckingValidPastYear(propertyDate, today);
				NotificationTypes notificationTypeForFutureYear = NotificationTypes.None;
				if (needCheckFutureYear)
				{
					notificationTypeForFutureYear = GetNotificationTypeAfterCheckingValidFutureYear(propertyDate, today);
				}

				if (needCheckPastYear && notificationTypeForPastYear == NotificationTypes.Error)
				{
					propertyInfo.AddError(ErrorForPastYear(propertyDate.ToString(DATE_FORMAT, CultureInfo.InvariantCulture), Limits.PastYearsBeforeError));
				}
				else if (notificationTypeForFutureYear == NotificationTypes.Error)
				{
					propertyInfo.AddError(ErrorForFutureYear(propertyDate.ToString(DATE_FORMAT, CultureInfo.InvariantCulture), Limits.FutureYearsBeforeError));
				}
				else if (notificationTypeForPastYear == NotificationTypes.Warning)
				{
					propertyInfo.AddWarning(WarningForPastYear(propertyDate.ToString(DATE_FORMAT, CultureInfo.InvariantCulture), Limits.PastYearsBeforeWarning));
				}
				else if (notificationTypeForFutureYear == NotificationTypes.Warning)
				{
					propertyInfo.AddWarning(WarningForFutureYear(propertyDate.ToString(DATE_FORMAT, CultureInfo.InvariantCulture), Limits.FutureYearsBeforeWarning));
				}
			}
		}

		public static int MaximumFutureYears
		{
			get { return new ZDateTime(ZDateTime.MaxSmallDateTime.Ticks - ZDateTime.UtcNow.Ticks).Year; }
		}

		public static int MaximumPastYears
		{
			get { return new ZDateTime(ZDateTime.UtcToday.Ticks - ZDateTime.MinSmallDateTimeValue.Ticks).Year; }
		}

		internal bool IsWithinValidZDateTimeOffsetRangeWithoutError(ZDateTimeOffset dateTimeOffset)
		{
			if (!dateTimeOffset.IsValid)
			{
				return false;
			}

			return IsWithinValidZDateTimeRangeWithoutError(dateTimeOffset.ToDateTime());
		}

		internal bool IsWithinValidZDateTimeRangeWithoutError(ZDateTime dateTime)
		{
			ZDateTime today = ZDateTime.Today;
			return GetNotificationTypeAfterCheckingValidPastYear(dateTime, today) != NotificationTypes.Error &&
				GetNotificationTypeAfterCheckingValidFutureYear(dateTime, today) != NotificationTypes.Error;
		}

		NotificationTypes GetNotificationTypeAfterCheckingValidFutureYear(ZDateTime dateTime, ZDateTime today)
		{
			NotificationTypes result = NotificationTypes.None;

			if (dateTime > today.AddYears(Limits.FutureYearsBeforeError))
			{
				result = NotificationTypes.Error;
			}
			else if (dateTime > today.AddYears(Limits.FutureYearsBeforeWarning))
			{
				result = NotificationTypes.Warning;
			}

			return result;
		}

		NotificationTypes GetNotificationTypeAfterCheckingValidPastYear(ZDateTime dateTime, ZDateTime today)
		{
			NotificationTypes result = NotificationTypes.None;

			if (dateTime < today.AddYears(-Limits.PastYearsBeforeError))
			{
				result = NotificationTypes.Error;
			}
			else if (dateTime < today.AddYears(-Limits.PastYearsBeforeWarning))
			{
				result = NotificationTypes.Warning;
			}

			return result;
		}
	}
}
