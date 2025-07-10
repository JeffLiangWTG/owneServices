using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(ImportInvoiceLineDetailsLayout))]
	sealed class ImportInvoiceLineDetailsLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;
		protected override ICommonLayoutBuilder CommonLayoutBuilder => null;
		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn => null;
		protected override bool ShouldAssertThatIncludedControlsAreSorted => false;

		public new void TestIncludedControls()
		{
			IPanelLayoutProvider layout = new ImportInvoiceLineDetailsLayout();
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
			AssertEquals(nameof(ImportInvoiceLineDetailsControlBag.Instance.ProductCodeFindBox), row1.Parts[1].Name);
			AssertEquals(nameof(ImportInvoiceLineDetailsControlBag.Instance.LotNumberTextBox), row1.Parts[3].Name);

			AssertEquals(2, row2.Parts.Count);
			AssertEquals(nameof(ImportInvoiceLineDetailsControlBag.Instance.TariffFindBox), row2.Parts[1].Name);

			AssertEquals(2, row3.Parts.Count);
			AssertEquals(nameof(ImportInvoiceLineDetailsControlBag.Instance.GoodsDescriptionLongTextControl), row3.Parts[1].Name);

			AssertEquals(4, row4.Parts.Count);
			AssertEquals(nameof(ImportInvoiceLineDetailsControlBag.Instance.BrandCodeFindBox), row4.Parts[1].Name);
			AssertEquals(nameof(ImportInvoiceLineDetailsControlBag.Instance.BrandNameTextBox), row4.Parts[3].Name);

			AssertEquals(2, row5.Parts.Count);
			AssertEquals(nameof(ImportInvoiceLineDetailsControlBag.Instance.ModelTradeNameTextBox), row5.Parts[1].Name);

			AssertEquals(2, row6.Parts.Count);
			AssertEquals(nameof(ImportInvoiceLineDetailsControlBag.Instance.IngredientLongTextControl), row6.Parts[1].Name);
		}
	}
}
