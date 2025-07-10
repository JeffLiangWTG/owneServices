using Enterprise.DocumentVisualizer.Presentation;

namespace Enterprise.DocumentVisualizer.Testing.GUI
{
	sealed class DummyDocumentView : IDocumentView
	{
		public DummyPageViewCollection PageViews
		{
			get { return pageViewCollection; }
		}

		IViewCollection<IPageView> IDocumentView.PageViews
		{
			get { return PageViews; }
		}

		readonly DummyPageViewCollection pageViewCollection = new DummyPageViewCollection();

		IViewCollection<INotificationView> IDocumentView.NotificationViews
		{
			get { return notificationPanel ?? (notificationPanel = new DummyNotificationViews()); }
		}

		IViewCollection<INotificationView> notificationPanel;

		void IDocumentView.ShowWatermark(string text)
		{
			WatermarkShown = text;
		}

		public string WatermarkShown { get; private set; }

		void IDocumentView.Focus()
		{
			HasBeenFocused = true;
		}

		public bool HasBeenFocused { get; set; }

		void IDocumentView.Scale(float scale)
		{
		}

		void IDocumentView.Refresh()
		{
		}
	}
}