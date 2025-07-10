using CargoWise.DbUpgrader.Scripts.Definitions.Glow.Workflow;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Glow.Workflow.Testing
{
	[TestedType(typeof(TG_INS_WorkflowException))]
	class TG_INS_WorkflowExceptionTest : WorkflowProcessTaskTriggerTest<WorkflowException>
	{
		protected override string ProcessTaskType => "EXC";
	}
}

