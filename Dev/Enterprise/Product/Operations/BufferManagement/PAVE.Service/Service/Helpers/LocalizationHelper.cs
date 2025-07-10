namespace Enterprise.BufferManagement.Service.Helpers
{
	public static class LocalizationHelper
	{
		public static class WorkflowTaskReordering
		{
			public static string FieldChangedByAnotherUserMessage => Res.GetString("E7C82E58-7A9C-4AF6-9AEE-359CFDEF0CE7", "The field was changed by another user. Please refresh data.");

			public static string RecordNotFoundMessage => Res.GetString("676A25B9-8180-447A-AD06-B9DE4D76443E", "Record not found.");

			public static string TaskSequenceChangedByAnotherUserMessage => Res.GetString("84CF5684-0BEB-41CE-874C-EC788413F740", "The task sequence was changed by another user. Please refresh data.");

			public static string TaskSequenceRequiresPreviousOrNextMessage => Res.GetString("F0815E00-4635-4282-BE9D-D99FCA223154", "The task sequence reordering requires previous or next task.");

			public static string TaskSequenceIsTheSameMessage => Res.GetString("30FBF822-98E6-4861-9037-24EED9334493", "The workflow task sequence is the same.");

			public static string TaskSequenceBelongsToGroupMessage => Res.GetString("74D60376-6C97-4D63-99F5-0CF0D389C446", "This task belongs to a group and can't be reordered individually.");

			public static string PreviousAndNextMustBeDifferentMessage => Res.GetString("0A559E7F-0083-4BAE-B642-9B8E1E540523", "Previous and Next tasks must be different.");

			public static string PreviousAndNextMustBeInSequentialOrderMessage => Res.GetString("28B00128-6904-4101-A32D-76A2DE21C45D", "Previous and Next must be in sequential order.");

			public static string PreviousMustBeTheLastInPreviousGroup => Res.GetString("4E866719-EB97-4363-AC03-0E9622B2DCD3", "Previous task must be the last of the previous group.");

			public static string NextMustBeTheFirstInNextGroup => Res.GetString("F800DF64-3887-4901-88D9-5407E661C852", "Next task must be the first of the next group.");

			public static string TaskReorderingRequiresAtLeastTwoTasks => Res.GetString("F35790A1-526A-45C0-A55B-4AE3B8776146", "Task reorder requires at least two tasks in the workflow.");

			public static string TaskGroupingRequiresAtLeastTwoTasksWithDifferentSequenceNumber => Res.GetString("E1D3ECD9-59FF-4F4D-AE77-C7E9999527B5", "Task grouping requires at leas two tasks in the workflow with different sequence numbers.");

			public static string GroupChangedByAnotherUserMessage => Res.GetString("ACE34B08-006F-43F2-B650-42AC973C0965", "The group was changed by another user. Please refresh data.");

			public static string GroupMustContainsAtLeastTwoTasks => Res.GetString("3EFFD7E7-10BE-4CE5-ADA7-1DEF90FF1754", "Group must contains at least two tasks.");

			public static string MergeRequiresTwoDifferentGroups => Res.GetString("F5D9E108-85EB-4E63-B4EA-10A0DBC5A211", "Source and target groups must be different.");

			public static string TargetTaskBelongsToGroup => Res.GetString("F7D92709-60E0-48C0-91F1-F79CF1D862AE", "Target task belongs to a group.");
		}
	}
}
