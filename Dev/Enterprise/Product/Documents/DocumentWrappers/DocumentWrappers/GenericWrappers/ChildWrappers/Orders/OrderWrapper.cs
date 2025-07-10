using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Orders.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[DefaultField("OrderNo")]
	public class OrderWrapper : GenericWrapper
	{
		public OrderWrapper(Order orderBO, BusinessObjectFactory factory)
			: base(null, factory)
		{
			OrderBO = orderBO ?? factory.GetNull<Order>();
			fOrderNo = OrderBO.JD_OrderNumberAndSplit;
		}

		public OrderWrapper(OrderItem orderBO, BusinessObjectFactory factory)
			: base(null, factory)
		{
			OrderBO = factory.GetNull<Order>();
			fOrderNo = orderBO == null ? ZString.Empty : orderBO.JT_OrderReference;
		}

		readonly Order OrderBO;
		readonly ZString fOrderNo;

		public ZString OrderNo
		{
			get { return fOrderNo; }
		}

		public ZDateTime OrderDate
		{
			get { return OrderBO == null ? ZDateTime.Empty : OrderBO.JD_OrderDate; }
		}

		public OrderLineWrapperCollection Lines
		{
			get
			{
				if (fLines == null)
				{
					fLines = new OrderLineWrapperCollection(OrderBO, Factory);
				}
				return fLines;
			}
		}
		OrderLineWrapperCollection fLines;
	}
}
