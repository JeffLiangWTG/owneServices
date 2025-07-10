using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Module.Testing
{
	[TestedType(typeof(InvestigationItemModule))]
	public class InvestigationItemModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return Modules.ClientModuleRegistration.InvestigationItem;
		}

		public void TestIOperationalActionSupportable()
		{
			using (var module = new InvestigationItemModule())
			{
				AssertNotNull(module.Plugins.GetPlugin(ControllerIDs.OperationalActions));
				AssertEquals(typeof(InvestigationItemActionSupporter), ((IOperationalActionSupportable)module).OperationalActionSupporter.GetType());
			}
		}
	}
}
