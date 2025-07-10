using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ProcessManagement.Business.Test;
using Enterprise.Registry.Business;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZClientWebCargoWiseEDI.Services;
using Enterprise.ZClientWebCargoWiseEDI.WebApi.Models.WorkItem;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using WTG.DevTools.Configuration;
using WTG.DevTools.ServiceClient.Assess;
using WTG.DevTools.ServiceClient.Common;
using ZClientEDI.Business.Test;
using WorkItemProcessTask = Enterprise.Client.EDI.IncidentManager.Business.WorkItemProcessTask;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	public class WorkItemControllerTest : TestCaseWithFactory
	{
		#region WorkItemStatuses

		public void TestGetStatuses_WhenWorkItemsExists_ShouldReturnWorkItemsList()
		{
			TestCaseHelper.ClearTable(WorkItemSchema.Constants.TableName);

			ProcessMgmtTestHelper.CreateWorkItem(Factory);
			ProcessMgmtTestHelper.CreateWorkItem(Factory);
			Factory.Save();

			using var controller = GetWorkItemController();
			var workItemsResponse = controller.GetStatuses(new WorkItemStatusRequestData { WorkItemNumbers = ["WI00000001", "WI00000002"] });
			Assert(workItemsResponse.IsSuccessStatusCode);
			var rawData = workItemsResponse.Content.ReadAsStringAsync().Result;
			var result = JsonConvert.DeserializeObject<WorkItemStatusResponseData>(rawData);
			AssertEquals("Result should contain 2 objects", result.Statuses.Count(), 2);
		}

		public void TestGetStatuses_WhenWorkItemDoesNotExists_ShouldReturnEmptyList()
		{
			TestCaseHelper.ClearTable(WorkItemSchema.Constants.TableName);

			ProcessMgmtTestHelper.CreateWorkItem(Factory);
			Factory.Save();

			using var controller = GetWorkItemController();
			var workItemsResponse = controller.GetStatuses(new WorkItemStatusRequestData { WorkItemNumbers = ["WI99999999"] });
			Assert(workItemsResponse.IsSuccessStatusCode);
			var rawData = workItemsResponse.Content.ReadAsStringAsync().Result;
			var result = JsonConvert.DeserializeObject<WorkItemStatusResponseData>(rawData);
			AssertEquals("Result should contain no objects", result.Statuses.Count(), 0);
		}

		#endregion

		#region WorkItemHyperlink

		public void TestGetWorkItemHyperlink_WhenWorkItemExists_ShouldReturnValidHyperlink()
		{
			TestCaseHelper.ClearTable(WorkItemSchema.Constants.TableName);

			var workItem = ProcessMgmtTestHelper.CreateWorkItem(Factory);
			Factory.Save();

			AssertEquals("WI00000001", workItem.WKI_WorkItemNumber);

			using (var controller = new WorkItemController())
			{
				var hyperlink = controller.GetWorkItemHyperlink("WI00000001");

				AssertStartsWith("Should produce a valid hyperlink", "edient:Command=ShowEditForm&ControllerID=WorkItem&BusinessEntityPK=" + workItem.PK, hyperlink);
			}
		}

		public void TestGetWorkItemHyperlink_WhenWorkItemDoesNotExist_ShouldReturnNull()
		{
			using (var controller = new WorkItemController())
			{
				var hyperlink = controller.GetWorkItemHyperlink("Scomo's Handshakes");

				AssertNull(hyperlink);
			}
		}

		#endregion

		#region UpdateProcessTask

		public void TestCancelProcessTask()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "ABC";
			staff.GS_LoginName = "edward.onwodi";
			staff.GS_FullName = "Edward Onwodi";

			NewWorkItem workItem = Factory.NewWithValidTestData<NewWorkItem>();
			workItem.WKI_Status = "WRK";
			ProcessTask task1 = workItem.WorkflowItems.AddNew();
			task1.P9_Status = "CLS";
			ProcessTask task2 = workItem.WorkflowItems.AddNew();
			task2.P9_Status = "WRK";
			task2.P9_Notes = Encoding.UTF8.GetBytes("Work item is setup correctly");

			Factory.Save();

			string json = @"{
""PK"": """ + task2.PK.ToString() + @""",
""Notes"": ""VGhpcyB0YXNrIHdpbGwgY2xvc2U=""
}";
			var obj = (JObject)JsonConvert.DeserializeObject(json);

			using (var controller = new WorkItemController { Request = new HttpRequestMessage() })
			{
				var response = controller.CancelProcessTask(obj);
				AssertEquals(response.StatusCode, HttpStatusCode.OK);
			}

			AssertEquals("CLS", workItem.WKI_Status);
			AssertEquals("CAN", task2.P9_Status);
			AssertEquals("This task will close", Encoding.UTF8.GetString(task2.P9_Notes));
		}

		public void TestUpdateProcessTaskWithNewShelfInformation()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "ABC";
			staff.GS_LoginName = "edward.onwodi";
			staff.GS_FullName = "Edward Onwodi";

			NewWorkItem workItem = Factory.NewWithValidTestData<NewWorkItem>();
			workItem.WKI_Status = "CLS";
			ProcessTask task1 = workItem.WorkflowItems.AddNew();
			task1.P9_Status = "CLS";
			ProcessTask task2 = workItem.WorkflowItems.AddNew();
			task2.P9_Status = "WRK";
			task2.P9_Notes = Encoding.UTF8.GetBytes("Work item is setup correctly");

			Factory.Save();

			string json = @"{
""PK"": """ + task2.PK.ToString() + @""",
""ShelfName"": ""WI00105366"",
""StaffCode"": ""ABC"",
""CheckInTaskType"": ""CH0""
}";
			var obj = (JObject)JsonConvert.DeserializeObject(json);

			using (var controller = new WorkItemController { Request = new HttpRequestMessage() })
			{
				var response = controller.UpdateProcessTaskWithNewShelfInformation(obj);
				AssertEquals(response.StatusCode, HttpStatusCode.OK);
			}

			AssertEquals("WRK", workItem.WKI_Status);
			AssertEquals("ASN", task2.P9_Status);
			AssertEquals("ABC", task2.P9_GS_NKAssignedStaffMember);
			AssertEquals("WI00105366", task2.P9_Description);
			AssertEquals("CH0", task2.P9_Type);
		}

		public void TestUpdateProcessTaskWithNotes()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "PRO";
			staff.GS_LoginName = "dr.programmer";
			staff.GS_FullName = "Dr Programmar";

			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var task = workItem.WorkflowItems.AddNew();
			task.P9_Status = "ASN";

			Factory.Save();

			string json = @"{
""PK"": """ + task.PK.ToString() + @""",
""ShelfName"": ""Patch DPR"",
""StaffCode"": ""PRO"",
""CheckInTaskType"": ""CH1"",
""Notes"": ""https://devops.wisetechglobal.com/wtg/Glow/_git/Glow/pullrequest/118"",
}";
			var obj = (JObject)JsonConvert.DeserializeObject(json);

			using (var controller = new WorkItemController { Request = new HttpRequestMessage() })
			{
				var response = controller.UpdateProcessTaskWithNewShelfInformation(obj);
				AssertEquals(response.StatusCode, HttpStatusCode.OK);
			}

			AssertEquals("ASN", task.P9_Status);
			AssertEquals("PRO", task.P9_GS_NKAssignedStaffMember);
			AssertEquals("Patch DPR", task.P9_Description);
			AssertEquals("CH1", task.P9_Type);
			AssertEquals("https://devops.wisetechglobal.com/wtg/Glow/_git/Glow/pullrequest/118", task.P9_NotesAsString);
		}

		#endregion

		#region AssignProcessTaskBackToUser

		public void TestAssignProcessTaskBackToUser()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "ABC";
			staff.GS_LoginName = "edward.onwodi";
			staff.GS_FullName = "Edward Onwodi";

			NewWorkItem workItem = Factory.NewWithValidTestData<NewWorkItem>();
			workItem.WKI_Status = "WRK";
			ProcessTask task1 = workItem.WorkflowItems.AddNew();
			task1.P9_Status = "CLS";
			ProcessTask task2 = workItem.WorkflowItems.AddNew();
			task2.P9_Status = "WRK";
			task2.P9_Notes = Encoding.UTF8.GetBytes("Work item is setup correctly");

			Factory.Save();

			string json = @"{
""PK"": """ + task2.PK.ToString() + @""",
""StaffCode"": ""ABC""
}";
			var obj = (JObject)JsonConvert.DeserializeObject(json);

			using (var controller = new WorkItemController { Request = new HttpRequestMessage() })
			{
				var response = controller.AssignProcessTaskBackToUser(obj);
				AssertEquals(response.StatusCode, HttpStatusCode.OK);
			}

			AssertEquals("WRK", workItem.WKI_Status);
			AssertEquals("SUS", task2.P9_Status);
		}

		[TestDate(2017, 9, 26)]
		public void TestAssignProcessTaskBackToUser_QualityIterationShouldBeCreatedForFaildMergeWhenAutoPatch()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.SetUpForQualityIterations(false, "PCH", @"Failed Auto Patch");

			var firstWorkflow = testHelper.AddWorkflow("YES", null, null, "CBC,ADK,CLS,3", "INV,WGN,CLS,1", "CDU,WGN,CLS,2");
			var secondWorkflow = testHelper.AddWorkflow("YES", firstWorkflow, "CH0,WGN,ASN,6", "CDF,WGN,CLS,4", "CBF,DEA,CLS,5");
			var qcbTask = testHelper.AddTask(secondWorkflow, new string[] { "CH1", "DAT", "ASN", "7" });

			Factory.Save();

			string json = @"{
			  ""PK"": """ + qcbTask.PK + @""",
			  ""StaffCode"": ""WGN"",
		""ReasonCode"": ""PCH"",
		""IterationType"": ""Manually Patch DPR""
			}";
			var obj = (JObject)JsonConvert.DeserializeObject(json);
			using (var controller = new WorkItemController { Request = new HttpRequestMessage() })
			{
				var response = controller.AssignProcessTaskBackToUser(obj);
				AssertEquals(response.StatusCode, HttpStatusCode.OK);
			}

			testHelper.AssertWorkItemTasks(10);

			testHelper.AssertTaskExists(8, "COD", "ASN");
			var codingTask = testHelper.GetTaskBySequenceNumber(8);
			testHelper.AssertTaskDetails(codingTask, "COD", ProcessTaskStatusCodeList.Codes.Assigned, "Manually Patch DPR", "WGN", 20, null);

			testHelper.AssertTaskExists(9, "CBC", "ASN");
			var codeReviewTask = testHelper.GetTaskBySequenceNumber(9);
			testHelper.AssertTaskDetails(codeReviewTask, "CBC", ProcessTaskStatusCodeList.Codes.Assigned, "Code Review", "ADK", 10, null);

			testHelper.AssertTaskExists(10, "CH1", "SUS");
			var patchTask = testHelper.GetTaskBySequenceNumber(10);
			testHelper.AssertTaskDetails(patchTask, "CH1", ProcessTaskStatusCodeList.Codes.Suspended, "Checkin to something", "WGN", 0, null);

			testHelper.AssertTaskExists(7, "CH1", "CAN");

			AssertEquals("Silly Hats Only (Manually Patch DPR 1)", testHelper.GetWorkflowCompletionStatement(codingTask));
			AssertEquals("Silly Hats Only (Manually Patch DPR 1)", testHelper.GetWorkflowCompletionStatement(codeReviewTask));
			AssertEquals("Silly Hats Only (Manually Patch DPR 1)", testHelper.GetWorkflowCompletionStatement(patchTask));
		}

		public void TestAssignProcessTaskBackToUser_QualityIterationShouldBeCreatedInNewWorkflowForFailedMergeWhenAutoPatch()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.SetUpForQualityIterations(false, "PCH", "Failed Auto Patch");

			var topWorkflow = testHelper.AddWorkflow("YES", null, "CH0,WGN,ASN,6", "INV,WGN,CLS,1", "CDU,WGN,CLS,2", "CBC,ADK,CLS,3", "CDF,WGN,CLS,4", "CBF,DEA,CLS,5");
			var qcbTask = testHelper.AddTask(topWorkflow, new string[] { "CH1", "DAT", "ASN", "7" });

			Factory.Save();

			string json = @"{
			  ""PK"": """ + qcbTask.PK + @""",
			  ""StaffCode"": ""WGN"",
			  ""ReasonCode"": ""PCH"",
			  ""IterationType"": ""Manually Patch DPR""
			}";
			var obj = (JObject)JsonConvert.DeserializeObject(json);
			using (var controller = new WorkItemController { Request = new HttpRequestMessage() })
			{
				var response = controller.AssignProcessTaskBackToUser(obj);
				AssertEquals(response.StatusCode, HttpStatusCode.OK);
			}

			testHelper.AssertWorkItemTasks(10);
			testHelper.AssertTaskExists(7, "CH1", "CAN");

			testHelper.AssertTaskExists(8, "COD", "ASN");
			var codingTask = testHelper.GetTaskBySequenceNumber(8);
			testHelper.AssertTaskDetails(codingTask, "COD", ProcessTaskStatusCodeList.Codes.Assigned, "Manually Patch DPR", "WGN", 20, null);
			AssertEquals(nameof(codingTask.Iteration), "1", codingTask.Iteration);

			var patchWorkflow = codingTask.ProcessHeader;
			AssertNotEquals("Iteration tasks should be created in a new workflow in the case of auto patcher failure.", topWorkflow.PK, patchWorkflow.PK);

			testHelper.AssertTaskExists(9, "CBC", "ASN", patchWorkflow.PK);
			var codeReviewTask = testHelper.GetTaskBySequenceNumber(9);
			testHelper.AssertTaskDetails(codeReviewTask, "CBC", ProcessTaskStatusCodeList.Codes.Assigned, "Code Review", "ADK", 10, null);
			AssertEquals(nameof(codeReviewTask.Iteration), "1", codeReviewTask.Iteration);

			testHelper.AssertTaskExists(10, "CH1", "SUS", patchWorkflow.PK);
			var patchTask = testHelper.GetTaskBySequenceNumber(10);
			testHelper.AssertTaskDetails(patchTask, "CH1", ProcessTaskStatusCodeList.Codes.Suspended, "Checkin to something", "WGN", 0, null);
			AssertEquals(nameof(patchTask.Iteration), "1", patchTask.Iteration);
		}

		public void TestAssignProcessTaskBackToUser_QualityIterationShouldBeCreatedInSameWorkflowForFailedMergeWhenManualPatch()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.SetUpForQualityIterations(false, "SHV", "DAT shelf failure");

			var topWorkflow = testHelper.AddWorkflow("YES", null, "CH0,WGN,ASN,6", "INV,WGN,CLS,1", "CDU,WGN,CLS,2", "CBC,ADK,CLS,3", "CDF,WGN,CLS,4", "CBF,DEA,CLS,5");
			var patchTask = testHelper.AddTask(topWorkflow, new string[] { "CH1", "DAT", "CAN", "7" });

			var bmTestHelper = ObjectFactory.Get<IBMTestHelper>();
			var patchWorkflow = bmTestHelper.CreateQualityIteration(patchTask, patchTask, shouldCreateWorkflowForIteration: true);

			testHelper.AddTask(patchWorkflow, new string[] { "COD", "WGN", "CLS", "8" });
			testHelper.AddTask(patchWorkflow, new string[] { "CBC", "ADK", "CLS", "9" });
			patchTask = (WorkItemProcessTask)patchWorkflow.Tasks.FirstOrDefault(task => task.P9_Sequence == 8 && task.P9_Type == "CH1");
			AssertNotNull($"The expected task [8 - CH1] was not found in the workflow {patchWorkflow.Description}.", patchTask);
			patchTask.P9_Sequence = 10;

			Factory.Save();

			testHelper.AssertWorkItemTasks(10);
			testHelper.AssertTaskExists(10, "CH1", "ASN", patchWorkflow.PK);
			patchTask = testHelper.GetTaskBySequenceNumber(10);
			AssertEquals(nameof(patchTask.Iteration), "1", patchTask.Iteration);

			var json = @"{
			  ""PK"": """ + patchTask.PK + @""",
			  ""StaffCode"": ""WGN"",
			  ""ReasonCode"": ""SHV"",
			  ""IterationType"": ""Manually Patch DPR""
			}";
			var obj = (JObject)JsonConvert.DeserializeObject(json);
			using (var controller = new WorkItemController { Request = new HttpRequestMessage() })
			{
				var response = controller.AssignProcessTaskBackToUser(obj);
				AssertEquals(response.StatusCode, HttpStatusCode.OK);
			}

			testHelper.AssertWorkItemTasks(13);
			testHelper.AssertTaskExists(10, "CH1", "CAN", patchWorkflow.PK);

			var codingTask = testHelper.AssertTaskExists(11, "COD", "ASN", patchWorkflow.PK);
			testHelper.AssertTaskDetails(codingTask, "COD", ProcessTaskStatusCodeList.Codes.Assigned, "Manually Patch DPR", "WGN", 20, null);
			AssertEquals(nameof(codingTask.Iteration), "2", codingTask.Iteration);

			var codeReviewTask = testHelper.AssertTaskExists(12, "CBC", "ASN", patchWorkflow.PK);
			testHelper.AssertTaskDetails(codeReviewTask, "CBC", ProcessTaskStatusCodeList.Codes.Assigned, "Code Review", "ADK", 10, null);
			AssertEquals(nameof(codeReviewTask.Iteration), "2", codeReviewTask.Iteration);

			patchTask = (WorkItemProcessTask)testHelper.AssertTaskExists(13, "CH1", "SUS", patchWorkflow.PK);
			testHelper.AssertTaskDetails(patchTask, "CH1", ProcessTaskStatusCodeList.Codes.Suspended, "Checkin to something", "WGN", 0, null);
			AssertEquals(nameof(patchTask.Iteration), "2", patchTask.Iteration);
		}

		public void TestAssignProcessTaskBackToUser_WhenQualityIterationAlreadyPresent()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.SetUpForQualityIterations(true, "PCH", @"Failed Auto Patch");
			WorkflowDataRegistry.Instance.CreateNewWorkflowsForQualityIterationsByDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var codingTask = MasterFilesTestHelper.CreateTask(workItem, "DEA", status: ProcessTaskStatusCodeList.Codes.Closed, taskType: "CDF", sequence: 1);
			var codeReviewTask = MasterFilesTestHelper.CreateTask(workItem, "ADK", status: ProcessTaskStatusCodeList.Codes.Closed, taskType: "CBC", sequence: 2);
			var checkinTask = MasterFilesTestHelper.CreateTask(workItem, "DEA", status: ProcessTaskStatusCodeList.Codes.Closed, taskType: "CHK", sequence: 5);
			var autoPatchTask = MasterFilesTestHelper.CreateTask(workItem, "DEA", status: ProcessTaskStatusCodeList.Codes.Assigned, taskType: "CH1", sequence: 6);

			var bmTestHelper = ObjectFactory.Get<IBMTestHelper>();
			bmTestHelper.CreateQualityIteration(codingTask, codeReviewTask, shouldCreateWorkflowForIteration: false);
			AssertEquals(6, workItem.WorkflowItems.Tasks.Count);

			Factory.Save();

			var jsonObject = (JObject)JsonConvert.DeserializeObject($@"{{
				""PK"": ""{autoPatchTask.PK}"",
				""StaffCode"": ""DEA"",
				""ReasonCode"": ""PCH"",
				""IterationType"": ""Manually Patch DPR""
			}}");
			using (var controller = new WorkItemController { Request = new HttpRequestMessage() })
			{
				var response = controller.AssignProcessTaskBackToUser(jsonObject);
				AssertEquals(response.StatusCode, HttpStatusCode.OK);
			}

			workItem = Factory.CreateNewFactory().Load<NewWorkItem>(workItem.PK);
			autoPatchTask.Reload();

			AssertEquals(9, workItem.WorkflowItems.Tasks.Count);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, autoPatchTask.P9_Status);

			var manualPatchTask = workItem.WorkflowItems.Tasks.Cast<ProcessTask>().Single(t => t.P9_Description == "Manually Patch DPR");
			var reviewPatchTask = workItem.WorkflowItems.Tasks.Cast<ProcessTask>().Single(t => t.P9_Sequence == manualPatchTask.P9_Sequence + 1 && t.P9_Type == "CBC");
			var requeuePatchTask = workItem.WorkflowItems.Tasks.Cast<ProcessTask>().Single(t => t.P9_Sequence == manualPatchTask.P9_Sequence + 2 && t.P9_Type == "CH1");

			CombineAssertions("Iteration tasks should be created in a new workflow in the case of auto patcher failure, and Iteration should start with 1 in the new workflow.", () =>
			{
				AssertEquals("1", manualPatchTask.Iteration);
				AssertEquals("1", reviewPatchTask.Iteration);
				AssertEquals("1", requeuePatchTask.Iteration);
			});
		}

		[TestDate(2017, 9, 26)]
		public void TestAssignProcessTaskBackToUser_WhenExistingCodeReviewTasksWithCapability_PatchCodeReviewShouldUseCapabilityFromTemplatedTask()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.SetUpForQualityIterations(true, "PCH", @"Failed Auto Patch");

			var codingCapability = MasterFilesTestHelper.CreateCapability(Factory, "COD", "Coding");
			var thisTeamCodeReviewCapability1 = MasterFilesTestHelper.CreateCapability(Factory, "CR1", "Code Reviewers #1");
			var thisTeamCodeReviewCapability2 = MasterFilesTestHelper.CreateCapability(Factory, "CR2", "Code Reviewers #2");
			var otherTeamCodeReviewCapability = MasterFilesTestHelper.CreateCapability(Factory, "CR3", "Code Reviewers #3");

			var template = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, testHelper.WorkItem.WorkflowType);
			MasterFilesTestHelper.CreateTask(template, taskType: "CDU", requiredCapability: codingCapability, sequence: 10, assignedStaff: "WGN");
			MasterFilesTestHelper.CreateTask(template, taskType: "CDF", requiredCapability: codingCapability, sequence: 20, assignedStaff: "WGN");
			MasterFilesTestHelper.CreateTask(template, taskType: "CBC", requiredCapability: thisTeamCodeReviewCapability1, sequence: 30, assignedStaff: "DEA");
			MasterFilesTestHelper.CreateTask(template, taskType: "CBC", requiredCapability: thisTeamCodeReviewCapability2, sequence: 31, assignedStaff: "ADK");
			MasterFilesTestHelper.CreateTask(template, taskType: "CHK", requiredCapability: codingCapability, sequence: 40, assignedStaff: "WGN");
			MasterFilesTestHelper.CreateTask(template, taskType: "CH1", requiredCapability: codingCapability, sequence: 50, assignedStaff: "WGN");
			MasterFilesTestHelper.CreateTask(template, taskType: "DEP", requiredCapability: codingCapability, sequence: 60, assignedStaff: "WGN");

			Factory.Save();

			testHelper.WorkItem.ApplyWorkflowTemplates();
			testHelper.AssertWorkItemTasks(7);

			MasterFilesTestHelper.CreateTask(testHelper.WorkItem, "DKV", taskType: "CBC", status: ProcessTaskStatusCodeList.Codes.Closed, requiredCapability: otherTeamCodeReviewCapability, sequence: 29);

			Factory.Save();

			var containmentBarrierTask = testHelper.WorkItem.WorkflowItems.Tasks.Cast<ProcessTask>().Single(t => t.P9_Type == "CH1");
			var jsonObject = (JObject)JsonConvert.DeserializeObject($@"{{
				""PK"": ""{containmentBarrierTask.PK}"",
				""StaffCode"": ""WGN"",
				""ReasonCode"": ""PCH"",
				""IterationType"": ""Manually Patch DPR""
			}}");
			using (var controller = new WorkItemController { Request = new HttpRequestMessage() })
			{
				var response = controller.AssignProcessTaskBackToUser(jsonObject);
				AssertEquals(response.StatusCode, HttpStatusCode.OK);
			}

			testHelper.AssertWorkItemTasks(11);

			var failedPatchTask = testHelper.AssertTaskExists(50, "CH1", ProcessTaskStatusCodeList.Codes.Cancelled);
			testHelper.AssertTaskDetails(failedPatchTask, "CH1", ProcessTaskStatusCodeList.Codes.Cancelled, "Checkin to something", "WGN", 0, codingCapability);

			var manualPatchTask = testHelper.AssertTaskExists(51, "COD", ProcessTaskStatusCodeList.Codes.Assigned);
			testHelper.AssertTaskDetails(manualPatchTask, "COD", ProcessTaskStatusCodeList.Codes.Assigned, "Manually Patch DPR", "WGN", 20, null);

			var codeReviewTask = testHelper.AssertTaskExists(52, "CBC", ProcessTaskStatusCodeList.Codes.Assigned);
			testHelper.AssertTaskDetails(codeReviewTask, "CBC", ProcessTaskStatusCodeList.Codes.Assigned, "Code Review", string.Empty, 10, thisTeamCodeReviewCapability1);

			var nextPatchTask = testHelper.AssertTaskExists(53, "CH1", ProcessTaskStatusCodeList.Codes.Suspended);
			testHelper.AssertTaskDetails(nextPatchTask, "CH1", ProcessTaskStatusCodeList.Codes.Suspended, "Checkin to something", "WGN", 0, codingCapability);

			var deployTask = testHelper.AssertTaskExists(63, "DEP", ProcessTaskStatusCodeList.Codes.Assigned);
			testHelper.AssertTaskDetails(deployTask, "DEP", ProcessTaskStatusCodeList.Codes.Assigned, "Deploy", "WGN", 0, codingCapability);
		}

		public void TestAssignProcessTaskBackToUser_AlphaCheckinTaskAssignedToDAT_ShouldNotCreateQualityIteration()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.SetUpForQualityIterations(defaultSettingForReleaseGroupsNotSpecified: true, shouldAddTaskTypes: false);

			var workflow = testHelper.AddWorkflow(null, null, "CHK,DAT,ASN,3", "CDF,DEA,CLS,1", "CBC,ADK,CLS,2"); // Stringly-typed objects make me :(
			var checkinTask = workflow.Tasks.Single(t => t.P9_GS_NKAssignedStaffMember == "DAT");

			Factory.Save();

			MasterFilesTestHelper.AddTaskTypesToRegistry("WKI", "CHK", "CDF", "CBC"); // Makes the task types present, but not marked as containment barriers.

			var json = GetJsonRequest(@"{
			  ""PK"": """ + checkinTask.PK + @""",
			  ""StaffCode"": ""DEA"",
		""ReasonCode"": ""SHV"",
		""IterationType"": ""DAT don't fail me now""
			}");
			using (var controller = new WorkItemController { Request = new HttpRequestMessage() })
			{
				var response = controller.AssignProcessTaskBackToUser(json);
				AssertEquals(response.StatusCode, HttpStatusCode.OK);
			}

			CombineAssertions("Should have assigned the checkin task back to the developer. They don't need to fix any code, they just need to ensure the task is assigned correctly. DAT doesn't write code, so ALP checkins assigned to DAT doesn't make sense.", () =>
			{
				AssertEquals("DEA", checkinTask.P9_GS_NKAssignedStaffMember);
				AssertEquals(ProcessTaskStatusCodeList.Codes.Suspended, checkinTask.P9_Status);
			});
		}

		[TestDate(2021, 9, 17)]
		public void TestAssignProcessTaskBackToUser_WhenQualityIterationCreated_ShouldSelectContainmentBarrierTaskAssignedUser_ForResourceUnderReview_PrefillEnabled()
		{
			WorkflowDataRegistry.Instance.PrefillResourceUnderReview.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.SetUpForQualityIterations(false, "PCH", @"Failed Auto Patch");

			testHelper.AddUser("ADK", "Adam");
			testHelper.AddUser("WGN", "Wayne");

			var workflow = testHelper.AddWorkflow("YES", null, null, "CBC,ADK,CLS,3", "INV,WGN,ASN,1", "CDU,WGN,CLS,2");
			var qcbTask = testHelper.AddTask(workflow, new string[] { "CH1", "DAT", "ASN", "7" });

			var bigTask = workflow.Tasks.Single(x => x.P9_Type == "INV");
			bigTask.P9_Status = "WRK";

			TestDateAttribute.AddHours(1);
			bigTask.P9_Status = "CLS";

			Factory.Save();

			string json = @"{
			  ""PK"": """ + qcbTask.PK + @""",
			  ""StaffCode"": ""ADK"",
		""ReasonCode"": ""PCH"",
		""IterationType"": ""Manually Patch DPR""
			}";
			var obj = (JObject)JsonConvert.DeserializeObject(json);
			using (var controller = new WorkItemController { Request = new HttpRequestMessage() })
			{
				var response = controller.AssignProcessTaskBackToUser(obj);
				AssertEquals(response.StatusCode, HttpStatusCode.OK);
			}

			var link = Factory.LoadTop1<IProcessTaskIterationLink>(new ZQuery());
			AssertEquals("The staff assigned to the CH0 task should always be the resource under review for auto patcher iterations, even when prefill is enabled.", "ADK", link.P9I_GS_NKResourceUnderReview);
		}

		public void TestAssignProcessTaskBackToUser_WhenQualityIterationCreated_ShouldSelectContainmentBarrierTaskAssignedUser_ForResourceUnderReview_PrefillDisabled()
		{
			WorkflowDataRegistry.Instance.PrefillResourceUnderReview.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.SetUpForQualityIterations(false, "PCH", @"Failed Auto Patch");

			testHelper.AddUser("ADK", "Adam");
			testHelper.AddUser("WGN", "Wayne");

			var workflow = testHelper.AddWorkflow("YES", null, null, "CBC,ADK,CLS,3", "INV,WGN,ASN,1", "CDU,WGN,CLS,2");
			var qcbTask = testHelper.AddTask(workflow, new string[] { "CH1", "DAT", "ASN", "7" });

			var bigTask = workflow.Tasks.Single(x => x.P9_Type == "INV");
			bigTask.P9_Status = "WRK";

			TestDateAttribute.AddHours(1);
			bigTask.P9_Status = "CLS";

			Factory.Save();

			string json = @"{
			  ""PK"": """ + qcbTask.PK + @""",
			  ""StaffCode"": ""ADK"",
		""ReasonCode"": ""PCH"",
		""IterationType"": ""Manually Patch DPR""
			}";
			var obj = (JObject)JsonConvert.DeserializeObject(json);
			using (var controller = new WorkItemController { Request = new HttpRequestMessage() })
			{
				var response = controller.AssignProcessTaskBackToUser(obj);
				AssertEquals(response.StatusCode, HttpStatusCode.OK);
			}

			var link = Factory.LoadTop1<IProcessTaskIterationLink>(new ZQuery());
			AssertEquals("The staff assigned to the CH0 task should always be the resource under review for auto patcher iterations, even when prefill is disabled.", "ADK", link.P9I_GS_NKResourceUnderReview);
		}

		#endregion

		#region EnsureRequiredReviewsExist Creates New Review

		public void TestEnsureRequiredReviewsExist_CreatesANewReview_WhenNoReviewExists()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.AddWorkflow(null, null,
				$"SH0,{coder.GS_Code},ASN,1",
				$"CDF,{coder.GS_Code},CLS,1");

			AssertNewReviewCreated(testHelper);
		}

		public void TestEnsureRequiredReviewsExist_CreatesANewReview_WhenAReviewExistsButIsClosed()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.AddWorkflow(null, null,
				$"SH0,{coder.GS_Code},ASN,20",
				$"CDF,{coder.GS_Code},CLS,10",
				$"{reviewTaskType},{capableReviewer.GS_Code},CLS,30");

			AssertNewReviewCreated(testHelper);
		}

		public void TestEnsureRequiredReviewsExist_CreatesANewReview_WithTruncatedName()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.AddWorkflow(null, null,
				$"SH0,{coder.GS_Code},ASN,20",
				$"CDF,{coder.GS_Code},CLS,10",
				$"{reviewTaskType},{capableReviewer.GS_Code},CLS,30");

			AssertNewReviewCreated(testHelper, "AspectNameTruncatedAfter24Characters");
		}

		public void TestEnsureRequiredReviewsExist_CreatesANewReview_WhenOnlyReviewsExistsBeforeTheShelf()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.AddWorkflow(null, null,
				$"SH0,{coder.GS_Code},ASN,130",
				$"{reviewTaskType},{reviewer.GS_Code},SUS,30,{capability.PK}",
				$"{reviewTaskType},{capableReviewer.GS_Code},ASN,50",
				$"CDF,{coder.GS_Code},CLS,110",
				$"INV,{coder.GS_Code},CLS,120");

			AssertNewReviewCreated(testHelper);
		}

		public void TestEnsureRequiredReviewsExist_CreatesANewReview_WhenOnlyReviewsExistsBeforeTheShelfWithMultipleWorkflows()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);

			var nestedWorkflow1 = testHelper.AddWorkflow(null, null,
				null,
				$"{reviewTaskType},{capableReviewer.GS_Code},ASN,50");
			var nestedWorkflow2 = testHelper.AddWorkflow(null, nestedWorkflow1,
				$"SH0,{coder.GS_Code},ASN,130",
				$"CDF,{coder.GS_Code},CLS,110",
				$"INV,{coder.GS_Code},CLS,120");
			testHelper.AddWorkflow(null, nestedWorkflow2,
				null,
				$"{reviewTaskType},{capableReviewer.GS_Code},SUS,30");

			AssertNewReviewCreated(testHelper);

			var newReviewTask = testHelper.WorkItemTasks.Where(t => (t.P9_Type == reviewTaskType) && t.P9_Sequence > testHelper.ShelfTask.P9_Sequence && !t.IsClosed).OrderBy(t => t.P9_Sequence).SingleOrDefault();
			AssertNotNull("Unable to find new review task", newReviewTask);
			AssertEquals("Review Task should be on the same workflow as the shelf", newReviewTask.P9_FH_ProcessHeader, testHelper.ShelfTask.P9_FH_ProcessHeader);
		}

		void AssertNewReviewCreated(ProcessedShelfsServiceTaskTestHelper testHelper, string aspectName = "", bool shouldReviewBeCreated = true)
		{
			Factory.Save();
			testHelper.RefreshWorkItemTasks();
			var initialTaskCount = testHelper.WorkItemTasks.Length;
			using (var controller = GetWorkItemController())
			{
				var jsonRequest = GetJsonRequest(testHelper, aspectName);
				var truncatedAspectName = aspectName.Substring(0, Math.Min(24, aspectName.Length));
				using (var response = controller.EnsureRequiredReviewsExist(jsonRequest))
				{
					AssertEquals(response.Content?.ReadAsStringAsync()?.Result, HttpStatusCode.OK, response.StatusCode);
				}

				testHelper.RefreshWorkItemTasks();

				if (shouldReviewBeCreated)
				{
					CombineAssertions(() =>
					{
						AssertEquals("1 review should be created", initialTaskCount + 1, testHelper.WorkItemTasks.Length);
						var newReviewTask = testHelper.WorkItemTasks.Where(t => (t.P9_Type == reviewTaskType) && t.P9_Sequence > testHelper.ShelfTask.P9_Sequence && !t.IsClosed).OrderBy(t => t.P9_Sequence).SingleOrDefault();
						AssertNotNull("Unable to find new review task", newReviewTask);
						AssertContains("Task notes should be set", jsonRequest["TaskNotes"].ToString(), newReviewTask.P9_NotesAsString);
						AssertEquals("Task should have description", $"Aspect Review - {truncatedAspectName} (with CB)", newReviewTask.P9_Description);
						Assert("No other task should have the task note message", testHelper.WorkItemTasks.Where(t => t != newReviewTask).All(t => !t.P9_NotesAsString.Contains(jsonRequest["TaskNotes"].ToString())));
						AssertEquals("Sequence Number of new task should be one more than shelf when no valid review tasks exist", testHelper.ShelfTask.P9_Sequence + 1, newReviewTask.P9_Sequence);
						AssertEquals("Time estimate should exist on new task", new ZInt(10).GetDateTimeFromMinutes(), newReviewTask.P9_EstDuration);
					});

					using (var repeatedResponse = controller.EnsureRequiredReviewsExist(jsonRequest))
					{
						AssertEquals(repeatedResponse.Content?.ReadAsStringAsync()?.Result, HttpStatusCode.OK, repeatedResponse.StatusCode);
					}

					testHelper.RefreshWorkItemTasks();
					AssertEquals("the review created in the first request should be used", initialTaskCount + 1, testHelper.WorkItemTasks.Length);
				}
				else
				{
					testHelper.AssertWorkItemTasks(initialTaskCount);
				}
			}
		}

		#endregion

		#region EnsureRequiredReviewsExist Creates New Review Using Existing Review Sequence Number Plus 1

		public void TestEnsureRequiredReviewsExist_CreatesAnotherReview_WhenNoReviewOfRequiredCapabilityExists()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.AddWorkflow(null, null,
				$"SH0,{coder.GS_Code},ASN,20",
				$"CDF,{coder.GS_Code},CLS,10",
				$"{reviewTaskType},{reviewer.GS_Code},ASN,30");

			AssertNewReviewCreatedWithExpectedSequenceNumber(testHelper, 31);
		}

		public void TestEnsureRequiredReviewsExist_CreatesAnotherReview_IfOneExistsWithRequiredCapability()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.AddWorkflow(null, null,
				$"SH0,{coder.GS_Code},ASN,20",
				$"CDF,{coder.GS_Code},CLS,10",
				$"{reviewTaskType},,ASN,30,{capability.PK},Aspect Review - WrongAspectName (with CB)");

			AssertNewReviewCreatedWithExpectedSequenceNumber(testHelper, 31);
		}

		public void TestEnsureRequiredReviewsExist_CreatesAnotherReview_InSameWorkflowAsExistingReview_Before()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			var nestedWorkflow1 = testHelper.AddWorkflow(null, null,
				null,
				$"{reviewTaskType},{reviewer.GS_Code},ASN,100");
			var nestedWorkflow2 = testHelper.AddWorkflow(null, nestedWorkflow1,
				$"SH0,{coder.GS_Code},ASN,30",
				$"CDF,{coder.GS_Code},CLS,10",
				$"INV,{coder.GS_Code},CLS,20");
			testHelper.AddWorkflow(null, nestedWorkflow2,
				null,
				$"{alternateReviewTaskType},{reviewer.GS_Code},SUS,200");

			AssertNewReviewCreatedWithExpectedSequenceNumber(testHelper, 31);

			var newReviewTask = testHelper.WorkItemTasks.Where(t => t.P9_Sequence == 31).Single();
			AssertEquals("Review Task should be on the same workflow as the shelf", newReviewTask.P9_FH_ProcessHeader, testHelper.ShelfTask.P9_FH_ProcessHeader);
		}

		public void TestEnsureRequiredReviewsExist_CreatesAnotherReview_InSameWorkflowAsExistingReview_After()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			var nestedWorkflow1 = testHelper.AddWorkflow(null, null,
				null,
				$"{reviewTaskType},{reviewer.GS_Code},ASN,200");
			var nestedWorkflow2 = testHelper.AddWorkflow(null, nestedWorkflow1,
				$"SH0,{coder.GS_Code},ASN,30",
				$"CDF,{coder.GS_Code},CLS,10",
				$"INV,{coder.GS_Code},CLS,20");
			testHelper.AddWorkflow(null, nestedWorkflow2,
				null,
				$"{alternateReviewTaskType},{reviewer.GS_Code},SUS,100");

			AssertNewReviewCreatedWithExpectedSequenceNumber(testHelper, 31);

			var newReviewTask = testHelper.WorkItemTasks.Where(t => t.P9_Sequence == 31).Single();
			AssertEquals("Review Task should be on the same workflow as the shelf", newReviewTask.P9_FH_ProcessHeader, testHelper.ShelfTask.P9_FH_ProcessHeader);
		}

		public void TestEnsureRequiredReviewsExist_CreatesAnotherReview_WhenReviewerIsNotCapableEvenIfTaskHasCapaility()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.AddWorkflow(null, null,
				$"SH0,{coder.GS_Code},ASN,20",
				$"CDF,{coder.GS_Code},CLS,10",
				$"{reviewTaskType},{reviewer.GS_Code},ASN,30,{capability.PK}");

			AssertNewReviewCreatedWithExpectedSequenceNumber(testHelper, 31);
		}

		public void TestEnsureRequiredReviewsExist_CreatesAnotherReview_WhenReviewTaskIsUnassigned()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			var randomCapability = Factory.New<GlbCapability>();
			randomCapability.G4_Code = "ARD";
			testHelper.AddWorkflow(null, null
				, $"SH0,{coder.GS_Code},ASN,204"
				, $"TLT,{coder.GS_Code},CLS,198"
				, $"CBL,{reviewer.GS_Code},CLS,199"
				, $"CDU,{coder.GS_Code},CLS,200"
				, $"CDF,{coder.GS_Code},CLS,202"
				, $"{reviewTaskType},,ASN,206,{randomCapability.PK}"
				, $"PRV,{coder.GS_Code},ASN,206"
				, $"CHK,{coder.GS_Code},ASN,290"
				);

			AssertNewReviewCreatedWithExpectedSequenceNumber(testHelper, 207);
		}

		public void TestEnsureRequiredReviewsExist_UsesCheckinWhenNoReviewsExist()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			var randomCapability = Factory.New<GlbCapability>();
			randomCapability.G4_Code = "ARD";
			testHelper.AddWorkflow(null, null
				, $"SH0,{coder.GS_Code},ASN,204"
				, $"TLT,{coder.GS_Code},CLS,198"
				, $"CBL,{reviewer.GS_Code},CLS,199"
				, $"CDU,{coder.GS_Code},CLS,200"
				, $"CDF,{coder.GS_Code},CLS,202"
				, $"PRV,{coder.GS_Code},ASN,206"
				, $"CHK,{coder.GS_Code},ASN,290"
				);

			AssertNewReviewCreatedWithExpectedSequenceNumber(testHelper, 289);
		}

		public void TestEnsureRequiredReviewsExist_OnePlusLastTaskWhenNoCheckinAndNoReviewsExist()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			var randomCapability = Factory.New<GlbCapability>();
			randomCapability.G4_Code = "ARD";
			testHelper.AddWorkflow(null, null
				, $"SH0,{coder.GS_Code},ASN,204"
				, $"TLT,{coder.GS_Code},CLS,198"
				, $"CBL,{reviewer.GS_Code},CLS,199"
				, $"CDU,{coder.GS_Code},CLS,200"
				, $"CDF,{coder.GS_Code},CLS,202"
				, $"PRV,{coder.GS_Code},ASN,206"
				, $"INV,{coder.GS_Code},ASN,290"
				);

			AssertNewReviewCreatedWithExpectedSequenceNumber(testHelper, 291);
		}

		public void TestEnsureRequiredReviewsExist_OneLessThanCheckinTaskUnlessZero()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			var randomCapability = Factory.New<GlbCapability>();
			randomCapability.G4_Code = "ARD";
			testHelper.AddWorkflow(null, null
				, $"SH0,{coder.GS_Code},ASN,0"
				, $"CHK,{coder.GS_Code},ASN,0"
				);

			AssertNewReviewCreatedWithExpectedSequenceNumber(testHelper, 0);
		}

		public void TestEnsureRequiredReviewsExist_OneLessThanCheckinTaskZero()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			var randomCapability = Factory.New<GlbCapability>();
			randomCapability.G4_Code = "ARD";
			testHelper.AddWorkflow(null, null
				, $"SH0,{coder.GS_Code},ASN,0"
				, $"CHK,{coder.GS_Code},ASN,1"
				);

			AssertNewReviewCreatedWithExpectedSequenceNumber(testHelper, 0);
		}

		public void TestEnsureRequiredReviewsExist_OneLessThanCheckinTask()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			var randomCapability = Factory.New<GlbCapability>();
			randomCapability.G4_Code = "ARD";
			testHelper.AddWorkflow(null, null
				, $"SH0,{coder.GS_Code},ASN,0"
				, $"CHK,{coder.GS_Code},ASN,10"
				);

			AssertNewReviewCreatedWithExpectedSequenceNumber(testHelper, 9);
		}

		public void TestEnsureRequiredReviewsExist_OneMoreThanFirstReview()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			var randomCapability = Factory.New<GlbCapability>();
			randomCapability.G4_Code = "ARD";
			testHelper.AddWorkflow(null, null
				, $"SH0,{coder.GS_Code},ASN,204"
				, $"TLT,{coder.GS_Code},CLS,198"
				, $"CBL,{reviewer.GS_Code},CLS,199"
				, $"CDU,{coder.GS_Code},CLS,200"
				, $"CDF,{coder.GS_Code},CLS,202"
				, $"PRV,{coder.GS_Code},ASN,206"
				, $"CBA,{reviewer.GS_Code},ASN,290"
				, $"CDU,{coder.GS_Code},ASN,300"
				, $"CDF,{coder.GS_Code},ASN,302"
				, $"PRV,{coder.GS_Code},ASN,306"
				, $"CBC,{reviewer.GS_Code},ASN,390"
				, $"CHK,{coder.GS_Code},ASN,400"
				);

			AssertNewReviewCreatedWithExpectedSequenceNumber(testHelper, 291);
		}

		public void TestEnsureRequiredReviewsExistWithQualityIteration()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.SetUpForQualityIterations(ZBool.False);

			testHelper.AddWorkflow(
				"YES",
				null,
				"SH0,LCD,ASN,40",
				"INV,LCD,CLS,10",
				"CDU,LCD,CLS,20",
				"CDF,LCD,CLS,30",
				"CBC,DNK,ASN,50",
				"CHK,LCD,ASN,60");

			Factory.Save();

			testHelper.AssertWorkItemTasks(6);

			testHelper.CreateQualityIteration();

			testHelper.AssertWorkItemTasks(8);
			testHelper.AssertTaskExists(10, "INV", "CLS");
			testHelper.AssertTaskExists(20, "CDU", "CLS");
			testHelper.AssertTaskExists(30, "CDF", "CLS");
			testHelper.AssertTaskExists(40, "SH0", "CAN");
			testHelper.AssertTaskExists(41, "COD", "ASN");
			testHelper.AssertTaskExists(42, "SHV", "ASN");
			testHelper.AssertTaskExists(52, "CBC", "ASN");
			testHelper.AssertTaskExists(62, "CHK", "ASN");

			//code and submit shelf on QI
			testHelper.WorkItemTasks.Single(t => t.P9_Status == "ASN" && t.P9_Type == "COD").P9_Status = "CLS";
			var shelf = testHelper.WorkItemTasks.Single(t => t.P9_Status == "ASN" && t.P9_Type == "SHV");
			shelf.P9_Type = "SH0";
			Factory.Save();

			testHelper.RefreshWorkItemTasks();
			var beforeWorkItems = testHelper.WorkItemTasks;
			var initialWorkItemCount = testHelper.WorkItemTasks.Length;
			string json1 = $"{{\"ShelfTaskPK\": \"{shelf.PK}\", \"ReviewTaskCapability\": \"{capability.G4_Code}\", \"ReviewName\": \"Review Workflow 1\", \"TaskNotes\": \"Task Notes\"}} ";
			using (var controller = GetWorkItemController())
			using (var response1 = controller.EnsureRequiredReviewsExist(GetJsonRequest(json1)))
			{
				AssertEquals(response1.Content?.ReadAsStringAsync()?.Result, HttpStatusCode.OK, response1.StatusCode);
			}

			testHelper.RefreshWorkItemTasks();
			CombineAssertions(() =>
			{
				AssertEquals("1 reviews should created", initialWorkItemCount + 1, testHelper.WorkItemTasks.Length);
				var newReviewTask = testHelper.WorkItemTasks.Except(beforeWorkItems).Single();

				AssertEquals(53, newReviewTask.P9_Sequence);
				AssertNotEquals("Should be in workflow who has checkin", newReviewTask.P9_FH_ProcessHeader, shelf.P9_FH_ProcessHeader);
			});
		}

		public void TestEnsureRequiredReviewsExistWithDeletedQualityIterationAndCH0()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.SetUpForQualityIterations(ZBool.False);

			testHelper.AddWorkflow(
				"YES",
				null,
				"SH0,LCD,ASN,40",
				"INV,LCD,CLS,10",
				"CDU,LCD,CLS,20",
				"CDF,LCD,CLS,30",
				"CBC,DNK,ASN,50",
				"CHK,LCD,ASN,60");

			Factory.Save();

			testHelper.AssertWorkItemTasks(6);

			testHelper.CreateQualityIteration();

			testHelper.AssertWorkItemTasks(8);
			testHelper.AssertTaskExists(10, "INV", "CLS");
			testHelper.AssertTaskExists(20, "CDU", "CLS");
			testHelper.AssertTaskExists(30, "CDF", "CLS");
			testHelper.AssertTaskExists(40, "SH0", "CAN");
			testHelper.AssertTaskExists(41, "COD", "ASN");
			testHelper.AssertTaskExists(42, "SHV", "ASN");
			testHelper.AssertTaskExists(52, "CBC", "ASN");
			testHelper.AssertTaskExists(62, "CHK", "ASN");

			var iterationLinks = testHelper.ShelfTask.IterationLinks;
			//Naughty user had a SH0 Quality Iterated due to DAT not liking them. so they deleted the workflow leaving the link pointing to null
			foreach (IProcessTaskIterationLink iterationLink in iterationLinks)
			{
				iterationLink.P9I_FH_IterationWorkflow = ZGuid.Empty;
			}

			testHelper.ShelfTask.P9_Status = "ASN";
			Factory.Save();

			var beforeWorkItems = testHelper.WorkItemTasks;
			var initialWorkItemCount = testHelper.WorkItemTasks.Length;
			string json1 = $"{{\"ShelfTaskPK\": \"{testHelper.ShelfTask.PK}\", \"ReviewTaskCapability\": \"{capability.G4_Code}\", \"ReviewName\": \"Review Workflow 1\", \"TaskNotes\": \"Task Notes\"}} ";
			using (var controller = GetWorkItemController())
			using (var response1 = controller.EnsureRequiredReviewsExist(GetJsonRequest(json1)))
			{
				AssertEquals(response1.Content?.ReadAsStringAsync()?.Result, HttpStatusCode.OK, response1.StatusCode);
			}

			testHelper.RefreshWorkItemTasks();
			CombineAssertions(() =>
			{
				AssertEquals("1 reviews should created", initialWorkItemCount + 1, testHelper.WorkItemTasks.Length);
				var newReviewTask = testHelper.WorkItemTasks.Except(beforeWorkItems).Single();

				AssertEquals(53, newReviewTask.P9_Sequence);
				AssertEquals("Should be in same workflow as checkin", newReviewTask.P9_FH_ProcessHeader, testHelper.ShelfTask.P9_FH_ProcessHeader);
			});
		}

		public void TestEnsureRequiredReviewsExistWithQualityIterationAndWeirdNumbers()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.SetUpForQualityIterations(ZBool.False);

			testHelper.AddWorkflow(
				"YES",
				null,
				"SH0,LCD,ASN,40",
				"INV,LCD,CLS,10",
				"CDU,LCD,CLS,20",
				"CDF,LCD,CLS,30",
				"CBC,DNK,ASN,50",
				"CHK,LCD,ASN,60");

			Factory.Save();

			testHelper.AssertWorkItemTasks(6);

			testHelper.CreateQualityIteration();

			testHelper.AssertWorkItemTasks(8);
			testHelper.AssertTaskExists(10, "INV", "CLS");
			testHelper.AssertTaskExists(20, "CDU", "CLS");
			testHelper.AssertTaskExists(30, "CDF", "CLS");
			testHelper.AssertTaskExists(40, "SH0", "CAN");
			testHelper.AssertTaskExists(41, "COD", "ASN");
			testHelper.AssertTaskExists(42, "SHV", "ASN");
			testHelper.AssertTaskExists(52, "CBC", "ASN");
			testHelper.AssertTaskExists(62, "CHK", "ASN");

			//code and submit shelf on QI
			testHelper.WorkItemTasks.Single(t => t.P9_Status == "ASN" && t.P9_Type == "COD").P9_Status = "CLS";
			var shelf = testHelper.WorkItemTasks.Single(t => t.P9_Status == "ASN" && t.P9_Type == "SHV");
			shelf.P9_Type = "SH0";
			shelf.P9_Sequence = 123; //since this is in a Quality Iteration it's actual order should be before the CBC review task
			Factory.Save();

			testHelper.RefreshWorkItemTasks();
			var beforeWorkItems = testHelper.WorkItemTasks;
			var initialWorkItemCount = testHelper.WorkItemTasks.Length;
			string json1 = $"{{\"ShelfTaskPK\": \"{shelf.PK}\", \"ReviewTaskCapability\": \"{capability.G4_Code}\", \"ReviewName\": \"Review Workflow 1\", \"TaskNotes\": \"Task Notes\"}} ";
			using (var controller = GetWorkItemController())
			using (var response1 = controller.EnsureRequiredReviewsExist(GetJsonRequest(json1)))
			{
				AssertEquals(response1.Content?.ReadAsStringAsync()?.Result, HttpStatusCode.OK, response1.StatusCode);
			}

			testHelper.RefreshWorkItemTasks();
			CombineAssertions(() =>
			{
				AssertEquals("1 reviews should created", initialWorkItemCount + 1, testHelper.WorkItemTasks.Length);
				var newReviewTask = testHelper.WorkItemTasks.Except(beforeWorkItems).Single();

				AssertEquals(53, newReviewTask.P9_Sequence);
				AssertNotEquals("Should be in workflow who has checkin", newReviewTask.P9_FH_ProcessHeader, shelf.P9_FH_ProcessHeader);
			});
		}

		public void TestEnsureRequiredReviewsExistWithMultipleQualityIterations()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.SetUpForQualityIterations(ZBool.False);

			testHelper.AddWorkflow(
				"YES",
				null,
				"SH0,LCD,ASN,40",
				"INV,LCD,CLS,10",
				"CDU,LCD,CLS,20",
				"CDF,LCD,CLS,30",
				"CBC,DNK,ASN,50",
				"CHK,LCD,ASN,60");

			Factory.Save();

			testHelper.AssertWorkItemTasks(6);

			testHelper.CreateQualityIteration();

			testHelper.AssertWorkItemTasks(8);

			//code and submit shelf on QI
			testHelper.WorkItemTasks.Single(t => t.P9_Status == "ASN" && t.P9_Type == "COD").P9_Status = "CLS";
			var shelf1 = testHelper.WorkItemTasks.Single(t => t.P9_Status == "ASN" && t.P9_Type == "SHV");
			shelf1.P9_Type = "SH0";
			testHelper.ShelfTask = shelf1;
			Factory.Save();

			testHelper.CreateQualityIteration();

			testHelper.AssertWorkItemTasks(10);
			testHelper.AssertTaskExists(10, "INV", "CLS");
			testHelper.AssertTaskExists(20, "CDU", "CLS");
			testHelper.AssertTaskExists(30, "CDF", "CLS");
			testHelper.AssertTaskExists(40, "SH0", "CAN");
			testHelper.AssertTaskExists(41, "COD", "CLS");
			testHelper.AssertTaskExists(42, "SH0", "CAN");
			testHelper.AssertTaskExists(43, "COD", "ASN");
			testHelper.AssertTaskExists(44, "SHV", "ASN");
			testHelper.AssertTaskExists(54, "CBC", "ASN");
			testHelper.AssertTaskExists(64, "CHK", "ASN");

			//code and submit shelf on QI
			testHelper.WorkItemTasks.Single(t => t.P9_Status == "ASN" && t.P9_Type == "COD").P9_Status = "CLS";
			var shelf2 = testHelper.WorkItemTasks.Single(t => t.P9_Status == "ASN" && t.P9_Type == "SHV");
			shelf2.P9_Type = "SH0";
			testHelper.ShelfTask = shelf2;
			Factory.Save();

			testHelper.RefreshWorkItemTasks();
			var beforeWorkItems = testHelper.WorkItemTasks;
			var initialWorkItemCount = testHelper.WorkItemTasks.Length;
			string json1 = $"{{\"ShelfTaskPK\": \"{shelf2.PK}\", \"ReviewTaskCapability\": \"{capability.G4_Code}\", \"ReviewName\": \"Review Workflow 1\", \"TaskNotes\": \"Task Notes\"}} ";
			using (var controller = GetWorkItemController())
			using (var response1 = controller.EnsureRequiredReviewsExist(GetJsonRequest(json1)))
			{
				AssertEquals(response1.Content?.ReadAsStringAsync()?.Result, HttpStatusCode.OK, response1.StatusCode);
			}

			testHelper.RefreshWorkItemTasks();
			CombineAssertions(() =>
			{
				AssertEquals("1 reviews should created", initialWorkItemCount + 1, testHelper.WorkItemTasks.Length);
				var newReviewTask = testHelper.WorkItemTasks.Except(beforeWorkItems).Single();

				AssertEquals(55, newReviewTask.P9_Sequence);
				AssertNotEquals("Should be in workflow who has checkin", newReviewTask.P9_FH_ProcessHeader, shelf2.P9_FH_ProcessHeader);
			});
		}

		public void TestEnsureRequiredReviewsExist_OneMoreThanFirstReviewBlock()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			var randomCapability = Factory.New<GlbCapability>();
			randomCapability.G4_Code = "ARD";
			testHelper.AddWorkflow(null, null
				, $"SH0,{coder.GS_Code},ASN,204"
				, $"TLT,{coder.GS_Code},CLS,198"
				, $"CBL,{reviewer.GS_Code},CLS,199"
				, $"CDU,{coder.GS_Code},CLS,200"
				, $"CDF,{coder.GS_Code},CLS,202"
				, $"PRV,{coder.GS_Code},ASN,206"
				, $"CBA,{reviewer.GS_Code},ASN,290"
				, $"CBB,{reviewer.GS_Code},ASN,295"
				, $"CDU,{coder.GS_Code},ASN,300"
				, $"CDF,{coder.GS_Code},ASN,302"
				, $"PRV,{coder.GS_Code},ASN,306"
				, $"CBC,{reviewer.GS_Code},ASN,390"
				, $"CHK,{coder.GS_Code},ASN,400"
				);

			AssertNewReviewCreatedWithExpectedSequenceNumber(testHelper, 296);
		}

		void AssertNewReviewCreatedWithExpectedSequenceNumber(ProcessedShelfsServiceTaskTestHelper testHelper, int expectedSequenceNumber)
		{
			Factory.Save();
			testHelper.RefreshWorkItemTasks();
			var beforeWorkItems = testHelper.WorkItemTasks;
			var initialWorkItemCount = testHelper.WorkItemTasks.Length;
			using (var controller = GetWorkItemController())
			{
				var jsonRequest = GetJsonRequest(testHelper);
				using (var response = controller.EnsureRequiredReviewsExist(jsonRequest))
				{
					AssertEquals(response.Content?.ReadAsStringAsync()?.Result, HttpStatusCode.OK, response.StatusCode);
				}

				testHelper.RefreshWorkItemTasks();
				CombineAssertions(() =>
				{
					AssertEquals("1 review should created", initialWorkItemCount + 1, testHelper.WorkItemTasks.Length);
					var newReviewTask = testHelper.WorkItemTasks.Except(beforeWorkItems).Single();

					AssertContains("Task notes should be set", jsonRequest["TaskNotes"].ToString(), newReviewTask.P9_NotesAsString);
					AssertEquals("Task should have description", $"Aspect Review - {jsonRequest["ReviewName"].ToString()} (with CB)", newReviewTask.P9_Description);
					Assert("No other task should have the task note message", testHelper.WorkItemTasks.Where(t => t != newReviewTask).All(t => !t.P9_NotesAsString.Contains(jsonRequest["TaskNotes"].ToString())));
					AssertEquals("Sequence Number of new task should be ...", expectedSequenceNumber, newReviewTask.P9_Sequence);
					AssertEquals("Time estimate should exist on new task", new ZInt(10).GetDateTimeFromMinutes(), newReviewTask.P9_EstDuration);
				});

				using (var repeatedResponse = controller.EnsureRequiredReviewsExist(jsonRequest))
				{
					AssertEquals(repeatedResponse.Content?.ReadAsStringAsync()?.Result, HttpStatusCode.OK, repeatedResponse.StatusCode);
				}

				testHelper.RefreshWorkItemTasks();
				AssertEquals("the same review should used", initialWorkItemCount + 1, testHelper.WorkItemTasks.Length);
			}
		}

		#endregion

		#region EnsureRequiredReviewsExist Uses Existing Review

		public void TestEnsureRequiredReviewsExist_UsesExistingReview_IfOneExistsWithRequiredCapability()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.AddWorkflow(null, null,
				$"SH0,{coder.GS_Code},ASN,20",
				$"CDF,{coder.GS_Code},CLS,10",
				$"{reviewTaskType},,ASN,30,{capability.PK},Aspect Review - AspectName (with CB)");

			AssertExistingReviewUsed(testHelper, 30);
		}

		public void TestEnsureRequiredReviewsExist_UsesExistingReview_IfOneExistsAndAssignedWithRequiredCapability()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.AddWorkflow(null, null,
				$"SH0,{coder.GS_Code},ASN,20",
				$"CDF,{coder.GS_Code},CLS,10",
				$"{reviewTaskType},{capableReviewer.GS_Code},ASN,30,{capability.PK},Aspect Review - AspectName (with CB)");

			AssertExistingReviewUsed(testHelper, 30);
		}

		public void TestEnsureRequiredReviewsExist_UsesExistingReview_IfOneIsSuspendedWithRequiredCapability()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.AddWorkflow(null, null,
				$"SH0,{coder.GS_Code},ASN,20",
				$"CDF,{coder.GS_Code},CLS,10",
				$"{reviewTaskType},{capableReviewer.GS_Code},SUS,30,{capability.PK},Aspect Review - AspectName (with CB)");

			AssertExistingReviewUsed(testHelper, 30);
		}

		public void TestEnsureRequiredReviewsExist_UsesExistingReview_IfOneIsWorkingWithRequiredCapability()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.AddWorkflow(null, null,
				$"SH0,{coder.GS_Code},ASN,20",
				$"CDF,{coder.GS_Code},CLS,10",
				$"{reviewTaskType},{capableReviewer.GS_Code},WRK,30,{capability.PK},Aspect Review - AspectName (with CB)");

			AssertExistingReviewUsed(testHelper, 30);
		}

		public void TestEnsureRequiredReviewsExist_UsesExistingReview_EvenIfOtherClosedReviewsExists()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.AddWorkflow(null, null,
				$"SH0,{coder.GS_Code},ASN,20",
				$"CDF,{coder.GS_Code},CLS,10",
				$"{reviewTaskType},{reviewer.GS_Code},CLS,23,{capability.PK}",
				$"{reviewTaskType},{capableReviewer.GS_Code},CLS,25",
				$"{reviewTaskType},{reviewer.GS_Code},CLS,33,{capability.PK}",
				$"{reviewTaskType},{capableReviewer.GS_Code},CLS,35",
				$"{reviewTaskType},,ASN,30,{capability.PK},Aspect Review - AspectName (with CB)");

			AssertExistingReviewUsed(testHelper, 30);
		}

		public void TestEnsureRequiredReviewsExist_UsesExistingReview_WhenOtherAspectReviewsExists()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.AddWorkflow(null, null,
				$"SH0,{coder.GS_Code},ASN,20",
				$"CDF,{coder.GS_Code},CLS,10",
				$"{reviewTaskType},,ASN,30,{capability.PK},Aspect Review - WrongAspectName (with CB)",
				$"{reviewTaskType},,ASN,40,{capability.PK},Aspect Review - DifferentAspectName (with CB)",
				$"{reviewTaskType},,ASN,50,{capability.PK},Aspect Review - AspectName (with CB)",
				$"{reviewTaskType},,ASN,60,{capability.PK},Aspect Review - AnotherAspect (with CB)",
				$"{reviewTaskType},,ASN,70,{capability.PK},Aspect Review - TooManyAspects (with CB)");

			AssertExistingReviewUsed(testHelper, 50);
		}

		public void TestEnsureRequiredReviewsExist_UsesExistingReview_ExistsWithTruncatedName()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.AddWorkflow(null, null,
				$"SH0,{coder.GS_Code},ASN,20",
				$"CDF,{coder.GS_Code},CLS,10",
				$"{reviewTaskType},,ASN,30,{capability.PK},Aspect Review - AspectNameTruncatedAfter (with CB)");

			AssertExistingReviewUsed(testHelper, 30, "AspectNameTruncatedAfter24Characters");
		}

		public void TestEnsureRequiredReviewsExist_UsesExistingReview_EvenWithOtherReviewBeforeShelf()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.AddWorkflow(null, null,
				$"SH0,{coder.GS_Code},ASN,120",
				$"CDF,{coder.GS_Code},CLS,10",
				$"SH0,{coder.GS_Code},ASN,20",
				$"{reviewTaskType},{reviewer.GS_Code},CLS,30,{capability.PK}",
				$"{reviewTaskType},{capableReviewer.GS_Code},CLS,40",
				$"CDF,{coder.GS_Code},CLS,110",
				$"{reviewTaskType},{capableReviewer.GS_Code},ASN,130,{capability.PK},Aspect Review - AspectName (with CB)");

			AssertExistingReviewUsed(testHelper, 130);
		}

		public void TestEnsureRequiredReviewsExist_UsesFirstReview_WithMultipleAvailable()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.AddWorkflow(null, null,
				$"SH0,{coder.GS_Code},ASN,20",
				$"CDF,{coder.GS_Code},CLS,10",
				$"{reviewTaskType},{reviewer.GS_Code},ASN,30,{capability.PK},Aspect Review - AspectName (with CB)",
				$"{reviewTaskType},{capableReviewer.GS_Code},ASN,40,{capability.PK},Aspect Review - AspectName (with CB)",
				$"{reviewTaskType},,ASN,50,{capability.PK},Aspect Review - AspectName (with CB)");

			AssertExistingReviewUsed(testHelper, 40);
		}

		public void TestEnsureRequiredReviewsExist_AppendsToExistingTaskNotes()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.AddWorkflow(null, null,
				$"SH0,{coder.GS_Code},ASN,20",
				$"CDF,{coder.GS_Code},CLS,10",
				$"{reviewTaskType},{capableReviewer.GS_Code},ASN,30,{capability.PK},Aspect Review - AspectName (with CB)");
			testHelper.RefreshWorkItemTasks();
			var reviewTask = testHelper.WorkItemTasks.Where(t => t.P9_Sequence == 30).Single();
			var initialNotesForTask = "These initial notes\nshould still appear\nin the tasknotes\nbefore the other notes\nare added";
			reviewTask.P9_NotesAsString = initialNotesForTask;

			AssertExistingReviewUsed(testHelper, 30);

			reviewTask = testHelper.WorkItemTasks.Where(t => t.P9_Sequence == 30).Single();

			AssertContains("Task notes should be appended", "These initial notes", reviewTask.P9_NotesAsString);
			AssertContains("Task notes should be appended", GetJsonRequest(testHelper)["TaskNotes"].ToString(), reviewTask.P9_NotesAsString);
		}

		void AssertExistingReviewUsed(ProcessedShelfsServiceTaskTestHelper testHelper, int expectedSequenceNumber, string aspectName = "AspectName")
		{
			Factory.Save();
			testHelper.RefreshWorkItemTasks();
			var initialWorkItemCount = testHelper.WorkItemTasks.Length;
			var jsonRequest = GetJsonRequest(testHelper, aspectName);
			var truncatedAspectName = aspectName.Substring(0, Math.Min(24, aspectName.Length));
			using (var controller = GetWorkItemController())
			using (var response = controller.EnsureRequiredReviewsExist(jsonRequest))
			{
				AssertEquals(response.Content?.ReadAsStringAsync()?.Result, HttpStatusCode.OK, response.StatusCode);
			}

			testHelper.RefreshWorkItemTasks();
			CombineAssertions(() =>
			{
				AssertEquals("No review should created as existing review capable", initialWorkItemCount, testHelper.WorkItemTasks.Length);
				var reviewTask = testHelper.WorkItemTasks
					.Where(t =>
						t.P9_Type == reviewTaskType
						&& t.P9_Sequence == expectedSequenceNumber
						&& !t.IsClosed
						&& t.P9_Description == $"Aspect Review - {truncatedAspectName} (with CB)"
						&& (t.P9_GS_NKAssignedStaffMember == "CRV" || (t.P9_GS_NKAssignedStaffMember == "" && t.P9_G4_RequiredCapability.Equals(capability.PK))))
					.First();
				AssertContains("Task notes should be set", jsonRequest["TaskNotes"].ToString(), reviewTask.P9_NotesAsString);
				Assert("No other task should have the task note message", testHelper.WorkItemTasks.Where(t => t != reviewTask).All(t => !t.P9_NotesAsString.Contains(jsonRequest["TaskNotes"].ToString())));
			});
		}

		#endregion

		#region Consider Checkin Task

		public void TestEnsureRequiredReviewsExist_CheckInWillOnlyUseReviewIfIsOpenAndSameOrEarlierSeq()
		{
			var testList = new List<Tuple<int, string, bool>>
			{
				new Tuple<int, string, bool>(0, "ASN", true),
				new Tuple<int, string, bool>(2, "OPN", true),
				new Tuple<int, string, bool>(2, "ASN", true),
				new Tuple<int, string, bool>(2, "WRK", true),
				new Tuple<int, string, bool>(2, "SUS", true),
				new Tuple<int, string, bool>(2, "CLS", false),
				new Tuple<int, string, bool>(2, "CAN", false),
				new Tuple<int, string, bool>(3, "ASN", false)
			};

			foreach (var values in testList)
			{
				var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);

				testHelper.AddWorkflow(null, null,
					$"CH0,{coder.GS_Code},ASN,2",
					$"CDF,{coder.GS_Code},CLS,1",
					$"{reviewTaskType},{capableReviewer.GS_Code},{values.Item2},{values.Item1},{capability.PK},Aspect Review - AspectName (with CB)");

				if (values.Item3)
				{
					AssertExistingReviewUsed(testHelper, values.Item1);
				}
				else
				{
					AssertNewReviewCreatedWithExpectedSequenceNumber(testHelper, 1);
				}
			}
		}

		public void TestEnsureRequiredReviewsExist_CheckInWillNotConsiderReviewsInOtherWorkflows()
		{
			var testList = new List<Tuple<int, string>>
			{
				new Tuple<int, string>(1, "ASN"),
				new Tuple<int, string>(2, "OPN"),
				new Tuple<int, string>(2, "ASN"),
				new Tuple<int, string>(2, "WRK"),
				new Tuple<int, string>(2, "SUS"),
				new Tuple<int, string>(3, "ASN")
			};

			foreach (var values in testList)
			{
				var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);

				testHelper.AddWorkflow(null, null,
					$"CH0,{coder.GS_Code},ASN,2",
					$"CDF,{coder.GS_Code},CLS,1");
				testHelper.AddWorkflow(null, null,
					null,
					$"{reviewTaskType},{capableReviewer.GS_Code},{values.Item2},{values.Item1},{capability.PK},Aspect Review - AspectName (with CB)");
				AssertNewReviewCreatedWithExpectedSequenceNumber(testHelper, 1);
			}
		}

		public void TestEnsureRequiredReviewsExist_ShelfAndCheckInSameWorkflowWillNotConsiderOtherWorkflows_Existing()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);

			testHelper.AddWorkflow(null, null,
				null,
				$"{reviewTaskType},{capableReviewer.GS_Code},ASN,5,{capability.PK},Aspect Review - AspectName (with CB)");

			testHelper.AddWorkflow(null, null,
				$"SH0,{coder.GS_Code},ASN,2",
				$"CDF,{coder.GS_Code},CLS,1",
				$"{reviewTaskType},{capableReviewer.GS_Code},ASN,8,{capability.PK},Aspect Review - AspectName (with CB)",
				$"CHK,{coder.GS_Code},ASN,10");

			testHelper.AddWorkflow(null, null,
				null,
				$"{reviewTaskType},{capableReviewer.GS_Code},ASN,6,{capability.PK},Aspect Review - AspectName (with CB)");

			AssertExistingReviewUsed(testHelper, 8);
		}

		public void TestEnsureRequiredReviewsExist_ShelfAndCheckInSameWorkflowWillNotConsiderOtherWorkflows_New()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);

			testHelper.AddWorkflow(null, null,
				null,
				$"{reviewTaskType},{capableReviewer.GS_Code},ASN,5,{capability.PK},Aspect Review - AspectName (with CB)");

			testHelper.AddWorkflow(null, null,
				$"SH0,{coder.GS_Code},ASN,2",
				$"CDF,{coder.GS_Code},CLS,1",
				$"CHK,{coder.GS_Code},ASN,10");

			testHelper.AddWorkflow(null, null,
				null,
				$"{reviewTaskType},{capableReviewer.GS_Code},ASN,6,{capability.PK},Aspect Review - AspectName (with CB)");

			AssertNewReviewCreatedWithExpectedSequenceNumber(testHelper, 9);
		}

		public void TestEnsureRequiredReviewsExist_ShelfAndCheckInSameWorkflowWillNotConsiderOtherWorkflows_OutsideSeqNum()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);

			testHelper.AddWorkflow(null, null,
				null,
				$"{reviewTaskType},{capableReviewer.GS_Code},ASN,5,{capability.PK},Aspect Review - AspectName (with CB)");

			testHelper.AddWorkflow(null, null,
				$"SH0,{coder.GS_Code},ASN,2",
				$"CDF,{coder.GS_Code},CLS,1",
				$"{reviewTaskType},{capableReviewer.GS_Code},ASN,20,{capability.PK},Aspect Review - AspectName (with CB)",
				$"{reviewTaskType},{capableReviewer.GS_Code},ASN,0,{capability.PK},Aspect Review - AspectName (with CB)",
			$"CHK,{coder.GS_Code},ASN,10");

			testHelper.AddWorkflow(null, null,
				null,
				$"{reviewTaskType},{capableReviewer.GS_Code},ASN,6,{capability.PK},Aspect Review - AspectName (with CB)");

			AssertNewReviewCreatedWithExpectedSequenceNumber(testHelper, 9);
		}

		public void TestEnsureRequiredReviewsExist_ShelfAndCheckInDifferentWorkflowWillConsiderAll()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);

			testHelper.AddWorkflow(null, null,
				null,
				$"{reviewTaskType},{capableReviewer.GS_Code},ASN,5,{capability.PK},Aspect Review - AspectName (with CB)");

			testHelper.AddWorkflow(null, null,
				$"SH0,{coder.GS_Code},ASN,2",
				$"CDF,{coder.GS_Code},CLS,1",
				$"{reviewTaskType},{capableReviewer.GS_Code},ASN,9,{capability.PK},Aspect Review - AspectName (with CB)");

			testHelper.AddWorkflow(null, null,
				null,
				$"{reviewTaskType},{capableReviewer.GS_Code},ASN,8,{capability.PK},Aspect Review - AspectName (with CB)",
				$"CHK,{coder.GS_Code},ASN,10");

			AssertExistingReviewUsed(testHelper, 9);
		}

		#endregion

		#region EnsureRequiredReviewsExist Creates Tasks To Fix Unknown Capability Code

		public void TestEnsureRequiredReviewsExist_InvalidCapabilityShouldRetunOkAndCreateTasksToFixCapability()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.AddWorkflow(null, null,
				$"SH0,{coder.GS_Code},ASN,1",
				$"CDF,{coder.GS_Code},CLS,1");
			var datAdmins = Factory.New<GlbCapability>();
			datAdmins.G4_Code = "DAT";
			Factory.Save();
			var shelfTask = testHelper.ShelfTask;

			capability = Factory.New<GlbCapability>();
			var capabilityCode = capability.G4_Code = "ERR";

			var taskDescription = "Fix aspect capability assignment in DAT";
			var capabilityNote = $"The aspect capability '{capabilityCode}' could not be found.";
			var investigateNote = "Please determine the correct capability for this aspect review," +
								  " and communicate with DAT admins to get it fixed in DAT.";
			var assistNote = $"Please assist {shelfTask.P9_GS_NKAssignedStaffMember} to fix DAT's aspect review capability assignment.";

			using (var controller = GetWorkItemController())
			using (var response = controller.EnsureRequiredReviewsExist(GetJsonRequest(testHelper)))
			{
				AssertEquals(HttpStatusCode.OK, response.StatusCode);
			}

			testHelper.RefreshWorkItemTasks();
			var jsonRequest = GetJsonRequest(testHelper, "");

			var taskInv = AssertNewTaskCreated("INV");
			AssertEquals("INV task should be assigned to shelf staff", shelfTask.AssignedStaffMember, taskInv.AssignedStaffMember);
			AssertContains($"INV task should contain investigation note", investigateNote, taskInv.P9_NotesAsString);

			var taskAst = AssertNewTaskCreated("AST");
			AssertNull("AST task should not have staff assigned", taskAst.AssignedStaffMember);
			AssertEquals("AST task should be assigned to DAT admins", datAdmins.PK, taskAst.P9_G4_RequiredCapability);
			AssertContains($"AST task should contain assist note", assistNote, taskAst.P9_NotesAsString);

			ProcessTask AssertNewTaskCreated(string taskType)
			{
				var task = testHelper.WorkItemTasks.Where(t => t.P9_Type == taskType).FirstOrDefault();

				AssertNotNull($"{taskType} task should be created", task);
				AssertEquals($"{taskType} task should have expected description", taskDescription, task.P9_Description);
				AssertEquals($"{taskType} task should have same sequence as shelf task", shelfTask.P9_Sequence, task.P9_Sequence);
				AssertEquals($"{taskType} task should have same prcess header as shelf task", shelfTask.P9_FH_ProcessHeader, task.P9_FH_ProcessHeader);
				AssertContains($"{taskType} task should contain capability missing note", capabilityNote, task.P9_NotesAsString);
				AssertContains($"{taskType} task should contain aspect notes", jsonRequest["TaskNotes"].ToString(), task.P9_NotesAsString);
				return task;
			}
		}

		#endregion

		#region EnsureRequiredReviewsExist Errors

		public void TestEnsureRequiredReviewsExist_NoRequestShouldReturnForbidden()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.AddWorkflow(null, null,
				$"SH0,{coder.GS_Code},ASN,1",
				$"CDF,{coder.GS_Code},CLS,1");
			using (var controller = new WorkItemController())
			using (var response = controller.EnsureRequiredReviewsExist(GetJsonRequest(testHelper)))
			{
				AssertEquals(HttpStatusCode.Forbidden, response.StatusCode);
			}
		}

		public void TestEnsureRequiredReviewsExist_ShelfTaskNotFoundShouldGiveBadRequest()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.AddWorkflow(null, null,
				$"SH0,{coder.GS_Code},ASN,1",
				$"CDF,{coder.GS_Code},CLS,1");
			Factory.Save();
			testHelper.ShelfTask = Factory.NewWithValidTestData<WorkItemProcessTask>();

			using (var controller = GetWorkItemController())
			using (var response = controller.EnsureRequiredReviewsExist(GetJsonRequest(testHelper)))
			{
				AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
				AssertContains("Shelf Task not found with PK", response.Content.ReadAsStringAsync().Result);
			}
		}

		public void TestEnsureRequiredReviewsExist_BadJSONRequestShouldGiveBadRequest()
		{
			using (var controller = GetWorkItemController())
			{
				var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
				testHelper.AddWorkflow(null, null,
					$"SH0,{coder.GS_Code},ASN,1",
					$"CDF,{coder.GS_Code},CLS,1");

				Factory.Save();

				using (var response = controller.EnsureRequiredReviewsExist(JObject.Parse("{}")))
				{
					AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
					AssertContains("Invalid Request, Required JSON keys are ShelfTaskPK, ReviewTaskCapability, ReviewName and TaskNotes", response.Content.ReadAsStringAsync().Result);
				}

				using (var response = controller.EnsureRequiredReviewsExist(JObject.Parse($"{{\"ShelfTaskPK\": \"{testHelper.ShelfTask.PK}\", \"ReviewTaskCapability\": \"{capability.PK}\", \"ReviewName\": \"Review Name\", \"NoTaskNotes\": \"wrong key name for TaskNotes\"}} ")))
				{
					AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
					AssertContains("Invalid Request, Required JSON keys are ShelfTaskPK, ReviewTaskCapability, ReviewName and TaskNotes", response.Content.ReadAsStringAsync().Result);
				}

				using (var response = controller.EnsureRequiredReviewsExist(JObject.Parse($"{{\"ShelfTaskPK\": \"not-a-guid\", \"ReviewTaskCapability\": \"{capability.PK}\", \"ReviewName\": \"Review Name\", \"TaskNotes\": \"bad shelf GUID\"}} ")))
				{
					AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
					AssertContains("Unable to parse GUID for ShelfTaskPK", response.Content.ReadAsStringAsync().Result);
				}
			}
		}

		#endregion

		#region EnsureRequiredReviewsExist No-op

		public void TestEnsureRequiredReviewsExist_WhenShelfTaskClosed()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.AddWorkflow(null, null,
				$"SH0,{coder.GS_Code},CLS,1",
				$"CDF,{coder.GS_Code},CLS,1");

			AssertNewReviewCreated(testHelper, shouldReviewBeCreated: false);
		}

		public void TestEnsureRequiredReviewsExist_WhenShelfTaskCancelled()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.AddWorkflow(null, null,
				$"SH0,{coder.GS_Code},CAN,1",
				$"CDF,{coder.GS_Code},CLS,1");

			AssertNewReviewCreated(testHelper, shouldReviewBeCreated: false);
		}

		#endregion

		#region DoesReviewExistWithCapability

		public void TestDoesReviewExistWithCapability_ShouldReturnTrueWhenCapabilityAssignedInReview()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.AddWorkflow(null, null,
				$"SH0,{coder.GS_Code},ASN,20",
				$"CDF,{coder.GS_Code},CLS,10",
				$"{alternateReviewTaskType},,ASN,30,{capability.PK}",
				$"CHK,{coder.GS_Code},ASN,40");
			Factory.Save();
			testHelper.RefreshWorkItemTasks();
			using (var controller = GetWorkItemController())
			{
				var jsonRequest = GetJsonRequestForDoesReviewExistWithCapability(testHelper);
				using (var response = controller.DoesReviewExistWithCapability(jsonRequest))
				{
					Assert(response.Content?.ReadAsStringAsync()?.Result, response.IsSuccessStatusCode);
					AssertEquals("Expect a bool type with a value of true", true, ((ObjectContent<bool>)response.Content).Value);
				}

				testHelper.GetTaskBySequenceNumber(30).P9_G4_RequiredCapability = ZGuid.Empty;
				Factory.Save();

				using (var response = controller.DoesReviewExistWithCapability(jsonRequest))
				{
					Assert(response.Content?.ReadAsStringAsync()?.Result, response.IsSuccessStatusCode);
					AssertEquals("Expect a bool type with a value of false", false, ((ObjectContent<bool>)response.Content).Value);
				}
			}
		}

		public void TestDoesReviewExistWithCapability_ShouldReturnTrueWhenReviewAssignedStaffHasCapability()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.AddWorkflow(null, null,
				$"SH0,{coder.GS_Code},ASN,20",
				$"CDF,{coder.GS_Code},CLS,10",
				$"{alternateReviewTaskType},{capableReviewer.GS_Code},ASN,30",
				$"CHK,{coder.GS_Code},ASN,40");
			Factory.Save();
			testHelper.RefreshWorkItemTasks();
			using (var controller = GetWorkItemController())
			{
				var jsonRequest = GetJsonRequestForDoesReviewExistWithCapability(testHelper);
				using (var response = controller.DoesReviewExistWithCapability(jsonRequest))
				{
					Assert(response.Content?.ReadAsStringAsync()?.Result, response.IsSuccessStatusCode);
					AssertEquals("Expect a bool type with a value of true", true, ((ObjectContent<bool>)response.Content).Value);
				}

				testHelper.GetTaskBySequenceNumber(30).P9_GS_NKAssignedStaffMember = coder.GS_Code;
				Factory.Save();

				using (var response = controller.DoesReviewExistWithCapability(jsonRequest))
				{
					Assert(response.Content?.ReadAsStringAsync()?.Result, response.IsSuccessStatusCode);
					AssertEquals("Expect a bool type with a value of false", false, ((ObjectContent<bool>)response.Content).Value);
				}
			}
		}

		public void TestDoesReviewExistWithCapability_ShouldReturnFalseWhenNoReviewFound()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.AddWorkflow(null, null,
				$"SH0,{coder.GS_Code},ASN,30",
				$"CDU,{coder.GS_Code},CLS,10",
				$"CDF,{coder.GS_Code},CLS,20",
				$"CHK,{coder.GS_Code},ASN,40");
			Factory.Save();
			testHelper.RefreshWorkItemTasks();
			var jsonRequest = GetJsonRequestForDoesReviewExistWithCapability(testHelper);
			using (var controller = GetWorkItemController())
			using (var response = controller.DoesReviewExistWithCapability(jsonRequest))
			{
				Assert(response.Content?.ReadAsStringAsync()?.Result, response.IsSuccessStatusCode);
				AssertEquals("Expect a bool type with a value of false", false, ((ObjectContent<bool>)response.Content).Value);
			}
		}

		public void TestDoesReviewExistWithCapability_ShouldReturnFalseWhenReviewIsNotCBC()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.AddWorkflow(null, null,
				$"SH0,{coder.GS_Code},ASN,20",
				$"CDF,{coder.GS_Code},CLS,10",
				$"XXX,{capableReviewer.GS_Code},ASN,30,{capability.PK}",
				$"CHK,{coder.GS_Code},ASN,40");
			Factory.Save();
			using (var controller = GetWorkItemController())
			{
				testHelper.RefreshWorkItemTasks();
				var jsonRequest = GetJsonRequestForDoesReviewExistWithCapability(testHelper);

				for (var c = 'A'; c <= 'Z'; c++)
				{
					var reviewType = "CB" + c;
					testHelper.RefreshWorkItemTasks();
					testHelper.WorkItemTasks[2].P9_Type = reviewType;
					Factory.Save();

					using (var response = controller.DoesReviewExistWithCapability(jsonRequest))
					{
						Assert(response.Content?.ReadAsStringAsync()?.Result, response.IsSuccessStatusCode);
						AssertEquals($"Expect a bool type with a value of false unless review type is {WorkItemProcessTask.CodeReviewTaskType}, type = {reviewType}", reviewType == WorkItemProcessTask.CodeReviewTaskType, ((ObjectContent<bool>)response.Content).Value);
					}
				}
			}
		}

		public void TestDoesReviewExistWithCapability_ShouldReturnFalseWhenReviewCancelled()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.AddWorkflow(null, null,
				$"SH0,{coder.GS_Code},ASN,20",
				$"CDF,{coder.GS_Code},CLS,10",
				$"CBC,{capableReviewer.GS_Code},OPN,30,{capability.PK}",
				$"CHK,{coder.GS_Code},ASN,40");
			Factory.Save();
			testHelper.RefreshWorkItemTasks();
			using (var controller = GetWorkItemController())
			{
				var jsonRequest = GetJsonRequestForDoesReviewExistWithCapability(testHelper);

				var codelist = new ProcessTaskStatusCodeList();
				foreach (var code in codelist.GetAllCodes())
				{
					testHelper.RefreshWorkItemTasks();
					testHelper.WorkItemTasks[2].P9_Status = code;
					Factory.Save();

					using (var response = controller.DoesReviewExistWithCapability(jsonRequest))
					{
						Assert(response.Content?.ReadAsStringAsync()?.Result, response.IsSuccessStatusCode);
						AssertEquals("Expect a bool type with a value of true unless review cancelled - status = " + code, code != ProcessTaskStatusCodeList.Codes.Cancelled, ((ObjectContent<bool>)response.Content).Value);
					}
				}
			}
		}

		public void TestDoesReviewExistWithCapability_ShouldReturnFalseWhenReviewsInUnrelatedWorkflows()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.AddWorkflow(null, null,
				$"SH0,{coder.GS_Code},ASN,22",  //this is the shelf task sent to controller, but no reviews in this workflow
				$"CDF,{coder.GS_Code},CLS,21",
				$"CHK,{coder.GS_Code},ASN,23,{capability.PK}");
			testHelper.AddWorkflow(null, null,
				null,
				$"CDF,{coder.GS_Code},CLS,1",
				$"CBC,{reviewer.GS_Code},CLS,2,{capability.PK}");
			testHelper.AddWorkflow(null, null,
				null,
				$"AST,{capableReviewer.GS_Code},CLS,10",
				$"CDF,{coder.GS_Code},CLS,11",
				$"CBA,{capableReviewer.GS_Code},CLS,12");
			testHelper.AddWorkflow(null, null,
				null,
				$"CBC,{coder.GS_Code},ASN,41",
				$"PRV,{coder.GS_Code},ASN,42",
				$"CH0,{coder.GS_Code},ASN,43",
				$"CBB,{capableReviewer.GS_Code},ASN,44");
			Factory.Save();
			testHelper.RefreshWorkItemTasks();
			var jsonRequest = GetJsonRequestForDoesReviewExistWithCapability(testHelper);
			using (var controller = GetWorkItemController())
			using (var response = controller.DoesReviewExistWithCapability(jsonRequest))
			{
				Assert(response.Content?.ReadAsStringAsync()?.Result, response.IsSuccessStatusCode);
				AssertEquals("Expect a bool type with a value of false", false, ((ObjectContent<bool>)response.Content).Value);
			}
		}

		public void TestDoesReviewExistWithCapability_ShouldReturnTrueWhenReviewFoundAsFirstTask()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.AddWorkflow(null, null,
				$"SH0,{coder.GS_Code},ASN,30",
				$"CDU,{coder.GS_Code},ASN,10",
				$"CDF,{coder.GS_Code},CLS,20",
				$"CHK,{coder.GS_Code},ASN,40",
				$"CBC,{capableReviewer.GS_Code},ASN,1,{capability.PK}");
			Factory.Save();
			testHelper.RefreshWorkItemTasks();
			var jsonRequest = GetJsonRequestForDoesReviewExistWithCapability(testHelper);
			using (var controller = GetWorkItemController())
			using (var response = controller.DoesReviewExistWithCapability(jsonRequest))
			{
				Assert(response.Content?.ReadAsStringAsync()?.Result, response.IsSuccessStatusCode);
				AssertEquals("Expect a bool type with a value of true", true, ((ObjectContent<bool>)response.Content).Value);
			}
		}

		public void TestDoesReviewExistWithCapability_ShouldReturnTrueWhenReviewFoundAsLastTask()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.AddWorkflow(null, null,
				$"SH0,{coder.GS_Code},ASN,20",
				$"CDU,{coder.GS_Code},ASN,10",
				$"CDF,{coder.GS_Code},CLS,20",
				$"CHK,{coder.GS_Code},ASN,40",
				$"CBC,{capableReviewer.GS_Code},ASN,100,{capability.PK}");
			Factory.Save();
			testHelper.RefreshWorkItemTasks();
			var jsonRequest = GetJsonRequestForDoesReviewExistWithCapability(testHelper);
			using (var controller = GetWorkItemController())
			using (var response = controller.DoesReviewExistWithCapability(jsonRequest))
			{
				Assert(response.Content?.ReadAsStringAsync()?.Result, response.IsSuccessStatusCode);
				AssertEquals("Expect a bool type with a value of true", true, ((ObjectContent<bool>)response.Content).Value);
			}
		}

		public void TestDoesReviewExistWithCapability_ShouldReturnTrueWhenCheckWithClosedReview()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.AddWorkflow(null, null,
				$"CH0,{coder.GS_Code},ASN,100",
				$"CDU,{coder.GS_Code},CLS,10",
				$"CDF,{coder.GS_Code},CLS,20",
				$"SH0,{coder.GS_Code},CLS,30",
				$"CBC,{capableReviewer.GS_Code},CLS,40,{capability.PK}");
			Factory.Save();
			testHelper.RefreshWorkItemTasks();
			var jsonRequest = GetJsonRequestForDoesReviewExistWithCapability(testHelper);
			using (var controller = GetWorkItemController())
			using (var response = controller.DoesReviewExistWithCapability(jsonRequest))
			{
				Assert(response.Content?.ReadAsStringAsync()?.Result, response.IsSuccessStatusCode);
				AssertEquals("Expect a bool type with a value of true", true, ((ObjectContent<bool>)response.Content).Value);
			}
		}

		public void TestDoesReviewExistWithCapability_ShouldReturnTrueWhenReviewFoundInParentWorkflow()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			var parentWorkflow = testHelper.AddWorkflow(null, null,
				null,
				$"CBC,{capableReviewer.GS_Code},ASN,50,{capability.PK}");
			var childWorkflow = testHelper.AddChildWorkflow(parentWorkflow, "Child 1",
				$"SH0,{coder.GS_Code},ASN,30",
				$"CDF,{coder.GS_Code},CLS,10",
				$"INV,{coder.GS_Code},CLS,20");
			ProcessedShelfsServiceTaskTestHelper.CreateQualityIterationLinks(parentWorkflow.Tasks.Single(), childWorkflow);
			Factory.Save();
			testHelper.RefreshWorkItemTasks();
			var jsonRequest = GetJsonRequestForDoesReviewExistWithCapability(testHelper);
			using (var controller = GetWorkItemController())
			using (var response = controller.DoesReviewExistWithCapability(jsonRequest))
			{
				Assert(response.Content?.ReadAsStringAsync()?.Result, response.IsSuccessStatusCode);
				AssertEquals("Expect a bool type with a value of true", true, ((ObjectContent<bool>)response.Content).Value);
			}
		}

		public void TestDoesReviewExistWithCapability_ShouldReturnTrueWhenReviewFoundInChildWorkFlow()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			var parentWorkflow = testHelper.AddWorkflow(null, null,
				$"SH0,{coder.GS_Code},ASN,30",
				$"CDF,{coder.GS_Code},SUS,10",
				$"INV,{coder.GS_Code},SUS,20");
			var qualityIterationWorkflow = testHelper.AddChildWorkflow(parentWorkflow, "Child 1",
				null,
				$"CDF,{coder.GS_Code},SUS,100",
				$"CBC,{capableReviewer.GS_Code},SUS,200,{capability.PK}");
			var containmentBarrierTask = parentWorkflow.Tasks.Single(t => t.P9_Sequence == 30);
			Factory.Save();
			testHelper.RefreshWorkItemTasks();
			var jsonRequest = GetJsonRequestForDoesReviewExistWithCapability(testHelper);
			using (var controller = GetWorkItemController())
			using (var response = controller.DoesReviewExistWithCapability(jsonRequest))
			{
				Assert(response.Content?.ReadAsStringAsync()?.Result, response.IsSuccessStatusCode);
				AssertEquals("Expect a bool type with a value of true", true, ((ObjectContent<bool>)response.Content).Value);
			}
		}

		public void TestDoesReviewExistWithCapability_WhenReviewFoundInPrerequisiteWorkflow()
		{
			var bmTestHelper = ObjectFactory.Get<IBMTestHelper>();
			var jobHeader = bmTestHelper.CreateJobHeader<NewWorkItem>(Factory);
			var workflow1 = bmTestHelper.CreateWorkflow(jobHeader, "Workflow 1");
			var workflow2 = bmTestHelper.CreateWorkflow(jobHeader, "Workflow 2");
			var workflow3 = bmTestHelper.CreateWorkflow(jobHeader, "Workflow 3");
			var codeReviewTask = bmTestHelper.CreateTask(workflow1, taskType: WorkItemProcessTask.CodeReviewTaskType, sequence: 10, capability: capability, staffCode: capableReviewer.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var submissionTask = bmTestHelper.CreateTask(workflow3, taskType: WorkItemProcessTask.ShelfsetTestTask, sequence: 20);
			Factory.Save();

			AssertReviewExistsWithCapability(submissionTask.PK.ToGuid(), false);

			workflow1.GetOrCreateDependencyLink(workflow2);
			workflow2.GetOrCreateDependencyLink(workflow3);
			Factory.Save();

			AssertReviewExistsWithCapability(submissionTask.PK.ToGuid(), true);
		}

		public void TestDoesReviewExistWithCapability_ShouldReturnTrueWhenReviewFoundInSiblingWorkflow()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			var parentWorkflow = testHelper.AddWorkflow(null, null,
				null,
				$"INV,{coder.GS_Code},SUS,10");
			var workflow1 = testHelper.AddChildWorkflow(parentWorkflow, "Child 1",
				$"SH0,{coder.GS_Code},ASN,30",
				$"CDF,{coder.GS_Code},SUS,20");
			var workflow2 = testHelper.AddChildWorkflow(parentWorkflow, "Child 2",
				null,
				$"CBC,{capableReviewer.GS_Code},ASN,40,{capability.PK}");
			workflow1.GetOrCreateDependencyLink(workflow2);

			Factory.Save();
			testHelper.RefreshWorkItemTasks();
			var jsonRequest = GetJsonRequestForDoesReviewExistWithCapability(testHelper);
			using (var controller = GetWorkItemController())
			using (var response = controller.DoesReviewExistWithCapability(jsonRequest))
			{
				Assert(response.Content?.ReadAsStringAsync()?.Result, response.IsSuccessStatusCode);
				AssertEquals("Expect a bool type with a value of true", true, ((ObjectContent<bool>)response.Content).Value);
			}
		}

		public void TestDoesReviewExistWithCapability_ShouldReturnTrueWhenReviewFoundInGrandChildWorkflow()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			var parentWorkflow = testHelper.AddWorkflow(null, null,
				null,
				$"INV,{coder.GS_Code},SUS,10");
			var child1 = testHelper.AddChildWorkflow(parentWorkflow, "Child 1.1",
				$"SH0,{coder.GS_Code},ASN,30",
				$"CDF,{coder.GS_Code},SUS,20");
			var child2 = testHelper.AddChildWorkflow(child1, "Child 1.1.1",
				null,
				$"CDF,{coder.GS_Code},SUS,20");
			var child3 = testHelper.AddChildWorkflow(child2, "Child 1.1.1.1",
				null,
				$"CDF,{coder.GS_Code},SUS,20");

			testHelper.AddChildWorkflow(child3, "Child 1.1.1.1.1",
				null,
				$"CBC,{capableReviewer.GS_Code},ASN,40,{capability.PK}");

			Factory.Save();
			testHelper.RefreshWorkItemTasks();
			var jsonRequest = GetJsonRequestForDoesReviewExistWithCapability(testHelper);
			using (var controller = GetWorkItemController())
			using (var response = controller.DoesReviewExistWithCapability(jsonRequest))
			{
				Assert(response.Content?.ReadAsStringAsync()?.Result, response.IsSuccessStatusCode);
				AssertEquals("Expect a bool type with a value of true", true, ((ObjectContent<bool>)response.Content).Value);
			}
		}

		public void TestDoesReviewExistWithCapability_ShouldReturnTrueWhenReviewFoundSomewhereInCurrentWorkflow()
		{
			var bmTestHelper = ObjectFactory.Get<IBMTestHelper>();
			bmTestHelper.EnableBMSInRegistry();
			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType(EDITaskTypes.TaskShelfTest, WorkflowDescriptors.WorkItemWorkflowDescriptorCode);
			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType(EDITaskTypes.TaskActiveShelfTest, WorkflowDescriptors.WorkItemWorkflowDescriptorCode);

			var jobHeader = bmTestHelper.CreateJobHeader<NewWorkItem>(Factory);
			var dummyWorkflow1 = bmTestHelper.CreateWorkflow(jobHeader, "Workflow 1");
			bmTestHelper.CreateTask(dummyWorkflow1, coder.GS_Code, taskType: "CDU", taskStatus: ProcessTaskStatusCodeList.Codes.Assigned, sequence: 10);
			bmTestHelper.CreateTask(dummyWorkflow1, coder.GS_Code, taskType: "CDF", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 20);
			bmTestHelper.CreateTask(dummyWorkflow1, coder.GS_Code, taskType: "SH0", taskStatus: ProcessTaskStatusCodeList.Codes.Assigned, sequence: 20);
			bmTestHelper.CreateTask(dummyWorkflow1, coder.GS_Code, taskType: "CHK", taskStatus: ProcessTaskStatusCodeList.Codes.Assigned, sequence: 40);
			bmTestHelper.CreateTask(dummyWorkflow1, capableReviewer.GS_Code, taskType: "CBC", taskStatus: ProcessTaskStatusCodeList.Codes.Assigned, sequence: 50, capability: capability);

			var dummyChildWorkflow1 = bmTestHelper.CreateWorkflow(jobHeader, "Workflow 1.1");
			bmTestHelper.CreateTask(dummyChildWorkflow1, coder.GS_Code, taskType: "CDU", taskStatus: ProcessTaskStatusCodeList.Codes.Assigned, sequence: 110);
			bmTestHelper.CreateTask(dummyChildWorkflow1, coder.GS_Code, taskType: "CDF", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 120);
			bmTestHelper.CreateTask(dummyChildWorkflow1, coder.GS_Code, taskType: "SH0", taskStatus: ProcessTaskStatusCodeList.Codes.Assigned, sequence: 130);
			bmTestHelper.CreateTask(dummyChildWorkflow1, coder.GS_Code, taskType: "CHK", taskStatus: ProcessTaskStatusCodeList.Codes.Assigned, sequence: 140);
			bmTestHelper.CreateTask(dummyChildWorkflow1, capableReviewer.GS_Code, taskType: "CBC", taskStatus: ProcessTaskStatusCodeList.Codes.Assigned, sequence: 150, capability: capability);
			dummyChildWorkflow1.GetOrCreateLinkToParent(dummyWorkflow1);

			var parentWorkflow = bmTestHelper.CreateWorkflow(jobHeader, "Workflow 2");
			var task1 = bmTestHelper.CreateTask(parentWorkflow, coder.GS_Code, taskType: "CDF", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 210);
			var task2 = bmTestHelper.CreateTask(parentWorkflow, coder.GS_Code, taskType: "SH0", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 220);
			var task3 = bmTestHelper.CreateTask(parentWorkflow, capableReviewer.GS_Code, taskType: "CBC", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 260, capability: capability); // This is the relevant review task since all other CBA tasks are in workflows not related to this one via dependencies or quality iterations.

			var iterationWorkflow1 = bmTestHelper.CreateQualityIteration(task1, task2, shouldCreateWorkflowForIteration: true);
			AssertEquals(2, iterationWorkflow1.Tasks.Count());
			var iteration1Task1 = iterationWorkflow1.Tasks.First();
			var iteration1Task2 = iterationWorkflow1.Tasks.Last();

			var iterationWorkflow2 = bmTestHelper.CreateQualityIteration(iteration1Task1, iteration1Task2, shouldCreateWorkflowForIteration: true);
			AssertEquals(2, iterationWorkflow2.Tasks.Count());
			var iteration2Task1 = iterationWorkflow2.Tasks.First();
			var iteration2Task2 = iterationWorkflow2.Tasks.Last();

			var iterationWorkflow3 = bmTestHelper.CreateQualityIteration(iteration2Task1, iteration2Task2, shouldCreateWorkflowForIteration: true);
			AssertEquals(2, iterationWorkflow3.Tasks.Count());
			var iteration3Task2 = iterationWorkflow2.Tasks.Last();

			var dummyWorkflow2 = bmTestHelper.CreateWorkflow(jobHeader, "Workflow 3");
			bmTestHelper.CreateTask(dummyWorkflow2, coder.GS_Code, taskType: "CDU", taskStatus: ProcessTaskStatusCodeList.Codes.Assigned, sequence: 310);
			bmTestHelper.CreateTask(dummyWorkflow2, coder.GS_Code, taskType: "CDF", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 320);
			bmTestHelper.CreateTask(dummyWorkflow2, coder.GS_Code, taskType: "SH0", taskStatus: ProcessTaskStatusCodeList.Codes.Assigned, sequence: 330);
			bmTestHelper.CreateTask(dummyWorkflow2, coder.GS_Code, taskType: "CHK", taskStatus: ProcessTaskStatusCodeList.Codes.Assigned, sequence: 340);
			bmTestHelper.CreateTask(dummyWorkflow2, capableReviewer.GS_Code, taskType: "CBC", taskStatus: ProcessTaskStatusCodeList.Codes.Assigned, sequence: 350, capability: capability);

			var dummyChildWorkflow2 = bmTestHelper.CreateWorkflow(jobHeader, "Workflow 3.1");
			bmTestHelper.CreateTask(dummyChildWorkflow2, coder.GS_Code, taskType: "CDU", taskStatus: ProcessTaskStatusCodeList.Codes.Assigned, sequence: 410);
			bmTestHelper.CreateTask(dummyChildWorkflow2, coder.GS_Code, taskType: "CDF", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 420);
			bmTestHelper.CreateTask(dummyChildWorkflow2, coder.GS_Code, taskType: "SH0", taskStatus: ProcessTaskStatusCodeList.Codes.Assigned, sequence: 430);
			bmTestHelper.CreateTask(dummyChildWorkflow2, coder.GS_Code, taskType: "CHK", taskStatus: ProcessTaskStatusCodeList.Codes.Assigned, sequence: 440);
			bmTestHelper.CreateTask(dummyChildWorkflow2, capableReviewer.GS_Code, taskType: "CBC", taskStatus: ProcessTaskStatusCodeList.Codes.Assigned, sequence: 450, capability: capability);
			dummyChildWorkflow2.GetOrCreateLinkToParent(dummyWorkflow2);

			Factory.Save();

			var submissionTasks = new[] { task2, iteration1Task2, iteration2Task2, iteration3Task2 };

			foreach (var task in submissionTasks)
			{
				AssertReviewExistsWithCapability(task.PK.ToGuid(), true);
			}

			// With the review task no longer having type CBC all submission tasks will no longer be found to have a review with the appropriate capability.
			task3.P9_Type = "INV";
			Factory.Save();

			foreach (var task in submissionTasks)
			{
				AssertReviewExistsWithCapability(task.PK.ToGuid(), false);
			}
		}

		public void TestDoesReviewExistWithCapability_MakeSureWeCanHandleAUserWithNoCapability()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.AddWorkflow(null, null,
				$"SH0,{coder.GS_Code},ASN,20",
				$"CDF,{coder.GS_Code},CLS,10",
				$"CBC,{coder.GS_Code},ASN,30",
				$"CHK,{coder.GS_Code},ASN,40");
			Factory.Save();
			testHelper.RefreshWorkItemTasks();
			var jsonRequest = GetJsonRequestForDoesReviewExistWithCapability(testHelper);
			using (var controller = GetWorkItemController())
			using (var response = controller.DoesReviewExistWithCapability(jsonRequest))
			{
				Assert(response.Content?.ReadAsStringAsync()?.Result, response.IsSuccessStatusCode);
				AssertEquals("Expect a bool type with a value of false", false, ((ObjectContent<bool>)response.Content).Value);
			}
		}

		public void TestDoesReviewExistWithCapability_MakeSureWeCanHandleNoAssignedUser()
		{
			var newCapability = Factory.New<GlbCapability>();
			newCapability.G4_Code = "ABC";

			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.AddWorkflow(null, null,
				$"SH0,{coder.GS_Code},ASN,20",
				$"CDF,{coder.GS_Code},CLS,10",
				$"CBC,,ASN,30,{newCapability.PK}",
				$"CHK,{coder.GS_Code},ASN,40");
			Factory.Save();
			testHelper.RefreshWorkItemTasks();
			var jsonRequest = GetJsonRequestForDoesReviewExistWithCapability(testHelper);
			using (var controller = GetWorkItemController())
			using (var response = controller.DoesReviewExistWithCapability(jsonRequest))
			{
				Assert(response.Content?.ReadAsStringAsync()?.Result, response.IsSuccessStatusCode);
				AssertEquals("Expect a bool type with a value of false", false, ((ObjectContent<bool>)response.Content).Value);
			}
		}

		public void TestDoesReviewExistWithCapability_ParentChildShouldNotTraverseDifferentWorkitems()
		{
			///	dummyWI			Parent
			///					/	\
			///	shelfWI		shelf	child
			var testHelperParent = new ProcessedShelfsServiceTaskTestHelper(Factory);
			var parentWorkflow = testHelperParent.AddWorkflow(null, null,
				$"SH0,{coder.GS_Code},ASN,20",
				$"CDF,{coder.GS_Code},CLS,10",
				$"CBC,{capableReviewer.GS_Code},ASN,30,{capability.PK}",    //review in different work item
				$"CHK,{coder.GS_Code},ASN,40");
			parentWorkflow.FH_CompletionStatement = "Parent Workflow";
			var testHelperShelf = new ProcessedShelfsServiceTaskTestHelper(Factory);
			var shelfWorkflow = testHelperShelf.AddChildWorkflow(parentWorkflow, "workitem",
				$"SH0,{coder.GS_Code},ASN,120", //shelf task
				$"CDF,{coder.GS_Code},CLS,110",
				$"CBC,,ASN,130,{capability.PK}",    //review task
				$"CHK,{coder.GS_Code},ASN,140");
			shelfWorkflow.FH_CompletionStatement = "Shelf Workflow";
			var childWorkflow = testHelperShelf.AddChildWorkflow(parentWorkflow, "child woritem-workflow",
				null,
				$"SH0,{coder.GS_Code},ASN,220",
				$"CDF,{coder.GS_Code},CLS,210",
				$"CBC,,ASN,230,{capability.PK}",    //review in differnet workflow - parent is in different workitem
				$"CHK,{coder.GS_Code},ASN,240");
			childWorkflow.FH_CompletionStatement = "Child Workflow";
			Factory.Save();

			testHelperParent.RefreshWorkItemTasks();
			testHelperShelf.RefreshWorkItemTasks();
			using (var controller = GetWorkItemController())
			{
				var jsonRequest = GetJsonRequestForDoesReviewExistWithCapability(testHelperShelf);
				using (var response = controller.DoesReviewExistWithCapability(jsonRequest))
				{
					Assert(response.Content?.ReadAsStringAsync()?.Result, response.IsSuccessStatusCode);
					AssertEquals("Expect a bool type with a value of true", true, ((ObjectContent<bool>)response.Content).Value);
				}

				testHelperShelf.GetTaskBySequenceNumber(130).P9_Type = "INV";
				Factory.Save();

				using (var response = controller.DoesReviewExistWithCapability(jsonRequest))
				{
					Assert(response.Content?.ReadAsStringAsync()?.Result, response.IsSuccessStatusCode);
					AssertEquals("Expect a bool type with a value of false", false, ((ObjectContent<bool>)response.Content).Value);
				}
			}
		}

		public void TestDoesReviewExistWithCapability_ParentChildShouldNotTraverseDifferentWorkitems_ThisIsGettingSilly()
		{
			///	dummyWI			child1
			///					/	\
			///	shelfWI		parent	shelf2	child4
			///							\	/
			///	dummmWI					child3
			var testHelperDummy1 = new ProcessedShelfsServiceTaskTestHelper(Factory);
			var testHelperDummy2 = new ProcessedShelfsServiceTaskTestHelper(Factory);
			var testHelperShelf = new ProcessedShelfsServiceTaskTestHelper(Factory);
			var parentWorkflow = testHelperShelf.AddWorkflow(null, null,
				null,
				$"SH0,{coder.GS_Code},ASN,20",
				$"CDF,{coder.GS_Code},CLS,10",
				$"CBC,,ASN,30,{capability.PK}", //review task in QI parent workflow in same WI so should be considered
				$"CHK,{coder.GS_Code},ASN,40");

			var child1 = testHelperDummy1.AddChildWorkflow(parentWorkflow, "workitem",
				null,
				$"SH0,{coder.GS_Code},ASN,120",
				$"CDF,{coder.GS_Code},CLS,110",
				$"CBC,,ASN,130,{capability.PK}", //review task in different workitem
				$"CHK,{coder.GS_Code},ASN,140");

			var shelf2 = testHelperShelf.AddChildWorkflow(child1, "workitem",
				$"SH0,{coder.GS_Code},ASN,220", //submission task we care about
				$"CDF,{coder.GS_Code},CLS,210",
				$"CBC,,ASN,230,{capability.PK}", //review task in same workflow as submission task, so should be considered
				$"CHK,{coder.GS_Code},ASN,240");

			var child3 = testHelperDummy2.AddChildWorkflow(shelf2, "workitem",
				null,
				$"SH0,{coder.GS_Code},ASN,320",
				$"CDF,{coder.GS_Code},CLS,310",
				$"CBC,,ASN,330,{capability.PK}", //review task in different workitem
				$"CHK,{coder.GS_Code},ASN,340");

			var child4 = testHelperShelf.AddChildWorkflow(child3, "workitem",
				null,
				$"SH0,{coder.GS_Code},ASN,420",
				$"CDF,{coder.GS_Code},CLS,410",
				$"CBC,,ASN,430,{capability.PK}", //review task in different workflow in same WI, and not connected to submission task, so is not considered
				$"CHK,{coder.GS_Code},ASN,440");

			ProcessedShelfsServiceTaskTestHelper.CreateQualityIterationLinks(parentWorkflow.Tasks.Single(t => t.P9_Type == "CBC"), shelf2);

			Factory.Save();
			testHelperDummy1.RefreshWorkItemTasks();
			testHelperDummy2.RefreshWorkItemTasks();
			testHelperShelf.RefreshWorkItemTasks();
			using (var controller = GetWorkItemController())
			{
				var jsonRequest = GetJsonRequestForDoesReviewExistWithCapability(testHelperShelf);
				using (var response = controller.DoesReviewExistWithCapability(jsonRequest))
				{
					Assert(response.Content?.ReadAsStringAsync()?.Result, response.IsSuccessStatusCode);
					AssertEquals("Expect a bool type with a value of true", true, ((ObjectContent<bool>)response.Content).Value);
				}

				testHelperShelf.GetTaskBySequenceNumber(230).P9_Type = "INV";   //shelf
				Factory.Save();
				using (var response = controller.DoesReviewExistWithCapability(jsonRequest))
				{
					Assert(response.Content?.ReadAsStringAsync()?.Result, response.IsSuccessStatusCode);
					AssertEquals("Expect a bool type with a value of true", true, ((ObjectContent<bool>)response.Content).Value);
				}

				testHelperShelf.GetTaskBySequenceNumber(30).P9_Type = "INV";    //parent
				Factory.Save();
				using (var response = controller.DoesReviewExistWithCapability(jsonRequest))
				{
					Assert(response.Content?.ReadAsStringAsync()?.Result, response.IsSuccessStatusCode);
					AssertEquals("Expect a bool type with a value of false, since the only remaining review is not connected via a parent/child link between workflows in the same job", false, ((ObjectContent<bool>)response.Content).Value);
				}
			}
		}

		#region DoesReviewExistWithCapability Errors

		public void TestDoesReviewExistWithCapability_NoRequestShuoldReturnForbidden()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.AddWorkflow(null, null,
				$"SH0,{coder.GS_Code},ASN,1",
				$"CDF,{coder.GS_Code},CLS,1");
			using (var controller = new WorkItemController())
			{
				var response = controller.DoesReviewExistWithCapability(GetJsonRequestForDoesReviewExistWithCapability(testHelper));
				AssertEquals(HttpStatusCode.Forbidden, response.StatusCode);
			}
		}

		public void TestDoesReviewExistWithCapability_ShelfTaskNotFoundShouldGiveBadRequest()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.AddWorkflow(null, null,
				$"SH0,{coder.GS_Code},ASN,1",
				$"CDF,{coder.GS_Code},CLS,1");
			Factory.Save();
			testHelper.ShelfTask = Factory.NewWithValidTestData<WorkItemProcessTask>();

			using (var controller = GetWorkItemController())
			using (var response = controller.DoesReviewExistWithCapability(GetJsonRequestForDoesReviewExistWithCapability(testHelper)))
			{
				AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
				AssertContains("Shelf Task not found with PK", response.Content.ReadAsStringAsync().Result);
			}
		}

		public void TestDoesReviewExistWithCapability_InvalidCapabilityShouldGiveBadRequest()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.AddWorkflow(null, null,
				$"SH0,{coder.GS_Code},ASN,1",
				$"CDF,{coder.GS_Code},CLS,1");

			Factory.Save();
			capability = Factory.New<GlbCapability>();
			capability.G4_Code = "ERR";
			using (var controller = GetWorkItemController())
			using (var response = controller.DoesReviewExistWithCapability(GetJsonRequestForDoesReviewExistWithCapability(testHelper)))
			{
				AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
				AssertContains("Unable to find Capability, ReviewTaskCapability: ERR", response.Content.ReadAsStringAsync().Result);
			}
		}

		public void TestDoesReviewExistWithCapability_BadJSONRequestShouldGiveBadRequest()
		{
			using (var controller = GetWorkItemController())
			{
				var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
				testHelper.AddWorkflow(null, null,
					$"SH0,{coder.GS_Code},ASN,1",
					$"CDF,{coder.GS_Code},CLS,1");

				Factory.Save();

				using (var response = controller.DoesReviewExistWithCapability(JObject.Parse("{}")))
				{
					AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
					AssertContains("Invalid Request, Required JSON keys are ShelfTaskPK and ReviewTaskCapability", response.Content.ReadAsStringAsync().Result);
				}

				using (var response = controller.DoesReviewExistWithCapability(JObject.Parse($"{{\"ShelfTaskPK\": \"{testHelper.ShelfTask.PK}\", \"BadKeyReviewTaskCapability\": \"{capability.PK}\"}} ")))
				{
					AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
					AssertContains("Invalid Request, Required JSON keys are ShelfTaskPK and ReviewTaskCapability", response.Content.ReadAsStringAsync().Result);
				}

				using (var response = controller.DoesReviewExistWithCapability(JObject.Parse($"{{\"ShelfTaskPK\": \"not-a-guid\", \"ReviewTaskCapability\": \"{capability.PK}\"}} ")))
				{
					AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
					AssertContains("Unable to parse GUID for ShelfTaskPK", response.Content.ReadAsStringAsync().Result);
				}

				using (var response = controller.DoesReviewExistWithCapability(JObject.Parse($"{{\"ShelfTaskPK\": \"{testHelper.ShelfTask.PK}\", \"ReviewTaskCapability\": \"\"}} ")))
				{
					AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
					AssertContains("Unable to find Capability, ReviewTaskCapability:", response.Content.ReadAsStringAsync().Result);
				}
			}
		}

		#endregion

		#endregion

		#region EnsureReviewExistsWithAllSkills

		public void TestEnsureReviewExistsWithAllSkills_CreatesNewReview_WhenClosedReviewExistsWithCapability()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.AddWorkflow(null, null,
				$"CH0,{coder.GS_Code},ASN,100",
				$"CDU,{coder.GS_Code},CLS,20",
				$"CDF,{coder.GS_Code},CLS,40",
				$"SH0,{coder.GS_Code},CLS,60",
				$"CBC,{reviewer.GS_Code},CLS,80");

			Factory.Save();

			var aspectPK = Guid.NewGuid();
			assessServiceClientMock
				.SetupHasCompletedLearningUnit(coder, aspectPK, false)
				.SetupHasCompletedLearningUnit(reviewer, aspectPK, false)
				.SetupHasCompletedLearningUnit(capableReviewer, aspectPK, true)
				.SetupGetLearningUnitName(aspectPK, "123")
				.SetupGetLearningUnitUrl(aspectPK, "https://nothing/assess/123")
				.SetupEnsureEnrolled(reviewer, aspectPK);

			testHelper.RefreshWorkItemTasks();
			testHelper.ShelfTask.P9_Description = "ShelfName";
			Factory.Save();
			using (var controller = GetWorkItemController())
			using (var response = controller.EnsureAppropriateReviewExists(GetJsonRequestForEnsureAppropriateReviewExists(testHelper.ShelfTask.PK, new[] { aspectPK })))
			{
				AssertEquals(HttpStatusCode.OK, response.StatusCode);
				AssertAppropriateReviewExists(response, reviewExists: false);
			}

			testHelper.RefreshWorkItemTasks();
			testHelper.AssertWorkItemTasks(6);
			testHelper.AssertTaskExists(20, "CDU", "CLS");
			testHelper.AssertTaskExists(40, "CDF", "CLS");
			testHelper.AssertTaskExists(60, "SH0", "CLS");
			testHelper.AssertTaskExists(80, "CBC", "CLS");
			testHelper.AssertTaskExists(99, "CBC", "ASN");
			testHelper.AssertTaskExists(100, "CH0", "ASN");
			AssertAssessReviewHasLearningUnitInfo(testHelper.WorkItemTasks.Single(t => t.P9_Sequence == 99), aspectPK);
			AssertReviewAssignedMatch(testHelper.WorkItemTasks.Single(t => t.P9_Sequence == 99), testHelper.WorkItemTasks.Single(t => t.P9_Sequence == 80));

			var reviewTask = testHelper.WorkItemTasks.Single(t => t.P9_Sequence == 99);
			reviewTask.P9_GS_NKAssignedStaffMember = capableReviewer.GS_Code;
			reviewTask.P9_Status = "CLS";
			Factory.Save();

			using (var controller = GetWorkItemController())
			using (var response = controller.EnsureAppropriateReviewExists(GetJsonRequestForEnsureAppropriateReviewExists(testHelper.ShelfTask.PK, [aspectPK])))
			{
				AssertEquals(HttpStatusCode.OK, response.StatusCode);
				AssertAppropriateReviewExists(response);
			}
		}

		public void TestEnsureReviewExistsWithAllSkills_SingleSkill_ReviewWithSkill()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.AddWorkflow(null, null,
				$"CH0,{coder.GS_Code},ASN,100",
				$"CDU,{coder.GS_Code},CLS,20",
				$"CDF,{coder.GS_Code},CLS,40",
				$"SH0,{coder.GS_Code},CLS,60",
				$"CBC,{capableReviewer.GS_Code},CLS,80");
			Factory.Save();

			var aspectPK = Guid.NewGuid();
			assessServiceClientMock.SetupHasCompletedLearningUnit(capableReviewer, aspectPK, true);

			using (var controller = GetWorkItemController())
			using (var response = controller.EnsureAppropriateReviewExists(GetJsonRequestForEnsureAppropriateReviewExists(testHelper.ShelfTask.PK, [aspectPK])))
			{
				AssertEquals(HttpStatusCode.OK, response.StatusCode);
				AssertAppropriateReviewExists(response);
			}

			testHelper.RefreshWorkItemTasks();
			testHelper.AssertWorkItemTasks(5);
			testHelper.AssertTaskExists(20, "CDU", "CLS");
			testHelper.AssertTaskExists(40, "CDF", "CLS");
			testHelper.AssertTaskExists(60, "SH0", "CLS");
			testHelper.AssertTaskExists(80, "CBC", "CLS");
			testHelper.AssertTaskExists(100, "CH0", "ASN");
		}

		public void TestEnsureReviewExistsWithAllSkills_SingleSkill_ReviewWithSkillCancelled()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.AddWorkflow(null, null,
				$"CH0,{coder.GS_Code},ASN,100",
				$"CDU,{coder.GS_Code},CLS,20",
				$"CDF,{coder.GS_Code},CLS,40",
				$"SH0,{coder.GS_Code},CLS,60",
				$"CBC,{capableReviewer.GS_Code},CAN,80");
			Factory.Save();

			var aspectPK = Guid.NewGuid();
			assessServiceClientMock
				.SetupHasCompletedLearningUnit(capableReviewer, aspectPK, true)
				.SetupGetLearningUnitName(aspectPK, "1")
				.SetupGetLearningUnitUrl(aspectPK, "https://nothing/assess/123")
				.SetupEnsureEnrolled(capableReviewer, aspectPK);

			testHelper.RefreshWorkItemTasks();
			testHelper.ShelfTask.P9_Description = "ShelfName";
			Factory.Save();
			using (var controller = GetWorkItemController())
			using (var response = controller.EnsureAppropriateReviewExists(GetJsonRequestForEnsureAppropriateReviewExists(testHelper.ShelfTask.PK, [aspectPK])))
			{
				AssertEquals(HttpStatusCode.OK, response.StatusCode);
				AssertAppropriateReviewExists(response, reviewExists: false);
			}

			testHelper.RefreshWorkItemTasks();
			testHelper.AssertWorkItemTasks(6);
			testHelper.AssertTaskExists(20, "CDU", "CLS");
			testHelper.AssertTaskExists(40, "CDF", "CLS");
			testHelper.AssertTaskExists(60, "SH0", "CLS");
			testHelper.AssertTaskExists(80, "CBC", "CAN");
			testHelper.AssertTaskExists(99, "CBC", "ASN");
			testHelper.AssertTaskExists(100, "CH0", "ASN");
			AssertAssessReviewHasLearningUnitInfo(testHelper.WorkItemTasks.Single(t => t.P9_Sequence == 99), aspectPK);
			AssertReviewAssignedMatch(testHelper.WorkItemTasks.Single(t => t.P9_Sequence == 99), testHelper.WorkItemTasks.Single(t => t.P9_Sequence == 80));
		}

		public void TestEnsureReviewExistsWithAllSkills_SingleSkill_ReviewWithSkillCancelled_Replayed()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.AddWorkflow(null, null,
				$"CH0,{coder.GS_Code},ASN,100",
				$"CDU,{coder.GS_Code},CLS,20",
				$"CDF,{coder.GS_Code},CLS,40",
				$"SH0,{coder.GS_Code},CLS,60",
				$"CBC,{capableReviewer.GS_Code},CAN,80");
			Factory.Save();

			var aspectPK = Guid.NewGuid();
			assessServiceClientMock
				.SetupHasCompletedLearningUnit(capableReviewer, aspectPK, true)
				.SetupGetLearningUnitName(aspectPK, "1")
				.SetupGetLearningUnitUrl(aspectPK, "https://nothing/assess/123")
				.SetupEnsureEnrolled(capableReviewer, aspectPK);

			testHelper.RefreshWorkItemTasks();
			testHelper.ShelfTask.P9_Description = "ShelfName";
			Factory.Save();
			using (var controller = GetWorkItemController())
			using (var response = controller.EnsureAppropriateReviewExists(GetJsonRequestForEnsureAppropriateReviewExists(testHelper.ShelfTask.PK, [aspectPK])))
			{
				AssertEquals(HttpStatusCode.OK, response.StatusCode);
				AssertAppropriateReviewExists(response, reviewExists: false);
			}

			using (var controller = GetWorkItemController())
			using (var response = controller.EnsureAppropriateReviewExists(GetJsonRequestForEnsureAppropriateReviewExists(testHelper.ShelfTask.PK, [aspectPK])))
			{
				AssertEquals(HttpStatusCode.OK, response.StatusCode);
				AssertAppropriateReviewExists(response, reviewExists: false);
			}

			testHelper.RefreshWorkItemTasks();
			testHelper.AssertWorkItemTasks(6);
			testHelper.AssertTaskExists(20, "CDU", "CLS");
			testHelper.AssertTaskExists(40, "CDF", "CLS");
			testHelper.AssertTaskExists(60, "SH0", "CLS");
			testHelper.AssertTaskExists(80, "CBC", "CAN");
			testHelper.AssertTaskExists(99, "CBC", "ASN");
			testHelper.AssertTaskExists(100, "CH0", "ASN");
			AssertAssessReviewHasLearningUnitInfo(testHelper.WorkItemTasks.Single(t => t.P9_Sequence == 99), aspectPK);
			AssertReviewAssignedMatch(testHelper.WorkItemTasks.Single(t => t.P9_Sequence == 99), testHelper.WorkItemTasks.Single(t => t.P9_Sequence == 80));
		}

		public void TestEnsureReviewExistsWithAllSkills_SingleSkill_ShelfInQIChildReviewInParent()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.SetUpForQualityIterations(ZBool.False);

			testHelper.AddWorkflow("YES", null,
				$"CH0,{coder.GS_Code},ASN,100",
				$"CDU,{coder.GS_Code},CLS,20",
				$"CDF,{coder.GS_Code},CLS,40",
				$"SH0,{coder.GS_Code},CLS,60",
				$"CBC,{capableReviewer.GS_Code},CLS,80");

			Factory.Save();

			var aspectPK = Guid.NewGuid();
			assessServiceClientMock.SetupHasCompletedLearningUnit(capableReviewer, aspectPK, true);

			testHelper.AssertWorkItemTasks(5);

			testHelper.CreateQualityIteration();

			testHelper.RefreshWorkItemTasks();
			testHelper.AssertWorkItemTasks(9);
			testHelper.AssertTaskExists(20, "CDU", "CLS");
			testHelper.AssertTaskExists(40, "CDF", "CLS");
			testHelper.AssertTaskExists(60, "SH0", "CLS");
			testHelper.AssertTaskExists(80, "CBC", "CLS");
			testHelper.AssertTaskExists(100, "CH0", "CAN");
			testHelper.AssertTaskExists(101, "CDF", "ASN");
			testHelper.AssertTaskExists(102, "SHV", "ASN");
			testHelper.AssertTaskExists(103, "CBC", "ASN");
			testHelper.AssertTaskExists(104, "CHK", "ASN");

			//with an unrelated failure, the coder will cancel QI tasks and resubmit CH0 task
			testHelper.WorkItemTasks.Single(t => t.P9_Sequence == 101).P9_Status = "CAN";
			testHelper.WorkItemTasks.Single(t => t.P9_Sequence == 102).P9_Status = "CAN";
			testHelper.WorkItemTasks.Single(t => t.P9_Sequence == 103).P9_Status = "CAN";
			testHelper.WorkItemTasks.Single(t => t.P9_Sequence == 103).P9_GS_NKAssignedStaffMember = reviewer.GS_Code;
			var ch0TaskInChildQI = testHelper.WorkItemTasks.Single(t => t.P9_Sequence == 104);
			ch0TaskInChildQI.P9_Type = "CH0";

			Factory.Save();
			using (var controller = GetWorkItemController())
			using (var response = controller.EnsureAppropriateReviewExists(GetJsonRequestForEnsureAppropriateReviewExists(ch0TaskInChildQI.PK, [aspectPK])))
			{
				AssertEquals(HttpStatusCode.OK, response.StatusCode);
				AssertAppropriateReviewExists(response);
			}
		}

		public void TestEnsureReviewExistsWithAllSkills_SingleSkill_IgnoresAspectReview()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.AddWorkflow(null, null,
				$"CH0,{coder.GS_Code},ASN,100",
				$"CDU,{coder.GS_Code},CLS,20",
				$"CDF,{coder.GS_Code},CLS,40",
				$"SH0,{coder.GS_Code},CLS,60",
				$"CBC,{reviewer.GS_Code},CLS,70",
				$"CBS,{capableReviewer.GS_Code},CLS,80");
			Factory.Save();

			var aspectPK = Guid.NewGuid();
			assessServiceClientMock
				.SetupHasCompletedLearningUnit(capableReviewer, aspectPK, true)
				.SetupHasCompletedLearningUnit(reviewer, aspectPK, false)
				.SetupGetLearningUnitName(aspectPK, "1")
				.SetupGetLearningUnitUrl(aspectPK, "https://nothing/assess/123")
				.SetupEnsureEnrolled(reviewer, aspectPK);

			testHelper.RefreshWorkItemTasks();
			testHelper.AssertWorkItemTasks(6);
			testHelper.WorkItemTasks.Single(t => t.P9_Sequence == 80).P9_Description = "Aspect Review - Ignore Me (with CB)";
			testHelper.ShelfTask.P9_Description = "ShelfName";
			Factory.Save();
			using (var controller = GetWorkItemController())
			using (var response = controller.EnsureAppropriateReviewExists(GetJsonRequestForEnsureAppropriateReviewExists(testHelper.ShelfTask.PK, [aspectPK])))
			{
				AssertEquals(HttpStatusCode.OK, response.StatusCode);
				AssertAppropriateReviewExists(response, reviewExists: false);
			}

			testHelper.RefreshWorkItemTasks();
			testHelper.AssertWorkItemTasks(7);
			testHelper.AssertTaskExists(20, "CDU", "CLS");
			testHelper.AssertTaskExists(40, "CDF", "CLS");
			testHelper.AssertTaskExists(60, "SH0", "CLS");
			testHelper.AssertTaskExists(70, "CBC", "CLS");
			testHelper.AssertTaskExists(80, "CBS", "CLS");
			testHelper.AssertTaskExists(99, "CBC", "ASN");
			testHelper.AssertTaskExists(100, "CH0", "ASN");
			AssertAssessReviewHasLearningUnitInfo(testHelper.WorkItemTasks.Single(t => t.P9_Sequence == 99), aspectPK);
			AssertReviewAssignedMatch(testHelper.WorkItemTasks.Single(t => t.P9_Sequence == 99), testHelper.WorkItemTasks.Single(t => t.P9_Sequence == 70));
		}

		public void TestEnsureReviewExistsWithAllSkills_MultipleSkills_NoReview()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.AddWorkflow(null, null,
				$"CH0,{coder.GS_Code},ASN,20",
				$"CDF,{coder.GS_Code},CLS,10");
			Factory.Save();

			var aspectPK1 = Guid.NewGuid();
			var aspectPK2 = Guid.NewGuid();
			assessServiceClientMock
				.SetupGetLearningUnitName(aspectPK1, "1")
				.SetupGetLearningUnitName(aspectPK2, "2")
				.SetupGetLearningUnitUrl(aspectPK1, "https://nothing/assess/1")
				.SetupGetLearningUnitUrl(aspectPK2, "https://nothing/assess/2")
				.SetupHasCompletedLearningUnit(capableReviewer, aspectPK1, false)
				.SetupHasCompletedLearningUnit(capableReviewer, aspectPK2, true);

			using (var controller = GetWorkItemController())
			using (var response = controller.EnsureAppropriateReviewExists(GetJsonRequestForEnsureAppropriateReviewExists(testHelper.ShelfTask.PK, [aspectPK1, aspectPK2])))
			{
				AssertEquals(HttpStatusCode.OK, response.StatusCode);
				AssertAppropriateReviewExists(response, reviewExists: false);
			}

			testHelper.RefreshWorkItemTasks();
			testHelper.AssertWorkItemTasks(2);
			testHelper.AssertTaskExists(10, "CDF", "CLS");
			testHelper.AssertTaskExists(20, "CH0", "ASN");
		}

		public void TestEnsureReviewExistsWithAllSkills_MultipleSkills_ReviewerHasOneSkill()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.AddWorkflow(null, null,
				$"CH0,{coder.GS_Code},ASN,100",
				$"CDU,{coder.GS_Code},CLS,20",
				$"CDF,{coder.GS_Code},CLS,40",
				$"SH0,{coder.GS_Code},CLS,60",
				$"CBC,{capableReviewer.GS_Code},CLS,80");
			Factory.Save();

			var aspectPK1 = Guid.NewGuid();
			var aspectPK2 = Guid.NewGuid();
			assessServiceClientMock
				.SetupHasCompletedLearningUnit(capableReviewer, aspectPK1, true)
				.SetupHasCompletedLearningUnit(capableReviewer, aspectPK2, false)
				.SetupGetLearningUnitName(aspectPK1, "1")
				.SetupGetLearningUnitName(aspectPK2, "2")
				.SetupGetLearningUnitUrl(aspectPK1, "https://nothing/assess/1")
				.SetupGetLearningUnitUrl(aspectPK2, "https://nothing/assess/2")
				.SetupEnsureEnrolled(capableReviewer, aspectPK2);

			testHelper.RefreshWorkItemTasks();
			testHelper.ShelfTask.P9_Description = "ShelfName";
			Factory.Save();
			using (var controller = GetWorkItemController())
			using (var response = controller.EnsureAppropriateReviewExists(GetJsonRequestForEnsureAppropriateReviewExists(testHelper.ShelfTask.PK, [aspectPK1, aspectPK2])))
			{
				AssertEquals(HttpStatusCode.OK, response.StatusCode);
				AssertAppropriateReviewExists(response, reviewExists: false);
			}

			testHelper.RefreshWorkItemTasks();
			testHelper.AssertWorkItemTasks(6);
			testHelper.AssertTaskExists(20, "CDU", "CLS");
			testHelper.AssertTaskExists(40, "CDF", "CLS");
			testHelper.AssertTaskExists(60, "SH0", "CLS");
			testHelper.AssertTaskExists(80, "CBC", "CLS");
			testHelper.AssertTaskExists(99, "CBC", "ASN");
			testHelper.AssertTaskExists(100, "CH0", "ASN");
			AssertAssessReviewHasLearningUnitInfo(testHelper.WorkItemTasks.Single(t => t.P9_Sequence == 99), aspectPK1, hasCompletedLearningUnit: true);
			AssertAssessReviewHasLearningUnitInfo(testHelper.WorkItemTasks.Single(t => t.P9_Sequence == 99), aspectPK2);
			AssertReviewAssignedMatch(testHelper.WorkItemTasks.Single(t => t.P9_Sequence == 99), testHelper.WorkItemTasks.Single(t => t.P9_Sequence == 80));
		}

		public void TestEnsureReviewExistsWithAllSkills_MultipleSkills_SkillsSpreadAcrossManyReviewers()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.AddWorkflow(null, null,
				$"CH0,{coder.GS_Code},ASN,100",
				$"CDU,{coder.GS_Code},CLS,20",
				$"CDF,{coder.GS_Code},CLS,40",
				$"SH0,{coder.GS_Code},CLS,60",
				$"CBC,{reviewer.GS_Code},CLS,70",
				$"CBC,{capableReviewer.GS_Code},CLS,80");
			Factory.Save();

			var aspectPK1 = Guid.NewGuid();
			var aspectPK2 = Guid.NewGuid();
			assessServiceClientMock
				.SetupHasCompletedLearningUnit(capableReviewer, aspectPK1, true)
				.SetupHasCompletedLearningUnit(capableReviewer, aspectPK2, false)
				.SetupHasCompletedLearningUnit(reviewer, aspectPK1, false)
				.SetupHasCompletedLearningUnit(reviewer, aspectPK2, true);

			using (var controller = GetWorkItemController())
			using (var response = controller.EnsureAppropriateReviewExists(GetJsonRequestForEnsureAppropriateReviewExists(testHelper.ShelfTask.PK, [aspectPK1, aspectPK2])))
			{
				AssertEquals(HttpStatusCode.OK, response.StatusCode);
				AssertAppropriateReviewExists(response);
			}

			testHelper.RefreshWorkItemTasks();
			testHelper.AssertWorkItemTasks(6);
			testHelper.AssertTaskExists(20, "CDU", "CLS");
			testHelper.AssertTaskExists(40, "CDF", "CLS");
			testHelper.AssertTaskExists(60, "SH0", "CLS");
			testHelper.AssertTaskExists(70, "CBC", "CLS");
			testHelper.AssertTaskExists(80, "CBC", "CLS");
			testHelper.AssertTaskExists(100, "CH0", "ASN");
		}

		public void TestEnsureReviewExistsWithAllSkills_MultipleSkills_ReviewerHasExactSkills()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.AddWorkflow(null, null,
				$"CH0,{coder.GS_Code},ASN,100",
				$"CDU,{coder.GS_Code},CLS,20",
				$"CDF,{coder.GS_Code},CLS,40",
				$"SH0,{coder.GS_Code},CLS,60",
				$"CBC,{capableReviewer.GS_Code},CLS,80");
			Factory.Save();

			var aspectPK1 = Guid.NewGuid();
			var aspectPK2 = Guid.NewGuid();
			assessServiceClientMock
				.SetupHasCompletedLearningUnit(capableReviewer, aspectPK1, true)
				.SetupHasCompletedLearningUnit(capableReviewer, aspectPK2, true);

			using (var controller = GetWorkItemController())
			using (var response = controller.EnsureAppropriateReviewExists(GetJsonRequestForEnsureAppropriateReviewExists(testHelper.ShelfTask.PK, [aspectPK1, aspectPK2])))
			{
				AssertEquals(HttpStatusCode.OK, response.StatusCode);
				AssertAppropriateReviewExists(response);
			}

			testHelper.RefreshWorkItemTasks();
			testHelper.AssertWorkItemTasks(5);
			testHelper.AssertTaskExists(20, "CDU", "CLS");
			testHelper.AssertTaskExists(40, "CDF", "CLS");
			testHelper.AssertTaskExists(60, "SH0", "CLS");
			testHelper.AssertTaskExists(80, "CBC", "CLS");
			testHelper.AssertTaskExists(100, "CH0", "ASN");
		}

		public void TestEnsureReviewExistsWithAllSkills_ReviewInSeparateWorkflow_WithPrerequisiteRelationships()
		{
			EnsureReviewExistsWithAllSkills_ReviewInSeperateWorkflow(shouldSetPrerequisites: true, true);
		}

		public void TestEnsureReviewExistsWithAllSkills_ReviewInSeparateWorkflow_NoPrerequisiteRelationships()
		{
			EnsureReviewExistsWithAllSkills_ReviewInSeperateWorkflow(shouldSetPrerequisites: false, false);
		}

		public void EnsureReviewExistsWithAllSkills_ReviewInSeperateWorkflow(bool shouldSetPrerequisites, bool reviewExists)
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			var design = testHelper.AddWorkflow(null, null,
				null,
				$"DDB,{coder.GS_Code},CLS,10",
				$"CDB,{reviewer.GS_Code},CLS,20",
				$"CNK,{coder.GS_Code},CLS,30");

			var coding = testHelper.AddWorkflow(null, null,
				null,
				$"CDU,{coder.GS_Code},CLS,110",
				$"CDF,{coder.GS_Code},CLS,120",
				$"SH0,{coder.GS_Code},CLS,130");

			var review = testHelper.AddWorkflow(null, null,
				null,
				$"CBC,{capableReviewer.GS_Code},CLS,210");

			var publish = testHelper.AddWorkflow(null, null,
				$"CH0,{coder.GS_Code},ASN,310"); //this is the chekin task sent to the controller

			if (shouldSetPrerequisites)
			{
				design.GetOrCreateDependencyLink(coding);
				coding.GetOrCreateDependencyLink(review);
				review.GetOrCreateDependencyLink(publish);
			}

			Factory.Save();

			var aspectPK = Guid.NewGuid();
			assessServiceClientMock
				.SetupHasCompletedLearningUnit(capableReviewer, aspectPK, true)
				.SetupGetLearningUnitUrl(aspectPK, "https://nothing/assess/123")
				.SetupGetLearningUnitName(aspectPK, "1");

			using (var controller = GetWorkItemController())
			using (var response = controller.EnsureAppropriateReviewExists(GetJsonRequestForEnsureAppropriateReviewExists(testHelper.ShelfTask.PK, [aspectPK])))
			{
				AssertEquals(HttpStatusCode.OK, response.StatusCode);
				AssertAppropriateReviewExists(response, reviewExists);
			}

			testHelper.RefreshWorkItemTasks();
			testHelper.AssertWorkItemTasks(8);
		}

		#region EnsureReviewExistsWithAllSkills_Errors

		public void TestEnsureReviewExistsWithAllSkills_NoRequestShouldReturnForbidden()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.AddWorkflow(null, null,
				$"CH0,{coder.GS_Code},ASN,1",
				$"CDF,{coder.GS_Code},CLS,1");
			using (var controller = new WorkItemController())
			using (var response = controller.EnsureAppropriateReviewExists(GetJsonRequestForEnsureAppropriateReviewExists(testHelper.ShelfTask.PK, [Guid.NewGuid()])))
			{
				AssertEquals(HttpStatusCode.Forbidden, response.StatusCode);
			}
		}

		public void TestEnsureReviewExistsWithAllSkills_ShelfTaskNotFoundShouldGiveBadRequest()
		{
			var taskPK = ZGuid.NewZGuid();

			using (var controller = GetWorkItemController())
			using (var response = controller.EnsureAppropriateReviewExists(GetJsonRequestForEnsureAppropriateReviewExists(taskPK, [])))
			{
				AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
				AssertContains("Shelf Task not found with PK " + taskPK, response.Content.ReadAsStringAsync().Result);
			}
		}

		public void TestEnsureReviewExistsWithAllSkills_BadJSONRequestShouldGiveBadRequest()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.AddWorkflow(null, null,
				$"SH0,{coder.GS_Code},ASN,1",
				$"CDF,{coder.GS_Code},CLS,1");

			Factory.Save();

			using (var controller = GetWorkItemController())
			{
				using (var response = controller.EnsureAppropriateReviewExists(JObject.Parse("{}")))
				{
					AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
					AssertContains("Shelf Task not found with PK 00000000-0000-0000-0000-000000000000", response.Content.ReadAsStringAsync().Result);
				}

				using (var response = controller.EnsureAppropriateReviewExists(JObject.Parse($"{{\"TaskPK\": \"Harrharr\"}} ")))
				{
					AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
					AssertContains("Unable to parse JSON request", response.Content.ReadAsStringAsync().Result);
				}
			}
		}

		#endregion EnsureReviewExistsWithAllSkills_Errors

		#endregion EnsureReviewExistsWithAllSkills

		#region EnsureAppropriateReviewExists

		public void TestEnsureAppropriateReviewExists_RequiresWtaSubject()
		{
			var workItem = SetupPaveEnabledWorkItem();
			var codingTask = MasterFilesTestHelper.CreateTask(workItem, coder.GS_Code, taskType: "CDF", status: ProcessTaskStatusCodeList.Codes.Closed, sequence: 10);
			var codeReviewTask = MasterFilesTestHelper.CreateTask(workItem, capableReviewer.GS_Code, taskType: "CBC", status: ProcessTaskStatusCodeList.Codes.Closed, sequence: 20);
			var checkinTask = MasterFilesTestHelper.CreateTask(workItem, coder.GS_Code, taskType: "CHK", status: ProcessTaskStatusCodeList.Codes.Assigned, sequence: 30);

			Factory.Save();

			AssertNotEquals(ZGuid.Empty, capableReviewer.GS_PER);

			var aspectPK1 = Guid.NewGuid();
			var aspectPK2 = Guid.NewGuid();

			assessServiceClientMock
				.SetupHasCompletedLearningUnit(capableReviewer, aspectPK1, true)
				.SetupHasCompletedLearningUnit(capableReviewer, aspectPK2, false)
				.SetupGetLearningUnitName(aspectPK1, "Outer Wilds")
				.SetupGetLearningUnitName(aspectPK2, "Echoes of the Eye")
				.SetupGetLearningUnitUrl(aspectPK2, "https://nothing/assess/EchoesOfTheEye")
				.SetupEnsureEnrolled(capableReviewer, aspectPK2);

			using (var controller = GetWorkItemController())
			using (var response = controller.EnsureAppropriateReviewExists(GetJsonRequestForEnsureAppropriateReviewExists(checkinTask.PK, [aspectPK1, aspectPK2])))
			{
				AssertEquals(HttpStatusCode.OK, response.StatusCode);
				var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				var result = JsonConvert.DeserializeObject<EnsureAppropriateReviewExistsResult>(json);
				AssertEquals(false, result.AppropriateReviewExists);
				AssertSequencesEqual(Array.Empty<string>(), result.MissingCompetencies);
				AssertSequencesEqual(new[] { "Echoes of the Eye" }, result.MissingWiseTechAcademySubjects);
			}

			var assessReviewTask = workItem.WorkflowItems.Tasks.Cast<ProcessTask>().SingleOrDefault(t => t.P9_Description == "ASSESS Review");
			AssertNotNull(assessReviewTask);
			AssertMultilineASCIIEquals(
@"In order to proceed with the check-in, the submission needs to be reviewed by someone who has completed the following ASSESS courses:
Echoes of the Eye [https://nothing/assess/EchoesOfTheEye]
Outer Wilds [complete]", assessReviewTask.P9_NotesAsString);

			assessServiceClientMock.VerifyAll();

			AssertEquals(2, assessReviewTask.SkillsPivots.Count);
			var pivotDetails = assessReviewTask.SkillsPivots.Cast<ProcessTaskRequiredSkill>().Where(p => !p.P9S_Aspect.IsEmpty).Select(p => p.P9S_Aspect.ToGuid()).OrderBy(g => g);
			AssertSequencesEqual(new[] { aspectPK1, aspectPK2 }.OrderBy(g => g), pivotDetails);
		}

		public void TestEnsureAppropriateReviewExists_RequiresWtaSubjects_ShouldIncludeMissingCourseUrls()
		{
			var workItem = SetupPaveEnabledWorkItem();
			var codingTask = MasterFilesTestHelper.CreateTask(workItem, coder.GS_Code, taskType: "CDF", status: ProcessTaskStatusCodeList.Codes.Closed, sequence: 10);
			var codeReviewTask = MasterFilesTestHelper.CreateTask(workItem, capableReviewer.GS_Code, taskType: "CBC", status: ProcessTaskStatusCodeList.Codes.Closed, sequence: 20);
			var checkinTask = MasterFilesTestHelper.CreateTask(workItem, coder.GS_Code, taskType: "CHK", status: ProcessTaskStatusCodeList.Codes.Assigned, sequence: 30);

			Factory.Save();

			AssertNotEquals(ZGuid.Empty, capableReviewer.GS_PER);

			var aspectPK1 = Guid.NewGuid();
			var aspectPK2 = Guid.NewGuid();
			var aspectPK3 = Guid.NewGuid();

			assessServiceClientMock
				.SetupHasCompletedLearningUnit(capableReviewer, aspectPK1, true)
				.SetupHasCompletedLearningUnit(capableReviewer, aspectPK2, false)
				.SetupHasCompletedLearningUnit(capableReviewer, aspectPK3, false)
				.SetupGetLearningUnitName(aspectPK1, "Myst")
				.SetupGetLearningUnitName(aspectPK2, "Riven")
				.SetupGetLearningUnitName(aspectPK3, "Exile")
				.SetupGetLearningUnitUrl(aspectPK2, "https://nothing/assess/Riven")
				.SetupGetLearningUnitUrl(aspectPK3, "https://nothing/assess/Exile")
				.SetupEnsureEnrolled(capableReviewer, aspectPK2)
				.SetupEnsureEnrolled(capableReviewer, aspectPK3);

			using (var controller = GetWorkItemController())
			using (var response = controller.EnsureAppropriateReviewExists(GetJsonRequestForEnsureAppropriateReviewExists(checkinTask.PK, [aspectPK1, aspectPK2, aspectPK3])))
			{
				AssertEquals(HttpStatusCode.OK, response.StatusCode);
				var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				var result = JsonConvert.DeserializeObject<EnsureAppropriateReviewExistsResult>(json);
				AssertEquals(false, result.AppropriateReviewExists);
				AssertSequencesEqual(Array.Empty<string>(), result.MissingCompetencies);
				AssertSequencesEqual(new[] { "Exile", "Riven" }, result.MissingWiseTechAcademySubjects);
				AssertSequencesEqual(new[] { "https://nothing/assess/Exile", "https://nothing/assess/Riven" }, result.MissingWiseTechAcademyCourseUrls);
			}
		}

		public void TestEnsureAppropriateReviewExists_RequiresWtaSubject_WithMissingCourseName()
		{
			var workItem = SetupPaveEnabledWorkItem();
			var codingTask = MasterFilesTestHelper.CreateTask(workItem, coder.GS_Code, taskType: "CDF", status: ProcessTaskStatusCodeList.Codes.Closed, sequence: 10);
			var codeReviewTask = MasterFilesTestHelper.CreateTask(workItem, capableReviewer.GS_Code, taskType: "CBC", status: ProcessTaskStatusCodeList.Codes.Closed, sequence: 20);
			var checkinTask = MasterFilesTestHelper.CreateTask(workItem, coder.GS_Code, taskType: "CHK", status: ProcessTaskStatusCodeList.Codes.Assigned, sequence: 30);

			Factory.Save();

			AssertNotEquals(ZGuid.Empty, capableReviewer.GS_PER);

			var aspectPK1 = Guid.NewGuid();
			var aspectPK2 = Guid.NewGuid();

			assessServiceClientMock
				.SetupHasCompletedLearningUnit(capableReviewer, aspectPK1, true)
				.SetupHasCompletedLearningUnit(capableReviewer, aspectPK2, false)
				.SetupGetLearningUnitName(aspectPK1, "Outer Wilds")
				.SetupGetLearningUnitName(aspectPK2, ServiceResponse<string>.Failure(HttpStatusCode.NotFound, "Boop"))
				.SetupGetLearningUnitUrl(aspectPK2, "https://nothing/assess/Unknown")
				.SetupEnsureEnrolled(capableReviewer, aspectPK2);

			using (var controller = GetWorkItemController())
			using (var response = controller.EnsureAppropriateReviewExists(GetJsonRequestForEnsureAppropriateReviewExists(checkinTask.PK, [aspectPK1, aspectPK2])))
			{
				AssertEquals(HttpStatusCode.OK, response.StatusCode);
				var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				var result = JsonConvert.DeserializeObject<EnsureAppropriateReviewExistsResult>(json);
				AssertEquals(false, result.AppropriateReviewExists);
				AssertSequencesEqual(Array.Empty<string>(), result.MissingCompetencies);
				AssertSequencesEqual(new[] { "Unknown course" }, result.MissingWiseTechAcademySubjects);
			}

			var assessReviewTask = workItem.WorkflowItems.Tasks.Cast<ProcessTask>().SingleOrDefault(t => t.P9_Description == "ASSESS Review");
			AssertNotNull(assessReviewTask);
			AssertMultilineASCIIEquals(
@"In order to proceed with the check-in, the submission needs to be reviewed by someone who has completed the following ASSESS courses:
Unknown course [https://nothing/assess/Unknown]
Outer Wilds [complete]", assessReviewTask.P9_NotesAsString);

			assessServiceClientMock.VerifyAll();
		}

		public void TestEnsureAppropriateReviewExists_RequiresWtaSubject_WithUnsuccessfulResponseToHasPassedCourse()
		{
			var workItem = SetupPaveEnabledWorkItem();
			var codingTask = MasterFilesTestHelper.CreateTask(workItem, coder.GS_Code, taskType: "CDF", status: ProcessTaskStatusCodeList.Codes.Closed, sequence: 10);
			var codeReviewTask = MasterFilesTestHelper.CreateTask(workItem, capableReviewer.GS_Code, taskType: "CBC", status: ProcessTaskStatusCodeList.Codes.Closed, sequence: 20);
			var checkinTask = MasterFilesTestHelper.CreateTask(workItem, coder.GS_Code, taskType: "CHK", status: ProcessTaskStatusCodeList.Codes.Assigned, sequence: 30);

			Factory.Save();

			AssertNotEquals(ZGuid.Empty, capableReviewer.GS_PER);

			var aspectPK1 = Guid.NewGuid();
			var aspectPK2 = Guid.NewGuid();

			assessServiceClientMock
				.SetupHasCompletedLearningUnit(capableReviewer, aspectPK1, ServiceResponse<bool>.Failure(HttpStatusCode.NotFound, "Service error"))
				.SetupHasCompletedLearningUnit(capableReviewer, aspectPK2, false)
				.SetupGetLearningUnitName(aspectPK2, ServiceResponse<string>.Failure(HttpStatusCode.NotFound, "Boop"))
				.SetupGetLearningUnitUrl(aspectPK2, "https://nothing/assess/Unknown")
				.SetupEnsureEnrolled(capableReviewer, aspectPK2);

			using (var controller = GetWorkItemController())
			using (var response = controller.EnsureAppropriateReviewExists(GetJsonRequestForEnsureAppropriateReviewExists(checkinTask.PK, [aspectPK1, aspectPK2])))
			{
				AssertEquals(HttpStatusCode.OK, response.StatusCode);
				var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				var result = JsonConvert.DeserializeObject<EnsureAppropriateReviewExistsResult>(json);
				AssertEquals(false, result.AppropriateReviewExists);
				AssertSequencesEqual(Array.Empty<string>(), result.MissingCompetencies);
				AssertSequencesEqual(new[] { "Unknown course" }, result.MissingWiseTechAcademySubjects);
			}

			var assessReviewTask = workItem.WorkflowItems.Tasks.Cast<ProcessTask>().SingleOrDefault(t => t.P9_Description == "ASSESS Review");
			AssertNotNull(assessReviewTask);
			AssertMultilineASCIIEquals(
@"In order to proceed with the check-in, the submission needs to be reviewed by someone who has completed the following ASSESS course:
Unknown course [https://nothing/assess/Unknown]", assessReviewTask.P9_NotesAsString);

			assessServiceClientMock.VerifyAll();
		}

		public void TestEnsureAppropriateReviewExists_RequiresWtaSubject_WhenServiceReturnsError()
		{
			var workItem = SetupPaveEnabledWorkItem();
			var codingTask = MasterFilesTestHelper.CreateTask(workItem, coder.GS_Code, taskType: "CDF", status: ProcessTaskStatusCodeList.Codes.Closed, sequence: 10);
			var codeReviewTask = MasterFilesTestHelper.CreateTask(workItem, capableReviewer.GS_Code, taskType: "CBC", status: ProcessTaskStatusCodeList.Codes.Closed, sequence: 20);
			var checkinTask = MasterFilesTestHelper.CreateTask(workItem, coder.GS_Code, taskType: "CHK", status: ProcessTaskStatusCodeList.Codes.Assigned, sequence: 30);

			Factory.Save();

			AssertNotEquals(ZGuid.Empty, capableReviewer.GS_PER);

			var aspectPK1 = Guid.NewGuid();
			var aspectPK2 = Guid.NewGuid();

			assessServiceClientMock
				.SetupHasCompletedLearningUnit(capableReviewer, aspectPK1, true)
				.SetupHasCompletedLearningUnit(capableReviewer, aspectPK2, false)
				.SetupGetLearningUnitName(aspectPK1, "Outer Wilds")
				.SetupGetLearningUnitName(aspectPK2, "Echoes of the Eye")
				.SetupGetLearningUnitUrl(aspectPK2, "https://nothing/assess/EchoesOfTheEye")
				.SetupEnsureEnrolled(capableReviewer, aspectPK2, ServiceResponse.Failure(HttpStatusCode.Unauthorized, "You shall not pass!"));

			using (var controller = GetWorkItemController())
			using (var response = controller.EnsureAppropriateReviewExists(GetJsonRequestForEnsureAppropriateReviewExists(checkinTask.PK, [aspectPK1, aspectPK2])))
			{
				AssertEquals(HttpStatusCode.OK, response.StatusCode);
				var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				var result = JsonConvert.DeserializeObject<EnsureAppropriateReviewExistsResult>(json);
				AssertEquals(false, result.AppropriateReviewExists);
				AssertSequencesEqual(Array.Empty<string>(), result.MissingCompetencies);
				AssertSequencesEqual(new[] { "Echoes of the Eye" }, result.MissingWiseTechAcademySubjects);
			}

			var assessReviewTask = workItem.WorkflowItems.Tasks.Cast<ProcessTask>().SingleOrDefault(t => t.P9_Description == "ASSESS Review");
			AssertNotNull(assessReviewTask);
			AssertMultilineASCIIEquals(
@"In order to proceed with the check-in, the submission needs to be reviewed by someone who has completed the following ASSESS courses:
Echoes of the Eye [https://nothing/assess/EchoesOfTheEye]
Outer Wilds [complete]", assessReviewTask.P9_NotesAsString);

			AssertNullOrEmpty(ErrorReporter.LastKeyReported);
			AssertNullOrEmpty(ErrorReporter.LastMessageReported);

			assessServiceClientMock.VerifyAll();
		}

		public void TestEnsureAppropriateReviewExists_RequiresWtaSubjects_WhenTwoReviewersHaveEachCompletedOneSubject()
		{
			var secondReviewer = Factory.NewWithValidTestData<GlbStaff>();
			var workItem = SetupPaveEnabledWorkItem();
			var codingTask = MasterFilesTestHelper.CreateTask(workItem, coder.GS_Code, taskType: "CDF", status: ProcessTaskStatusCodeList.Codes.Closed, sequence: 10);
			var codeReviewTask1 = MasterFilesTestHelper.CreateTask(workItem, capableReviewer.GS_Code, taskType: "CBC", status: ProcessTaskStatusCodeList.Codes.Closed, sequence: 20);
			var codeReviewTask2 = MasterFilesTestHelper.CreateTask(workItem, secondReviewer.GS_Code, taskType: "CBC", status: ProcessTaskStatusCodeList.Codes.Closed, sequence: 20);
			var checkinTask = MasterFilesTestHelper.CreateTask(workItem, coder.GS_Code, taskType: "CHK", status: ProcessTaskStatusCodeList.Codes.Assigned, sequence: 30);

			Factory.Save();

			var aspectPK1 = Guid.NewGuid();
			var aspectPK2 = Guid.NewGuid();

			assessServiceClientMock
				.SetupHasCompletedLearningUnit(capableReviewer, aspectPK1, true)
				.SetupHasCompletedLearningUnit(capableReviewer, aspectPK2, false)
				.SetupHasCompletedLearningUnit(secondReviewer, aspectPK1, false)
				.SetupHasCompletedLearningUnit(secondReviewer, aspectPK2, true);

			using (var controller = GetWorkItemController())
			using (var response = controller.EnsureAppropriateReviewExists(GetJsonRequestForEnsureAppropriateReviewExists(checkinTask.PK, [aspectPK1, aspectPK2])))
			{
				AssertEquals(HttpStatusCode.OK, response.StatusCode);
				var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				var result = JsonConvert.DeserializeObject<EnsureAppropriateReviewExistsResult>(json);
				AssertEquals(true, result.AppropriateReviewExists);
				AssertSequencesEqual(Array.Empty<string>(), result.MissingCompetencies);
				AssertSequencesEqual(Array.Empty<string>(), result.MissingWiseTechAcademySubjects);
			}

			var assessReviewTask = workItem.WorkflowItems.Tasks.Cast<ProcessTask>().SingleOrDefault(t => t.P9_Description == "ASSESS Review");
			AssertNull(assessReviewTask);
		}

		public void TestEnsureAppropriateReviewExists_NoWtaRequirements()
		{
			var workItem = SetupPaveEnabledWorkItem();
			var codingTask = MasterFilesTestHelper.CreateTask(workItem, coder.GS_Code, taskType: "CDF", status: ProcessTaskStatusCodeList.Codes.Closed, sequence: 10);
			var codeReviewTask = MasterFilesTestHelper.CreateTask(workItem, capableReviewer.GS_Code, taskType: "CBC", status: ProcessTaskStatusCodeList.Codes.Closed, sequence: 20);
			var checkinTask = MasterFilesTestHelper.CreateTask(workItem, coder.GS_Code, taskType: "CHK", status: ProcessTaskStatusCodeList.Codes.Assigned, sequence: 30);

			Factory.Save();

			AssertNotEquals(ZGuid.Empty, capableReviewer.GS_PER);

			using (var controller = GetWorkItemController())
			using (var response = controller.EnsureAppropriateReviewExists(GetJsonRequestForEnsureAppropriateReviewExists(checkinTask.PK, [])))
			{
				AssertEquals(HttpStatusCode.OK, response.StatusCode);
				var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				var result = JsonConvert.DeserializeObject<EnsureAppropriateReviewExistsResult>(json);
				AssertEquals(true, result.AppropriateReviewExists);
				AssertSequencesEqual(Array.Empty<string>(), result.MissingCompetencies);
				AssertSequencesEqual(Array.Empty<string>(), result.MissingWiseTechAcademySubjects);
			}

			var assessReviewTask = workItem.WorkflowItems.Tasks.Cast<ProcessTask>().SingleOrDefault(t => t.P9_Description == "ASSESS Review");
			AssertNull(assessReviewTask);
		}

		public void TestEnsureAppropriateReviewExistsWhenNullAspectPKs()
		{
			var workItem = SetupPaveEnabledWorkItem();
			var codingTask = MasterFilesTestHelper.CreateTask(workItem, coder.GS_Code, taskType: "CDF", status: ProcessTaskStatusCodeList.Codes.Closed, sequence: 10);
			var codeReviewTask = MasterFilesTestHelper.CreateTask(workItem, capableReviewer.GS_Code, taskType: "CBC", status: ProcessTaskStatusCodeList.Codes.Closed, sequence: 20);
			var checkinTask = MasterFilesTestHelper.CreateTask(workItem, coder.GS_Code, taskType: "CHK", status: ProcessTaskStatusCodeList.Codes.Assigned, sequence: 30);

			Factory.Save();

			AssertNotEquals(ZGuid.Empty, capableReviewer.GS_PER);

			var request = new EnsureAppropriateReviewExistsRequest
			{
				TaskPK = checkinTask.PK.ToGuid(),
				AspectPKs = null,
			};

			var jObject = GetJsonRequest(JsonConvert.SerializeObject(request));
			using (var controller = GetWorkItemController())
			using (var response = controller.EnsureAppropriateReviewExists(jObject))
			{
				AssertEquals(HttpStatusCode.OK, response.StatusCode);
				var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				var result = JsonConvert.DeserializeObject<EnsureAppropriateReviewExistsResult>(json);
				AssertEquals(true, result.AppropriateReviewExists);
				AssertSequencesEqual(Array.Empty<string>(), result.MissingCompetencies);
				AssertSequencesEqual(Array.Empty<string>(), result.MissingWiseTechAcademySubjects);
				AssertSequencesEqual(Array.Empty<string>(), result.MissingWiseTechAcademyCourseUrls);
			}
		}

		NewWorkItem SetupPaveEnabledWorkItem()
		{
			var bmTestHelper = ObjectFactory.Get<IBMTestHelper>();
			bmTestHelper.EnableBMSInRegistry();
			bmTestHelper.CreateSystem(Factory, WorkflowDescriptors.WorkItemWorkflowDescriptorCode);

			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var jobHeader = ProcessJobHeaderProvider.GetForParent(workItem, Factory);

			return workItem;
		}

		#endregion

		class LearningCenterServiceForTest : LearningCenterService
		{
			protected override void HandleError(string errorMessage, string errorDetail = "")
			{
				LastErrorMessage = errorMessage;
				LastErrorDetail = errorDetail;
			}

			public string LastErrorMessage { get; private set; }

			public string LastErrorDetail { get; private set; }

			protected override void ValidateRequestIpAddress()
			{
			}
		}

		#region ProductDetails

		public void TestGetProductDetails()
		{
			var workItem1 = Factory.New<NewWorkItem>();
			var workItem2 = Factory.New<NewWorkItem>();
			var workItem3 = Factory.New<NewWorkItem>();

			workItem1.WKI_WorkItemType = "ENT";
			workItem2.WKI_WorkItemType = "GLW";
			workItem3.WKI_WorkItemType = "BOR";

			workItem1.WKI_WorkItemArea = "PAV";
			workItem2.WKI_WorkItemArea = "GLW";
			workItem3.WKI_WorkItemArea = "PLT";

			workItem1.WKI_ActivityType = "BUF";
			workItem2.WKI_ActivityType = "HTM";
			workItem3.WKI_ActivityType = "WEB";

			Factory.Save();

			using (var controller = new WorkItemController { Request = new HttpRequestMessage() })
			{
				var response = controller.GetProductDetails(new[] { workItem1.PK.ToGuid(), workItem2.PK.ToGuid(), Guid.NewGuid() }).ToArray();

				AssertContainsExactElementsInAnyOrder(new[] { new ProductDetails("ENT", "PAV", "BUF"), new ProductDetails("GLW", "GLW", "HTM") }, response);
			}
		}

		public void TestGetProductDetailsForNullParameterShouldNotThrowException()
		{
			ProductDetails[] response = null;

			AssertNoExceptionThrown(() =>
			{
				using (var controller = new WorkItemController { Request = new HttpRequestMessage() })
				{
					response = controller.GetProductDetails(null).ToArray();
				}
			});

			Assert(response.IsNullOrEmpty());
		}

		#endregion

		#region Helpers

		void AssertReviewExistsWithCapability(Guid submissionTaskPK, bool expectedValue)
		{
			var jsonRequest = GetJsonRequestForDoesReviewExistWithCapability(submissionTaskPK);
			using (var controller = GetWorkItemController())
			using (var response = controller.DoesReviewExistWithCapability(jsonRequest))
			{
				if (!response.IsSuccessStatusCode)
				{
					Fail($"{response.ReasonPhrase}: {response.Content?.ReadAsStringAsync()?.GetAwaiter().GetResult()}");
				}
				AssertEquals(expectedValue, ((ObjectContent<bool>)response.Content).Value);
			}
		}

		void AssertAssessReviewHasLearningUnitInfo(WorkItemProcessTask review, Guid aspectPK, bool hasCompletedLearningUnit = false)
		{
			AssertCollectionContains(FormattableString.Invariant($"Aspect [{aspectPK}] should be in review SkillsPivots"), new ZGuid(aspectPK), review.SkillsPivots.Select(p => p.P9S_Aspect));
			AssertEquals("ASSESS Review", review.P9_Description);
			AssertContains("In order to proceed with the check-in, the submission needs to be reviewed by someone who has completed the following ASSESS course", review.P9_NotesAsString);

			var humanReadableDescription = WorkItemTaskHelper.GetHumanReadableText(new CompetencyRequirement(aspectPK), assessServiceClientMock.Object);
			if (hasCompletedLearningUnit)
			{
				humanReadableDescription += " [complete]";
			}
			AssertContains(humanReadableDescription, review.P9_NotesAsString);
		}

		void AssertReviewAssignedMatch(WorkItemProcessTask review1, WorkItemProcessTask review2)
		{
			AssertEquals("Assigned user for reviews should match", review1.P9_GS_NKAssignedStaffMember, review2.P9_GS_NKAssignedStaffMember);
			AssertEquals("Assigned capability for reviews should match", review1.P9_G4_RequiredCapability, review2.P9_G4_RequiredCapability);
		}

		static void AssertAppropriateReviewExists(HttpResponseMessage response, bool reviewExists = true)
		{
			var responseText = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
			var responseObject = JsonConvert.DeserializeObject<EnsureAppropriateReviewExistsResult>(responseText);
			AssertEquals(reviewExists, responseObject.AppropriateReviewExists);
		}

		GlbStaff CreateStaff(string staffCode, string loginName, string fullName, string email = null)
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = staffCode;
			staff.GS_LoginName = loginName;
			staff.GS_FullName = fullName;
			staff.GS_EmailAddress = email;
			return staff;
		}

		protected override void SetUp()
		{
			base.SetUp();

			capability = Factory.New<GlbCapability>();
			capability.G4_Code = reviewCapability;

			reviewer = CreateStaff("RV", "vi.ewer", "Vi Ewer");
			coder = CreateStaff("COD", "co.der", "Co Der");
			capableReviewer = CreateStaff("CRV", "re.viewer", "Re Viewer", "re@viewer.com");
			capableReviewer.Capabilities.Add(capability);

			assessServiceClientMock = new();
			ObjectFactory.Substitute(assessServiceClientMock.Object);
		}

		Mock<IAssessServiceClient> assessServiceClientMock;

		WorkItemController GetWorkItemController()
		{
			var request = new HttpRequestMessage(HttpMethod.Post, "https://myaccount-portal.cargowise.com/myaccount/api/WorkItem/Mocked");
			request.Properties.Add("MS_HttpConfiguration", new HttpConfiguration());
			return new WorkItemController(assessServiceClientMock.Object)
			{
				Request = request
			};
		}

		JObject GetJsonRequest(ProcessedShelfsServiceTaskTestHelper testHelper, string reviewName = "AspectName")
		{
			string json = $"{{\"ShelfTaskPK\": \"{testHelper.ShelfTask.PK}\", \"ReviewTaskCapability\": \"{capability.G4_Code}\", \"ReviewName\": \"{reviewName}\", \"TaskNotes\": \"Task Notes\"}} ";
			return GetJsonRequest(json);
		}

		JObject GetJsonRequestForDoesReviewExistWithCapability(ProcessedShelfsServiceTaskTestHelper testHelper)
		{
			return GetJsonRequestForDoesReviewExistWithCapability(testHelper.ShelfTask.PK.ToGuid());
		}

		JObject GetJsonRequestForDoesReviewExistWithCapability(Guid taskPK)
		{
			var json = $"{{\"ShelfTaskPK\": \"{taskPK}\", \"ReviewTaskCapability\": \"{capability.G4_Code}\"}} ";
			return GetJsonRequest(json);
		}

		JObject GetJsonRequestForEnsureAppropriateReviewExists(ZGuid checkinTaskPK, Guid[] aspectPKs = null)
		{
			var request = new EnsureAppropriateReviewExistsRequest
			{
				TaskPK = checkinTaskPK.ToGuid(),
				RequiredSkills = [],
				WTASubjectCodes = [],
				AspectPKs = aspectPKs ?? [],
			};

			var json = JsonConvert.SerializeObject(request);
			return GetJsonRequest(json);
		}

		JObject GetJsonRequest(string rawJson)
		{
			return (JObject)JsonConvert.DeserializeObject(rawJson);
		}

		GlbCapability capability;
		GlbStaff coder;
		GlbStaff reviewer;
		GlbStaff capableReviewer;

		const string reviewTaskType = WorkItemProcessTask.AspectReviewTaskType;
		const string alternateReviewTaskType = "CBC";
		const string reviewCapability = "RVR";

		#endregion
	}
}
