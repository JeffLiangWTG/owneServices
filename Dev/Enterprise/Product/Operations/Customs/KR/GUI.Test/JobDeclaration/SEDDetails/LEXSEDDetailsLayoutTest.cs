using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(LEXSEDDetailsLayout))]
	sealed class LEXSEDDetailsLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;
		protected override ICommonLayoutBuilder CommonLayoutBuilder => null;
		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn => null;
		protected override bool ShouldAssertThatIncludedControlsAreSorted => false;

		public new void TestIncludedControls()
		{
			IPanelLayoutProvider layout = new LEXSEDDetailsLayout();
			var columns = layout.Layout.Columns;
			AssertEquals(1, columns.Count);

			AssertEquals(5, columns[0].Rows.Count);
			var row1 = columns[0].Rows[0];
			var row2 = columns[0].Rows[1];
			var row3 = columns[0].Rows[2];
			var row4 = columns[0].Rows[3];
			var row5 = columns[0].Rows[4];

			AssertEquals(4, row1.Parts.Count);
			AssertEquals("CustomsOfficeCodeFindBox", row1.Parts[1].Name);
			AssertEquals("CustomsDivisionCodeFindBox", row1.Parts[3].Name);

			AssertEquals(4, row2.Parts.Count);
			AssertEquals("BondedAreaCodeFindBox", row2.Parts[1].Name);
			AssertEquals("CrewCountCalcEdit", row2.Parts[3].Name);

			AssertEquals(2, row3.Parts.Count);
			AssertEquals("SubLocationOfGoodsTextBox", row3.Parts[1].Name);

			AssertEquals(4, row4.Parts.Count);
			AssertEquals("BlanketDeclarationDropEdit", row4.Parts[1].Name);
			AssertEquals("DeclarationDateEdit", row4.Parts[3].Name);

			AssertEquals(1, row5.Parts.Count);
			AssertEquals("GridUserControl", row5.Parts[0].Name);
		}
	}
}
