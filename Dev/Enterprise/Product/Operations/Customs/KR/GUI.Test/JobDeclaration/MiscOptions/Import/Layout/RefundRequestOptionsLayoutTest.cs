using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(RefundRequestOptionsLayout))]
	sealed class RefundRequestOptionsLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => null;
		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn => null;
		protected override bool ShouldAssertThatIncludedControlsAreSorted => false;

		public new void TestIncludedControls()
		{
			IPanelLayoutProvider layout = new RefundRequestOptionsLayout();
			var columns = layout.Layout.Columns;
			AssertEquals(1, columns.Count);

			AssertEquals(1, columns[0].Rows.Count);
			var row1 = columns[0].Rows[0];

			AssertEquals(2, row1.Parts.Count);
			AssertEquals(nameof(MiscOptionsControlBag.Instance.TaxOfficeCodeFindBox), row1.Parts[1].Name);
		}
	}
}
