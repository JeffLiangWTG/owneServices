using System;
using Enterprise.Customs.Business;
using Enterprise.Integration;

namespace Enterprise.Customs.CA.Business
{
	public class LVXJobNotificationCollector : SendsMessagesToCustomsShutterUpperer
	{
		public LVXJobNotificationCollector(Action<LogType, string, object[]> notify) : base(false)
		{
			this.notify = notify;
		}

		readonly Action<LogType, string, object[]> notify;

		public override void NotifyUserOfAnInvalidOperation(string text)
		{
			base.NotifyUserOfAnInvalidOperation(text);
			if (notify != null)
			{
				notify(LogType.Warning, text, Array.Empty<object>());
			}
		}
	}
}
