using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Module
{
	[TestedType(typeof(EDIOrgOpportunityModule))]
	class EDIOrgOpportunityModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.Opportunity;
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (EDIOrgOpportunityModule module = new EDIOrgOpportunityModule())
			{
				AssertEquals(typeof(EDIOrgOpportunityFilterBusinessObject), module.InternalGetNewFilterBusinessObject().GetType());
			}
		}
	}
}
