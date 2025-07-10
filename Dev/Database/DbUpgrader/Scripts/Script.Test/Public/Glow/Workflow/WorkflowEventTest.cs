using CargoWise.DbUpgrader.Scripts.Definitions.Glow.Workflow;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Glow.Workflow.Testing
{
	[TestedType(typeof(WorkflowEvent))]
	internal sealed class WorkflowEventTest : WorkflowLogTest
	{
		protected override bool[] ExpectedItems => new[] { false, true, true, false, false, false, false, false, false, false };

		protected override string[] ExpectedColumns => new[]
		{
			"SL_PK",
			"SL_Table",
			"SL_Parent",
			"SL_IsEstimate",
			"SL_IsCancelled",
			"SL_Reference",
			"SL_PostedTimeUtc",
			"SL_SE_NKEvent",
			"SL_EventTime",
			"SL_EventTimeUtc",
			"SL_GS_NKUser",
			"SL_GB_NKBranch",
			"SL_DataSource",
			"SL_FireWorkflow"
		};
	}
}

