using CargoWise.Common;
using CargoWise.ComponentModel;

namespace Enterprise.UniversalDataBuss.Management
{
	public class NotificationsAdapter : INotifications
	{
		public NotificationsAdapter(XmlSessionTracker logger)
		{
			this.logger = Argument.NotNull(logger, "logger");
		}

		readonly XmlSessionTracker logger;

		public void Add(INotification notification)
		{
			logger.LogErrorToServiceTaskOnly(notification.Message);
		}
	}
}
