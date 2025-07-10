using Enterprise.Customs.EU.H7.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.Manifest.H7.GUI;

public class DocumentRequestGridColumnLayout : IGridColumnLayoutProvider
{
	IGridColumnLayout IGridColumnLayoutProvider.Layout => layout ?? (layout = CreateLayout());
	IGridColumnLayout layout;

	IGridColumnLayout CreateLayout()
	{
		var builder = GridColumnLayoutBuilder.Create();
		builder.AddColumn<ZCheckBoxColumnStyleInfo>(nameof(AutoMessageSendingObject.ShouldSend), 140);
		builder.AddColumn<ZTextBoxColumnStyleInfo>(AutoMessageSendingObject.Schema.BillNumber, 140);
		builder.AddColumn<ZTextBoxColumnStyleInfo>(nameof(Business.UploadDocumentsSendingAction.G3LocalReferenceNumber), 140);
		builder.AddColumn<ZTextBoxColumnStyleInfo>(nameof(Business.UploadDocumentsSendingAction.G3MovementReferenceNumber), 140);
		builder.AddColumn<ZTextBoxColumnStyleInfo>(nameof(Business.UploadDocumentsSendingAction.H7MovementReferenceNumber), 140);
		builder.AddColumn<ZTextBoxColumnStyleInfo>(AutoMessageSendingObject.Schema.MessageStatus, 140);
		builder.AddColumn<ZTextBoxColumnStyleInfo>(AutoMessageSendingObject.Schema.CustomsStatus, 140);
		return builder.Build();
	}
}
