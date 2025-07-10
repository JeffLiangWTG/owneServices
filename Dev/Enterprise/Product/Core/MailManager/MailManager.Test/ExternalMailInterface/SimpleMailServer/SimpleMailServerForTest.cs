using CargoWise.Async;

namespace Enterprise.MailManager.ExternalMailInterface.Testing
{
	public static class SimpleMailServerForTest
	{
		static SimpleSmtpServer smtpServer;
		static SimplePop3Server pop3Server;
		static ProlongedBlockingPop3Server prolongedBlockingPop3Server;
		static SimpleImapServer imapServer;

		public static void SetUp<T>() where T : SimpleMailServer
		{
			if (typeof(T) == typeof(SimpleSmtpServer))
			{
				smtpServer ??= new SimpleSmtpServer();
				AsyncHelper.RunTask(() => smtpServer.Start(), "SimpleSmtpServer");
			}
			else if (typeof(T) == typeof(SimplePop3Server))
			{
				pop3Server ??= new SimplePop3Server();
				AsyncHelper.RunTask(() => pop3Server.Start(), "SimplePop3Server");
			}
			else if (typeof(T) == typeof(ProlongedBlockingPop3Server))
			{
				prolongedBlockingPop3Server ??= new ProlongedBlockingPop3Server();
				AsyncHelper.RunTask(() => prolongedBlockingPop3Server.Start(), "ProlongedBlockingPop3Server");
			}
			else if (typeof(T) == typeof(SimpleImapServer))
			{
				imapServer ??= new SimpleImapServer();
				AsyncHelper.RunTask(() => imapServer.Start(), "SimpleImapServer");
			}
		}

		public static void TearDown()
		{
			smtpServer?.Dispose();
			pop3Server?.Dispose();
			prolongedBlockingPop3Server?.Dispose();
			imapServer?.Dispose();
			AsyncHelper.WaitAllActiveTasksForTest();

			SimpleSmtpServer.ClearAll();
		}

		public static SimpleMailServer GetMailServer<T>() where T : SimpleMailServer
		{
			if (typeof(T) == typeof(SimpleSmtpServer) && smtpServer != null)
			{
				return smtpServer;
			}

			if (typeof(T) == typeof(SimplePop3Server) && pop3Server != null)
			{
				return pop3Server;
			}

			if (typeof(T) == typeof(SimpleImapServer) && imapServer != null)
			{
				return imapServer;
			}

			return SimpleMailServer.EmptyServer;
		}
	}
}
