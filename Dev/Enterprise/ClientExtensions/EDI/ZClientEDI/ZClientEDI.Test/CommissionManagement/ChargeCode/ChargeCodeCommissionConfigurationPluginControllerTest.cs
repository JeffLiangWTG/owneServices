using Enterprise.Client.EDI.Modules;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.CommissionManagement.GUI.Testing
{
	[TestedType(typeof(ChargeCodeCommissionConfigurationPluginController))]
	internal class ChargeCodeCommissionConfigurationPluginControllerTest : ZControllerBasherTest
	{
		#region ID
		protected override ControllerID GetControllerID()
		{
			return ClientControllerRegistration.ChargeCodeCommissionConfiguration;
		}

		#endregion
		#region Plugin
		public void TestPluginTabPageCaption()
		{
			var controller = new ChargeCodeCommissionConfigurationPluginController();
			AssertEquals("Commission Configuration", controller.PluginTabPageCaption.Caption);
		}
		#endregion
	}
}
