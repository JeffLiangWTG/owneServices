using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.WorkflowManager.ServiceTasks.Testing
{
	sealed class NotificationAndTriggeringLogsTest : TestCaseWithFactory
	{
		public void TestCtorAndProperties()
		{
			ProcessTaskNotification notification = Factory.New<ProcessTaskNotification>();
			NotificationAndTriggeringLogs notificationLogs = new NotificationAndTriggeringLogs(notification);
			AssertEquals(notification, notificationLogs.Notification);
			AssertContainsExactElementsInAnyOrder(Array.Empty<StmChangeLog>(), notificationLogs.Logs);

			StmChangeLog changeLog = Factory.New<StmChangeLog>();
			notificationLogs = new NotificationAndTriggeringLogs(notification, changeLog);
			AssertEquals(notification, notificationLogs.Notification);
			AssertContainsExactElementsInAnyOrder(new StmChangeLog[] { changeLog }, notificationLogs.Logs);
		}

		public void TestAddChangeLog()
		{
			ProcessTaskNotification notification = Factory.New<ProcessTaskNotification>();
			StmChangeLog changeLog = Factory.New<StmChangeLog>();

			NotificationAndTriggeringLogs notificationLogs = new NotificationAndTriggeringLogs(notification, changeLog);
			AssertContainsExactElementsInAnyOrder(new StmChangeLog[] { changeLog }, notificationLogs.Logs);

			StmChangeLog changeLog1 = Factory.New<StmChangeLog>();
			StmChangeLog changeLog2 = Factory.New<StmChangeLog>();

			notificationLogs.AddChangeLog(changeLog);
			notificationLogs.AddChangeLog(changeLog1);
			notificationLogs.AddChangeLog(changeLog2);
			notificationLogs.AddChangeLog(changeLog2);
			AssertContainsExactElementsInAnyOrder("No duplicates", new StmChangeLog[] { changeLog, changeLog1, changeLog2 }, notificationLogs.Logs);
		}

		public void TestMatchesNotification()
		{
			#region Test of logic for null parent match
			ProcessTaskNotification actionNullParent = Factory.New<ProcessTaskNotification>();
			ProcessTaskNotification actionNullParent1 = Factory.New<ProcessTaskNotification>();
			AssertEquals("Test of logic for null parent match", true, new NotificationAndTriggeringLogs(actionNullParent).MatchesNotification(actionNullParent1));
			#endregion

			#region Test of logic for null parent match for XUE
			actionNullParent = Factory.New<ProcessTaskNotification>();
			actionNullParent.PQ_TriggerType = "XUE";
			actionNullParent1 = Factory.New<ProcessTaskNotification>();
			actionNullParent1.PQ_TriggerType = "XUE";
			AssertEquals("Test of logic for null parent match for XUE", true, new NotificationAndTriggeringLogs(actionNullParent).MatchesNotification(actionNullParent1));
			#endregion

			#region Test of logic for normal matching
			Func<ProcessTask, string, string, ProcessTaskNotification> createNotification = (task, triggerType, triggerParty) =>
			{
				ProcessTaskNotification result = task.ProcessTaskNotifications.AddNew();
				result.PQ_TriggerType = triggerType;
				result.PQ_TriggerParty = triggerParty;
				return result;
			};

			ZGuid parentID = ZGuid.NewZGuid();
			ZGuid anotherParentID = ZGuid.NewZGuid();

			var dummy1 = Factory.New<DummyWithWorkflow>();
			var dummy2 = Factory.New<DummyWithWorkflow>();

			var task1 = dummy1.WorkflowItems.Triggers.AddNew();
			var task2 = dummy1.WorkflowItems.Triggers.AddNew();
			var anotherTask1 = dummy2.WorkflowItems.Triggers.AddNew();

			ProcessTaskNotification action11 = createNotification(task1, "AAA", "BBB");
			ProcessTaskNotification action12 = createNotification(task1, "AAA", "BBB");
			ProcessTaskNotification action13 = createNotification(task1, "XXX", "BBB");

			ProcessTaskNotification action21 = createNotification(task2, "AAA", "BBB");
			ProcessTaskNotification action22 = createNotification(task2, "AAA", "ZZZ");

			ProcessTaskNotification anotherAction11 = createNotification(anotherTask1, "AAA", "BBB");
			ProcessTaskNotification anotherAction12 = createNotification(anotherTask1, "AAA", "BBB");
			ProcessTaskNotification anotherAction13 = createNotification(anotherTask1, "XXX", "BBB");

			NotificationAndTriggeringLogs notificationLogs = new NotificationAndTriggeringLogs(action11);
			AssertEquals(true, notificationLogs.MatchesNotification(action11));
			AssertEquals(true, notificationLogs.MatchesNotification(action12));
			AssertEquals(false, notificationLogs.MatchesNotification(action13));
			AssertEquals(true, notificationLogs.MatchesNotification(action21));
			AssertEquals(false, notificationLogs.MatchesNotification(action22));
			AssertEquals(false, notificationLogs.MatchesNotification(anotherAction11));
			AssertEquals(false, notificationLogs.MatchesNotification(anotherAction12));
			AssertEquals(false, notificationLogs.MatchesNotification(anotherAction13));
			#endregion

			#region Test of logic for SetField Action Logic
			Func<ProcessTask, string, string, ProcessTaskNotification> createSetFieldAction = (task, fieldName, fieldValue) =>
			{
				ProcessTaskNotification result = task.ProcessTaskNotifications.AddNew();
				result.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
				result.PQ_FieldName = fieldName;
				result.PQ_FieldValue = fieldValue;
				return result;
			};

			ProcessTaskNotification setFieldAction1 = createSetFieldAction(task1, "Z0_VarCharMax", "AAA");
			ProcessTaskNotification setFieldAction2 = createSetFieldAction(task1, "Z0_VarCharMax", "AAA");
			ProcessTaskNotification setFieldAction3 = createSetFieldAction(task1, "Z0_VarCharMax", "BBB");
			ProcessTaskNotification setFieldAction4 = createSetFieldAction(task1, "Z0_AnotherText", "AAA");

			notificationLogs = new NotificationAndTriggeringLogs(setFieldAction1);
			AssertEquals(true, notificationLogs.MatchesNotification(setFieldAction1));
			AssertEquals(true, notificationLogs.MatchesNotification(setFieldAction2));
			AssertEquals(false, notificationLogs.MatchesNotification(setFieldAction3));
			AssertEquals(false, notificationLogs.MatchesNotification(setFieldAction4));
			#endregion

			#region Test of SendUniversalEventXML Action
			Func<ProcessTask, string, bool, ProcessTaskNotification> createSendUniversalEventXMLAction = (task, triggerParty, withEDoc) =>
			{
				ProcessTaskNotification result = task.ProcessTaskNotifications.AddNew();
				result.PQ_TriggerType = withEDoc ? WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXMLWithEDoc : WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML;
				result.PQ_TriggerParty = triggerParty;
				return result;
			};

			ProcessTask task_ARV_ARV = Factory.NewWithValidTestData<ProcessTask>();
			task_ARV_ARV.P9_Type = "TRG";
			task_ARV_ARV.P9_ParentID = new ZGuid();
			task_ARV_ARV.TriggerConditions.TriggerEventCode = "ARV";
			task_ARV_ARV.TriggerConditions.TriggerFieldName = "JS_E_ARV";
			ProcessTask task_DEP_DEP = Factory.NewWithValidTestData<ProcessTask>();
			task_DEP_DEP.P9_Type = "TRG";
			task_DEP_DEP.P9_ParentID = new ZGuid();
			task_DEP_DEP.TriggerConditions.TriggerEventCode = "DEP";
			task_DEP_DEP.TriggerConditions.TriggerFieldName = "JS_E_DEP";
			ProcessTask task_ARV_DEP = Factory.NewWithValidTestData<ProcessTask>();
			task_ARV_DEP.P9_Type = "TRG";
			task_ARV_DEP.P9_ParentID = new ZGuid();
			task_ARV_DEP.TriggerConditions.TriggerEventCode = "ARV";
			task_ARV_DEP.TriggerConditions.TriggerFieldName = "JS_E_DEP";

			ProcessTaskNotification xUEAction1 = createSendUniversalEventXMLAction(task_ARV_ARV, "ORP", false);
			ProcessTaskNotification xUEAction2 = createSendUniversalEventXMLAction(task_DEP_DEP, "ORP", false);
			ProcessTaskNotification xUEAction3 = createSendUniversalEventXMLAction(task_DEP_DEP, "SHP", false);
			ProcessTaskNotification xUEAction4 = createSendUniversalEventXMLAction(task_ARV_DEP, "ORP", false);

			CombineAssertions(() =>
			{
				notificationLogs = new NotificationAndTriggeringLogs(xUEAction1);
				AssertEquals("test_XUE_11", true, notificationLogs.MatchesNotification(xUEAction1));
				AssertEquals("test_XUE_12", false, notificationLogs.MatchesNotification(xUEAction2));
				AssertEquals("test_XUE_13", false, notificationLogs.MatchesNotification(xUEAction3));
				AssertEquals("test_XUE_14", true, notificationLogs.MatchesNotification(xUEAction4));
				notificationLogs = new NotificationAndTriggeringLogs(xUEAction2);
				AssertEquals("test_XUE_22", true, notificationLogs.MatchesNotification(xUEAction2));
				AssertEquals("test_XUE_23", false, notificationLogs.MatchesNotification(xUEAction3));
				AssertEquals("test_XUE_24", false, notificationLogs.MatchesNotification(xUEAction4));
				notificationLogs = new NotificationAndTriggeringLogs(xUEAction3);
				AssertEquals("test_XUE_33", true, notificationLogs.MatchesNotification(xUEAction3));
				AssertEquals("test_XUE_34", false, notificationLogs.MatchesNotification(xUEAction4));
				notificationLogs = new NotificationAndTriggeringLogs(xUEAction4);
				AssertEquals("test_XUE_44", true, notificationLogs.MatchesNotification(xUEAction4));
			});

			xUEAction1 = createSendUniversalEventXMLAction(task_ARV_ARV, "ORP", true);
			xUEAction2 = createSendUniversalEventXMLAction(task_DEP_DEP, "ORP", true);
			xUEAction3 = createSendUniversalEventXMLAction(task_DEP_DEP, "SHP", true);
			xUEAction4 = createSendUniversalEventXMLAction(task_ARV_DEP, "ORP", true);

			CombineAssertions(() =>
			{
				notificationLogs = new NotificationAndTriggeringLogs(xUEAction1);
				AssertEquals("test_XUD_11", true, notificationLogs.MatchesNotification(xUEAction1));
				AssertEquals("test_XUD_12", false, notificationLogs.MatchesNotification(xUEAction2));
				AssertEquals("test_XUD_13", false, notificationLogs.MatchesNotification(xUEAction3));
				AssertEquals("test_XUD_14", true, notificationLogs.MatchesNotification(xUEAction4));
				notificationLogs = new NotificationAndTriggeringLogs(xUEAction2);
				AssertEquals("test_XUD_22", true, notificationLogs.MatchesNotification(xUEAction2));
				AssertEquals("test_XUD_23", false, notificationLogs.MatchesNotification(xUEAction3));
				AssertEquals("test_XUD_24", false, notificationLogs.MatchesNotification(xUEAction4));
				notificationLogs = new NotificationAndTriggeringLogs(xUEAction3);
				AssertEquals("test_XUD_33", true, notificationLogs.MatchesNotification(xUEAction3));
				AssertEquals("test_XUD_34", false, notificationLogs.MatchesNotification(xUEAction4));
				notificationLogs = new NotificationAndTriggeringLogs(xUEAction4);
				AssertEquals("test_XUD_44", true, notificationLogs.MatchesNotification(xUEAction4));
			});
			#endregion

			#region some special case test
			task1 = Factory.NewWithValidTestData<ProcessTask>();
			task1.P9_Type = "TRG";
			task1.P9_ParentID = new ZGuid();
			task1.TriggerConditions.TriggerEventCode = "DEP";
			task1.TriggerConditions.TriggerFieldName = "JS_E_DEP";
			task2 = Factory.NewWithValidTestData<ProcessTask>();
			task2.P9_Type = "TRG";
			task2.P9_ParentID = new ZGuid();
			task2.TriggerConditions.TriggerEventCode = "";
			task2.TriggerConditions.TriggerFieldName = "JS_E_DEP";

			var action1 = createSendUniversalEventXMLAction(task1, "ORP", false);
			var action2 = createSendUniversalEventXMLAction(task2, "ORP", false);

			notificationLogs = new NotificationAndTriggeringLogs(action1);
			AssertEquals(false, notificationLogs.MatchesNotification(action2));
			#endregion
		}
	}
}
