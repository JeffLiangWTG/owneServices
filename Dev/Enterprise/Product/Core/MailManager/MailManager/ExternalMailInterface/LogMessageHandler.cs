using System.Diagnostics;

namespace Enterprise.MailManager.ExternalMailInterface
{
	public delegate void LogMessageHandler(TraceEventType eventType, string message);
}
