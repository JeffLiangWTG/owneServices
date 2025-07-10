using System;
using CargoWise.PAVE.Common.DTO;
using Enterprise.BufferManagement.Service.Shared.Task;
using Enterprise.BufferManagement.Service.Shared.Task.Dtos;
using Enterprise.BufferManagement.Service.Shared.Task.Dtos.Lifecycle;
using WiseTech.Business.Core;

namespace Enterprise.BufferManagement.Service.Shared
{
	public interface ITaskService
	{
		bool TryUpdateType(Guid taskId, UpdateTaskTypeRequest updateRequest, out BusinessResponse businessResponse);

		TryChangeTaskChannelResult TryChangeChannel(ChangeTaskChannelRequestDTO dto, bool noMove);

		bool TryUpdateEstimates(Guid taskId, UpdateTaskEstimatesRequest updateRequest, out PaveError error);

		bool TryAssignToCapability(Guid taskId, AssignToCapabilityRequest dto, out BusinessResponse businessResponse);

		bool TryDeleteAssignment(Guid taskId, out PaveError error);

		bool TryDeleteAssignmentCapability(Guid taskId, out PaveError error);

		bool TryDeleteAssignmentStaff(Guid taskId, out PaveError error);

		TryAddNewTaskResult TryAddNewTask(NewTaskRequest dto, out PaveError error);

		TaskNotesResponse GetNotes(Guid taskId);

		TryUpdateNotesResult TryUpdateNotes(Guid taskId, UpdateTaskNotesRequest updateRequest);

		bool TryDeleteTask(Guid taskId, out PaveError error);

		bool TryUpdateActualDuration(Guid taskId, UpdateActualDurationRequest updateRequest, out BusinessResponse businessResponse);

		bool TryDelete(Guid taskId, DeleteTaskRequest deleteTaskRequest, out BusinessResponse businessResponse);

		bool TryUpdateTypeAndNotes(Guid taskId, UpdateTypeAndNotesRequest updateRequest, out PaveError error);

		TryUpdateTaskDescriptionResult TryUpdateTaskDescription(Guid taskId, UpdateTaskDescriptionRequest dto);
	}
}
