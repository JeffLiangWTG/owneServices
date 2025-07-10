using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Module.Testing
{
	[TestedType(typeof(IncidentDiagnosticCriteriaModule))]
	public class IncidentDiagnosticCriteriaModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return Modules.ClientModuleRegistration.IncidentDiagnosticCriteria;
		}

		public void TestIOperationalActionSupportable()
		{
			using (var module = new IncidentDiagnosticCriteriaModule())
			{
				AssertNotNull(module.Plugins.GetPlugin(ControllerIDs.OperationalActions));
				AssertEquals(typeof(IncidentDiagnosticCriteriaActionSupporter), ((IOperationalActionSupportable)module).OperationalActionSupporter.GetType());
			}
		}
	}
}
