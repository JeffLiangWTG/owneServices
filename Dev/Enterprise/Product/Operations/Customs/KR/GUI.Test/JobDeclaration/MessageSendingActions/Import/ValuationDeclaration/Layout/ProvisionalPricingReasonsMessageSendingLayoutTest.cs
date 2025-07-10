using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(ProvisionalPricingReasonsMessageSendingLayout))]
	sealed class ProvisionalPricingReasonsMessageSendingLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;
		protected override ICommonLayoutBuilder CommonLayoutBuilder => null;
		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn => null;
		protected override bool ShouldAssertThatIncludedControlsAreSorted => false;

		public new void TestIncludedControls()
		{
			IPanelLayoutProvider layout = new ProvisionalPricingReasonsMessageSendingLayout();
			var columns = layout.Layout.Columns;
			AssertEquals(1, columns.Count);

			AssertEquals(10, columns[0].Rows.Count);
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

			AssertEquals(8, row1.Parts.Count);
			AssertEquals(nameof(ProvisionalPricingReasonsControlBag.InstanceForMessageSendingObject.ProvisionalPricingReason101CheckBox), row1.Parts[1].Name);
			AssertEquals(nameof(ProvisionalPricingReasonsControlBag.InstanceForMessageSendingObject.ProvisionalPricingReason102CheckBox), row1.Parts[3].Name);
			AssertEquals(nameof(ProvisionalPricingReasonsControlBag.InstanceForMessageSendingObject.ProvisionalPricingReason103CheckBox), row1.Parts[5].Name);
			AssertEquals(nameof(ProvisionalPricingReasonsControlBag.InstanceForMessageSendingObject.ProvisionalPricingReason104CheckBox), row1.Parts[7].Name);

			AssertEquals(8, row2.Parts.Count);
			AssertEquals(nameof(ProvisionalPricingReasonsControlBag.InstanceForMessageSendingObject.ProvisionalPricingReason105CheckBox), row2.Parts[1].Name);
			AssertEquals(nameof(ProvisionalPricingReasonsControlBag.InstanceForMessageSendingObject.ProvisionalPricingReason106CheckBox), row2.Parts[3].Name);
			AssertEquals(nameof(ProvisionalPricingReasonsControlBag.InstanceForMessageSendingObject.ProvisionalPricingReason107CheckBox), row2.Parts[5].Name);
			AssertEquals(nameof(ProvisionalPricingReasonsControlBag.InstanceForMessageSendingObject.ProvisionalPricingReason108CheckBox), row2.Parts[7].Name);

			AssertEquals(8, row3.Parts.Count);
			AssertEquals(nameof(ProvisionalPricingReasonsControlBag.InstanceForMessageSendingObject.ProvisionalPricingReason109CheckBox), row3.Parts[1].Name);
			AssertEquals(nameof(ProvisionalPricingReasonsControlBag.InstanceForMessageSendingObject.ProvisionalPricingReason110CheckBox), row3.Parts[3].Name);
			AssertEquals(nameof(ProvisionalPricingReasonsControlBag.InstanceForMessageSendingObject.ProvisionalPricingReason111CheckBox), row3.Parts[5].Name);
			AssertEquals(nameof(ProvisionalPricingReasonsControlBag.InstanceForMessageSendingObject.ProvisionalPricingReason112CheckBox), row3.Parts[7].Name);

			AssertEquals(2, row4.Parts.Count);
			AssertEquals(nameof(ProvisionalPricingReasonsControlBag.InstanceForMessageSendingObject.ProvisionalPricingReason113CheckBox), row4.Parts[1].Name);

			AssertEquals(2, row5.Parts.Count);
			AssertEquals(nameof(ProvisionalPricingReasonsControlBag.InstanceForMessageSendingObject.ProvisionalPricingReason114CheckBox), row5.Parts[1].Name);

			AssertEquals(2, row6.Parts.Count);
			AssertEquals(nameof(ProvisionalPricingReasonsControlBag.InstanceForMessageSendingObject.ProvisionalPricingReason115CheckBox), row6.Parts[1].Name);

			AssertEquals(2, row7.Parts.Count);
			AssertEquals(nameof(ProvisionalPricingReasonsControlBag.InstanceForMessageSendingObject.ProvisionalPricingReason116CheckBox), row7.Parts[1].Name);

			AssertEquals(2, row8.Parts.Count);
			AssertEquals(nameof(ProvisionalPricingReasonsControlBag.InstanceForMessageSendingObject.ProvisionalPricingReason117CheckBox), row8.Parts[1].Name);

			AssertEquals(2, row9.Parts.Count);
			AssertEquals(nameof(ProvisionalPricingReasonsControlBag.InstanceForMessageSendingObject.ProvisionalPricingReason120CheckBox), row9.Parts[1].Name);

			AssertEquals(2, row10.Parts.Count);
			AssertEquals(nameof(ProvisionalPricingReasonsControlBag.InstanceForMessageSendingObject.OtherReasonTextBox), row10.Parts[1].Name);
		}
	}
}
