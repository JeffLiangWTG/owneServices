using System;
using System.Threading;
using Enterprise.Integration;
using ServiceManager.Host.CW1.Test.EndToEndTests;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	"TS1",
	"Test Service Provider 1",
	"TST",
	typeof(TestServiceProvider),
	DefaultScheduleRunEvery = "1minute"
)]
[assembly: HostedService(
	"TS2",
	"Test Service Provider 2",
	"TST",
	typeof(TestServiceProvider),
	DefaultScheduleRunEvery = "1minute"
)]

namespace ServiceManager.Host.CW1.Test.EndToEndTests
{
	public class TestServiceProvider : ServiceProviderImpl
	{
		public override void RunTask(CancellationToken token = new CancellationToken())
		{
			ServiceLogger.Log(LogType.Information, string.Format("{0} {1}", "TS1/TS2", AppDomain.CurrentDomain.FriendlyName));
			ServiceLogger.Log(LogType.Debug, string.Format("{0} {1} - This is a debug log", "TS1/TS2", AppDomain.CurrentDomain.FriendlyName));
		}
	}
}
