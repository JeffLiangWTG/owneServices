using System.Collections.Generic;
using CargoWise.Macros;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Delivery;
using Enterprise.DocumentVisualizer.Integration;

namespace Enterprise.DocumentVisualizer.Presentation
{
	public sealed class SendMessageWithdrawalCommand : MessagingCommand
	{
		public override string Id => CommandIds.SendWithdrawal;

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

			var withdrawalMessageSender = new WithdrawalMessageSender(
				descriptor.MessageInstructions,
				document,
				documentData,
				services);

			if (map == null)
			{
				map = new MacroMap(new Dictionary<string, object>
				{
					[WithdrawalMessageSender.Parameters.SentCurrentDocumentDataName] = false
				});
			}

			var parameters = WithdrawalMessageSender.Parameters.New(map);
			var eDocsInstructions = descriptor.EDocsInstructions;

			var res = withdrawalMessageSender.Send(parameters);

			if (res
				&& eDocsInstructions.SaveCopyToEDocs)
			{
				info.NotifyProgress(Res.GetString("B078281D-F8C7-40CA-B59B-D799FABE0E46", "Attaching copy to eDocs"));
				var eDocDeliveryParameters = CreateEDocsDeliveryParameters(descriptor, eDocsInstructions);
				document.AddCopyToEDocs(eDocDeliveryParameters);
			}

			return res;
		}
	}
}
