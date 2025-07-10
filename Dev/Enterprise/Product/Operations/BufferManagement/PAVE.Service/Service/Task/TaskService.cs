using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.PAVE.Common.DTO;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Service.Helpers;
using Enterprise.BufferManagement.Service.Shared;
using Enterprise.BufferManagement.Service.Shared.Task;
using Enterprise.BufferManagement.Service.Shared.Task.Dtos;
using Enterprise.BufferManagement.Service.Shared.Task.Dtos.Lifecycle;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Workflow.Business;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json;
using WiseTech.Business.Core;

namespace Enterprise.BufferManagement.Service
{
	public class TaskService : ITaskService
	{
		#region BusinessMessages
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Stateless localized strings")]
		public static class BusinessMessages
		{
			static string CanonicalPrefix => $"{WiseTech.Business.Taxonomy.TaxonomyCommon.WiseTechCanonicalPrefix}.workflows.tasks";

			public static BusinessMessage ActualDurationInMinutesMustBeGreaterThanOrEqualsToZero => BusinessMessage.BuildError(
				englishText: "Actual duration must be greater than or equals to zero minutes.",
				localizationKey: Guid.Parse("E8A79FE2-C233-46D9-944D-1297608ADED6"),
				canonicalPrefix: CanonicalPrefix,
				nameof(ActualDurationInMinutesMustBeGreaterThanOrEqualsToZero)
			);

			public static BusinessMessage ActualDurationIsMandatoryForThisTask => BusinessMessage.BuildError(
				englishText: "Actual duration is mandatory for this task.",
				localizationKey: Guid.Parse("E8A79FE2-C233-46D9-944D-1297608ADED6"),
				canonicalPrefix: CanonicalPrefix,
				nameof(ActualDurationIsMandatoryForThisTask)
			);

			public static BusinessMessage ActualDurationWasChangedByAnotherUser => BusinessMessage.BuildError(
				englishText: "Actual duration was changed by another user.",
				localizationKey: Guid.Parse("356A3482-FFF4-4035-BB81-41CC735E75A2"),
				canonicalPrefix: CanonicalPrefix,
				nameof(ActualDurationWasChangedByAnotherUser)
			);

			public static BusinessMessage ErrorUpdatingActualDuration => BusinessMessage.BuildError(
				englishText: "Error updating actual duration.",
				localizationKey: Guid.Parse("1A129636-CE47-41F1-ADFE-EAA3EDBAFD9C"),
				canonicalPrefix: CanonicalPrefix,
				nameof(ErrorUpdatingActualDuration)
			);

			public static BusinessMessage PreviousHashIsRequiredToUpdateActualDuration => BusinessMessage.BuildError(
				englishText: "Previous hash is required to update actual duration.",
				localizationKey: Guid.Parse("1FDFA6D3-A1A7-44C1-846A-7C1C1F3F000C"),
				canonicalPrefix: CanonicalPrefix,
				nameof(PreviousHashIsRequiredToUpdateActualDuration)
			);

			public static BusinessMessage MandatoryTaskCantBeDeleted => BusinessMessage.BuildError(
				englishText: "Mandatory Task cannot be deleted.",
				localizationKey: Guid.Parse("ADC796BB-EC82-479A-8145-58CB4F89056A"),
				canonicalPrefix: CanonicalPrefix,
				nameof(MandatoryTaskCantBeDeleted)
			);

			public static BusinessMessage ErrorAssigningCapability => BusinessMessage.BuildError(
				englishText: "Error assigning capability.",
				localizationKey: Guid.Parse("68131377-C8D0-4E1D-AAE1-7A870586B822"),
				canonicalPrefix: CanonicalPrefix,
				nameof(ErrorAssigningCapability)
			);

			public static BusinessMessage ErrorUpdatingType => BusinessMessage.BuildError(
				englishText: "Error updating task type.",
				localizationKey: Guid.Parse("2E946748-D5E7-4F94-AFAE-05FEDF24024B"),
				canonicalPrefix: CanonicalPrefix,
				nameof(ErrorUpdatingType)
			);

			public static BusinessMessage TaskWithFormFlowTypeCannotBeDeleted => BusinessMessage.BuildError(
				englishText: "Cannot delete system maintained tasks linked to jobs.",
				localizationKey: Guid.Parse("d4abec4d-8a33-48a6-9f4c-2333aed6f844"),
				canonicalPrefix: CanonicalPrefix,
				nameof(TaskWithFormFlowTypeCannotBeDeleted)
			);
		}
		#endregion

		#region Status and Type

		public bool TryUpdateType(Guid taskId, UpdateTaskTypeRequest updateRequest, out BusinessResponse businessResponse)
		{
			if (updateRequest.PreviousType == null)
			{
				throw new ArgumentNullException(nameof(updateRequest), nameof(updateRequest.PreviousType));
			}

			if (updateRequest.NewType == null)
			{
				throw new ArgumentNullException(nameof(updateRequest), nameof(updateRequest.NewType));
			}

			var factory = CreateFactory();

			if (!TryLoadTask(taskId, factory, throwIfNotFound: false, out var task, out businessResponse))
			{
				return false;
			}

			if (task.P9_Type != updateRequest.PreviousType)
			{
				businessResponse = BusinessResponse.Build(WiseTech.Business.Core.BusinessMessages.UpdateAttemptWithOutdatedDataVersion);
				return false;
			}

			task.P9_Type = updateRequest.NewType.ToUpper();

			if (!TaskUtil.ValidateTask(task, out _, out var secondaryBusinessMessages))
			{
				businessResponse = BusinessResponse.Build(BusinessMessages.ErrorUpdatingType, secondaryBusinessMessages);
				return false;
			}

			factory.Save();

			return true;
		}

		static string GetStatusCode(TaskStatus taskStatus)
		{
			return taskStatus switch
			{
				TaskStatus.Open => ProcessTaskStatusCodeList.Codes.Open,
				TaskStatus.Assigned => ProcessTaskStatusCodeList.Codes.Assigned,
				TaskStatus.Working => ProcessTaskStatusCodeList.Codes.Working,
				TaskStatus.Suspended => ProcessTaskStatusCodeList.Codes.Suspended,
				TaskStatus.Closed => ProcessTaskStatusCodeList.Codes.Closed,
				TaskStatus.Cancelled => ProcessTaskStatusCodeList.Codes.Cancelled,
				_ => null,
			};
		}

		#endregion

		#region Channel

		public TryChangeTaskChannelResult TryChangeChannel(ChangeTaskChannelRequestDTO dto, bool noMove)
		{
			var factory = CreateFactory();
			var task = factory.Load<ProcessTask>(dto.TaskPK);
			var workflow = (ProcessHeader)task?.ProcessHeader;
			ZGuid.TryParse(dto.DestinationChannelEntityPK, out var entityPK);
			var destinationStaff = factory.Load<GlbStaff>(entityPK);

			var validationError = ValidateDTO(task, dto, destinationStaff);

			if (validationError != null)
			{
				return new TryChangeTaskChannelResult(validationError.WrapWithEnumerable());
			}

			var (_, tasks) = GetChangeChannelActionsTasks(dto.Method, task, workflow);

			if (!noMove)
			{
				MoveTasks(destinationStaff, tasks.ToArray());
			}

			factory.Save();

			return new TryChangeTaskChannelResult();
		}

		void MoveTasks(GlbStaff staff, params ProcessTask[] tasks)
		{
			static void PerformReassignment(ProcessTask task, string channelEntityCode)
			{
				if (task.IsOpenOrAssigned)
				{
					task.P9_GS_NKAssignedStaffMember = channelEntityCode;
				}
			}

			if (WorkflowDataRegistry.Instance.RequireResourceToHaveCapability.Value)
			{
				tasks = CheckResourceCapability(staff, tasks);
			}

			tasks.ForEach(t => PerformReassignment(t, staff.GS_Code));
		}

		bool ShouldAddAssignmentActions(ProcessTask task, int count)
		{
			return count > 0 && !(task.IsOpenOrAssigned && count == 1);
		}

		(IEnumerable<ChangeTaskChannelAction>, IEnumerable<ProcessTask>) GetChangeChannelActionsTasks(TaskChangeChannelMethod method, ProcessTask task, ProcessHeader workflow)
		{
			var actions = new List<ChangeTaskChannelAction>();
			var processTasks = new List<ProcessTask>();

			var openTasks = workflow.Tasks.Where(t => t.IsOpenOrAssigned).ToArray();

			if (task.IsOpenOrAssigned)
			{
				actions.Add(new ChangeTaskChannelAction()
				{
					Method = TaskChangeChannelMethod.SelectedTask,
					Text = Res.GetString("A92A4086-14E8-4E5B-AC61-851F81ECA3D6", "Selected Task"),
					Tooltip = ResString.GetMultilingualString("F3ED1FAF-4CC0-41B9-A5E0-685D60430AEE", "Move only the selected task into another channel.").EnglishText
				});
				if (actions.Last()?.Method == method)
				{
					processTasks.Add(task);
				}
			}

			if (ShouldAddAssignmentActions(task, openTasks.Count(t => t.P9_GS_NKAssignedStaffMember == task.P9_GS_NKAssignedStaffMember)))
			{
				actions.Add(new ChangeTaskChannelAction()
				{
					Method = TaskChangeChannelMethod.TasksAssignedToStaff,
					Text = Res.GetString("3B9E923C-A613-4AC1-9C77-BC1C5EA26DEC", "Tasks Assigned to Resource"),
					Tooltip = ResString.GetMultilingualString("83E6B131-A11C-43AA-B3A7-69B369477343", "Move tasks from this workflow that are not yet started assigned to the resource [{0}] into this channel.", task.StaffName).EnglishText,
					HideFormBeforeFireAction = true,
				});

				if (actions.Last()?.Method == method)
				{
					processTasks.AddRange(workflow.Tasks.Where(t => string.Equals(t.P9_GS_NKAssignedStaffMember, task.P9_GS_NKAssignedStaffMember, StringComparison.CurrentCultureIgnoreCase)));
				}
			}

			if (ShouldAddAssignmentActions(task, openTasks.Count(t => t.RequiresResourceWithCapability && t.P9_G4_RequiredCapability == task.P9_G4_RequiredCapability)))
			{
				var capabilityName = task.CapabilityName;
				actions.Add(new ChangeTaskChannelAction()
				{
					Method = TaskChangeChannelMethod.TasksAssignedToCapability,
					Text = !string.IsNullOrEmpty(capabilityName) ? Res.GetString("08C0EFA9-52AA-4D7E-8FC4-CF4415167B59", "Tasks Assigned to Capability")
						: Res.GetString("A04670B2-23D7-449D-8D97-E2D96989420D", "Tasks with No Capability"),
					Tooltip = string.IsNullOrEmpty(capabilityName) ? ResString.GetMultilingualString("E49A12B3-1824-4C1B-BFDF-02D2C1D24DD5", "Move all tasks from this workflow assigned to the capability [{0}] into this channel.", capabilityName)
						: ResString.GetMultilingualString("EB1D298E-2F7E-4CE6-A686-76FFCD651374", "Move all tasks from this workflow with no capability into this channel."),
				});

				if (actions.Last()?.Method == method)
				{
					processTasks.AddRange(workflow.Tasks.Where(t => t.P9_G4_RequiredCapability == task.P9_G4_RequiredCapability));
				}
			}

			if (ShouldAddAssignmentActions(task, openTasks.Count(t => t.RequiresResourceWithinGroup && t.P9_GG_AssignedGroup == task.P9_GG_AssignedGroup)))
			{
				var groupName = task.GroupName;
				actions.Add(new ChangeTaskChannelAction()
				{
					Method = TaskChangeChannelMethod.TasksAssignedToGroup,
					Text = !string.IsNullOrEmpty(groupName) ? Res.GetString("871E1EBB-A866-440D-A70E-E6692977A033", "Tasks Assigned to Group")
					: Res.GetString("E0236613-79EC-41B1-B476-A6BFFB2AD6D0", "Tasks with No Group"),
					Tooltip = !string.IsNullOrEmpty(groupName) ? ResString.GetMultilingualString("C8C889E6-4018-4487-9402-BA330BC77AD5", "Move all tasks from this workflow assigned to the group [{0}] into this channel.", groupName)
					: ResString.GetMultilingualString("AB95676C-919E-4547-8A3A-ABA85A233A61", "Move all tasks from this workflow with no group into this channel.")
				});

				if (actions.Last()?.Method == method)
				{
					processTasks.AddRange(workflow.Tasks.Where(t => t.P9_GG_AssignedGroup == task.P9_GG_AssignedGroup));
				}
			}

			if (ShouldAddAssignmentActions(task, openTasks.Length))
			{
				actions.Add(new ChangeTaskChannelAction()
				{
					Method = TaskChangeChannelMethod.AllTasksInWorkflow,
					Text = Res.GetString("6CAC28B2-39B0-40DE-9C23-D7AAD242C113", "Entire Workflow"),
					Tooltip = ResString.GetMultilingualString("5421F45A-1F44-45CE-A555-63C4FFD64332", "Move all tasks in the workflow [{0}] to the selected channel.", workflow.FH_CompletionStatement),
					HideFormBeforeFireAction = true,
				});

				if (actions.Last()?.Method == method)
				{
					processTasks.AddRange(workflow.Tasks);
				}
			}

			actions.Add(new ChangeTaskChannelAction()
			{
				Method = TaskChangeChannelMethod.None,
				Text = Res.GetString("B58B995B-F2B3-4C53-9EEF-E81BBE7F09EB", "Cancel"),
				Tooltip = ResString.GetMultilingualString("4DADB780-81B4-4ADE-B8B4-21EF9A994876", "Don't move anything."),
			});

			return (actions, processTasks);
		}

		ProcessTask[] CheckResourceCapability(GlbStaff staff, ProcessTask[] tasks)
		{
			var tasksThatResourceDoesntHaveCapabilityFor = new List<ProcessTask>();

			foreach (var task in tasks)
			{
				if (task.P9_G4_RequiredCapability != ZGuid.Empty && !staff.Capabilities.Contains(task.RequiredCapability))
				{
					tasksThatResourceDoesntHaveCapabilityFor.Add(task);
				}
			}

			if (tasksThatResourceDoesntHaveCapabilityFor.Count != 0)
			{
				var sb = new StringBuilder(Res.GetString("37D2F8D6-DB73-414B-8CC4-D71DD1C2BE0E", "{0} does not have the capability to do the following tasks, and therefore they have not been reassigned:", staff.GS_FullName));
				sb.AppendLine();
				tasksThatResourceDoesntHaveCapabilityFor.ForEach(t => sb.AppendLine(t.HumanReadableName));
			}

			return tasks.Except(tasksThatResourceDoesntHaveCapabilityFor).ToArray();
		}

		static string ValidateDTO(ProcessTask task, ChangeTaskChannelRequestDTO dto, GlbStaff destinationStaff)
		{
			if (task == null)
			{
				return Res.GetString("6185A6CC-11A4-4540-9A50-4E9401050060", "Task not found.");
			}

			if (destinationStaff == null)
			{
				return Res.GetString("8B888A17-F7F8-4820-9CB1-90BD477E723F", "Destination staff is null.");
			}

			if (destinationStaff.GS_Code == task.P9_GS_NKAssignedStaffMember)
			{
				return Res.GetString("C5AA3340-D8C8-4AD7-9B05-E3358AD0C942", "Destination and source staffs are the same");
			}

			if (dto.DestinationChannelEntityPK.IsNullOrEmpty())
			{
				return Res.GetString("03AE5D48-D3C8-4A92-9E73-506F24F95C78", "Destination channel is missing.");
			}

			if (dto.TaskPK == Guid.Empty)
			{
				return Res.GetString("D1C7A996-AE89-4E88-9A82-908306086185", "Task PK is missing.");
			}

			return default;
		}

		#endregion

		#region Containment Barrier

		internal static ContainmentBarrierRequiredDTO CreateContainmentBarrier(ProcessTask task, string newStatus)
		{
			using (var containmentBarrierViewModel = new ContainmentBarrierViewModel(task, newStatus))
			{
				var lookups = new ContainmentBarrierViewModelLookups(containmentBarrierViewModel);
				var reasons = GetReasons(lookups);
				var resourcesUnderReview = GetResourcesUnderReview(lookups, containmentBarrierViewModel.FindBestResourceUnderReview);
				var tasks = GetTasksPreview(task, lookups.ProcessTaskList);
				var possibleOptions = GetPossibleOptions(containmentBarrierViewModel.ValidResponses);

				var containmentBarrierRequired = new ContainmentBarrierRequiredDTO()
				{
					Reasons = reasons,
					ResourcesUnderReview = resourcesUnderReview,
					Tasks = tasks,
					PossibleOptions = possibleOptions,
				};

				// set to new status to calculate actual duration upto current time
				task.P9_Status = newStatus;
				containmentBarrierRequired.ActualDurationInMinutes = decimal.ToInt32(task.ActualDurationHours * 60);

				return containmentBarrierRequired;
			}
		}

		internal static Dictionary<string, string> GetReasons(ContainmentBarrierViewModelLookups lookups)
		{
			return lookups
				.IterationReasonsRegistryLists
				.Cast<WorkflowIterationReason>()
				.ToDictionary(reason => reason.Code.ToString(), reason => reason.Description.ToString());
		}

		internal static IEnumerable<ResourceUnderReviewDTO> GetResourcesUnderReview(ContainmentBarrierViewModelLookups lookups, Func<ZString> findBestResourceUnderReview)
		{
			var resourcesUnderReview = lookups
				.ResourceUnderReviewList
				.Cast<GlbStaff>()
				.Select(staff => new ResourceUnderReviewDTO()
				{
					PK = staff.PK.ToGuid(),
					Code = staff.GS_Code,
					Name = staff.GS_FullName,
				})
				.ToList();

			var bestResourceUnderReview = resourcesUnderReview
				.FirstOrDefault(resource => resource.Code.Equals(findBestResourceUnderReview(), StringComparison.OrdinalIgnoreCase));

			if (bestResourceUnderReview != null && resourcesUnderReview.FirstOrDefault()?.PK != bestResourceUnderReview.PK)
			{
				resourcesUnderReview.Remove(bestResourceUnderReview);
				resourcesUnderReview.Insert(0, bestResourceUnderReview);
			}

			return resourcesUnderReview;
		}

		internal static IEnumerable<ContainmentBarrierTaskPreviewDTO> GetTasksPreview(IProcessTask containmentBarrierTask, ProcessTaskFriendlyViewCollectionView tasks)
		{
			var factory = tasks.Factory;
			var taskPKs = tasks
				.Select(processTaskFriendlyView => processTaskFriendlyView.PK)
				.Append(containmentBarrierTask.PK);

			return factory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.PK, taskPKs))
				.Select(processTask => new ContainmentBarrierTaskPreviewDTO()
				{
					PK = processTask.PK.ToGuid(),
					Description = processTask.P9_Description,
					ResourceName = processTask.StaffName,
					Sequence = processTask.P9_Sequence,
					WorkflowDescription = processTask.ProcessHeader?.FH_CompletionStatement
				})
				.OrderBy(task => task.PK == containmentBarrierTask.PK) // place all non containment barrier tasks first before the containment barrier task
				.ThenBy(task => task.Sequence);
		}

		internal static IEnumerable<ContainmentBarrierResponse> GetPossibleOptions(ContainmentBarrierResponses validResponses)
		{
			var possibleOptions = new List<ContainmentBarrierResponse>(5)
			{
				ContainmentBarrierResponse.Passed,
				ContainmentBarrierResponse.Canceled,
				ContainmentBarrierResponse.IterationRequired
			};

			if (validResponses.HasFlag(ContainmentBarrierResponses.AcceptIterationCreatedByOtherResource))
			{
				possibleOptions.Add(ContainmentBarrierResponse.AcceptIterationCreatedByOtherResource);
			}

			if (validResponses.HasFlag(ContainmentBarrierResponses.DeferredToAnotherResource))
			{
				possibleOptions.Add(ContainmentBarrierResponse.DeferredToAnotherResource);
			}

			return possibleOptions;
		}

		static void CommitResponse(IProcessTask task, ContainmentBarrierRequestDTO containmentBarrierDTO)
		{
			var response = GetContainmentBarrierResponse(containmentBarrierDTO.Response);

			using (var containmentBarrierViewModel = new ContainmentBarrierViewModel(task, ProcessTaskStatusCodeList.Codes.Closed))
			{
				containmentBarrierViewModel.Response = response;

				if (containmentBarrierDTO.Response == ContainmentBarrierResponse.IterationRequired)
				{
					CreateIteration(containmentBarrierViewModel, containmentBarrierDTO);
				}

				containmentBarrierViewModel.CommitResponse();
			}
		}

		internal static ContainmentBarrierResponses GetContainmentBarrierResponse(ContainmentBarrierResponse response)
		{
			return (ContainmentBarrierResponses)Enum.Parse(typeof(ContainmentBarrierResponses), Enum.GetName(typeof(ContainmentBarrierResponse), response));
		}

		internal static void CreateIteration(ContainmentBarrierViewModel containmentBarrierViewModel, ContainmentBarrierRequestDTO containmentBarrierDTO)
		{
			var lookups = new ContainmentBarrierViewModelLookups(containmentBarrierViewModel);
			var iterateReasonPK = lookups
				.IterationReasonsRegistryLists
				.Cast<RegistryBusinessObject>()
				.First(reason => containmentBarrierDTO.ReasonCode.Equals(reason.Code, StringComparison.OrdinalIgnoreCase))
				.PK;

			containmentBarrierViewModel.IterateFromTaskPK = containmentBarrierDTO.SelectedTaskPKs.First();
			containmentBarrierViewModel.IterateReasonPK = iterateReasonPK;
			containmentBarrierViewModel.ResourceUnderReviewNK = containmentBarrierDTO.ResourceCodeUnderReview;

			foreach (var taskPreview in containmentBarrierViewModel.IterationTaskPreviews.Cast<TaskPreviewCopy>())
			{
				taskPreview.IncludeInIteration = containmentBarrierDTO.SelectedTaskPKs.Any(selectedTaskPK => selectedTaskPK == taskPreview.Task.PK);
			}
		}

		#endregion

		#region Estimates and Durations

		public bool TryUpdateEstimates(Guid taskId, UpdateTaskEstimatesRequest updateRequest, out PaveError error)
		{
			error = null;
			var factory = CreateFactory();
			var task = factory.Load<ProcessTask>(taskId);

			if (updateRequest.PreviousHash == null)
			{
				throw new ArgumentNullException(nameof(updateRequest), nameof(updateRequest.PreviousHash));
			}

			if (updateRequest.NewEstimateFactor == null || updateRequest.NewLowEstimate == null)
			{
				throw new ArgumentNullException(nameof(updateRequest), nameof(updateRequest.PreviousHash));
			}

			string validationError = null;

			if (updateRequest.NewLowEstimate < 0 || updateRequest.NewLowEstimate > 999 * 60)
			{
				validationError = Res.GetString("DD818C7D-72CF-4F26-953D-52D4453EB55E", "Low estimate must be between 0 and 999 hours.");
			}

			if (updateRequest.NewEstimateFactor <= 0)
			{
				validationError = Res.GetString("915FC6D8-B872-453A-A360-F63227680000", "Estimation factor must be a positive number.");
			}

			if (validationError != null)
			{
				error = new PaveError(PaveErrorCode.ValidationError, validationError.WrapWithEnumerable());
				return false;
			}

			var estimateFactor = (int)task.P9_EstimateVariationFactor;
			int? lowEstimate = null;
			if (task.P9_EstDuration != ZDateTime.Empty)
			{
				lowEstimate = (int)(task.P9_EstDuration - new ZDateTime(task.P9_EstDuration.Year, 1, 1)).TotalMinutes;
			}

			var jsonString = JsonConvert.SerializeObject(new
			{
				LowEstimate = lowEstimate,
				EstimateFactor = estimateFactor,
			});
			var hash = HashHelper.GetHash(jsonString);

			if (hash != updateRequest.PreviousHash)
			{
				error = new PaveError(PaveErrorCode.ValidationError, new string[] { LocalizationHelper.WorkflowTaskReordering.FieldChangedByAnotherUserMessage });
				return false;
			}

			task.P9_EstimateVariationFactor = new ZDecimal(updateRequest.NewEstimateFactor);
			task.P9_EstDuration = new ZDateTime(2024, 1, 1).AddMinutes(updateRequest.NewLowEstimate ?? 0);
			factory.Save();

			return true;
		}

		public bool TryUpdateActualDuration(Guid taskId, UpdateActualDurationRequest updateRequest, out BusinessResponse businessResponse)
		{
			if (string.IsNullOrWhiteSpace(updateRequest.PreviousHash))
			{
				businessResponse = BusinessResponse.Build(TaskService.BusinessMessages.PreviousHashIsRequiredToUpdateActualDuration);
				return false;
			}

			if (updateRequest.NewActualDuration is null or < 0)
			{
				businessResponse = BusinessResponse.Build(BusinessMessages.ActualDurationInMinutesMustBeGreaterThanOrEqualsToZero);
				return false;
			}

			var factory = CreateFactory();
			var taskToUpdate = factory.Load<ProcessTask>(taskId);

			if (!TaskUtil.ValidateTask(taskToUpdate, out _, out var secondaryBusinessMessages))
			{
				businessResponse = BusinessResponse.Build(BusinessMessages.ErrorUpdatingActualDuration, secondaryBusinessMessages);
				return false;
			}

			if (!CheckUpdateActualDurationCoreRules(taskToUpdate, updateRequest.NewActualDuration, out businessResponse))
			{
				return false;
			}

			int? currentActualDuration = taskToUpdate.P9_ActualDuration != ZDateTime.Empty ? (int)(taskToUpdate.P9_ActualDuration - new ZDateTime(taskToUpdate.P9_ActualDuration.Year, 1, 1)).TotalMinutes : null;
			var currentActualDurationHash = HashHelper.GetHash(currentActualDuration.ToString());

			if (currentActualDurationHash != updateRequest.PreviousHash)
			{
				businessResponse = BusinessResponse.Build(BusinessMessages.ActualDurationWasChangedByAnotherUser);
				return false;
			}

			taskToUpdate.P9_ActualDuration = TimeSpan.FromMinutes(updateRequest.NewActualDuration ?? 0);

			factory.Save();

			return true;
		}

		internal static bool CheckUpdateActualDurationCoreRules(ProcessTask task, int? newActualDurationInMinutes, out BusinessResponse businessResponse)
		{
			businessResponse = null;

			bool isRequireActualDuration = WorkflowDataRegistry.Instance.TaskTypes.Value
				.GetTaskTypesFromWorkflowCode(task.WorkflowType)
				.OfType<WorkflowTaskType>()
				.SingleOrDefault(w => w.Code == task.P9_Type)?.IsRequireActualDuration ?? false;

			if (isRequireActualDuration && newActualDurationInMinutes is null)
			{
				businessResponse = BusinessResponse.Build(TaskService.BusinessMessages.ActualDurationIsMandatoryForThisTask);
				return false;
			}

			if (newActualDurationInMinutes is < 0)
			{
				businessResponse = BusinessResponse.Build(BusinessMessages.ActualDurationInMinutesMustBeGreaterThanOrEqualsToZero);
				return false;
			}

			return true;
		}

		#endregion

		#region Assignment

		public bool TryAssignToCapability(Guid taskId, AssignToCapabilityRequest dto, out BusinessResponse businessResponse)
		{
			var factory = CreateFactory();

			if (!TryLoadTask(taskId, factory, throwIfNotFound: false, out var task, out businessResponse))
			{
				return false;
			}

			var capability = factory.Load<GlbCapability>(dto.CapabilityId);

			if (capability == null)
			{
				businessResponse = BusinessResponse.Build(WiseTech.Business.HumanResourcesManagement.BusinessMessages.CapabilityNotFound);
				return false;
			}

			task.P9_G4_RequiredCapability = dto.CapabilityId;

			if (!TaskUtil.ValidateTask(task, out _, out var secondaryBusinessMessages))
			{
				businessResponse = BusinessResponse.Build(BusinessMessages.ErrorAssigningCapability, secondaryBusinessMessages);
				return false;
			}

			factory.Save();

			return true;
		}

		public bool TryDeleteAssignment(Guid taskId, out PaveError error)
		{
			error = null;
			var factory = CreateFactory();
			var task = factory.Load<ProcessTask>(taskId);
			var validationErrors = ValidateEnumerable(ValidateDto(task));

			if (!validationErrors.IsNullOrEmpty())
			{
				error = new PaveError(PaveErrorCode.ValidationError, validationErrors);
				return false;
			}

			task.P9_GS_NKAssignedStaffMember = string.Empty;
			task.P9_G4_RequiredCapability = Guid.Empty;

			UpdateTaskStatus(task);

			validationErrors = ValidateTask(task);
			if (validationErrors != null && validationErrors.Any())
			{
				error = new PaveError(PaveErrorCode.ValidationError, validationErrors);
				return false;
			}

			factory.Save();

			return true;
		}

		public bool TryDeleteAssignmentCapability(Guid taskId, out PaveError error)
		{
			error = null;
			var factory = CreateFactory();
			var task = factory.Load<ProcessTask>(taskId);
			var validationErrors = ValidateEnumerable(ValidateDto(task));

			if (!validationErrors.IsNullOrEmpty())
			{
				error = new PaveError(PaveErrorCode.ValidationError, validationErrors);
				return false;
			}

			task.P9_G4_RequiredCapability = Guid.Empty;
			UpdateTaskStatus(task);

			validationErrors = ValidateTask(task);
			if (validationErrors != null && validationErrors.Any())
			{
				error = new PaveError(PaveErrorCode.ValidationError, validationErrors);
				return false;
			}

			factory.Save();

			return true;
		}

		public bool TryDeleteAssignmentStaff(Guid taskId, out PaveError error)
		{
			error = null;
			var factory = CreateFactory();
			var task = factory.Load<ProcessTask>(taskId);
			var validationErrors = ValidateEnumerable(ValidateDto(task));

			if (!validationErrors.IsNullOrEmpty())
			{
				error = new PaveError(PaveErrorCode.ValidationError, validationErrors);
				return false;
			}

			task.P9_GS_NKAssignedStaffMember = string.Empty;
			UpdateTaskStatus(task);

			validationErrors = ValidateTask(task);
			if (validationErrors != null && validationErrors.Any())
			{
				error = new PaveError(PaveErrorCode.ValidationError, validationErrors);
				return false;
			}

			factory.Save();

			return true;
		}

		public bool TryDeleteTask(Guid taskId, out PaveError error)
		{
			error = null;
			var factory = CreateFactory();
			var task = factory.Load<ProcessTask>(taskId);
			var validationErrors = ValidateEnumerable(ValidateDto(task));

			if (!validationErrors.IsNullOrEmpty())
			{
				error = new PaveError(PaveErrorCode.ValidationError, validationErrors);
				return false;
			}

			task.Delete();

			factory.Save();

			return true;
		}

		static void UpdateTaskStatus(ProcessTask task) =>
			task.P9_Status = task.P9_GS_NKAssignedStaffMember.IsEmpty ? ProcessTaskStatusCodeList.Codes.Open : task.P9_Status;
		static string ValidateDto(ProcessTask task)
		{
			var result = default(string);
			if (task == null)
			{
				result = Res.GetString("7C22F49D-7581-45DB-9643-9869BDCDA86C", "Task not found.");
			}
			else if (!task.P9_FormFlowType.IsEmpty)
			{
				result = Res.GetString("4ac04539-eff4-4530-aa06-36f04e9501a1", "Cannot delete system maintained tasks linked to jobs.");
			}
			return result;
		}

		static string ValidateDto(ProcessHeader processHeader) =>
			processHeader == null ? Res.GetString("a596b81e-193a-44c8-8560-0bc138d50c38", "Workflow not found.") : default;

		static IEnumerable<string> ValidateEnumerable(params string[] errors) =>
			errors.Where(e => !e.IsNullOrEmpty());

		#endregion

		#region Notes

		public TaskNotesResponse GetNotes(Guid taskId)
		{
			var factory = new BusinessObjectFactory() { NameForDebugging = nameof(WorkItemService), RefreshEnabled = false };
			var task = factory.Load<IProcessTask>(taskId);
			var (hash, notes) = CommonServiceHelper.BlobToHtmlWithHash(task.P9_Notes);

			return new TaskNotesResponse()
			{
				Hash = hash,
				Content = notes,
			};
		}

		public TryUpdateNotesResult TryUpdateNotes(Guid taskId, UpdateTaskNotesRequest updateRequest)
		{
			var factory = CreateFactory();
			var task = factory.Load<ProcessTask>(taskId);

			if (string.IsNullOrWhiteSpace(updateRequest.PreviousHash))
			{
				throw new ArgumentNullException(nameof(updateRequest), nameof(updateRequest.PreviousHash));
			}

			if (updateRequest.NewNotes == null)
			{
				throw new ArgumentNullException(nameof(updateRequest), nameof(updateRequest.NewNotes));
			}

			var hash = HashHelper.GetHash(task.P9_Notes.ToUTF8());

			if (hash != updateRequest.PreviousHash)
			{
				return new TryUpdateNotesResult(LocalizationHelper.WorkflowTaskReordering.FieldChangedByAnotherUserMessage.WrapWithEnumerable());
			}

			task.P9_Notes_HTML = ZBlob.FromUTF8(updateRequest.NewNotes);

			factory.Save();

			var (newHash, notes) = task.P9_Notes.BlobToHtmlWithHash();

			return new TryUpdateNotesResult(new TaskNotesResponse { Hash = newHash, Content = notes });
		}
		#endregion

		#region Type And Notes
		public bool TryUpdateTypeAndNotes(Guid taskId, UpdateTypeAndNotesRequest updateRequest, out PaveError error)
		{
			if (updateRequest.PreviousType == null)
			{
				throw new ArgumentNullException(nameof(updateRequest), nameof(updateRequest.PreviousType));
			}

			if (updateRequest.NewType == null)
			{
				throw new ArgumentNullException(nameof(updateRequest), nameof(updateRequest.NewType));
			}

			var factory = CreateFactory();
			var task = factory.Load<ProcessTask>(taskId);

			if (task == null)
			{
				error = new PaveError(PaveErrorCode.Other, new string[] { LocalizationHelper.WorkflowTaskReordering.RecordNotFoundMessage });
				return false;
			}

			if (task.P9_Type != updateRequest.PreviousType)
			{
				error = new PaveError(PaveErrorCode.ValidationError, new string[] { LocalizationHelper.WorkflowTaskReordering.FieldChangedByAnotherUserMessage });
				return false;
			}

			task.P9_Type = updateRequest.NewType.ToUpper();
			task.P9_Notes_HTML = ZBlob.FromUTF8(updateRequest.Notes);

			var validationErrors = ValidateTask(task);
			if (validationErrors != null && validationErrors.Length != 0)
			{
				error = new PaveError(PaveErrorCode.ValidationError, validationErrors);
				return false;
			}

			factory.Save();

			error = null;
			return true;
		}

		#endregion

		#region Add new task

		public TryAddNewTaskResult TryAddNewTask(NewTaskRequest dto, out PaveError error)
		{
			error = null;
			try
			{
				var factory = CreateFactory();
				var workflow = factory.Load<ProcessHeader>(dto.WorkflowId);
				var validationErrors = ValidateEnumerable(ValidateDto(workflow));

				if (validationErrors != null && validationErrors.Any())
				{
					error = new PaveError(PaveErrorCode.ValidationError, validationErrors);
					return new TryAddNewTaskResult(validationErrors);
				}

				var task = workflow.Parent.WorkflowItems.AddNew();
				task.P9_Description = dto.Description;
				task.P9_Type = dto.Type;
				task.P9_Sequence = dto.RelatedTaskId != null ? factory.Load<ProcessTask>((Guid)dto.RelatedTaskId).P9_Sequence : workflow.Tasks.Max(t => t.P9_Sequence) + 100;
				task.CapabilityCode = dto.CapabilityCode ?? new ZString();
				task.P9_GS_NKAssignedStaffMember = dto.StaffId.HasValue ? factory.Load<GlbStaff>((ZGuid)dto.StaffId).GS_Code : task.P9_GS_NKAssignedStaffMember;

				if (!dto.CapabilityCode.IsNullOrEmpty() && task.P9_G4_RequiredCapability == Guid.Empty)
				{
					var message = Res.GetString("1B419006-B66E-410A-9517-C9EE3D1AF53D", "Error creating a new task.");
					error = new PaveError(PaveErrorCode.Other, new string[] { message });
					return new TryAddNewTaskResult(new string[] { message });
				}

				UpdateTaskStatus(task);

				task.P9_EstimateVariationFactor = dto.EstimateFactor ?? 2;
				task.P9_EstDuration = new ZInt(dto.LowEstimate ?? 0).GetDateTimeFromMinutes();
				task.P9_FH_ProcessHeader = dto.WorkflowId;
				task.P9_ActualDuration = new ZInt(dto.ActualDuration ?? 0).GetDateTimeFromMinutes();

				if (!string.IsNullOrEmpty(dto.Notes))
				{
					task.P9_Notes_HTML = ZBlob.FromUTF8(dto.Notes);
				}

				validationErrors = ValidateTask(task);
				if (validationErrors != null && validationErrors.Any())
				{
					error = new PaveError(PaveErrorCode.ValidationError, validationErrors);
					return new TryAddNewTaskResult(validationErrors);
				}

				factory.Save();

				return new TryAddNewTaskResult(task.PK.ToGuid());
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				var message = Res.GetString("1B419006-B66E-410A-9517-C9EE3D1AF53D", "Error creating a new task.");
				ErrorReporter.ReportOnce("EE75EB30-FAEC-44B7-B580-46C679E4F054", message, ex);
				error = new PaveError(PaveErrorCode.Other, new string[] { message });
				return new TryAddNewTaskResult(new string[] { message });
			}
		}

		#endregion

		#region Delete a task

		public bool TryDelete(Guid taskId, DeleteTaskRequest deleteTaskRequest, out BusinessResponse businessResponse)
		{
			var factory = CreateFactory();

			if (!TryLoadTask(taskId, factory, throwIfNotFound: false, out var task, out businessResponse))
			{
				return false;
			}

			if (task.P9_TaskCannotBeDeleted == true)
			{
				businessResponse = BusinessResponse.Build(BusinessMessages.MandatoryTaskCantBeDeleted);
				return false;
			}

			if (!string.IsNullOrEmpty(task.P9_FormFlowType))
			{
				businessResponse = BusinessResponse.Build(BusinessMessages.TaskWithFormFlowTypeCannotBeDeleted);
				return false;
			}

			task.Delete();

			factory.Save();

			return true;
		}

		#endregion

		#region Task Description

		public TryUpdateTaskDescriptionResult TryUpdateTaskDescription(Guid taskId, UpdateTaskDescriptionRequest dto)
		{
			var factory = CreateFactory();
			var task = factory.Load<ProcessTask>(taskId);

			if (dto.PreviousHash.IsNullOrEmpty())
			{
				throw new ArgumentException(nameof(dto.PreviousHash));
			}

			if (dto.NewDescription.IsNullOrEmpty())
			{
				throw new ArgumentException(nameof(dto.NewDescription));
			}

			var hash = HashHelper.GetHash(task.P9_Description);

			if (dto.PreviousHash != hash)
			{
				return new TryUpdateTaskDescriptionResult(LocalizationHelper.WorkflowTaskReordering.FieldChangedByAnotherUserMessage.WrapWithEnumerable());
			}

			task.P9_Description = dto.NewDescription;
			factory.Save();

			return new TryUpdateTaskDescriptionResult(new TaskDescriptionResponse()
			{
				Hash = HashHelper.GetHash(dto.NewDescription),
				Description = dto.NewDescription,
			});
		}

		#endregion

		#region Validation

		static string[] ValidateTask(ProcessTask task)
		{
			task.Validation.ValidateAll();

			if (!task.HasErrors)
			{
				return default;
			}

			return task.PropertiesWithNotifications
				.SelectMany(propertyInfo => propertyInfo.Notifications.Where(notification => notification.Type == NotificationType.Error))
				.Distinct()
				.Select(notification => notification.Message)
				.ToArray();
		}

		#endregion

		#region Helpers

		internal virtual BusinessObjectFactory CreateFactory() => new() { NameForDebugging = nameof(WorkItemService), RefreshEnabled = false };

		internal static bool TryLoadTask(Guid taskId, BusinessObjectFactory factory, bool throwIfNotFound, out ProcessTask task, out BusinessResponse businessResponse)
		{
			businessResponse = null;

			if (!TryLoadTask(taskId, factory, throwIfNotFound, out task))
			{
				businessResponse = BusinessResponse.Build(WiseTech.Business.WorkflowManagement.BusinessMessages.TaskNotFound);
				return false;
			}

			return true;
		}

		internal static bool TryLoadTask(Guid taskId, BusinessObjectFactory factory, bool throwIfNotFound, out ProcessTask task)
		{
			task = factory.Load<ProcessTask>(taskId);

			if (task == null && throwIfNotFound)
			{
				throw new KeyNotFoundException($"A task with id {taskId} was not found.");
			}

			return task != null;
		}

		#endregion
	}
}
