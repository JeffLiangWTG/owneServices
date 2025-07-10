using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Macros;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Delivery;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentVisualizer.Presentation
{
	public sealed class DeliverDocumentCommand : ICommand, INotifiableDocumentInfoCreated
	{
		public DeliverDocumentCommand(IDocumentSupportable documentSupportable)
		{
			this.documentSupportable = documentSupportable ?? throw new ArgumentNullException(nameof(documentSupportable));
		}

		readonly IDocumentSupportable documentSupportable;

		public string Id => CommandIds.DeliverDocument;

		public string Caption => Res.GetString("B7D38E53-20B7-4444-A6BC-2819F7BF308F", "Deliver Document");

		public object Image { get; }

		public bool IsEnabled => documentInfos.Count > 0;

		public bool IsVisible => true;

		public bool Invoke() => Invoke(null);

		public bool Invoke(MacroMap parameters) => Deliver();

		#region Deliver

		string DocumentDeliveryCaption => Res.GetString("2751e554-6a7b-420c-b08b-39c6875f48c7", "Document Delivery");

		bool Deliver()
		{
			var printableDocuments = documentInfos
				.Where(i => i.CanDeliver())
				.ToArray();

			if (!printableDocuments.Any())
			{
				return false;
			}

			var primaryInfo = printableDocuments.First();

			var documentDeliveries = printableDocuments
				.SelectMany(info => info.Descriptor.PrintInstructions.DeliveryModes
					.Where(mode => !string.IsNullOrWhiteSpace(mode))
					.Select(mode => new PrintDocumentDelivery(info, mode)))
				.ToArray();

			return documentDeliveries.Length > 0
				&& Deliver(primaryInfo.Services, primaryInfo.Document, documentDeliveries);
		}

		bool Deliver(IServiceContainer services, IDocument primaryDocument, IDocumentDelivery[] documentDeliveries)
		{
			var security = services.Resolve<IDocumentSecurityService>();

			if (!security.CanDeliver)
			{
				security.ShowDeliveryError();
				return false;
			}

			if (primaryDocument.HasDeliveryErrors())
			{
				services
					.Resolve<IUserNotificationService>()
					.ShowMessage(
						Res.GetString("95ab0260-2efc-4ab6-894a-c336e706a14f", "This document contains delivery errors. Please fix all message errors before delivering."),
						DocumentDeliveryCaption);

				return false;
			}

			var data = primaryDocument.Data;

			if (data != null && data.HasChanges)
			{
				services
					.Resolve<IUserNotificationService>()
					.ShowMessage(
						Res.GetString("1046ba07-758f-4ab2-91e4-02cb9752dd30", "Please save changes before delivering."),
						DocumentDeliveryCaption);

				return false;
			}

			if (!primaryDocument.HasErrors()
				|| ConfirmDelivery(services))
			{
				var useDraftWatermark = false;
				if (documentSupportable.GetSupporter().ShouldUseDraftWatermark(primaryDocument))
				{
					useDraftWatermark = ConfirmDraftWatermark(primaryDocument);

					if (!useDraftWatermark)
					{
						return false;
					}
				}

				services.Resolve<IDocumentDeliveryService>().Deliver(documentSupportable, documentDeliveries, useDraftWatermark);
				return true;
			}

			return false;
		}

		bool ConfirmDraftWatermark(IDocument document)
		{
			var dialogResult = Globals.Message.Show(message: Res.GetString("c97169da-8a93-49db-8632-b5c6fcf8df3b", "By clicking ‘Deliver Document’, the document will be delivered as a Draft. \r\nDo you want to Continue?"),
										caption: Res.GetString("93cdd323-aeec-41ba-8d29-2c2bc63d823c", "Deliver Document"),
										buttons: ZMessageBoxButtons.YesNo,
										icon: ZMessageBoxIcon.Question,
										defaultResult: ZDialogResult.No);

			return dialogResult == ZDialogResult.Yes;
		}

		bool ConfirmDelivery(IServiceContainer services)
		{
			return services.Resolve<IUserNotificationService>().ShowConfirmation(
				Res.GetString("adc0c8f9-145d-41c9-85eb-526d8f932d25", "This document has errors. Do you want to proceed with delivery?"),
				DocumentDeliveryCaption);
		}

		#endregion

		readonly HashSet<IDocumentInfo> documentInfos = new HashSet<IDocumentInfo>();

		void INotifiableDocumentInfoCreated.NotifyDocumentInfoCreated(IDocumentInfo documentInfo)
		{
			if (documentInfo != null)
			{
				documentInfos.Add(documentInfo);
			}
		}
	}
}
