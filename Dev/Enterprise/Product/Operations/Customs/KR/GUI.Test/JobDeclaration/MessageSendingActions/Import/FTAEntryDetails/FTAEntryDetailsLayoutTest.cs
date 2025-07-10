using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(FTAEntryDetailsLayout))]
	sealed class FTAEntryDetailsLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;
		protected override ICommonLayoutBuilder CommonLayoutBuilder => null;
		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn => null;
		protected override bool ShouldAssertThatIncludedControlsAreSorted => false;

		public new void TestIncludedControls()
		{
			IPanelLayoutProvider layout = new FTAEntryDetailsLayout();
			var columns = layout.Layout.Columns;
			AssertEquals(2, columns.Count);

			AssertEquals(7, columns[0].Rows.Count);
			var row1 = columns[0].Rows[0];
			var row2 = columns[0].Rows[1];
			var row3 = columns[0].Rows[2];
			var row4 = columns[0].Rows[3];
			var row5 = columns[0].Rows[4];
			var row6 = columns[0].Rows[5];
			var row7 = columns[0].Rows[6];

			AssertEquals(2, row1.Parts.Count);
			AssertEquals(nameof(FTAEntryDetailsControlBag.Instance.EntryNumberTextBox), row1.Parts[1].Name);

			AssertEquals(2, row2.Parts.Count);
			AssertEquals(nameof(FTAEntryDetailsControlBag.Instance.LawCodeDropEdit), row2.Parts[1].Name);

			AssertEquals(2, row3.Parts.Count);
			AssertEquals(nameof(FTAEntryDetailsControlBag.Instance.DepartureDateEdit), row3.Parts[1].Name);

			AssertEquals(2, row4.Parts.Count);
			AssertEquals(nameof(FTAEntryDetailsControlBag.Instance.DeparturePortCodeFindBox), row4.Parts[1].Name);

			AssertEquals(2, row5.Parts.Count);
			AssertEquals(nameof(FTAEntryDetailsControlBag.Instance.DepartureCountryCodeFindBox), row5.Parts[1].Name);

			AssertEquals(2, row6.Parts.Count);
			AssertEquals(nameof(FTAEntryDetailsControlBag.Instance.ManufacturerAddressControl), row6.Parts[1].Name);

			AssertEquals(2, row7.Parts.Count);
			AssertEquals(nameof(FTAEntryDetailsControlBag.Instance.ManufacturerAreaPostcodeTextBox), row7.Parts[1].Name);

			AssertEquals(7, columns[1].Rows.Count);
			row1 = columns[1].Rows[0];
			row2 = columns[1].Rows[1];
			row3 = columns[1].Rows[2];
			row4 = columns[1].Rows[3];
			row5 = columns[1].Rows[4];
			row6 = columns[1].Rows[5];
			row7 = columns[1].Rows[6];

			AssertEquals(2, row1.Parts.Count);
			AssertEquals(nameof(FTAEntryDetailsControlBag.Instance.CustomsDisbursementBillNoTextBox), row1.Parts[1].Name);

			AssertEquals(2, row2.Parts.Count);
			AssertEquals(nameof(FTAEntryDetailsControlBag.Instance.TransshipmentYNDropEdit), row2.Parts[1].Name);

			AssertEquals(2, row3.Parts.Count);
			AssertEquals(nameof(FTAEntryDetailsControlBag.Instance.TransshipmentDateEdit), row3.Parts[1].Name);

			AssertEquals(2, row4.Parts.Count);
			AssertEquals(nameof(FTAEntryDetailsControlBag.Instance.TransshipmentPortCodeFindBox), row4.Parts[1].Name);

			AssertEquals(2, row5.Parts.Count);
			AssertEquals(nameof(FTAEntryDetailsControlBag.Instance.TransshipmentCountryCodeFindBox), row5.Parts[1].Name);

			AssertEquals(2, row6.Parts.Count);
			AssertEquals(nameof(FTAEntryDetailsControlBag.Instance.ImporterAddressControl), row6.Parts[1].Name);

			AssertEquals(2, row7.Parts.Count);
			AssertEquals(nameof(FTAEntryDetailsControlBag.Instance.ExporterAddressControl), row7.Parts[1].Name);
		}
	}
}
