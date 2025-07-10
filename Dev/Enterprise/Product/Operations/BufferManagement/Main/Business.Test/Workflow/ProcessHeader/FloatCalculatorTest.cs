using System;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	class FloatCalculatorTest : BMSTestCaseWithFactory
	{
		public void TestRecursiveRelationship_DoesNotExplode()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			workflow1.MakePrerequisiteOf(workflow2);
			var link = workflow2.PostrequisiteLinks_ForBinding.AddNew();
			link.FP_FH_HeaderTo = workflow2.PK;

			AssertNoExceptionThrown(() => ProcessHeaderFloatCalculator.CreateCalculator(jobHeader));
		}

		public void TestEarliestFinish_ShouldIncludeChildWorkflowTimes()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var grandParentWorkflow = jobHeader.ProcessHeaders[0];
			var parentWorkflow = jobHeader.ProcessHeaders.AddNew();
			var childWorkflow = jobHeader.ProcessHeaders.AddNew();

			CreateTask(grandParentWorkflow, GlbStaff.CurrentUser.GS_Code, 60);
			CreateTask(parentWorkflow, GlbStaff.CurrentUser.GS_Code, 120);
			CreateTask(parentWorkflow, GlbStaff.CurrentUser.GS_Code, 120);

			parentWorkflow.GetOrCreateLinkToParent(grandParentWorkflow);
			childWorkflow.GetOrCreateLinkToParent(parentWorkflow);

			var schedule = ProcessHeaderFloatCalculator.CreateCalculator(jobHeader).GetSchedule(grandParentWorkflow);

			AssertSchedule(
				"Parent workflow schedule should include all child workflow times",
				schedule,
				expectedFloat: 0,
				expectedEarliestStart: 0,
				expectedLatestStart: 0,
				expectedEarliestFinish: 7.5,
				expectedLatestFinish: 7.5);
		}

		public void TestEarliestFinish_ShouldIncludeChildWorkflowOpenTaskTimes()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var grandParentWorkflow = jobHeader.ProcessHeaders[0];
			var parentWorkflow = jobHeader.ProcessHeaders.AddNew();
			var childWorkflow = jobHeader.ProcessHeaders.AddNew();

			CreateTask(grandParentWorkflow, GlbStaff.CurrentUser.GS_Code, 60);
			CreateTask(parentWorkflow, GlbStaff.CurrentUser.GS_Code, 120);
			CreateTask(parentWorkflow, GlbStaff.CurrentUser.GS_Code, 120, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);

			parentWorkflow.GetOrCreateLinkToParent(grandParentWorkflow);
			childWorkflow.GetOrCreateLinkToParent(parentWorkflow);

			var schedule = ProcessHeaderFloatCalculator.CreateCalculator(jobHeader, includeClosedTaskTimes: false).GetSchedule(grandParentWorkflow);

			AssertSchedule(
				"Parent workflow schedule should include all child workflow times",
				schedule,
				expectedFloat: 0,
				expectedEarliestStart: 0,
				expectedLatestStart: 0,
				expectedEarliestFinish: 4.5,
				expectedLatestFinish: 4.5);
		}

		[TestDate(2013, 12, 11, 9, 0, 0)]
		public void TestFloat() // Corresponds to Latest Start Pass on the Float Calcs doc of WI00050044
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var workflow3 = jobHeader.ProcessHeaders.AddNew();
			var workflow4 = jobHeader.ProcessHeaders.AddNew();
			var workflow5 = jobHeader.ProcessHeaders.AddNew();
			var workflow6 = jobHeader.ProcessHeaders.AddNew();

			workflow1.FH_CompletionStatement = "workflow1";
			workflow2.FH_CompletionStatement = "workflow2";
			workflow3.FH_CompletionStatement = "workflow3";
			workflow4.FH_CompletionStatement = "workflow4";
			workflow5.FH_CompletionStatement = "workflow5";
			workflow6.FH_CompletionStatement = "workflow6";

			workflow1.MakePrerequisiteOf(workflow2);
			workflow1.MakePrerequisiteOf(workflow4);
			workflow2.MakePrerequisiteOf(workflow3);
			workflow3.MakePrerequisiteOf(workflow6);
			workflow4.MakePrerequisiteOf(workflow5);
			workflow5.MakePrerequisiteOf(workflow6);

			CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 3 * 60, estVariationFactor: 1);
			CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 2 * 60, estVariationFactor: 1);
			CreateTask(workflow3, GlbStaff.CurrentUser.GS_Code, 4 * 60, estVariationFactor: 1);
			CreateTask(workflow4, GlbStaff.CurrentUser.GS_Code, 3 * 60, estVariationFactor: 1);
			CreateTask(workflow5, GlbStaff.CurrentUser.GS_Code, 5 * 60, estVariationFactor: 1);
			CreateTask(workflow6, GlbStaff.CurrentUser.GS_Code, 3 * 60, estVariationFactor: 1);

			Factory.Save();

			var calculator = ProcessHeaderFloatCalculator.CreateCalculator(jobHeader);

			AssertEquals(true, ((IProposedNetworkEntity)workflow1).IsOnCriticalPath);
			AssertEquals(false, ((IProposedNetworkEntity)workflow2).IsOnCriticalPath);
			AssertEquals(false, ((IProposedNetworkEntity)workflow3).IsOnCriticalPath);
			AssertEquals(true, ((IProposedNetworkEntity)workflow4).IsOnCriticalPath);
			AssertEquals(true, ((IProposedNetworkEntity)workflow5).IsOnCriticalPath);
			AssertEquals(true, ((IProposedNetworkEntity)workflow6).IsOnCriticalPath);

			AssertSchedule("Is on critical path, so zero float", calculator.GetSchedule(workflow1), 0, 0, 0, 3, 3);
			AssertSchedule("Non-critical path", calculator.GetSchedule(workflow2), 2, 3, 5, 5, 7);
			AssertSchedule("Non-critical path", calculator.GetSchedule(workflow3), 2, 5, 7, 9, 11);
			AssertSchedule("Is on critical path, so zero float", calculator.GetSchedule(workflow4), 0, 3, 3, 6, 6);
			AssertSchedule("Is on critical path, so zero float", calculator.GetSchedule(workflow5), 0, 6, 6, 11, 11);
			AssertSchedule("Is on critical path, so zero float", calculator.GetSchedule(workflow6), 0, 11, 11, 14, 14);
		}

		[TestDate(2013, 12, 12, 8, 30, 0)]
		public void TestFloat_WithAgreedDeliveryDate_NotEnoughTimeToCompleteCriticalPath() // Corresponds to Agreed Delivery Date Pass on the Float Calcs doc of WI00050044
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_RL_NKHomePort = "GBLON"; // Using UTC zone because converting between local/UTC increments local time by zone offset, except that TestDateAttribute makes UTC and local both the same...

			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var workflow3 = jobHeader.ProcessHeaders.AddNew();
			var workflow4 = jobHeader.ProcessHeaders.AddNew();
			var workflow5 = jobHeader.ProcessHeaders.AddNew();
			var workflow6 = jobHeader.ProcessHeaders.AddNew();

			workflow1.FH_CompletionStatement = "workflow1";
			workflow2.FH_CompletionStatement = "workflow2";
			workflow3.FH_CompletionStatement = "workflow3";
			workflow4.FH_CompletionStatement = "workflow4";
			workflow5.FH_CompletionStatement = "workflow5";
			workflow6.FH_CompletionStatement = "workflow6";

			workflow3.FH_AgreedDeliveryDate = new ZDateTime(2013, 12, 13, 10, 30, 0, DateTimeKind.Utc); // 10 working hours later
			workflow4.FH_AgreedDeliveryDate = ZDateTime.UtcNow.AddHours(5);

			workflow1.MakePrerequisiteOf(workflow2);
			workflow1.MakePrerequisiteOf(workflow4);
			workflow2.MakePrerequisiteOf(workflow3);
			workflow3.MakePrerequisiteOf(workflow6);
			workflow4.MakePrerequisiteOf(workflow5);
			workflow5.MakePrerequisiteOf(workflow6);

			CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 3 * 60, estVariationFactor: 1);
			CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 2 * 60, estVariationFactor: 1);
			CreateTask(workflow3, GlbStaff.CurrentUser.GS_Code, 4 * 60, estVariationFactor: 1);
			CreateTask(workflow4, GlbStaff.CurrentUser.GS_Code, 3 * 60, estVariationFactor: 1);
			CreateTask(workflow5, GlbStaff.CurrentUser.GS_Code, 5 * 60, estVariationFactor: 1);
			CreateTask(workflow6, GlbStaff.CurrentUser.GS_Code, 3 * 60, estVariationFactor: 1);

			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var calculator = ProcessHeaderFloatCalculator.CreateCalculator(jobHeader);

				AssertEquals(true, ((IProposedNetworkEntity)workflow1).IsOnCriticalPath);
				AssertEquals(false, ((IProposedNetworkEntity)workflow2).IsOnCriticalPath);
				AssertEquals(false, ((IProposedNetworkEntity)workflow3).IsOnCriticalPath);
				AssertEquals(true, ((IProposedNetworkEntity)workflow4).IsOnCriticalPath);
				AssertEquals(true, ((IProposedNetworkEntity)workflow5).IsOnCriticalPath);
				AssertEquals(true, ((IProposedNetworkEntity)workflow6).IsOnCriticalPath);

				CombineAssertions(() =>
				{
					AssertSchedule("workflow1 is on critical path, so was pushed into negative float by workflow4 critical path hit", calculator.GetSchedule(workflow1), 0, 0, 0, 3, 3, new DeliveryDateThreat(workflow4, 1.5m));
					AssertSchedule("workflow2 is not on the critical path, so still has a float value, but was brought forward by Agreed Delivery Date hit", calculator.GetSchedule(workflow2), 2, 3, 5, 5, 7, new DeliveryDateThreat(workflow3, 1.5m));
					AssertSchedule("workflow3 is not on the critical path, so still has a float value, but was brought forward by Agreed Delivery Date hit", calculator.GetSchedule(workflow3), 2, 5, 7, 9, 11, new DeliveryDateThreat(workflow3, 1.5m));
					AssertSchedule("workflow4 is on critical path, but has a Agreed Delivery Date it cannot meet, so has negative float", calculator.GetSchedule(workflow4), 0, 3, 3, 6, 6, new DeliveryDateThreat(workflow4, 1.5m));
					AssertSchedule("workflow5 is on critical path, so zero float", calculator.GetSchedule(workflow5), 0, 6, 6, 11, 11);
					AssertSchedule("workflow6 is on critical path, so zero float", calculator.GetSchedule(workflow6), 0, 11, 11, 14, 14);
				});
			}
		}

		[TestDate(2013, 12, 12, 8, 30, 0)]
		public void TestFloat_WithAgreedDeliveryDate_WithEnoughTimeToCompleteCriticalPath() // Corresponds to Agreed Delivery Date Pass on the Float Calcs doc of WI00050044, with an additional hit on workflow4/d
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_RL_NKHomePort = "GBLON"; // Using UTC zone because converting between local/UTC increments local time by zone offset, except that TestDateAttribute makes UTC and local both the same...

			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var workflow3 = jobHeader.ProcessHeaders.AddNew();
			var workflow4 = jobHeader.ProcessHeaders.AddNew();
			var workflow5 = jobHeader.ProcessHeaders.AddNew();
			var workflow6 = jobHeader.ProcessHeaders.AddNew();

			workflow1.FH_CompletionStatement = "workflow1";
			workflow2.FH_CompletionStatement = "workflow2";
			workflow3.FH_CompletionStatement = "workflow3";
			workflow4.FH_CompletionStatement = "workflow4";
			workflow5.FH_CompletionStatement = "workflow5";
			workflow6.FH_CompletionStatement = "workflow6";

			workflow3.FH_AgreedDeliveryDate = new ZDateTime(2013, 12, 13, 10, 30, 0, DateTimeKind.Utc); // 10 working hours later
			workflow4.FH_AgreedDeliveryDate = ZDateTime.UtcNow.AddDays(2);

			workflow1.MakePrerequisiteOf(workflow2);
			workflow1.MakePrerequisiteOf(workflow4);
			workflow2.MakePrerequisiteOf(workflow3);
			workflow3.MakePrerequisiteOf(workflow6);
			workflow4.MakePrerequisiteOf(workflow5);
			workflow5.MakePrerequisiteOf(workflow6);

			CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 3 * 60, estVariationFactor: 1);
			CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 2 * 60, estVariationFactor: 1);
			CreateTask(workflow3, GlbStaff.CurrentUser.GS_Code, 4 * 60, estVariationFactor: 1);
			CreateTask(workflow4, GlbStaff.CurrentUser.GS_Code, 3 * 60, estVariationFactor: 1);
			CreateTask(workflow5, GlbStaff.CurrentUser.GS_Code, 5 * 60, estVariationFactor: 1);
			CreateTask(workflow6, GlbStaff.CurrentUser.GS_Code, 3 * 60, estVariationFactor: 1);

			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var calculator = ProcessHeaderFloatCalculator.CreateCalculator(jobHeader);

				AssertEquals(true, ((IProposedNetworkEntity)workflow1).IsOnCriticalPath);
				AssertEquals(false, ((IProposedNetworkEntity)workflow2).IsOnCriticalPath);
				AssertEquals(false, ((IProposedNetworkEntity)workflow3).IsOnCriticalPath);
				AssertEquals(true, ((IProposedNetworkEntity)workflow4).IsOnCriticalPath);
				AssertEquals(true, ((IProposedNetworkEntity)workflow5).IsOnCriticalPath);
				AssertEquals(true, ((IProposedNetworkEntity)workflow6).IsOnCriticalPath);

				CombineAssertions(() =>
				{
					AssertSchedule("workflow1 is on critical path, so zero float", calculator.GetSchedule(workflow1), 0, 0, 0, 3, 3);
					AssertSchedule("workflow2 is not on the critical path, so still has a float value, but was brought forward by Agreed Delivery Date hit", calculator.GetSchedule(workflow2), 2, 3, 5, 5, 7, new DeliveryDateThreat(workflow3, 1.5m));
					AssertSchedule("workflow3 is not on the critical path, so still has a float value, but was brought forward by Agreed Delivery Date hit", calculator.GetSchedule(workflow3), 2, 5, 7, 9, 11, new DeliveryDateThreat(workflow3, 1.5m));
					AssertSchedule("workflow4 is on critical path, so zero float", calculator.GetSchedule(workflow4), 0, 3, 3, 6, 6);
					AssertSchedule("workflow5 is on critical path, so zero float", calculator.GetSchedule(workflow5), 0, 6, 6, 11, 11);
					AssertSchedule("workflow6 is on critical path, so zero float", calculator.GetSchedule(workflow6), 0, 11, 11, 14, 14);
				});
			}
		}

		[TestDate(2013, 12, 12, 8, 30, 0)]
		public void TestFloat_MeshNetwork_WithAgreedDeliveryDate_NotEnoughTimeToCompleteCriticalPath()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_RL_NKHomePort = "GBLON"; // Using UTC zone because converting between local/UTC increments local time by zone offset, except that TestDateAttribute makes UTC and local both the same...

			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var workflow3 = jobHeader.ProcessHeaders.AddNew();
			var workflow4 = jobHeader.ProcessHeaders.AddNew();
			var workflow5 = jobHeader.ProcessHeaders.AddNew();
			var workflow6 = jobHeader.ProcessHeaders.AddNew();

			workflow1.FH_CompletionStatement = "workflow1";
			workflow2.FH_CompletionStatement = "workflow2";
			workflow3.FH_CompletionStatement = "workflow3";
			workflow4.FH_CompletionStatement = "workflow4";
			workflow5.FH_CompletionStatement = "workflow5";
			workflow6.FH_CompletionStatement = "workflow6";

			workflow3.FH_AgreedDeliveryDate = new ZDateTime(2013, 12, 13, 10, 30, 0, DateTimeKind.Utc); // 10 working hours later
			workflow4.FH_AgreedDeliveryDate = ZDateTime.UtcNow.AddHours(5);

			workflow1.MakePrerequisiteOf(workflow2);
			workflow1.MakePrerequisiteOf(workflow4);
			workflow2.MakePrerequisiteOf(workflow3);
			workflow3.MakePrerequisiteOf(workflow6);
			workflow4.MakePrerequisiteOf(workflow3);
			workflow4.MakePrerequisiteOf(workflow5);
			workflow5.MakePrerequisiteOf(workflow6);

			CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 3 * 60, estVariationFactor: 1);
			CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 2 * 60, estVariationFactor: 1);
			CreateTask(workflow3, GlbStaff.CurrentUser.GS_Code, 4 * 60, estVariationFactor: 1);
			CreateTask(workflow4, GlbStaff.CurrentUser.GS_Code, 3 * 60, estVariationFactor: 1);
			CreateTask(workflow5, GlbStaff.CurrentUser.GS_Code, 5 * 60, estVariationFactor: 1);
			CreateTask(workflow6, GlbStaff.CurrentUser.GS_Code, 3 * 60, estVariationFactor: 1);

			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var calculator = ProcessHeaderFloatCalculator.CreateCalculator(jobHeader);

				AssertEquals(true, ((IProposedNetworkEntity)workflow1).IsOnCriticalPath);
				AssertEquals(false, ((IProposedNetworkEntity)workflow2).IsOnCriticalPath);
				AssertEquals(false, ((IProposedNetworkEntity)workflow3).IsOnCriticalPath);
				AssertEquals(true, ((IProposedNetworkEntity)workflow4).IsOnCriticalPath);
				AssertEquals(true, ((IProposedNetworkEntity)workflow5).IsOnCriticalPath);
				AssertEquals(true, ((IProposedNetworkEntity)workflow6).IsOnCriticalPath);

				CombineAssertions(() =>
				{
					AssertSchedule("workflow1 is on critical path, so was pushed into negative float by workflow4 critical path hit", calculator.GetSchedule(workflow1), 0, 0, 0, 3, 3, new DeliveryDateThreat(workflow4, 1.5m));
					AssertSchedule("workflow2 is not on the critical path, so still has a float value, but was brought forward by Agreed Delivery Date hit", calculator.GetSchedule(workflow2), 2, 3, 5, 5, 7, new DeliveryDateThreat(workflow3, 1.5m));
					AssertSchedule("workflow3 is not on the critical path, but it's prereq of workflow4 means that it no longer has a float, since it was already brought forward by Agreed Delivery Date hit", calculator.GetSchedule(workflow3), 1, 6, 7, 10, 11, new DeliveryDateThreat(workflow3, 1.5m));
					AssertSchedule("workflow4 is on critical path, but has a Agreed Delivery Date it cannot meet, so has negative float", calculator.GetSchedule(workflow4), 0, 3, 3, 6, 6, new DeliveryDateThreat(workflow4, 1.5m));
					AssertSchedule("workflow5 is on critical path, so zero float", calculator.GetSchedule(workflow5), 0, 6, 6, 11, 11);
					AssertSchedule("workflow6 is on critical path, so zero float", calculator.GetSchedule(workflow6), 0, 11, 11, 14, 14);
				});
			}
		}

		void AssertSchedule(string message, ScheduleNode schedule, double expectedFloat, double expectedEarliestStart, double expectedLatestStart, double expectedEarliestFinish, double expectedLatestFinish, DeliveryDateThreat expectedThreat = null)
		{
			AssertEquals(message + ": FloatHours", (decimal)expectedFloat, schedule.FloatHours);
			AssertEquals(message + ": EarliestStartHours", (decimal)expectedEarliestStart, schedule.EarliestStartHours);
			AssertEquals(message + ": LatestStartHours", (decimal)expectedLatestStart, schedule.LatestStartHours);
			AssertEquals(message + ": EarliestFinishHours", (decimal)expectedEarliestFinish, schedule.EarliestFinishHours);
			AssertEquals(message + ": LatestFinishHours", (decimal)expectedLatestFinish, schedule.LatestFinishHours);

			if (expectedThreat == null)
			{
				AssertNull(message + ": DeliveryDateThreat", schedule.DeliveryDateThreat);
			}
			else
			{
				AssertNotNull(message + ": DeliveryDateThreat", schedule.DeliveryDateThreat);
				if (schedule.DeliveryDateThreat != null)
				{
					AssertEquals(message + ": DeliveryDateThreat.ResponsibleEntity", expectedThreat.ResponsibleEntity, schedule.DeliveryDateThreat.ResponsibleEntity);
					AssertEquals(message + ": DeliveryDateThreat.FloatConsumption", expectedThreat.FloatConsumption, schedule.DeliveryDateThreat.FloatConsumption);
				}
			}
		}
	}
}
