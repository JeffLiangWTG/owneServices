using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(MethodTwoToSixItemUseCodeLayout))]
	sealed class MethodTwoToSixItemUseCodeLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;
		protected override ICommonLayoutBuilder CommonLayoutBuilder => null;
		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn => null;
		protected override bool ShouldAssertThatIncludedControlsAreSorted => false;

		public new void TestIncludedControls()
		{
			IPanelLayoutProvider layout = new MethodTwoToSixItemUseCodeLayout();
			var columns = layout.Layout.Columns;
			AssertEquals(2, columns.Count);

			AssertEquals(4, columns[0].Rows.Count);
			var row1 = columns[0].Rows[0];
			var row2 = columns[0].Rows[1];
			var row3 = columns[0].Rows[2];
			var row4 = columns[0].Rows[3];

			AssertEquals(2, row1.Parts.Count);
			AssertEquals(nameof(MethodTwoToSixControlBag.SampleItemCheckBox), row1.Parts[1].Name);

			AssertEquals(2, row2.Parts.Count);
			AssertEquals(nameof(MethodTwoToSixControlBag.AdvertisingUseCheckBox), row2.Parts[1].Name);

			AssertEquals(2, row3.Parts.Count);
			AssertEquals(nameof(MethodTwoToSixControlBag.UseOfDefectiveRepairCheckBox), row3.Parts[1].Name);

			AssertEquals(2, row4.Parts.Count);
			AssertEquals(nameof(MethodTwoToSixControlBag.ReplacementItemCheckBox), row4.Parts[1].Name);

			AssertEquals(3, columns[1].Rows.Count);
			row1 = columns[1].Rows[0];
			row2 = columns[1].Rows[1];
			row3 = columns[1].Rows[2];

			AssertEquals(2, row1.Parts.Count);
			AssertEquals(nameof(MethodTwoToSixControlBag.GiftOrFreeDonationCheckBox), row1.Parts[1].Name);

			AssertEquals(2, row2.Parts.Count);
			AssertEquals(nameof(MethodTwoToSixControlBag.ForProductionAndManufactureCheckBox), row2.Parts[1].Name);

			AssertEquals(2, row3.Parts.Count);
			AssertEquals(nameof(MethodTwoToSixControlBag.ItemUseCodeOtherReasonTextBox), row3.Parts[1].Name);
		}
	}
}
