using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ProcessManagement.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(WorkQueueMembershipLink))]
	class WorkQueueMembershipLinkTest : EnterpriseBusinessObjectTestCase
	{
		#region Properties
		public void TestCurrentTaskProperties()
		{
			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.G4_Description = "Capability Test1";
			var capability2 = Factory.NewWithValidTestData<GlbCapability>();
			capability2.G4_Description = "Capability Test2";

			var job = Factory.NewWithValidTestData<OrgHeader>();
			job.OH_Code = "MAIFRISTORG";
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var workflow = BMSTestHelper.CreateWorkQueue(Factory, "AAA", "aaa");

			var statff1 = Factory.NewWithValidTestData<GlbStaff>();
			statff1.GS_FullName = "Staff Test1";

			var statff2 = Factory.NewWithValidTestData<GlbStaff>();
			statff2.GS_FullName = "Staff Test2";
			Factory.Save();

			var link = workflow.AddMember(jobHeader).Link;

			var currentTask = BMSTestHelper.CreateTask(jobHeader, statff1.GS_Code, 60, taskStatus: ProcessTaskStatusCodeList.Codes.Working, sequence: 1, capability: capability1);
			AssertEquals("Staff Test1", link.StaffAssignedToNextStartableTask);
			AssertEquals("Capability Test1", link.CapabilityAssignedToNextStartableTask);

			currentTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals(ZString.Empty, link.StaffAssignedToNextStartableTask);
			AssertEquals(ZString.Empty, link.CapabilityAssignedToNextStartableTask);

			var currentTask1 = BMSTestHelper.CreateTask(jobHeader, statff1.GS_Code, 60, taskStatus: ProcessTaskStatusCodeList.Codes.Working, sequence: 3, capability: capability1);
			var currentTask2 = BMSTestHelper.CreateTask(jobHeader, statff2.GS_Code, 60, taskStatus: ProcessTaskStatusCodeList.Codes.Working, sequence: 2, capability: capability2);
			AssertEquals("Staff Test2", link.StaffAssignedToNextStartableTask);
			AssertEquals("Capability Test2", link.CapabilityAssignedToNextStartableTask);

			currentTask2.P9_Sequence = 3;
			AssertEquals("Staff Test1", link.StaffAssignedToNextStartableTask);
			AssertEquals("Capability Test1", link.CapabilityAssignedToNextStartableTask);
		}

		[TestDate(2019, 09, 16)]
		[TestUtcOffset(10, 0, 0)]
		public void TestWorkQueueMemberProperties()
		{
			var resource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "FRO", "Frodo Baggins");
			Factory.Save();

			using (Env.SetTemporaryUserContext(resource.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				var workitemWithWorkQueueMembersProvider = Factory.New<DummyWithWorkflowWithWorkQueueMembersProvider>();
				var jobHeader1 = ProcessJobHeader.GetForParent(workitemWithWorkQueueMembersProvider, Factory);
				var workflow1 = BMSTestHelper.CreateWorkQueue(Factory, "AAA", "aaa");
				var link1 = workflow1.AddMember(jobHeader1).Link;
				Factory.Save();

				AssertEquals("", link1.JobCreatedBy);
				AssertEquals(ZDateTime.Empty, link1.JobCreatedDate);
				AssertEquals("", link1.JobCriteria1);
				AssertEquals("", link1.JobCriteria2);
				AssertEquals("", link1.JobCriteria3);
				AssertEquals("", link1.JobCriteria4);
				AssertEquals("", link1.JobCriteria5);

				workitemWithWorkQueueMembersProvider.JobCreatedBy = "FRO";
				workitemWithWorkQueueMembersProvider.JobCreatedDate = new ZDateTime(2019, 09, 16, 0, 0, 0);
				workitemWithWorkQueueMembersProvider.JobCriteria1 = "1ZZ";
				workitemWithWorkQueueMembersProvider.JobCriteria2 = "2ZZ";
				workitemWithWorkQueueMembersProvider.JobCriteria3 = "3ZZ";
				workitemWithWorkQueueMembersProvider.JobCriteria4 = "4ZZ";
				workitemWithWorkQueueMembersProvider.JobCriteria5 = "5ZZ";

				AssertEquals("Frodo Baggins", link1.JobCreatedBy);
				AssertEquals(new ZDateTime(2019, 09, 16, 10, 0, 0), link1.JobCreatedDate);
				AssertEquals("1ZZ", link1.JobCriteria1);
				AssertEquals("2ZZ", link1.JobCriteria2);
				AssertEquals("3ZZ", link1.JobCriteria3);
				AssertEquals("4ZZ", link1.JobCriteria4);
				AssertEquals("5ZZ", link1.JobCriteria5);
			}
		}

		class DummyWithWorkflowWithWorkQueueMembersProvider : DummyWithWorkflow, IWorkQueueMembersProvider
		{
			public DummyWithWorkflowWithWorkQueueMembersProvider(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
			{
			}

			public ZString JobCreatedBy { get; set; }

			public ZDateTime JobCreatedDate { get; set; }

			public ZString JobCriteria1 { get; set; }

			public ZString JobCriteria2 { get; set; }

			public ZString JobCriteria3 { get; set; }

			public ZString JobCriteria4 { get; set; }

			public ZString JobCriteria5 { get; set; }
		}

		public void TestCurrentWorkflowProperties()
		{
			var system = Factory.New<IBMSystem>();
			system.FS_Name = "Hey";
			var component1 = Factory.New<IBMComponent>();
			component1.FC_FS_System = system.PK;
			component1.FC_Name = "Component Test1";
			var component2 = Factory.New<IBMComponent>();
			component2.FC_FS_System = system.PK;
			component2.FC_Name = "Component Test2";

			var workitem = Factory.New<IWorkItem>() as IWorkflowProvider;
			var jobHeader = ProcessJobHeader.GetForParent(workitem, Factory);
			var workflow = BMSTestHelper.CreateWorkQueue(Factory, "AAA", "aaa");
			var link = workflow.AddMember(jobHeader).Link;

			var processHeader1 = jobHeader.ProcessHeaders.AddNew();
			var task1 = workitem.WorkflowItems.Tasks.AddNew();
			task1.P9_FH_ProcessHeader = processHeader1.PK;

			processHeader1.FH_FC_CurrentComponent = component1.PK;
			processHeader1.FH_CompletionStatement = "Completion Statement 1";
			AssertEquals("Completion Statement 1", link.DescriptionOfTheFirstOpenWorkflowInTheJob);
			AssertEquals("Component Test1", link.CurrentComponentForTheFirstOpenWorkflowInTheJob);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals(ZString.Empty, link.DescriptionOfTheFirstOpenWorkflowInTheJob);
			AssertEquals(ZString.Empty, link.CurrentComponentForTheFirstOpenWorkflowInTheJob);

			var processHeader2 = jobHeader.ProcessHeaders.AddNew();
			var task2 = workitem.WorkflowItems.Tasks.AddNew();
			task2.P9_FH_ProcessHeader = processHeader2.PK;
			processHeader2.FH_FC_CurrentComponent = component2.PK;
			processHeader2.FH_CompletionStatement = "Completion Statement 2";

			AssertEquals("Completion Statement 2", link.DescriptionOfTheFirstOpenWorkflowInTheJob);
			AssertEquals("Component Test2", link.CurrentComponentForTheFirstOpenWorkflowInTheJob);
		}

		public void TestCurrentWorkflowProperties_MoreThanTenWorkfflowWithChildWorkflow()
		{
			var workitem = Factory.New<IWorkItem>() as IWorkflowProvider;
			var jobHeader = ProcessJobHeader.GetForParent(workitem, Factory);
			var workflow = BMSTestHelper.CreateWorkQueue(Factory, "AAA", "aaa");
			var link = workflow.AddMember(jobHeader).Link;
			var headers = jobHeader.ProcessHeaders;
			headers.DeleteAll();
			AssertEquals("PreCondition", 0, headers.Count);

			var system = Factory.New<IBMSystem>();
			system.FS_Name = "Hey";
			var sequence = 1;
			for (var i = 1; i < 11; i++)
			{
				for (var j = 0; j < 11; j++)
				{
					if (j == 0 || i == 5)
					{
						var component = Factory.New<IBMComponent>();
						component.FC_FS_System = system.PK;
						component.FC_Name = j == 0 ? "Component Test " + i : "Component Test " + i + "." + j;

						var processHeader = jobHeader.ProcessHeaders.AddNew();
						processHeader.FH_FC_CurrentComponent = component.PK;
						processHeader.FH_CompletionStatement = j == 0 ? "Completion Statement " + i : "Completion Statement " + i + "." + j;
						if (j != 0)
						{
							processHeader.GetOrCreateLinkToParent(jobHeader.ProcessHeaders[4]); //Only "Completion Statement5" has child workflow
						}

						var task = workitem.WorkflowItems.Tasks.AddNew();
						task.P9_FH_ProcessHeader = processHeader.PK;
						task.P9_Status = (i == 5 && (j == 5 || j == 10)) || i == 10 ? ProcessTaskStatusCodeList.Codes.Open : ProcessTaskStatusCodeList.Codes.Closed;
						AssertEquals(sequence++, task.P9_Sequence);
						Factory.Save();
					}
				}
			}

			CombineAssertions("DescriptionOfTheFirstOpenWorkflowInTheJob", () =>
			{
				AssertEquals(20, headers.Count);
				AssertEquals("Order by Sequence directly", "1,10,2,3,4,5,5.1,5.10,5.2,5.3,5.4,5.5,5.6,5.7,5.8,5.9,6,7,8,9", string.Join(",", headers.Select(x => x.Sequence).OrderBy(x => x).ToArray()));
				AssertEquals(4, headers.Count(x => x.IsOpen));
				AssertEquals("Open workflows", "Completion Statement 5,Completion Statement 5.5,Completion Statement 5.10,Completion Statement 10", string.Join(",", headers.Where(x => x.IsOpen).Select(x => x.FH_CompletionStatement).ToArray()));
				AssertEquals("Completion Statement 5", link.DescriptionOfTheFirstOpenWorkflowInTheJob);
				AssertEquals("Component Test 5", link.CurrentComponentForTheFirstOpenWorkflowInTheJob);
			});
		}

		[TestDate(2014, 10, 16)]
		[TestUtcOffset(10, 0, 0)]
		public void TestProperties()
		{
			var resource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "FRO", "Frodo Baggins");
			Factory.Save();

			var job = Factory.NewWithValidTestData<OrgHeader>();
			job.OH_Code = "MAIFRISTORG";
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var queue = BMSTestHelper.CreateWorkQueue(Factory, "AAA", "aaa");

			using (Env.SetTemporaryUserContext(resource.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				var link = queue.AddMember(jobHeader).Link;
				Factory.Save();

				AssertEquals("Job Organization (MAIFRISTORG) is complete.", link.WorkflowDescription);
				AssertEquals(new ZDateTime(2014, 10, 16, 10, 0, 0), link.LocalTimeAddedToQueue);
				AssertEquals("Frodo Baggins", link.AddedByName);
			}
		}

		public void TestQueueStatus()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var queue1 = BMSTestHelper.CreateWorkQueue(Factory, "AAA", "aaa");
			var queue2 = BMSTestHelper.CreateWorkQueue(Factory, "BBB", "bbb");

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);

			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2");
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow3");
			var workflow4 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow4");

			var task1 = BMSTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code);
			var task2 = BMSTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code);
			var task3 = BMSTestHelper.CreateTask(workflow3, GlbStaff.CurrentUser.GS_Code);
			var task4 = BMSTestHelper.CreateTask(workflow4, GlbStaff.CurrentUser.GS_Code);

			var link1 = queue1.AddMember(workflow1).Link;
			var link2 = queue1.AddMember(workflow2).Link;
			var link3 = queue2.AddMember(workflow3).Link;
			var link4 = queue2.AddMember(workflow4).Link;

			Factory.Save();

			AssertEquals(QueueStatusList.Codes.ReadyToRelease, link1.QueueStatus);
			AssertEquals(QueueStatusList.Codes.Blocked, link2.QueueStatus);
			AssertEquals(QueueStatusList.Codes.ReadyToRelease, link3.QueueStatus);
			AssertEquals(QueueStatusList.Codes.Blocked, link4.QueueStatus);

			AssertEquals(QueueStatusList.Descriptions.ReadyToRelease, link1.QueueStatusDescription);
			AssertEquals(QueueStatusList.Descriptions.Blocked, link2.QueueStatusDescription);
			AssertEquals(QueueStatusList.Descriptions.ReadyToRelease, link3.QueueStatusDescription);
			AssertEquals(QueueStatusList.Descriptions.Blocked, link4.QueueStatusDescription);

			// Release item at the front of queue1
			workflow1.FH_FC_CurrentComponent = config.Buffer.PK;
			Factory.Save();

			AssertEquals(QueueStatusList.Codes.ReadyToRelease, link1.QueueStatus);
			AssertEquals(QueueStatusList.Codes.ReadyToRelease, link2.QueueStatus);
			AssertEquals(QueueStatusList.Codes.ReadyToRelease, link3.QueueStatus);
			AssertEquals(QueueStatusList.Codes.Blocked, link4.QueueStatus);

			// Close item at the front of queue2
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			AssertEquals(QueueStatusList.Codes.ReadyToRelease, link1.QueueStatus);
			AssertEquals(QueueStatusList.Codes.ReadyToRelease, link2.QueueStatus);
			AssertEquals(QueueStatusList.Codes.ReadyToRelease, link3.QueueStatus);
			AssertEquals(QueueStatusList.Codes.ReadyToRelease, link4.QueueStatus);
		}

		#endregion

		#region Security

		public void TestSequence_Security()
		{
			var job = Factory.NewWithValidTestData<OrgHeader>();
			job.OH_Code = "YAYYY";
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var queue = BMSTestHelper.CreateWorkQueue(Factory, "AAA", "aaa");
			var link = queue.AddMember(jobHeader).Link;

			Factory.Save();

			using (Env.SetTemporarySecurityInstanceForTest(BMSecurityTestHelper.GetSecurityInstance(Factory)))
			{
				Env.Security.FindCheckPoint("triggerSecurityLoading");
				BMSecurityTestHelper.DenyPermissionsForCheckpoint(Env.Security.WorkQueuesResequenceQueue);
				Assert("Sequence property should be readonly when security is denied for resequencing", link.TGL_SequenceInfo.ReadOnly);
			}
		}

		public void TestSecurity_MagnitudeReadonly()
		{
			var queue = BMSTestHelper.CreateWorkQueue(Factory, "WQA", "Why is description a required parameter?");
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Oh HAI! Of all the nudges, by far the biggest.");
			var link = queue.AddMember(workflow).Link;
			Factory.Save();

			using (Env.SetTemporarySecurityInstanceForTest(BMSecurityTestHelper.GetSecurityInstance(Factory)))
			{
				Env.Security.FindCheckPoint("triggerSecurityLoading");
				BMSecurityTestHelper.DenyPermissionsForCheckpoint(Env.Security.WorkQueuesRemoveFromQueue);
				link.Validation.ValidateAll();
				Assert("Work Queue Membership Link Magnitude should be readonly when the user does not have permission to remove from the queue", link.TGL_TGM_MagnitudeInfo.ReadOnly);
			}
		}

		public void TestSecurity_DefinitionReadonly()
		{
			var queue = BMSTestHelper.CreateWorkQueue(Factory, "WQA", "Why is description a required parameter?");
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Oh HAI! Of all the nudges, by far the biggest.");
			var link = queue.AddMember(workflow).Link;
			Factory.Save();

			using (Env.SetTemporarySecurityInstanceForTest(BMSecurityTestHelper.GetSecurityInstance(Factory)))
			{
				Env.Security.FindCheckPoint("triggerSecurityLoading");
				BMSecurityTestHelper.DenyPermissionsForCheckpoint(Env.Security.WorkQueuesRemoveFromQueue);
				link.Validation.ValidateAll();
				Assert("Work Queue Membership Link Definition should be readonly when the user does not have permission to remove from the queue", link.TagDefinitionPkInfo.ReadOnly);
			}
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return base.GetNewBusinessObjectForDeleteTest(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var queue = BMSTestHelper.CreateWorkQueue(factory, "AAA", "aaa");
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(factory);

			return queue.AddMember(jobHeader).Link;
		}

		#endregion
	}
}
