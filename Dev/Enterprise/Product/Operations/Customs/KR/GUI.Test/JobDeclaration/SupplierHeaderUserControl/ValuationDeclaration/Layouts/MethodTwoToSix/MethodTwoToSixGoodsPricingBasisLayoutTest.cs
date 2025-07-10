using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(MethodTwoToSixGoodsPricingBasisLayout))]
	sealed class MethodTwoToSixGoodsPricingBasisLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;
		protected override ICommonLayoutBuilder CommonLayoutBuilder => null;
		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn => null;
		protected override bool ShouldAssertThatIncludedControlsAreSorted => false;

		public new void TestIncludedControls()
		{
			IPanelLayoutProvider layout = new MethodTwoToSixGoodsPricingBasisLayout();
			var columns = layout.Layout.Columns;
			AssertEquals(2, columns.Count);

			AssertEquals(3, columns[0].Rows.Count);
			var row1 = columns[0].Rows[0];
			var row2 = columns[0].Rows[1];
			var row3 = columns[0].Rows[2];

			AssertEquals(2, row1.Parts.Count);
			AssertEquals(nameof(MethodTwoToSixControlBag.PerformancePriceOfPaidTransactionCheckBox), row1.Parts[1].Name);

			AssertEquals(2, row2.Parts.Count);
			AssertEquals(nameof(MethodTwoToSixControlBag.PriceListCheckBox), row2.Parts[1].Name);

			AssertEquals(2, row3.Parts.Count);
			AssertEquals(nameof(MethodTwoToSixControlBag.ManufacturingCostCheckBox), row3.Parts[1].Name);

			AssertEquals(2, columns[1].Rows.Count);
			row1 = columns[1].Rows[0];
			row2 = columns[1].Rows[1];

			AssertEquals(2, row1.Parts.Count);
			AssertEquals(nameof(MethodTwoToSixControlBag.InvoiceCheckBox), row1.Parts[1].Name);

			AssertEquals(2, row2.Parts.Count);
			AssertEquals(nameof(MethodTwoToSixControlBag.GoodsPricingBasisOtherReasonTextBox), row2.Parts[1].Name);
		}
	}
}
