using CargoWise.DbUpgrader.Scripts.Definitions.Core.LogWalker;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Core.LogWalker.Testing
{
	[TestedType(typeof(CreateLogQueueItemsForWorkflowTriggerEvents))]
	class CreateLogQueueItemsForWorkflowTriggerEventsTest : DbCreateScriptTest
	{
		// currently just moving existing code. Tests are in scheduler assembly
	}
}
