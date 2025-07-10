using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Freight.Forwarding.Orders.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class OrderLineWrapperCollection : GenericWrapperCollection<OrderLineWrapper>
	{
		public OrderLineWrapperCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public OrderLineWrapperCollection(Order orderBO, BusinessObjectFactory factory)
			: base(factory)
		{
			if (orderBO != null)
			{
				foreach (OrderLine orderLine in orderBO.OrderLines)
				{
					Add(new OrderLineWrapper(orderLine, factory));
				}
			}
		}

		public OrderLineWrapperCollection(IAttachOrders ordersParent, BusinessObjectFactory factory)
			: base(factory)
		{
			if (ordersParent != null)
			{
				foreach (OrderLine orderLine in ordersParent.AttachedOrders.Cast<Order>().SelectMany(x => x.OrderLines))
				{
					base.Add(new OrderLineWrapper(orderLine, factory));
				}
			}
		}
	}
}
