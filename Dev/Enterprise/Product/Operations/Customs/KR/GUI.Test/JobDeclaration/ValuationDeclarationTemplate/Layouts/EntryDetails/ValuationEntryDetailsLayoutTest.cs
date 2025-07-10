using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(ValuationEntryDetailsLayout))]
	sealed class ValuationEntryDetailsLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn => null;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => null;

		protected override bool ShouldAssertThatIncludedControlsAreSorted => false;

		public new void TestIncludedControls()
		{
			IPanelLayoutProvider layout = new ValuationEntryDetailsLayout();
			var columns = layout.Layout.Columns;
			AssertEquals(3, columns.Count);

			AssertEquals(3, columns[0].Rows.Count);
			var row1 = columns[0].Rows[0];
			var row2 = columns[0].Rows[1];
			var row3 = columns[0].Rows[2];
			AssertEquals(nameof(ValuationEntryDetailsControlBag.Instance.EntryNumberTextBox), row1.Parts[1].Name);
			AssertEquals(nameof(ValuationEntryDetailsControlBag.Instance.MessageStatusDropEdit), row2.Parts[1].Name);
			AssertEquals(nameof(ValuationEntryDetailsControlBag.Instance.EntryStatusDropEdit), row3.Parts[1].Name);

			AssertEquals(3, columns[1].Rows.Count);
			row1 = columns[1].Rows[0];
			row2 = columns[1].Rows[1];
			row3 = columns[1].Rows[2];
			AssertEquals(nameof(ValuationEntryDetailsControlBag.Instance.EntrySubmittedDateDateEdit), row1.Parts[1].Name);
			AssertEquals(nameof(ValuationEntryDetailsControlBag.Instance.AcceptedDateDateEdit), row2.Parts[1].Name);
			AssertEquals(nameof(ValuationEntryDetailsControlBag.Instance.ApprovalDateDateEdit), row3.Parts[1].Name);

			AssertEquals(3, columns[2].Rows.Count);
			row1 = columns[2].Rows[0];
			row2 = columns[2].Rows[1];
			row3 = columns[2].Rows[2];
			AssertEquals(nameof(ValuationEntryDetailsControlBag.Instance.EffectiveToDateDateEdit), row1.Parts[1].Name);
			AssertEquals(nameof(ValuationEntryDetailsControlBag.Instance.ApprovalNumberTextBox), row2.Parts[1].Name);
			AssertEquals(nameof(ValuationEntryDetailsControlBag.Instance.ResultReasonTextBox), row3.Parts[1].Name);
		}
	}
}
