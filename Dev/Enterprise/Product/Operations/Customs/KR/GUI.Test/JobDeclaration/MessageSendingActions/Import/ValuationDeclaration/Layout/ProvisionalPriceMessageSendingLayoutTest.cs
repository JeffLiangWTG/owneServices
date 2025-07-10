using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(ProvisionalPriceMessageSendingLayout))]
	sealed class ProvisionalPriceMessageSendingLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;
		protected override ICommonLayoutBuilder CommonLayoutBuilder => null;
		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn => null;
		protected override bool ShouldAssertThatIncludedControlsAreSorted => false;

		public new void TestIncludedControls()
		{
			IPanelLayoutProvider layout = new ProvisionalPriceMessageSendingLayout();
			var columns = layout.Layout.Columns;
			AssertEquals(1, columns.Count);

			AssertEquals(2, columns[0].Rows.Count);
			var row1 = columns[0].Rows[0];
			var row2 = columns[0].Rows[1];

			AssertEquals(6, row1.Parts.Count);
			AssertEquals(nameof(DetailsAndProvisionalPriceControlBag.InstanceForMessageSendingObject.ProvisionalPricingDropEdit), row1.Parts[1].Name);
			AssertEquals(nameof(DetailsAndProvisionalPriceControlBag.InstanceForMessageSendingObject.ProvisionalAdditionRateCalcEdit), row1.Parts[3].Name);
			AssertEquals(nameof(DetailsAndProvisionalPriceControlBag.InstanceForMessageSendingObject.ProvisionalAdditionAmountCalcEdit), row1.Parts[5].Name);

			AssertEquals(4, row2.Parts.Count);
			AssertEquals(nameof(DetailsAndProvisionalPriceControlBag.InstanceForMessageSendingObject.EstimatedDateOfFinalPriceDateEdit), row2.Parts[1].Name);
			AssertEquals(nameof(DetailsAndProvisionalPriceControlBag.InstanceForMessageSendingObject.ContractExpirationDateEdit), row2.Parts[3].Name);
		}
	}
}
