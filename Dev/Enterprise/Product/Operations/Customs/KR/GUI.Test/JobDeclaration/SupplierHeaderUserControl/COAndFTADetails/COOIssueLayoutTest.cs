using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(COOIssueLayout))]
	sealed class COOIssueLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;
		protected override ICommonLayoutBuilder CommonLayoutBuilder => null;
		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn => null;
		protected override bool ShouldAssertThatIncludedControlsAreSorted => false;

		public new void TestIncludedControls()
		{
			IPanelLayoutProvider layout = new COOIssueLayout();
			var columns = layout.Layout.Columns;
			AssertEquals(2, columns.Count);

			AssertEquals(4, columns[0].Rows.Count);
			var row1 = columns[0].Rows[0];
			var row2 = columns[0].Rows[1];
			var row3 = columns[0].Rows[2];
			var row4 = columns[0].Rows[3];

			AssertEquals(2, row1.Parts.Count);
			AssertEquals(nameof(COOandFTAControlBag.InstanceForInvoicerHeader.COReferenceNumberTextBox), row1.Parts[1].Name);

			AssertEquals(2, row2.Parts.Count);
			AssertEquals(nameof(COOandFTAControlBag.InstanceForInvoicerHeader.COIssuingCountryCodeFindBox), row2.Parts[1].Name);

			AssertEquals(2, row3.Parts.Count);
			AssertEquals(nameof(COOandFTAControlBag.InstanceForInvoicerHeader.COIssueDateEdit), row3.Parts[1].Name);

			AssertEquals(2, row4.Parts.Count);
			AssertEquals(nameof(COOandFTAControlBag.InstanceForInvoicerHeader.COSplitYNDropEdit), row4.Parts[1].Name);

			AssertEquals(4, columns[1].Rows.Count);
			row1 = columns[1].Rows[0];
			row2 = columns[1].Rows[1];
			row3 = columns[1].Rows[2];
			row4 = columns[1].Rows[3];

			AssertEquals(2, row1.Parts.Count);
			AssertEquals(nameof(COOandFTAControlBag.InstanceForInvoicerHeader.IssuingAgencyNameTextBox), row1.Parts[1].Name);

			AssertEquals(2, row2.Parts.Count);
			AssertEquals(nameof(COOandFTAControlBag.InstanceForInvoicerHeader.IssuingAreaNameTextBox), row2.Parts[1].Name);

			AssertEquals(2, row3.Parts.Count);
			AssertEquals(nameof(COOandFTAControlBag.InstanceForInvoicerHeader.IssuingPersonNameTextBox), row3.Parts[1].Name);

			AssertEquals(2, row4.Parts.Count);
			AssertEquals(nameof(COOandFTAControlBag.InstanceForInvoicerHeader.COCodeDropEdit), row4.Parts[1].Name);
		}
	}
}
