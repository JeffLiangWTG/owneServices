using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Messaging.Module.Test
{
	[TestedType(typeof(EDICommunicationsModeModule))]
	public class EDICommunicationsModeModuleTest : ZEmbeddedModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Messaging.EDICommunicationsMode;

		public void TestOperationalActionsPlugInAddedInModule()
		{
			using (var module = new EDICommunicationsModeModule())
			{
				AssertNotNull(module.Plugins.GetPlugin(ControllerIDs.OperationalActions));
			}
		}
	}
}
