using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.PayableOrder
{
	public class QtyReceivedEqualsQtyOrderedValidation : ValidationProvider
	{
		public QtyReceivedEqualsQtyOrderedValidation(AccPayableOrderLine orderLine)
			: base(orderLine)
		{
			this.OrderLine = orderLine;
		}

		public void CheckQtyReceivedEqualsQtyOrdered()
		{
			if (!OrderLine.APL_Quantity.IsEmpty && !OrderLine.APL_QtyReceived.IsEmpty)
			{
				decimal qtyOrdered = OrderLine.APL_Quantity;
				decimal qtyReceived = OrderLine.APL_QtyReceived;

				if (qtyReceived > 0) // no need to validate if order has not yet been fullfilled
				{
					if (qtyReceived > qtyOrdered)
					{
						string message = Res.GetString("dcf4a8c3-d0c3-406f-8a6d-3c7191ee9c3a", "The {0} is greater than the {1}.", OrderLine.APL_QtyReceivedInfo.Description, OrderLine.APL_QuantityInfo.Description);
						OrderLine.APL_QtyReceivedInfo.AddWarning(message);
					}
					else if (qtyReceived < qtyOrdered)
					{
						string message = Res.GetString("fffb1378-f1d1-48a8-ab70-bd20b2134efb", "The {0} is less than the {1}.", OrderLine.APL_QtyReceivedInfo.Description, OrderLine.APL_QuantityInfo.Description);
						OrderLine.APL_QtyReceivedInfo.AddWarning(message);
					}
				}
			}
		}

		#region Implementation

		protected AccPayableOrderLine OrderLine;

		#endregion
	}
}
