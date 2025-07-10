using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.Freight.Forwarding.Orders.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.ClientSharedComponents.DataTransfer
{
	public class ClientSharedOrderValueObjectDataAdapter : OrderValueObjectDataAdapter
	{
		public ClientSharedOrderValueObjectDataAdapter()
			: base()
		{
		}

		protected override Order CreateOrUpdateFromValueObjectCore(Xsd.Order xsdOrder, IValueObjectImportContext context)
		{
			Order result = null;

			if (xsdOrder != null)
			{
				result = FindBusinessObject(xsdOrder, context);

				if (result != null)
				{
					if (AllowUpdate(result, xsdOrder, context))
					{
						ProcessExcessOrderlines(result, xsdOrder, context);
						ImportFromValueObject(result, xsdOrder, context);
					}
					else
					{
						OnUserDeclinedImport(result, xsdOrder, context);
					}
				}
				else
				{
					result = NewBusinessObject(xsdOrder, context);
					ImportFromValueObject(result, xsdOrder, context);
				}
			}
			return result;
		}

		#region Implementation

		protected virtual void ProcessExcessOrderline(OrderLine orderline)
		{
			orderline.JO_LineStatus = Core.Constants.OrderStatus.Cancelled;
		}

		protected virtual ZBool AllowUpdate(Order order, Xsd.Order xsdOrder, IValueObjectImportContext context)
		{
			return ShouldUpdateExistingObject(order, context);
		}

		void ProcessExcessOrderlines(Order order, Xsd.Order xsdOrder, IValueObjectImportContext context)
		{
			List<OrderLine> excessOrderlines = new List<OrderLine>();
			foreach (OrderLine orderline in order.OrderLines)
			{
				ZBool exists = false;
				foreach (Xsd.OrderOrderLine xsdOrderline in xsdOrder.OrderLines)
				{
					if (orderline.JO_LineNo == xsdOrderline.OrderLineNo)
					{
						exists = true;
						break;
					}
				}

				if (!exists)
				{
					excessOrderlines.Add(orderline);
				}
			}

			for (int i = excessOrderlines.Count - 1; i >= 0; i--)
			{
				ProcessExcessOrderline(excessOrderlines[i]);
			}
		}

		#endregion
	}
}
