
using Enterprise.Freight.Forwarding.Orders.Business;

namespace Enterprise.Client.Wow
{
	public class WoolworthsOrderLineDeliveryValidation : JobOrderLineDeliveryValidation
	{
		public WoolworthsOrderLineDeliveryValidation(WoolworthsOrderLineDelivery parent)
			: base(parent)
		{
		}

		public new WoolworthsOrderLineDelivery Parent
		{
			get { return (WoolworthsOrderLineDelivery)base.Parent; }
		}

		protected override void CheckJ4_CustomDecimal5()
		{
			base.CheckJ4_CustomDecimal5();
			if (Parent.J4_QuantityOrdered < Parent.J4_Calc_TotalQuantityInvoiced)
			{
				Parent.J4_CustomDecimal5Info.AddError("The total of quantity invoiced on the container lines must not be greater than the quantity ordered on the delivery line.");
			}
		}

		protected override bool ErrorIfDeliveryPointNotEntered
		{
			get { return true; }
		}
	}
}
