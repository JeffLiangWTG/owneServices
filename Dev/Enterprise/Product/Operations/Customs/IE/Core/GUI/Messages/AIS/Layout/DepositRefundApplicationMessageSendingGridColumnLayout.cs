using Enterprise.Customs.IE.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI
{
	sealed class DepositRefundApplicationMessageSendingGridColumnLayout : IGridColumnLayoutProvider
	{
		IGridColumnLayout IGridColumnLayoutProvider.Layout => layout ?? (layout = CreateLayout());
		IGridColumnLayout layout;

		static IGridColumnLayout CreateLayout()
		{
			var builder = GridColumnLayoutBuilder.Create();
			builder.AddColumn<ZCheckBoxColumnStyleInfo>(DepositRefundApplicationMessageSendingAction.SchemaShouldSend, 40);
			builder.AddColumn<ZTextBoxColumnStyleInfo>(nameof(DepositRefundApplicationMessageSendingAction.MovementReferenceNumber), 150);
			builder.AddColumn<ZDropEditColumnStyleInfo>(nameof(DepositRefundApplicationMessageSendingAction.ExportMovementReferenceNumber), 150, c =>
			{
				c.ShowInDropDown = ZDropEdit.ShowInDropDownList.OnlyShowCode;
			});
			builder.AddColumn<ZDateEditColumnStyleInfo>(nameof(DepositRefundApplicationMessageSendingAction.ExportDate), 80, c =>
			{
				c.DateTimeFormat = ZDateTimePickerFormat.Short;
			});
			builder.AddColumn<ZCalcEditColumnStyleInfo>(nameof(DepositRefundApplicationMessageSendingAction.CustomsDuty), 80);
			builder.AddColumn<ZCalcEditColumnStyleInfo>(nameof(DepositRefundApplicationMessageSendingAction.Vat), 50);
			builder.AddColumn<ZCalcEditColumnStyleInfo>(nameof(DepositRefundApplicationMessageSendingAction.OtherDuties), 80);
			builder.AddColumn<ZCheckBoxColumnStyleInfo>(nameof(DepositRefundApplicationMessageSendingAction.ImportedGoodsDischarged), 150);
			builder.AddColumn<ZCalcEditColumnStyleInfo>(nameof(DepositRefundApplicationMessageSendingAction.OutstandingBalance), 120);
			builder.AddColumn<ZCalcEditColumnStyleInfo>(nameof(DepositRefundApplicationMessageSendingAction.AmountOfDepositRefund), 160);
			builder.AddColumn<ZTextBoxColumnStyleInfo>(nameof(DepositRefundApplicationMessageSendingAction.PayerEori), 150);
			builder.AddColumn<ZCalcEditColumnStyleInfo>(nameof(DepositRefundApplicationMessageSendingAction.PeriodForDischarge), 120);
			builder.AddColumn<ZMultiLineTextBoxColumnInfo>(nameof(DepositRefundApplicationMessageSendingAction.RateOfYield), 200);
			return builder.Build();
		}
	}
}
