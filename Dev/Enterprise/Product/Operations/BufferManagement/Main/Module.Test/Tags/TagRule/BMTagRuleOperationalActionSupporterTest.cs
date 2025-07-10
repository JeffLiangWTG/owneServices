using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(BMTagRuleOperationalActionSupporter))]
	sealed class BMTagRuleOperationalActionSupporterTest : OperationalActionSupporterTest<BMTagRuleOperationalActionSupporter>
	{
		protected override ModuleIdentifier ModuleID => ModuleIDs.BMTagRule;

		public override bool ShouldSupportDocuments => false;
	}
}
