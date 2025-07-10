using CargoWise.ComponentModel;

namespace Enterprise.Accounting.ElectronicMessaging.Common
{
	public interface INotificationWithGroupKey : INotification
	{
		string GroupKey { get; }
	}
}
