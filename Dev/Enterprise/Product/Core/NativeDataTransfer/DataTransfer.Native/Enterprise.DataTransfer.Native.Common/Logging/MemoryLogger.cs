using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Integration;

namespace Enterprise.DataTransfer.Native.Common.Logging
{
	public class MemoryLogger : ILogger, ILogBufferProvider
	{
		#region Constructor

		public MemoryLogger()
		{
			buffer = new List<Log>();
		}

		#endregion

		#region SyncRoot

		object syncRoot;
		object SyncRoot
		{
			get
			{
				if (syncRoot == null)
				{
					Interlocked.CompareExchange(ref syncRoot, new object(), null);
				}

				return syncRoot;
			}
		}

		#endregion

		readonly List<Log> buffer;

		public void Clear()
		{
			buffer.Clear();
		}

		public ILogBuffer Buffer
		{
			get { return new LogBuffer(buffer); }
		}

		#region ILogger Members

		public void Log(Log log)
		{
			lock (SyncRoot)
			{
				buffer.Add(log);
			}
		}

		public void Log(LogType type, string message)
		{
			lock (SyncRoot)
			{
				Log(new Log(message, type));
			}
		}

		public void Log(LogType type, string message, Exception ex)
		{
			lock (SyncRoot)
			{
				Log(new Log(message, type));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe use")]
		public void LogOrReportException(Exception ex, string requestBody, AncillaryImportServices sessionServices)
		{
			var dataEx = new [] { ex }.Concat(ex.SelectRecursive(f => new [] {  f.InnerException }.WhereNotNull()))
				.OfType<ZDataException>()
				.FirstOrDefault();
			if (dataEx != null)
			{
				var friendlyMessage = string.Empty;
				try
				{
					friendlyMessage = dataEx.FriendlyMessage;
					if (friendlyMessage.Contains("The value of Organization Code must be unique on Organization"))
					{
						var stringToFind = "The duplicate value(s) are: (";
						var startIndex = friendlyMessage.IndexOf(stringToFind, StringComparison.OrdinalIgnoreCase) + stringToFind.Length;
						var length = friendlyMessage.IndexOf(")", startIndex, StringComparison.OrdinalIgnoreCase) - startIndex;
						var localCode = friendlyMessage.Substring(startIndex, length);
						if (sessionServices.OrganisationLocalToForeignCodeMappings.TryGetValue(localCode, out var foreignCode)) // Guid.Empty is a special case storing code mapping details.
						{
							friendlyMessage = friendlyMessage.Replace(localCode, FormattableString.Invariant($"{foreignCode}"));
						}
					}
				}
				catch (Exception errorMessageGenerationException) when (!errorMessageGenerationException.IsCriticalException())
				{
				}

				if (!string.IsNullOrEmpty(friendlyMessage))
				{
					Log(new Log(friendlyMessage, LogType.Error));
					return;
				}
			}
			else if (ex is SqlException && ex.Message.Equals("The conversion of a varchar data type to a smalldatetime data type resulted in an out-of-range value."))
			{
				LogKnownException(ex);
				return;
			}

			if (ExceptionVisibilityAttribute.GetFirstOccurenceOfUserException(ex) != null)
			{
				LogKnownException(ex);
			}
			else
			{
				var sb = new StringBuilder();
				sb.AppendLine(ex.Message);
				sb.AppendLine("This error has been submitted to WTG for further investigation.");
				sb.Append(System.Environment.NewLine);
				sb.AppendLine(ex.StackTrace);

				Log(new Log(sb.ToString(), LogType.Error));

				ErrorReporter.ReportOnce(string.Format(CultureInfo.InvariantCulture, "Unknown exception occurred while importing Native XML, If you are a developer looking at the issue (yes you!!) please handle the exception.\r\n\r\nIf this exception occured because of a problem with the incoming XML, please wrap this exception in an exception type that has the ExceptionVisibility.User attribute on it. (eg: NativeXMLUserVisibleException) That will cause the exception to be reported to the User instead of being reported to WTG as an Issue. Make sure that you provide a clear message for the new exception that a User can follow to understand and fix the processing error that has occurred.\r\n{0}", requestBody), ex);
			}
		}

		void LogKnownException(Exception ex)
		{
			Log(new Log(ex.Message, LogType.Error));

			if (ex.InnerException != null)
			{
				LogKnownException(ex.InnerException);
			}
		}
		#endregion
	}

	public interface ILogBufferProvider
	{
		ILogBuffer Buffer { get; }
	}
}
