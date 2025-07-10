using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.PayableOrder
{
	public class AccPayableOrderLineCollection : ActiveBusinessObjectCollection<AccPayableOrderLine>
	{
		public AccPayableOrderLineCollection(AccPayableOrderHeader parentOrder)
			: base(parentOrder)
		{
		}

		public AccPayableOrderLineCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public AccPayableOrderLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public bool ShouldResetDispositionWhenSaving
		{
			get
			{
				foreach (AccPayableOrderLine line in this)
				{
					if ((!line.IsInDatabase && line.HasChanges) || (line.APL_AGInfo.HasChanges || line.APL_ACInfo.HasChanges || line.APL_QuantityInfo.HasChanges || line.APL_ItemPriceInfo.HasChanges || line.APL_LinePriceInfo.HasChanges))
					{
						return true;
					}
				}
				return false;
			}
		}

		public AccPayableOrderHeader ParentOrder
		{
			get { return Relationship.Master as AccPayableOrderHeader; }
		}

		protected override void SetDefaultsForNewElementCore(AccPayableOrderLine newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			newElement.APL_LineNo = OrderLineHighestLineNo + 1;
			if (ParentOrder != null && ParentOrder.Supplier != null && ParentOrder.Supplier.MiscServ != null)
			{
				if (ParentOrder.Supplier.MiscServ.OM_AC_APDefaultChargeCode.IsValid)
				{
					newElement.GenericCharge = ParentOrder.Supplier.MiscServ.OM_AC_APDefaultChargeCode;
				}
			}
		}

		ZInt OrderLineHighestLineNo
		{
			get
			{
				int highestLineNo = 0;

				foreach (AccPayableOrderLine orderLine in this)
				{
					if (orderLine.APL_LineNo > highestLineNo)
					{
						highestLineNo = orderLine.APL_LineNo;
					}
				}

				return highestLineNo;
			}
		}

		public bool IsOrderPartiallyComplete
		{
			get { return !AreAllOrderLinesEmpty && !AreAllOrderLinesFulfilled; }
	}

		internal bool AreAllOrderLinesEmpty
		{
			get { return this.All(orderLine => orderLine.APL_QuantityRemaining == orderLine.APL_Quantity); }
		}

		bool AreAllOrderLinesFulfilled
		{
			get { return this.All(orderLine => orderLine.APL_QuantityRemaining == 0); }
		}
	}
}

