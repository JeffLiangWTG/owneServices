using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(CustomsDetailLayout))]
	sealed class CustomsDetailLayouttTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;
		protected override ICommonLayoutBuilder CommonLayoutBuilder => null;
		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn => null;
		protected override bool ShouldAssertThatIncludedControlsAreSorted => false;

		public new void TestIncludedControls()
		{
			IPanelLayoutProvider layout = new CustomsDetailLayout();
			var columns = layout.Layout.Columns;
			AssertEquals(1, columns.Count);

			AssertEquals(8,  columns[0].Rows.Count);
			var row1 = columns[0].Rows[0];
			var row2 = columns[0].Rows[1];
			var row3 = columns[0].Rows[2];
			var row4 = columns[0].Rows[3];
			var row5 = columns[0].Rows[4];
			var row6 = columns[0].Rows[5];
			var row7 = columns[0].Rows[6];
			var row8 = columns[0].Rows[7];

			AssertEquals(2, row1.Parts.Count);
			AssertEquals(nameof(RefundDeclarationControlBag.RefundTypeDropEdit), row1.Parts[1].Name);

			AssertEquals(2,  row2.Parts.Count);
			AssertEquals(nameof(RefundDeclarationControlBag.RefundCauseDropEdit), row2.Parts[1].Name);

			AssertEquals(2, row3.Parts.Count);
			AssertEquals(nameof(RefundDeclarationControlBag.RefundReasonDropEdit), row3.Parts[1].Name);

			AssertEquals(2, row4.Parts.Count);
			AssertEquals(nameof(RefundDeclarationControlBag.CustomsOfficeCodeFindBox), row4.Parts[1].Name);

			AssertEquals(2, row5.Parts.Count);
			AssertEquals(nameof(RefundDeclarationControlBag.DepartmentCodeFindBox), row5.Parts[1].Name);

			AssertEquals(2, row6.Parts.Count);
			AssertEquals(nameof(RefundDeclarationControlBag.TaxOfficeCodeFindBox), row6.Parts[1].Name);

			AssertEquals(2, row7.Parts.Count);
			AssertEquals(nameof(RefundDeclarationControlBag.BranchCodeGuidFindBox), row7.Parts[1].Name);

			AssertEquals(2, row8.Parts.Count);
			AssertEquals(nameof(RefundDeclarationControlBag.BrokerCodeFindBox), row8.Parts[1].Name);
		}
	}
}
