using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Forwarding.Orders.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocOrderLineDelivery : DocumentWrapper
	{
		DocOrderLineDelivery(OrderLineDelivery orderLineDelivery, BusinessObjectFactory factoryToWrap)
			: base(orderLineDelivery, factoryToWrap)
		{
		}

		public static DocOrderLineDelivery New(OrderLineDelivery orderLineDelivery, BusinessObjectFactory factoryToWrap)
		{
			if (orderLineDelivery == null)
			{
				return null;
			}
			else
			{
				return new DocOrderLineDelivery(orderLineDelivery, factoryToWrap);
			}
		}

		#region Overrides

		public override string ToString()
		{
			return ZString.Empty;
		}

		#endregion

		#region ZString Fields

		public ZString PrimaryKey
		{
			get { return this.OrderLineDelivery.PK.ToString(); }
		}

		public ZString DeliverPointAndPrimaryKey
		{
			get { return this.OrderLineDelivery.J4_OA_NKDeliveryPoint + PrimaryKey; }
		}

		public ZString DeliverPointCode
		{
			get { return OrderLineDelivery.J4_OA_NKDeliveryPoint; }
		}

		public ZString DeliverPointAddress1
		{
			get { return OrderLineDelivery.J4_DeliverPointAddress1; }
		}

		public ZString DeliverPointAddress2
		{
			get { return OrderLineDelivery.J4_DeliverPointAddress2; }
		}

		public ZString CustomAttribute1
		{
			get { return OrderLineDelivery.J4_CustomAttribute1; }
		}

		public ZString CustomAttribute2
		{
			get { return OrderLineDelivery.J4_CustomAttribute2; }
		}

		public ZString CustomAttribute3
		{
			get { return OrderLineDelivery.J4_CustomAttribute3; }
		}

		public ZString CustomAttribute4
		{
			get { return OrderLineDelivery.J4_CustomAttribute4; }
		}

		public ZString CustomAttribute5
		{
			get { return OrderLineDelivery.J4_CustomAttribute5; }
		}

		#endregion

		#region Collections

		public DocOrderLineDeliverContainerCollection Containers
		{
			get
			{
				DocOrderLineDeliverContainerCollection result = new DocOrderLineDeliverContainerCollection(Factory);
				foreach (OrderLineDeliverContainer container in this.OrderLineDelivery.Containers)
				{
					result.Add(DocOrderLineDeliverContainer.New(container, Factory));
				}
				return result;
			}
		}

		#endregion

		#region Wrapper Fields

		public DocOrderLine OrderLine
		{
			get { return DocOrderLine.New(OrderLineDelivery.OrderLine, Factory); }
		}

		public DocAddress DeliverPoint
		{
			get { return OrderLineDelivery.DeliveryPoint == null ? null : DocAddress.New(OrderLineDelivery.DeliveryPoint, Factory); }
		}

		public DocUNLOCO DestinationPort
		{
			get { return OrderLineDelivery.J4_RL_NKDestinationPort.IsValid ? DocUNLOCO.New(OrderLineDelivery.Factory, OrderLineDelivery.J4_RL_NKDestinationPort) : null; }
		}

		#endregion

		#region ZDecimal Fields

		public ZDecimal TotalQuantity
		{
			get { return OrderLineDelivery.J4_Calc_TotalQuantityAllocated; }
		}

		public ZDecimal Allocated
		{
			get { return OrderLineDelivery.J4_Allocated; }
		}

		public ZDecimal CustomDecimal1
		{
			get { return OrderLineDelivery.J4_CustomDecimal1; }
		}

		public ZDecimal CustomDecimal2
		{
			get { return OrderLineDelivery.J4_CustomDecimal2; }
		}

		public ZDecimal CustomDecimal3
		{
			get { return OrderLineDelivery.J4_CustomDecimal3; }
		}

		public ZDecimal CustomDecimal4
		{
			get { return OrderLineDelivery.J4_CustomDecimal4; }
		}

		public ZDecimal CustomDecimal5
		{
			get { return OrderLineDelivery.J4_CustomDecimal5; }
		}

		#endregion

		#region ZDateTime Fields

		public ZDateTime CustomDate1
		{
			get { return OrderLineDelivery.J4_CustomDate1; }
		}

		public ZDateTime CustomDate2
		{
			get { return OrderLineDelivery.J4_CustomDate2; }
		}

		public ZDateTime CustomDate3
		{
			get { return OrderLineDelivery.J4_CustomDate3; }
		}

		public ZDateTime CustomDate4
		{
			get { return OrderLineDelivery.J4_CustomDate4; }
		}

		public ZDateTime CustomDate5
		{
			get { return OrderLineDelivery.J4_CustomDate5; }
		}

		#endregion

		#region ZBool Fields

		public ZBool CustomFlag1
		{
			get { return OrderLineDelivery.J4_CustomFlag1; }
		}

		public ZBool CustomFlag2
		{
			get { return OrderLineDelivery.J4_CustomFlag2; }
		}

		public ZBool CustomFlag3
		{
			get { return OrderLineDelivery.J4_CustomFlag3; }
		}

		public ZBool CustomFlag4
		{
			get { return OrderLineDelivery.J4_CustomFlag4; }
		}

		public ZBool CustomFlag5
		{
			get { return OrderLineDelivery.J4_CustomFlag5; }
		}

		#endregion

		#region Implementation

		OrderLineDelivery OrderLineDelivery
		{
			get { return (OrderLineDelivery)WrappedObject; }
		}

		#endregion
	}
}
