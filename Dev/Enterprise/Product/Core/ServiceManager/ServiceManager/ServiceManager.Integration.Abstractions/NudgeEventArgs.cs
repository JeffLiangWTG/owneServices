using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace ServiceManager.Integration.Abstractions
{
	public abstract class NudgeEventArgs : EventArgs
	{
		public IEnumerable<string> TaskCodes { get; private set; }
		public abstract string Description { get; }
		public abstract string EventType { get; }
		public int RetriesRemaining { get; private set; }

		protected NudgeEventArgs(IEnumerable<string>? taskCodes, int retriesRemaining = 0)
		{
			TaskCodes = taskCodes ?? Array.Empty<string>();
			RetriesRemaining = retriesRemaining;
		}

		public override string ToString()
		{
			var retries = (RetriesRemaining > 0) ? string.Format(CultureInfo.InvariantCulture, " (retries remaining = {0})", RetriesRemaining) : string.Empty;
			return string.Format(CultureInfo.InvariantCulture, "Nudge of Task(s)='{0}' {1}{2}. {3}", string.Join(",", TaskCodes), EventType, retries, Description);
		}
	}

	public class NudgeSucceededEventArgs : NudgeEventArgs
	{
		public NudgeSucceededEventArgs(IEnumerable<string> taskCodes)
			: base(taskCodes)
		{
		}

		public override string Description => string.Empty;
		public override string EventType => "succeeded";
	}

	public class NudgeStartedEventArgs : NudgeEventArgs
	{
		public NudgeStartedEventArgs(IEnumerable<string> taskCodes, StackTrace? stackTrace)
			: base(taskCodes)
		{
			this.stackTrace = stackTrace;
		}

		readonly StackTrace? stackTrace;

		public override string Description => (stackTrace != null) ? stackTrace.ToString() : string.Empty;
		public override string EventType => "started";
	}

	public class NudgeIgnoredEventArgs : NudgeEventArgs
	{
		public override string Description => description;
		public override string EventType => "ignored";

		readonly string description;

		public NudgeIgnoredEventArgs(IEnumerable<string>? taskCodes, string description)
			: base(taskCodes)
		{
			this.description = description;
		}
	}

	public class NudgeFailedEventArgs : NudgeEventArgs
	{
		public Exception? Exception { get; private set; }
		public override string Description => (Exception != null) ? Exception.ToString() : description!;

		public override string EventType => "failed";

		public NudgeFailedEventArgs(IEnumerable<string>? taskCodes, Exception? ex, int retriesRemaining) : base(taskCodes, retriesRemaining)
		{
			Exception = ex ?? throw new ArgumentNullException(nameof(ex));
		}

		public NudgeFailedEventArgs(IEnumerable<string> taskCodes, string description, int retriesRemaining) : base(taskCodes, retriesRemaining)
		{
			this.description = description ?? throw new ArgumentNullException(nameof(description));
		}

		readonly string? description;
	}

	public class NudgeDeferredEventArgs : NudgeEventArgs
	{
		public NudgeDeferredEventArgs(IEnumerable<string> taskCodes, string description)
			: base(taskCodes)
		{
			this.description = description;
		}

		readonly string description;

		public override string Description => description;
		public override string EventType => "deferred";
	}

	public class NudgeAbandonedEventArgs : NudgeEventArgs
	{
		public NudgeAbandonedEventArgs(IEnumerable<string> taskCodes, string description)
			: base(taskCodes)
		{
			this.description = description;
		}

		readonly string description;

		public override string Description => description;
		public override string EventType => "abandoned";
	}
}
