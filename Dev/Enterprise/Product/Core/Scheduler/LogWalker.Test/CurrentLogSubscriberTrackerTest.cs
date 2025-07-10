using System.Threading;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.LogWalker.Test
{
	sealed class CurrentLogSubscriberTrackerTest : LogWalkerTestCase
	{
		public void TestSubscriberYield()
		{
			var dummy = (BusinessObject)Factory.New<IDummyWithWorkflow>();
			dummy.GetLogs().AddNew(Events.CustomisableEvent00);
			dummy.GetLogs().AddNew(Events.CustomisableEvent01);

			Factory.Save();

			var tracker = new CurrentLogSubscriberTracker();
			AssertNull(tracker.CurrentName);

			ProcessLogs processLogs = logs => Logger.Log(Integration.LogType.Information, $"Current subscriber is [{tracker.CurrentName}]");

			var bestSubscriber = new MockSubscriber(processLogs, new[] { Events.CustomisableEvent00Code }, new[] { DummyBizoSchema.Constants.TableName }, "BestSubscriber");
			var idiotSubscriber = new MockSubscriber(processLogs, new[] { Events.CustomisableEvent01Code }, new[] { DummyBizoSchema.Constants.TableName }, "IdiotSubscriber");

			ObjectFactory.Substitute("SystemLogSubscribers", new ILogSubscriber[] { bestSubscriber, idiotSubscriber });

			LogWalkerRunner.Master().Process(Logger, CancellationToken.None);
			LogWalkerRunner.Default().Process(Logger, CancellationToken.None);
			LogWalkerRunner.Purge().Process(Logger, CancellationToken.None);

			AssertContains("Current subscriber is [BestSubscriber]", string.Join("\r\n", Logger.LogEntries));
			AssertContains("Current subscriber is [IdiotSubscriber]", string.Join("\r\n", Logger.LogEntries));
		}
	}
}
