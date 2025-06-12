using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CargoWise.eHub.MessageEvent.KafkaProducerService.Logging;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace CargoWise.eHub.MessageEvent.KafkaProducerService.Tests.Logging
{
	public class TestLog
	{
		private StringWriter _writer;
		private Regex _timeStampRex = new Regex(@"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{3}[\+-]\d{2}:\d{2}$", RegexOptions.Compiled);

		private ILogger _logger;

		[SetUp]
		public void SetUp()
		{
			Log.Logger = new LoggerConfiguration()
				.MinimumLevel.Verbose()
				.Enrich.FromLogContext()
				.Enrich.WithMachineName()
				.Enrich.WithProcessId()
				.Enrich.WithThreadId()
				.Enrich.With<LogLevelEnricher>()
				.Enrich.With<ContextSourceReplaceEnricher>()
				.WriteTo.Console(formatter: new IndentationMessageTemplateTextFormatter())
				.CreateBootstrapLogger();

			var loggerFactory = LoggerFactory.Create(builder =>
			{
				builder.AddSerilog();
			});

			_logger = loggerFactory.CreateLogger<TestLog>();
			_writer = new StringWriter();
			Console.SetOut(_writer);
		}

		[Test]
		public void TestHeader()
		{
			_writer.GetStringBuilder().Clear();
			_logger.eHubLog(LogLevel.Debug, "MESSAGE_ID", "ACTIVITY_ID", "", "This is Message");
			CheckHeader(_writer.ToString(), "DBG", "MESSAGE_ID", "ACTIVITY_ID", typeof(TestLog).FullName);

			_writer.GetStringBuilder().Clear();
			_logger.eHubLog(LogLevel.Debug, "MESSAGE_ID", "", "", "This is Message");
			CheckHeader(_writer.ToString(), "DBG", "MESSAGE_ID", "", typeof(TestLog).FullName);


			_writer.GetStringBuilder().Clear();
			_logger.eHubLog(LogLevel.Debug, "", "ACTIVITY_ID", "", "This is Message");
			CheckHeader(_writer.ToString(), "DBG", "", "ACTIVITY_ID", typeof(TestLog).FullName);

			_writer.GetStringBuilder().Clear();
			_logger.eHubLog(LogLevel.Debug, "", "", "", "This is Message");
			CheckHeader(_writer.ToString(), "DBG", "", "", typeof(TestLog).FullName);

		}

		private void CheckHeader(string msg, string level, string messageId, string activityId, string source)
		{
			var message = new Message(msg, 1);
			Assert.That(_timeStampRex.IsMatch(message.MsgHeader.Timestamp), Is.True);
			Assert.That(message.MsgHeader.Level, Is.EqualTo(level));

			if (string.IsNullOrEmpty(messageId))
			{
				Assert.That(string.IsNullOrEmpty(message.MsgHeader.MessageId), Is.True);
			}
			else
			{
				Assert.That(message.MsgHeader.MessageId, Is.EqualTo("MessageID=" + messageId));
			}

			if (string.IsNullOrEmpty(activityId))
			{
				Assert.That(string.IsNullOrEmpty(message.MsgHeader.ActivityId), Is.True);
			}
			else
			{
				Assert.That(message.MsgHeader.ActivityId, Is.EqualTo("ActivityID=" + activityId));
			}
			Assert.That(message.MsgHeader.Source, Is.EqualTo("Source=" + source));

			Regex headerRex;
			if (string.IsNullOrEmpty(messageId) && string.IsNullOrEmpty(activityId))
			{
				headerRex = new Regex(@"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{3}[\+-]\d{2}:\d{2} (TRC|DBG|INF|WRN|ERR|FAL) Source=\S+$", RegexOptions.Compiled);
			}
			else if (!string.IsNullOrEmpty(messageId) && string.IsNullOrEmpty(activityId))
			{
				headerRex = new Regex(@"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{3}[\+-]\d{2}:\d{2} (TRC|DBG|INF|WRN|ERR|FAL) MessageID=\S+ Source=\S+$", RegexOptions.Compiled);
			}
			else if (string.IsNullOrEmpty(messageId) && !string.IsNullOrEmpty(activityId))
			{
				headerRex = new Regex(@"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{3}[\+-]\d{2}:\d{2} (TRC|DBG|INF|WRN|ERR|FAL) ActivityID=\S+ Source=\S+$", RegexOptions.Compiled);
			}
			else
			{
				headerRex = new Regex(@"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{3}[\+-]\d{2}:\d{2} (TRC|DBG|INF|WRN|ERR|FAL) MessageID=\S+ ActivityID=\S+ Source=\S+$", RegexOptions.Compiled);
			}

			Assert.That(headerRex.IsMatch(message.MsgHeader.Raw), Is.True);
		}

		[Test]
		public void TestLevel()
		{
			_writer.GetStringBuilder().Clear();
			_logger.eHubLog(LogLevel.Trace, "", "", "", "This is Message");
			CheckHeader(_writer.ToString(), "TRC", "", "", typeof(TestLog).FullName);

			_writer.GetStringBuilder().Clear();
			_logger.eHubLog(LogLevel.Debug, "", "", "", "This is Message");
			CheckHeader(_writer.ToString(), "DBG", "", "", typeof(TestLog).FullName);

			_writer.GetStringBuilder().Clear();
			_logger.eHubLog(LogLevel.Information, "", "", "", "This is Message");
			CheckHeader(_writer.ToString(), "INF", "", "", typeof(TestLog).FullName);

			_writer.GetStringBuilder().Clear();
			_logger.eHubLog(LogLevel.Warning, "", "", "", "This is Message");
			CheckHeader(_writer.ToString(), "WRN", "", "", typeof(TestLog).FullName);

			_writer.GetStringBuilder().Clear();
			_logger.eHubLog(LogLevel.Error, "", "", "", "This is Message");
			CheckHeader(_writer.ToString(), "ERR", "", "", typeof(TestLog).FullName);

			_writer.GetStringBuilder().Clear();
			_logger.eHubLog(LogLevel.Critical, "", "", "", "This is Message");
			CheckHeader(_writer.ToString(), "FAL", "", "", typeof(TestLog).FullName);
		}


		[Test]
		public void TestSource()
		{
			_writer.GetStringBuilder().Clear();
			_logger.eHubLog(LogLevel.Debug, "", "", "", "This is Message");
			CheckHeader(_writer.ToString(), "DBG", "", "", typeof(TestLog).FullName);

			_writer.GetStringBuilder().Clear();
			_logger.eHubLog(LogLevel.Debug, "", "", "Source", "This is Message");
			CheckHeader(_writer.ToString(), "DBG", "", "", "Source");

		}

		[Test]
		public void TestMachineNameLine()
		{
			_writer.GetStringBuilder().Clear();
			_logger.eHubLog(LogLevel.Debug, "", "", "", "This is Message");
			var message = new Message(_writer.ToString(), 1);
			Assert.That(message.MachineNameLineCheck(), Is.True);
		}


		[Test]
		public void TestMessage()
		{
			_writer.GetStringBuilder().Clear();
			_logger.eHubLog(LogLevel.Debug, "MESSAGE_ID", "ACTIVITY_ID", "", "This is Message");
			CheckMessage(_writer.ToString(), 1, new string[] { "This is Message" });


			_writer.GetStringBuilder().Clear();
			_logger.eHubLog(LogLevel.Debug, "MESSAGE_ID", "ACTIVITY_ID", "", "This is Message\r\nThis is the second Line");
			CheckMessage(_writer.ToString(), 2, new string[] {
				"This is Message",
				"This is the second Line"
			});

			_writer.GetStringBuilder().Clear();
			_logger.eHubLog(LogLevel.Debug, "MESSAGE_ID", "ACTIVITY_ID", "", "This is Message\nThis is the second Line");
			CheckMessage(_writer.ToString(), 2, new string[] {
				"This is Message",
				"This is the second Line"
			});

			_writer.GetStringBuilder().Clear();
			var obj = new { id = 1, name = "anonymous object", meta = "metaInfo" };
			_logger.eHubLog(LogLevel.Debug, "MESSAGE_ID", "ACTIVITY_ID", "", "{obj}", obj);
			CheckMessage(_writer.ToString(), 1, new string[] {
				"\"{ id = 1, name = anonymous object, meta = metaInfo }\""
			});

			_writer.GetStringBuilder().Clear();
			obj = new { id = 1, name = "anonymous object", meta = "metaInfo\r\nwith return" };
			_logger.eHubLog(LogLevel.Debug, "MESSAGE_ID", "ACTIVITY_ID", "", "{obj}", obj);

			CheckMessage(_writer.ToString(), 2, new string[] {
				"\"{ id = 1, name = anonymous object, meta = metaInfo",
				"with return }\""
			});

			_writer.GetStringBuilder().Clear();
			var arr = new object[]
			{
				obj,
				"obj string",
			};
			_logger.eHubLog(LogLevel.Debug, "MESSAGE_ID", "ACTIVITY_ID", "", "{arr}", arr);
			CheckMessage(_writer.ToString(), 2, new string[] {
				"\"{ id = 1, name = anonymous object, meta = metaInfo",
				"with return }\"",
			});

			_writer.GetStringBuilder().Clear();
			var dict = new Dictionary<string, string>
			{
				{ "key1", "value1" },
				{ "key2\r\nSub2", "value2" },
				{ "key3", "value3\r\nSub3" }
			};
			_logger.eHubLog(LogLevel.Debug, "MESSAGE_ID", "ACTIVITY_ID", "", "{dict}", dict);
			CheckMessage(_writer.ToString(), 3, new string[] {
				"[(\"key1\": \"value1\"), (\"key2",
				"Sub2\": \"value2\"), (\"key3\": \"value3",
				"Sub3\")]"
			});
		}

		private void CheckMessage(string msg, int msgLineCount, string[] messages)
		{
			var message = new Message(msg, msgLineCount);
			for (int i = 0; i < messages.Length; i++)
			{
				Assert.That(message.MsgMessage[i], Is.EqualTo("   " + messages[i]));
			}
		}

		[Test]
		public void TestException()
		{
			_writer.GetStringBuilder().Clear();
			try
			{
				ThrowExceptionMethod();
			}
			catch (Exception ex)
			{
				_logger.eHubLog(LogLevel.Error, "", "", "", ex, "A message");
			}
			var message = new Message(_writer.ToString(), 1);

			Assert.That(message.MsgMessage[0], Is.EqualTo("   A message"));
			Assert.That(message.MsgException[0], Is.EqualTo("   System.ArgumentNullException: Value cannot be null. (Parameter 'Will through here')"));
			Assert.That(message.MsgException[1].StartsWith("      at CargoWise.eHub.MessageEvent.KafkaProducerService.Tests.Logging.TestLog.ThrowExceptionMethod()"), Is.True);
			Assert.That(message.MsgException[2].StartsWith("      at CargoWise.eHub.MessageEvent.KafkaProducerService.Tests.Logging.TestLog.TestException()"), Is.True);
		}

		private void ThrowExceptionMethod()
		{
			throw new ArgumentNullException("Will through here");
		}

		[Test]
		public void TestForMultiThreads()
		{
			var logger = Log.Logger;
			_writer.GetStringBuilder().Clear();
			var taskList = Enumerable.Range(1, 1000)
				.Select(i => Task.Run(() =>
				{
					_logger.eHubLog(LogLevel.Debug, "messageID-" + i, "activityID-" + i, "Source-" + i, "message-" + i);
				}))
				.ToList();
			Task.WaitAll(taskList.ToArray());

			var msgs = _writer.ToString().Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);

			var messages = new List<Message>();
			for (int i = 0; i < msgs.Length - 1; i += 3)
			{
				messages.Add(new Message(msgs[i] + Environment.NewLine + msgs[i + 1] + Environment.NewLine + msgs[i + 2], 1));
			}

			foreach (var message in messages)
			{
				CheckThePropertiesAreSame(message);
			}

			_writer.GetStringBuilder().Clear();
		}

		private void CheckThePropertiesAreSame(Message message)
		{
			var msg = message.MsgMessage[0];
			var num = msg.Split('-')[1];

			Assert.That(message.MsgHeader.MessageId.Split('-')[1], Is.EqualTo(num));
			Assert.That(message.MsgHeader.ActivityId.Split('-')[1], Is.EqualTo(num));
			Assert.That(message.MsgHeader.Source.Split('-')[1], Is.EqualTo(num));

		}


		[Test]
		public void TestServeralLogCheck()
		{
			string msg = @"2023-06-26T18:21:44.974+10:00 ERR Source=CargoWise.eHub.MessageEvent.KafkaProducerService.Tests.Logging.TestLog
   MachineName=5DKTZT3 ProcessID=17840 ThreadID=16
   A message
2023-06-26T18:21:44.974+10:00 ERR Source=CargoWise.eHub.MessageEvent.KafkaProducerService.Tests.Logging.TestLog
   MachineName=5DKTZT3 ProcessID=17840 ThreadID=16
   A message
   System.ArgumentNullException: Value cannot be null. (Parameter 'Will through here')
      at CargoWise.eHub.MessageEvent.KafkaProducerService.Tests.Logging.TestLog.ThrowExceptionMethod() in C:\git\wtg\eServices\Apps\MessageEvent\KafkaProducerService.Tests\Logging\TestLog.cs:line 276
      at CargoWise.eHub.MessageEvent.KafkaProducerService.Tests.Logging.TestLog.TestException() in C:\git\wtg\eServices\Apps\MessageEvent\KafkaProducerService.Tests\Logging\TestLog.cs:line 260";

			Assert.That(Message.SplitAndCheck(msg), Is.True);

			msg = @"2023-06-26T18:21:44.974+10:00 ERR Source=CargoWise.eHub.MessageEvent.KafkaProducerService.Tests.Logging.TestLog
   MachineName=5DKTZT3 ProcessID=17840 ThreadID=16
   A message
2023-06-21T12:46:46.010 DEBUG This is message
2023-06-26T18:21:44.974+10:00 ERR Source=CargoWise.eHub.MessageEvent.KafkaProducerService.Tests.Logging.TestLog
   MachineName=5DKTZT3 ProcessID=17840 ThreadID=16
   A message
   System.ArgumentNullException: Value cannot be null. (Parameter 'Will through here')
      at CargoWise.eHub.MessageEvent.KafkaProducerService.Tests.Logging.TestLog.ThrowExceptionMethod() in C:\git\wtg\eServices\Apps\MessageEvent\KafkaProducerService.Tests\Logging\TestLog.cs:line 276
      at CargoWise.eHub.MessageEvent.KafkaProducerService.Tests.Logging.TestLog.TestException() in C:\git\wtg\eServices\Apps\MessageEvent\KafkaProducerService.Tests\Logging\TestLog.cs:line 260";
			Assert.That(Message.SplitAndCheck(msg), Is.False);

		}

		public class Message
		{
			public class Header
			{
				public string Raw { get; set; } = "";
				public string Timestamp { get; set; } = "";
				public string Level { get; set; } = "";
				public string MessageId { get; set; } = "";
				public string ActivityId { get; set; } = "";
				public string Source { get; set; } = "";

				public Header(string header)
				{
					string[] parts = header.Split(' ');
					Timestamp = parts[0];
					Level = parts[1];
					int loc = 2;
					if (parts[loc].StartsWith("MessageID") || parts[loc] == "(null)")
					{
						MessageId = parts[loc];
						loc++;
					}

					if (parts[loc].StartsWith("ActivityID") || parts[loc] == "(null)")
					{
						ActivityId = parts[loc];
						loc++;
					}

					Source = parts[loc];
					Raw = header;
				}

				public bool HeaderLineCheck()
				{
					return TimestampCheck()
						&& LevelCheck()
						&& MessageIdCheck()
						&& ActivityIdCheck()
						&& SourceCheck();
				}

				private bool TimestampCheck()
				{
					var reg = new Regex(@"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{3}[\+-]\d{2}:\d{2}$", RegexOptions.Compiled);

					return reg.IsMatch(Timestamp);
				}

				private bool LevelCheck()
				{
					var levels = new string[] { "TRC", "DBG", "INF", "WRN", "ERR", "FAL" };
					return levels.Any(l => l == Level);
				}

				private bool MessageIdCheck()
				{
					return string.IsNullOrEmpty(MessageId)
						|| MessageId.StartsWith("MessageID=");
				}

				private bool ActivityIdCheck()
				{
					return string.IsNullOrEmpty(ActivityId)
						|| ActivityId.StartsWith("ActivityID=");
				}

				private bool SourceCheck()
				{
					return Source.StartsWith("Source=");
				}
			}

			public Header MsgHeader { get; set; }
			public string MsgMachineNameLine { get; set; }

			public string[] MsgMessage { get; set; }

			public string[] MsgException { get; set; }

			public Message(string message, int messageLines)
			{

				if (messageLines < 0)
				{
					throw new ArgumentException("Message lines counts should not be negative");
				}

				if (message.Length < messageLines + 1)
				{
					throw new ArgumentException("A log message should have a header line at least");
				}

				string[] lines = message.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
				MsgHeader = new Header(lines[0]);
				MsgMachineNameLine = lines[1];
				if (messageLines > 0)
				{
					MsgMessage = lines.Skip(2)
						.Take(messageLines)
						.ToArray();
				}
				else
				{
					MsgMessage = Array.Empty<string>();
				}

				if (lines.Length > MsgMessage.Length + 2)
				{
					MsgException = lines.Skip(2 + MsgMessage.Length)
							.Take(lines.Length - MsgMessage.Length - 3)
						   .ToArray();
				}
				else
				{
					MsgException = Array.Empty<string>();
				}

			}

			public bool MachineNameLineCheck()
			{
				var reg = new Regex(@"^   MachineName=\S+ ProcessID=\S+ ThreadID=\S+$", RegexOptions.Compiled);

				return reg.IsMatch(MsgMachineNameLine);
			}

			public bool MessageCheck()
			{
				var result = MsgHeader.HeaderLineCheck() && MachineNameLineCheck();

				foreach (var message in MsgMessage)
				{
					result = result && message.StartsWith("   ");
				}

				foreach (var except in MsgException)
				{
					result = result && except.StartsWith("   ");
				}

				return result;
			}


			public static bool SplitAndCheck(string msg)
			{
				if (string.IsNullOrEmpty(msg))
					return true;

				var list = new List<Message>();
				string pattern = @"\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{3}[\+-]\d{2}:\d{2} (TRC|DBG|INF|WRN|ERR|FAL).*";
				var match = Regex.Match(msg, pattern);
				var pos = new List<int>();

				while (match.Success)
				{
					pos.Add(match.Index);
					match = match.NextMatch();
				}

				for (int i = 0; i < pos.Count - 1; i++)
				{
					Debug.WriteLine(msg.Substring(pos[i], pos[i + 1] - pos[i]));

					list.Add(new Message(msg.Substring(pos[i], pos[i + 1] - pos[i]), 0));
				}

				list.Add(new Message(msg.Substring(pos[pos.Count - 1], msg.Length - pos[pos.Count - 1]), 0));

				foreach (var message in list)
				{
					if (!message.MessageCheck())
					{
						return false;
					}
				}

				return true;
			}

		}
	}

}
