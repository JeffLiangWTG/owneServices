namespace Enterprise.DocumentVisualizer.Presentation
{
	public interface INotificationView
	{
		string Id { get; }
		Core.NotificationType NotificationType { get; set; }
		string Message { get; set; }
		bool Visible { get; set; }
	}
}