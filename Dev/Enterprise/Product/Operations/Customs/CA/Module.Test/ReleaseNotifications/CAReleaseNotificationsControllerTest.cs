using System;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(CAReleaseNotificationsController))]
	sealed class CAReleaseNotificationsControllerTest : ZControllerBasherTest
	{
		public void TestCheckPoints()
		{
			var message = Factory.New<Business.EDIMessage>();
			var controller = new CAReleaseNotificationsController();
			AssertEquals(Env.Security.CAReleaseNotificationsView, controller.GetCheckPointForView(message));
		}

		public override Type ControllerToBashType => typeof(CAReleaseNotificationsController);

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.CA.CAReleaseNotifications;
	}
}
