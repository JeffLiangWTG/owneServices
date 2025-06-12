using System.Collections.Generic;
using System.IO;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Display;

namespace CargoWise.eHub.MessageEvent.KafkaProducerService.Logging
{
	public class IndentationMessageTemplateTextFormatter : ITextFormatter
	{
		private const string OutputTemplateMessageIDOnly = "{Timestamp:yyyy-MM-ddTHH:mm:ss.fffzzz} {eHubLogLevel} MessageID={MessageID} Source={SourceContext}{NewLine}MachineName={MachineName} ProcessID={ProcessId} ThreadID={ThreadId}{NewLine}{Message}{NewLine}{Exception}";

		private const string OutputTemplateActivityIDOnly = "{Timestamp:yyyy-MM-ddTHH:mm:ss.fffzzz} {eHubLogLevel} ActivityID={ActivityID} Source={SourceContext}{NewLine}MachineName={MachineName} ProcessID={ProcessId} ThreadID={ThreadId}{NewLine}{Message}{NewLine}{Exception}";

		private const string OutputTemplateNone = "{Timestamp:yyyy-MM-ddTHH:mm:ss.fffzzz} {eHubLogLevel} Source={SourceContext}{NewLine}MachineName={MachineName} ProcessID={ProcessId} ThreadID={ThreadId}{NewLine}{Message}{NewLine}{Exception}";

		private const string OutputTemplateBoth = "{Timestamp:yyyy-MM-ddTHH:mm:ss.fffzzz} {eHubLogLevel} MessageID={MessageID} ActivityID={ActivityID} Source={SourceContext}{NewLine}MachineName={MachineName} ProcessID={ProcessId} ThreadID={ThreadId}{NewLine}{Message}{NewLine}{Exception}";

		private static readonly Dictionary<string, MessageTemplateTextFormatter> templates;

		static IndentationMessageTemplateTextFormatter()
		{
			templates = new Dictionary<string, MessageTemplateTextFormatter>()
			{
				{ "MessageIdOnly", new MessageTemplateTextFormatter(OutputTemplateMessageIDOnly, null) },
				{ "ActivityIdOnly", new MessageTemplateTextFormatter(OutputTemplateActivityIDOnly, null) },
				{ "BothOfThem", new MessageTemplateTextFormatter(OutputTemplateBoth, null) },
				{ "NoneOfThem", new MessageTemplateTextFormatter(OutputTemplateNone, null) },
			};
		}

		public void Format(LogEvent logEvent, TextWriter output)
		{
			MessageTemplateTextFormatter template;

			var properties = logEvent.Properties;

			if (properties.ContainsKey("MessageID") && properties.ContainsKey("ActivityID"))
			{
				template = templates["BothOfThem"];
			}
			else if (properties.ContainsKey("MessageID") && !properties.ContainsKey("ActivityID"))
			{
				template = templates["MessageIdOnly"];
			}
			else if (!properties.ContainsKey("MessageID") && properties.ContainsKey("ActivityID"))
			{
				template = templates["ActivityIdOnly"];
			}
			else
			{
				template = templates["NoneOfThem"];
			}

			using (var stream = new MemoryStream())
			using (var writer = new StreamWriter(stream))
			{
				template.Format(logEvent, writer);
				writer.Flush();
				stream.Position = 0;

				using (var reader = new StreamReader(stream))
				{
					output.WriteLine(reader.ReadLine());

					string? line;
					while ((line = reader.ReadLine()) != null)
					{
						output.Write("   ");
						output.WriteLine(line);
					}
				}
			}
		}
	}

	public static class IndentationFormatterWrapper
	{
		public static ITextFormatter Formatter { get; } = new IndentationMessageTemplateTextFormatter();
	}
}
