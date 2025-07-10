using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(NEXDOCNotificationController))]
	sealed class NEXDOCNotificationControllerTestCase : ZControllerBasherTest
	{
		public void TestTabText()
		{
			var controller = new NEXDOCNotificationController();
			AssertEquals("NEXDOC Notification", controller.PluginTabPageCaption.Caption);
		}

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.AU.NEXDOCNotificationController;
	}
}
