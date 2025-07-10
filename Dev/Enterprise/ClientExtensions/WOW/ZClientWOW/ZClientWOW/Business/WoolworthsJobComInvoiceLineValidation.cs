using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;

namespace Enterprise.Client.Wow
{
	/// <summary>
	/// Summary description for WoolworthsJobComInvoiceLineValidation.
	/// </summary>
	public class WoolworthsJobComInvoiceLineValidation : Enterprise.Customs.Business.AutoJobComInvoiceLineValidation
	{
		public WoolworthsJobComInvoiceLineValidation(WoolworthsJobComInvoiceLine line) : base(line)
		{
		}

		public WoolworthsJobComInvoiceLine Line
		{
			get
			{
				return (WoolworthsJobComInvoiceLine)base.Parent;
			}
		}

		public void ValidateUnitPrice()
		{
			ValidateCalculatedProperty(Line.UnitPriceInfo);
		}

		protected virtual void CheckUnitPrice()
		{
			foreach (OrderLineDelivery delivery in Line.OrderLineDeliveries)
			{
				if (Line.UnitPrice != delivery.OrderLine.JO_ItemPrice)
				{
					Line.UnitPriceInfo.AddWarning("Unit price not consistent across Invoice Line and Order Line Delivery.");
					break;
				}
			}
		}

		protected override void CheckJI_LinePrice()
		{
			base.CheckJI_LinePrice();
			foreach (OrderLineDelivery delivery in Line.OrderLineDeliveries)
			{
				if (Line.JI_LinePrice != delivery.OrderLine.JO_LinePrice)
				{
					Line.JI_LinePriceInfo.AddWarning("Line price not consistent across InvoiceLine and Order Line Delivery.");
					break;
				}
			}
		}

		protected override void CheckJI_InvoiceQuantity()
		{
			base.CheckJI_InvoiceQuantity();
			OrderLineDeliverContainer orderContainer = Line.OrderLineDeliverContainer;
			if (orderContainer != null)
			{
				if (Line.GetTotalInvoiceQuantityForLinkedOrderContainer() != orderContainer.J5_QuantityInStore)
				{
					Line.JI_InvoiceQuantityInfo.AddWarning("Invoice quantity not consistent across Invoice Line and Order Line Container.");
				}

				ZDecimal vendorPack = orderContainer.OrderLineDelivery.OrderLine.JO_InnerPacks;
				if (vendorPack != 0 && Line.JI_InvoiceQuantity % vendorPack != 0)
				{
					Line.JI_InvoiceQuantityInfo.AddWarning("Quantity invoiced does not divide into order vendor packs (" + vendorPack + ")");
				}
			}
		}

		protected override void CheckJI_InvoiceUQ()
		{
			base.CheckJI_InvoiceUQ();
			foreach (OrderLineDelivery delivery in Line.OrderLineDeliveries)
			{
				if (Line.JI_InvoiceUQ != delivery.OrderLine.JO_F3_NKPackType)
				{
					Line.JI_InvoiceUQInfo.AddWarning("Unit of quantity not consistent across Invoice Line and Order Line Delivery.");
					break;
				}
			}
		}
	}
}
