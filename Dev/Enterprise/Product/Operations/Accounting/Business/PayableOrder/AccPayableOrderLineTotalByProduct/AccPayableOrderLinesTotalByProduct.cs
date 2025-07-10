using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.PayableOrder
{
	public class AccPayableOrderLinesTotalByProduct : AutoAccPayableOrderLinesTotalByProduct
	{
		public AccPayableOrderLinesTotalByProduct(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		internal List<AccPayableOrderLine> OrderLines
		{
			get { return this.orderLines ?? (this.orderLines = new List<AccPayableOrderLine>()); }
			set { this.orderLines = value; }
		}
		List<AccPayableOrderLine> orderLines;

		protected override ZString GetProductDescription()
		{
			ZString result = ZString.Empty;

			if (OrderLines.Count > 0)
			{
				OrgSupplierPart product = OrderLines[0].Product;

				if (product != null)
				{
					result = product.OP_Desc;
				}
				else
				{
					result = OrderLines[0].APL_Desc;
				}
			}

			return result;
		}

		public ZInt InnerPacks
		{
			get
			{
				if (!this.innerPacks.HasValue)
				{
					this.innerPacks = (ZInt)OrderLines.Sum(x => x.APL_InnerPacks);
				}

				return this.innerPacks.Value;
			}
		}
		ZInt? innerPacks;

		public ZInt OuterPacks
		{
			get
			{
				if (!this.outerPacks.HasValue)
				{
					this.outerPacks = (ZInt)OrderLines.Sum(x => x.APL_OuterPacks);
				}

				return this.outerPacks.Value;
			}
		}
		ZInt? outerPacks;

		public override ZDecimal Quantity
		{
			get
			{
				if (!this.quantity.HasValue)
				{
					this.quantity = OrderLines.Sum(x => x.APL_Quantity);
				}

				return this.quantity.Value;
			}
		}
		ZDecimal? quantity;

		public override ZDecimal QuantityInvoiced
		{
			get
			{
				if (!this.quantityInvoiced.HasValue)
				{
					this.quantityInvoiced = OrderLines.Sum(x => x.APL_QtyInvoiced);
				}

				return this.quantityInvoiced.Value;
			}
		}
		ZDecimal? quantityInvoiced;

		public override ZDecimal QuantityReceived
		{
			get
			{
				if (!this.quantityReceived.HasValue)
				{
					this.quantityReceived = OrderLines.Sum(x => x.APL_QtyReceived);
				}

				return this.quantityReceived.Value;
			}
		}
		ZDecimal? quantityReceived;

		public override ZDecimal QuantityRemaining
		{
			get
			{
				if (!quantityRemaining.HasValue)
				{
					this.quantityRemaining = OrderLines.Sum(x => x.APL_QuantityRemaining);
				}

				return this.quantityRemaining.Value;
			}
		}
		ZDecimal? quantityRemaining;

		public override ZDecimal LinePrice
		{
			get
			{
				if (!linePrice.HasValue)
				{
					this.linePrice = OrderLines.Sum(x => x.APL_LinePrice);
				}

				return this.linePrice.Value;
			}
		}
		ZDecimal? linePrice;
	}
}

