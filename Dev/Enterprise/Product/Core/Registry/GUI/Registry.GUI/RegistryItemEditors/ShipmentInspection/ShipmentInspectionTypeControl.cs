using Enterprise.Core.Forms;
using Enterprise.Registry.Business;

namespace Enterprise.Registry.GUI
{
	public partial class ShipmentInspectionTypeControl : RegistryZUserControl
	{
		public ShipmentInspectionTypeControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			KnownShipperTypesGrid.ReadOnly = readOnly;
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			RemoveAllowedOnPassengerFlightsColumnIfNotRequired(dataSource);
			base.SetDataBinding(dataSource, dataMember);
		}

		void RemoveAllowedOnPassengerFlightsColumnIfNotRequired(object dataSource)
		{
			var shipmentInspectionTypes = dataSource as ShipmentInspectionTypes;
			if (shipmentInspectionTypes != null && !shipmentInspectionTypes.AllowedOnPassengerFlightsApplies)
			{
				for (int i = KnownShipperTypesGrid.ColumnStyles.Count - 1; i >= 0; --i)
				{
					var columnInfo = (ZGridColumnInfo)KnownShipperTypesGrid.ColumnStyles[i];
					if (columnInfo.ColumnName == allowedOnPassengerFlightsColumnName)
					{
						KnownShipperTypesGrid.ColumnStyles.RemoveAt(i);
						break;
					}
				}
			}
		}

		internal const string allowedOnPassengerFlightsColumnName = "AllowedOnPassengerFlights";
	}
}
