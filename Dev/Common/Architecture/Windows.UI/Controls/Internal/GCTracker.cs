using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CargoWise.BrandManager;
using CargoWise.Common;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.Windows.UI.Controls.Internal
{
	public class GCTracker
	{
		static readonly ConcurrentHashSet<string> LeakedKeys = new ConcurrentHashSet<string>(1, 1);
		static readonly ConcurrentHashSet<string> ReportedKeys = new ConcurrentHashSet<string>();

		readonly string key;
		readonly Control control;
		readonly StackTrace creationStack;

		GCTracker(string key, Control c, StackTrace stack)
		{
			this.key = key;
			control = c;
			creationStack = stack;
		}

		public static GCTracker Track(string key, Control c)
		{
			if (isTracking)
			{
				return LeakedKeys.Contains(key) && !ReportedKeys.Contains(key) ? new GCTracker(key, c, new StackTrace()) : new GCTracker(key, c, null);
			}

			return null;
		}

		public static void StopTracking() => isTracking = false;

		public static void StartTracking() => isTracking = true;

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "No particular synchronization needed here.")]
		static bool isTracking = true;

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", Justification = "Exception message")]
		public void NotifyDisposed(bool byFinaliser, bool reportToLogEvent = false)
		{
			if (!byFinaliser)
			{
				var noSystemShutdownThrown = !ErrorReporter.LastExceptionsReported()
					.Any(exceptionText => exceptionText.IndexOf(SqlExceptionName, StringComparison.InvariantCulture) >= 0 && exceptionText.IndexOf(ShutdownErrorMessageHeader, StringComparison.InvariantCulture) >= 0);
				var noNetworkOrAccessSQLExceptionThrown = !ErrorReporter.LastExceptionsReported()
					.Any(exceptionText => exceptionText.IndexOf(SqlExceptionName, StringComparison.InvariantCulture) >= 0 &&
					(exceptionText.Contains(NetworkSQLExceptionMessageHeader, StringComparison.InvariantCulture) || exceptionText.Contains(SQLAccessExceptionMessageHeader, StringComparison.InvariantCulture)));
				var noTCPConnectionFailureThrown = !ErrorReporter.LastExceptionsReported()
					.Any(exceptionText => exceptionText.IndexOf(SqlExceptionName, StringComparison.InvariantCulture) >= 0 && exceptionText.IndexOf(TCPConnectionFailureMessageHeader, StringComparison.InvariantCulture) >= 0);
				var noArgumentExceptionWithinSystemDrawingThrown = !ErrorReporter.LastExceptionsReported()
					.Any(exceptionText => exceptionText.IndexOf(typeof(ArgumentException).FullName, StringComparison.InvariantCulture) >= 0 && exceptionText.IndexOf("System.Drawing", StringComparison.InvariantCulture) >= 0);
				var reportLeaks = noSystemShutdownThrown && noNetworkOrAccessSQLExceptionThrown && noTCPConnectionFailureThrown && noArgumentExceptionWithinSystemDrawingThrown;

				if (isTracking && !LeakedKeys.TryAdd(key) && creationStack != null && ReportedKeys.TryAdd(key) && reportLeaks)
				{
					var stack = creationStack.ToString();
					var message = new StringBuilder();
					message.AppendLine(
					FormattableString.Invariant($@"A finalizer has been called on {control.Name} ({control.GetType().FullName})). This means dispose has not been called.
Any exception occurring during the construction of this object can lead to this kind of problems upon destruction. Therefore looking into Issues Manager for exceptions occurring in Constructors, for the same client and at a very close date and time, may help you in understanding this problem.
Tips: Check how this form or parent form of this control was created and disposed, if ShowDialog() was called without disposed, wrap the form object with a using() clause or replace it with ZFormModaliser.ShowDialogAndDispose().
Construction Stack: 
") + stack);

					if (control.GetType().ToString().Contains("ZMessageBox"))
					{
						message.AppendLine(FormattableString.Invariant($@"MessageBox message:
						{control.GetType().GetProperty("Message")?.GetValue(control)?.ToString()}"));
						message.AppendLine(FormattableString.Invariant($@"Form title:
						{(control as KForm)?.TextIncludingSuffix}"));
					}

					if (reportToLogEvent)
					{
						WriteToEventLog(message.ToString());
					}
					else
					{
						try
						{
							ErrorReporter.ReportOnce("FinalizerError:" + stack.GetHashCode(), message.ToString());
						}
						catch (Exception ex) when (!ex.IsCriticalException())
						{
							var messageEx = FormattableString.Invariant($"The following exception occurred when trying to report a finalizer being called:\r\n{ex}\r\n\r\nThe original exception details:\r\n{message.ToString()}"); // This is an error message
							WriteToEventLog(messageEx);
						}
					}
				}
			}
		}

		string SqlExceptionName => sqlExceptionName ?? (sqlExceptionName = typeof(SqlException).FullName);
		string sqlExceptionName;

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Message headers")]
		public const string ShutdownErrorMessageHeader = "SHUTDOWN is in progress.";
		public const string NetworkSQLExceptionMessageHeader = "A network-related or instance-specific error occurred while establishing a connection to SQL Server.";
		public const string SQLAccessExceptionMessageHeader = "Unable to access availability database";
		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Message headers")]
		public const string TCPConnectionFailureMessageHeader = "A connection was successfully established with the server, but then an error occurred during the pre-login handshake. (provider: TCP Provider, error: 0 - The specified network name is no longer available.)";

		const int MaxLogLength = 31718;

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Logging")]
		void WriteToEventLog(string message)
		{
			if (message.Length > MaxLogLength)
			{
				message = message.Substring(0, MaxLogLength - 4) + " ...";
			}

			new EventLog("Application") { Source = BrandingFactory.Instance.ProductName }.SafeWriteEntry(message, EventLogEntryType.Warning);
		}

#if DEBUG
		public static void ClearListForTest()
		{
			LeakedKeys.Clear();
			ReportedKeys.Clear();
		}

		public static bool IsTrackingForTest => isTracking;
#endif
	}
}
