using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using NLog;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	public class NLogWrapperForTest : NLogWrapper
	{
		public NLogWrapperForTest(Type type) : base(type)
		{
			Init();
		}

		static readonly string GuidPattern = @"\b[A-Fa-f0-9]{8}[-][A-Fa-f0-9]{4}[-][A-Fa-f0-9]{4}[-][A-Fa-f0-9]{4}[-][A-Fa-f0-9]{12}\b";
		static readonly Regex GuidRegex = new Regex(GuidPattern);

		protected void Init()
		{
			var config = new NLog.Config.LoggingConfiguration();
			memoryTarget = new NLog.Targets.MemoryTarget();
			memoryTarget.Layout = "${level} | ${logger}: ${message} | ${event-properties:status_code} | ${event-properties:session_id} | ${event-properties:user_id} | ${event-properties:routing_path} | ${event-properties:product} | ${event-properties:system_id} | ${event-properties:tenant_id}";
			config.AddRuleForAllLevels(memoryTarget);
			LogManager.Configuration = config;
		}

		public void ClearLog()
		{
			memoryTarget.Logs.Clear();
		}

		public override string ToString()
		{
			var builder = new StringBuilder();
			foreach (string log in memoryTarget.Logs)
			{
				builder.AppendLine(log);
			}
			var actualMessages = builder.ToString().TrimEnd('\r', '\n');
			return actualMessages;
		}

		public bool ContainsLogMessages(string expectedLogMessage, bool needCheckContaionSessionId)
		{
			var builder = new StringBuilder();
			foreach (var log in memoryTarget.Logs)
			{
				builder.AppendLine(log);
			}
			var actualMessages = builder.ToString().TrimEnd('\r', '\n');
			var containsMsg = actualMessages.Contains(expectedLogMessage);

			if (!containsMsg)
			{
				return containsMsg;
			}

			if (needCheckContaionSessionId)
			{
				return GuidRegex.IsMatch(actualMessages);
			}

			return containsMsg;
		}

		public bool ContainsLogMessages(string[] expectedLogMessages, bool needCheckContaionSessionId)
		{
			var builder = new StringBuilder();
			foreach (var log in memoryTarget.Logs)
			{
				builder.AppendLine(log);
			}
			var actualMessages = builder.ToString().TrimEnd('\r', '\n');

			var containsMsg = true;
			foreach (var log in expectedLogMessages)
			{
				if (!containsMsg)
				{
					break;
				}
				containsMsg = actualMessages.Contains(log);
			}

			if (!containsMsg)
			{
				return containsMsg;
			}

			if (needCheckContaionSessionId)
			{
				return GuidRegex.IsMatch(actualMessages);
			}

			return containsMsg;
		}

		public IList<string> LogEntries => memoryTarget.Logs;

		NLog.Targets.MemoryTarget memoryTarget;
	}
}
