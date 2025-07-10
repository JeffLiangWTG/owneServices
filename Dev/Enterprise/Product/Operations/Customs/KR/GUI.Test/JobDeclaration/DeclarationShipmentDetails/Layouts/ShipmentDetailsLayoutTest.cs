using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(ShipmentDetailsLayout))]
	sealed class ShipmentDetailsLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 2;
		protected override ICommonLayoutBuilder CommonLayoutBuilder => null;
		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn => null;
		protected override bool ShouldAssertThatIncludedControlsAreSorted => false;

		public new void TestIncludedControls()
		{
			IPanelLayoutProvider layout = new ShipmentDetailsLayout();
			var columns = layout.Layout.Columns;
			AssertEquals(1, columns.Count);

			AssertEquals(9, columns[0].Rows.Count);
			var row1 = columns[0].Rows[0];
			var row2 = columns[0].Rows[1];
			var row3 = columns[0].Rows[2];
			var row4 = columns[0].Rows[3];
			var row5 = columns[0].Rows[4];
			var row6 = columns[0].Rows[5];
			var row7 = columns[0].Rows[6];
			var row8 = columns[0].Rows[7];
			var row9 = columns[0].Rows[8];

			AssertEquals(3, row1.Parts.Count);
			AssertEquals(nameof(Customs.GUI.ShipmentDetailsControlBag.Instance.HouseBillParcelPostTextBox), row1.Parts[1].Name);

			AssertEquals(4, row2.Parts.Count);
			AssertEquals(nameof(ShipmentDetailsControlBag.Instance.OriginCodeFindBox), row2.Parts[1].Name);
			AssertEquals(nameof(ShipmentDetailsControlBag.Instance.EstimatedDepartureDateEdit), row2.Parts[3].Name);

			AssertEquals(4, row3.Parts.Count);
			AssertEquals(nameof(ShipmentDetailsControlBag.Instance.FinalDestinationCodeFindBox), row3.Parts[1].Name);
			AssertEquals(nameof(ShipmentDetailsControlBag.Instance.EstimatedArrivalDateEdit), row3.Parts[3].Name);

			AssertEquals(3, row4.Parts.Count);
			AssertEquals(nameof(Customs.GUI.ShipmentDetailsControlBag.Instance.GoodsDescriptionTextBox), row4.Parts[1].Name);

			AssertEquals(3, row5.Parts.Count);
			AssertEquals(nameof(Customs.GUI.ShipmentDetailsControlBag.Instance.OwnersReferenceTextBox), row5.Parts[1].Name);

			AssertEquals(4, row6.Parts.Count);
			AssertEquals(nameof(ShipmentDetailsControlBag.Instance.WeightCalcDropEdit), row6.Parts[1].Name);
			AssertEquals(nameof(ShipmentDetailsControlBag.Instance.VolumeCalcDropEdit), row6.Parts[3].Name);

			AssertEquals(4, row7.Parts.Count);
			AssertEquals(nameof(Customs.GUI.ShipmentDetailsControlBag.Instance.TotalNoOfPacksCalcDropEdit), row7.Parts[1].Name);
			AssertEquals(nameof(Customs.GUI.ShipmentDetailsControlBag.Instance.ContainerCountCalcEdit), row7.Parts[3].Name);

			AssertEquals(2, row8.Parts.Count);
			AssertEquals(nameof(Customs.GUI.ShipmentDetailsControlBag.Instance.ShipmentDetailsIncoTermsUserControl), row8.Parts[1].Name);

			AssertEquals(2, row9.Parts.Count);
			AssertEquals(nameof(ShipmentDetailsControlBag.Instance.ShipmentDetailsScreeningUserControl), row9.Parts[1].Name);
		}
	}
}
