using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[DefaultField("Description")]
	public class OrderLineWrapper : GenericWrapper
	{
		public OrderLineWrapper(OrderLine orderLineBO, BusinessObjectFactory factory)
			: base(null, factory)
		{
			OrderLineBO = orderLineBO ?? factory.GetNull<OrderLine>();
		}

		public ZInt LineNumber
		{
			get { return OrderLineBO.JO_LineNo; }
		}

		public ZString Description
		{
			get { return OrderLineBO.JO_Description; }
		}

		public ZDecimal OuterPacks
		{
			get { return OrderLineBO.JO_OuterPacks; }
		}

		public ZDecimal InnerPacks
		{
			get { return OrderLineBO.JO_InnerPacks; }
		}

		public ZDecimal TotalInnerPacks
		{
			get { return OrderLineBO.JO_TotalInnerPacks; }
		}

		public ValueAndUnitWrapper QuantityOrdered
		{
			get { return new ValueAndUnitWrapper(OrderLineBO.JO_Quantity, OrderLineBO.JO_F3_NKPackType, 2, OrderLineBO.JO_F3_NKPackType_List, Factory); }
		}

		public ValueAndUnitWrapper QuantityInvoiced
		{
			get { return new ValueAndUnitWrapper(OrderLineBO.JO_QtyInvoiced, OrderLineBO.JO_F3_NKPackType, 2, OrderLineBO.JO_F3_NKPackType_List, Factory); }
		}

		public ValueAndUnitWrapper QuantityReceived
		{
			get { return new ValueAndUnitWrapper(OrderLineBO.JO_QtyReceived, OrderLineBO.JO_F3_NKPackType, 2, OrderLineBO.JO_F3_NKPackType_List, Factory); }
		}

		public ValueAndUnitWrapper QuantityRemaining
		{
			get { return new ValueAndUnitWrapper(OrderLineBO.JO_QuantityRemaining, OrderLineBO.JO_F3_NKPackType, 2, OrderLineBO.JO_F3_NKPackType_List, Factory); }
		}

		public MoneyWrapper ItemPrice
		{
			get { return new MoneyWrapper(new Money(OrderLineBO.JO_ItemPrice, Currency), Factory); }
		}

		public MoneyWrapper TotalLinePrice
		{
			get { return new MoneyWrapper(new Money(OrderLineBO.JO_LinePrice, Currency), Factory); }
		}

		public ZDateTime RequiredDate
		{
			get { return OrderLineBO.JO_LineDropDate; }
		}

		public CodeAndDescriptionWrapper Status
		{
			get { return new CodeAndDescriptionWrapper(OrderLineBO.JO_LineStatus, OrderLineBO.JO_LineStatus_List, Factory); }
		}

		public CodeAndDescriptionWrapper Product
		{
			get { return new CodeAndDescriptionWrapper(OrderLineBO.JO_Partno, OrderLineBO.JO_Partno_List, Factory); }
		}

		public RefCurrency Currency
		{
			get { return OrderLineBO.Order != null ? OrderLineBO.Order.OrderCurrency : null; }
		}

		public ZString OrderNumber
		{
			get { return OrderLineBO.Order != null ? OrderLineBO.Order.JD_OrderNumberAndSplit : ZString.Empty; }
		}

		readonly OrderLine OrderLineBO;
	}
}
