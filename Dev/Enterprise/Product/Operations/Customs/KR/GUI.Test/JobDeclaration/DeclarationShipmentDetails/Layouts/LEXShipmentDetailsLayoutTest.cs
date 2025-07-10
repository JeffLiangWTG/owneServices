using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(LEXShipmentDetailsLayout))]
	sealed class LEXShipmentDetailsLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 2;
		protected override ICommonLayoutBuilder CommonLayoutBuilder => null;
		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn => null;
		protected override bool ShouldAssertThatIncludedControlsAreSorted => false;

		public new void TestIncludedControls()
		{
			IPanelLayoutProvider layout = new LEXShipmentDetailsLayout();
			var columns = layout.Layout.Columns;
			AssertEquals(1, columns.Count);

			AssertEquals(6, columns[0].Rows.Count);
			var row1 = columns[0].Rows[0];
			var row2 = columns[0].Rows[1];
			var row3 = columns[0].Rows[2];
			var row4 = columns[0].Rows[3];
			var row5 = columns[0].Rows[4];
			var row6 = columns[0].Rows[5];

			AssertEquals(3, row1.Parts.Count);
			AssertEquals(nameof(Customs.GUI.ShipmentDetailsControlBag.Instance.GoodsDescriptionTextBox), row1.Parts[1].Name);

			AssertEquals(3, row2.Parts.Count);
			AssertEquals(nameof(Customs.GUI.ShipmentDetailsControlBag.Instance.OwnersReferenceTextBox), row2.Parts[1].Name);

			AssertEquals(4, row3.Parts.Count);
			AssertEquals(nameof(ShipmentDetailsControlBag.Instance.WeightCalcDropEdit), row3.Parts[1].Name);
			AssertEquals(nameof(ShipmentDetailsControlBag.Instance.VolumeCalcDropEdit), row3.Parts[3].Name);

			AssertEquals(2, row4.Parts.Count);
			AssertEquals(nameof(Customs.GUI.ShipmentDetailsControlBag.Instance.TotalNoOfPacksCalcDropEdit), row4.Parts[1].Name);

			AssertEquals(2, row5.Parts.Count);
			AssertEquals(nameof(Customs.GUI.ShipmentDetailsControlBag.Instance.ShipmentDetailsIncoTermsUserControl), row5.Parts[1].Name);

			AssertEquals(2, row6.Parts.Count);
			AssertEquals(nameof(ShipmentDetailsControlBag.Instance.ShipmentDetailsScreeningUserControl), row6.Parts[1].Name);
		}
	}
}
