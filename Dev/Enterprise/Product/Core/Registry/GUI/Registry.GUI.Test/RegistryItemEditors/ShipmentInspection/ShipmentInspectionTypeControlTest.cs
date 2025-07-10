using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Core.Forms;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(ShipmentInspectionTypeControl))]
	sealed class ShipmentInspectionTypeControlTest : Testing.RegistryZUserControlTestCase
	{
		public void TestPassengerFlightsColumn()
		{
			using (var dummyForm = new Form())
			using (var control = new ShipmentInspectionTypeControl())
			{
				var source = new ShipmentInspectionTypes("HK");
				control.SetDataBinding(source, "");

				dummyForm.Controls.Add(control);
				dummyForm.Show();

				bool containsPassengerFlightsColumn = false;
				foreach (ZGridColumnInfo column in control.KnownShipperTypesGrid.ColumnStyles)
				{
					if (column.ColumnName == ShipmentInspectionTypeControl.allowedOnPassengerFlightsColumnName)
					{
						containsPassengerFlightsColumn = true;
					}
				}

				Assert("Should contain Passenger Flights column", containsPassengerFlightsColumn);
			}

			using (var dummyForm = new Form())
			using (var control = new ShipmentInspectionTypeControl())
			{
				var source = new ShipmentInspectionTypes("AU");
				control.SetDataBinding(source, "");

				dummyForm.Controls.Add(control);
				dummyForm.Show();

				foreach (ZGridColumnInfo column in control.KnownShipperTypesGrid.ColumnStyles)
				{
					Assert("Should not contain Passenger Flights column", column.ColumnName != ShipmentInspectionTypeControl.allowedOnPassengerFlightsColumnName);
				}
			}
		}

		protected override IBusiness GetNewBusinessEntity()
		{
			return new ShipmentInspectionTypes();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((ShipmentInspectionTypeControl)control).KnownShipperTypesGrid.ReadOnly;
		}
	}
}
