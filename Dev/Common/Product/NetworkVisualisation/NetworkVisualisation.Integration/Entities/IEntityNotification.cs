
namespace CargoWise.NetworkVisualisation.Integration
{
	public interface IEntityNotification
	{
		EntityNotifcationType NotificationType { get; }
		string Message { get; }
		string ParentName { get; }
	}
}