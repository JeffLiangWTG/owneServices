using CargoWise.DbUpgrader.Scripts.Definitions.Glow.Workflow;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Glow.Workflow.Testing
{
	[TestedType(typeof(WorkflowTask))]
	internal sealed class WorkflowTaskTest : WorkflowProcessTaskTest
	{
		protected override bool[] ExpectedItems => new bool[] { false, false, false, false, false, false, true, true };

		protected override string[] ExpectedColumns => new[] {
			"P9_PK",
			"P9_ActualDate",
			"P9_ActualDateUtc",
			"P9_ActualDuration",
			"P9_CompletedTimeUtc",
			"P9_Description",
			"P9_EstDuration",
			"P9_EstimateVariationFactor",
			"P9_FH_ProcessHeader",
			"P9_G4_RequiredCapability",
			"P9_GS_NKAssignedStaffMember",
			"P9_IsPublished",
			"P9_IsValid",
			"P9_Notes",
			"P9_ParentID",
			"P9_ParentTableCode",
			"P9_Sequence",
			"P9_Status",
			"P9_Type",
			"P9_SystemCreateUser",
			"P9_SystemLastEditUser",
			"P9_SystemCreateTimeUtc",
			"P9_SystemLastEditTimeUtc",
			"P9_Outcome",
			"P9_SuspendedAt",
			"P9_SuspendedAtUtc",
			"P9_TotalSuspendedDuration",
			"P9_SE_NKTaskCompletionEvent",
		};
	}
}

