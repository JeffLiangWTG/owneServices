using System;
using System.Collections.Generic;
using System.Diagnostics;
using ServiceManager.Integration.NudgingClient.Abstractions;

namespace ServiceManager.Integration.Abstractions
{
	public interface INudgingController
	{
		void ScheduleTasks(IEnumerable<string> taskCodes, bool? echoes = null, TimeSpan? delay = null);

		IEnumerable<IServiceTaskBinding> ServiceTaskBindings { get; }

		void ReportNudgeStarted(IEnumerable<string> taskCodes, StackTrace stackTrace);
		void ReportNudgeIgnored(IEnumerable<string> taskCodes, string description);
		void ReportNudgeFailed(IEnumerable<string> taskCodes, Exception ex, int retriesRemaining);
		void ReportNudgeFailed(IEnumerable<string> taskCodes, string description, int retriesRemaining);
		void ReportNudgeSucceeded(IEnumerable<string> taskCodes);
		void ReportNudgeDeferred(IEnumerable<string> taskCodes, string description);
		void ReportNudgeAbandoned(IEnumerable<string> taskCodes, string description);

		event EventHandler<NudgeFailedEventArgs> NudgeFailedEvent;
		event EventHandler<NudgeEventArgs> NudgeTrackingEvent;
	}
}
