using Enterprise.Customs.IE.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI
{
	sealed class RefundApplicationSendingGridColumnLayout : IGridColumnLayoutProvider
	{
		IGridColumnLayout IGridColumnLayoutProvider.Layout => layout ?? (layout = CreateLayout());
		IGridColumnLayout layout;

		static IGridColumnLayout CreateLayout()
		{
			var builder = GridColumnLayoutBuilder.Create();
			builder.AddColumn<ZCheckBoxColumnStyleInfo>(RefundApplicationMessageSendingAction.SchemaShouldSend, 40);
			builder.AddColumn<ZTextBoxColumnStyleInfo>(nameof(RefundApplicationMessageSendingAction.MovementReferenceNumber), 160);
			builder.AddColumn<ZDropEditColumnStyleInfo>(nameof(RefundApplicationMessageSendingAction.RefundType), 50);
			builder.AddColumn<ZCodeFindBoxColumnStyleInfo>(nameof(RefundApplicationMessageSendingAction.OfficeOfDebt), 160);
			builder.AddColumn<ZCodeFindBoxColumnStyleInfo>(nameof(RefundApplicationMessageSendingAction.OfficeOfResponsibility), 160);
			builder.AddColumn<ZDropEditColumnStyleInfo>(nameof(RefundApplicationMessageSendingAction.LegalBasis), 160);
			builder.AddColumn<ZTextBoxColumnStyleInfo>(nameof(RefundApplicationMessageSendingAction.DescriptionOfGrounds), 160);
			builder.AddColumn<ZTextBoxColumnStyleInfo>(nameof(RefundApplicationMessageSendingAction.BankDetails), 160);
			builder.AddColumn<ZCalcEditColumnStyleInfo>(nameof(RefundApplicationMessageSendingAction.Amount), 100);
			builder.AddColumn<ZTextBoxColumnStyleInfo>(nameof(RefundApplicationMessageSendingAction.AdditionalInformation), 160);
			return builder.Build();
		}
	}
}
