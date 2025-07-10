using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(AdditionalAdjustmentSendingObjectLayout))]
	sealed class AdditionalAdjustmentSendingObjectLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;
		protected override ICommonLayoutBuilder CommonLayoutBuilder => null;
		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn => null;
		protected override bool ShouldAssertThatIncludedControlsAreSorted => false;

		public new void TestIncludedControls()
		{
			IPanelLayoutProvider layout = new AdditionalAdjustmentSendingObjectLayout();
			var columns = layout.Layout.Columns;
			AssertEquals(1, columns.Count);

			AssertEquals(3, columns[0].Rows.Count);
			var row1 = columns[0].Rows[0];
			var row2 = columns[0].Rows[1];
			var row3 = columns[0].Rows[2];

			AssertEquals(4, row1.Parts.Count);
			AssertEquals(nameof(MethodTwoToThreeControlBag.AdditionalAdjustmentQuantityDiscountCalcEdit), row1.Parts[1].Name);
			AssertEquals(nameof(MethodTwoToThreeControlBag.AdditionalAdjustmentCommercialAmountCalcEdit), row1.Parts[3].Name);

			AssertEquals(4, row2.Parts.Count);
			AssertEquals(nameof(MethodTwoToThreeControlBag.AdditionalAdjustmentTransportationCostCalcEdit), row2.Parts[1].Name);
			AssertEquals(nameof(MethodTwoToThreeControlBag.AdditionalAdjustmentShippingPortCostCalcEdit), row2.Parts[3].Name);

			AssertEquals(4, row3.Parts.Count);
			AssertEquals(nameof(MethodTwoToThreeControlBag.AdditionalAdjustmentInsuranceCalcEdit), row3.Parts[1].Name);
			AssertEquals(nameof(MethodTwoToThreeControlBag.TotalAdditionalAdjustmentAmountCalcEdit), row3.Parts[3].Name);
		}
	}
}
