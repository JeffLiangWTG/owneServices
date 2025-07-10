using Enterprise.Customs.IE.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI
{
	sealed class DocumentMessageSendingGridColumnLayout : IGridColumnLayoutProvider
	{
		#region IGridColumnLayoutProvider

		IGridColumnLayout IGridColumnLayoutProvider.Layout => layout ?? (layout = CreateLayout());
		IGridColumnLayout layout;

		#endregion

		IGridColumnLayout CreateLayout()
		{
			var builder = GridColumnLayoutBuilder.Create();
			builder.AddColumn<ZCheckBoxColumnStyleInfo>(DocumentsSendingAction.SchemaShouldSend, 40);
			builder.AddColumn<ZTextBoxColumnStyleInfo>(nameof(DocumentsSendingAction.LocalReferenceNumber), 250);
			builder.AddColumn<ZTextBoxColumnStyleInfo>(nameof(DocumentsSendingAction.MovementReference), 250);
			return builder.Build();
		}
	}
}
