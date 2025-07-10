using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class BillDetailsPlugInUserControlTestCase : TestCaseWithFactory
	{
		public void TestShipmentStatusDescriptionZTextBoxColor()
		{
			var house = Factory.New<CusSCAHouse>();
			using (var form = new ZForm(house))
			{
				var control = new BillDetailsPlugInUserControl();
				form.Controls.Add(control);
				control.SetDataBinding(house, "");
				AssertEquals(System.Drawing.SystemColors.Control, control.ShipmentStatusDescriptionZTextBox.BackColor);
				house.CA_ShipmentStatus = SupplementaryCargoReportJobStatusList.Codes.Validated;
				control.ShipmentStatusDescriptionZTextBox.Text = house.ShipmentStatusDescription;
				AssertEquals(System.Drawing.SystemColors.Control, control.ShipmentStatusDescriptionZTextBox.BackColor);
				house.CA_ShipmentStatus = SupplementaryCargoReportJobStatusList.Codes.Clear;
				control.ShipmentStatusDescriptionZTextBox.Text = house.ShipmentStatusDescription;
				AssertEquals(System.Drawing.Color.LightGreen, control.ShipmentStatusDescriptionZTextBox.BackColor);
				house.CA_ShipmentStatus = SupplementaryCargoReportJobStatusList.Codes.DoNotLoad;
				control.ShipmentStatusDescriptionZTextBox.Text = house.ShipmentStatusDescription;
				AssertEquals(System.Drawing.Color.LightSalmon, control.ShipmentStatusDescriptionZTextBox.BackColor);
				house.CA_ShipmentStatus = SupplementaryCargoReportJobStatusList.Codes.RACleared;
				control.ShipmentStatusDescriptionZTextBox.Text = house.ShipmentStatusDescription;
				AssertEquals(System.Drawing.Color.LightGreen, control.ShipmentStatusDescriptionZTextBox.BackColor);
				house.CA_ShipmentStatus = SupplementaryCargoReportJobStatusList.Codes.DoNotUnload;
				control.ShipmentStatusDescriptionZTextBox.Text = house.ShipmentStatusDescription;
				AssertEquals(System.Drawing.Color.LightSalmon, control.ShipmentStatusDescriptionZTextBox.BackColor);
				house.CA_ShipmentStatus = SupplementaryCargoReportJobStatusList.Codes.Error;
				control.ShipmentStatusDescriptionZTextBox.Text = house.ShipmentStatusDescription;
				AssertEquals(System.Drawing.Color.Red, control.ShipmentStatusDescriptionZTextBox.BackColor);
				house.CA_ShipmentStatus = SupplementaryCargoReportJobStatusList.Codes.Hold;
				control.ShipmentStatusDescriptionZTextBox.Text = house.ShipmentStatusDescription;
				AssertEquals(System.Drawing.Color.LightSalmon, control.ShipmentStatusDescriptionZTextBox.BackColor);
				house.CA_ShipmentStatus = SupplementaryCargoReportJobStatusList.Codes.Cancelled;
				control.ShipmentStatusDescriptionZTextBox.Text = house.ShipmentStatusDescription;
				AssertEquals(System.Drawing.SystemColors.Control, control.ShipmentStatusDescriptionZTextBox.BackColor);
				house.CA_ShipmentStatus = SupplementaryCargoReportJobStatusList.Codes.Unknown;
				control.ShipmentStatusDescriptionZTextBox.Text = house.ShipmentStatusDescription;
				AssertEquals(System.Drawing.Color.LightSalmon, control.ShipmentStatusDescriptionZTextBox.BackColor);
				house.CA_ShipmentStatus = SupplementaryCargoReportJobStatusList.Codes.NotMatched;
				control.ShipmentStatusDescriptionZTextBox.Text = house.ShipmentStatusDescription;
				AssertEquals(System.Drawing.SystemColors.Control, control.ShipmentStatusDescriptionZTextBox.BackColor);
			}
		}
	}
}
