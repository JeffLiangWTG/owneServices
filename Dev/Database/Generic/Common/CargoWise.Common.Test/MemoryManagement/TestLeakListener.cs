using System;
using System.Diagnostics;
using CargoWise.Common.MemoryManagement;
using CargoWise.Shared;
using NUnit.Framework;

namespace CargoWise.Common.Testing.MemoryManagement
{
	public class TestLeakListener : BaseTestListener
	{
		bool wasLeakTrackingEnabledBeforeTestStarted;

		public override void StartAllTests(DateTime startTime)
		{
			wasLeakTrackingEnabledBeforeTestStarted = DisposableLeakListener.Instance.LeakTrackingEnabled;
			DisposableLeakListener.Instance.LeakTrackingEnabled = true;
		}

		public override void EndAllTests(DateTime endTime)
		{
			DisposableLeakListener.Instance.LeakTrackingEnabled = wasLeakTrackingEnabledBeforeTestStarted;
		}

		public override void StartTest(TestCase test, DateTime startTime)
		{
			DisposableLeakListener.Instance.Clear();
		}

		public override void AfterEachTest(DateTime endTime)
		{
			var failureMessage = DisposableLeakListener.Instance.GetFailureMessageAndCleanup(() =>
			{
				var dumpFileUrl = string.Empty;
				var tempFile = TempForTest.GetTempFileName();
				try
				{
					if (MemoryDump.Create(tempFile, Process.GetCurrentProcess()))
					{
						dumpFileUrl = new WTG.DevTools.Common.TestFailureDataClient().Upload(tempFile + ".prfsession", "application/octet-stream", ".prfsession").Result;
					}
				}
				finally
				{
					FileIO.DeleteFile(tempFile);
					FileIO.DeleteFile(tempFile + ".prfsession");
				}
				return dumpFileUrl;
			}, HtmlStackTraceFormatter.Format);

			if (failureMessage != null)
			{
				Assertion.HtmlFail(failureMessage);
			}
		}
	}
}
