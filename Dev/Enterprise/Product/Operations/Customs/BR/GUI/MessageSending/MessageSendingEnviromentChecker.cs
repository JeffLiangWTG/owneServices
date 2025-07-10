using System.Windows.Forms;
using Enterprise.xTMessaging.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.BR.GUI
{
	public static class MessageSendingEnviromentChecker
	{
		public static bool CheckIsOKToSend()
		{
			return DirectxTMessagingRegistry.Instance.ConnectionToXTServer.Value == ConnectionToXTServerOptions.XtProduction.Code
				|| Globals.Message.Show(
					ResString.GetMultilingualString("6F707361-75A6-46E7-86CD-482C99BD4001", "The selected messages will be sent to a test environment!"),
					ResString.GetMultilingualString("2EBDADB9-BD13-496F-939B-ED6AF1B30976", "Continue to Send"), MessageBoxButtons.OKCancel,
					MessageBoxIcon.Question) == DialogResult.OK;
		}
	}
}
