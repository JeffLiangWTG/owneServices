using System.Windows.Forms;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(BMTagRuleModule))]
	class BMTagRuleModuleTest : ZModuleBasherTest
	{
		public void TestHasDeleteMenuItem()
		{
			using (var form = new ZForm())
			using (var module = new BMTagRuleModule())
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				Application.DoEvents();

				AssertEquals("&Delete", module.DeleteMenuItem.Text);
				AssertNull("Should be no overridden DeleteButtonText, since that's used to signify it's a 'Deactivate' button rather than Delete.", module.DeleteButtonText);
			}
		}

		public void TestHasOperationalActionsPlugin()
		{
			using (tagRules = new BMTagRuleModule())
			{
				AssertNotNull(tagRules.Plugins.GetPlugin(ControllerIDs.OperationalActions));
			}
		}

		public void TestHasOperationalActions()
		{
			using (tagRules = new BMTagRuleModule())
			{
				var supportable = tagRules as IOperationalActionSupportable;
				AssertNotNull(supportable);
				AssertNotNull(supportable.OperationalActionSupporter);
			}
		}

		BMTagRuleModule tagRules;

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.BMTagRule;
		}
	}
}
