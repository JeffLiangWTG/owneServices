using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsOrderInventorySelectionHeader : NctsInventorySelectionHeader
	{
		public NctsOrderInventorySelectionHeader(NctsBill parentConsignment) : base(parentConsignment)
		{
		}

		public void ImportInventories(IWhsOrder order)
		{
			var orderPickLines = EU.Business.BondedWarehousingHelper.GetPickLinesForOrderLines(Factory, order);
			if (!AttemptedImportOrderWithMultiplePicksOnOrderLine(orderPickLines, order))
			{
				var wrappers = new List<WhsInventoryWrapper>();
				foreach (var entry in orderPickLines)
				{
					var orderLine = entry.Key;
					var pickLine = entry.Value.Single();

					var receiveLine = EU.Business.BondedWarehousingHelper.GetReceiveLineForPickLine(Factory, pickLine);
					if (receiveLine != null)
					{
						wrappers.Add(new WhsInventoryWrapper(receiveLine.Inventory, this, new WhsOrderLineWrapper(orderLine, Factory) ) { QuantityToDraw = pickLine.WZ_Units, Order = order, OrderLines = new[] { orderLine } });
					}
				}

				SelectedLines.AddRange(wrappers);
				ImportInventories();

				Bill.IsOutwardOrderImported = true;

				LinkHeaderToOrder(order);
			}
		}

		protected override void UpdateParentData()
		{
		}

		protected override NctsDepartureCargoDesc CreateGoodsItemProductLineFromWarehouseDataCore(WhsInventoryWrapper inventoryWrapper,
			IWhsDocketLine whsReceiveLine, IWhsBondedWarehouseAttribute whsBondedWarehouseAttribute, ZDecimal invoiceQuantity,
			ZDecimal ratio)
		{
			var goodsItem = base.CreateGoodsItemProductLineFromWarehouseDataCore(inventoryWrapper, whsReceiveLine, whsBondedWarehouseAttribute, invoiceQuantity, ratio);
			goodsItem.BY_BondedWhsUnitQty = whsReceiveLine.WE_F3_NKPackType;
			return goodsItem;
		}

		public virtual IWhsOrderCollection GetCollectionForWhsOrderSelection(BusinessObjectFactory factory) =>
			Enterprise.Customs.EU.Business.BondedWarehousingHelper.GetCollectionForWhsOrderSelection(Factory);

		bool AttemptedImportOrderWithMultiplePicksOnOrderLine(Dictionary<IWhsDocketLine, IEnumerable<IWhsPickLine>> orderLineToPickLines, IWhsOrder order)
		{
			var result = false;
			if (orderLineToPickLines.Any(x => x.Value.Count() != 1))
			{
				var orderWithMultiplePickLines = orderLineToPickLines.First(x => x.Value.Count() != 1).Key;
				ImportInventoriesResult += Res.GetString("ec867722-a474-48ba-9670-21e6b3bb6801", "You cannot import order {0} with multiple picks on order line {1}", order.WD_ExternalReference, orderWithMultiplePickLines.WE_LineNo);
				result = true;
			}

			return result;
		}

		void LinkHeaderToOrder(IWhsOrder order)
		{
			var pivotParent = Bill.Header;
			var pivotQuery = new ZQuery(WhsDocketJobPivotSchema.WV_WD_Docket, order.PK);
			pivotQuery.AddToFilter(WhsDocketJobPivotSchema.WV_DocketType, order.WD_DocketType);
			pivotQuery.AddToFilter(WhsDocketJobPivotSchema.WV_ParentId, pivotParent.PK);
			pivotQuery.AddToFilter(WhsDocketJobPivotSchema.WV_ParentTableCode, pivotParent.TableCode);

			var type = ObjectFactory.GetType<IWhsDocketJobPivot>();

			if (!Bill.Factory.Exists(type, pivotQuery))
			{
				var pivot = Bill.Factory.New<IWhsDocketJobPivot>();
				pivot.WV_WD_Docket = order.PK;
				pivot.WV_DocketType = order.WD_DocketType;
				pivot.WV_ParentId = pivotParent.PK;
				pivot.WV_ParentTableCode = pivotParent.TableCode;
			}
		}
	}
}
