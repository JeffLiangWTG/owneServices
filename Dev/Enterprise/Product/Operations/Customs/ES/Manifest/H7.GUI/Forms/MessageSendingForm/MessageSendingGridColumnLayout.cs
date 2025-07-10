using Enterprise.Customs.ES.Manifest.H7.Business;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.Manifest.H7.GUI
{
	public class MessageSendingGridColumnLayout : IGridColumnLayoutProvider
	{
		IGridColumnLayout layout;

		IGridColumnLayout IGridColumnLayoutProvider.Layout => layout ?? (layout = CreateLayout());

		IGridColumnLayout CreateLayout()
		{
			var builder = GridColumnLayoutBuilder.Create();
			builder.AddColumn<ZCheckBoxColumnStyleInfo>(nameof(AutoMessageSendingObject.ShouldSend), 140);
			builder.AddColumn<ZTextBoxColumnStyleInfo>(AutoMessageSendingObject.Schema.BillNumber, 140);
			builder.AddColumn<ZDropEditColumnStyleInfo>(AutoMessageSendingObject.Schema.Action, 140);
			builder.AddColumn<ZTextBoxColumnStyleInfo>(nameof(H7MessageSendingObject.G3LocalReferenceNumber), 140);
			builder.AddColumn<ZTextBoxColumnStyleInfo>(nameof(H7MessageSendingObject.G3MovementReferenceNumber), 140);
			builder.AddColumn<ZTextBoxColumnStyleInfo>(nameof(H7MessageSendingObject.H7MovementReferenceNumber), 140);
			builder.AddColumn<ZTextBoxColumnStyleInfo>(AutoMessageSendingObject.Schema.MessageStatus, 100);
			builder.AddColumn<ZTextBoxColumnStyleInfo>(AutoMessageSendingObject.Schema.CustomsStatus, 100);
			builder.AddColumn<ZDropEditColumnStyleInfo>(AutoMessageSendingObject.Schema.OperationCode, 140);

			return builder.Build();
		}
	}
}
