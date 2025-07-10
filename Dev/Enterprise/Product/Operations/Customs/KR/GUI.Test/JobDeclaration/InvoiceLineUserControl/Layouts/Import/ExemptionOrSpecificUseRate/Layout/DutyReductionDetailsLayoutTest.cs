using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(DutyReductionDetailsLayout))]
	sealed class DutyReductionDetailsLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;
		protected override ICommonLayoutBuilder CommonLayoutBuilder => null;
		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn => null;
		protected override bool ShouldAssertThatIncludedControlsAreSorted => false;

		public new void TestIncludedControls()
		{
			IPanelLayoutProvider layout = new DutyReductionDetailsLayout();
			var columns = layout.Layout.Columns;
			AssertEquals(1, columns.Count);

			AssertEquals(2, columns[0].Rows.Count);
			var row1 = columns[0].Rows[0];
			var row2 = columns[0].Rows[1];

			AssertEquals(2, row1.Parts.Count);
			AssertEquals(nameof(DutyReductionDetailsControlBag.InstanceForDeclaration.GroupNumberDropEdit), row1.Parts[1].Name);

			AssertEquals(4, row2.Parts.Count);
			AssertEquals(nameof(DutyReductionDetailsControlBag.InstanceForDeclaration.SeqNumberTextBox), row2.Parts[1].Name);
			AssertEquals(nameof(DutyReductionDetailsControlBag.InstanceForDeclaration.ItemNumberTextBox), row2.Parts[3].Name);
		}
	}
}
