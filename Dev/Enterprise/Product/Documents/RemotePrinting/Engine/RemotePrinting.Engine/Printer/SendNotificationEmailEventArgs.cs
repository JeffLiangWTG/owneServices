using System;

namespace Enterprise.RemotePrinting.Engine
{
	public class SendNotificationEmailEventArgs : EventArgs
	{
		public string Subject { get; }

		public string Body { get; }

		public SendNotificationEmailEventArgs(string subject, string body)
		{
			Subject = subject;
			Body = body;
		}
	}
}
