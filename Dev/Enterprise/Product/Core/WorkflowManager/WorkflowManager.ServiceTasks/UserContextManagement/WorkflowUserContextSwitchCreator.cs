using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.LogWalker;
using Enterprise.MasterFiles.Business;

namespace Enterprise.WorkflowManager.ServiceTasks
{
	public static class WorkflowUserContextSwitchCreator
	{
		public static UserContextSwitcher GetTemporaryUserContext(
			IBaseTrigger trigger,
			BusinessObject job,
			IWorkflowTriggerSource log,
			INotifications notifications)
		{
			Argument.NotNull(trigger, nameof(trigger));
			Argument.NotNull(log, nameof(log));
			Argument.NotNull(notifications, nameof(notifications));

			var userContext = GetWorkflowUserContext(trigger, job, log, notifications);
			var triggerContext = WorkflowUserContextDecider.GetTriggerContext(trigger, log, notifications);
			return UserContextSwitcher.Build(userContext, triggerContext.Message);
		}

		static WorkflowUserContext GetWorkflowUserContext(IBaseTrigger trigger, BusinessObject job, IWorkflowTriggerSource log, INotifications notifications)
		{
			var userContext = WorkflowUserContext.Create(log);
			if (userContext == null || userContext.IsNull() || userContext.Staff?.GS_IsActive == false)
			{
				return WorkflowUserContextDecider.GetTemporaryUserContext(trigger, job, log, notifications);
			}
			else
			{
				return userContext;
			}
		}
	}
}
