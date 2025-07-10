using System;
using CargoWise.Common;

namespace Enterprise.RemotePrinting.Server.RPSCore
{
	public static class ErrorReportSender
	{
		public static void TrySend(string message, Exception ex)
		{
			Argument.NotNull(ex, nameof(ex));

			ErrorReporter.ReportOnce(message, ex);
		}
	}
}
