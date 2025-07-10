using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class IConsolSendsMessagesToCustomsTest : TestCaseWithFactory
	{
		public void TestRefreshShowsMessageIfThereAreNoMessagesToSend()
		{
			_ = Sender.WhichMessagesShouldWeReset(Array.Empty<SingleMessageManager>());
			AssertEquals("LastMessage.WasError", true, UnitTestUserNotification.Instance.LastMessage.WasError);
			AssertEquals("LastMessage.Text", "There is nothing available for resetting", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		Customs.GUI.Testing.TestHelperSendsMessagesToCustomsGUI sender;
		Customs.GUI.Testing.TestHelperSendsMessagesToCustomsGUI Sender => sender ?? (sender = new Customs.GUI.Testing.TestHelperSendsMessagesToCustomsGUI());
	}
}
