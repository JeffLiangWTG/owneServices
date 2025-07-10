using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.GUI.Testing
{
	[TestedType(typeof(ImportInvoiceLineCalculationsLayout))]
	sealed class ImportInvoiceLineCalculationsLayoutTest : LayoutsAbstractTest
	{
		public new void TestIncludedControls()
		{
			IPanelLayoutProvider layout = new ImportInvoiceLineCalculationsLayout();
			var columns = layout.Layout.Columns;
			AssertEquals("InvoiceLineSummaryLayout should have 1 column.", 1, columns.Count);

			var column = columns[0];
			AssertEquals(12, columns[0].Rows.Count);
			var row0 = column.Rows[0];
			var row1 = column.Rows[1];
			var row2 = column.Rows[2];
			var row3 = column.Rows[3];
			var row4 = column.Rows[4];
			var row5 = column.Rows[5];
			var row6 = column.Rows[6];
			var row7 = column.Rows[7];
			var row8 = column.Rows[8];
			var row9 = column.Rows[9];
			var rowA = column.Rows[10];
			var rowB = column.Rows[11];

			var euBag = InvoiceLineSummaryControlBag.Instance;
			var baseBag = CommonInvoiceLineCalculationsControlBag.Instance;

			CombineAssertions("Items of Column 0", () =>
			{
				AssertRow(0, column.Rows[0], nameof(baseBag.CurrentInvoiceLabel));
				AssertRow(1, column.Rows[1], nameof(baseBag.BalanceConvertToLocalCurrencyControl));
				AssertRow(2, column.Rows[2], nameof(baseBag.LinesEnteredConvertToLocalCurrencyControl));
				AssertRow(3, column.Rows[3], nameof(baseBag.LinesTotalConvertToLocalCurrencyControl));
				AssertRow(4, column.Rows[4], nameof(baseBag.SummaryLabel));
				AssertRow(5, column.Rows[5], nameof(baseBag.DutyAmountIncludingWHEstimateConvertToLocalCurrencyControl));
				AssertRow(6, column.Rows[6], nameof(baseBag.GSTVATAmountIncludingWHEstimateConvertToLocalCurrencyControl));
				AssertRow(7, column.Rows[7], nameof(euBag.GSTVATDeferredConvertToLocalCurrencyControl));
				AssertRow(8, column.Rows[8], nameof(euBag.CustomsValueConvertToLocalCurrencyControl));
				AssertRow(9, column.Rows[9], nameof(euBag.ValueForVatConvertToLocalCurrencyControl));
				AssertRow(10, column.Rows[10], nameof(euBag.StatisticalValueConvertToLocalCurrencyControl));
				AssertRow(11, column.Rows[11], nameof(baseBag.CIFConvertToLocalCurrencyControl));
			});

			void AssertRow(int rowIndex, PanelLayoutRow row, string expectedMainControlName)
			{
				AssertEquals($"Row{{{rowIndex}}} should contain the control {expectedMainControlName}", true, row.Parts.Any(p => p.Name == expectedMainControlName));
			}
		}

		protected override int ControlBagCount => 2;
		protected override ICommonLayoutBuilder CommonLayoutBuilder => null;
		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn => null;
		protected override bool ShouldAssertThatIncludedControlsAreSorted => false;
	}
}
