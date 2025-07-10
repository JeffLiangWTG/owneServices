using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(ValuationDetailsLayout))]
	sealed class ValuationDetailsLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => null;
		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn => null;
		protected override bool ShouldAssertThatIncludedControlsAreSorted => false;

		public new void TestIncludedControls()
		{
			IPanelLayoutProvider layout = new ValuationDetailsLayout();
			var columns = layout.Layout.Columns;
			AssertEquals(1, columns.Count);

			var rows = columns[0].Rows;
			AssertEquals(6, rows.Count);
			AssertEquals(nameof(ValuationDetailsControlBag.Instance.ProductCodeCodeFindBox), rows[0].Parts[1].Name);
			AssertEquals(nameof(ValuationDetailsControlBag.Instance.TariffFindBox), rows[1].Parts[1].Name);
			AssertEquals(nameof(ValuationDetailsControlBag.Instance.DescriptionLongTextControl), rows[2].Parts[1].Name);
			AssertEquals(nameof(ValuationDetailsControlBag.Instance.ModelTradeNameTextBox), rows[3].Parts[1].Name);
			AssertEquals(nameof(ValuationDetailsControlBag.Instance.BrandNameTextBox), rows[4].Parts[1].Name);
			AssertEquals(nameof(ValuationDetailsControlBag.Instance.IngredientLongTextControl), rows[5].Parts[1].Name);
		}
	}
}
