using CargoWise.DbUpgrader.Scripts.Definitions.Glow.Workflow;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Glow.Workflow.Testing
{
	[TestedType(typeof(WorkflowException))]
	internal sealed class WorkflowExceptionTest : WorkflowProcessTaskTest
	{
		protected override bool[] ExpectedItems => new bool[] { false, false, true, true, false, false, false, false };

		protected override string[] ExpectedColumns => new[] {
			"P9_PK",
			"P9_Description",
			"P9_ParentID",
			"P9_ParentTableCode",
			"P9_SE_NKExceptionEvent",
			"P9_ActualDateUtc",
			"P9_ActualDate",
			"P9_GG_AssignedGroup",
			"P9_GS_NKAssignedStaffMember",
			"P9_Notes",
			"P9_Sequence",
			"P9_Status",
			"P9_SystemCreateUser",
			"P9_SystemLastEditUser",
			"P9_SystemCreateTimeUtc",
			"P9_SystemLastEditTimeUtc"
		};
	}
}

