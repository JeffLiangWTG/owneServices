using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Module.Testing
{
	[TestedType(typeof(StmPrintQueueActionSupporter))]
	sealed class StmPrintQueueActionSupporterTest : OperationalActionSupporterTest<StmPrintQueueActionSupporter>
	{
		protected override ModuleIdentifier ModuleID => ModuleIDs.PrintQueue;

		public override bool ShouldSupportDocuments => false;
	}
}
