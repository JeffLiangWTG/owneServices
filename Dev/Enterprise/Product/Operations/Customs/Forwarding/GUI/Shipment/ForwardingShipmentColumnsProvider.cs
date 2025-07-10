using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Forwarding.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Forwarding.GUI
{
	public class ForwardingShipmentColumnsProvider : GridCustomColumnsProvider
	{
		protected override ZGridCustomColumnsInitializer GetCustomColumnsInitializer(ZGrid grid, IBusinessObjectCollection collection, ICustomPropertyContainer propertyContainer)
		{
			return new ForwardingZGridCustomColumnsInitializer(grid, collection, GroupName, propertyContainer);
		}

		protected override CustomPropertyContainer GetCustomPropertyContainer()
		{
			var propertyContainer = new CustomPropertyContainer();

			propertyContainer.AddCustomProperty(GetCustomProperty(ForwardingShipmentCustomsColumnConstants.Schema.CustomsCargoStatus, ForwardingShipmentCustomsColumnConstants.Captions.CustomsCargoStatusDescription));
			propertyContainer.AddCustomProperty(GetCustomProperty(ForwardingShipmentCustomsColumnConstants.Schema.CustomsMessageStatus, ForwardingShipmentCustomsColumnConstants.Captions.CustomsMessageStatusDescription));

			propertyContainer.AddCustomProperty(GetCustomProperty(ForwardingShipmentCustomsColumnConstants.Schema.AFRBillStatus, ForwardingShipmentCustomsColumnConstants.Captions.AFRBillStatusDescription));
			propertyContainer.AddCustomProperty(GetCustomProperty(ForwardingShipmentCustomsColumnConstants.Schema.AFRBillStatusDescription, ForwardingShipmentCustomsColumnConstants.Captions.AFRBillStatusDescriptionDescription));

			propertyContainer.AddCustomProperty(GetCustomProperty(ForwardingShipmentCustomsColumnConstants.Schema.ACICargoStatus, ForwardingShipmentCustomsColumnConstants.Captions.ACICargoStatusDescription));
			propertyContainer.AddCustomProperty(GetCustomProperty(ForwardingShipmentCustomsColumnConstants.Schema.ACIMessageStatus, ForwardingShipmentCustomsColumnConstants.Captions.ACIMessageStatusDescription));

			propertyContainer.AddCustomProperty(GetCustomProperty(ForwardingShipmentCustomsColumnConstants.Schema.DestinationGoodsValue, ForwardingShipmentCustomsColumnConstants.Captions.DestinationGoodsValue));
			propertyContainer.AddCustomProperty(GetCustomProperty(ForwardingShipmentCustomsColumnConstants.Schema.DestinationCurrencyCode, ForwardingShipmentCustomsColumnConstants.Captions.DestinationCurrencyCode));
			propertyContainer.AddCustomProperty(GetCustomProperty(ForwardingShipmentCustomsColumnConstants.Schema.DestinationExchangeRate, ForwardingShipmentCustomsColumnConstants.Captions.DestinationExchangeRate));

			return propertyContainer;
		}

		protected override void AddFetchHintForView(BusinessObject[] businessObjects, TableColumn[] columns)
		{
			foreach (var column in columns)
			{
				foreach (var businessObject in businessObjects)
				{
					var requiresConsol = false;
					var requiresCusSCAHouse = false;
					var requiresCusHawbs = false;

					switch (column.ColumnName)
					{
						case ForwardingShipmentCustomsInformation.Schema.CustomsCargoStatus:
						case ForwardingShipmentCustomsInformation.Schema.CustomsMessageStatus:
							requiresCusSCAHouse = true;
							requiresCusHawbs = true;
							break;
						case ForwardingShipmentCustomsInformation.Schema.ACICargoStatus:
						case ForwardingShipmentCustomsInformation.Schema.ACIMessageStatus:
							requiresCusSCAHouse = true;
							break;
						case ForwardingShipmentCustomsInformation.Schema.AFRBillStatus:
						case ForwardingShipmentCustomsInformation.Schema.AFRBillStatusDescription:
							requiresConsol = true;
							break;
					}

					if (requiresCusHawbs)
					{
						businessObject.Factory.AddFetchHint(CusHAWBSchema.CS_JS, businessObject.PK);
					}

					if (requiresCusSCAHouse)
					{
						businessObject.Factory.AddFetchHint(CusCAeMHHouseSchema.BW_ParentID, businessObject.PK);
						businessObject.Factory.AddFetchHint(CusSCAHouseSchema.CA_JS, businessObject.PK);
					}

					if (requiresConsol)
					{
						var consolShipmentPivotQuery = new ZDBOnlySubQuery(typeof(JobConShipLink), JobConShipLinkSchema.JN_JK);
						consolShipmentPivotQuery.AddToFilter(JobConShipLinkSchema.JN_JS, businessObject.PK);

						var consolsQuery = new ZDBOnlyQuery(typeof(ForwardingConsol));
						consolsQuery.AddSubQuery(consolShipmentPivotQuery, JoinCondition.And);

						businessObject.Factory.AddFetchHint(typeof(ForwardingConsol), consolsQuery);
					}
				}
			}
		}

		CustomPropertyImplementation<BusinessObject> GetCustomProperty(string propertyName, MultilingualString caption)
		{
			return new CustomPropertyImplementation<BusinessObject>(propertyName, caption, typeof(ZString), bo => GetCustomsProperyValue(bo, propertyName));
		}

		object GetCustomsProperyValue(BusinessObject businessObject, string propertyName)
		{
			var shipment = businessObject as ForwardingShipment;

			if (shipment == null)
			{
				return null;
			}

			ForwardingShipmentCustomsInformation customsInformation;

			if (customsInformationsCache.ContainsKey(shipment.PK))
			{
				customsInformation = customsInformationsCache[shipment.PK];
			}
			else
			{
				customsInformation = new ForwardingShipmentCustomsInformation(shipment);
				customsInformationsCache.Add(shipment.PK, customsInformation);
			}

			return customsInformation[propertyName];
		}

		readonly Dictionary<ZGuid, ForwardingShipmentCustomsInformation> customsInformationsCache = new Dictionary<ZGuid, ForwardingShipmentCustomsInformation>();
	}
}
