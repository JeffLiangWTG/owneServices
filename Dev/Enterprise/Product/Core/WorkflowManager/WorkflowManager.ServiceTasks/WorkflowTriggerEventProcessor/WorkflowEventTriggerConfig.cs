using System;
using CargoWise.Common;
using Enterprise.Registry.Business;

namespace Enterprise.WorkflowManager.ServiceTasks
{
	interface IWorkflowEventTriggerConfig
	{
		TimeSpan LogAllowedLag { get; }
	}

	partial class WorkflowEventTriggerConfig : IWorkflowEventTriggerConfig
	{
		public static IWorkflowEventTriggerConfig Create()
		{
			var result = (IWorkflowEventTriggerConfig)new WorkflowEventTriggerConfig();
			HookConfigForTesting(ref result);
			return result;
		}

		WorkflowEventTriggerConfig()
		{
			LogAllowedLag = new TimeSpan(
				0,
				SystemDataRegistry.Instance.WorkflowEventTriggerProccessorDelayLogTime.Value,
				0);
		}

		public TimeSpan LogAllowedLag { get; }

		static partial void HookConfigForTesting(ref IWorkflowEventTriggerConfig workflowEventTriggerConfig);
	}

#region Test

#if DEBUG

	partial class WorkflowEventTriggerConfig : IWorkflowEventTriggerConfig
	{
		static partial void HookConfigForTesting(ref IWorkflowEventTriggerConfig workflowEventTriggerConfig)
		{
			if (TestWorkflowEventTriggerConfig.GetWorkflowEventTriggerConfigForTest() != null)
			{
				workflowEventTriggerConfig = TestWorkflowEventTriggerConfig.GetWorkflowEventTriggerConfigForTest();
			}
		}
	}

	class TestWorkflowEventTriggerConfig : IWorkflowEventTriggerConfig
	{
		static readonly Overridable<IWorkflowEventTriggerConfig> OverridableWorkflowEventTriggerConfig = new Overridable<IWorkflowEventTriggerConfig>(null);

		public static void SetWorkflowEventTriggerConfigForTest(IWorkflowEventTriggerConfig workflowEventTriggerConfig)
		{
			OverridableWorkflowEventTriggerConfig.Value = workflowEventTriggerConfig;
		}

		public static IWorkflowEventTriggerConfig GetWorkflowEventTriggerConfigForTest()
		{
			return OverridableWorkflowEventTriggerConfig.Value;
		}

		public TestWorkflowEventTriggerConfig(TimeSpan logAllowedLag)
		{
			LogAllowedLag = logAllowedLag;
		}

		public TimeSpan LogAllowedLag { get; set; }
	}

#endif

#endregion
}

