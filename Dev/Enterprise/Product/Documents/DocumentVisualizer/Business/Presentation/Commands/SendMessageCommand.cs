using CargoWise.Macros;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Delivery;
using Enterprise.DocumentVisualizer.Integration;

namespace Enterprise.DocumentVisualizer.Presentation
{
	public sealed class SendMessageCommand : MessagingCommand
	{
		public override string Id => CommandIds.SendMessage;

		public override bool Invoke(MacroMap map, IDocumentInfo info)
		{
			if (info == null)
			{
				return false;
			}

			var descriptor = info.Descriptor;
			var document = info.Document;
			var documentData = info.DocumentData;
			var services = info.Services;

			if (!CheckAllowSendMessage(services)
				|| CheckHasChanges(services, document, documentData))
			{
				return false;
			}

			var messageSender = new MessageSender(
				descriptor.MessageInstructions,
				document,
				documentData,
				services);

			var parameters = MessageSender.Parameters.New(map);
			var eDocsInstructions = descriptor.EDocsInstructions;

			var res = messageSender.Send(parameters);

			if (res
				&& eDocsInstructions.SaveCopyToEDocs)
			{
				var eDocDeliveryParameters = CreateEDocsDeliveryParameters(descriptor, eDocsInstructions);
				document.AddCopyToEDocs(eDocDeliveryParameters);
			}

			return res;
		}
	}
}
