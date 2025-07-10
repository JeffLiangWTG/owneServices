using CargoWise.ComponentModel;

namespace Enterprise.ZArchitecture
{
	public interface INotificationSubscriberNotification : INotification
	{
		string MultiLineDisplayMessage { get; }
		bool AllowBlankDisplayMessage { get; }
		bool ShouldBeDisplayedOnBatchProcessor { get; }
		string AdditionalInfo { get; }
	}
}
