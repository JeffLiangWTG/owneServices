using CargoWise.Common;
using Enterprise.Customs.Business.MessageManagers;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.CA.Business.MessageManagers;
using Enterprise.Customs.Common.MessageBuilders;

namespace Enterprise.Customs.CA.Business
{
	public interface IRNSAutoSender
	{
		bool Process();
	}

	public class RNSAutoSender : IRNSAutoSender
	{
		public RNSAutoSender(CusEntryHeader entryHeader, IUserNotification notification)
		{
			this.entryHeader = Argument.NotNull(entryHeader, "entryHeader");
			this.notification = Argument.NotNull(notification, "notification");
		}

		readonly CusEntryHeader entryHeader;
		readonly IUserNotification notification;

		public bool Process()
		{
			var dataWrapper = new StatusQueryMessageWrapper(entryHeader);
			var manager = new RNSMessageManager(dataWrapper, notification, true);
			manager.SetShouldJobBeSavedBeforeSendingMessage(false);
			return manager.SendMessage(MessageSubTypes.Request, false);
		}
	}
}
