using System.Linq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class GlowLogTest : TestCase
	{
		public void TestAppendLog()
		{
			var log = new GlowLog();
			log.AppendLog("Error", "ErrorMessage", 0);
			log.AppendLog("Warning", "WarningMessage", 0);
			AssertEquals(2, log.CountWithoutVerbose);
			AssertEquals(log.GetLogType(0), "Error");
			AssertEquals(log.GetLogMessage(0), "ErrorMessage");
			AssertEquals(log.GetLogType(1), "Warning");
			AssertEquals(log.GetLogMessage(1), "WarningMessage");
			var lines = log.AsEnumerableWithoutVerbose();
			AssertEquals(lines.ToList()[0].Key, "Error");
			AssertEquals(lines.ToList()[0].Value, "ErrorMessage");
			AssertEquals(lines.ToList()[1].Key, "Warning");
			AssertEquals(lines.ToList()[1].Value, "WarningMessage");
		}

		public void TestHasErrors()
		{
			var log = new GlowLog();
			log.AppendLog(LogType.Warning, "WarningMessage", 0);
			log.AppendLog(LogType.Info, "InfoMessage", 10);

			AssertEquals(false, log.HasErrors);

			log.AppendLog(LogType.Error, "ErrorMessage", 0);
			log.AppendLog(LogType.ImportFinished, "FinishedMessage", 0);

			AssertEquals(true, log.HasErrors);
		}

		public void TestAppendLogs()
		{
			var log = new GlowLog();
			log.AppendLog("Error", "ErrorMessage", 0);
			log.AppendLog("Warning", "WarningMessage", 0);
			log.AppendLog("Verbose", "VerboseLogging1", 0);
			log.AppendLog("Verbose", "VerboseLogging2", 0);
			AssertEquals(2, log.CountWithoutVerbose);
			AssertEquals(true, log.HasVerboseErrors);

			var lines = log.GetLogs();
			AssertEquals(lines, "ErrorMessage\r\nWarningMessage\r\nVerboseLogging1\r\nVerboseLogging2");
		}

		public void TestHasVerboseErrors()
		{
			var log = new GlowLog();
			log.AppendLog(LogType.Warning, "WarningMessage", 0);
			log.AppendLog(LogType.Info, "InfoMessage", 10);

			AssertEquals(false, log.HasVerboseErrors);

			log.AppendLog(LogType.Verbose, "ErrorMessage", 0);

			AssertEquals(true, log.HasVerboseErrors);
		}

		public void TestAddNotifications()
		{
			var log = new GlowLog();
			log.Add(new ErrorNotification(ErrorType.Error, "Message"));
			log.Add(new NewlineNotification());
			log.Add(new WarningNotification(WarningType.Warning, "Message"));
			log.Add(new NewlineNotification());
			log.Add(new InfoNotification("Message"));

			AssertEquals(5, log.CountWithoutVerbose);

			AssertEquals(log.GetLogType(0), "Error");
			AssertEquals(log.GetLogMessage(0), "Error: Message");

			var result = System.Guid.TryParse(log.GetLogType(3), out var type1);
			AssertEquals("Random type for empty message", true, result);
			AssertEquals(log.GetLogMessage(1), "");

			AssertEquals(log.GetLogType(2), "Warning");
			AssertEquals(log.GetLogMessage(2), "Warning: Message");

			result = System.Guid.TryParse(log.GetLogType(3), out var type2);
			AssertEquals("Random type for empty message", true, result);
			AssertEquals(log.GetLogMessage(3), "");

			AssertEquals(log.GetLogType(4), "Info");
			AssertEquals(log.GetLogMessage(4), "Message");
		}
	}
}
