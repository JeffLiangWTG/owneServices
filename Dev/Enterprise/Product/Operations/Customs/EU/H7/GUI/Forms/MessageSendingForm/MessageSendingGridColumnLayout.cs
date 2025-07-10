using Enterprise.Customs.EU.H7.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.H7.GUI
{
	public class MessageSendingGridColumnLayout : IGridColumnLayoutProvider
	{
		IGridColumnLayout IGridColumnLayoutProvider.Layout => layout ?? (layout = CreateLayout());
		IGridColumnLayout layout;

		IGridColumnLayout CreateLayout()
		{
			var builder = GridColumnLayoutBuilder.Create();
			builder.AddColumn<ZCheckBoxColumnStyleInfo>(nameof(AutoMessageSendingObject.ShouldSend), 140);
			builder.AddColumn<ZTextBoxColumnStyleInfo>(AutoMessageSendingObject.Schema.BillNumber, 140);
			builder.AddColumn<ZDropEditColumnStyleInfo>(AutoMessageSendingObject.Schema.Action, 140);
			builder.AddColumn<ZDropEditColumnStyleInfo>(AutoMessageSendingObject.Schema.SubStyle, 100);
			builder.AddColumn<ZTextBoxColumnStyleInfo>(AutoMessageSendingObject.Schema.LocalReferenceNumber, 140);
			builder.AddColumn<ZTextBoxColumnStyleInfo>(AutoMessageSendingObject.Schema.MRN, 150);
			builder.AddColumn<ZTextBoxColumnStyleInfo>(AutoMessageSendingObject.Schema.MessageStatus, 100);
			builder.AddColumn<ZTextBoxColumnStyleInfo>(AutoMessageSendingObject.Schema.CustomsStatus, 100);

			AddCountrySpecificColumns(builder);

			return builder.Build();
		}

		protected virtual void AddCountrySpecificColumns(GridColumnLayoutBuilder builder)
		{
		}
	}
}
