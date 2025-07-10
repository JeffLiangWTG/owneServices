using Enterprise.Customs.IE.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI
{
	sealed class AISUploadDocumentMessageSendingGridColumnLayout : IGridColumnLayoutProvider
	{
		IGridColumnLayout IGridColumnLayoutProvider.Layout => layout ?? (layout = CreateLayout());
		IGridColumnLayout layout;

		static IGridColumnLayout CreateLayout()
		{
			var builder = GridColumnLayoutBuilder.Create();
			builder.AddColumn<ZCheckBoxColumnStyleInfo>(UploadDocumentsSendingAction.SchemaShouldSend, 40);
			builder.AddColumn<ZTextBoxColumnStyleInfo>(nameof(UploadDocumentsSendingAction.MovementReferenceNumber), 160);
			builder.AddColumn<ZTextBoxColumnStyleInfo>(nameof(UploadDocumentsSendingAction.LocalReferenceNumber), 160);
			builder.AddColumn<ZDropEditColumnStyleInfo>(nameof(UploadDocumentsSendingAction.MessageTypeForDisplay), 80);
			builder.AddColumn<ZDropEditColumnStyleInfo>(nameof(UploadDocumentsSendingAction.MessageTypeDescription), 160);
			builder.AddColumn<ZTextBoxColumnStyleInfo>(nameof(UploadDocumentsSendingAction.EntryStatus), 160);
			return builder.Build();
		}
	}
}
