using CargoWise.EntityFramework.Testing;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Module.Testing
{
	[TestedType(typeof(OperationalActionsController))]
	sealed class OperationalActionsControllerTest : ZControllerBasherTest
	{
		public void TestGetModulePluginWithoutActionSupport()
		{
			using (DummyFilterGridModule module = new DummyFilterGridModule())
			{
				OperationalActionsModulePlugin plugin = Plugin(module);
				AssertNull("plugin should be null", plugin);
			}
		}

		public void TestGetModulePluginWithoutDocumentSupport()
		{
			using (DummyModuleWithActionsSupport<DummyBusinessObject> module = new DummyModuleWithActionsSupport<DummyBusinessObject>())
			{
				OperationalActionsModulePlugin plugin = Plugin(module);
				AssertNotNull("plugin should be null", plugin);
				AssertEquals("plugin should be of the correct type", typeof(OperationalActionsModulePlugin), plugin.GetType());
			}
		}

		public void TestGetModulePluginWithSupport()
		{
			using (DummyModuleWithActionsSupport<DummyBusinessObjectWithDocumentSupport> module = new DummyModuleWithActionsSupport<DummyBusinessObjectWithDocumentSupport>())
			{
				OperationalActionsModulePlugin plugin = Plugin(module);
				AssertNotNull("plugin should not be null", plugin);
				AssertEquals("plugin should be of the correct type", typeof(OperationalActionsModulePlugin), plugin.GetType());
			}
		}

		#region Implementation

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.OperationalActions;
		}

		OperationalActionsModulePlugin Plugin(ZFilterGridModule module)
		{
			module.Plugins.Add(ControllerIDs.OperationalActions);
			return (OperationalActionsModulePlugin)module.Plugins.GetPlugin(ControllerIDs.OperationalActions);
		}

		#endregion
	}
}
