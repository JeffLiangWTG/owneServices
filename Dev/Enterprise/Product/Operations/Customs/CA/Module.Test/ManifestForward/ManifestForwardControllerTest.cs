using System;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(ManifestForwardController))]
	sealed class ManifestForwardControllerTest : ZControllerBasherTest
	{
		public void TestCheckPoints()
		{
			var message = Factory.New<Business.EDIMessage>();
			var controller = new ManifestForwardController();
			AssertEquals(Env.Security.CAManifestForwardView, controller.GetCheckPointForView(message));
		}

		public override Type ControllerToBashType => typeof(ManifestForwardController);

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.CA.CAManifestForward;
	}
}
