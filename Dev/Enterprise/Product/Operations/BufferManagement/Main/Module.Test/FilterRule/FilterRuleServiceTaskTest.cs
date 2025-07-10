using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	class FilterRuleServiceTaskTest : BMSTestCaseWithFactory
	{
		[TestDate(2014, 7, 2)]
		public void TestRun_FilterRuleIgnoresWorkflows()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");

			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "dAN", "Daniel Keogh");

			var bucket = BMSTestHelper.CreateBucket(system);
			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");
			var link = BMSTestHelper.LinkComponents(bucket, buffer);

			FilterStripsTestHelper.AddFilterStrips(link.FilterRule,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Look out!",
			});

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);

			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_FC_CurrentComponent = buffer.PK;
			workflow1.FH_CompletionStatement = "Look out!";
			var task1 = BMSTestHelper.CreateTask(workflow1, staff.GS_Code, 9000);

			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow2.FH_FC_CurrentComponent = bucket.PK;
			workflow2.FH_CompletionStatement = "Look out!!";
			workflow2.FH_ReleaseDateTime = ZDateTime.UtcNow.AddHours(-1);
			var task2 = BMSTestHelper.CreateTask(workflow2, staff.GS_Code, 10);

			var workflow3 = jobHeader.ProcessHeaders.AddNew();
			workflow3.FH_FC_CurrentComponent = bucket.PK;
			workflow3.FH_CompletionStatement = "Surely this workflow will be ignored!";
			workflow3.FH_ReleaseDateTime = ZDateTime.UtcNow;
			var task3 = BMSTestHelper.CreateTask(workflow3, staff.GS_Code, 10);

			Factory.Save();

			ReleaseGateKeeperTest.RunReleaseGate(system, failureLogService: ReleaseLogFailureServiceForTest);

			workflow2.Reload();
			AssertEquals(bucket, workflow2.CurrentComponent);
			AssertMultilineASCIIEquals("",
@"There is insufficient capacity to release this workflow. The required capacity has been reserved within this Release Gate run to prevent other workflows later in the queue jumping in front of this one inappropriately. Details of relevant resource capacity are listed below.
	Daniel Keogh: required 0.25 hours, currently has -177 hours of available capacity at 1st place in the queue for this resource.
		Tasks assigned: T00001001 (0.25 hours)".StripTaskIds(), workflow2.GetReleaseFailureReasonForBuffer(buffer).StripTaskIds());

			task1.P9_EstDuration = new ZInt(6000).GetDateTimeFromMinutes();
			Factory.Save();

			ReleaseGateKeeperTest.RunReleaseGate(system, failureLogService: ReleaseLogFailureServiceForTest);

			AssertMultilineASCIIEquals("Re-running the release gate should update the message",
@"There is insufficient capacity to release this workflow. The required capacity has been reserved within this Release Gate run to prevent other workflows later in the queue jumping in front of this one inappropriately. Details of relevant resource capacity are listed below.
	Daniel Keogh: required 0.25 hours, currently has -102 hours of available capacity at 1st place in the queue for this resource.
		Tasks assigned: T00001001 (0.25 hours)".StripTaskIds(), workflow2.GetReleaseFailureReasonForBuffer(buffer).StripTaskIds());

			AssertEquals(ProcessHeader.ReleaseFailureReasonNotAvailableMessage, workflow3.GetReleaseFailureReasonForBuffer(buffer));
		}

		[TestDate(2014, 1, 15)]
		public void TestFilterRule_ByWorkflowType_WithMultipleWorkflowTypesOnSingleLinks()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "FRO", "Frodo Baggins");

			var system = BMSTestHelper.CreateSystem(Factory, "ORG", "INQ", "OPP");

			var bucket = BMSTestHelper.CreateBucket(system, "bucket");
			var buffer1 = BMSTestHelper.CreateBuffer(system, "buffer1");
			var buffer2 = BMSTestHelper.CreateBuffer(system, "buffer2");

			var link1 = BMSTestHelper.LinkComponents(bucket, buffer1, 0);
			var link2 = BMSTestHelper.LinkComponents(bucket, buffer2, 1);

			FilterStripsTestHelper.AddFilterStrips(link1.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.WorkflowType,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "ORG",
				OrCategory = FilterOrCategory.Blue,
			}, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.WorkflowType,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "INQ",
				OrCategory = FilterOrCategory.Blue,
			});

			FilterStripsTestHelper.AddFilterStrips(link2.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.WorkflowType,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "OPP"
			});

			var orgWorkflow = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory).ProcessHeaders[0];
			BMSTestHelper.CreateTask(orgWorkflow, staff.GS_Code, 60);

			var enquiryWorkflow = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory).ProcessHeaders[0];
			BMSTestHelper.CreateTask(enquiryWorkflow, staff.GS_Code, 60);

			var oppWorkflow = BMSTestHelper.CreateJobHeader<OrgOpportunity>(Factory).ProcessHeaders[0];
			BMSTestHelper.CreateTask(oppWorkflow, staff.GS_Code, 60);

			Factory.Save();

			ReleaseGateKeeperTest.RunReleaseGate(system, failureLogService: ReleaseLogFailureServiceForTest);

			orgWorkflow.Reload();
			enquiryWorkflow.Reload();
			oppWorkflow.Reload();

			CombineAssertions(() =>
			{
				AssertEquals("Organisation workflow should be routed to buffer1", buffer1.PK, orgWorkflow.FH_FC_CurrentComponent);
				AssertEquals("Sales Inquiry workflow should be routed to buffer1", buffer1.PK, enquiryWorkflow.FH_FC_CurrentComponent);
				AssertEquals("Org Opportunity workflow should be routed to buffer1", buffer2.PK, oppWorkflow.FH_FC_CurrentComponent);
			});
		}

		[TestDate(2014, 4, 13)]
		public void TestFilterRule_AuditColumns()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket1 = BMSTestHelper.CreateBucket(system, "bucket1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "bucket2");

			var link = BMSTestHelper.LinkComponents(bucket1, bucket2);

			FilterStripsTestHelper.AddFilterStrips(link.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Last Edit Time",
				FilterStripValueSetter = f => ((ModuleDateFilter)f).PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Today,
			});

			var workflow1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory).ProcessHeaders.AddNew();
			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(2);

			var workflow2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory).ProcessHeaders.AddNew();
			Factory.Save();

			AssertEquals(ZDateTime.UtcNow.AddDays(-2), workflow1.FH_SystemLastEditTimeUtc);
			AssertEquals(ZDateTime.UtcNow, workflow2.FH_SystemLastEditTimeUtc);

			new TestTransferRuleRunner(system, new DummyLogger()).Process_ForTest();

			workflow1.Reload();
			workflow2.Reload();

			AssertEquals("workflow1 is outside the 'today' filter range, so should not have been moved", bucket1, workflow1.CurrentComponent);
			AssertEquals("workflow1 is inside the 'today' filter range, so should have been moved", bucket2, workflow2.CurrentComponent);
		}

		public void TestFilters_ShouldUseJobOrWorkflowOptimisation_ReleaseGate()
		{
			AssertFilters_ShouldUseJobOrWorkflowOptimisation(system => ReleaseGateKeeperTest.RunReleaseGate(system));
		}

		public void TestFilters_ShouldUseJobOrWorkflowOptimisation_TransferRuleRunner()
		{
			AssertFilters_ShouldUseJobOrWorkflowOptimisation(system => new TestTransferRuleRunner(system, new DummyLogger()).Process_ForTest());
		}

		void AssertFilters_ShouldUseJobOrWorkflowOptimisation(Action<BMSystem> runProcessorAction)
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var otherBucket = BMSTestHelper.CreateBucket(config.System, "other bucket");
			var otherLink = BMSTestHelper.LinkComponents(otherBucket, config.Bucket);
			FilterStripsTestHelper.AddFilterStrip<ModuleGuidFilter>(config.ComponentLink.FilterRule, ProcessHeader.ModuleFilterConstants.CurrentComponent, filter => filter.Property = config.Buffer.PK);
			FilterStripsTestHelper.AddFilterStrip<ModuleGuidFilter>(otherLink.FilterRule, ProcessHeader.ModuleFilterConstants.CurrentComponent, filter => filter.Property = config.Buffer.PK);
			Factory.Save();

			string controlQueryWithoutOptimisation;
			string controlQueryWithOptimisation;

			using (var module = ZFilterModule.GetZFilterModule(ModuleIDs.BMFilterRule))
			{
				var filterBizo = module.FilterBusinessObject;
				var currentComponentFilter = filterBizo.AddGuidFilterStrip(ProcessHeader.ModuleFilterConstants.CurrentComponent, config.Buffer.PK);
				controlQueryWithoutOptimisation = RemoveUniqueParameterNames(currentComponentFilter.Query.ParameterisedText.ParameterisedQueryText);

				var jobOrWorkflowFilter = filterBizo.AddFilterStrip<JobOrWorkflowFilter>(ProcessHeader.ModuleFilterConstants.JobOrWorkflow);
				jobOrWorkflowFilter.SetWorkflowOnly();
				controlQueryWithOptimisation = RemoveUniqueParameterNames(currentComponentFilter.Query.ParameterisedText.ParameterisedQueryText);

				AssertNotEquals("The presence of the Job or Workflow filter should cause the query to change. SAD!", controlQueryWithoutOptimisation, controlQueryWithOptimisation);
			}

			IEnumerable<string> executedCommands;

			using (TestConnection.TrackExecutedCommands())
			{
				runProcessorAction.Invoke(config.System);
				executedCommands = TestConnection.ExecutedCommands;
			}

			var commands = executedCommands.Where(x => x.Contains("Buffer Entry") || x.Contains("other bucket -> bucket")).ToArray();

			foreach (var command in commands)
			{
				var relevantQuery = RemoveUniqueParameterNames(command);
				AssertContains(controlQueryWithOptimisation, relevantQuery);
				AssertNotContains(controlQueryWithoutOptimisation, relevantQuery);
			}
		}

		static string RemoveUniqueParameterNames(string query)
		{
			return Regex.Replace(query, @"CWO\d", "PARAM");
		}

		protected override void SetUp()
		{
			base.SetUp();
			BMSTestHelper.CreateServiceTask(Factory, TransferRuleRunnerServiceTask.Code, TransferRuleRunnerServiceTask.Description, taskPeriodCount: 1, taskPeriod: 'H', branch: Env.CurrentBranchPK);
			Factory.Save();
		}
	}
}
