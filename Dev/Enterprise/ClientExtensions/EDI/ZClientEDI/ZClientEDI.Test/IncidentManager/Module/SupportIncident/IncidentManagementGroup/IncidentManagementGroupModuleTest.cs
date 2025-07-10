using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Module.Testing
{
	[TestedType(typeof(IncidentManagementGroupModule))]
	public class IncidentManagementGroupModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return Modules.ClientModuleRegistration.IncidentManagementGroup;
		}

		public void TestAllowDelete()
		{
			using (IncidentManagementGroupModule mod = new IncidentManagementGroupModule())
			{
				AssertEquals("Can't delete Incident Management Groups", false, mod.AllowDelete);
			}
		}
	}
}
