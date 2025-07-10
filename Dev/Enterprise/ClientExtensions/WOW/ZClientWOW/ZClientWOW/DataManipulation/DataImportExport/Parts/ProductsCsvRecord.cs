using System;
using System.Collections;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.Wow
{
	public class ProductCsvRecord : CsvRecord
	{
		public ProductCsvRecord(string line)
			: base(line, 12)
		{
			ProductNo = FieldValues[1];
			WarehouseLocation = FieldValues[2];

			try
			{
				StockTakeCount = int.Parse(FieldValues[3]);
			}
			catch
			{
				throw new InvalidCastException(String.Format((NoResString)"Failed during converting {0} to Integer.", FieldValues[3]));
			}
		}

		public readonly string ProductNo;
		public readonly string WarehouseLocation;
		public readonly int StockTakeCount;

		public override string DisplayIdentifier
		{
			get { return "Part '" + ProductNo + "' at location '" + WarehouseLocation + "'"; }
		}

		protected override void OnUpdateBusinessData(BusinessObjectFactoryProvider factoryProvider, INotifications notify)
		{
			WoolworthsProduct part = UpdateOrAddPart(factoryProvider.Current, notify);
			if (part != null)
			{
				UpdateOrAddPartLocation(factoryProvider.Current, part, notify);
				part.OP_QtyInStock = part.OP_Calc_TotalLocationsStockTakeCount;
				part.UpdateWeightedCostFromLocations();
			}
		}

		#region Implementation

		#region Column Mappings

		internal static readonly string[] PartPropertyMappings = new string[]
		{
			"",																// Record type
			WoolworthsProduct.Schema.OP_PartNum,					// Item Reference Number (NK)
			"",
			WoolworthsProduct.Schema.OP_OrderMultipleQty,		// Item Pack Size
			WoolworthsProduct.Schema.OP_VendorPackQty,			// Vendor Pack Size
			"",
			WoolworthsProduct.Schema.OP_Department,				// Fine Department No
			WoolworthsProduct.Schema.OP_Desc,						// Vendor Description of the item
			WoolworthsProduct.Schema.OP_Division,					// Prefix
			"",
			"",																// Purchase cost by location
			"",																// Fine Dept Name (used for lookup)
			"",
			"",
			WoolworthsProduct.Schema.OP_Cubic // cubic
		};

		internal static readonly string[] PartLocationPropertyMappings = new string[]
		{
			"",																// Record type
			"",
			OrgPartLocation.Schema.OR_Warehouse,					// Warehouse Code
			"",
			"",
			OrgPartLocation.Schema.OR_BinLocation,					// Location No
			"",
			"",
			"",
			OrgPartLocation.Schema.OR_StockTakeCount,				// Sum of Stock on Hand for each location,
			OrgPartLocation.Schema.OR_InStock,									//in stock on hand for each location
			OrgPartLocation.Schema.OR_WeightCostThisLocation,	// Purchase cost by location										
			"",
			OrgPartLocation.Schema.OR_Ti,								// Ti
			OrgPartLocation.Schema.OR_Hi								// Hi
		};

		#endregion

		protected WoolworthsProduct UpdateOrAddPart(BusinessObjectFactory factory, INotifications notify)
		{
			ZQuery filter = new ZQuery(OrgSupplierPartSchema.OP_PartNum, SQLComparisonOperator.Equal, ProductNo);
			NotificationBuffer bufferForDetectingAmbiguousMatch = new NotificationBuffer(notify);
			WoolworthsProduct partToUpdate = WowStringToBusinessObjectFieldConverter.Instance.LoadPartForBuyerTakeOn(
				factory, ProductNo, ProductNo, bufferForDetectingAmbiguousMatch, true);
			if (partToUpdate == null &&
				!bufferForDetectingAmbiguousMatch.ContainsNotificationType(WowErrorType.MoreThan1PartNumber))
			{
				partToUpdate = (WoolworthsProduct)factory.New(typeof(WoolworthsProduct));
			}

			if (partToUpdate != null)
			{
				partToUpdate.SuspendValidation();

				((ISupportDataImporting)partToUpdate).IsImportingData = true;
				try
				{
					ZGuid declarationImporter = WowDataRegistry.Instance.DeclarationImporter;
					if (!declarationImporter.IsEmpty)
					{
						partToUpdate.RelatedOrganisations.AddOrganisationIfNotExist(declarationImporter, OrgPartRelation.RelationshipTypes.Owner);
					}
					partToUpdate.OP_StockKeepingUnit = Enterprise.Core.Constants.PkgUnit.Unit;
					partToUpdate.OP_Wow_ImportedFromMI = true;

					WowStringToBusinessObjectFieldConverter.Instance.SetPropertyInfoValue(
						partToUpdate.OP_PartNumInfo, FieldValues[1], ForeignKeyType.None, notify);
					WowStringToBusinessObjectFieldConverter.Instance.SetPropertyInfoValue(
						partToUpdate.OP_OrderMultipleQtyInfo, FieldValues[3], ForeignKeyType.None, notify);
					WowStringToBusinessObjectFieldConverter.Instance.SetPropertyInfoValue(
						partToUpdate.OP_VendorPackQtyInfo, FieldValues[4], ForeignKeyType.None, notify);
					WowStringToBusinessObjectFieldConverter.Instance.SetPropertyInfoValue(
						partToUpdate.OP_DepartmentInfo, FieldValues[6], ForeignKeyType.None, notify);
					WowStringToBusinessObjectFieldConverter.Instance.SetPropertyInfoValue(
						partToUpdate.OP_DescInfo, FieldValues[7], ForeignKeyType.None, notify);
					WowStringToBusinessObjectFieldConverter.Instance.SetPropertyInfoValue(
						partToUpdate.OP_DivisionInfo, FieldValues[8], ForeignKeyType.None, notify);

					if (FieldValues.Length > 14)
					{
						WowStringToBusinessObjectFieldConverter.Instance.SetPropertyInfoValue(
							partToUpdate.OP_CubicInfo, FieldValues[14], ForeignKeyType.None, notify);
					}
				}
				finally
				{
					((ISupportDataImporting)partToUpdate).IsImportingData = false;
				}
			}

			return partToUpdate;
		}

		protected void AssignBuyersToPartFromAllOrder(BusinessObjectFactory factory, WoolworthsProduct part)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(Order));
			ZDBOnlySubQuery orderLineQuery = new ZDBOnlySubQuery(typeof(OrderLine), JobOrderLineSchema.JO_JD);
			orderLineQuery.AddToFilter(JobOrderLineSchema.JO_Partno, part.OP_PartNum);
			query.AddSubQuery(orderLineQuery, JoinCondition.And);

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			newFactory.SuspendValidation();
			WoolworthsOrder[] orders = (WoolworthsOrder[])newFactory.Load(typeof(WoolworthsOrder), query);
			foreach (WoolworthsOrder order in orders)
			{
				foreach (OrderLine line in order.OrderLines)
				{
					if (line.JO_Partno == part.OP_PartNum)
					{
						part.RelatedOrganisations.AddOrganisationIfNotExist(order.BuyerPK, OrgPartRelation.RelationshipTypes.Owner, true);
					}
				}
			}
		}

		protected ZGuid[] GetBuyerPKsFromOrdersNotIncludingUnmatchOrg(WoolworthsOrder[] orders)
		{
			Hashtable result = new Hashtable();
			foreach (WoolworthsOrder order in orders)
			{
				if (order.BuyerPK != WowDataRegistry.Instance.UnmatchedDataItemsAccount)
				{
					result[order.BuyerPK] = null;
				}
			}
			return (ZGuid[])new ArrayList(result.Keys).ToArray(typeof(ZGuid));
		}

		protected void UpdateOrAddPartLocation(BusinessObjectFactory factory, WoolworthsProduct part, INotifications notify)
		{
			OrgPartLocation locationToUpdate = null;
			foreach (OrgPartLocation location in part.Locations)
			{
				if (location.OR_Warehouse == this.WarehouseLocation)
				{
					locationToUpdate = location;
				}
			}
			if (locationToUpdate == null)
			{
				locationToUpdate = part.Locations.AddNew();
			}

			((ISupportDataImporting)locationToUpdate).IsImportingData = true;
			try
			{
				WowStringToBusinessObjectFieldConverter.Instance.SetPropertyInfoValue(
					locationToUpdate.OR_WarehouseInfo, FieldValues[2], ForeignKeyType.None, notify);
				WowStringToBusinessObjectFieldConverter.Instance.SetPropertyInfoValue(
					locationToUpdate.OR_BinLocationInfo, FieldValues[5], ForeignKeyType.None, notify);
				WowStringToBusinessObjectFieldConverter.Instance.SetPropertyInfoValue(
					locationToUpdate.OR_StockTakeCountInfo, FieldValues[9], ForeignKeyType.None, notify);
				WowStringToBusinessObjectFieldConverter.Instance.SetPropertyInfoValue(
				locationToUpdate.OR_InStockInfo, FieldValues[9], ForeignKeyType.None, notify);
				WowStringToBusinessObjectFieldConverter.Instance.SetPropertyInfoValue(
					locationToUpdate.OR_WeightCostThisLocationInfo, FieldValues[10], ForeignKeyType.None, notify);
				if (FieldValues.Length > 12)
				{
					WowStringToBusinessObjectFieldConverter.Instance.SetPropertyInfoValue(
						locationToUpdate.OR_TiInfo, FieldValues[12], ForeignKeyType.None, notify);
				}
				if (FieldValues.Length > 13)
				{
					WowStringToBusinessObjectFieldConverter.Instance.SetPropertyInfoValue(
						locationToUpdate.OR_HiInfo, FieldValues[13], ForeignKeyType.None, notify);
				}
			}
			finally
			{
				((ISupportDataImporting)locationToUpdate).IsImportingData = false;
			}
		}

		#endregion
	}
}
