using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class WhsLoadWrapper : WarehouseJobGenericWrapper
	{
		public WhsLoadWrapper(BusinessObject whsLoadBO, BusinessObjectFactory factory)
			: base(whsLoadBO, factory)
		{
		}

		#region Properties

		protected override ZString DockDoorCore => WhsLoad?.PlannedDockDoor?.WLV_LocationString ?? ZString.Empty;

		protected override WarehouseBOWrapper WarehouseCore
		{
			get
			{
				var warehouse = WhsLoad?.PlannedDockDoor?.Warehouse;
				return warehouse != null
					? Factory.GetCachedValue(warehouse.PK.ToString(), () => new WarehouseBOWrapper(Res.GetString("516c6191-b10b-41f9-a72a-bf6c80b9a1a9", "Warehouse"), warehouse, Factory))
					: null;
			}
		}

		protected override ZString SealCore => WhsLoad?.WLO_SealNumber ?? ZString.Empty;

		protected override ZString DispatchDriverNameCore => WhsLoad?.WLO_DriversName ?? ZString.Empty;

		protected override EquipmentWrapper TransportationUnitCore
		{
			get
			{
				if (transportationUnitWrapper == null)
				{
					var transportationUnit = WhsLoad?.TransportationUnit;
					transportationUnitWrapper = transportationUnit != null
						? new EquipmentWrapper(transportationUnit, Factory)
						: null;
				}

				return transportationUnitWrapper;
			}
		}
		EquipmentWrapper transportationUnitWrapper;

		public override WarehousePickableDocketWrapperCollection Orders
		{
			get
			{
				var orders = new WarehouseOrderWrapperForManifestCollection(OrdersCollection, Factory);
				var ordersCollection = OrdersCollection.Cast<WhsOrder>();
				orders.ForEach(order =>
				{
					var od = (WarehouseOrderWrapperForManifest)order;
					var loadingSupport = (ILoadingSupport)order;
					loadingSupport.LoadPK = WhsLoad.PK;
					loadingSupport.CommonLoadWeightUQ = GetMostCommonUnit(ordersCollection, (whsOrder) => whsOrder.WD_TotalWeightUnit);
					loadingSupport.CommonLoadVolumeUQ = GetMostCommonUnit(ordersCollection, (whsOrder) => whsOrder.WD_TotalCubicUnit);
				});
				return orders;
			}
		}

		WhsLegacyPickableDocketCollection OrdersCollection
		{
			get
			{
				if (orders == null)
				{
					var zQuery = GetOrdersOnLoadQuery();
					var lorders = new WhsLegacyPickableDocketCollection(Factory, zQuery);
					lorders.Load();
					orders = lorders;
				}

				return orders;
			}
		}
		WhsLegacyPickableDocketCollection orders;

		ZDBOnlyQuery GetOrdersOnLoadQuery()
		{
			var pivotQuery = new ZDBOnlySubQuery(typeof(WhsLoadPkgPackagePivot), WhsLoadPkgPackagePivotSchema.WLP_KP_Package);
			pivotQuery.AddToFilter(WhsLoadPkgPackagePivotSchema.WLP_LoadedTime, SQLComparisonOperator.NotEqual, null);
			pivotQuery.AddToFilter(WhsLoadPkgPackagePivotSchema.WLP_UnloadedTime, SQLComparisonOperator.Equal, null);
			pivotQuery.AddToFilter(WhsLoadPkgPackagePivotSchema.WLP_WLO_Load, WhsLoad.PK);

			var packageQuery = new ZDBOnlySubQuery(typeof(PkgPackage), PkgPackageSchema.KP_KJ_ParentPackageJob);
			packageQuery.AddSubQuery(pivotQuery, JoinCondition.And);

			var packageJobQuery = new ZDBOnlySubQuery(typeof(PkgPackageJob), PkgPackageJobSchema.KJ_ParentID);
			packageJobQuery.AddToFilter(PkgPackageJobSchema.KJ_ParentTableCode, WhsDocketSchema.Constants.Prefix);
			packageJobQuery.AddSubQuery(packageQuery, JoinCondition.And);

			var query = new ZDBOnlyQuery(typeof(WhsDocket));
			query.AddSubQuery(packageJobQuery, JoinCondition.And);

			return query;
		}

		protected override ZString DeliveryRouteCore
		{
			get
			{
				var deliveryRoute = ZString.Empty;
				var consigneeAddressRoutes = OrdersCollection
					.Select(o => o.ConsigneeAddress)
					.Select(a => a?.OA_DeliveryRoute ?? string.Empty)
					.Distinct()
					.Take(2)
					.ToArray();

				if (consigneeAddressRoutes.Length > 0)
				{
					deliveryRoute = consigneeAddressRoutes.Length > 1
						? Res.GetString("583b48c2-4440-4cc4-a9b3-76106e300c9c", "Many")
						: GetDeliveryRoute(consigneeAddressRoutes[0]);
				}

				return deliveryRoute;
			}
		}

		static string GetDeliveryRoute(string deliveryRouteCode)
		{
			var deliveryRouteDescription = DataRegistry.Instance.DeliveryRoutesList.GetDescriptionFromCode(deliveryRouteCode);
			return deliveryRouteDescription.IsNullOrEmpty()
				? deliveryRouteCode
				: deliveryRouteDescription;
		}

		static ZString GetMostCommonUnit(IEnumerable<WhsOrder> orders, Func<WhsOrder, ZString> unitSelector)
		{
			return orders
				.GroupBy(unitSelector)
				.OrderByDescending(orderGrouping => orderGrouping.Count())
				.Select(orderGrouping => orderGrouping.Key)
				.First();
		}

		#endregion

		#region Implementation

		WhsLoad WhsLoad => whsLoad ?? (whsLoad = (WhsLoad)WrappedBO);
		WhsLoad whsLoad;

		#endregion
	}
}
