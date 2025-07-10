using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentWrappers
{
	public class DocOrderLine : DocumentWrapper
	{
		protected DocOrderLine(OrderLine orderLine, BusinessObjectFactory factoryToWrap)
			: base(orderLine, factoryToWrap)
		{
		}

		public static DocOrderLine New(BusinessObjectFactory factory, ZGuid pK)
		{
			return New(factory.Load<OrderLine>(pK), factory);
		}

		public static DocOrderLine New(OrderLine orderLine, BusinessObjectFactory factoryToWrap)
		{
			if (orderLine == null)
			{
				return null;
			}
			else
			{
				return new DocOrderLine(orderLine, factoryToWrap);
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
			get { return OrderLine.PK.ToString(); }
		}

		public ZString LineID
		{
			get
			{
				ZString result;
				if (DoMulptipleOrderLinesExistOnThisOrder)
				{
					result = OrderLine.JO_LineNo.ToString() + "-" + OrderLine.JO_SubLineNo.ToString();
				}
				else
				{
					result = OrderLine.JO_LineNo.ToString();
				}

				return result;
			}
		}

		public ZString CustomAttrib1
		{
			get { return OrderLine.JO_CustomAttrib1; }
		}

		public ZString CustomAttrib2
		{
			get { return OrderLine.JO_CustomAttrib2; }
		}

		public ZString CustomAttrib3
		{
			get { return OrderLine.JO_CustomAttrib3; }
		}

		public ZString CustomAttrib4
		{
			get { return OrderLine.JO_CustomAttrib4; }
		}

		public ZString CustomAttrib5
		{
			get { return OrderLine.JO_CustomAttrib5; }
		}

		public ZString CustomAttrib6
		{
			get { return OrderLine.JO_CustomAttrib6; }
		}

		public ZString PartAttrib1
		{
			get { return OrderLine.JO_PartAttrib1; }
		}

		public ZString PartAttrib2
		{
			get { return OrderLine.JO_PartAttrib2; }
		}

		public ZString PartAttrib3
		{
			get { return OrderLine.JO_PartAttrib3; }
		}

		public ZString SerialNumber
		{
			get { return OrderLine.JO_SerialNumber; }
		}

		public ZString Description
		{
			get { return OrderLine.JO_Description; }
		}

		public ZString LineStatus
		{
			get { return OrderLine.JO_LineStatus; }
		}

		public ZString OrderUnitOfQty
		{
			get { return OrderLine.JO_F3_NKPackType; }
		}

		public ZString Partno
		{
			get { return OrderLine.JO_Partno; }
		}

		public ZString LineReference => OrderLine.JO_LineReference;

		#endregion

		#region Collections

		public DocOrderLineDeliveryCollection Deliveries
		{
			get
			{
				DocOrderLineDeliveryCollection result = new DocOrderLineDeliveryCollection(Factory);
				foreach (OrderLineDelivery delivery in OrderLine.Deliveries)
				{
					result.Add(DocOrderLineDelivery.New(delivery, Factory));
				}
				return result;
			}
		}

		#endregion

		#region Wrapper Fields

		public DocOrder Order
		{
			get { return DocOrder.New(OrderLine.Order, Factory); }
		}

		public DocOrgSupplierPart Product
		{
			get
			{
				if (OrderLine.Order != null)
				{
					return DocOrgSupplierPart.New(OrderLine.Product, Factory);
				}
				return null;
			}
		}

		#endregion

		#region ZDecimal Fields

		public ZDecimal QuantityRemaining
		{
			get { return OrderLine.JO_QuantityRemaining; }
		}

		public ZDecimal TotalInnerPacks
		{
			get { return OrderLine.JO_TotalInnerPacks; }
		}

		//		[Obsolete("You dont have to expose everything dude, whoever u are")]
		//		public ZDecimal TotalQuantityInvoiced
		//		{
		//			get { return OrderLine.JO_Calc_TotalQtyReceived; } 
		//		}

		public ZDecimal CustomDecimal1
		{
			get { return OrderLine.JO_CustomDecimal1; }
		}

		public ZDecimal CustomDecimal2
		{
			get { return OrderLine.JO_CustomDecimal2; }
		}

		public ZDecimal CustomDecimal3
		{
			get { return OrderLine.JO_CustomDecimal3; }
		}

		public ZDecimal CustomDecimal4
		{
			get { return OrderLine.JO_CustomDecimal4; }
		}

		public ZDecimal CustomDecimal5
		{
			get { return OrderLine.JO_CustomDecimal5; }
		}

		public ZDecimal InnerPacks
		{
			get { return OrderLine.JO_InnerPacks; }
		}

		public ZDecimal ItemPrice
		{
			get { return OrderLine.JO_ItemPrice; }
		}

		public ZDecimal UnitPrice
		{
			get { return OrderLine.JO_ItemPrice; }
		}

		public ZDecimal LinePrice
		{
			get { return OrderLine.JO_LinePrice; }
		}

		public ZDecimal OuterPacks
		{
			get { return OrderLine.JO_OuterPacks; }
		}

		public ZDecimal QtyInvoiced
		{
			get { return OrderLine.JO_QtyInvoiced; }
		}

		public ZDecimal QtyReceived
		{
			get { return OrderLine.JO_QtyReceived; }
		}

		public ZDecimal ReceivedQty
		{
			get { return OrderLine.JO_QtyReceived; }
		}

		public ZDecimal Quantity
		{
			get { return OrderLine.JO_Quantity; }
		}

		public ZDecimal OpenQuantity
		{
			get { return OrderLine.JO_OpenQuantity; }
		}

		public ZDecimal PackedQuantity
		{
			get { return OrderLine.JO_QtyPacked; }
		}

		#endregion

		#region ZBool Fields

		public ZBool ContainersVisible
		{
			get { return OrderLine.JO_ContainersVisible; }
		}

		public ZBool CustomFlag1
		{
			get { return OrderLine.JO_CustomFlag1; }
		}

		public ZBool CustomFlag2
		{
			get { return OrderLine.JO_CustomFlag2; }
		}

		public ZBool CustomFlag3
		{
			get { return OrderLine.JO_CustomFlag3; }
		}

		public ZBool CustomFlag4
		{
			get { return OrderLine.JO_CustomFlag4; }
		}

		public ZBool CustomFlag5
		{
			get { return OrderLine.JO_CustomFlag5; }
		}

		#endregion

		#region ZDateTime Fields

		public ZDateTime CustomDate1
		{
			get { return OrderLine.JO_CustomDate1; }
		}

		public ZDateTime CustomDate2
		{
			get { return OrderLine.JO_CustomDate2; }
		}

		public ZDateTime CustomDate3
		{
			get { return OrderLine.JO_CustomDate3; }
		}

		public ZDateTime CustomDate4
		{
			get { return OrderLine.JO_CustomDate4; }
		}

		public ZDateTime CustomDate5
		{
			get { return OrderLine.JO_CustomDate5; }
		}

		public ZDateTime LineDropDate
		{
			get { return OrderLine.JO_LineDropDate; }
		}

		public ZDateTime ShipmentWindowStart
		{
			get { return OrderLine.JO_ShipmentWindowStart; }
		}

		public ZDateTime ShipmentWindowEnd
		{
			get { return OrderLine.JO_ShipmentWindowEnd; }
		}

		#endregion

		#region ZInt Fields

		public ZInt LineNo
		{
			get { return OrderLine.JO_LineNo; }
		}

		#endregion

		#region Implementation

		protected OrderLine OrderLine
		{
			get { return (OrderLine)WrappedObject; }
		}

		protected bool DoMulptipleOrderLinesExistOnThisOrder
		{
			get
			{
				ZQuery filter = new ZQuery(JobOrderLineSchema.JO_JD, SQLComparisonOperator.Equal, OrderLine.JO_JD);
				filter.AddToFilter(JoinCondition.And, JobOrderLineSchema.JO_LineNo, SQLComparisonOperator.Equal, OrderLine.JO_LineNo);
				OrderLine[] lines = (OrderLine[])Factory.Load(typeof(OrderLine), filter);

				return (lines.Length > 1);
			}
		}

		#endregion
	}
}
