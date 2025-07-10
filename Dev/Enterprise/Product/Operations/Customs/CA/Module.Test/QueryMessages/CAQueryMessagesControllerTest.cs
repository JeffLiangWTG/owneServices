using System;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(CAQueryMessagesController))]
	sealed class CAQueryMessagesControllerTest : ZControllerBasherTest
	{
		public void TestCheckPoints()
		{
			var message = Factory.New<Business.EDIMessage>();
			var controller = new CAQueryMessagesController();
			AssertEquals(Env.Security.CAQueryMessagesView, controller.GetCheckPointForView(message));
		}

		public override Type ControllerToBashType => typeof(CAQueryMessagesController);

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.CA.CAQueryMessages;
	}
}
