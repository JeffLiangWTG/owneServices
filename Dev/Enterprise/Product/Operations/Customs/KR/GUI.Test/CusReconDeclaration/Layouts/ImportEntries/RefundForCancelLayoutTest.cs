using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(RefundForCancelLayout))]
	sealed class RefundForCancelLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;
		protected override ICommonLayoutBuilder CommonLayoutBuilder => null;
		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn => null;
		protected override bool ShouldAssertThatIncludedControlsAreSorted => false;

		public new void TestIncludedControls()
		{
			IPanelLayoutProvider layout = new RefundForCancelLayout();
			var columns = layout.Layout.Columns;
			AssertEquals(1, columns.Count);

			AssertEquals(7, columns[0].Rows.Count);
			var row1 = columns[0].Rows[0];
			var row2 = columns[0].Rows[1];
			var row3 = columns[0].Rows[2];
			var row4 = columns[0].Rows[3];
			var row5 = columns[0].Rows[4];
			var row6 = columns[0].Rows[5];
			var row7 = columns[0].Rows[6];

			AssertEquals(2, row1.Parts.Count);
			AssertEquals(nameof(RefundForCancelControlBag.CancelReasonDropEdit), row1.Parts[1].Name);

			AssertEquals(2, row2.Parts.Count);
			AssertEquals(nameof(RefundForCancelControlBag.DisposalNumberTextBox), row2.Parts[1].Name);

			AssertEquals(2, row3.Parts.Count);
			AssertEquals(nameof(RefundForCancelControlBag.DisposalDateEdit), row3.Parts[1].Name);

			AssertEquals(2, row4.Parts.Count);
			AssertEquals(nameof(RefundForCancelControlBag.GoodsLocationDescriptionLongTextControl), row4.Parts[1].Name);

			AssertEquals(2, row5.Parts.Count);
			AssertEquals(nameof(RefundForCancelControlBag.ResidualSubstanceDescriptionLongTextControl), row5.Parts[1].Name);

			AssertEquals(2, row6.Parts.Count);
			AssertEquals(nameof(RefundForCancelControlBag.DamageSituationLongTextControl), row6.Parts[1].Name);

			AssertEquals(4, row7.Parts.Count);
			AssertEquals(nameof(RefundForCancelControlBag.ExportEntryNumberTextBox), row7.Parts[1].Name);
			AssertEquals(nameof(RefundForCancelControlBag.ExportEntryLineNumberTextBox), row7.Parts[3].Name);
		}
	}
}
