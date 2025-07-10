using CargoWise.EntityFramework;
using CargoWise.Types;

using Enterprise.DocumentWrappers;
using Enterprise.Freight.Forwarding.Orders.Business;

namespace Enterprise.Client.Wow
{
	class WowDocImportOrderLine : DocOrderLine
	{
		WowDocImportOrderLine(OrderLine orderLine, BusinessObjectFactory factoryToWrap, bool isUpdated)
			: base(orderLine, factoryToWrap)
		{
			IsUpdated = isUpdated;
		}

		public static WowDocImportOrderLine New(OrderLine orderLine, BusinessObjectFactory factoryToWrap, bool isUpdated)
		{
			if (orderLine == null)
			{
				return null;
			}
			else
			{
				return new WowDocImportOrderLine(orderLine, factoryToWrap, isUpdated);
			}
		}

		public ZString PaddedLineID
		{
			get
			{
				ZString result = OrderLine.JO_LineNo.ToString().PadLeft(4, '0');
				return (DoMulptipleOrderLinesExistOnThisOrder) ? ZString.Format("{0}-{1}", result, OrderLine.JO_SubLineNo.ToString()) : result;
			}
		}

		public ZBool IsUpdated
		{
			get; private set;
		}
	}
}
