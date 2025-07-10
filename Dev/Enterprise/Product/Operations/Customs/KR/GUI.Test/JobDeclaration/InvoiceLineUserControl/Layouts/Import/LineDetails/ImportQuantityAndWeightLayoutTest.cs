using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(ImportQuantityAndWeightLayout))]
	sealed class ImportQuantityAndWeightLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;
		protected override ICommonLayoutBuilder CommonLayoutBuilder => null;
		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn => null;
		protected override bool ShouldAssertThatIncludedControlsAreSorted => false;

		public new void TestIncludedControls()
		{
			IPanelLayoutProvider layout = new ImportQuantityAndWeightLayout();
			var columns = layout.Layout.Columns;
			AssertEquals(1, columns.Count);

			AssertEquals(6, columns[0].Rows.Count);
			var row1 = columns[0].Rows[0];
			var row2 = columns[0].Rows[1];
			var row3 = columns[0].Rows[2];
			var row4 = columns[0].Rows[3];
			var row5 = columns[0].Rows[4];
			var row6 = columns[0].Rows[5];

			AssertEquals(4, row1.Parts.Count);
			AssertEquals(nameof(ImportInvoiceLineDetailsControlBag.Instance.CustomsQtyCalcDropEdit), row1.Parts[1].Name);
			AssertEquals(nameof(ImportInvoiceLineDetailsControlBag.Instance.CustomsUnitPriceCalcEdit), row1.Parts[3].Name);

			AssertEquals(4, row2.Parts.Count);
			AssertEquals(nameof(ImportInvoiceLineDetailsControlBag.Instance.CustomsSecondQuantityCalcDropEdit), row2.Parts[1].Name);
			AssertEquals(nameof(ImportInvoiceLineDetailsControlBag.Instance.CustomsThirdQuantityCalcDropEdit), row2.Parts[3].Name);

			AssertEquals(4, row3.Parts.Count);
			AssertEquals(nameof(ImportInvoiceLineDetailsControlBag.Instance.CustomsFourthQuantityCalcDropEdit), row3.Parts[1].Name);
			AssertEquals(nameof(ImportInvoiceLineDetailsControlBag.Instance.DrawBackQtyCalcDropEdit), row3.Parts[3].Name);

			AssertEquals(4, row4.Parts.Count);
			AssertEquals(nameof(ImportInvoiceLineDetailsControlBag.Instance.GrossWeightCalcDropEdit), row4.Parts[1].Name);
			AssertEquals(nameof(ImportInvoiceLineDetailsControlBag.Instance.NetWeightCalcDropEdit), row4.Parts[3].Name);

			AssertEquals(4, row5.Parts.Count);
			AssertEquals(nameof(ImportInvoiceLineDetailsControlBag.Instance.InvoiceQtyCalcDropEdit), row5.Parts[1].Name);
			AssertEquals(nameof(ImportInvoiceLineDetailsControlBag.Instance.UnitPriceCalcEdit), row5.Parts[3].Name);

			AssertEquals(4, row6.Parts.Count);
			AssertEquals(nameof(ImportInvoiceLineDetailsControlBag.Instance.PriceCalcFindBox), row6.Parts[1].Name);
			AssertEquals(nameof(ImportInvoiceLineDetailsControlBag.Instance.InstallationCostCalcEdit), row6.Parts[3].Name);
		}
	}
}
