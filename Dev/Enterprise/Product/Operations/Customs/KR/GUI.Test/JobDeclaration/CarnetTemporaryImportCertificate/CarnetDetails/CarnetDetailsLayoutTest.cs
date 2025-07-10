using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(CarnetDetailsLayout))]
	sealed class CarnetDetailsLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;
		protected override ICommonLayoutBuilder CommonLayoutBuilder => null;
		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn => null;
		protected override bool ShouldAssertThatIncludedControlsAreSorted => false;

		public new void TestIncludedControls()
		{
			IPanelLayoutProvider layout = new CarnetDetailsLayout();
			var columns = layout.Layout.Columns;
			AssertEquals(1, columns.Count);

			AssertEquals(7, columns[0].Rows.Count);
			var row1 = columns[0].Rows[0];
			var row2 = columns[0].Rows[1];
			var row3 = columns[0].Rows[2];
			var row4 = columns[0].Rows[3];
			var row5 = columns[0].Rows[4];
			var row6 = columns[0].Rows[5];
			var row7 = columns[0].Rows[6];

			AssertEquals(4, row1.Parts.Count);
			AssertEquals(nameof(CarnetDetailsControlBag.Instance.CarnetCertificateNoTextBox), row1.Parts[1].Name);
			AssertEquals(nameof(CarnetDetailsControlBag.Instance.EffectiveToDateDateEdit), row1.Parts[3].Name);

			AssertEquals(2, row2.Parts.Count);
			AssertEquals(nameof(CarnetDetailsControlBag.Instance.CarnetUseDropEdit), row2.Parts[1].Name);

			AssertEquals(2, row3.Parts.Count);
			AssertEquals(nameof(CarnetDetailsControlBag.Instance.RepresentativeProductNameLongTextControl), row3.Parts[1].Name);

			AssertEquals(2, row4.Parts.Count);
			AssertEquals(nameof(CarnetDetailsControlBag.Instance.CargoManagementNoTextBox), row4.Parts[1].Name);

			AssertEquals(4, row5.Parts.Count);
			AssertEquals(nameof(CarnetDetailsControlBag.Instance.HouseBillTextBox), row5.Parts[1].Name);
			AssertEquals(nameof(CarnetDetailsControlBag.Instance.HouseBillSplitDropEdit), row5.Parts[3].Name);

			AssertEquals(4, row6.Parts.Count);
			AssertEquals(nameof(CarnetDetailsControlBag.Instance.TotalWeightCalcDropEdit), row6.Parts[1].Name);
			AssertEquals(nameof(CarnetDetailsControlBag.Instance.TotalQtyCalcEdit), row6.Parts[3].Name);

			AssertEquals(4, row7.Parts.Count);
			AssertEquals(nameof(CarnetDetailsControlBag.Instance.NoPackagesCalcDropEdit), row7.Parts[1].Name);
			AssertEquals(nameof(CarnetDetailsControlBag.Instance.TotalAmountConvertToLocalCurrencyControl), row7.Parts[3].Name);
		}
	}
}
