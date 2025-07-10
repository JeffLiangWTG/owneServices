using Enterprise.DocumentVisualizer.Core;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class DummyNotificationSource : INotificationSource
	{
		public string Description { get; set; }
	}
}