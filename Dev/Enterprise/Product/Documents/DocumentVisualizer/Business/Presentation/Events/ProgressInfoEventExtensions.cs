using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;

namespace Enterprise.DocumentVisualizer.Presentation
{
	public static class ProgressInfoEventExtensions
	{
		public static void NotifyProgress(this IEventBroker broker, string message)
		{
			if (broker == null
				|| string.IsNullOrWhiteSpace(message))
			{
				return;
			}

			var eventData = new ProgressInfoEvent(message);
			broker.Publish(eventData);
		}

		public static void NotifyProgress(this IDocumentInfo documentInfo, string message)
		{
			if (documentInfo?.Services == null
				|| string.IsNullOrWhiteSpace(message))
			{
				return;
			}

			var services = documentInfo.Services;
			var broker = services.Resolve<IEventBroker>();

			var eventData = new ProgressInfoEvent(message);
			broker.Publish(eventData);
		}
	}
}
