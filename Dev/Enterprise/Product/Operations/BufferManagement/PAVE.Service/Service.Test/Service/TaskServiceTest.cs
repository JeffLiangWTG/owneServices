using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.PAVE.Common.DTO;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.BufferManagement.Service.Helpers;
using Enterprise.BufferManagement.Service.Shared;
using Enterprise.BufferManagement.Service.Shared.Task.Dtos;
using Enterprise.BufferManagement.Service.Shared.Task.Dtos.Lifecycle;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json;
using NUnit.Framework;
using WTG.RtfConverter;

namespace Enterprise.BufferManagement.Service.Test
{
	public class TaskServiceTest : TestCaseWithFactory
	{
		#region Setup and Helpers

		internal static void AssertContainmentBarrierTask(ProcessTask task, ContainmentBarrierTaskPreviewDTO dto)
		{
			AssertEquals("PK", task.PK.ToGuid(), dto.PK);
			AssertEquals("WorkflowDescription", task.ProcessHeader?.FH_CompletionStatement, dto.WorkflowDescription);
			AssertEquals("Description", task.P9_Description, dto.Description);
			AssertEquals("Sequence", task.P9_Sequence, dto.Sequence);
			AssertEquals("ResourceName", task.StaffName, dto.ResourceName);
		}

		TaskServiceForTest Service;
		bool IsUserInteractive;

		class TaskServiceForTest : TaskService
		{
			public BusinessObjectFactory FactoryForTest { get; set; }

			internal override BusinessObjectFactory CreateFactory() => FactoryForTest ?? base.CreateFactory();
		}

		protected override void SetUp()
		{
			base.SetUp();
			BMSTestHelper.EnableBMSInRegistry();
			IsUserInteractive = Globals.IsUserInteractive;
			Globals.IsUserInteractive = false;
			TestConfigsHelper.CreateSchematicTestConfig(Factory);
			Service = new TaskServiceForTest();
		}

		protected override void TearDown()
		{
			Globals.IsUserInteractive = IsUserInteractive;
			MasterFilesTestHelper.ClearIterationReasonsFromRegistry("ORG");
			base.TearDown();
		}

		ProcessTask CreateWorkflowAndTask(string staffCode = "")
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow");
			var task = BMSTestHelper.CreateTask(workflow, description: "Task", staffCode: staffCode);

			return task;
		}

		ProcessTask ReloadTask(ZGuid taskPK) => new BusinessObjectFactory().Load<ProcessTask>(taskPK);

		EnableAddEditAndDeleteLogsItemCollection GetEnableAddEditAndDeleteLogsItemsRegistryItemValue(bool value)
		{
			var tempValues = new EnableAddEditAndDeleteLogsItemCollection();
			tempValues.Add(new EnableAddEditAndDeleteLogsItem()
			{
				Table = ProcessTasksSchema.Constants.TableName,
				EnableADDLogs = value,
				EnableEDTLogs = value,
				EnableDELLogs = value,
			});
			return tempValues;
		}

		#endregion

		#region TaskStatus

		public void TestTaskStatusLookups_CodesMatchWithPaveCommon_TaskStatusEnum()
		{
			var pairListCodes = typeof(ProcessTaskStatusCodeList.Codes).GetAllPublicConstantValues();
			var taskStatusEnumMemberValues = BMSTestHelper.GetEnumMemberAttributeValues<TaskStatus>();

			AssertContainsExactElementsInAnyOrder(pairListCodes, taskStatusEnumMemberValues);
		}

		public void TestPaveCommonContainmentBarrierResponse_ShouldHaveSameValuesOfWorkflowIntegrationContainmentBarrierResponses()
		{
			var workflowIntegrationContainmentBarrierResponsesValues = Enum.GetNames(typeof(ContainmentBarrierResponses)).ToList();
			workflowIntegrationContainmentBarrierResponsesValues.Remove(Enum.GetName(typeof(ContainmentBarrierResponses), ContainmentBarrierResponses.None));

			var paveCommonContainmentBarrierResponseValues = Enum.GetNames(typeof(ContainmentBarrierResponse)).ToList();

			AssertContainsExactElementsInAnyOrder("CargoWise.PAVE.Common.Interfaces.ContainmentBarrierResponse Should have all values of Enterprise.Workflow.Integration.ContainmentBarrierResponses except none.",
				workflowIntegrationContainmentBarrierResponsesValues,
				paveCommonContainmentBarrierResponseValues);
		}

		#endregion

		#region TryChangeType

		public void TestUpdateType()
		{
			var updateRequest = new UpdateTaskTypeRequest()
			{
				NewType = "A",
				PreviousType = null
			};

			AssertExceptionThrown<ArgumentException>(() => Service.TryUpdateType(Guid.NewGuid(), updateRequest, out var _));

			updateRequest.NewType = null;
			updateRequest.PreviousType = "B";

			AssertExceptionThrown<ArgumentException>(() => Service.TryUpdateType(Guid.NewGuid(), updateRequest, out var _));

			updateRequest.NewType = "A";
			updateRequest.PreviousType = "B";

			var result = Service.TryUpdateType(Guid.NewGuid(), updateRequest, out var businessResponse1);
			AssertEquals(false, result);
			AssertEquals(WiseTech.Business.WorkflowManagement.BusinessMessages.TaskNotFound.Text, businessResponse1.Message.Text);

			var task = CreateWorkflowAndTask();
			Factory.Save();

			result = Service.TryUpdateType(task.PK.ToGuid(), updateRequest, out var businessResponse2);
			AssertEquals(false, result);
			AssertEquals(WiseTech.Business.Core.BusinessMessages.UpdateAttemptWithOutdatedDataVersion.Text, businessResponse2.Message.Text);

			updateRequest.PreviousType = task.P9_Type = "UDF";
			Factory.Save();

			result = Service.TryUpdateType(task.PK.ToGuid(), updateRequest, out var businessResponse3);
			AssertEquals(false, result);
			AssertNotNull("Business response should not be null", businessResponse3);

			BMSTestHelper.AddTaskTypesToRegistry(WorkflowDescriptors.DummyWorkflowDescriptorCode, "BLA");
			updateRequest.NewType = "BLA";
			Factory.Save();

			result = Service.TryUpdateType(task.PK.ToGuid(), updateRequest, out var businessResponse4);
			AssertEquals(false, result);
			AssertNotNull("Business response should not be null", businessResponse4);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			task.P9_GS_NKAssignedStaffMember = staff.GS_Code = "STF";
			Factory.Save();

			result = Service.TryUpdateType(task.PK.ToGuid(), updateRequest, out var businessResponse5);
			Assert(result);
			AssertNull(businessResponse5);
			task = ReloadTask(task.PK);
			AssertEquals("BLA", task.P9_Type);
		}

		#endregion

		#region TaskChangeChannel

		public void TestTryChangeTaskChannel_ShouldReturnTaskNotFoundError_WhenNoTaskInDB()
		{
			//action
			var result = Service.TryChangeChannel(new ChangeTaskChannelRequestDTO()
			{
				TaskPK = Guid.NewGuid(),
				DestinationChannelEntityPK = Guid.NewGuid().ToString(),
				GetActions = false,
				Method = TaskChangeChannelMethod.SelectedTask
			}, false);

			//assert
			Assert("Should not return Success", !result.Success);
			AssertEquals("Task not found.", result.Error.Messages.FirstOrDefault());
		}

		public void TestTryChangeTaskChannel_WithMethod_SelectedTaskOnly()
		{
			//arrange
			var fromStaffTasksBeforeMove = 1;
			var toStaffTasksBeforeMove = 0;
			var toStaffTasksAfterMove = 0;
			var fromStaffTasksAfterMove = 0;
			var numberOfTasksThatShouldMove = 1;

			var system = BMSTestHelper.CreateSystem(Factory);
			var buffer = BMSTestHelper.CreateBuffer(system, "buffer");
			var board = system.Boards.AddNew();
			var boardPK = board.PK.ToGuid();
			var bufferSection = board.Sections.AddNew();

			var staffInFromChannel = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "AAA", "staffFrom fullName");
			var staffInToChannel = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "ABA", "staffTo fullName");
			staffInToChannel.GS_FriendlyName = "staffTo friendlyName";

			var taskToMove = CreateWorkflowAndTask(staffInFromChannel.GS_Code.ToString());
			taskToMove.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			bufferSection.MS_FC_Component = buffer.PK;

			var fromChannel = BMSTestHelper.CreatePrimaryChannelForSection(bufferSection, ChannelTypeList.Codes.Resource, staffInFromChannel.PK);
			var toChannel = BMSTestHelper.CreatePrimaryChannelForSection(bufferSection, ChannelTypeList.Codes.Resource, staffInToChannel.PK);

			Factory.Save();

			//action
			var result = Service.TryChangeChannel(new ChangeTaskChannelRequestDTO()
			{
				TaskPK = taskToMove.PK.ToGuid(),
				DestinationChannelEntityPK = toChannel.EntityPK.ToString(),
				GetActions = false,
				Method = TaskChangeChannelMethod.SelectedTask
			}, false);

			taskToMove.Reload();

			var workflow = (ProcessHeader)taskToMove.ProcessHeader;

			toStaffTasksAfterMove = workflow.Tasks.Count(x => x.P9_GS_NKAssignedStaffMember == staffInToChannel.GS_Code);
			fromStaffTasksAfterMove = workflow.Tasks.Count(x => x.P9_GS_NKAssignedStaffMember == staffInFromChannel.GS_Code);

			//assert
			CombineAssertions(() =>
			{
				Assert("Should return Success", result.Success);
				AssertNull("Should return no validationErrors", result.Error);
				AssertEquals("Task should be assigned to ABA", staffInToChannel.GS_Code, taskToMove.P9_GS_NKAssignedStaffMember);
				AssertEquals($"{numberOfTasksThatShouldMove} should have been removed from source", fromStaffTasksBeforeMove - numberOfTasksThatShouldMove, fromStaffTasksAfterMove);
				AssertEquals($"{numberOfTasksThatShouldMove} should have been moved to destination", toStaffTasksBeforeMove + numberOfTasksThatShouldMove, toStaffTasksAfterMove);
			});
		}

		public void TestTryChangeTaskChannel_WithMethod_TasksAssignedToGroup()
		{
			//arrange
			var fromStaffTasksAfterMove = 0;
			var toStaffTasksBeforeMove = 0;
			var toStaffTasksAfterMove = 3;
			var numberOfTasksThatShouldMove = 3;

			var system = BMSTestHelper.CreateSystem(Factory);
			var buffer = BMSTestHelper.CreateBuffer(system, "buffer");
			var board = system.Boards.AddNew();
			var boardPK = board.PK.ToGuid();
			var bufferSection = board.Sections.AddNew();
			var group1 = BMSTestHelper.CreateGroup(Factory, "gr1");
			var group2 = BMSTestHelper.CreateGroup(Factory, "gr2");

			var staffFromChannel = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "AAA", "staffFrom fullName");
			var staffToChannel = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "ABA", "staffTo fullName");
			staffToChannel.GS_FriendlyName = "staffTo friendlyName";

			var wflow = BMSTestHelper.CreateWorkflow(Factory, "completion statement");

			var task1 = VisualBoardsTestHelper.CreateTask(wflow, staffCode: "", taskGroupPK: group1.PK);
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			var task2 = VisualBoardsTestHelper.CreateTask(wflow, staffCode: "", taskGroupPK: group2.PK);
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			var task3 = VisualBoardsTestHelper.CreateTask(wflow, staffCode: "", taskGroupPK: group2.PK);
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			var taskToMove = VisualBoardsTestHelper.CreateTask(wflow, staffCode: staffFromChannel.GS_Code, taskGroupPK: group2.PK);
			taskToMove.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			bufferSection.MS_FC_Component = buffer.PK;

			var fromChannel = BMSTestHelper.CreatePrimaryChannelForSection(bufferSection, ChannelTypeList.Codes.Resource, staffFromChannel.PK);
			var toChannel = BMSTestHelper.CreatePrimaryChannelForSection(bufferSection, ChannelTypeList.Codes.Resource, staffToChannel.PK);

			Factory.Save();

			//action
			var result = Service.TryChangeChannel(new ChangeTaskChannelRequestDTO()
			{
				TaskPK = taskToMove.PK.ToGuid(),
				DestinationChannelEntityPK = toChannel.EntityPK.ToString(),
				GetActions = false,
				Method = TaskChangeChannelMethod.TasksAssignedToGroup
			}, false);

			taskToMove = ReloadTask(taskToMove.PK);
			var workflow = (ProcessHeader)taskToMove.ProcessHeader;

			toStaffTasksAfterMove = workflow.Tasks.Count(x => x.P9_GS_NKAssignedStaffMember == staffToChannel.GS_Code);
			fromStaffTasksAfterMove = workflow.Tasks.Count(x => x.P9_GS_NKAssignedStaffMember == staffFromChannel.GS_Code);

			//assert
			CombineAssertions(() =>
			{
				Assert("Should return Success", result.Success);
				AssertNull("Should return no validationErrors", result.Error);
				AssertEquals("Task should be assigned to ST1", staffToChannel.GS_Code, taskToMove.P9_GS_NKAssignedStaffMember);
				AssertEquals($"{numberOfTasksThatShouldMove} should have been moved to destination", toStaffTasksBeforeMove + numberOfTasksThatShouldMove, toStaffTasksAfterMove);
			});
		}

		public void TestTryChangeTaskChannel_WithMethod_EntireWorkFlow()
		{
			//arrange
			var fromStaffTasksBeforeMove = 2;
			var fromStaffTasksAfterMove = 0;
			var toStaffTasksBeforeMove = 0;
			var toStaffTasksAfterMove = 2;
			var numberOfTasksThatShouldMove = 2;

			var system = BMSTestHelper.CreateSystem(Factory);
			var buffer = BMSTestHelper.CreateBuffer(system, "buffer");
			var board = system.Boards.AddNew();
			var boardPK = board.PK.ToGuid();
			var bufferSection = board.Sections.AddNew();

			var staffFromChannel = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "AAA", "staffFrom fullName");
			var staffToChannel = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "ABA", "staffTo fullName");
			staffToChannel.GS_FriendlyName = "staffTo friendlyName";

			var taskInWorkFlow = CreateWorkflowAndTask(staffFromChannel.GS_Code.ToString());
			taskInWorkFlow.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			var taskToMove = VisualBoardsTestHelper.CreateTask((ProcessHeader)taskInWorkFlow.ProcessHeader, staffCode: "");
			taskToMove.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			bufferSection.MS_FC_Component = buffer.PK;

			var fromChannel = BMSTestHelper.CreatePrimaryChannelForSection(bufferSection, ChannelTypeList.Codes.Resource, staffFromChannel.PK);
			var toChannel = BMSTestHelper.CreatePrimaryChannelForSection(bufferSection, ChannelTypeList.Codes.Resource, staffToChannel.PK);

			Factory.Save();

			//action
			var result = Service.TryChangeChannel(new ChangeTaskChannelRequestDTO()
			{
				TaskPK = taskToMove.PK.ToGuid(),
				DestinationChannelEntityPK = toChannel.EntityPK.ToString(),
				GetActions = false,
				Method = TaskChangeChannelMethod.AllTasksInWorkflow
			}, false);

			taskInWorkFlow.Reload();
			taskToMove.Reload();

			var workflow = (ProcessHeader)taskToMove.ProcessHeader;

			toStaffTasksAfterMove = workflow.Tasks.Count(x => x.P9_GS_NKAssignedStaffMember == staffToChannel.GS_Code);
			fromStaffTasksAfterMove = workflow.Tasks.Count(x => x.P9_GS_NKAssignedStaffMember == staffFromChannel.GS_Code);

			//assert
			CombineAssertions(() =>
			{
				Assert("Should return Success", result.Success);
				AssertNull("Should return no validationErrors", result.Error);
				AssertEquals("Task should have been moved ", staffToChannel.GS_Code, taskToMove.P9_GS_NKAssignedStaffMember);
				AssertEquals("Task should have been moved ", staffToChannel.GS_Code, taskInWorkFlow.P9_GS_NKAssignedStaffMember);
				AssertEquals($"{numberOfTasksThatShouldMove} should have been removed from source", fromStaffTasksBeforeMove - numberOfTasksThatShouldMove, fromStaffTasksAfterMove);
				AssertEquals($"{numberOfTasksThatShouldMove} should have been moved to destination", toStaffTasksBeforeMove + numberOfTasksThatShouldMove, toStaffTasksAfterMove);
			});
		}

		public void TestTryChangeTaskChannel_ShouldReturnOkWithNoChange_TasksAssignedToCapability()
		{
			//arrange
			var fromStaffTasksBeforeMove = 4;
			var fromStaffTasksAfterMove = 1;
			var toStaffTasksBeforeMove = 0;
			var toStaffTasksAfterMove = 3;
			var numberOfTasksThatShouldMove = 3;

			var system = BMSTestHelper.CreateSystem(Factory);
			var buffer = BMSTestHelper.CreateBuffer(system, "buffer");
			var board = system.Boards.AddNew();
			var boardPK = board.PK.ToGuid();
			var bufferSection = board.Sections.AddNew();
			var cap1 = BMSTestHelper.CreateCapability(Factory, "Ca1");
			var cap2 = BMSTestHelper.CreateCapability(Factory, "Ca2");

			var staffInFromChannel = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "AAA", "staffFrom fullName");
			var staffInToChannel = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "ABA", "staffTo fullName");
			staffInToChannel.GS_FriendlyName = "staffTo friendlyName";

			var task0 = CreateWorkflowAndTask(staffInFromChannel.GS_Code.ToString());
			task0.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			var taskToMove = VisualBoardsTestHelper.CreateTask((ProcessHeader)task0.ProcessHeader, capability: cap1, staffCode: staffInFromChannel.GS_Code.ToString());
			taskToMove.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			var task1 = VisualBoardsTestHelper.CreateTask((ProcessHeader)taskToMove.ProcessHeader, capability: cap1);
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			var task2 = VisualBoardsTestHelper.CreateTask((ProcessHeader)taskToMove.ProcessHeader, capability: cap1);
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			bufferSection.MS_FC_Component = buffer.PK;

			var fromChannel = BMSTestHelper.CreatePrimaryChannelForSection(bufferSection, ChannelTypeList.Codes.Resource, staffInFromChannel.PK);
			var toChannel = BMSTestHelper.CreatePrimaryChannelForSection(bufferSection, ChannelTypeList.Codes.Resource, staffInToChannel.PK);

			Factory.Save();

			//action
			var result = Service.TryChangeChannel(new ChangeTaskChannelRequestDTO()
			{
				TaskPK = taskToMove.PK.ToGuid(),
				DestinationChannelEntityPK = toChannel.EntityPK.ToString(),
				GetActions = false,
				Method = TaskChangeChannelMethod.TasksAssignedToCapability
			}, false);

			taskToMove = ReloadTask(taskToMove.PK);
			var workflow = (ProcessHeader)taskToMove.ProcessHeader;

			toStaffTasksAfterMove = workflow.Tasks.Count(x => x.P9_GS_NKAssignedStaffMember == staffInToChannel.GS_Code);
			fromStaffTasksAfterMove = workflow.Tasks.Count(x => x.P9_GS_NKAssignedStaffMember == staffInFromChannel.GS_Code);

			//assert
			CombineAssertions(() =>
			{
				Assert("Should return Success", result.Success);
				AssertNull("Should return no validationErrors", result.Error);
				AssertEquals("Task should have been moved", staffInToChannel.GS_Code, taskToMove.P9_GS_NKAssignedStaffMember);
				AssertEquals($"{numberOfTasksThatShouldMove} should have been removed from source", fromStaffTasksBeforeMove - numberOfTasksThatShouldMove, fromStaffTasksAfterMove);
				AssertEquals($"{numberOfTasksThatShouldMove} should have been moved to destination", toStaffTasksBeforeMove + numberOfTasksThatShouldMove, toStaffTasksAfterMove);
			});
		}

		#endregion

		#region TryUpdateTaskEstimates

		public void TestUpdateEstimates_ShouldThrownException_WhenHashNullOrEmpty()
		{
			var updateRequest = new UpdateTaskEstimatesRequest()
			{
				NewEstimateFactor = 2,
				NewLowEstimate = 120,
				PreviousHash = null
			};

			AssertExceptionThrown<ArgumentNullException>(() =>
			{
				var result = Service.TryUpdateEstimates(Guid.NewGuid(), updateRequest, out PaveError error);
			});
		}

		public void TestUpdateEstimates_ShouldThrownException_WhenEstimatesNull()
		{
			var updateRequest = new UpdateTaskEstimatesRequest()
			{
				NewEstimateFactor = null,
				NewLowEstimate = null,
				PreviousHash = Guid.NewGuid().ToString(),
			};

			AssertExceptionThrown<ArgumentNullException>(() =>
			{
				var result = Service.TryUpdateEstimates(Guid.NewGuid(), updateRequest, out PaveError error);
			});
		}

		public void TestUpdateEstimates_ShouldWork()
		{
			var task = Factory.New<ProcessTask>();

			var estimateFactor = 1;
			var lowEstimate = 60;

			task.P9_EstimateVariationFactor = estimateFactor;
			task.P9_EstDuration = new ZDateTime(2024, 1, 1, lowEstimate / 60, 0, 0);

			Factory.Save();

			var newEstimateFactor = 2;
			var newLowEstimate = 120;

			var updateRequest = new UpdateTaskEstimatesRequest()
			{
				NewEstimateFactor = newEstimateFactor,
				NewLowEstimate = newLowEstimate,
				PreviousHash = HashHelper.GetHash(JsonConvert.SerializeObject(new
				{
					LowEstimate = lowEstimate,
					EstimateFactor = estimateFactor,
				}))
			};

			var result = Service.TryUpdateEstimates(task.PK.ToGuid(), updateRequest, out PaveError error);

			AssertNull(error);
			AssertEquals(expected: true, result);

			task = ReloadTask(task.PK);

			AssertEquals((int)task.P9_EstimateVariationFactor, newEstimateFactor);
			AssertEquals((int)(task.P9_EstDuration - new ZDateTime(task.P9_EstDuration.Year, 1, 1)).TotalMinutes, newLowEstimate);
		}

		public void TestUpdateEstimates_ShouldReturnFalse_WhenPreviousHashDoesNotMatch()
		{
			var task = Factory.New<ProcessTask>();

			task.P9_EstimateVariationFactor = 1;
			task.P9_EstDuration = new ZDateTime(2024, 1, 1, 1, 0, 0);

			Factory.Save();

			var newEstimateFactor = 2;
			var newLowEstimate = 120;

			var updateRequest = new UpdateTaskEstimatesRequest()
			{
				NewEstimateFactor = newEstimateFactor,
				NewLowEstimate = newLowEstimate,
				PreviousHash = "wrong",
			};

			var result = Service.TryUpdateEstimates(task.PK.ToGuid(), updateRequest, out PaveError error);

			AssertEquals(expected: false, result);
			AssertNotNull(error);
		}

		public void TestUpdateEstimates_ShouldWork_WhenOnlyFactorChanged()
		{
			var task = Factory.New<ProcessTask>();

			var estimateFactor = 1;
			var lowEstimate = 60;

			task.P9_EstimateVariationFactor = estimateFactor;
			task.P9_EstDuration = new ZDateTime(2024, 1, 1, lowEstimate / 60, 0, 0);

			Factory.Save();

			var newEstimateFactor = 2;
			var newLowEstimate = 60;

			var updateRequest = new UpdateTaskEstimatesRequest()
			{
				NewEstimateFactor = newEstimateFactor,
				NewLowEstimate = newLowEstimate,
				PreviousHash = HashHelper.GetHash(JsonConvert.SerializeObject(new
				{
					LowEstimate = lowEstimate,
					EstimateFactor = estimateFactor,
				}))
			};

			var result = Service.TryUpdateEstimates(task.PK.ToGuid(), updateRequest, out PaveError error);

			AssertNull(error);
			AssertEquals(expected: true, result);

			task = ReloadTask(task.PK);

			AssertEquals((int)task.P9_EstimateVariationFactor, newEstimateFactor);
			AssertEquals((int)(task.P9_EstDuration - new ZDateTime(task.P9_EstDuration.Year, 1, 1)).TotalMinutes, newLowEstimate);
		}

		public void TestUpdateEstimates_ShouldWork_WhenOnlyLowEstimateChanged()
		{
			var task = Factory.New<ProcessTask>();

			var estimateFactor = 1;
			var lowEstimate = 60;

			task.P9_EstimateVariationFactor = estimateFactor;
			task.P9_EstDuration = new ZDateTime(2024, 1, 1, lowEstimate / 60, 0, 0);

			Factory.Save();

			var newEstimateFactor = 1;
			var newLowEstimate = 120;

			var updateRequest = new UpdateTaskEstimatesRequest()
			{
				NewEstimateFactor = newEstimateFactor,
				NewLowEstimate = newLowEstimate,
				PreviousHash = HashHelper.GetHash(JsonConvert.SerializeObject(new
				{
					LowEstimate = lowEstimate,
					EstimateFactor = estimateFactor,
				}))
			};

			var result = Service.TryUpdateEstimates(task.PK.ToGuid(), updateRequest, out PaveError error);

			AssertNull(error);
			AssertEquals(expected: true, result);

			task = ReloadTask(task.PK);

			AssertEquals((int)task.P9_EstimateVariationFactor, newEstimateFactor);
			AssertEquals((int)(task.P9_EstDuration - new ZDateTime(task.P9_EstDuration.Year, 1, 1)).TotalMinutes, newLowEstimate);
		}

		public void TestUpdateEstimates_ShouldThrownException_WhenFactorIsZero()
		{
			var updateRequest = new UpdateTaskEstimatesRequest()
			{
				NewEstimateFactor = 0,
				NewLowEstimate = 120,
				PreviousHash = Guid.NewGuid().ToString(),
			};

			var result = Service.TryUpdateEstimates(Guid.NewGuid(), updateRequest, out PaveError error);
			Assert("Should not return Success", !result);
			AssertEquals("Estimation factor must be a positive number.", error.Messages.FirstOrDefault());
		}

		public void TestUpdateEstimates_ShouldReturnValidationError_WhenFactorIsNegative()
		{
			var updateRequest = new UpdateTaskEstimatesRequest()
			{
				NewEstimateFactor = -1,
				NewLowEstimate = 120,
				PreviousHash = Guid.NewGuid().ToString(),
			};

			var result = Service.TryUpdateEstimates(Guid.NewGuid(), updateRequest, out PaveError error);

			Assert("Should not return Success", !result);
			AssertEquals("Estimation factor must be a positive number.", error.Messages.FirstOrDefault());
		}

		public void TestUpdateEstimates_ShouldReturnValidationError_WhenLowEstimateIsNegative()
		{
			var updateRequest = new UpdateTaskEstimatesRequest()
			{
				NewEstimateFactor = 2,
				NewLowEstimate = -60,
				PreviousHash = Guid.NewGuid().ToString(),
			};

			var result = Service.TryUpdateEstimates(Guid.NewGuid(), updateRequest, out PaveError error);

			Assert("Should not return Success", !result);
			AssertEquals("Low estimate must be between 0 and 999 hours.", error.Messages.FirstOrDefault());
		}

		public void TestUpdateEstimates_ShouldReturnValidationError_WhenLowEstimateIsLargerOrEqualToThousand()
		{
			var updateRequest = new UpdateTaskEstimatesRequest()
			{
				NewEstimateFactor = 2,
				NewLowEstimate = 1000 * 60,
				PreviousHash = Guid.NewGuid().ToString(),
			};

			var result = Service.TryUpdateEstimates(Guid.NewGuid(), updateRequest, out PaveError error);

			Assert("Should not return Success", !result);
			AssertEquals("Low estimate must be between 0 and 999 hours.", error.Messages.FirstOrDefault());
		}

		#endregion

		#region TryUpdateTaskActualDuration

		public void TestUpdateActualDuration_ShouldFail_WhenPreviousHashIsNull()
		{
			var updateDurationRequestWithNullDuration = new UpdateActualDurationRequest()
			{
				NewActualDuration = 60,
				PreviousHash = null,
			};

			var result = Service.TryUpdateActualDuration(new Guid(), updateDurationRequestWithNullDuration, out var businessResponse);

			Assert("TryUpdateActualDuration should fail", !result);
			AssertNotNull("Negative duration should cause an error to be returned", businessResponse);
			AssertEquals(TaskService.BusinessMessages.PreviousHashIsRequiredToUpdateActualDuration.Text, businessResponse.Message.Text);
		}

		public void TestUpdateActualDuration_ShouldFail_WhenActualDurationIsNull()
		{
			var updateDurationRequestWithNullDuration = new UpdateActualDurationRequest()
			{
				NewActualDuration = null,
				PreviousHash = Guid.NewGuid().ToString(),
			};

			var result = Service.TryUpdateActualDuration(new Guid(), updateDurationRequestWithNullDuration, out var businessResponse);

			Assert("TryUpdateActualDuration should fail", !result);
			AssertNotNull("Negative duration should cause an error to be returned", businessResponse);
			AssertEquals(TaskService.BusinessMessages.ActualDurationInMinutesMustBeGreaterThanOrEqualsToZero.Text, businessResponse.Message.Text);
		}

		[TestDate(2024, 12, 31)]
		public void TestUpdateActualDuration_ShouldWork()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ST1";
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			var task = CreateWorkflowAndTask(staff.GS_Code);

			var currentActualDuration = 60;

			task.P9_ActualDuration = new ZDateTime(2024, 1, 1, currentActualDuration / 60, 0, 0);
			Factory.Save();

			var newActualDuration = 120;

			var updateDurationRequest = new UpdateActualDurationRequest()
			{
				NewActualDuration = newActualDuration,
				PreviousHash = HashHelper.GetHash(currentActualDuration.ToString())
			};

			var result = Service.TryUpdateActualDuration(task.PK.ToGuid(), updateDurationRequest, out var businessResponse);

			Assert("TryUpdateActualDuration should succeed", result);
			AssertNull("No error should be returned when the update of actual duration is successful", businessResponse);

			task = ReloadTask(task.PK);
			AssertEquals(newActualDuration, (int)(task.P9_ActualDuration - ZDateTime.DefaultDurationEpoch).TotalMinutes);
		}

		public void TestUpdateActualDuration_ShouldReturnFalse_WhenPreviousHashDoesNotMatch()
		{
			var task = Factory.New<ProcessTask>();

			task.P9_ActualDuration = new ZDateTime(2024, 1, 1, 1, 0, 0);
			Factory.Save();

			var updateDurationRequest = new UpdateActualDurationRequest()
			{
				NewActualDuration = 120,
				PreviousHash = "test"
			};

			var result = Service.TryUpdateActualDuration(task.PK.ToGuid(), updateDurationRequest, out var businessResponse);

			Assert("TryUpdateActualDuration should fail", !result);
			AssertNotNull("Difference in hashes should cause an error to be returned", businessResponse);
		}

		public void TestUpdateActualDuration_ShouldReturnFalse_WhenNewActualDurationIsNegative()
		{
			var task = Factory.New<ProcessTask>();

			task.P9_ActualDuration = new ZDateTime(2024, 1, 1, 1, 0, 0);
			Factory.Save();

			var updateDurationRequest = new UpdateActualDurationRequest()
			{
				NewActualDuration = -60,
				PreviousHash = Guid.NewGuid().ToString(),
			};

			var result = Service.TryUpdateActualDuration(task.PK.ToGuid(), updateDurationRequest, out var businessResponse);

			Assert("TryUpdateActualDuration should fail", !result);
			AssertNotNull("Negative duration should cause an error to be returned", businessResponse);
			AssertEquals(TaskService.BusinessMessages.ActualDurationInMinutesMustBeGreaterThanOrEqualsToZero.Text, businessResponse.Message.Text);
		}

		#endregion

		#region AssignmentCapability

		public void TestTryAssignmentCapability_ShouldAssignCapability_WhenNoErrors()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ST1";
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			var task = CreateWorkflowAndTask(staff.GS_Code);
			Factory.Save();

			var result = Service.TryAssignToCapability(
				task.PK.ToGuid(),
				new AssignToCapabilityRequest { CapabilityId = capability.PK.ToGuid() },
				out var businessResponse);

			Assert("TryAssignmentCapability should succeed", result);
			AssertNull("No error should be returned when the update of actual duration is successful", businessResponse);

			task = ReloadTask(task.PK);

			AssertEquals("Should have changed capability assignment", capability.PK, task.P9_G4_RequiredCapability);
			AssertEquals("Should have assigned status", ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
		}

		[TestDateIncremental(seconds: 1)]
		public void TestTryAssignmentCapability_ShouldAssignCapability_WhenNoErrorsAndWRK()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ST1";
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			var task = CreateWorkflowAndTask(staff.GS_Code);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();

			task.Reload();

			var result = Service.TryAssignToCapability(
				task.PK.ToGuid(),
				new AssignToCapabilityRequest { CapabilityId = capability.PK.ToGuid() },
				out var businessResponse);

			Assert("TryAssignmentCapability should succeed", result);
			AssertNull("No error should be returned when the update of actual duration is successful", businessResponse);

			task = ReloadTask(task.PK);

			AssertEquals("Should have changed capability assignment", capability.PK, task.P9_G4_RequiredCapability);
			AssertEquals("Should keep working status", ProcessTaskStatusCodeList.Codes.Working, task.P9_Status);
		}

		public void TestTryAssignmentCapability_ShouldFail_WhenInvalidTask()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ST1";
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			var task = CreateWorkflowAndTask(staff.GS_Code);
			Factory.Save();

			var result = Service.TryAssignToCapability(
				Guid.NewGuid(),
				new AssignToCapabilityRequest { CapabilityId = capability.PK.ToGuid() },
				out var businessResponse);

			Assert("TryAssignmentCapability should fail", !result);
			AssertNotNull("Should return businessResponse", businessResponse);
			AssertEquals(WiseTech.Business.WorkflowManagement.BusinessMessages.TaskNotFound.Text, businessResponse.Message.Text);
		}

		public void TestTryAssignmentCapability_ShouldFail_WhenInvalidCapability()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ST1";
			var task = CreateWorkflowAndTask(staff.GS_Code);
			Factory.Save();

			var result = Service.TryAssignToCapability(
				task.PK.ToGuid(),
				new AssignToCapabilityRequest { CapabilityId = Guid.NewGuid() },
				out var businessResponse);

			task.Reload();

			Assert("TryAssignmentCapability should fail", !result);
			AssertNotNull("Should return businessResponse", businessResponse);
			AssertEquals(WiseTech.Business.HumanResourcesManagement.BusinessMessages.CapabilityNotFound.Text, businessResponse.Message.Text);
		}

		#endregion

		#region DeleteAssignment

		[TestDateIncremental(seconds: 1)]
		public void TestTryDeleteAssignmentStaff_ShouldDeleteStaffCode_WhenNoErrors()
		{
			var tempValue = GetEnableAddEditAndDeleteLogsItemsRegistryItemValue(true);

			using (SystemDataRegistry.Instance.EnableAddEditAndDeleteLogsItemsRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tempValue))
			{
				var staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_Code = "ST1";
				var task = CreateWorkflowAndTask(staff.GS_Code);
				Factory.Save();

				var dateTimeBeforeCall = ZDateTime.Now;
				var result = Service.TryDeleteAssignmentStaff(task.PK.ToGuid(), out PaveError error);

				task.Reload();

				var logs = task.GetLogs().Find(log => log.SL_EventTime > dateTimeBeforeCall);

				CombineAssertions(() =>
				{
					AssertEquals("Should return Success", expected: true, result);
					AssertNull("Should return no validationErrors", error);

					AssertEquals("Should have StartabilityChanged Log", 1, logs.Count(log => log.SL_SE_NKEvent == Events.StartabilityChanged.Code));
					AssertEquals("Should have EditedARecord log", 1, logs.Count(log => log.SL_SE_NKEvent == Events.EditedARecordCode));

					AssertEquals("Should have removed staff assignment", string.Empty, task.P9_GS_NKAssignedStaffMember);
					AssertEquals("Should have open status", ProcessTaskStatusCodeList.Codes.Open, task.P9_Status);
				});
			}
		}

		[TestDateIncremental(seconds: 1)]
		public void TestTryDeleteAssignmentStaff_ShouldDeleteStaffCode_WhenWorking()
		{
			var tempValue = GetEnableAddEditAndDeleteLogsItemsRegistryItemValue(true);

			using (SystemDataRegistry.Instance.EnableAddEditAndDeleteLogsItemsRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tempValue))
			{
				var staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_Code = "ST1";
				var task = CreateWorkflowAndTask(staff.GS_Code);
				task.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
				Factory.Save();

				var dateTimeBeforeCall = ZDateTime.Now;
				var result = Service.TryDeleteAssignmentStaff(task.PK.ToGuid(), out PaveError error);

				task.Reload();

				var logs = task.GetLogs().Find(log => log.SL_EventTime > dateTimeBeforeCall);

				CombineAssertions(() =>
				{
					AssertEquals("Should return Success", expected: true, result);
					AssertNull("Should return no validationErrors", error);

					AssertEquals("Should not have StartabilityChanged Log", 0, logs.Count(log => log.SL_SE_NKEvent == Events.StartabilityChanged.Code));
					AssertEquals("Should have EditedARecord log", 1, logs.Count(log => log.SL_SE_NKEvent == Events.EditedARecordCode));

					AssertEquals("Should have removed staff assignment", string.Empty, task.P9_GS_NKAssignedStaffMember);
					AssertEquals("Should have open status", ProcessTaskStatusCodeList.Codes.Open, task.P9_Status);
				});
			}
		}

		[TestDateIncremental(seconds: 1)]
		public void TestTryDeleteAssignmentStaff_ShouldWork_WhenNoStaffCode()
		{
			var tempValue = GetEnableAddEditAndDeleteLogsItemsRegistryItemValue(true);

			using (SystemDataRegistry.Instance.EnableAddEditAndDeleteLogsItemsRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tempValue))
			{
				var task = CreateWorkflowAndTask();
				task.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
				Factory.Save();

				var dateTimeBeforeCall = ZDateTime.Now;
				var result = Service.TryDeleteAssignmentStaff(task.PK.ToGuid(), out PaveError error);

				task.Reload();

				var logs = task.GetLogs().Find(log => log.SL_EventTime > dateTimeBeforeCall);

				CombineAssertions(() =>
				{
					AssertEquals("Should return Success", expected: true, result);
					AssertNull("Should return no validationErrors", error);

					AssertEquals("Should not have StartabilityChanged Log", 0, logs.Count(log => log.SL_SE_NKEvent == Events.StartabilityChanged.Code));
					AssertEquals("Should not have EditedARecord log", 0, logs.Count(log => log.SL_SE_NKEvent == Events.EditedARecordCode));

					AssertEquals("Should have no staff assignment", string.Empty, task.P9_GS_NKAssignedStaffMember);
				});
			}
		}

		[TestDateIncremental(seconds: 1)]
		public void TestTryDeleteAssignmentStaff_TaskBecomeOPN_WhenCapabilityAssigned()
		{
			var tempValue = GetEnableAddEditAndDeleteLogsItemsRegistryItemValue(true);

			using (SystemDataRegistry.Instance.EnableAddEditAndDeleteLogsItemsRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tempValue))
			{
				var capability = Factory.NewWithValidTestData<GlbCapability>();
				var staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_Code = "ST1";
				var task = CreateWorkflowAndTask(staff.GS_Code);
				task.P9_G4_RequiredCapability = capability.PK.ToGuid();
				Factory.Save();

				var dateTimeBeforeCall = ZDateTime.Now;
				var result = Service.TryDeleteAssignmentStaff(task.PK.ToGuid(), out PaveError error);

				task.Reload();

				var logs = task.GetLogs().Find(log => log.SL_EventTime > dateTimeBeforeCall);

				CombineAssertions(() =>
				{
					AssertEquals("Should return Success", expected: true, result);
					AssertNull("Should return no validationErrors", error);

					AssertEquals("Should have StartabilityChanged Log", 1, logs.Count(log => log.SL_SE_NKEvent == Events.StartabilityChanged.Code));
					AssertEquals("Should have EditedARecord log", 1, logs.Count(log => log.SL_SE_NKEvent == Events.EditedARecordCode));

					AssertEquals("Should have removed staff assignment", string.Empty, task.P9_GS_NKAssignedStaffMember);
					AssertEquals("Should have open status", ProcessTaskStatusCodeList.Codes.Open, task.P9_Status);
				});
			}
		}

		[TestDateIncremental(seconds: 1)]
		public void TestTryDeleteAssignmentStaff_ShouldReturnTaskNotFound_WhenInvalidTask()
		{
			var tempValue = GetEnableAddEditAndDeleteLogsItemsRegistryItemValue(true);

			using (SystemDataRegistry.Instance.EnableAddEditAndDeleteLogsItemsRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tempValue))
			{
				var staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_Code = "ST1";
				var task = CreateWorkflowAndTask(staff.GS_Code);
				Factory.Save();

				var dateTimeBeforeCall = ZDateTime.Now;
				var result = Service.TryDeleteAssignmentStaff(Guid.NewGuid(), out PaveError error);

				task.Reload();

				var logs = task.GetLogs().Find(log => log.SL_EventTime > dateTimeBeforeCall);

				CombineAssertions(() =>
				{
					AssertEquals("Should return not Success", expected: false, result);
					AssertEquals("Should return validationErrors", "Task not found.", error.Messages.Single());
					AssertEquals("Task Status should be Assigned", ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
					AssertEquals("Should not have StartabilityChanged Log", 0, logs.Count(log => log.SL_SE_NKEvent == Events.StartabilityChanged.Code));
					AssertEquals("Should not have EditedARecord log", 0, logs.Count(log => log.SL_SE_NKEvent == Events.EditedARecordCode));
				});
			}
		}

		[TestDateIncremental(seconds: 1)]
		public void TestTryDeleteAssignmentCapability_ShouldDeleteCapability_WhenNoErrors()
		{
			var tempValue = GetEnableAddEditAndDeleteLogsItemsRegistryItemValue(true);

			using (SystemDataRegistry.Instance.EnableAddEditAndDeleteLogsItemsRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tempValue))
			{
				var capability = Factory.NewWithValidTestData<GlbCapability>();
				var task = CreateWorkflowAndTask();
				task.P9_G4_RequiredCapability = capability.PK;
				Factory.Save();

				var dateTimeBeforeCall = ZDateTime.Now;
				var result = Service.TryDeleteAssignmentCapability(task.PK.ToGuid(), out PaveError error);

				task.Reload();

				var logs = task.GetLogs().Find(log => log.SL_EventTime > dateTimeBeforeCall);

				CombineAssertions(() =>
				{
					AssertEquals("Should return Success", expected: true, result);
					AssertNull("Should return no validationErrors", error);

					AssertEquals("Should StartabilityChanged Log", 1, logs.Count(log => log.SL_SE_NKEvent == Events.StartabilityChanged.Code));
					AssertEquals("Should have EditedARecord log", 1, logs.Count(log => log.SL_SE_NKEvent == Events.EditedARecordCode));

					AssertEquals("Should have removed capability assignment", Guid.Empty, task.P9_G4_RequiredCapability);
					AssertEquals("Should have open status", ProcessTaskStatusCodeList.Codes.Open, task.P9_Status);
				});
			}
		}

		[TestDateIncremental(seconds: 1)]
		public void TestTryDeleteAssignmentCapability_ShouldWork_WhenNoCapability()
		{
			var tempValue = GetEnableAddEditAndDeleteLogsItemsRegistryItemValue(true);

			using (SystemDataRegistry.Instance.EnableAddEditAndDeleteLogsItemsRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tempValue))
			{
				var task = CreateWorkflowAndTask();
				task.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
				Factory.Save();

				var dateTimeBeforeCall = ZDateTime.Now;
				var result = Service.TryDeleteAssignmentCapability(task.PK.ToGuid(), out PaveError error);

				task.Reload();

				var logs = task.GetLogs().Find(log => log.SL_EventTime > dateTimeBeforeCall);

				CombineAssertions(() =>
				{
					AssertEquals("Should return Success", expected: true, result);
					AssertNull("Should return no validationErrors", error);

					AssertEquals("Should not StartabilityChanged Log", 0, logs.Count(log => log.SL_SE_NKEvent == Events.StartabilityChanged.Code));
					AssertEquals("Should not have EditedARecord log", 0, logs.Count(log => log.SL_SE_NKEvent == Events.EditedARecordCode));

					AssertEquals("Should have no capability assignment", Guid.Empty, task.P9_G4_RequiredCapability);
					AssertEquals("Should have open status", ProcessTaskStatusCodeList.Codes.Open, task.P9_Status);
				});
			}
		}

		[TestDateIncremental(seconds: 1)]
		public void TestTryDeleteAssignmentCapability_ShouldWork_WhenWorking()
		{
			var tempValue = GetEnableAddEditAndDeleteLogsItemsRegistryItemValue(true);

			using (SystemDataRegistry.Instance.EnableAddEditAndDeleteLogsItemsRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tempValue))
			{
				var capability = Factory.NewWithValidTestData<GlbCapability>();
				var staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_Code = "ST1";
				var task = CreateWorkflowAndTask(staff.GS_Code);
				task.P9_G4_RequiredCapability = capability.PK;
				task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
				Factory.Save();

				var dateTimeBeforeCall = ZDateTime.Now;
				var result = Service.TryDeleteAssignmentCapability(task.PK.ToGuid(), out PaveError error);

				task.Reload();

				var logs = task.GetLogs().Find(log => log.SL_EventTime > dateTimeBeforeCall);

				CombineAssertions(() =>
				{
					AssertEquals("Should return Success", expected: true, result);
					AssertNull("Should return no validationErrors", error);

					AssertEquals("Should not StartabilityChanged Log", 0, logs.Count(log => log.SL_SE_NKEvent == Events.StartabilityChanged.Code));
					AssertEquals("Should have EditedARecord log", 1, logs.Count(log => log.SL_SE_NKEvent == Events.EditedARecordCode));

					AssertEquals("Should have no capability assignment", Guid.Empty, task.P9_G4_RequiredCapability);
					AssertEquals("Should have open status", ProcessTaskStatusCodeList.Codes.Working, task.P9_Status);
				});
			}
		}

		[TestDateIncremental(seconds: 1)]
		public void TestTryDeleteAssignmentCapability_TaskShouldRemainASN_WhenStaffAssigned()
		{
			var tempValue = GetEnableAddEditAndDeleteLogsItemsRegistryItemValue(true);

			using (SystemDataRegistry.Instance.EnableAddEditAndDeleteLogsItemsRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tempValue))
			{
				var capability = Factory.NewWithValidTestData<GlbCapability>();
				var staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_Code = "ST1";
				var task = CreateWorkflowAndTask(staff.GS_Code);
				task.P9_G4_RequiredCapability = capability.PK.ToGuid();
				Factory.Save();

				var dateTimeBeforeCall = ZDateTime.Now;
				var result = Service.TryDeleteAssignmentCapability(task.PK.ToGuid(), out PaveError error);

				task.Reload();

				var logs = task.GetLogs().Find(log => log.SL_EventTime > dateTimeBeforeCall);

				CombineAssertions(() =>
				{
					AssertEquals("Should return Success", expected: true, result);
					AssertNull("Should return no validationErrors", error);

					AssertEquals("Should not have StartabilityChanged Log", 0, logs.Count(log => log.SL_SE_NKEvent == Events.StartabilityChanged.Code));
					AssertEquals("Should have EditedARecord log", 1, logs.Count(log => log.SL_SE_NKEvent == Events.EditedARecordCode));

					AssertEquals("Should have removed capability assignment", Guid.Empty, task.P9_G4_RequiredCapability);
					AssertEquals("Should have assigned status", ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
				});
			}
		}

		[TestDateIncremental(seconds: 1)]
		public void TestTryDeleteAssignmentCapability_ShouldReturnTaskNotFound_WhenInvalidTask()
		{
			var tempValue = GetEnableAddEditAndDeleteLogsItemsRegistryItemValue(true);

			using (SystemDataRegistry.Instance.EnableAddEditAndDeleteLogsItemsRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tempValue))
			{
				var staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_Code = "ST1";
				var task = CreateWorkflowAndTask(staff.GS_Code);
				Factory.Save();

				var dateTimeBeforeCall = ZDateTime.Now;
				var result = Service.TryDeleteAssignmentCapability(Guid.NewGuid(), out PaveError error);

				task.Reload();

				var logs = task.GetLogs().Find(log => log.SL_EventTime > dateTimeBeforeCall);

				CombineAssertions(() =>
				{
					AssertEquals("Should return not Success", expected: false, result);
					AssertEquals("Should return validationErrors", "Task not found.", error.Messages.Single());
					AssertEquals("Task Status should be Assigned", ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
					AssertEquals("Should not have StartabilityChanged Log", 0, logs.Count(log => log.SL_SE_NKEvent == Events.StartabilityChanged.Code));
					AssertEquals("Should not have EditedARecord log", 0, logs.Count(log => log.SL_SE_NKEvent == Events.EditedARecordCode));
				});
			}
		}

		[TestDateIncremental(seconds: 1)]
		public void TestTryDeleteAssignment_ShouldDeleteCapabilityAndStaff_WhenNoErrors()
		{
			var tempValue = GetEnableAddEditAndDeleteLogsItemsRegistryItemValue(true);

			using (SystemDataRegistry.Instance.EnableAddEditAndDeleteLogsItemsRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tempValue))
			{
				var staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_Code = "ST1";
				var capability = Factory.NewWithValidTestData<GlbCapability>();
				var task = CreateWorkflowAndTask(staff.GS_Code);
				task.P9_G4_RequiredCapability = capability.PK;
				Factory.Save();

				var dateTimeBeforeCall = ZDateTime.Now;
				var result = Service.TryDeleteAssignment(task.PK.ToGuid(), out PaveError error);

				task.Reload();

				var logs = task.GetLogs().Find(log => log.SL_EventTime > dateTimeBeforeCall);

				CombineAssertions(() =>
				{
					AssertEquals("Should return Success", expected: true, result);
					AssertNull("Should return no validationErrors", error);

					AssertEquals("Should have StartabilityChanged Log", 1, logs.Count(log => log.SL_SE_NKEvent == Events.StartabilityChanged.Code));
					AssertEquals("Should have EditedARecord log", 1, logs.Count(log => log.SL_SE_NKEvent == Events.EditedARecordCode));

					AssertEquals("Should have removed capability assignment", Guid.Empty, task.P9_G4_RequiredCapability);
					AssertEquals("Should have removed staff assignment", string.Empty, task.P9_GS_NKAssignedStaffMember);
					AssertEquals("Should have open status", ProcessTaskStatusCodeList.Codes.Open, task.P9_Status);
				});
			}
		}

		[TestDateIncremental(seconds: 1)]
		public void TestTryDeleteAssignment_ShouldWork_WhenNoCapability()
		{
			var tempValue = GetEnableAddEditAndDeleteLogsItemsRegistryItemValue(true);

			using (SystemDataRegistry.Instance.EnableAddEditAndDeleteLogsItemsRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tempValue))
			{
				var staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_Code = "ST1";
				var task = CreateWorkflowAndTask(staff.GS_Code);
				Factory.Save();

				var dateTimeBeforeCall = ZDateTime.Now;
				var result = Service.TryDeleteAssignment(task.PK.ToGuid(), out PaveError error);

				task.Reload();

				var logs = task.GetLogs().Find(log => log.SL_EventTime > dateTimeBeforeCall);

				CombineAssertions(() =>
				{
					AssertEquals("Should return Success", expected: true, result);
					AssertNull("Should return no validationErrors", error);

					AssertEquals("Should have StartabilityChanged Log", 1, logs.Count(log => log.SL_SE_NKEvent == Events.StartabilityChanged.Code));
					AssertEquals("Should have EditedARecord log", 1, logs.Count(log => log.SL_SE_NKEvent == Events.EditedARecordCode));

					AssertEquals("Should have no capability assignment", Guid.Empty, task.P9_G4_RequiredCapability);
					AssertEquals("Should have removed staff assignment", string.Empty, task.P9_GS_NKAssignedStaffMember);
					AssertEquals("Should have open status", ProcessTaskStatusCodeList.Codes.Open, task.P9_Status);
				});
			}
		}

		[TestDateIncremental(seconds: 1)]
		public void TestTryDeleteAssignment_ShouldWork_WhenNoStaff()
		{
			var tempValue = GetEnableAddEditAndDeleteLogsItemsRegistryItemValue(true);

			using (SystemDataRegistry.Instance.EnableAddEditAndDeleteLogsItemsRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tempValue))
			{
				var capability = Factory.NewWithValidTestData<GlbCapability>();
				var task = CreateWorkflowAndTask();
				task.P9_G4_RequiredCapability = capability.PK;
				Factory.Save();

				var dateTimeBeforeCall = ZDateTime.Now;
				var result = Service.TryDeleteAssignment(task.PK.ToGuid(), out PaveError error);

				task.Reload();

				var logs = task.GetLogs().Find(log => log.SL_EventTime > dateTimeBeforeCall);

				CombineAssertions(() =>
				{
					AssertEquals("Should return Success", expected: true, result);
					AssertNull("Should return no validationErrors", error);

					AssertEquals("Should have StartabilityChanged Log", 1, logs.Count(log => log.SL_SE_NKEvent == Events.StartabilityChanged.Code));
					AssertEquals("Should have EditedARecord log", 1, logs.Count(log => log.SL_SE_NKEvent == Events.EditedARecordCode));

					AssertEquals("Should have removed capability assignment", Guid.Empty, task.P9_G4_RequiredCapability);
					AssertEquals("Should have no staff assignment", string.Empty, task.P9_GS_NKAssignedStaffMember);
					AssertEquals("Should have open status", ProcessTaskStatusCodeList.Codes.Open, task.P9_Status);
				});
			}
		}

		[TestDateIncremental(seconds: 1)]
		public void TestTryDeleteAssignment_ShouldWork_WhenNoStaffOrCapability()
		{
			var tempValue = GetEnableAddEditAndDeleteLogsItemsRegistryItemValue(true);

			using (SystemDataRegistry.Instance.EnableAddEditAndDeleteLogsItemsRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tempValue))
			{
				var staff = Factory.NewWithValidTestData<GlbStaff>();
				var task = CreateWorkflowAndTask();
				task.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
				Factory.Save();

				var dateTimeBeforeCall = ZDateTime.Now;
				var result = Service.TryDeleteAssignment(task.PK.ToGuid(), out PaveError error);

				task.Reload();

				var logs = task.GetLogs().Find(log => log.SL_EventTime > dateTimeBeforeCall);

				CombineAssertions(() =>
				{
					AssertEquals("Should return Success", expected: true, result);
					AssertNull("Should return no validationErrors", error);

					AssertEquals("Should not have StartabilityChanged Log", 0, logs.Count(log => log.SL_SE_NKEvent == Events.StartabilityChanged.Code));
					AssertEquals("Should not have EditedARecord log", 0, logs.Count(log => log.SL_SE_NKEvent == Events.EditedARecordCode));

					AssertEquals("Should have no capability assignment", Guid.Empty, task.P9_G4_RequiredCapability);
					AssertEquals("Should have no staff assignment", string.Empty, task.P9_GS_NKAssignedStaffMember);
				});
			}
		}

		[TestDateIncremental(seconds: 1)]
		public void TestTryDeleteAssignment_ShouldReturnTaskNotFound_WhenInvalidTask()
		{
			var tempValue = GetEnableAddEditAndDeleteLogsItemsRegistryItemValue(true);

			using (SystemDataRegistry.Instance.EnableAddEditAndDeleteLogsItemsRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tempValue))
			{
				var staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_Code = "ST1";
				var task = CreateWorkflowAndTask(staff.GS_Code);
				Factory.Save();

				var dateTimeBeforeCall = ZDateTime.Now;
				var result = Service.TryDeleteAssignment(Guid.NewGuid(), out PaveError error);

				task.Reload();

				var logs = task.GetLogs().Find(log => log.SL_EventTime > dateTimeBeforeCall);

				CombineAssertions(() =>
				{
					AssertEquals("Should return not Success", expected: false, result);
					AssertEquals("Should return validationErrors", "Task not found.", error.Messages.Single());
					AssertEquals("Task Status should be Assigned", ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
					AssertEquals("Should not have StartabilityChanged Log", 0, logs.Count(log => log.SL_SE_NKEvent == Events.StartabilityChanged.Code));
					AssertEquals("Should not have EditedARecord log", 0, logs.Count(log => log.SL_SE_NKEvent == Events.EditedARecordCode));
				});
			}
		}

		#endregion

		#region DeleteTask

		[TestDateIncremental(seconds: 1)]
		public void TestTryDeleteTask_ShouldWork()
		{
			var tempValue = GetEnableAddEditAndDeleteLogsItemsRegistryItemValue(true);

			using (SystemDataRegistry.Instance.EnableAddEditAndDeleteLogsItemsRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tempValue))
			{
				var task = CreateWorkflowAndTask();
				Factory.Save();

				var retrievedTask = Factory.Load<ProcessTask>(task.PK);
				AssertNotNull("The task should exist", task);

				var result = Service.TryDeleteTask(task.PK.ToGuid(), out var error);
				retrievedTask = ReloadTask(retrievedTask.PK);

				AssertNull("The task should not exist anymore", retrievedTask);
				AssertEquals("Should return Success", expected: true, result);
				AssertNull("Should not return validationErrors", error);
			}
		}

		[TestDateIncremental(seconds: 1)]
		public void TestTryDeleteTask_ShouldNotWorkWithInvalidId()
		{
			var tempValue = GetEnableAddEditAndDeleteLogsItemsRegistryItemValue(true);

			using (SystemDataRegistry.Instance.EnableAddEditAndDeleteLogsItemsRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tempValue))
			{
				var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
				var orgPK = helper.CreateClient("Org1");
				var warehouse = helper.CreateWarehouse("WHS", "A");
				var product = (OrgSupplierPart)helper.CreateProduct(orgPK, "Pro01");
				var receive = helper.CreateWhsReceiveWithInventory(orgPK, warehouse.PK, "R1", product.PK, 10m);
				Factory.Save();

				var task = CreateWorkflowAndTask();
				task.P9_ParentID = receive.PK;
				task.P9_ParentTableCode = "WD";
				task.P9_FormFlowType = "WUL";
				Factory.Save();

				var retrievedTask = Factory.Load<ProcessTask>(task.PK);
				AssertNotNull(task);

				var result = Service.TryDeleteTask(Guid.NewGuid(), out var error);
				retrievedTask = ReloadTask(task.PK);

				AssertNotNull(retrievedTask);
				AssertEquals("Should not return Success", expected: false, result);
				AssertNotNull("Should return validationErrors", error);
			}
		}

		[TestDateIncremental(seconds: 1)]
		public void TestTryDeleteTask_TaskHasFormFlowType()
		{
			var tempValue = GetEnableAddEditAndDeleteLogsItemsRegistryItemValue(true);

			using (SystemDataRegistry.Instance.EnableAddEditAndDeleteLogsItemsRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tempValue))
			{
				var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
				var orgPK = helper.CreateClient("Org1");
				var warehouse = helper.CreateWarehouse("WHS", "A");
				var product = (OrgSupplierPart)helper.CreateProduct(orgPK, "Pro01");
				var receive = helper.CreateWhsReceiveWithInventory(orgPK, warehouse.PK, "R1", product.PK, 10m);
				Factory.Save();

				var task = CreateWorkflowAndTask();
				task.P9_ParentID = receive.PK;
				task.P9_ParentTableCode = "WD";
				task.P9_FormFlowType = "WUL";
				Factory.Save();

				var retrievedTask = Factory.Load<ProcessTask>(task.PK);
				AssertNotNull(task);

				var result = Service.TryDeleteTask(Guid.NewGuid(), out var error);
				retrievedTask = ReloadTask(task.PK);

				AssertNotNull(retrievedTask);
				AssertEquals("Should not return Success", expected: false, result);
				AssertNotNull("Should return validationErrors", error);
				AssertNotNull("Cannot delete system maintained tasks linked to jobs.", error.Messages.First());
			}
		}

		#endregion

		#region AddNewTask

		[TestDateIncremental(seconds: 1)]
		public void TestTryAddNewTask_ShouldWork()
		{
			var tempValue = GetEnableAddEditAndDeleteLogsItemsRegistryItemValue(true);

			using (SystemDataRegistry.Instance.EnableAddEditAndDeleteLogsItemsRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tempValue))
			{
				var staff = Factory.NewWithValidTestData<GlbStaff>();
				var capability = Factory.NewWithValidTestData<GlbCapability>();
				var workflow = BMSTestHelper.CreateWorkflow(Factory, "workflow");
				var workflowPK = workflow.PK;
				var workflowTask = BMSTestHelper.CreateTask(workflow);
				var notes = "<p>blablabla</p>";
				Factory.Save();

				var maxSeq = workflowTask
					.Parent
					.Workflows
					.ToList<IProcessHeader>()
					.First(p => p.PK.Equals(workflowPK))
					.Tasks
					.Max(t => t.P9_Sequence);

				var dateTimeBeforeCall = ZDateTime.Now;
				var result = Service.TryAddNewTask(
					new NewTaskRequest
					{
						WorkflowId = workflowPK.ToGuid(),
						Description = "Spelng Ereers",
						Type = "UDF",
						EstimateFactor = 4,
						LowEstimate = 60,
						ActualDuration = 60,
						CapabilityCode = capability.G4_Code,
						StaffId = staff.PK.ToGuid(),
						Notes = notes,
					},
					out var error);

				workflowTask = ReloadTask(workflowTask.PK);
				var task = workflowTask.Parent.WorkflowItems.Where(t => t.P9_Description.Equals("Spelng Ereers")).First();
				var newBlob = ZBlob.FromUTF8(notes);

				CombineAssertions(() =>
				{
					AssertNotNull("New task should exist", task);
					AssertEquals("Should return Success", expected: true, result.Success);
					AssertEquals("Should return new task id", expected: task.PK.ToGuid(), result.Response.NewTaskId);
					AssertNull("Should not return validationErrors", error);
					AssertEquals("Task Status should be Assigned", ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
					AssertEquals("Task should have staff assigned", staff.GS_Code, task.P9_GS_NKAssignedStaffMember);
					AssertEquals("Task should have capability assigned", capability.PK, task.P9_G4_RequiredCapability);
					AssertEquals("Task should have estimate factor", new ZDecimal(4), task.P9_EstimateVariationFactor);
					AssertEquals("Task should have low estimate", new ZDecimal(1), task.LowEstimatedDurationHours);
					AssertEquals("Task should have actual duration", new ZDecimal(1), task.ActualDurationHours);
					AssertEquals("Task should be assigned status", ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
					AssertEquals("Task should have notes", newBlob, task.P9_Notes_HTML);
					AssertLessThan("Task seq should be greater than last max task seq in workflow", maxSeq, task.P9_Sequence);
					AssertEquals("Workflow is parent of task", workflowPK, task.P9_FH_ProcessHeader);
				});
			}
		}

		[TestDateIncremental(seconds: 1)]
		public void TestTryAddNewTask_ShouldWork_WhenNoEstimateOrAssignment()
		{
			var tempValue = GetEnableAddEditAndDeleteLogsItemsRegistryItemValue(true);

			using (SystemDataRegistry.Instance.EnableAddEditAndDeleteLogsItemsRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tempValue))
			{
				var staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_Code = "ST1";
				var workflowTask = CreateWorkflowAndTask();
				var workflowPK = workflowTask.P9_FH_ProcessHeader;
				var capability = Factory.NewWithValidTestData<GlbCapability>();
				Factory.Save();

				var dateTimeBeforeCall = ZDateTime.Now;
				var result = Service.TryAddNewTask(
					new NewTaskRequest
					{
						WorkflowId = workflowPK.ToGuid(),
						Description = "Spelng Ereers",
						Type = "UDF",
					},
					out var error);

				workflowTask = ReloadTask(workflowTask.PK);
				var task = workflowTask.Parent.WorkflowItems.Where(t => t.P9_Description.Equals("Spelng Ereers")).First();

				CombineAssertions(() =>
				{
					AssertNotNull("New task should exist", task);
					AssertEquals("Should return Success", expected: true, result.Success);
					AssertNull("Should not return validationErrors", error);
					AssertEquals("Task should be open status", ProcessTaskStatusCodeList.Codes.Open, task.P9_Status);
					AssertEquals("Workflow is parent of task", workflowPK, task.P9_FH_ProcessHeader);
				});
			}
		}

		[TestDateIncremental(seconds: 1)]
		public void TestTryAddNewTask_ShouldFail_WhenInvalidWorkflow()
		{
			var tempValue = GetEnableAddEditAndDeleteLogsItemsRegistryItemValue(true);

			using (SystemDataRegistry.Instance.EnableAddEditAndDeleteLogsItemsRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tempValue))
			{
				var staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_Code = "ST1";
				var workflowPK = Guid.Empty;
				var capability = Factory.NewWithValidTestData<GlbCapability>();
				Factory.Save();

				var dateTimeBeforeCall = ZDateTime.Now;
				var result = Service.TryAddNewTask(
					new NewTaskRequest
					{
						WorkflowId = workflowPK,
						Description = "Spelng Ereers",
						Type = "UDF",
						EstimateFactor = 2,
						LowEstimate = 10,
						CapabilityCode = capability.G4_Code,
						StaffId = staff.PK.ToGuid(),
					},
					out var error);

				CombineAssertions(() =>
				{
					AssertEquals("Should not return Success", expected: false, result.Success);
					AssertEquals("Should return validationErrors", "Workflow not found.", error.Messages.Single());
				});
			}
		}

		[TestDateIncremental(seconds: 1)]
		public void TestTryAddNewTask_ShouldFail_WhenInvalidCapability()
		{
			var tempValue = GetEnableAddEditAndDeleteLogsItemsRegistryItemValue(true);

			using (SystemDataRegistry.Instance.EnableAddEditAndDeleteLogsItemsRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tempValue))
			{
				var staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_Code = "ST1";
				var workflow = CreateWorkflowAndTask();
				var workflowPK = workflow.P9_FH_ProcessHeader;
				Factory.Save();

				var dateTimeBeforeCall = ZDateTime.Now;
				var result = Service.TryAddNewTask(
					new NewTaskRequest
					{
						WorkflowId = workflowPK.ToGuid(),
						Description = "Spelng Ereers",
						Type = "UDF",
						EstimateFactor = 2,
						LowEstimate = 10,
						CapabilityCode = new ZString("This is an invalid capability"),
						StaffId = staff.PK.ToGuid(),
					},
					out var error);

				AssertContains("Error creating a new task.", error.Messages.FirstOrDefault());
			}
		}

		[TestDateIncremental(seconds: 1)]
		public void TestTryAddNewTask_ShouldFail_WhenNoType()
		{
			var tempValue = GetEnableAddEditAndDeleteLogsItemsRegistryItemValue(true);

			using (SystemDataRegistry.Instance.EnableAddEditAndDeleteLogsItemsRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tempValue))
			{
				var staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_Code = "ST1";
				var workflow = CreateWorkflowAndTask();
				var workflowPK = workflow.P9_FH_ProcessHeader;
				Factory.Save();

				var dateTimeBeforeCall = ZDateTime.Now;
				var result = Service.TryAddNewTask(
					new NewTaskRequest
					{
						WorkflowId = workflowPK.ToGuid(),
						Description = "Spelng Ereers",
						EstimateFactor = 2,
						LowEstimate = 10,
						CapabilityCode = new ZString(),
						StaffId = staff.PK.ToGuid(),
					},
					out var error);

				AssertEquals("Should not return Success", expected: false, result.Success);
			}
		}

		[TestDateIncremental(seconds: 1)]
		public void TestTryAddNewTask_ShouldFail_WhenDescriptionTooLong()
		{
			var tempValue = GetEnableAddEditAndDeleteLogsItemsRegistryItemValue(true);

			using (SystemDataRegistry.Instance.EnableAddEditAndDeleteLogsItemsRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tempValue))
			{
				var staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_Code = "ST1";
				var workflow = CreateWorkflowAndTask();
				var workflowPK = workflow.P9_FH_ProcessHeader;
				Factory.Save();

				var dateTimeBeforeCall = ZDateTime.Now;
				var result = Service.TryAddNewTask(
					new NewTaskRequest
					{
						WorkflowId = workflowPK.ToGuid(),
						Description = "123456789012345678901234567890123456789012345678901234567890",
						Type = "UDF",
					},
					out var error);

				// Removes developer notification exceptions
				ErrorReporter.Clear();

				AssertContains("Error creating a new task.", error.Messages.FirstOrDefault());
			}
		}

		public void TestTryAddNewTask_ShouldCreateTaskWithSameSequenceNumberAsRelatedTask_WhenSupplied()
		{
			var tempValue = GetEnableAddEditAndDeleteLogsItemsRegistryItemValue(true);

			using (SystemDataRegistry.Instance.EnableAddEditAndDeleteLogsItemsRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tempValue))
			{
				var staff1 = Factory.NewWithValidTestData<GlbStaff>();
				staff1.GS_Code = "ST1";
				var staff2 = Factory.NewWithValidTestData<GlbStaff>();
				staff2.GS_Code = "ST2";

				var task = CreateWorkflowAndTask(staff1.GS_Code);
				var workflowPK = task.P9_FH_ProcessHeader;
				task.P9_Sequence = 100;

				Factory.Save();

				var result = Service.TryAddNewTask(new NewTaskRequest()
				{
					WorkflowId = workflowPK.ToGuid(),
					Description = "this is a description",
					Type = "UDF",
					StaffId = staff2.PK.ToGuid(),
					RelatedTaskId = task.PK.ToGuid(),
				}, out var error);

				task = new BusinessObjectFactory().Load<ProcessTask>(task.PK);
				var assistTask = task.Parent.WorkflowItems.Where(t => t.P9_Description == "this is a description").First();

				AssertEquals("Task sequences should be equal", task.P9_Sequence, assistTask.P9_Sequence);
			}
		}

		#endregion

		#region Notes

		public void TestUpdateNotes()
		{
			var task = Factory.New<ProcessTask>();
			Factory.Save();
			var content = "<p>new notes</p>";
			var updateRequest = new UpdateTaskNotesRequest()
			{
				NewNotes = content,
				PreviousHash = HashHelper.GetHash(task.P9_Notes.ToUTF8()),
			};

			var result = Service.TryUpdateNotes(task.PK.ToGuid(), updateRequest);
			task = new BusinessObjectFactory().Load<ProcessTask>(task.PK);

			AssertNull(result.Error);
			AssertEquals(content, result.TaskNotesResponse.Content);
			AssertEquals(HashHelper.GetHash(task.P9_Notes.ToUTF8()), result.TaskNotesResponse.Hash);
			var newBlob = ZBlob.FromUTF8(new HtmlToRtfConverter().Convert(updateRequest.NewNotes));
			Assert(task.P9_Notes.Equals(newBlob));
		}

		public void TestUpdateNotes_ShouldThrownException_WhenNewNotesAreNullOrHashNullOrEmpty()
		{
			var updateRequest1 = new UpdateTaskNotesRequest()
			{
				NewNotes = "Notes",
				PreviousHash = null
			};
			var updateRequest2 = new UpdateTaskNotesRequest()
			{
				NewNotes = "Notes",
				PreviousHash = string.Empty,
			};
			var updateRequest3 = new UpdateTaskNotesRequest()
			{
				NewNotes = null,
				PreviousHash = "hash"
			};
			var updateRequest4 = new UpdateTaskNotesRequest()
			{
				NewNotes = string.Empty,
				PreviousHash = "hash"
			};

			AssertExceptionThrown<ArgumentNullException>(() => Service.TryUpdateNotes(Guid.NewGuid(), updateRequest1));
			AssertExceptionThrown<ArgumentNullException>(() => Service.TryUpdateNotes(Guid.NewGuid(), updateRequest2));
			AssertExceptionThrown<ArgumentNullException>(() => Service.TryUpdateNotes(Guid.NewGuid(), updateRequest3));

			var task = Factory.New<ProcessTask>();
			Factory.Save();

			AssertNoExceptionThrown(() => Service.TryUpdateNotes(task.PK.ToGuid(), updateRequest4));
		}

		public void TestUpdateNotes_ShouldReturnFalse_WhenPreviousHashDoesNotMatch()
		{
			var task = Factory.New<ProcessTask>();
			Factory.Save();
			var content = "<p>new notes</p>";
			var updateRequest = new UpdateTaskNotesRequest()
			{
				NewNotes = content,
				PreviousHash = "wrong",
			};

			var result = Service.TryUpdateNotes(task.PK.ToGuid(), updateRequest);

			Assert(!result.Success);
			AssertNotNull(result.Error);
		}
		#endregion

		#region TryUpdateTypeAndNotes

		public void TestUpdateTypeAndNotes()
		{
			var updateRequest = new UpdateTypeAndNotesRequest()
			{
				NewType = "A",
				PreviousType = null,
				Notes = "<p>new notes</p>"
			};

			AssertExceptionThrown<ArgumentException>(() => Service.TryUpdateTypeAndNotes(Guid.NewGuid(), updateRequest, out _));

			updateRequest.NewType = null;
			updateRequest.PreviousType = "B";

			AssertExceptionThrown<ArgumentException>(() => Service.TryUpdateTypeAndNotes(Guid.NewGuid(), updateRequest, out _));

			updateRequest.NewType = "A";
			updateRequest.PreviousType = "B";

			var result = Service.TryUpdateTypeAndNotes(Guid.NewGuid(), updateRequest, out var error1);
			AssertEquals(false, result);
			AssertEquals(LocalizationHelper.WorkflowTaskReordering.RecordNotFoundMessage, error1.Messages.Single());

			var task = CreateWorkflowAndTask();
			Factory.Save();

			result = Service.TryUpdateTypeAndNotes(task.PK.ToGuid(), updateRequest, out var error2);
			AssertEquals(false, result);
			AssertEquals(LocalizationHelper.WorkflowTaskReordering.FieldChangedByAnotherUserMessage, error2.Messages.Single());

			updateRequest.PreviousType = task.P9_Type = "UDF";
			Factory.Save();

			result = Service.TryUpdateTypeAndNotes(task.PK.ToGuid(), updateRequest, out var error3);
			AssertEquals(false, result);
			Assert(error3.Messages.Any());

			BMSTestHelper.AddTaskTypesToRegistry(WorkflowDescriptors.DummyWorkflowDescriptorCode, "BLA");
			updateRequest.NewType = "BLA";
			Factory.Save();

			result = Service.TryUpdateTypeAndNotes(task.PK.ToGuid(), updateRequest, out var error4);
			AssertEquals(false, result);
			Assert(error4.Messages.Any());

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			task.P9_GS_NKAssignedStaffMember = staff.GS_Code = "STF";
			Factory.Save();

			result = Service.TryUpdateTypeAndNotes(task.PK.ToGuid(), updateRequest, out var error5);
			Assert(result);
			AssertNull(error5);
			task = ReloadTask(task.PK);
			AssertEquals("BLA", task.P9_Type);
			AssertEquals(ZBlob.FromUTF8(new HtmlToRtfConverter().Convert(updateRequest.Notes)), task.P9_Notes);
		}

		#endregion

		#region Delete

		public void Test_Delete_Task()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ST1";

			var task = CreateWorkflowAndTask(staff.GS_Code);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			Factory.Save();

			var result = Service.TryDelete(task.PK.ToGuid(), new DeleteTaskRequest(), out var businessResponse);

			Assert("Should work", result);
			var taskResult = Factory.CreateNewFactory().Load<ProcessTask>(task.PK);
			AssertEquals("Deleted task should not exist", taskResult, null);
		}

		public void Test_Delete_Task_ShouldFail_When_TaskDoesntExist()
		{
			var result = Service.TryDelete(Guid.NewGuid(), new DeleteTaskRequest(), out var businessResponse);
			Assert("Shouldn't work", !result);
			AssertEquals("Incorrect reason to not delete", businessResponse.Message.Text, "Task not found.");
		}

		public void Test_Delete_Task_ShouldFailWhen_TaskCannotBeDeleted()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ST1";

			var task = CreateWorkflowAndTask(staff.GS_Code);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task.P9_TaskCannotBeDeleted = true;

			Factory.Save();

			var result = Service.TryDelete(task.PK.ToGuid(), new DeleteTaskRequest(), out var businessResponse);

			var taskResult = Factory.CreateNewFactory().Load<ProcessTask>(task.PK);
			AssertNotEquals("Task should not be deleted", taskResult, null);
			Assert("Shouldn't work", !result);
			AssertEquals("Incorrect reason to not delete", businessResponse.Message.Text, "Mandatory Task cannot be deleted.");
		}

		public void Test_Delete_Task_ShouldFailWhen_TaskHasFormFlowType()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ST1";
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var orgPK = helper.CreateClient("Org1");
			var warehouse = helper.CreateWarehouse("WHS", "A");
			var product = (OrgSupplierPart)helper.CreateProduct(orgPK, "Pro01");
			var receive = helper.CreateWhsReceiveWithInventory(orgPK, warehouse.PK, "R1", product.PK, 10m);
			Factory.Save();

			var task = CreateWorkflowAndTask(staff.GS_Code);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task.P9_ParentID = receive.PK;
			task.P9_ParentTableCode = "WD";
			task.P9_FormFlowType = "WUL";
			Factory.Save();

			var result = Service.TryDelete(task.PK.ToGuid(), new DeleteTaskRequest(), out var businessResponse);

			var taskResult = Factory.CreateNewFactory().Load<ProcessTask>(task.PK);
			AssertNotEquals("Task should not be deleted", taskResult, null);
			Assert("Shouldn't work", !result);
			AssertEquals("Incorrect reason to not delete", "Cannot delete system maintained tasks linked to jobs.", businessResponse.Message.Text);
		}

		#endregion
	}
}
