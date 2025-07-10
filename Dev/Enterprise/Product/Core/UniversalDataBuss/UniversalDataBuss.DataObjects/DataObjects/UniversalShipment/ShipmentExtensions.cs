using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	public static class ShipmentExtensions
	{
		public static Shipment GetExactDataObject(this List<Shipment> shipmentCollection, ZString dataSourceKey, string dataSourceType = null)
		{
			Shipment result = null;
			if (shipmentCollection != null)
			{
				foreach (var subShipment in shipmentCollection)
				{
					result = subShipment.GetExactDataObject(dataSourceKey, dataSourceType);
					if (result != null)
					{
						break;
					}
				}
			}

			return result;
		}

		public static Shipment GetExactDataObject(this Shipment shipment, ZString dataSourceKey, string dataSourceType = null)
		{
			Argument.NotNull(shipment, nameof(shipment));

			bool compareType = dataSourceType != null;

			Shipment result = null;
			if (shipment != null && shipment.DataContext != null && shipment.DataContext.DataSourceCollection != null)
			{
				var selectedDataSource = shipment.DataContext.DataSourceCollection.
						Select((dataSource, index) => new { dataSource, index }).
						Where(x => x.dataSource.Key.GetValueOrDefault() == dataSourceKey && !x.dataSource.Type.GetValueOrDefault().IsEmpty && (!compareType || x.dataSource.Type.GetValueOrDefault() == dataSourceType)).
						Select(x => new { x.dataSource, x.index }).FirstOrDefault();
				if (selectedDataSource != null)
				{
					result = selectedDataSource.index == 0 ? shipment : shipment.SubShipmentCollection.GetExactDataObjectFromSubShipmentCollection(dataSourceKey, dataSourceType);
				}
			}

			return result;
		}

		public static Shipment GetExactDataObjectFromSubShipmentCollection(this DataObjectList<Shipment> subShipmentCollection, ZString dataSourceKey, string dataSourceType = null)
		{
			Shipment result = null;
			if (subShipmentCollection != null)
			{
				foreach (var subShipment in subShipmentCollection)
				{
					result = subShipment.GetExactDataObject(dataSourceKey, dataSourceType);
					if (result != null)
					{
						break;
					}
				}
			}

			return result;
		}
	}
}
