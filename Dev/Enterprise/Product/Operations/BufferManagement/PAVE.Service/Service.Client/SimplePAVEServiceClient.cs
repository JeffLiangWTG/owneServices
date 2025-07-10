using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.BufferManagement.Service.Shared;
using Enterprise.Integration;

namespace Enterprise.BufferManagement.Service.Client
{
	public abstract class SimplePAVEServiceClient : IPAVEService
	{
		void IPAVEService.Process(IEnumerable<Guid> processedPKs, ILogger logger, string callerMemberName)
		{
			Process(processedPKs, logger);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Log information")]
		protected void Process(IEnumerable<Guid> processedPKs, ILogger logger)
		{
			var pks = processedPKs?.ToList();
			if (pks == null || !pks.Any())
			{
				Log(logger, "No PKs to process");
				return;
			}

			ProcessCore(pks, logger);
		}

		protected abstract void ProcessCore(IReadOnlyCollection<Guid> processedPKs, ILogger logger);

		protected void Log(ILogger logger, string message, Exception ex = null)
		{
			message = FormattableString.Invariant($"{GetType().Name}|{message}"); // System Notification

			if (ex == null)
			{
				logger.Information(message);
			}
			else
			{
				logger.ErrorAndReportException(message, ex);
			}
		}
	}
}
