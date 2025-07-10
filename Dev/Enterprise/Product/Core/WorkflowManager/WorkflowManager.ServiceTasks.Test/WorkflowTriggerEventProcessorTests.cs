using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Integration;
using Enterprise.LogWalker;
using Enterprise.LogWalker.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.WorkflowManager.ServiceTasks.Testing
{
	public class WorkflowTriggerEventProcessorTests : TestCase
	{
		protected override void SetUp()
		{
			Db.Connection.ExecuteNonQuery("delete from dbo.StmALogQueueWTE"); // Clean up so we don't trip over crap in the DB.
			Db.Connection.ExecuteNonQuery("delete from dbo.StmJobQueue"); // Clean up so we don't trip over crap in the DB.
			base.SetUp();
		}

		static void EnableLog(CodeDescriptionBoolRegistryItem registryItem, string key)
		{
			var pairs = registryItem.Value;
			pairs.Set(key, true);
			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, pairs);
		}

		[UseSnapshotProtection]
		[TestDate(2020, 01, 01)]
		public void TestDelayedStmJobLoggingOnlyOnePeriodicReporter()
		{
			var logger = new LoggerForTesting();
			var threads = new List<Thread>();
			EnableLog(SystemDataRegistry.Instance.WorkflowEventTriggerProccessorLogging, SystemDataRegistry.WorkflowEventTriggerLoggingKeys.DelayedLog);
			EnableLog(SystemDataRegistry.Instance.LogWalkerLogging, SystemDataRegistry.LogWalkerLoggingKeys.StmJobQueueReport);
			SystemDataRegistry.Instance.LogWalkerBatchSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);
			var factory = new BusinessObjectFactory() { RefreshEnabled = false };

			for (int i = 0; i < 20; ++i)
			{
				var index = i;

				// Setup StmALog entries
				var shipment = factory.New<Forwarding.IForwardingShipment>();
				shipment.JS_UniqueConsignRef = index.ToString();
				((BusinessObject)shipment).FillWithValidTestData();
				var trigger = ((IWorkflowProvider)shipment).WorkflowItems.Triggers.AddNew();
				((ITriggerConditions)trigger).TriggerEventCode = "ADD";
				var notification = trigger.ProcessTaskNotifications.AddNew();
				notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
				notification.PQ_EmailText = "Blah";
				notification.PQ_EmailAddr = "<JS_GoodsDescription>";

				threads.Add(new Thread(() =>
				{
					TestDateAttribute.AddMinutes(20);
					using (var connectionDisposer = Db.DisposableActionForDbConnection())
					{
						var lwm = LogWalkerRunner.Master();
						var lwk = LogWalkerRunner.Default();
						var lwp = LogWalkerRunner.Purge();

						lwm.Process(logger, CancellationToken.None);
						lwk.Process(logger, CancellationToken.None);
						lwp.Process(logger, CancellationToken.None);
					}
				}));
			}

			factory.Save();

			foreach (var thread in threads)
			{
				thread.Start();
			}

			foreach (var thread in threads)
			{
				thread.Join();
			}

			var reportCount = 0;
			var delayedLogCount = 0;
			foreach (var logline in logger.NotifiedEventList)
			{
				if (logline.Contains("StmJobQueueReport"))
				{
					reportCount++;
				}

				if (logline.Contains("DelayedLog"))
				{
					delayedLogCount++;
				}
			}

			AssertEquals(20, delayedLogCount);
			AssertEquals(1, reportCount);
		}

		[UseSnapshotProtection]
		[TestDate(2020, 01, 01)]
		public void TestDelayedStmJobLoggingMaxCount()
		{
			var logger = new LoggerForTesting();
			EnableLog(SystemDataRegistry.Instance.WorkflowEventTriggerProccessorLogging, SystemDataRegistry.WorkflowEventTriggerLoggingKeys.DelayedLog);
			EnableLog(SystemDataRegistry.Instance.LogWalkerLogging, SystemDataRegistry.LogWalkerLoggingKeys.StmJobQueueReport);

			TestWorkflowEventTriggerConfig.SetWorkflowEventTriggerConfigForTest(
				new TestWorkflowEventTriggerConfig(new TimeSpan(0, 10, 0)));
			var factory = new BusinessObjectFactory() { RefreshEnabled = false };
			// Setup StmALog entries
			var shipment = factory.New<Forwarding.IForwardingShipment>();
			((BusinessObject)shipment).FillWithValidTestData();
			var trigger = ((IWorkflowProvider)shipment).WorkflowItems.Triggers.AddNew();
			((ITriggerConditions)trigger).TriggerEventCode = "Z00";
			var notification = trigger.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
			notification.PQ_EmailText = "Blah";
			notification.PQ_EmailAddr = "<JS_GoodsDescription>";

			((IWorkflowProvider)shipment).Logs.AddNew(Events.CustomisableEvent00);
			factory.Save();
			((IWorkflowProvider)shipment).Logs.AddNew(Events.CustomisableEvent00);
			factory.Save();
			((IWorkflowProvider)shipment).Logs.AddNew(Events.CustomisableEvent00);
			factory.Save();
			((IWorkflowProvider)shipment).Logs.AddNew(Events.CustomisableEvent00);
			factory.Save();

			TestDateAttribute.AddMinutes(20);

			var lwm = LogWalkerRunner.Master();
			var lwk = LogWalkerRunner.Default();
			var lwp = LogWalkerRunner.Purge();

			lwm.Process(logger, CancellationToken.None);
			lwk.Process(logger, CancellationToken.None);
			lwp.Process(logger, CancellationToken.None);

			var reportCount = 0;
			foreach (var logline in logger.NotifiedEventList)
			{
				if (logline.Contains("StmJobQueueReport"))
				{
					AssertEquals(5, Regex.Matches(logline, "PK:").Count);
					reportCount++;
				}
			}

			AssertEquals(1, reportCount);
		}

		[UseSnapshotProtection]
		[TestDate(2020, 01, 01)]
		public void TestStmALogDBDelete()
		{
			var factory = new BusinessObjectFactory() { RefreshEnabled = false };
			var shipment = factory.New<Forwarding.IForwardingShipment>();
			((BusinessObject)shipment).FillWithValidTestData();

			var trigger = ((IWorkflowProvider)shipment).WorkflowItems.Triggers.AddNew();
			((ITriggerConditions)trigger).TriggerEventCode = Events.CustomisableEvent00Code;

			var log = ((IWorkflowProvider)shipment).Logs.AddNew(Events.CustomisableEvent00);
			var testAction = new SaveInTransactionDelegateActionTest(factory, log.Cancel);
			factory.SaveInTransactionActions.Add(testAction);

			AssertNoExceptionThrown(factory.Save);
		}

		sealed class SaveInTransactionDelegateActionTest : SaveInTransactionActionWithFactory
		{
			readonly Action Delegate;
			public SaveInTransactionDelegateActionTest(BusinessObjectFactory factory, Action method)
				 : base(factory)
			{
				Delegate = method;
			}
			protected override void OnAllTransactionsCommitted(IChangedTableNames changedTableNames)
			{
				Delegate();
				return;
			}
			protected override IChangedTableNames SaveInTransaction()
			{
				return ChangedTableNames.Empty;
			}
		}
	}
}
