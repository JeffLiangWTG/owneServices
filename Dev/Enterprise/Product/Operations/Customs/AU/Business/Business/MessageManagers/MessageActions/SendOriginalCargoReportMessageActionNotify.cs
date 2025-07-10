using CargoWise.ComponentModel;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.AU.Declaration.Business
{
	class SendOriginalCargoReportMessageActionNotify : SendOriginalCargoReportMessageAction
	{
		public SendOriginalCargoReportMessageActionNotify(INotifications notifications)
			: base()
		{
			notify = notifications;
		}

		protected override void OnNotifyUserOfASuccessfulSend(string text)
		{
			base.OnNotifyUserOfASuccessfulSend(text);
			notify.Notify(new InfoNotification(text));
		}

		protected override void OnNotifyUserOfAnInvalidOperation(string text)
		{
			base.OnNotifyUserOfAnInvalidOperation(text);
			notify.Notify(new InfoNotification(text));
		}

		protected override void OnWarnUserAboutSomething(string message, string caption)
		{
			base.OnWarnUserAboutSomething(message, caption);
			notify.AddWarning(message);
		}

		protected override bool OnContinueWithAction(string message, string caption)
		{
			notify.AddWarning(message);
			return SendWithMessageErrors;
		}

		readonly INotifications notify;
	}
}
