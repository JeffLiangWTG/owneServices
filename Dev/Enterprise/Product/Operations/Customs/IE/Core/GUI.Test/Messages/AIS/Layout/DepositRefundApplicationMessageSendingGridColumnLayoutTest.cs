using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.IE.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.IE.GUI.Testing
{
	sealed class DepositRefundApplicationMessageSendingGridColumnLayoutTest : GridColumnLayoutProviderAbstractTest<DepositRefundApplicationMessageSendingGridColumnLayout>
	{
		public void TestExportMovementReferenceNumberColumn_ShowInDropDown()
		{
			var layout = ((IGridColumnLayoutProvider)CreateGridColumnLayoutProvider()).Layout;
			var column = (ZDropEditColumnStyleInfo)layout.Columns.First(c => c.ColumnName == nameof(DepositRefundApplicationMessageSendingAction.ExportMovementReferenceNumber));
			AssertEquals(ZDropEdit.ShowInDropDownList.OnlyShowCode, column.ShowInDropDown);
		}

		public void TestExportDateColumn_DateTimeFormat()
		{
			var layout = ((IGridColumnLayoutProvider)CreateGridColumnLayoutProvider()).Layout;
			var column = (ZDateEditColumnStyleInfo)layout.Columns.First(c => c.ColumnName == nameof(DepositRefundApplicationMessageSendingAction.ExportDate));
			AssertEquals(ZDateTimePickerFormat.Short, column.DateTimeFormat);
		}

		protected override IReadOnlyCollection<(string, Type, int)> ExpectedColumns => new[]
		{
			(DepositRefundApplicationMessageSendingAction.SchemaShouldSend, typeof(ZCheckBoxColumnStyleInfo), 40),
			(nameof(DepositRefundApplicationMessageSendingAction.MovementReferenceNumber), typeof(ZTextBoxColumnStyleInfo), 150),
			(nameof(DepositRefundApplicationMessageSendingAction.ExportMovementReferenceNumber), typeof(ZDropEditColumnStyleInfo), 150),
			(nameof(DepositRefundApplicationMessageSendingAction.ExportDate), typeof(ZDateEditColumnStyleInfo), 80),
			(nameof(DepositRefundApplicationMessageSendingAction.CustomsDuty), typeof(ZCalcEditColumnStyleInfo), 80),
			(nameof(DepositRefundApplicationMessageSendingAction.Vat), typeof(ZCalcEditColumnStyleInfo), 50),
			(nameof(DepositRefundApplicationMessageSendingAction.OtherDuties), typeof(ZCalcEditColumnStyleInfo), 80),
			(nameof(DepositRefundApplicationMessageSendingAction.ImportedGoodsDischarged), typeof(ZCheckBoxColumnStyleInfo), 150),
			(nameof(DepositRefundApplicationMessageSendingAction.OutstandingBalance), typeof(ZCalcEditColumnStyleInfo), 120),
			(nameof(DepositRefundApplicationMessageSendingAction.AmountOfDepositRefund), typeof(ZCalcEditColumnStyleInfo), 160),
			(nameof(DepositRefundApplicationMessageSendingAction.PayerEori), typeof(ZTextBoxColumnStyleInfo), 150),
			(nameof(DepositRefundApplicationMessageSendingAction.PeriodForDischarge), typeof(ZCalcEditColumnStyleInfo), 120),
			(nameof(DepositRefundApplicationMessageSendingAction.RateOfYield), typeof(ZMultiLineTextBoxColumnInfo), 200),
		};

		protected override Type GridBoundEntityType => typeof(DepositRefundApplicationMessageSendingAction);
	}
}
