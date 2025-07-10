using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Module.Testing
{
	[TestedType(typeof(IncidentTriageModule))]
	public class IncidentTriageModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return Modules.ClientModuleRegistration.IncidentTriage;
		}

		public void TestWorkflowType()
		{
			using (var module = (ZFilterGridModule)GetModule())
			{
				AssertEquals(IncidentTriageConstants.WorkflowDescriptorInformation.Code, module.WorkflowType);
			}
		}

		public void TestIOperationalActionSupportable()
		{
			using (var module = new IncidentTriageModule())
			{
				AssertNotNull(module.Plugins.GetPlugin(ControllerIDs.OperationalActions));
				AssertEquals(typeof(IncidentTriageActionSupporter), ((IOperationalActionSupportable)module).OperationalActionSupporter.GetType());
			}
		}
	}
}
