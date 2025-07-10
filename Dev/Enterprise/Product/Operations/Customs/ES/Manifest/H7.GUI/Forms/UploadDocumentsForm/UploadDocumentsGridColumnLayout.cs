using Enterprise.Customs.EU.H7.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.Manifest.H7.GUI
{
	public class UploadDocumentsGridColumnLayout : IGridColumnLayoutProvider
	{
		IGridColumnLayout IGridColumnLayoutProvider.Layout => layout ?? (layout = CreateLayout());
		IGridColumnLayout layout;

		IGridColumnLayout CreateLayout()
		{
			var builder = GridColumnLayoutBuilder.Create();
			builder.AddColumn<ZCheckBoxColumnStyleInfo>(nameof(AutoMessageSendingObject.ShouldSend), 140);
			builder.AddColumn<ZDropEditColumnStyleInfo>(AutoMessageSendingObject.Schema.Action, 140);
			builder.AddColumn<ZTextBoxColumnStyleInfo>(AutoMessageSendingObject.Schema.BillNumber, 140);
			builder.AddColumn<ZTextBoxColumnStyleInfo>(nameof(Business.UploadDocumentsSendingAction.G3LocalReferenceNumber), 140);
			builder.AddColumn<ZTextBoxColumnStyleInfo>(nameof(Business.UploadDocumentsSendingAction.G3MovementReferenceNumber), 140);
			builder.AddColumn<ZTextBoxColumnStyleInfo>(nameof(Business.UploadDocumentsSendingAction.H7MovementReferenceNumber), 140);
			builder.AddColumn<ZTextBoxColumnStyleInfo>(AutoMessageSendingObject.Schema.CustomsStatus, 100);
			builder.AddColumn<ZCheckBoxColumnStyleInfo>(Business.UploadDocumentsSendingAction.Schema.ClearanceRequested, 100);

			return builder.Build();
		}
	}
}
