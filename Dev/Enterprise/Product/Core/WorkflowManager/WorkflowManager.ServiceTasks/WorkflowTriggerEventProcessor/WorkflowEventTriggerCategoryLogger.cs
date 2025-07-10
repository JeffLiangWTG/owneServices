using System;
using CargoWise.Common;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.WorkflowManager.ServiceTasks
{
	public enum WorkflowEventTriggerCategories
	{
		DelayedLog,
		Default
	}

	class WorkflowEventTriggerCategoryLogger : ICategoryLogger<WorkflowEventTriggerCategories>
	{
		readonly ILogger logger;
		readonly CodeDescriptionBoolCollection registry = SystemDataRegistry.Instance.WorkflowEventTriggerProccessorLogging.Value;

		public WorkflowEventTriggerCategoryLogger(ILogger logger)
		{
			this.logger = Argument.NotNull(logger, nameof(logger));
		}

		public string GetCategoryName(WorkflowEventTriggerCategories category)
		{
			switch (category)
			{
				case WorkflowEventTriggerCategories.DelayedLog:
					return nameof(WorkflowEventTriggerCategories.DelayedLog);
				case WorkflowEventTriggerCategories.Default:
					return nameof(WorkflowEventTriggerCategories.Default);
				default:
					return (NoResString)"unknown";
			}
		}

		public string GetContext()
		{
			return ProcessTask.WorkflowEventTriggerJobQueueName;
		}

		public WorkflowEventTriggerCategories GetDefaultCategory()
		{
			return WorkflowEventTriggerCategories.Default;
		}

		public ILogger GetLogger()
		{
			return logger;
		}

		public bool ShouldLog(WorkflowEventTriggerCategories category)
		{
			switch (category)
			{
				case WorkflowEventTriggerCategories.DelayedLog:
					return registry.GetBoolFromCode(SystemDataRegistry.WorkflowEventTriggerLoggingKeys.DelayedLog);
				case WorkflowEventTriggerCategories.Default:
				default:
					return true;
			}
		}

		void ILogger.Log(LogType type, string message)
		{
			this.Log(type, message);
		}

		void ILogger.Log(LogType type, string message, Exception ex)
		{
			this.Log(type, message, ex);
		}
	}
}
