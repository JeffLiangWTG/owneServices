using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(DisbursementBillsLayout))]
	sealed class DisbursementBillsLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;
		protected override ICommonLayoutBuilder CommonLayoutBuilder => null;
		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn => null;
		protected override bool ShouldAssertThatIncludedControlsAreSorted => false;

		public new void TestIncludedControls()
		{
			IPanelLayoutProvider layout = new DisbursementBillsLayout();
			var columns = layout.Layout.Columns;
			AssertEquals(2, columns.Count);

			AssertEquals(6, columns[0].Rows.Count);
			var row0_1 = columns[0].Rows[0];
			var row0_2 = columns[0].Rows[1];
			var row0_3 = columns[0].Rows[2];
			var row0_4 = columns[0].Rows[3];
			var row0_5 = columns[0].Rows[4];
			var row0_6 = columns[0].Rows[5];

			AssertEquals(2, row0_1.Parts.Count);
			AssertEquals(nameof(StatementHeaderControlBag.Instance.FormattedNumberTextBox), row0_1.Parts[1].Name);

			AssertEquals(2, row0_2.Parts.Count);
			AssertEquals(nameof(StatementHeaderControlBag.Instance.FormattedEntryNumberTextBox), row0_2.Parts[1].Name);

			AssertEquals(2, row0_3.Parts.Count);
			AssertEquals(nameof(StatementHeaderControlBag.Instance.BillTypeDropEdit), row0_3.Parts[1].Name);

			AssertEquals(2, row0_4.Parts.Count);
			AssertEquals(nameof(StatementHeaderControlBag.Instance.CustomsAccountIDTextBox), row0_4.Parts[1].Name);

			AssertEquals(2, row0_5.Parts.Count);
			AssertEquals(nameof(StatementHeaderControlBag.Instance.ProcessPortCodeFindBox), row0_5.Parts[1].Name);

			AssertEquals(2, row0_6.Parts.Count);
			AssertEquals(nameof(StatementHeaderControlBag.Instance.ImporterGuidFindBox), row0_6.Parts[1].Name);

			AssertEquals(7, columns[1].Rows.Count);
			var row1_1 = columns[1].Rows[0];
			var row1_2 = columns[1].Rows[1];
			var row1_3 = columns[1].Rows[2];
			var row1_4 = columns[1].Rows[3];
			var row1_5 = columns[1].Rows[4];
			var row1_6 = columns[1].Rows[5];
			var row1_7 = columns[1].Rows[6];

			AssertEquals(2, row1_1.Parts.Count);
			AssertEquals(nameof(StatementHeaderControlBag.Instance.StatusDropEdit), row1_1.Parts[1].Name);

			AssertEquals(2, row1_2.Parts.Count);
			AssertEquals(nameof(StatementHeaderControlBag.Instance.DueDateEdit), row1_2.Parts[1].Name);

			AssertEquals(2, row1_3.Parts.Count);
			AssertEquals(nameof(StatementHeaderControlBag.Instance.IssueDateEdit), row1_3.Parts[1].Name);

			AssertEquals(2, row1_4.Parts.Count);
			AssertEquals(nameof(StatementHeaderControlBag.Instance.ProcessDateEdit), row1_4.Parts[1].Name);

			AssertEquals(2, row1_5.Parts.Count);
			AssertEquals(nameof(StatementHeaderControlBag.Instance.PaymentDateEdit), row1_5.Parts[1].Name);

			AssertEquals(2, row1_6.Parts.Count);
			AssertEquals(nameof(StatementHeaderControlBag.Instance.StatementAmountCalcEdit), row1_6.Parts[1].Name);

			AssertEquals(2, row1_7.Parts.Count);
			AssertEquals(nameof(StatementHeaderControlBag.Instance.TotalAmountAfterDueDateCalcEdit), row1_7.Parts[1].Name);
		}
	}
}
