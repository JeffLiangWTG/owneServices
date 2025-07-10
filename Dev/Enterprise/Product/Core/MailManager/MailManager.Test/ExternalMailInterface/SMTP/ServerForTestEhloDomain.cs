using Rnwood.SmtpServer;

namespace Enterprise.MailManager.ExternalMailInterface.Testing
{
	sealed class ServerForTestEhloDomain : ServerSupportingAuthLogin
	{
		public static volatile string SessionLog;
		public static volatile bool MessageReceivedFlag;

		public ServerForTestEhloDomain(int portNumber, string username, string password)
			: base(portNumber, username, password)
		{
			SessionLog = "";
			MessageReceivedFlag = false;

			Behaviour.SessionStarted += TestSessionStarted;
			Behaviour.MessageReceived += TestMessageReceived;
		}

		static void TestSessionStarted(object sender, SessionEventArgs e)
		{
			SessionLog += e.Session.Log + "\n";
		}

		static void TestMessageReceived(object sender, MessageEventArgs e)
		{
			SessionLog += e.Message.Session.Log + "\n";
			MessageReceivedFlag = true;
		}
	}
}
