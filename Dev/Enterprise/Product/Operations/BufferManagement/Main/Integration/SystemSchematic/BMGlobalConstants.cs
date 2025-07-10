using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.Integration
{
	public static class BMGlobalConstants
	{
		public static ZString DefaultJobWorkflowCompletionStatement => Res.GetString("63ce8522-136d-4f7c-987c-a2cce7dd0eaa", "Job is complete.");

		public static string DefaultWorkflowCompletionStatement => Res.GetString("68bbd937-3acf-40fe-a67c-15e94208dee2", "Job Workflow");

		public static ZGuid DefaultWorkflowTemplateID => ZGuid.ParseSafe("abcabcab-bead-b00f-bead-abcabcabcabc");

		public static MultilingualString BufferManagementCategoryDescription => ResString.GetMultilingualString("ModuleFilter|Common|WorkflowFilterCategory|BufferManagement", "Buffer Management");

		public const string CurrentTasksSQLFunctionText = "dbo.GetCurrentTasks";

		public const string CurrentTasksIgnoringIterationsSQLFunctionText = "dbo.GetCurrentTasksIgnoringIterations";

		public const string CurrentTasksInWorkflowsSQLFunctionText = "dbo.GetCurrentTasksInWorkflows";

		public const string CurrentTasksInWorkflowsIgnoringIterationsSQLFunctionText = "dbo.GetCurrentTasksInWorkflowsIgnoringIterations";
	}
}
