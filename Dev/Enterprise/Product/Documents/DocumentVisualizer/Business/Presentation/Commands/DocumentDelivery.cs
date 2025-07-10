using System.Collections.Generic;
using CargoWise.Common;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Integration;

namespace Enterprise.DocumentVisualizer.Presentation
{
	sealed class DocumentDelivery : CommandProvider
	{
		public DocumentDelivery(IDocumentSupportable documentSupportable)
		{
			Argument.NotNull(documentSupportable, nameof(documentSupportable));
			deliverDocumentCommand = new DeliverDocumentCommand(documentSupportable);
		}

		readonly DeliverDocumentCommand deliverDocumentCommand;

		protected override IEnumerable<ICommand> CreateCommandsCore()
		{
			yield return deliverDocumentCommand;
		}

		protected override void OnDocumentInfoCreatedCore(IDocumentInfo documentInfo)
		{
			if (deliverDocumentCommand is INotifiableDocumentInfoCreated notifiableDocumentInfoCreated)
			{
				notifiableDocumentInfoCreated.NotifyDocumentInfoCreated(documentInfo);
			}
		}
	}
}
