using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Forwarding.Business;
using Enterprise.Customs.Forwarding.GUI;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Forwarding.Module
{
	class ForwardingShipmentModuleCustomsColumnsProvider : GridCustomColumnsProvider
	{
		public ForwardingShipmentModuleCustomsColumnsProvider()
		{
		}

		protected override ZGridCustomColumnsInitializer GetCustomColumnsInitializer(ZGrid grid, IBusinessObjectCollection collection, ICustomPropertyContainer propertyContainer)
		{
			return new ForwardingZGridCustomColumnsInitializer(grid, collection, GroupName, propertyContainer);
		}

		protected override CustomPropertyContainer GetCustomPropertyContainer()
		{
			var propertyContainer = new CustomPropertyContainer();

			propertyContainer.AddCustomProperty(GetCustomProperty(ForwardingShipmentCustomsColumnConstants.Schema.CustomsCargoStatus, ForwardingShipmentCustomsColumnConstants.Captions.CustomsCargoStatusDescription));
			propertyContainer.AddCustomProperty(GetCustomProperty(ForwardingShipmentCustomsColumnConstants.Schema.CustomsMessageStatus, ForwardingShipmentCustomsColumnConstants.Captions.CustomsMessageStatusDescription));

			propertyContainer.AddCustomProperty(GetCustomProperty(ForwardingShipmentCustomsColumnConstants.Schema.ISFBillNumber, ForwardingShipmentCustomsColumnConstants.Captions.ISFBillNumberDescription));
			propertyContainer.AddCustomProperty(GetCustomProperty(ForwardingShipmentCustomsColumnConstants.Schema.ISFBillStatus, ForwardingShipmentCustomsColumnConstants.Captions.ISFBillStatusDescription));
			propertyContainer.AddCustomProperty(GetCustomProperty(ForwardingShipmentCustomsColumnConstants.Schema.ISFBillStatusDescription, ForwardingShipmentCustomsColumnConstants.Captions.ISFBillStatusDescriptionDescription));

			propertyContainer.AddCustomProperty(GetCustomProperty(ForwardingShipmentCustomsColumnConstants.Schema.AFRBillStatus, ForwardingShipmentCustomsColumnConstants.Captions.AFRBillStatusDescription));
			propertyContainer.AddCustomProperty(GetCustomProperty(ForwardingShipmentCustomsColumnConstants.Schema.AFRBillStatusDescription, ForwardingShipmentCustomsColumnConstants.Captions.AFRBillStatusDescriptionDescription));

			propertyContainer.AddCustomProperty(GetCustomProperty(ForwardingShipmentCustomsColumnConstants.Schema.ACICargoStatus, ForwardingShipmentCustomsColumnConstants.Captions.ACICargoStatusDescription));
			propertyContainer.AddCustomProperty(GetCustomProperty(ForwardingShipmentCustomsColumnConstants.Schema.ACIMessageStatus, ForwardingShipmentCustomsColumnConstants.Captions.ACIMessageStatusDescription));

			propertyContainer.AddCustomProperty(GetCustomProperty(ForwardingShipmentCustomsColumnConstants.Schema.EManifestCargoStatus, ForwardingShipmentCustomsColumnConstants.Captions.EManifestCargoStatusDescription));
			propertyContainer.AddCustomProperty(GetCustomProperty(ForwardingShipmentCustomsColumnConstants.Schema.EManifestMessageStatus, ForwardingShipmentCustomsColumnConstants.Captions.EManifestMessageStatusDescription));

			if (Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode) == Core.Constants.CountryCodes.UnitedStates)
			{
				propertyContainer.AddCustomProperty(GetCustomProperty(ForwardingShipmentCustomsColumnConstants.Schema.CRLStatus, ForwardingShipmentCustomsColumnConstants.Captions.CRLStatusDescription));
				propertyContainer.AddCustomProperty(GetCustomProperty(ForwardingShipmentCustomsColumnConstants.Schema.SEBillStatus, ForwardingShipmentCustomsColumnConstants.Captions.SEBillStatusDescription));
				propertyContainer.AddCustomProperty(GetCustomProperty(ForwardingShipmentCustomsColumnConstants.Schema.HLDOrEXMStatus, ForwardingShipmentCustomsColumnConstants.Captions.HLDOrEXMStatusDescription));
				propertyContainer.AddCustomProperty(GetCustomProperty(ForwardingShipmentCustomsColumnConstants.Schema.ENSStatus, ForwardingShipmentCustomsColumnConstants.Captions.ENSStatusDescription));
				propertyContainer.AddCustomProperty(GetCustomProperty(ForwardingShipmentCustomsColumnConstants.Schema.EXPStatus, ForwardingShipmentCustomsColumnConstants.Captions.EXPStatusDescription));

				propertyContainer.AddCustomProperty(ForwardingShipmentCustomsColumnConstants.Schema.CustomsEntryType, ForwardingShipmentCustomsColumnConstants.Captions.CustomsEntryTypeDescription, typeof(ZString), GetCustomsEntryType);
				propertyContainer.AddCustomProperty(ForwardingShipmentCustomsColumnConstants.Schema.ITEntryType, ForwardingShipmentCustomsColumnConstants.Captions.ITEntryTypeDescription, typeof(ZString), GetITEntryType);
			}
			else
			{
				propertyContainer.AddCustomProperty(GetCustomProperty(ForwardingShipmentCustomsColumnConstants.Schema.EntryStatusDescription, ForwardingShipmentCustomsColumnConstants.Captions.EntryStatusDescriptionDescription));
			}

			propertyContainer.AddCustomProperty(ForwardingShipmentCustomsColumnConstants.Schema.CustomsEntryAuthorisationDate, ForwardingShipmentCustomsColumnConstants.Captions.CustomsEntryAuthorisationDateDescription, typeof(ZDateTime), GetCustomsEntryAuthorisationDate);

			propertyContainer.AddCustomProperty(GetCustomProperty(ForwardingShipmentCustomsColumnConstants.Schema.DestinationGoodsValue, ForwardingShipmentCustomsColumnConstants.Captions.DestinationGoodsValue));
			propertyContainer.AddCustomProperty(GetCustomProperty(ForwardingShipmentCustomsColumnConstants.Schema.DestinationCurrencyCode, ForwardingShipmentCustomsColumnConstants.Captions.DestinationCurrencyCode));
			propertyContainer.AddCustomProperty(GetCustomProperty(ForwardingShipmentCustomsColumnConstants.Schema.DestinationExchangeRate, ForwardingShipmentCustomsColumnConstants.Captions.DestinationExchangeRate));

			return propertyContainer;
		}

		protected override void AddFetchHintForView(BusinessObject[] businessObjects, TableColumn[] columns)
		{
			foreach (TableColumn column in columns)
			{
				foreach (var businessObject in businessObjects)
				{
					AddFetchHintForViewWithColumn(businessObject, column);
				}
			}
		}

		static void AddFetchHintForViewWithColumn(BusinessObject businessObject, TableColumn column)
		{
			var requiresConsol = false;
			var requiresCusSCAHouse = false;
			var requiresCusHawbs = false;
			var requiresCAeMHHouse = false;

			switch (column.ColumnName)
			{
				case ForwardingShipmentCustomsInformation.Schema.CustomsCargoStatus:
				case ForwardingShipmentCustomsInformation.Schema.CustomsMessageStatus:
					requiresCusSCAHouse = true;
					requiresCusHawbs = true;
					break;
				case ForwardingShipmentCustomsInformation.Schema.EManifestCargoStatus:
				case ForwardingShipmentCustomsInformation.Schema.EManifestMessageStatus:
					requiresCAeMHHouse = true;
					break;
				case ForwardingShipmentCustomsInformation.Schema.ACICargoStatus:
				case ForwardingShipmentCustomsInformation.Schema.ACIMessageStatus:
					requiresCusSCAHouse = true;
					break;
				case ForwardingShipmentCustomsInformation.Schema.AFRBillStatus:
				case ForwardingShipmentCustomsInformation.Schema.AFRBillStatusDescription:
					requiresConsol = true;
					break;
				default:
					break;
			}

			if (requiresCAeMHHouse)
			{
				businessObject.Factory.AddFetchHint(CusCAeMHHouseSchema.BW_ParentID, businessObject.PK);
			}

			if (requiresCusHawbs)
			{
				businessObject.Factory.AddFetchHint(CusHAWBSchema.CS_JS, businessObject.PK);
			}

			if (requiresCusSCAHouse)
			{
				businessObject.Factory.AddFetchHint(CusSCAHouseSchema.CA_JS, businessObject.PK);
			}

			if (requiresConsol)
			{
				ZDBOnlySubQuery consolShipmentPivotQuery = new ZDBOnlySubQuery(typeof(JobConShipLink), JobConShipLinkSchema.JN_JK);
				consolShipmentPivotQuery.AddToFilter(JobConShipLinkSchema.JN_JS, businessObject.PK);

				ZDBOnlyQuery consolsQuery = new ZDBOnlyQuery(typeof(ForwardingConsol));
				consolsQuery.AddSubQuery(consolShipmentPivotQuery, JoinCondition.And);

				businessObject.Factory.AddFetchHint(typeof(ForwardingConsol), consolsQuery);
			}
		}

		object GetCustomsEntryType(BusinessObject parent)
		{
			var result = ZString.Empty;
			var declaration = GetDeclaration(parent);

			if (declaration != null)
			{
				result = (ZString)declaration[JobDeclarationSchema.JE_AddInfo];
				var entryTypeLocation = result.IndexOf("EntryType=", StringComparison.Ordinal);
				result = result.SubstringSafe(entryTypeLocation + 10, 2);
			}

			return result;
		}

		object GetITEntryType(BusinessObject parent)
		{
			var result = ZString.Empty;
			var declaration = GetDeclaration(parent);

			if (declaration != null)
			{
				result = (ZString)declaration[JobDeclarationSchema.JE_AddInfo];
				var entryTypeLocation = result.IndexOf("InbondType=", StringComparison.Ordinal);
				result = result.SubstringSafe(entryTypeLocation + 10, 2);
			}

			return result;
		}

		object GetCustomsEntryAuthorisationDate(BusinessObject parent)
		{
			var result = ZDateTime.Empty;
			var declaration = GetDeclaration(parent);

			if (declaration != null)
			{
				result = (ZDateTime)declaration[JobDeclarationSchema.JE_EntryAuthorisationDate];
			}

			return result;
		}

		BusinessObject GetDeclaration(BusinessObject businessObject)
		{
			var shipment = businessObject as ForwardingShipment;

			if (shipment == null)
			{
				return null;
			}

			var shipmentToDeclarationMap =
				businessObject.Factory.GetCachedValue("ForwardingShipmentModuleCustomsColumnsProvider.ShipmentToDeclarationMap", () => new Dictionary<ZGuid, BusinessObject>());

			if (shipmentToDeclarationMap.TryGetValue(shipment.PK, out var declaration))
			{
				return declaration;
			}

			var jobDecFilter = Common.JobDeclarationFilter.ForCompanyAndShipment(true, GlbCompany.CurrentCompany, businessObject.PK);
			jobDecFilter.OrderBy = JobDeclarationSchema.Constants.JE_SystemCreateTimeUtc;

			declaration = (BusinessObject)shipment.Factory.LoadTop1<Integration.Customs.IBaseJobDeclaration>(jobDecFilter);

			shipmentToDeclarationMap.Add(shipment.PK, declaration);

			return declaration;
		}

		CustomPropertyImplementation<BusinessObject> GetCustomProperty(string propertyName, MultilingualString caption)
		{
			return new CustomPropertyImplementation<BusinessObject>(propertyName, caption, typeof(ZString), (bo) => GetCustomsPropertyValue(bo, propertyName));
		}

		object GetCustomsPropertyValue(BusinessObject businessObject, string propertyName)
		{
			var shipment = businessObject as ForwardingShipment;
			if (shipment == null)
			{
				return null;
			}

			ForwardingShipmentCustomsInformation customsInformation = new ForwardingShipmentCustomsInformation(shipment);
			return customsInformation[propertyName];
		}
	}
}
