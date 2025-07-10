using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(VehicleLayout))]
	public class VehicleLayoutTest : LayoutsAbstractTest
	{
		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn => null;

		protected override int ControlBagCount => 1;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => null;

		public new void TestIncludedControls()
		{
			IPanelLayoutProvider layout = new VehicleLayout();
			var columns = layout.Layout.Columns;
			AssertEquals(2, columns.Count);

			AssertEquals(4, columns[0].Rows.Count);
			AssertEquals(4, columns[1].Rows.Count);
			AssertLayout(columns[0].Rows[0], nameof(VehicleControlBag.NameTextBox));
			AssertLayout(columns[0].Rows[1], nameof(VehicleControlBag.ExhaustVolumeCalcEdit));
			AssertLayout(columns[0].Rows[2], nameof(VehicleControlBag.ManufacturingCountryCodeFindBox));
			AssertLayout(columns[0].Rows[3], nameof(VehicleControlBag.FirstRegistrationDateEdit));
			AssertLayout(columns[1].Rows[0], nameof(VehicleControlBag.VehicleIDNumberTextBox));
			AssertLayout(columns[1].Rows[1], nameof(VehicleControlBag.ModelYearTextBox));
			AssertLayout(columns[1].Rows[2], nameof(VehicleControlBag.SeatCapacityCalcEdit));
			AssertLayout(columns[1].Rows[3], nameof(VehicleControlBag.CurrentRegistrationDateEdit));
		}

		protected override bool ShouldAssertThatIncludedControlsAreSorted => false;

		void AssertLayout(PanelLayoutRow row, string column)
		{
			AssertEquals(column, row.Parts[1].Name);
		}
	}
}
