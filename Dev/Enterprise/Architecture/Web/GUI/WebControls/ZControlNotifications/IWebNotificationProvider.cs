using CargoWise.ComponentModel;

namespace Enterprise.ZArchitecture.Web.GUI
{
	public interface IWebNotificationProvider : INotificationProvider
	{
		string NotificationID { get; }
	}
}
