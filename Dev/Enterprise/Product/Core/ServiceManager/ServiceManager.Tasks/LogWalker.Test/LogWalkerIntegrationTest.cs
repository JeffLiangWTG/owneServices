using System;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.ServiceManager.Tasks.LogWalker.Test
{
	public class LogWalkerIntegrationTest : TestCaseWithFactory
	{
		#region Setup

		protected override void SetUp()
		{
			base.SetUp();
			LazyLogger = new Lazy<DummyLogger>(() => new DummyLogger());
			LazyMasterTask = new Lazy<LogWalkerMasterServiceTask>(() => new LogWalkerMasterServiceTask { ServiceLogger = Logger });
			LazyDefaultTask = new Lazy<LogWalkerServiceTask>(() => new LogWalkerServiceTask { ServiceLogger = Logger });
		}

		Lazy<DummyLogger> LazyLogger { get; set; }
		DummyLogger Logger => LazyLogger.Value;
		Lazy<LogWalkerMasterServiceTask> LazyMasterTask { get; set; }
		LogWalkerMasterServiceTask MasterTask => LazyMasterTask.Value;
		Lazy<LogWalkerServiceTask> LazyDefaultTask { get; set; }
		LogWalkerServiceTask DefaultTask => LazyDefaultTask.Value;

		DummyWithWorkflow MakeDummy()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			var notification = trigger.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendXMLSimplified;

			return dummy;
		}

		StmALog AddEvent(DummyWithWorkflow dummy)
		{
			return dummy.Logs.AddNew(Events.CustomisableEvent00);
		}

		StmALog[] GetAllNewWTE()
		{
			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode) { FetchOnlyFromLocalCache = true };
			return Factory.Load<StmALog>(query).Where(s => !s.IsInDatabase).ToArray();
		}

		void AssertIsQueued(params StmALog[] logs)
		{
			AssertEquals(logs.Length, Factory.GetDatabaseCount(ObjectFactory.GetType<IQueuedLog>(), new ZQuery(StmJobQueueSchema.SJ_ParentID, logs.Select(s => s.SL_Parent)) { ReLoadExistingRows = true }));
		}

		void AssertIsNotQueued(params StmALog[] logs)
		{
			AssertEquals(0, Factory.GetDatabaseCount(ObjectFactory.GetType<IQueuedLog>(), new ZQuery(StmJobQueueSchema.SJ_ParentID, logs.Select(s => s.SL_Parent)) { ReLoadExistingRows = true }));
		}

		void AssertProcessedCount(int expected, params StmALog[] logs)
		{
			var processed = CountProcessed(logs);
			AssertEquals("All logs should be processed", expected, processed);
		}

		int CountProcessed(StmALog[] logs)
		{
			var filter = new ZQuery(StmJobQueueSchema.SJ_ALogReference, logs.Select(s => s.PK));
			filter.AddToFilter(StmJobQueueSchema.SJ_Status, "PRS");

			return Factory.GetDatabaseCount(ObjectFactory.GetType<IQueuedLog>(), filter);
		}

		#endregion

		public void TestMaster_FlipsQueues()
		{
			var dummy = MakeDummy();
			var log = AddEvent(dummy);
			var logs = GetAllNewWTE();
			Factory.Save();
			MasterTask.RunTask();
			AssertIsQueued(logs);
			AssertProcessedCount(0, logs);
		}

		public void TestDefault_RequiresMaster()
		{
			var dummy = MakeDummy();
			var log = AddEvent(dummy);
			var logs = GetAllNewWTE();
			Factory.Save();
			DefaultTask.RunTask();
			AssertIsNotQueued(logs);
			AssertProcessedCount(0, logs);

			MasterTask.RunTask();
			AssertProcessedCount(0, logs);

			DefaultTask.RunTask();
			AssertProcessedCount(logs.Length, logs);
		}

		[ExpectNoExceptions]
		public void TestNudge()
		{
			var counter = new ServiceTaskNudgerWithCount();
			using (ObjectFactory.Substitute<IServiceTaskNudger>(counter))
			{
				var dummy = MakeDummy();
				var log = AddEvent(dummy);
				var logs = GetAllNewWTE();
				MasterTask.RunTask();

				AssertNotEquals("Don't really care how much we nudge, as long as we do.", 0, counter.Count);
			}
		}

		public void TestLogWalkerQueue()
		{
			var queueProvider = new LogWalkerQueue() as IHostedServiceQueueProvider;
			var startingQueue = queueProvider.QueueResult.QueueSize;

			var dummy = MakeDummy();
			AddEvent(dummy);
			Factory.Save();

			MasterTask.RunTask();

			AssertEquals("1 should be added to the queue", 1, queueProvider.QueueResult.QueueSize - startingQueue);
		}

		[TestDate(2022, 2, 2)]
		public void TestLogWalkerQueue_EventTime()
		{
			Db.Connection.ExecuteNonQuery("delete from dbo.StmJobQueue");
			var queueProvider = new LogWalkerQueue() as IHostedServiceQueueProvider;

			var dummy = MakeDummy();
			AddEvent(dummy);
			Factory.Save();

			MasterTask.RunTask();

			var query = new ZQuery(StmJobQueueSchema.SJ_ProcessTaskParentID, dummy.PK);
			var log = (BusinessObject)Factory.LoadTop1<IQueuedLog>(query);
			log[StmJobQueueSchema.SJ_IsDelayFired.Name] = true;
			Factory.Save();

			var result = queueProvider.QueueResult;
			AssertEquals("1 should be added to the queue when SJ_IsDelayFired is true and SJ_EventTimeUtc is less than now", 1, result.QueueSize);
			NUnit.Framework.Assert.That((int)result.MaximumItemAge.TotalSeconds, NUnit.Framework.Is.EqualTo((int)(DateTime.UtcNow - TestDateAttribute.Date).TotalSeconds).Within(60));
		}

		public void TestLogWalkerQueueUsesEventTimeUtc()
		{
			Db.Connection.ExecuteNonQuery("delete from dbo.StmJobQueue");
			using (Db.Connection.TrackExecutedCommands())
			{ 
				var queueProvider = new LogWalkerQueue() as IHostedServiceQueueProvider;
				var result = queueProvider.QueueResult;
				var queryText = Db.Connection.ExecutedCommands.FirstOrDefault(c => c.Replace("\n", "").Replace("\r", "").Replace("\t", " ").Contains("FROM dbo.StmJobQueue"));
				AssertNotNull(queryText);
				Assert("WHERE clause should not contain SJ_EventTime", !Regex.IsMatch(queryText, @$"\b{StmJobQueueSchema.SJ_EventTime.Name}\b"));
				Assert("WHERE clause should not contain SJ_EventTimeUtc", !Regex.IsMatch(queryText, @$"\b{StmJobQueueSchema.SJ_EventTimeUtc.Name}\b"));
				Assert("WHERE clause should contain SJ_ProcessOnOrAfterUtc", Regex.IsMatch(queryText, @$"\b{StmJobQueueSchema.SJ_ProcessOnOrAfterUtc.Name}\b"));
			}
		}

		[ExpectNoExceptions]
		public void TestLogWalkerQueue_Age_ProcessOnOrAfterUtc_EventTime()
		{
			// Arrange
			Db.Connection.ExecuteNonQuery("delete from dbo.StmJobQueue");
			var itemDate = new DateTime(2025, 1, 1);
			var expectedAge = DateTime.UtcNow - itemDate;
			var queueProvider = new LogWalkerQueue() as IHostedServiceQueueProvider;

			var dummy = MakeDummy();
			AddEvent(dummy);
			Factory.Save();

			MasterTask.RunTask();

			Db.Connection.ExecuteNonQuery($"update top(1) dbo.StmJobQueue set SJ_IsDelayFired = 1, SJ_EventTimeUtc = '{itemDate}'");

			var result = queueProvider.QueueResult;
			NUnit.Framework.Assert.That(result.QueueSize, Is.EqualTo(1));
			NUnit.Framework.Assert.That(result.MaximumItemAge.TotalSeconds, Is.EqualTo(expectedAge.TotalSeconds).Within(60));
		}

		[ExpectNoExceptions]
		public void TestLogWalkerQueue_Age_ProcessOnOrAfterUtc_PostedTime()
		{
			// Arrange
			Db.Connection.ExecuteNonQuery("delete from dbo.StmJobQueue");
			var itemDate = new DateTime(2025, 1, 1);
			var expectedAge = DateTime.UtcNow - itemDate;
			var queueProvider = new LogWalkerQueue() as IHostedServiceQueueProvider;

			var dummy = MakeDummy();
			AddEvent(dummy);
			Factory.Save();

			MasterTask.RunTask();

			Db.Connection.ExecuteNonQuery($"update top(1) dbo.StmJobQueue set SJ_IsDelayFired = 0, SJ_PostedTimeUtc = '{itemDate}'");

			var result = queueProvider.QueueResult;
			NUnit.Framework.Assert.That(result.QueueSize, Is.EqualTo(1));
			NUnit.Framework.Assert.That(result.MaximumItemAge.TotalSeconds, Is.EqualTo(expectedAge.TotalSeconds).Within(60));
		}

		class ServiceTaskNudgerWithCount : IServiceTaskNudger
		{
			public int Count { get; private set; }
			public void NudgeServiceTask(string serviceTaskCode, TimeSpan? delay = null)
			{
				Count++;
				serviceTaskNuder.NudgeServiceTask(serviceTaskCode, delay);
			}

			readonly IServiceTaskNudger serviceTaskNuder = ObjectFactory.Get<IServiceTaskNudger>();
		}
	}
}
