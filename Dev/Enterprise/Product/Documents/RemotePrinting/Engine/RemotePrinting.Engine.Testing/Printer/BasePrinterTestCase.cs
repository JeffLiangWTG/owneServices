using System.Collections.Generic;

namespace Enterprise.RemotePrinting.Engine.Testing
{
	public abstract class BasePrinterTestCase : PrintEngineTestCase
	{
		protected abstract string TestFileToPrint { get; }
		protected abstract BasePrinter GetPrinter(PrintEngineJob printJob);

		protected BasePrinter GetPrinter()
		{
			PrintEngineJob printJob = GetPrintEngineJob(TestFileToPrint);
			return GetPrinter(printJob);
		}

		public void TestVerboseLogsOnDispose()
		{
			var messages = new List<string>();
			using (var printer = GetPrinter())
			{
				printer.Logged += (sender, logEventArgs) => messages.Add(logEventArgs.Message);
			}

			AssertEquals(1, messages.Count);
			AssertEquals("Disposing printer", messages[0]);
		}

		public void TestNoVerboseLogsOnDispose()
		{
			var messages = new List<string>();
			using (var printer = GetPrinter())
			{
			}

			AssertEquals(0, messages.Count);
		}
	}
}
