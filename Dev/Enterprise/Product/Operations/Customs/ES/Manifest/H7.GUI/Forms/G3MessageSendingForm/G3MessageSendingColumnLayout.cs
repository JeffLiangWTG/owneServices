using Enterprise.Customs.ES.Manifest.H7.Business;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.Manifest.H7.GUI
{
	public class G3MessageSendingColumnLayout : IGridColumnLayoutProvider
	{
		IGridColumnLayout IGridColumnLayoutProvider.Layout => layout ?? (layout = CreateLayout());
		IGridColumnLayout layout;

		IGridColumnLayout CreateLayout()
		{
			var builder = GridColumnLayoutBuilder.Create();
			builder.AddColumn<ZCheckBoxColumnStyleInfo>(nameof(AutoMessageSendingObject.ShouldSend), 140);
			builder.AddColumn<ZTextBoxColumnStyleInfo>(AutoMessageSendingObject.Schema.BillNumber, 140);
			builder.AddColumn<ZDropEditColumnStyleInfo>(AutoMessageSendingObject.Schema.Action, 140, columnInfo => { columnInfo.IsReadOnly = false; });
			builder.AddColumn<ZTextBoxColumnStyleInfo>(nameof(G3MessageSendingObject.G3LocalReferenceNumber), 140, columnInfo => { columnInfo.IsReadOnly = true; });
			builder.AddColumn<ZTextBoxColumnStyleInfo>(nameof(G3MessageSendingObject.G3MovementReferenceNumber), 140, columnInfo => { columnInfo.IsReadOnly = true; });
			builder.AddColumn<ZTextBoxColumnStyleInfo>(nameof(G3MessageSendingObject.H7MovementReferenceNumber), 140, columnInfo => { columnInfo.IsReadOnly = true; });
			builder.AddColumn<ZTextBoxColumnStyleInfo>(AutoMessageSendingObject.Schema.MessageStatus, 140, columnInfo => { columnInfo.IsReadOnly = true; });
			builder.AddColumn<ZTextBoxColumnStyleInfo>(AutoMessageSendingObject.Schema.EntryStatus, 100, columnInfo => { columnInfo.IsReadOnly = true; });
			builder.AddColumn<ZDropEditColumnStyleInfo>(AutoMessageSendingObject.Schema.RevokeReason, 100, columnInfo => { columnInfo.IsReadOnly = false; });
			builder.AddColumn<ZTextBoxColumnStyleInfo>(AutoMessageSendingObject.Schema.RevokeReasonDescription, 140, columnInfo => { columnInfo.IsReadOnly = false; });

			return builder.Build();
		}
	}
}
