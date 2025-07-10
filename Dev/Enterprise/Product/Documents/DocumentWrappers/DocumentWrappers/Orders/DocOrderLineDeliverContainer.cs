using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Forwarding.Orders.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocOrderLineDeliverContainer : DocumentWrapper
	{
		DocOrderLineDeliverContainer(OrderLineDeliverContainer orderLineDeliverContainer, BusinessObjectFactory factoryToWrap)
			: base(orderLineDeliverContainer, factoryToWrap)
		{
		}

		public static DocOrderLineDeliverContainer New(OrderLineDeliverContainer orderLineDeliverContainer, BusinessObjectFactory factoryToWrap)
		{
			if (orderLineDeliverContainer == null)
			{
				return null;
			}
			else
			{
				return new DocOrderLineDeliverContainer(orderLineDeliverContainer, factoryToWrap);
			}
		}

		#region Overrides

		public override string ToString()
		{
			return ZString.Empty;
		}

		#endregion

		#region Wrapper Fields

		public DocOrderLineDelivery Delivery
		{
			get { return DocOrderLineDelivery.New(OrderLineDeliverContainer.OrderLineDelivery, Factory); }
		}

		public DocRefContainer RefContainer
		{
			get { return DocRefContainer.New(OrderLineDeliverContainer.ContainerType, Factory); }
		}

		public DocUNLOCO NKLoadPort
		{
			get { return DocUNLOCO.New(OrderLineDeliverContainer.LoadPort, Factory); }
		}

		public DocVessel NKArrivalVessel
		{
			get { return DocVessel.New(OrderLineDeliverContainer.ArrivalVessel, Factory); }
		}

		#endregion

		#region ZString Fields

		public ZString ContainerNum
		{
			get { return OrderLineDeliverContainer.J5_ContainerNum; }
		}

		public ZString ContainerSeal
		{
			get { return OrderLineDeliverContainer.J5_ContainerSeal; }
		}

		public ZString CustomAttribute1
		{
			get { return OrderLineDeliverContainer.J5_CustomAttribute1; }
		}

		public ZString CustomAttribute2
		{
			get { return OrderLineDeliverContainer.J5_CustomAttribute2; }
		}

		public ZString CustomAttribute3
		{
			get { return OrderLineDeliverContainer.J5_CustomAttribute3; }
		}

		public ZString MasterBill
		{
			get { return OrderLineDeliverContainer.J5_MasterBill; }
		}

		public ZString PackUQ
		{
			get { return OrderLineDeliverContainer.J5_F3_NKPackType; }
		}

		public ZString VolumeUQ
		{
			get { return OrderLineDeliverContainer.J5_VolumeUQ; }
		}

		public ZString Voyage
		{
			get { return OrderLineDeliverContainer.J5_Voyage; }
		}

		public ZString WeightUQ
		{
			get { return OrderLineDeliverContainer.J5_WeightUQ; }
		}

		#endregion

		#region ZDateTime Fields

		public ZDateTime CustomDate1
		{
			get { return OrderLineDeliverContainer.J5_CustomDate1; }
		}

		public ZDateTime CustomDate2
		{
			get { return OrderLineDeliverContainer.J5_CustomDate2; }
		}

		public ZDateTime CustomDate3
		{
			get { return OrderLineDeliverContainer.J5_CustomDate3; }
		}

		public ZDateTime ETA
		{
			get { return OrderLineDeliverContainer.J5_ETA; }
		}

		public ZDateTime ETD
		{
			get { return OrderLineDeliverContainer.J5_ETD; }
		}

		public ZDateTime InstoreDate
		{
			get { return OrderLineDeliverContainer.J5_InstoreDate; }
		}

		public ZDateTime SightedDate
		{
			get { return OrderLineDeliverContainer.J5_SightedDate; }
		}

		#endregion

		#region ZDecimal

		public ZDecimal CustomDecimal1
		{
			get { return OrderLineDeliverContainer.J5_CustomDecimal1; }
		}

		public ZDecimal CustomDecimal2
		{
			get { return OrderLineDeliverContainer.J5_CustomDecimal2; }
		}

		public ZDecimal CustomDecimal3
		{
			get { return OrderLineDeliverContainer.J5_CustomDecimal3; }
		}

		public ZDecimal QuantityInStore
		{
			get { return OrderLineDeliverContainer.J5_QuantityInStore; }
		}

		public ZDecimal QuantityInvoiced
		{
			get { return OrderLineDeliverContainer.J5_QuantityInvoiced; }
		}

		public ZDecimal Volume
		{
			get { return OrderLineDeliverContainer.J5_Volume; }
		}

		public ZDecimal Weight
		{
			get { return OrderLineDeliverContainer.J5_Weight; }
		}

		#endregion

		#region ZBool Fields

		public ZBool CustomFlag1
		{
			get { return OrderLineDeliverContainer.J5_CustomFlag1; }
		}

		public ZBool CustomFlag2
		{
			get { return OrderLineDeliverContainer.J5_CustomFlag2; }
		}

		public ZBool CustomFlag3
		{
			get { return OrderLineDeliverContainer.J5_CustomFlag3; }
		}

		#endregion

		#region ZShort Fields

		public ZShort PackCount
		{
			get { return OrderLineDeliverContainer.J5_PackCount; }
		}

		#endregion

		#region Implementation

		OrderLineDeliverContainer OrderLineDeliverContainer
		{
			get { return (OrderLineDeliverContainer)WrappedObject; }
		}

		#endregion
	}
}
