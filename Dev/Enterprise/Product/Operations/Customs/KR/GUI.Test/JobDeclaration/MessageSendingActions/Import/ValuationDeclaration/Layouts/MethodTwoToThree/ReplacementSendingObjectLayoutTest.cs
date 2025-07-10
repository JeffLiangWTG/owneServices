using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(ReplacementSendingObjectLayout))]
	sealed class ReplacementSendingObjectLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;
		protected override ICommonLayoutBuilder CommonLayoutBuilder => null;
		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn => null;
		protected override bool ShouldAssertThatIncludedControlsAreSorted => false;

		public new void TestIncludedControls()
		{
			IPanelLayoutProvider layout = new ReplacementSendingObjectLayout();
			var columns = layout.Layout.Columns;
			AssertEquals(1, columns.Count);

			AssertEquals(2, columns[0].Rows.Count);
			var row1 = columns[0].Rows[0];
			var row2 = columns[0].Rows[1];

			AssertEquals(4, row1.Parts.Count);
			AssertEquals(nameof(MethodTwoToThreeControlBag.ReplacementAmountCalcFindBox), row1.Parts[1].Name);
			AssertEquals(nameof(MethodTwoToThreeControlBag.ReplacementExchangeRateCalcEdit), row1.Parts[3].Name);

			AssertEquals(2, row2.Parts.Count);
			AssertEquals(nameof(MethodTwoToThreeControlBag.ReplacementAmountKRWCalcEdit), row2.Parts[1].Name);
		}
	}
}
