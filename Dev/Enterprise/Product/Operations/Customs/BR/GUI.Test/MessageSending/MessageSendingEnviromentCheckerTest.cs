using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.xTMessaging.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.BR.GUI.Testing
{
	class MessageSendingEnviromentCheckerTest : TestCaseWithFactory
	{
		public void TestCheckIsOKToSend()
		{
			using (DirectxTMessagingRegistry.Instance.ConnectionToXTServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ConnectionToXTServerOptions.XtProduction.Code))
			{
				AssertEquals(true, MessageSendingEnviromentChecker.CheckIsOKToSend());
			}

			using (DirectxTMessagingRegistry.Instance.ConnectionToXTServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ConnectionToXTServerOptions.XtTest.Code))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				AssertEquals(false, MessageSendingEnviromentChecker.CheckIsOKToSend());

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				AssertEquals(true, MessageSendingEnviromentChecker.CheckIsOKToSend());
			}
		}
	}
}
