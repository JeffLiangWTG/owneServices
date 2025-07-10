using System;
using System.ComponentModel;
using System.Threading;
using Enterprise.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.ServiceManager.Runner.Testing
{
	public class ProcessEnvironmentRecorderTest : TestCase
	{
		public void TestSecondsAggregation()
		{
			System.Environment.SetEnvironmentVariable("ServiceTaskSummary_TEST", null);

			for (var i = 0; i < 10; i++)
			{
				using (new ProcessEnvironmentRecorder(new Mock<ILogger>().Object).StartRun("TEST"))
				{
					Thread.Sleep(TimeSpan.FromSeconds(0.5));
				}
			}

			var result = System.Environment.GetEnvironmentVariable("ServiceTaskSummary_TEST").Split(' ');
			AssertEquals("RunCount=10", result[0]);
			Assert(result[1].StartsWith("CPUTime="));
			Assert(result[2].StartsWith("ClockTime="));
			AssertEquals("Clock seconds", 5, TimeSpan.Parse(result[2].Split('=')[1]).Seconds, 1m);
		}

		public void TestIt()
		{
			System.Environment.SetEnvironmentVariable("ServiceTaskSummary_TEST", null);

			using (new ProcessEnvironmentRecorder(new Mock<ILogger>().Object).StartRun("TEST"))
			{
				Thread.Sleep(TimeSpan.FromSeconds(1.5));
			}

			var result = System.Environment.GetEnvironmentVariable("ServiceTaskSummary_TEST").Split(' ');
			AssertEquals("RunCount=1", result[0]);
			Assert(result[1].StartsWith("CPUTime="));
			Assert(result[2].StartsWith("ClockTime="));
			AssertEquals("Clock seconds", 1, TimeSpan.Parse(result[2].Split('=')[1]).Seconds);
			using (new ProcessEnvironmentRecorder(new Mock<ILogger>().Object).StartRun("TEST"))
			{
				Thread.Sleep(TimeSpan.FromSeconds(1));
			}

			var result2 = System.Environment.GetEnvironmentVariable("ServiceTaskSummary_TEST").Split(' ');
			AssertEquals("RunCount=2", result2[0]);
		}

		public void TestHandlesAccessDeniedGracefully()
		{
			System.Environment.SetEnvironmentVariable("ServiceTaskSummary_TEST", null);

			var logger = new Mock<ILogger>();
			var processInfoProvider = new Mock<ICurrentProcessInfoProvider>();
			processInfoProvider.Setup(pip => pip.TotalProcessorTime).Throws(new Win32Exception(5));
			using (new ProcessEnvironmentRecorder(logger.Object, processInfoProvider.Object).StartRun("TEST"))
			{
				Thread.Sleep(TimeSpan.FromSeconds(1.5));
			}

			var result = System.Environment.GetEnvironmentVariable("ServiceTaskSummary_TEST").Split(' ');
			AssertEquals("RunCount=1", result[0]);
			AssertEquals("CPUTime=00:00:00", result[1]);
			Assert(result[2].StartsWith("ClockTime="));
			AssertEquals("Clock seconds", 1, TimeSpan.Parse(result[2].Split('=')[1]).Seconds);
			logger.Verify(l => l.Log(LogType.Warning, "Unable to calculate processor time for service task run.  Access is denied."), Times.Exactly(2));
		}
	}
}
