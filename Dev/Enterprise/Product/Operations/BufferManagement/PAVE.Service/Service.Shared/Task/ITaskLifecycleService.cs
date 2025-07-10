using System;
using Enterprise.BufferManagement.Service.Shared.Task.Dtos.Lifecycle;
using WiseTech.Business.Core;

namespace Enterprise.BufferManagement.Service.Shared
{
	public interface ITaskLifecycleService
	{
		bool TryStart(Guid taskId, StartTaskRequest startTaskRequest, out BusinessResponse businessResponse);

		bool TrySuspend(Guid taskId, SuspendTaskRequest suspendTaskRequest, out BusinessResponse businessResponse);

		bool TryResume(Guid taskId, ResumeTaskRequest resumeTaskRequest, out BusinessResponse businessResponse);

		bool TryCancel(Guid taskId, CancelTaskRequest cancelTaskRequest, out BusinessResponse businessResponse);

		bool TryAssign(Guid taskId, AssignTaskRequest assignTaskRequest, out BusinessResponse businessResponse);

		bool TryClaim(Guid taskId, ClaimTaskRequest claimAndStartTaskRequest, out BusinessResponse businessResponse);

		bool TryClaimAndStart(Guid taskId, ClaimAndStartTaskRequest claimAndStartTaskRequest, out BusinessResponse businessResponse);

		bool TryComplete(Guid taskId, CompleteTaskRequest completeTaskRequest, out BusinessResponse businessResponse);

		bool TryCheckBeforeComplete(Guid taskId, out TaskCompletionCheckResponse taskCompletionCheckResponse, out BusinessResponse businessResponse);

		bool TryReopen(Guid taskId, ReopenTaskRequest reopenTaskRequest, out BusinessResponse businessResponse);
	}
}
