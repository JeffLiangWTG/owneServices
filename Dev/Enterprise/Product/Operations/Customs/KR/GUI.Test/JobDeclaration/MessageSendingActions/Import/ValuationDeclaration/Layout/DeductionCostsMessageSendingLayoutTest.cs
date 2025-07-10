using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(DeductionCostsMessageSendingLayout))]
	sealed class DeductionCostsMessageSendingLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;
		protected override ICommonLayoutBuilder CommonLayoutBuilder => null;
		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn => null;
		protected override bool ShouldAssertThatIncludedControlsAreSorted => false;

		public new void TestIncludedControls()
		{
			IPanelLayoutProvider layout = new DeductionCostsLayout();
			var columns = layout.Layout.Columns;
			AssertEquals(1, columns.Count);

			AssertEquals(2, columns[0].Rows.Count);
			var row1 = columns[0].Rows[0];
			var row2 = columns[0].Rows[1];

			AssertEquals(4, row1.Parts.Count);
			AssertEquals(nameof(PriceControlBag.InstanceForMessageSendingObject.LocalTransportationCostCalcEdit), row1.Parts[1].Name);
			AssertEquals(nameof(PriceControlBag.InstanceForMessageSendingObject.TechnicalCostCalcEdit), row1.Parts[3].Name);

			AssertEquals(6, row2.Parts.Count);
			AssertEquals(nameof(PriceControlBag.InstanceForMessageSendingObject.OtherCostsCalcEdit), row2.Parts[1].Name);
			AssertEquals(nameof(PriceControlBag.InstanceForMessageSendingObject.DiscountAmountCalcEdit), row2.Parts[3].Name);
			AssertEquals(nameof(PriceControlBag.InstanceForMessageSendingObject.TotalDeductionAmountCalcEdit), row2.Parts[5].Name);
		}
	}
}
