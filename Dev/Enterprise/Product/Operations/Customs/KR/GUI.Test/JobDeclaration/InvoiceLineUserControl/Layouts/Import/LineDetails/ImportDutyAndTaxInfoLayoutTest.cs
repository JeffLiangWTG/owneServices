using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(ImportDutyAndTaxInfoLayout))]
	sealed class ImportDutyAndTaxInfoLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;
		protected override ICommonLayoutBuilder CommonLayoutBuilder => null;
		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn => null;
		protected override bool ShouldAssertThatIncludedControlsAreSorted => false;

		public new void TestIncludedControls()
		{
			IPanelLayoutProvider layout = new ImportDutyAndTaxInfoLayout();
			var columns = layout.Layout.Columns;
			AssertEquals(1, columns.Count);

			AssertEquals(15, columns[0].Rows.Count);
			var row1 = columns[0].Rows[0];
			var row2 = columns[0].Rows[1];
			var row3 = columns[0].Rows[2];
			var row4 = columns[0].Rows[3];
			var row5 = columns[0].Rows[4];
			var row6 = columns[0].Rows[5];
			var row7 = columns[0].Rows[6];
			var row8 = columns[0].Rows[7];
			var row9 = columns[0].Rows[8];
			var row10 = columns[0].Rows[9];
			var row11 = columns[0].Rows[10];
			var row12 = columns[0].Rows[11];
			var row13 = columns[0].Rows[12];
			var row14 = columns[0].Rows[13];
			var row15 = columns[0].Rows[14];

			AssertEquals(2, row1.Parts.Count);
			AssertEquals(nameof(ImportInvoiceLineDetailsControlBag.Instance.DutyRateTypeDropEdit), row1.Parts[1].Name);

			AssertEquals(2, row2.Parts.Count);
			AssertEquals(nameof(ImportInvoiceLineDetailsControlBag.Instance.MinMaxDutyDropEdit), row2.Parts[1].Name);

			AssertEquals(4, row3.Parts.Count);
			AssertEquals(nameof(ImportInvoiceLineDetailsControlBag.Instance.PreferenceCodeDropEdit), row3.Parts[1].Name);
			AssertEquals(nameof(ImportInvoiceLineDetailsControlBag.Instance.DutyRateCalcEdit), row3.Parts[3].Name);

			AssertEquals(4, row4.Parts.Count);
			AssertEquals(nameof(ImportInvoiceLineDetailsControlBag.Instance.DutyCodeDropEdit), row4.Parts[1].Name);
			AssertEquals(nameof(ImportInvoiceLineDetailsControlBag.Instance.AddDutyRateCalcEdit), row4.Parts[3].Name);

			AssertEquals(4, row5.Parts.Count);
			AssertEquals(nameof(ImportInvoiceLineDetailsControlBag.Instance.DutyReductionCodeFindBox), row5.Parts[1].Name);
			AssertEquals(nameof(ImportInvoiceLineDetailsControlBag.Instance.DutyReductionRateCalcEdit), row5.Parts[3].Name);

			AssertEquals(2, row6.Parts.Count);
			AssertEquals(nameof(ImportInvoiceLineDetailsControlBag.Instance.InstalmentCodeFindBox), row6.Parts[1].Name);

			AssertEquals(4, row7.Parts.Count);
			AssertEquals(nameof(ImportInvoiceLineDetailsControlBag.Instance.SpecificUsePermitNoTextBox), row7.Parts[1].Name);
			AssertEquals(nameof(ImportInvoiceLineDetailsControlBag.Instance.SpecificUseCheckBox), row7.Parts[3].Name);

			AssertEquals(4, row8.Parts.Count);
			AssertEquals(nameof(ImportInvoiceLineDetailsControlBag.Instance.DomesticTaxCodeFindBox), row8.Parts[1].Name);
			AssertEquals(nameof(ImportInvoiceLineDetailsControlBag.Instance.DomesticTaxTypeDropEdit), row8.Parts[3].Name);

			AssertEquals(2, row9.Parts.Count);
			AssertEquals(nameof(ImportInvoiceLineDetailsControlBag.Instance.DomesticTaxRateCalcEdit), row9.Parts[1].Name);

			AssertEquals(2, row10.Parts.Count);
			AssertEquals(nameof(ImportInvoiceLineDetailsControlBag.Instance.DomesticTaxExemptionCodeFindBox), row10.Parts[1].Name);

			AssertEquals(2, row11.Parts.Count);
			AssertEquals(nameof(ImportInvoiceLineDetailsControlBag.Instance.DomesticTaxBaseQtyOrPriceCalcEdit), row11.Parts[1].Name);

			AssertEquals(2, row12.Parts.Count);
			AssertEquals(nameof(ImportInvoiceLineDetailsControlBag.Instance.VATTypeDropEdit), row12.Parts[1].Name);

			AssertEquals(2, row13.Parts.Count);
			AssertEquals(nameof(ImportInvoiceLineDetailsControlBag.Instance.VATReductionCodeFindBox), row13.Parts[1].Name);

			AssertEquals(2, row14.Parts.Count);
			AssertEquals(nameof(ImportInvoiceLineDetailsControlBag.Instance.EducationTaxTypeDropEdit), row14.Parts[1].Name);

			AssertEquals(2, row15.Parts.Count);
			AssertEquals(nameof(ImportInvoiceLineDetailsControlBag.Instance.AgricultureTaxTypeDropEdit), row15.Parts[1].Name);
		}
	}
}
