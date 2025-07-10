using System;
using System.Collections;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.PAVE.Common.DTO;
using CargoWise.Types;
using Enterprise.BufferManagement.Service.Shared;
using Enterprise.BufferManagement.Service.Shared.Task.Dtos.Lifecycle;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Workflow.Business;
using Enterprise.Workflow.Integration;
using WiseTech.Business.Core;

namespace Enterprise.BufferManagement.Service
{
	//TODO: *** CHECK SECURITY RIGHTS **** IMPORTANT!
	public class TaskLifecycleService : ITaskLifecycleService
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Stateless localized strings")]
		public static class BusinessMessages
		{
			static string CanonicalPrefix => $"{WiseTech.Business.Taxonomy.TaxonomyCommon.WiseTechCanonicalPrefix}.workflows.task-lifecycle";

			#region Reopen

			public static BusinessMessage IncorrectReopenStatus => BusinessMessage.BuildError(
				englishText: "A task can only be reopened as working, assigned or suspended.",
				localizationKey: Guid.Parse("660845b6-7519-4a4d-9f8d-9b4b5d254bb7"),
				canonicalPrefix: $"{CanonicalPrefix}.reopen",
				nameof(IncorrectReopenStatus)
			);

			public static BusinessMessage OnlyClosedOrCancelledTasksCanBeReopened => BusinessMessage.BuildError(
				englishText: "Only closed or cancelled tasks can be reopened.",
				localizationKey: Guid.Parse("03688058-5457-4a93-8930-88626583f452"),
				canonicalPrefix: CanonicalPrefix,
				nameof(OnlyClosedOrCancelledTasksCanBeReopened)
			);

			public static BusinessMessage ErrorReopeningTask => BusinessMessage.BuildError(
				englishText: "Error reopening task.",
				localizationKey: Guid.Parse("98523471-6926-4EEB-A295-0DCDD41A5431"),
				canonicalPrefix: CanonicalPrefix,
				nameof(ErrorReopeningTask)
			);

			#endregion

			#region Start

			public static BusinessMessage OnlyAssignedTasksCanBeStarted => BusinessMessage.BuildError(
				englishText: "Only assigned tasks can be started.",
				localizationKey: Guid.Parse("E8A79FE2-EA40-4D7A-865C-C9F0D05FDFEB"),
				canonicalPrefix: $"{CanonicalPrefix}.start",
				nameof(OnlyAssignedTasksCanBeStarted)
			);

			public static BusinessMessage ErrorStartingTask => BusinessMessage.BuildError(
				englishText: "Error starting task.",
				localizationKey: Guid.Parse("ADC796BB-C233-46D9-944D-C9F0D05FDFEB"),
				canonicalPrefix: CanonicalPrefix,
				nameof(ErrorStartingTask)
			);

			#endregion

			#region Resume

			public static BusinessMessage ErrorResumingTask => BusinessMessage.BuildError(
				englishText: "Error resuming task.",
				localizationKey: Guid.Parse("7995C6F7-FF1A-4A4A-8612-83E48C8B3D45"),
				canonicalPrefix: CanonicalPrefix,
				nameof(ErrorResumingTask)
			);

			public static BusinessMessage OnlySuspendedTasksCanBeResumed => BusinessMessage.BuildError(
				englishText: "Only suspended tasks can be resumed.",
				localizationKey: Guid.Parse("5eeb734b-1790-4be2-a814-8afe44abf94d"),
				canonicalPrefix: CanonicalPrefix,
				nameof(OnlySuspendedTasksCanBeResumed)
			);

			#endregion

			#region Suspend

			public static BusinessMessage OnlyWorkingTasksCanBeSuspended => BusinessMessage.BuildError(
				englishText: "Only working tasks can be suspended.",
				localizationKey: Guid.Parse("E8A79FE2-EC82-479A-8145-58CB4F89056A"),
				canonicalPrefix: $"{CanonicalPrefix}.suspend",
				nameof(OnlyWorkingTasksCanBeSuspended)
			);

			#endregion

			#region Cancel

			public static BusinessMessage TaskIsAlreadyCancelled => BusinessMessage.BuildError(
				englishText: "Task is already cancelled.",
				localizationKey: Guid.Parse("929a0efd-55e2-4798-a15e-b71c97c70868"),
				canonicalPrefix: $"{CanonicalPrefix}.cancel",
				nameof(TaskIsAlreadyCancelled)
			);

			public static BusinessMessage ErrorCancellingTask => BusinessMessage.BuildError(
				englishText: "Error canceling task.",
				localizationKey: Guid.Parse("E8A79FE2-C233-46D9-944D-1297608ADED6"),
				canonicalPrefix: $"{CanonicalPrefix}.cancel",
				nameof(ErrorCancellingTask)
			);

			#endregion

			#region Complete

			public static BusinessMessage OnlyWorkingOrSuspendedOrAssignedTasksCanBeCompleted => BusinessMessage.BuildError(
				englishText: "Only working, suspended or assigned tasks can be completed.",
				localizationKey: Guid.Parse("d3b07384-d9a1-4d3b-8a1d-3b6d3b6d3b6d"),
				canonicalPrefix: $"{CanonicalPrefix}.complete",
				nameof(OnlyWorkingOrSuspendedOrAssignedTasksCanBeCompleted)
			);

			public static BusinessMessage ErrorCompletingTask => BusinessMessage.BuildError(
				englishText: "Error completing task.",
				localizationKey: Guid.Parse("E8A79FE2-C233-46D9-944D-1297608ADED6"),
				canonicalPrefix: $"{CanonicalPrefix}.complete",
				nameof(ErrorCompletingTask)
			);

			public static BusinessMessage ClosedTaskCantBeCancelled => BusinessMessage.BuildError(
				englishText: "Closed task can't be cancelled.",
				localizationKey: Guid.Parse("ADC796BB-EC82-479A-8145-58CB4F89056A"),
				canonicalPrefix: $"{CanonicalPrefix}.complete",
				nameof(ClosedTaskCantBeCancelled)
			);

			public static BusinessMessage ContainmentBarrierAnswerRequiredToCompleteTask => BusinessMessage.BuildError(
				englishText: "This is quality containment barrier task and requires a response to be completed.",
				localizationKey: Guid.Parse("88A80C54-EC9C-4235-8412-8BB340A8436D"),
				canonicalPrefix: $"{CanonicalPrefix}.complete",
				nameof(ContainmentBarrierAnswerRequiredToCompleteTask)
			);

			public static BusinessMessage TaskNeedStaffToBeCompleted => BusinessMessage.BuildError(
				englishText: "Task needs staff to be completed.",
				localizationKey: Guid.Parse("DB88C084-8D52-4FC4-8327-1736C59C1CF5"),
				canonicalPrefix: $"{CanonicalPrefix}.complete",
				nameof(TaskNeedStaffToBeCompleted)
			);

			public static BusinessMessage StaffLacksRequiredAssessCompetency => BusinessMessage.BuildError(
				englishText: "You do not have the required ASSESS competencies to work on this task.",
				localizationKey: Guid.Parse("4792fc10-2445-4079-940f-50050f9b97e3"),
				canonicalPrefix: $"{CanonicalPrefix}.complete",
				nameof(StaffLacksRequiredAssessCompetency)
			);

			#endregion

			#region Assign

			public static BusinessMessage TaskNeedStaffToBeAssigned => BusinessMessage.BuildError(
				englishText: "Task needs staff to be assigned.",
				localizationKey: Guid.Parse("DB88C084-8D52-4FC4-8327-1736C59C1CF5"),
				canonicalPrefix: $"{CanonicalPrefix}.assign",
				nameof(TaskNeedStaffToBeAssigned)
			);
			public static BusinessMessage TaskCantBeAssignToTheCurrentUser => BusinessMessage.BuildError(
				englishText: "Task can not be assign to the current user, use claim instead.",
				localizationKey: Guid.Parse("E0ED9227-3A73-4031-8EE5-3F1DE4A691D5"),
				canonicalPrefix: $"{CanonicalPrefix}.assign",
				nameof(TaskCantBeAssignToTheCurrentUser)
			);

			#endregion

			#region Claim

			public static BusinessMessage TaskIsAlreadyClaimed => BusinessMessage.BuildError(
				englishText: "Task is already claimed.",
				localizationKey: Guid.Parse("64786957-5964-4F68-A9C2-93C48B6BE5C3"),
				canonicalPrefix: $"{CanonicalPrefix}.claim",
				nameof(TaskIsAlreadyClaimed)
			);

			public static BusinessMessage ErrorClaimingTask => BusinessMessage.BuildError(
				englishText: "Error claiming task.",
				localizationKey: Guid.Parse("4557ab21-dba8-4764-bc18-88899b3a969b"),
				canonicalPrefix: $"{CanonicalPrefix}.claim",
				nameof(ErrorClaimingTask)
			);

			#endregion
		}

		bool TryStart(Guid taskId, StartTaskRequest startTaskRequest, out BusinessResponse businessResponse, BusinessObjectFactory factory)
		{
			if (!TaskService.TryLoadTask(taskId, factory, throwIfNotFound: false, out var task, out businessResponse))
			{
				return false;
			}

			if (task.P9_Status != ProcessTaskStatusCodeList.Codes.Assigned)
			{
				businessResponse = BusinessResponse.Build(BusinessMessages.OnlyAssignedTasksCanBeStarted);
				return false;
			}

			if (!TaskUtil.ValidateTask(task, out _, out var secondaryBusinessMessages))
			{
				businessResponse = BusinessResponse.Build(BusinessMessages.ErrorStartingTask, secondaryBusinessMessages);
				return false;
			}

			var taskStatusChangeResponders = ObjectFactory.Get<IEnumerable>("TaskStatusChangeResponders").Cast<IStatusChangeResponder>();
			var responder = taskStatusChangeResponders.FirstOrDefault(r => r.Responder == StatusChangeResponder.CompetencyRequirements);

			if (responder != null)
			{
				var (result, reason) = responder.CanChangeStatus(task, ProcessTaskStatusCodeList.Codes.Working);

				if (!result)
				{
					businessResponse = BusinessResponse.Build(BusinessMessages.StaffLacksRequiredAssessCompetency);
					return false;
				}
			}

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			return true;
		}

		public bool TryStart(Guid taskId, StartTaskRequest startTaskRequest, out BusinessResponse businessResponse)
		{
			var factory = CreateFactory();
			var result = TryStart(taskId, startTaskRequest, out businessResponse, factory);

			if (!result)
			{
				return false;
			}

			factory.Save();

			return true;
		}

		public bool TrySuspend(Guid taskId, SuspendTaskRequest suspendTaskRequest, out BusinessResponse businessResponse)
		{
			var factory = CreateFactory();

			if (!TaskService.TryLoadTask(taskId, factory, throwIfNotFound: false, out var task, out businessResponse))
			{
				return false;
			}

			if (task.P9_Status != ProcessTaskStatusCodeList.Codes.Working)
			{
				businessResponse = BusinessResponse.Build(BusinessMessages.OnlyWorkingTasksCanBeSuspended);
				return false;
			}

			if (!TaskUtil.ValidateTask(task, out _, out var secondaryBusinessMessages))
			{
				businessResponse = BusinessResponse.Build(BusinessMessages.ErrorResumingTask, secondaryBusinessMessages);
				return false;
			}

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;

			factory.Save();

			return true;
		}

		public bool TryResume(Guid taskId, ResumeTaskRequest resumeTaskRequest, out BusinessResponse businessResponse)
		{
			var factory = CreateFactory();

			if (!TaskService.TryLoadTask(taskId, factory, throwIfNotFound: false, out var task, out businessResponse))
			{
				return false;
			}

			if (task.P9_Status != ProcessTaskStatusCodeList.Codes.Suspended)
			{
				businessResponse = BusinessResponse.Build(BusinessMessages.OnlySuspendedTasksCanBeResumed);
				return false;
			}

			if (!TaskUtil.ValidateTask(task, out _, out var secondaryBusinessMessages))
			{
				businessResponse = BusinessResponse.Build(BusinessMessages.ErrorResumingTask, secondaryBusinessMessages);
				return false;
			}

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			factory.Save();

			return true;
		}

		public bool TryReopen(Guid taskId, ReopenTaskRequest reopenTaskRequest, out BusinessResponse businessResponse)
		{
			var factory = CreateFactory();

			if (!TaskService.TryLoadTask(taskId, factory, throwIfNotFound: false, out var task, out businessResponse))
			{
				return false;
			}

			if (task.P9_Status != ProcessTaskStatusCodeList.Codes.Cancelled && task.P9_Status != ProcessTaskStatusCodeList.Codes.Closed)
			{
				businessResponse = BusinessResponse.Build(BusinessMessages.OnlyClosedOrCancelledTasksCanBeReopened);
				return false;
			}

			if (!TaskUtil.ValidateTask(task, out _, out var secondaryBusinessMessages))
			{
				businessResponse = BusinessResponse.Build(BusinessMessages.ErrorReopeningTask, secondaryBusinessMessages);
				return false;
			}

			switch (reopenTaskRequest.NewStatus)
			{
				case nameof(ProcessTaskStatusCodeList.Codes.Working):
					task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
					break;
				case nameof(ProcessTaskStatusCodeList.Codes.Assigned):
					task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
					break;
				case nameof(ProcessTaskStatusCodeList.Codes.Suspended):
					task.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
					break;
				default:
					businessResponse = BusinessResponse.Build(BusinessMessages.IncorrectReopenStatus);
					return false;
			}

			factory.Save();

			return true;
		}

		public bool TryCancel(Guid taskId, CancelTaskRequest cancelTaskRequest, out BusinessResponse businessResponse)
		{
			var factory = CreateFactory();

			if (!TaskService.TryLoadTask(taskId, factory, throwIfNotFound: false, out var task, out businessResponse))
			{
				return false;
			}

			if (task.P9_Status == ProcessTaskStatusCodeList.Codes.Cancelled)
			{
				businessResponse = BusinessResponse.Build(BusinessMessages.TaskIsAlreadyCancelled);
				return false;
			}

			if (task.P9_Status == ProcessTaskStatusCodeList.Codes.Closed)
			{
				businessResponse = BusinessResponse.Build(BusinessMessages.ClosedTaskCantBeCancelled);
				return false;
			}

			if (!TaskUtil.ValidateTask(task, out _, out var secondaryBusinessMessages))
			{
				businessResponse = BusinessResponse.Build(BusinessMessages.ErrorCancellingTask, secondaryBusinessMessages);
				return false;
			}

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;

			factory.Save();

			return true;
		}

		public bool TryAssign(Guid taskId, AssignTaskRequest assignTaskRequest, out BusinessResponse businessResponse)
		{
			var factory = CreateFactory();

			if (!TaskService.TryLoadTask(taskId, factory, throwIfNotFound: false, out var task, out businessResponse))
			{
				return false;
			}

			if (assignTaskRequest == null)
			{
				businessResponse = BusinessResponse.Build(BusinessMessages.TaskNeedStaffToBeAssigned);
				return false;
			}

			var currentStaffId = GlbStaff.CurrentUser.PK.ToGuid();

			if (assignTaskRequest.StaffId == currentStaffId)
			{
				businessResponse = BusinessResponse.Build(BusinessMessages.TaskCantBeAssignToTheCurrentUser);
				return false;
			}

			var staff = factory.Load<GlbStaff>(new ZGuid(assignTaskRequest.StaffId));

			if (staff == null)
			{
				businessResponse = BusinessResponse.Build(WiseTech.Business.HumanResourcesManagement.BusinessMessages.StaffNotFound);
				return false;
			}

			task.P9_GS_NKAssignedStaffMember = staff.GS_Code;

			if (!TaskUtil.ValidateTask(task, out _, out var secondaryBusinessMessages))
			{
				businessResponse = BusinessResponse.Build(BusinessMessages.ErrorClaimingTask, secondaryBusinessMessages);
				return false;
			}

			factory.Save();

			return true;
		}

		bool TryClaim(Guid taskId, ClaimTaskRequest claimTaskRequest, out BusinessResponse businessResponse, BusinessObjectFactory factory)
		{
			if (!TaskService.TryLoadTask(taskId, factory, throwIfNotFound: false, out var task, out businessResponse))
			{
				return false;
			}

			var currentUserCode = GlbStaff.CurrentUser.GS_Code;

			if (task.P9_GS_NKAssignedStaffMember == currentUserCode)
			{
				businessResponse = BusinessResponse.Build(BusinessMessages.TaskIsAlreadyClaimed);
				return false;
			}

			task.P9_GS_NKAssignedStaffMember = currentUserCode;

			if (!TaskUtil.ValidateTask(task, out _, out var secondaryBusinessMessages))
			{
				businessResponse = BusinessResponse.Build(BusinessMessages.ErrorStartingTask, secondaryBusinessMessages);
				return false;
			}

			return true;
		}

		public bool TryClaim(Guid taskId, ClaimTaskRequest claimTaskRequest, out BusinessResponse businessResponse)
		{
			var factory = CreateFactory();
			var result = TryClaim(taskId, claimTaskRequest, out businessResponse, factory);

			if (!result)
			{
				return false;
			}

			factory.Save();

			return true;
		}

		public bool TryClaimAndStart(Guid taskId, ClaimAndStartTaskRequest claimAndStartTaskRequest, out BusinessResponse businessResponse)
		{
			var factory = CreateFactory();
			var result = TryClaim(taskId, new ClaimTaskRequest(), out businessResponse, factory);

			if (!result)
			{
				return false;
			}

			result = TryStart(taskId, new StartTaskRequest(), out businessResponse, factory);

			if (!result)
			{
				return false;
			}

			factory.Save();

			return true;
		}

		public bool TryComplete(Guid taskId, CompleteTaskRequest completeTaskRequest, out BusinessResponse businessResponse)
		{
			var factory = CreateFactory();

			if (!TaskService.TryLoadTask(taskId, factory, throwIfNotFound: false, out var task, out businessResponse))
			{
				return false;
			}

			if (string.IsNullOrEmpty(task.P9_GS_NKAssignedStaffMember))
			{
				businessResponse = BusinessResponse.Build(BusinessMessages.TaskNeedStaffToBeCompleted);
				return false;
			}

			if (task.P9_Status != ProcessTaskStatusCodeList.Codes.Working && task.P9_Status != ProcessTaskStatusCodeList.Codes.Suspended && task.P9_Status != ProcessTaskStatusCodeList.Codes.Assigned)
			{
				businessResponse = BusinessResponse.Build(BusinessMessages.OnlyWorkingOrSuspendedOrAssignedTasksCanBeCompleted);
				return false;
			}

			if (!TaskUtil.ValidateTask(task, out var errors, out var secondaryBusinessMessages))
			{
				businessResponse = BusinessResponse.Build(BusinessMessages.ErrorCompletingTask, secondaryBusinessMessages);
				return false;
			}

			var newStatus = ProcessTaskStatusCodeList.Codes.Closed;

			if (task.IsQualityContainmentBarrierTask() && !task.ContainmentBarrierStatusChangeResponderSupressed && completeTaskRequest?.ContainmentBarrierAnswer == null)
			{
				businessResponse = BusinessResponse.Build(BusinessMessages.ContainmentBarrierAnswerRequiredToCompleteTask);

				return false;
			}

			if (task.IsQualityContainmentBarrierTask() && completeTaskRequest.ContainmentBarrierAnswer != null)
			{
				var response = TaskService.GetContainmentBarrierResponse(completeTaskRequest.ContainmentBarrierAnswer.Response);

				using (var containmentBarrierViewModel = new ContainmentBarrierViewModel(task, ProcessTaskStatusCodeList.Codes.Closed))
				{
					containmentBarrierViewModel.Response = response;

					if (completeTaskRequest.ContainmentBarrierAnswer.Response == ContainmentBarrierResponse.IterationRequired)
					{
						TaskService.CreateIteration(containmentBarrierViewModel, completeTaskRequest.ContainmentBarrierAnswer);
					}

					containmentBarrierViewModel.CommitResponse();
				}
			}

			task.P9_Status = newStatus;

			if (!TaskService.CheckUpdateActualDurationCoreRules(task, completeTaskRequest.ActualDurationInMinutes, out businessResponse))
			{
				return false;
			}

			if (completeTaskRequest.ActualDurationInMinutes != null)
			{
				task.P9_ActualDuration = TimeSpan.FromMinutes(completeTaskRequest.ActualDurationInMinutes ?? 0);
			}

			factory.Save();

			return true;
		}

		public bool TryCheckBeforeComplete(Guid taskId, out TaskCompletionCheckResponse taskCompletionCheckResponse, out BusinessResponse businessResponse)
		{
			businessResponse = null;
			taskCompletionCheckResponse = new TaskCompletionCheckResponse();

			var factory = CreateFactory();

			if (!TaskService.TryLoadTask(taskId, factory, throwIfNotFound: false, out var task, out businessResponse))
			{
				return false;
			}

			const string newStatus = ProcessTaskStatusCodeList.Codes.Closed;

			if (task.IsQualityContainmentBarrierTask() && !task.ContainmentBarrierStatusChangeResponderSupressed)
			{
				taskCompletionCheckResponse.IsContainmentBarrierAnswerRequired = true;

				using var containmentBarrierViewModel = new ContainmentBarrierViewModel(task, newStatus);
				var lookups = new ContainmentBarrierViewModelLookups(containmentBarrierViewModel);
				var reasons = TaskService.GetReasons(lookups);
				var resourcesUnderReview = TaskService.GetResourcesUnderReview(lookups, containmentBarrierViewModel.FindBestResourceUnderReview);
				var tasks = TaskService.GetTasksPreview(task, lookups.ProcessTaskList);
				var possibleOptions = TaskService.GetPossibleOptions(containmentBarrierViewModel.ValidResponses);

				var containmentBarrierRequired = new ContainmentBarrierRequiredDTO()
				{
					Reasons = reasons,
					ResourcesUnderReview = resourcesUnderReview,
					Tasks = tasks,
					PossibleOptions = possibleOptions,
				};

				taskCompletionCheckResponse.ContainmentBarrierDetails = containmentBarrierRequired;
			}

			// set to new status to calculate actual duration up to current time
			task.P9_Status = newStatus;
			taskCompletionCheckResponse.ActualDurationInMinutes = decimal.ToInt32(task.ActualDurationHours * 60);

			taskCompletionCheckResponse.IsActualDurationRequired = WorkflowDataRegistry.Instance.TaskTypes.Value
				.GetTaskTypesFromWorkflowCode(task.WorkflowType)
				.OfType<WorkflowTaskType>()
				.SingleOrDefault(w => w.Code == task.P9_Type)?.IsRequireActualDuration ?? false;

			return true;
		}

		#region Helpers
		BusinessObjectFactory CreateFactory() => new() { NameForDebugging = nameof(TaskLifecycleService), RefreshEnabled = false };
		#endregion
	}
}
