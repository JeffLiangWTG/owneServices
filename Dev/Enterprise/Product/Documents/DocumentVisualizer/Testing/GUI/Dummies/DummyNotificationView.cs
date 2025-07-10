using System.Drawing;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Presentation;

namespace Enterprise.DocumentVisualizer.Testing.GUI
{
	sealed class DummyNotificationView : INotificationView
	{
		public string Id { get; set; }
		public NotificationType NotificationType { get; set; }
		public Color BackColor { get; set; }
		public string Message { get; set; }
		public bool Visible { get; set; }
	}
}