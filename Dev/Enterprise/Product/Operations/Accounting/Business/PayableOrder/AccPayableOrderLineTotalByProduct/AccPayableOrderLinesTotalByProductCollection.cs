using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.PayableOrder
{
	public class AccPayableOrderLinesTotalByProductCollection : NonPersistentBusinessObjectCollection<AccPayableOrderLinesTotalByProduct>
	{
		public AccPayableOrderLinesTotalByProductCollection(AccPayableOrderHeader order, BusinessObjectFactory factory)
			: base(factory)
		{
			Argument.NotNull(order, "order");
			this.order = order;
			this.hookedOrderLines = new List<AccPayableOrderLine>();
			this.isRefreshRequired = true;
			HookOrderEvents();
			SetReadOnlyIncludingChildren(true);
		}
		readonly AccPayableOrderHeader order;
		readonly List<AccPayableOrderLine> hookedOrderLines;
		bool isRefreshRequired;

		void HookOrderEvents()
		{
			order.OrderLines.CountChanged += OrderLines_CountChanged;
			HookOrderLinesEvents();
		}

		void HookOrderLinesEvents()
		{
			foreach (var orderLine in order.OrderLines)
			{
				orderLine.APL_PartNoInfo.ValueChanged += OrderLine_ValueChanged;
				orderLine.APL_QuantityInfo.ValueChanged += OrderLine_ValueChanged;
				orderLine.APL_QtyInvoicedInfo.ValueChanged += OrderLine_ValueChanged;
				orderLine.APL_QtyReceivedInfo.ValueChanged += OrderLine_ValueChanged;
				orderLine.APL_LinePriceInfo.ValueChanged += OrderLine_ValueChanged;
				this.hookedOrderLines.Add(orderLine);
			}
		}

		void UnHookOrderLinesEvents()
		{
			foreach (var orderLine in this.hookedOrderLines)
			{
				orderLine.APL_PartNoInfo.ValueChanged -= OrderLine_ValueChanged;
				orderLine.APL_QuantityInfo.ValueChanged -= OrderLine_ValueChanged;
				orderLine.APL_QtyInvoicedInfo.ValueChanged -= OrderLine_ValueChanged;
				orderLine.APL_QtyReceivedInfo.ValueChanged -= OrderLine_ValueChanged;
				orderLine.APL_LinePriceInfo.ValueChanged -= OrderLine_ValueChanged;
			}

			this.hookedOrderLines.Clear();
		}

		void OrderLine_ValueChanged(object sender, System.EventArgs e)
		{
			this.isRefreshRequired = true;
		}

		void OrderLines_CountChanged(object sender, System.EventArgs e)
		{
			this.isRefreshRequired = true;
			UnHookOrderLinesEvents();
			if (!this.order.IsDeleting)
			{
				HookOrderLinesEvents();
			}
		}

		public void Populate()
		{
			if (this.isRefreshRequired)
			{
				RemoveAndDeleteAll();
				Dictionary<ZString, List<AccPayableOrderLine>> orderLines = new Dictionary<ZString, List<AccPayableOrderLine>>();
				foreach (AccPayableOrderLine orderLine in this.order.OrderLines)
				{
					List<AccPayableOrderLine> currentList = null;
					ZString key = orderLine.APL_PartNo;
					if (!orderLines.ContainsKey(key))
					{
						currentList = new List<AccPayableOrderLine>();
						orderLines.Add(key, currentList);
					}
					else
					{
						currentList = orderLines[key];
					}

					currentList.Add(orderLine);
				}

				foreach (var pair in orderLines)
				{
					var total = AddNew();
					total.Product = pair.Key;
					total.OrderLines = pair.Value;
				}

				this.isRefreshRequired = false;
			}
		}

		#region Implementation

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new AccPayableOrderLinesTotalByProduct(Factory);
		}

		#endregion
	}
}

