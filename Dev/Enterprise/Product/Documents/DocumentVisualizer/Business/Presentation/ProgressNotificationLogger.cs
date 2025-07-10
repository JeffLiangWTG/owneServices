using System.Linq;
using CargoWise.Common;
using Enterprise.DocumentVisualizer.Core;
using Res = Enterprise.DocumentVisualizer.Business.Res;

namespace Enterprise.DocumentVisualizer.Presentation
{
	/// <summary>
	/// Notifies subscribers of ProgressInfoEvent about the progress of document build process
	/// </summary>
	public sealed class ProgressNotificationLogger : ILogger
	{
		public ProgressNotificationLogger(IEventBroker eventBroker, string documentName)
		{
			Argument.NotNull(eventBroker, nameof(eventBroker));
			this.eventBroker = eventBroker;
			this.documentName = documentName ?? Res.GetString("022833c2-f265-4bce-a3dc-d5f70535dfdc", "Document");
		}

		readonly string documentName;
		readonly IEventBroker eventBroker;

		void ILogger.Log(LogMessageType messageType, params object[] parameters)
		{
			var message = GetMessage(messageType, parameters);
			eventBroker.NotifyProgress(message);
		}

		string GetMessage(LogMessageType messageType, object[] parameters)
		{
			switch (messageType)
			{
				case LogMessageType.RetrievingData:
					return Res.GetString("ca161148-1830-47f2-8125-2916ce58a695", "{0}: Retrieving data", documentName);

				case LogMessageType.PreparingData:
					return Res.GetString("ba7f2802-afc5-45f9-bacd-e5e62f950948", "{0}: Preparing data", documentName);

				case LogMessageType.CreatingPage:
					return parameters != null && parameters.Any()
						? Res.GetString("d193d20c-4679-4605-943b-43185c5426ba", "{0}: Creating page {1}", new[] { documentName }.Concat(parameters).ToArray())
						: Res.GetString("90b05134-d061-4772-8b58-7b303b185e9a", "{0}: Creating pages", documentName);

				case LogMessageType.Finishing:
					return Res.GetString("01b96c8f-696f-441c-a83f-c819623e8975", "{0}: Finishing", documentName);
			}

			return string.Empty;
		}
	}
}
