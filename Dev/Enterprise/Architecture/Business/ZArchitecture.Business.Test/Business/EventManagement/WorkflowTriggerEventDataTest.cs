using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Workflow;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.ZArchitecture.Business.EventManagement.Testing
{
	sealed class WorkflowTriggerEventDataTest : TestCaseWithFactory
	{
		public void TestLegacyWTE_5Value()
		{
			var triggeringLogPK = ZGuid.NewZGuid();
			var triggerChainID = ZGuid.NewZGuid();

			var dummyBO = (BusinessObject)Factory.New<IDummyWithWorkflow>();
			var wteEvent = dummyBO.GetLogs().AddNew(Events.WorkflowTriggerEvent);
			using (wteEvent.LockForUpdatingKeyFields(false))
			{
				wteEvent.SL_Reference = $"{triggeringLogPK}|AAA|BBB|CCC|DDD|EEE|{triggerChainID}|{dummyBO.PK}";
			}

			var wteDataReloaded = new WorkflowTriggerEventData(wteEvent);

			AssertEquals("wteDataReloaded.TriggeringLogPK", triggeringLogPK, wteDataReloaded.TriggeringLogPK);
			AssertEquals("wteDataReloaded.TriggeringBranchCode", "AAA", wteDataReloaded.TriggeringBranchCode);
			AssertEquals("wteDataReloaded.TriggeringDepartmentCode", "BBB", wteDataReloaded.TriggeringDepartmentCode);
			AssertEquals("wteData.ContextStaffCode", "CCC", wteDataReloaded.ContextStaffCode);
			AssertEquals("wteData.ContextBranchCode", "DDD", wteDataReloaded.ContextBranchCode);
			AssertEquals("wteData.ContextDepartmentCode", "EEE", wteDataReloaded.ContextDepartmentCode);
			AssertEquals("wteDataReloaded.TriggerChainID", triggerChainID, wteDataReloaded.TriggerChainID);
			AssertEquals("wteDataReloaded.TriggeringLogParentPK", dummyBO.PK, wteDataReloaded.TriggeringLogParentPK);
		}

		public void TestLegacyWTE_8Value()
		{
			var triggeringLogPK = ZGuid.NewZGuid();
			var triggerChainID = ZGuid.NewZGuid();

			var dummyBO = (BusinessObject)Factory.New<IDummyWithWorkflow>();
			var wteEvent = dummyBO.GetLogs().AddNew(Events.WorkflowTriggerEvent);
			using (wteEvent.LockForUpdatingKeyFields(false))
			{
				wteEvent.SL_Reference = $"{triggeringLogPK}|AAA|BBB|{triggerChainID}|{dummyBO.PK}";
			}

			var wteDataReloaded = new WorkflowTriggerEventData(wteEvent);

			AssertEquals("wteDataReloaded.TriggeringLogPK", triggeringLogPK, wteDataReloaded.TriggeringLogPK);
			AssertEquals("wteDataReloaded.TriggeringBranchCode", "AAA", wteDataReloaded.TriggeringBranchCode);
			AssertEquals("wteDataReloaded.TriggeringDepartmentCode", "BBB", wteDataReloaded.TriggeringDepartmentCode);
			AssertEquals("wteData.ContextStaffCode", "", wteDataReloaded.ContextStaffCode);
			AssertEquals("wteData.ContextBranchCode", "", wteDataReloaded.ContextBranchCode);
			AssertEquals("wteData.ContextDepartmentCode", "", wteDataReloaded.ContextDepartmentCode);
			AssertEquals("wteDataReloaded.TriggerChainID", triggerChainID, wteDataReloaded.TriggerChainID);
			AssertEquals("wteDataReloaded.TriggeringLogParentPK", dummyBO.PK, wteDataReloaded.TriggeringLogParentPK);
		}

		public void TestUnknownWTEVersion()
		{
			var triggeringLogPK = ZGuid.NewZGuid();
			var triggerChainID = ZGuid.NewZGuid();

			var dummyBO = (BusinessObject)Factory.New<IDummyWithWorkflow>();
			var wteEvent = dummyBO.GetLogs().AddNew(Events.WorkflowTriggerEvent);
			using (wteEvent.LockForUpdatingKeyFields(false))
			{
				wteEvent.SL_Reference = $"V99|{triggeringLogPK}|AAA|BBB|CCC|DDD|EEE|{triggerChainID}|{dummyBO.PK}";
			}

			AssertExceptionThrown<ArgumentException>("Argument Exception", ReferenceStrategy.Logs.StratergyNotFound(99), () =>
			{
				new WorkflowTriggerEventData(wteEvent);
			});
		}

		public void TestDummyStratergy()
		{
			var reference = "TEST";
			var dummy = new DummyData();
			AssertExceptionThrown<ArgumentException>("Argument Exception", ReferenceStrategy.Logs.StratergyDoesNotExist(typeof(DummyData)), () =>
			{
				ReferenceStrategy.Instance.Deserialise(dummy, reference);
			});
		}

		public void TestUsageWithStmALog()
		{
			var dummyBO = (BusinessObject)Factory.New<IDummyWithWorkflow>();
			var triggeringEvent = dummyBO.GetLogs().AddNew(Events.Authorised);

			var wteData = new WorkflowTriggerEventData(triggeringEvent, triggeringEvent.SL_Parent, "BRA", "DEP", "AAA", "BBB", "CCC");

			AssertEquals("wteData.TriggeringLogPK", triggeringEvent.PK, wteData.TriggeringLogPK);
			AssertEquals("wteData.TriggeringBranchCode", "BRA", wteData.TriggeringBranchCode);
			AssertEquals("wteData.TriggeringDepartmentCode", "DEP", wteData.TriggeringDepartmentCode);
			AssertEquals("wteData.TriggerChainID", ZGuid.Empty, wteData.TriggerChainID);
			AssertEquals("wteData.TriggeringLogParentPK", dummyBO.PK, wteData.TriggeringLogParentPK);

			var wteEvent = dummyBO.GetLogs().AddNew(Events.WorkflowTriggerEvent);
			using (wteEvent.LockForUpdatingKeyFields(false))
			{
				wteEvent.SL_Reference = wteData.ToReference();
			}

			var wteDataReloaded = new WorkflowTriggerEventData(wteEvent);

			AssertEquals("wteDataReloaded.TriggeringLogPK", triggeringEvent.PK, wteDataReloaded.TriggeringLogPK);
			AssertEquals("wteDataReloaded.TriggeringBranchCode", "BRA", wteDataReloaded.TriggeringBranchCode);
			AssertEquals("wteDataReloaded.TriggeringDepartmentCode", "DEP", wteDataReloaded.TriggeringDepartmentCode);
			AssertEquals("wteDataReloaded.TriggerChainID", ZGuid.Empty, wteDataReloaded.TriggerChainID);
			AssertEquals("wteDataReloaded.TriggeringLogParentPK", dummyBO.PK, wteDataReloaded.TriggeringLogParentPK);
		}

		public void TestUsageWithIQueuedLog()
		{
			var dummyBO = (BusinessObject)Factory.New<IDummyWithWorkflow>();
			var triggeringEvent = dummyBO.GetLogs().AddNew(Events.Authorised);
			AssertCode(triggeringEvent, triggeringEvent.SL_Parent, "BRA", "DEP", "AAA", "BBB", "CCC", ZGuid.NewZGuid());
		}

		public void TestCodesWithPipe()
		{
			string[] codes = {
				"AAA",
				"|",
				"A|",
				"|A",
				"AA|",
				"|AA",
				"A|A",
				"\\",
				"A\\",
				"\\A",
				"AA\\",
				"\\AA",
				"A\\A",
				"\\|",
				"|\\",
				"|\\|",
				"|||",
				"\\\\\\"
			};

			CombineAssertions(() =>
			{
				foreach (var code in codes)
				{
					var dummyBO = (BusinessObject)Factory.New<IDummyWithWorkflow>();
					var triggeringEvent = dummyBO.GetLogs().AddNew(Events.Authorised);

					AssertCode(triggeringEvent, triggeringEvent.SL_Parent, code, code, code, code, code, ZGuid.NewZGuid());
				}
			});
		}

		void AssertCode(IWorkflowTriggerSource triggeringLog, ZGuid triggeringLogParent, ZString branchCode, ZString departmentCode, ZString contextStaffCode, ZString contextBranchCode, ZString contextDepartmentCode, ZGuid? triggerChainID = null)
		{
			AssertCode(new EventSource(triggeringLog), triggeringLogParent, branchCode, departmentCode, contextStaffCode, contextBranchCode, contextDepartmentCode);
		}

		void AssertCode(IEventSource triggeringLog, ZGuid triggeringLogParent, ZString branchCode, ZString departmentCode, ZString contextStaffCode, ZString contextBranchCode, ZString contextDepartmentCode, ZGuid? triggerChainID = null)
		{
			//Create
			var wteData = new WorkflowTriggerEventData(triggeringLog, triggeringLogParent, branchCode, departmentCode, contextStaffCode, contextBranchCode, contextDepartmentCode);
			if (triggerChainID != null)
			{
				wteData.TriggerChainID = (ZGuid)triggerChainID;
			}

			//Assert
			if (triggeringLog != null)
			{
				AssertEquals("wteData.TriggeringLogPK", triggeringLog.Identifier, wteData.TriggeringLogPK);
				AssertEquals("wteData.TriggeringLogPK", triggeringLog.SourceType, wteData.TriggeringSourceCode);
			}
			AssertEquals("wteData.TriggeringBranchCode", branchCode, wteData.TriggeringBranchCode);
			AssertEquals("wteData.TriggeringDepartmentCode", departmentCode, wteData.TriggeringDepartmentCode);
			AssertEquals("wteData.ContextStaffCode", contextStaffCode, wteData.ContextStaffCode);
			AssertEquals("wteData.ContextBranchCode", contextBranchCode, wteData.ContextBranchCode);
			AssertEquals("wteData.ContextDepartmentCode", contextDepartmentCode, wteData.ContextDepartmentCode);
			AssertEquals("wteData.TriggeringLogParentPK", triggeringLogParent, wteData.TriggeringLogParentPK);
			if (triggerChainID != null)
			{
				AssertEquals("wteData.TriggerChainID", triggerChainID, wteData.TriggerChainID);
			}

			//Serialise & Deserialise
			var queuedLog = new QueuedLogForTesting(wteData.ToReference());
			var wteDataReloaded = new WorkflowTriggerEventData(queuedLog);

			//Assert
			if (triggeringLog != null)
			{
				AssertEquals("wteDataReloaded.TriggeringLogPK", triggeringLog.Identifier, wteDataReloaded.TriggeringLogPK);
				AssertEquals("wteDataReloaded.TriggeringLogPK", triggeringLog.SourceType, wteDataReloaded.TriggeringSourceCode);
			}
			AssertEquals("wteDataReloaded.TriggeringBranchCode", branchCode, wteDataReloaded.TriggeringBranchCode);
			AssertEquals("wteDataReloaded.TriggeringDepartmentCode", departmentCode, wteDataReloaded.TriggeringDepartmentCode);
			AssertEquals("wteDataReloaded.ContextStaffCode", contextStaffCode, wteDataReloaded.ContextStaffCode);
			AssertEquals("wteDataReloaded.ContextBranchCode", contextBranchCode, wteDataReloaded.ContextBranchCode);
			AssertEquals("wteDataReloaded.ContextDepartmentCode", contextDepartmentCode, wteDataReloaded.ContextDepartmentCode);
			AssertEquals("wteDataReloaded.TriggeringLogParentPK", triggeringLogParent, wteDataReloaded.TriggeringLogParentPK);
			if (triggerChainID != null)
			{
				AssertEquals("wteDataReloaded.TriggerChainID", triggerChainID, wteDataReloaded.TriggerChainID);
			}
		}
	}
}
