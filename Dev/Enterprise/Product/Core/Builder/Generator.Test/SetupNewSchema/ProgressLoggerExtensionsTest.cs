using System;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace Enterprise.Builder.Generator.Test
{
	sealed class ProgressLoggerExtensionsTest : TestCase
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1122:DoNotUseDateTimeParse", Justification = "Testing")]
		public void TestAddProgressHeaderLine()
		{
			var logger = new DummyLogger();
			logger.ShowStatusLine("decoy");

			var pre = DateTime.Now;
			logger.AddProcessHeaderLine("FooBar");
			var post = DateTime.Now;

			var regex = new Regex(@"^\r\n \[(?<timestamp>[0-9:]+ (AM|PM))\] - FooBar\.\.\.\r\n$", RegexOptions.ExplicitCapture);
			AssertMatch(regex, logger.ToString());
			AssertEquals(logger.StatusBarText, "");

			var timeOfDay = DateTime.Parse(regex.Match(logger.ToString()).Groups["timestamp"].ToString()).TimeOfDay;
			// The log only includes the time and not the date, so we need to work that part out for ourselves.
			// We need to be aware that the date could have ticked over anywhere between pre and post. If the time
			// is early in the day, then we almost certainly want the date from post. If the time is late in the day,
			// then we almost certainly want the date from pre. This logic only falls appart if AddReportHeader
			// takes > 12 hours to run.
			var timestamp = (timeOfDay.Hours > 12 ? pre.Date : post.Date) + timeOfDay;

			// The log doesn't list miliseconds, so we need to truncate that from pre & post too.
			pre = TruncateToSecond(pre);
			post = TruncateToSecond(post);

			var message = $"{pre:HH:mm:ss.fff} < {timestamp:HH:mm:ss.fff} < {post:HH:mm:ss.fff}";
			AssertLessThanOrEqualTo(message, pre, timestamp);
			AssertLessThanOrEqualTo(message, timestamp, post);

			DateTime TruncateToSecond(DateTime t) => new DateTime(t.Year, t.Month, t.Day, t.Hour, t.Minute, t.Second);
		}

		sealed class DummyLogger : IProgressLogger
		{
			public string StatusBarText { get; private set; }

			public void AddProgressText(string text) => builder.AppendLine(text);

			public void ShowStatusLine(string text)
			{
				StatusBarText = text;
			}

			public void ReportSkippedFile(string text) => throw new NotSupportedException();

			public override string ToString() => builder.ToString();

			readonly StringBuilder builder = new StringBuilder();
		}
	}
}
