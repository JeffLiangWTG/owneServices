namespace Enterprise.DocumentVisualizer.Presentation
{
	public interface IDocumentView
	{
		IViewCollection<IPageView> PageViews { get; }

		IViewCollection<INotificationView> NotificationViews { get; }

		void Scale(float scale);

		void Refresh();

		void ShowWatermark(string text);

		void Focus();
	}
}