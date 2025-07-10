using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(ProcessHeaderOperationalActionSupporter))]
	internal sealed class ProcessHeaderOperationalActionSupporterTest : OperationalActionSupporterTest<ProcessHeaderOperationalActionSupporter>
	{
		protected override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.ProcessHeader; }
		}

		public override bool ShouldSupportDocuments
		{
			get { return false; }
		}
	}
}
