using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Security;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.Core
{
	public static class SafeEventLogExtensions
	{
		public static void SafeWriteEntryToApplicationLog(string message, EventLogEntryType entryType, bool ignoreAllExceptions = false)
		{
			SafeWriteEntryToApplicationLog(GetEventSource(), message, entryType, ignoreAllExceptions);
		}

		public static void SafeWriteEntryToApplicationLog(string source, string message, EventLogEntryType entryType, bool ignoreAllExceptions = false)
		{
			var eventLog = new EventLog { Source = source };

			if (string.IsNullOrEmpty(source))
			{
				eventLog.Source = GetEventSource();
			}

			eventLog.SafeWriteEntry(message, entryType, ignoreAllExceptions);
		}

		[SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "This is for the default source of an event log if the real one can't be determined.")]

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Default event log source if the real one can't be determined.")]
		static string GetEventSource()
		{
			return string.IsNullOrWhiteSpace(Constants.ProductName)
				? "CargoWise Next"
				: Constants.ProductName;
		}

		[SuppressMessage("CargoWiseOne", "CW1069:DoNotUseEventLogEventWrite", Justification = "This is the safe extension method recommended by this analyser")]
		public static void SafeWriteEntry(this EventLog eventLog, string message, EventLogEntryType entryType, bool ignoreAllExceptions = false)
		{
			try
			{
				const int maxLogLength = 31839;
				if (message.Length > maxLogLength)
				{
					message = message.Substring(0, maxLogLength);
				}
				if (string.IsNullOrEmpty(eventLog.Source))
				{
					eventLog.Source = GetEventSource();
				}
				eventLog.WriteEntry(message, entryType);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				if (!ignoreAllExceptions && (!IsProblemWithEventLog(ex) || !NotifyControllers(UserInformationString + message, ex)))
				{
					Globals.Message.ShowDeveloperException(ex);
				}
			}
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		static string UserInformationString
		{
			get
			{
				return $@"Current CW1 Login User: {EnvProxy.Instance?.CurrentUser?.LoginName}
Current Domain User: {System.Environment.UserDomainName}\{System.Environment.UserName}
"; // Default event log source if the real one can't be determined.
			}
		}

		static bool IsProblemWithEventLog(Exception ex)
		{
			bool isProblemWithEventLog = false;
			Win32Exception win32Exception = ex as Win32Exception ?? ex.InnerException as Win32Exception;
			if (win32Exception != null)
			{
				switch (win32Exception.NativeErrorCode)
				{
					case 5:    // Access is denied.
					case 31:   // A device attached to the system is not functioning
					case 87:   // The parameter is incorrect
					case 1500: // The event log file is corrupted.
					case 1501: // The event log file could not be opened, the registration of events did not start.
					case 1502: // The event log file is full.
					case 1503: // The event log file has changed between read operations
					case 1717: // The interface is unknown.
					case 1722: // The RPC server is unavailable
						isProblemWithEventLog = true;
						break;
				}
			}
			else if (ex is SecurityException)
			{
				isProblemWithEventLog = true;
			}

			return isProblemWithEventLog;
		}

		[SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
#if DEBUG
		[SuppressMessage("CargoWiseOne", "CW1022:ThreadStaticSetInStaticInitializerRule", Justification = "Baseline issue")]
		[ThreadStatic]
		internal static
#else
		const
#endif
 int timeoutInMinutes = 60;

#if DEBUG
		internal
#endif
		static Dictionary<string, ZDateTime> NotifiedExceptions
		{
			get
			{
				return notifiedExceptions ?? (notifiedExceptions = new Dictionary<string, ZDateTime>());
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1022:ThreadStaticSetInStaticInitializerRule", Justification = "Baseline issue")]
		[ThreadStatic]
		static Dictionary<string, ZDateTime> notifiedExceptions = null;

#if DEBUG
		internal static bool useTestEmail;
#endif

		static StringCollection StaffToNotify()
		{
#if DEBUG
			if (useTestEmail)
			{
				var stringCollection = new StringCollection();
				stringCollection.Add("a@b");
				return stringCollection;
			}
#endif
			if (DataRegistry.Instance.InfrastructureErrorsNotificationGroup != Guid.Empty)
			{
				var result = new EmailGroupUtility().GetGroupEmailCollection(DataRegistry.Instance.InfrastructureErrorsNotificationGroup, false);
				if (result.Count > 0)
				{
					return result;
				}
			}

			return new EmailGroupUtility().AdminStaff;
		}

#if DEBUG
		internal
#endif
 static bool NotifyControllers(string message, Exception ex)
		{
			#region SuppressResourceStringsCheckRegion

			var staffToNotify = StaffToNotify();
			if (staffToNotify.Count == 0)
			{
				return false;
			}

			string key = ex.GetType().ToString() + ex.Message;
			ZDateTime lastTimeOfException;
			if (NotifiedExceptions.TryGetValue(key, out lastTimeOfException))
			{
				if (ZDateTime.Now - lastTimeOfException > new TimeSpan(0, timeoutInMinutes, 0))
				{
					NotifiedExceptions.Remove(key);
				}
				else
				{
					return true;
				}
			}
			NotifiedExceptions.Add(key, ZDateTime.Now);

			var emailDef = new EmailDef
			{
				FromDisplayName = string.Format(CultureInfo.InvariantCulture, "{0} ({1})", Constants.ProductName, System.Environment.MachineName),
				Subject = "Failed to write to Windows Event Log",
				Body = string.Format(
	@"Failed to write to the Windows Event Log
Machine: {0}
Error message: {1}
Event log message: {2}", System.Environment.MachineName, ex.ToString(), message),
			};

			emailDef.AddRecipientForUserCommunication(staffToNotify);
			EnvProxy.Instance.OutgoingMailManager.CreateAndSave(emailDef);

			return true;

			#endregion
		}
	}
}
