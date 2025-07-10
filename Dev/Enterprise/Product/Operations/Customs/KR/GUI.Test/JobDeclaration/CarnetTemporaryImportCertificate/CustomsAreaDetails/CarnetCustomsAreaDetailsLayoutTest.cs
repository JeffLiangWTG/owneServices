using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(CarnetCustomsAreaDetailsLayout))]
	sealed class CarnetCustomsAreaDetailsLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;
		protected override ICommonLayoutBuilder CommonLayoutBuilder => null;
		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn => null;
		protected override bool ShouldAssertThatIncludedControlsAreSorted => false;

		public new void TestIncludedControls()
		{
			IPanelLayoutProvider layout = new CarnetCustomsAreaDetailsLayout();
			var columns = layout.Layout.Columns;
			AssertEquals(1, columns.Count);

			AssertEquals(2, columns[0].Rows.Count);
			var row1 = columns[0].Rows[0];
			var row2 = columns[0].Rows[1];

			AssertEquals(4, row1.Parts.Count);
			AssertEquals(nameof(CarnetCustomsAreaDetailsControlBag.Instance.CustomsOfficeCodeFindBox), row1.Parts[1].Name);
			AssertEquals(nameof(CarnetCustomsAreaDetailsControlBag.Instance.DepartmentCodeFindBox), row1.Parts[3].Name);

			AssertEquals(2, row2.Parts.Count);
			AssertEquals(nameof(CarnetCustomsAreaDetailsControlBag.Instance.BondedAreaCodeFindBox), row2.Parts[1].Name);
		}
	}
}
