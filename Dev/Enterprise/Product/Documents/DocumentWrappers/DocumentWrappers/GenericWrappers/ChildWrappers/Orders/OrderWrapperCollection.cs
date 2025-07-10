using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Orders.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class OrderWrapperCollection : GenericWrapperCollection<OrderWrapper>
	{
		public OrderWrapperCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public OrderWrapperCollection(IAttachOrders ordersParent, BusinessObjectFactory factory)
			: base(factory)
		{
			if (ordersParent != null)
			{
				if (!ordersParent.DocsAndCartage.IsDeleted)
				{
					var orderItems = ordersParent.DocsAndCartage.OrderItems;
					if (orderItems.Count > 1 && orderItems.SortInformation.PropertyName != OrderItem.Schema.JT_Sequence)
					{
						Sort(OrderItem.Schema.JT_Sequence, ListSortDirection.Ascending);
					}
					foreach (OrderItem order in orderItems)
					{
						Add(new OrderWrapper(order, Factory));
					}
				}

				foreach (Order order in ordersParent.AttachedOrders)
				{
					Add(new OrderWrapper(order, Factory));
				}
			}
		}
	}
}
