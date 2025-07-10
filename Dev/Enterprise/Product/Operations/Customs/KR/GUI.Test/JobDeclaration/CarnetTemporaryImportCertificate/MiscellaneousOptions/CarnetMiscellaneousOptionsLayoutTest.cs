using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(CarnetMiscellaneousOptionsLayout))]
	sealed class CarnetMiscellaneousOptionsLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;
		protected override ICommonLayoutBuilder CommonLayoutBuilder => null;
		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn => null;
		protected override bool ShouldAssertThatIncludedControlsAreSorted => false;

		public new void TestIncludedControls()
		{
			IPanelLayoutProvider layout = new CarnetMiscellaneousOptionsLayout();
			var columns = layout.Layout.Columns;
			AssertEquals(1, columns.Count);

			AssertEquals(1, columns[0].Rows.Count);
			var row1 = columns[0].Rows[0];

			AssertEquals(4, row1.Parts.Count);
			AssertEquals(nameof(CarnetMiscellaneousOptionsControlBag.Instance.BranchGuidFindBox), row1.Parts[1].Name);
			AssertEquals(nameof(CarnetMiscellaneousOptionsControlBag.Instance.BrokerCodeFindBox), row1.Parts[3].Name);
		}
	}
}
