using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using NtpDateTimeProvider;
using static System.FormattableString;

namespace Enterprise.DbHealth.Check
{
	class UtcTimeChecker : IChecker
	{
		public void Check(DbConnection connection, DbHealthWarningList warningList, ILogger logger)
		{
			DoDbServerUtcDateTimeCheck(connection, warningList, logger);
		}

		string IChecker.Description
		{
			get { return "Db Server UTC Time Checker"; }
		}

		void DoDbServerUtcDateTimeCheck(DbConnection connection, DbHealthWarningList warningList, ILogger logger)
		{
			DateTime? referenceUtc;
			DateTime lowerLimitDbServerUtc;
			var originalCount = warningList?.Count ?? 0;
			do
			{
				lowerLimitDbServerUtc = GetDbServerUtc();
				referenceUtc = GetReferenceUtc(warningList, logger);
			} while (!referenceUtc.HasValue && warningList?.Count == originalCount);

			var upperLimitDbServerUtc = GetDbServerUtc();

			if (referenceUtc.HasValue &&
				Math.Abs((referenceUtc.Value - lowerLimitDbServerUtc).TotalDays) < 1.0 &&
				Math.Abs((referenceUtc.Value - upperLimitDbServerUtc).TotalDays) < 1.0)
			{
				var allowedRefTimeDifference = GetAllowedRefTimeDifference(lowerLimitDbServerUtc, upperLimitDbServerUtc);

				if (referenceUtc.Value < lowerLimitDbServerUtc.AddMinutes(-allowedRefTimeDifference)
					|| referenceUtc.Value > upperLimitDbServerUtc.AddMinutes(allowedRefTimeDifference))
				{
					var description = string.Format(
						CultureInfo.InvariantCulture,
						"Database Server UTC Date/Time is incorrect [{0}]. Actual reference UTC = [{1}].",
						Env.Time.FormatDateTime(upperLimitDbServerUtc),
						Env.Time.FormatDateTime(referenceUtc.Value));
					var action =
						"Ensure the database server time is synchronized with the correct NTP servers and its time zone/daylight saving settings are configured correctly.\r\nNote: these are operating system settings (no action required in " + Constants.ProductName + ").";
					var warning = new DatabaseWarning(connection.ServerNameWithoutInstance, DatabaseWarning.IncorrectUtcTimeWarning, description, action);
					warningList.Add(warning);
				}
			}
		}

		internal virtual DateTime? GetReferenceUtc(DbHealthWarningList warningList, ILogger logger)
		{
			DateTime? utcResult = null;

			try
			{
				utcResult = GetUtcFromNTPService();
			}
			catch (Exception ntpException) when (!ntpException.IsCriticalException())
			{
				var genericMessage = "Cannot get current UTC Date/Time from any NTP servers.";
				logger.Log(LogType.Warning, genericMessage, ntpException);
				var message = Invariant($"{genericMessage}\r\n{ntpException.Message}");

				var isTimeout = ntpException.Message.Contains("did not properly respond after a period of time");
				if (isTimeout)
				{
					timeoutCount++;
				}
				if ((warningList != null && !isTimeout) || timeoutCount == 3)
				{
					var action = $"Please contact your IT administrator{(IsHosted ? string.Empty : " to ensure OS time is accurate.")} If NTP is required, ensure UDP traffic on port 123 is allowed and correct registry item is overwritten.";
					warningList.Add(new DatabaseWarning("UtcTimeChecker", "Cannot check UTC Time", message, action));
				}
			}

			return utcResult;
		}

		internal virtual bool IsHosted => EnvProxy.IsHostedWithCargowise;

		[ThreadStatic]
		static int timeoutCount;

		[Conditional("DEBUG")]
		public static void ResetTimeout() => timeoutCount = 0;

		internal virtual DateTime? GetUtcFromNTPService()
		{
			var servers = SystemDataRegistry.Instance.NtpTimeServers.Value;

			if (servers?.Length > 0)
			{
				for (var i = 0; i < servers.Length; i++)
				{
					try
					{
						var client = CreateNtpDateTimeClient(servers[i]);
						return client.UtcNow(true, TimeSpan.FromSeconds(15))?.DateTime;
					}
					catch (Exception ntpClientException) when (!ntpClientException.IsCriticalException())
					{
						// last server
						if (i == servers.Length - 1)
						{
							throw;
						}
					}
				}
			}
			else
			{
				return Env.Time.CurrentUtcDateTime;
			}

			return null;
		}

		internal virtual INtpDateTimeProvider CreateNtpDateTimeClient(string hostname)
		{
			return new NtpDateTimeClient(hostname);
		}

		internal virtual DateTime GetDbServerUtc()
		{
			return Env.Time.CurrentUtcDateTime;
		}

		int GetAllowedRefTimeDifference(DateTime lowerLimitDbServerUtc, DateTime upperLimitDbServerUtc)
		{
			var allowedRefTimeDifference = maxAllowedDifferenceInMinutes;

			var timeDelta = upperLimitDbServerUtc.Subtract(lowerLimitDbServerUtc);
			var timeDeltaInMinutes = (timeDelta.TotalMinutes < Int32.MaxValue) ? Convert.ToInt32(timeDelta.TotalMinutes) : Int32.MaxValue;

			if (timeDeltaInMinutes > 0)
			{
				allowedRefTimeDifference += timeDeltaInMinutes;
			}

			return allowedRefTimeDifference;
		}

		[SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		const int maxAllowedDifferenceInMinutes = 10;
	}
}
