using System.Collections.Generic;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Integration;

namespace Enterprise.DocumentVisualizer.Presentation
{
	sealed class Messaging : CommandProvider
	{
		readonly ProgressCommand sendMessage = new ProgressCommand(new SendMessageCommand());
		readonly ProgressCommand sendWithdrawal = new ProgressCommand(new SendMessageWithdrawalCommand());
		readonly ResetToOriginalCommand resetToOriginal = new ResetToOriginalCommand();

		protected override IEnumerable<ICommand> CreateCommandsCore()
		{
			yield return sendMessage;
			yield return sendWithdrawal;
			yield return resetToOriginal;
		}

		protected override void OnDocumentInfoCreatedCore(IDocumentInfo documentInfo)
		{
			sendMessage.NotifyDocumentInfoCreated(documentInfo);
			sendWithdrawal.NotifyDocumentInfoCreated(documentInfo);
			resetToOriginal.NotifyDocumentInfoCreated(documentInfo);
		}
	}
}
