using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(DeductionCostSendingObjectLayout))]
	sealed class DedectionCostSendingObjectLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;
		protected override ICommonLayoutBuilder CommonLayoutBuilder => null;
		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn => null;
		protected override bool ShouldAssertThatIncludedControlsAreSorted => false;

		public new void TestIncludedControls()
		{
			IPanelLayoutProvider layout = new DeductionCostSendingObjectLayout();
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
			AssertEquals(nameof(MethodFourControlBag.DeductionCostCustomsReferenceNumberTextBox), row1.Parts[1].Name);
			AssertEquals(nameof(MethodFourControlBag.DeductionCostConsignmentSalesFeeCalcEdit), row1.Parts[3].Name);

			AssertEquals(4, row2.Parts.Count);
			AssertEquals(nameof(MethodFourControlBag.DeductionCostGeneralCostCalcEdit), row2.Parts[1].Name);
			AssertEquals(nameof(MethodFourControlBag.DeductionCostCostRateCodeDropEdit), row2.Parts[3].Name);

			AssertEquals(6, row3.Parts.Count);
			AssertEquals(nameof(MethodFourControlBag.DeductionCostCostRateCalcEdit), row3.Parts[1].Name);
			AssertEquals(nameof(MethodFourControlBag.PercentageLabel), row3.Parts[3].Name);
			AssertEquals(nameof(MethodFourControlBag.DeductionCostTransportationCostCalcEdit), row3.Parts[5].Name);

			AssertEquals(4, row4.Parts.Count);
			AssertEquals(nameof(MethodFourControlBag.DeductionCosInsuranceCalcEdit), row4.Parts[1].Name);
			AssertEquals(nameof(MethodFourControlBag.DeductionCostUnloadCostCalcEdit), row4.Parts[3].Name);

			AssertEquals(4, row5.Parts.Count);
			AssertEquals(nameof(MethodFourControlBag.DeductionCostOtherTransportationCostsCalcEdit), row5.Parts[1].Name);
			AssertEquals(nameof(MethodFourControlBag.DeductionCostAdditionalCostCalcEdit), row5.Parts[3].Name);

			AssertEquals(4, row6.Parts.Count);
			AssertEquals(nameof(MethodFourControlBag.DeductionCosTaxCalcEdit), row6.Parts[1].Name);
			AssertEquals(nameof(MethodFourControlBag.DeductionCostTotalDeductionAmountCalcEdit), row6.Parts[3].Name);
		}
	}
}
