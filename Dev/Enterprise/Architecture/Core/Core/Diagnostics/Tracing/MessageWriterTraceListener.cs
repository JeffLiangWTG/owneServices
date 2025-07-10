using System;
using System.Diagnostics;

namespace Enterprise.ZArchitecture.Core.Diagnostics
{
	public class MessageWriterTraceListener : TraceListener
	{
		public MessageWriterTraceListener(IMessageWriter messageWriter, string traceFilter, Func<string> getMessagePrefix) : base()
		{
			MessageWriter = messageWriter ?? throw new ArgumentNullException(nameof(messageWriter));
			TraceFilter = traceFilter ?? throw new ArgumentNullException(nameof(traceFilter));
			GetMessagePrefix = getMessagePrefix;
			Counter = 1;
		}

		public int Counter { get; private set; }

		public string TraceFilter { get; private set; }

		public Func<string> GetMessagePrefix { get; private set; }

		public override void Write(string message)
		{
			var prefix = GetMessagePrefix != null ? $"{System.Environment.NewLine}{GetMessagePrefix.Invoke()}" : ""; // Diagnostic information.
			MessageWriter.WriteMessage(FormattableString.Invariant($"{prefix}{System.Environment.NewLine}{message}")); // Diagnostic information.
			Counter++;
		}

		public override void WriteLine(string message) => MessageWriter.WriteMessage($"{message}{System.Environment.NewLine}"); // Diagnostic information.

		readonly IMessageWriter MessageWriter;
	}
}
