using CargoWise.DbUpgrader.Scripts.Definitions.Glow.Workflow;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Glow.Workflow.Testing
{
	[TestedType(typeof(WorkflowAuditLog))]
	internal sealed class WorkflowAuditLogTest : WorkflowLogTest
	{
		protected override bool[] ExpectedItems => new[] { true, false, false, true, true, true, true, true, true, true };

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
			"SL_GS_NKUser",
			"SL_DataSource"
		};
	}
}

