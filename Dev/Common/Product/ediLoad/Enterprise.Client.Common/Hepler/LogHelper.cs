using System;
using System.Diagnostics;
using CargoWise.BrandManager;
using CargoWise.Common;

namespace Enterprise.Client.Common
{
	public static class LogHelper
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1069:Do Not Use EventLog.WriteEntry", Justification = "No reference to Enterprise.ZArchitecture.Core.dll")]
		internal static void WriteEventLog(string logMessage, EventLogEntryType logEntryType)
		{
			try
			{
				using (var eventLog = new EventLog { Source = BrandingFactory.Instance.ProductName })
				{
					eventLog.WriteEntry(logMessage, logEntryType);
				}
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
			}
		}
	}
}
