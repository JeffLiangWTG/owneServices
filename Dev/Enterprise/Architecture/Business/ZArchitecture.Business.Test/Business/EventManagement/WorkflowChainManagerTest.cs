using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.ZArchitecture.Business.EventManagement.Testing
{
	sealed class WorkflowChainManagerTest : TestCaseWithFactory
	{
		public void TestChainIDIsEmptyWhenNoChainIsActive()
		{
			var logParent = (DummyEnterpriseBusinessObject)Factory.New<IDummyWithWorkflow>();

			var trigger = ((IDummyWithWorkflow)logParent).AddNewTrigger();
			((IBaseTrigger)trigger).TriggerEventCode = Events.AuthorisedCode;
			trigger.P9_ParentID = logParent.PK;
			trigger.P9_Type = "TRG";

			var log = logParent.Logs.AddNew(Events.Authorised);

			var triggeringEventData = new WorkflowTriggerEventData(log, ZGuid.Empty, "BRA", "DEP", "AAA", "BBB", "CCC");
			var triggerWTELog = ((BusinessObject)trigger).GetLogs().AddNew(Events.WorkflowTriggerEvent, triggeringEventData.ToReference());

			var queuedLog = new QueuedLogForTesting(triggerWTELog);

			AssertEquals("WorkflowChainManager.HasWTEEventFromCurrentChain(milestone)", false, WorkflowChainManager.HasWTEEventFromCurrentChain((IBaseTrigger)trigger));
			AssertEquals("WorkflowChainManager.ChainID", ZGuid.Empty, WorkflowChainManager.ChainID);
		}

		public void TestChainIDIsFilledInAndBackUpdatesOriginalEventWhenChainIsActive()
		{
			var logParent = (DummyEnterpriseBusinessObject)Factory.New<IDummyWithWorkflow>();

			var trigger = ((IDummyWithWorkflow)logParent).AddNewTrigger();
			((IBaseTrigger)trigger).TriggerEventCode = Events.AuthorisedCode;
			trigger.P9_ParentID = logParent.PK;
			trigger.P9_Type = "TRG";

			var log = logParent.Logs.AddNew(Events.Authorised);

			var triggeringEventData = new WorkflowTriggerEventData(log, ZGuid.Empty, "BRA", "DEP", "AAA", "BBB", "CCC");
			var triggerWTELog = ((BusinessObject)trigger).GetLogs().AddNew(Events.WorkflowTriggerEvent, triggeringEventData.ToReference());

			var queuedLog = new QueuedLogForTesting(triggerWTELog);
			using (new WorkflowChainManager(queuedLog))
			{
				AssertEquals("WHEN we added WTE to trigger, THEN HasWTEEventFromCurrentChain should return true", true, WorkflowChainManager.HasWTEEventFromCurrentChain((IBaseTrigger)trigger));
				var chainID = WorkflowChainManager.ChainID;
				AssertNotEquals("WorkflowChainManager.ChainID while processing Workflow", ZGuid.Empty, chainID);

				var wteEventData = new WorkflowTriggerEventData(triggerWTELog);
				AssertEquals("Original wteEvent should have ChainID back updated", chainID, wteEventData.TriggerChainID);
			}

			AssertEquals("WorkflowChainManager.ChainID after processing workflow", ZGuid.Empty, WorkflowChainManager.ChainID);
		}

		public void TestHasWTEEventFromCurrentChain()
		{
			var logParent = (DummyEnterpriseBusinessObject)Factory.New<IDummyWithWorkflow>();

			var trigger = ((IDummyWithWorkflow)logParent).AddNewTrigger();
			((IBaseTrigger)trigger).TriggerEventCode = Events.AuthorisedCode;
			trigger.P9_ParentID = logParent.PK;
			trigger.P9_Type = "TRG";

			var log = logParent.Logs.AddNew(Events.Authorised);

			var triggeringEventData = new WorkflowTriggerEventData(log, ZGuid.Empty, "BRA", "DEP", "AAA", "BBB", "CCC");
			var triggerWTELog = ((BusinessObject)trigger).GetLogs().AddNew(Events.WorkflowTriggerEvent, triggeringEventData.ToReference());

			var wteEventData = new WorkflowTriggerEventData(triggerWTELog);
			var chainID = ZGuid.NewZGuid();
			wteEventData.TriggerChainID = chainID;
			using (triggerWTELog.LockForUpdatingKeyFields(false))
			{
				triggerWTELog.SL_Reference = wteEventData.ToReference();
			}

			var queuedLog = new QueuedLogForTesting(triggerWTELog);
			using (new WorkflowChainManager(queuedLog))
			{
				AssertEquals("WorkflowChainManager.HasWTEEventFromCurrentChain(milestone)", true, WorkflowChainManager.HasWTEEventFromCurrentChain((IBaseTrigger)trigger));
				AssertEquals("WorkflowChainManager.ChainID while processing Workflow", chainID, WorkflowChainManager.ChainID);
			}

			AssertEquals("WorkflowChainManager.ChainID after processing workflow", ZGuid.Empty, WorkflowChainManager.ChainID);
		}
	}
}
