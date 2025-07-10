using System;
using CargoWise.Application;
using CargoWise.Data;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Licensing
{
	public class LicenceExpiryCheck
	{
		public static LicenceExpiryCheck Create()
		{
			return new LicenceExpiryCheck();
		}

		LicenceExpiryCheck()
		{
			registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			if (registrationKey.DatabaseType == DatabaseTypes.Codes.Training ||
				registrationKey.DatabaseType == DatabaseTypes.Codes.Education ||
				registrationKey.DatabaseType == DatabaseTypes.Codes.Demo)
			{
				daysToExpiry = double.MaxValue;
			}
			else
			{
				expiryDateTime = GetExpiryDate(registrationKey.SystemExpiryDate);
				TimeSpan timeToExpiry = expiryDateTime - EnvProxy.Instance.Time.CurrentLocalDateTime;
				daysToExpiry = timeToExpiry.TotalDays;
			}
		}

		readonly IProductRegistrationKey registrationKey;
		readonly DateTime expiryDateTime;
		readonly double daysToExpiry;

		public DateTime ExpiryDateTime
		{
			get { return expiryDateTime; }
		}

		public double DaysToExpiry
		{
			get { return daysToExpiry; }
		}

		public bool SystemHasExpired
		{
			get { return daysToExpiry <= 0; }
		}

		public string StabilityErrorMessage
		{
			get
			{
				string result;

				result = GetCustomExpiryMessage();
				if (!string.IsNullOrWhiteSpace(result))
				{
					result = result.Replace("{ExpiryDate}", expiryDateTime.ToLongDateString());
				}
				else
				{
					result = GetDefaultExpiryMessage();
				}

				return result;
			}
		}

		string GetCustomExpiryMessage()
		{
			string result;
			if (daysToExpiry <= 0)
			{
				result = registrationKey.ExpiredMessage;
			}
			else if (daysToExpiry <= 7)
			{
				result = registrationKey.ExpiryWeekMessage;
			}
			else
			{
				result = registrationKey.ExpiryMonthMessage;
			}
			return result;
		}

		string GetDefaultExpiryMessage()
		{
			return
				String.Format("Your database is no longer in sync with WiseTech Global. All users will be locked out of your system {0}. "
				+ "Database Name: {1}, Server Name: {2}\r\n"
				+ "This indicates that your Process Controller is not running or that it is blocked from reaching the WiseTech Global registration web service.\r\n"
				+ (SystemHasExpired ? "You can login to diagnose this using a login with Non-Operational permissions. " : "")
				+ "Please access Maintain > System > Service Tasks to check the Process Controller(s). "
				+ "Please access Help > Register/Unregister Product to check the registration.",
				(SystemHasExpired ? "until this is resolved" : "starting " + expiryDateTime.ToLongDateString()), Db.DatabaseName, Db.ServerName
				);
		}

		static DateTime GetExpiryDate(DateTime keyValueDate)
		{
			DateTime expiryDate = (keyValueDate.DayOfWeek == DayOfWeek.Saturday || keyValueDate.DayOfWeek == DayOfWeek.Sunday ? GetNextMondayMidnight(keyValueDate) : keyValueDate);
			expiryDate = expiryDate.AddHours(ExpiryHourOfDay);
			return expiryDate;
		}

		static DateTime GetNextMondayMidnight(DateTime dateTime)
		{
			switch (dateTime.DayOfWeek)
			{
				case DayOfWeek.Saturday:
					dateTime = dateTime.AddDays(2);
					break;

				case DayOfWeek.Sunday:
					dateTime = dateTime.AddDays(1);
					break;

				default:
					return dateTime;
			}

			return dateTime.Date;
		}

		const int ExpiryHourOfDay = 16;
	}
}
