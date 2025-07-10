using System;
using System.IO;
using NUnit.Framework;

namespace Enterprise.Builder.Generator
{
	sealed class ConsoleProgressLoggerTest : TestCase
	{
		public void TestAddProgressText()
		{
			var output = CaptureConsoleOutput(() => new ConsoleProgressLogger().AddProgressText("FooBar"));
			AssertEquals("FooBar\r\n", output);
		}

		public void TestReportSkippedFile()
		{
			var output = CaptureConsoleOutput(() => new ConsoleProgressLogger().ReportSkippedFile("Filename.ext"));
			AssertEquals("Skipping File: Filename.ext\r\n", output);
		}

		public void TestShowStatusLine()
		{
			var output = CaptureConsoleOutput(() => new ConsoleProgressLogger().ShowStatusLine("FooBar"));
			AssertEquals("", output);
		}

		static string CaptureConsoleOutput(Action action)
		{
			var oldOut = Console.Out;

			try
			{
				using (var writer = new StringWriter())
				{
					Console.SetOut(writer);
					action();
					return writer.ToString();
				}
			}
			finally
			{
				Console.SetOut(oldOut);
			}
		}
	}
}
