
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.Wow
{
	public class WoolworthsOrderLineValidation : JobOrderLineValidation
	{
		public WoolworthsOrderLineValidation(WoolworthsOrderLine parent)
			: base(parent)
		{
		}

		public new WoolworthsOrderLine Parent { get { return base.Parent as WoolworthsOrderLine; } }

		protected override MultilingualString GetPartnoValidationMessage()
		{
			return (NoResString)("You have not entered an existing part number that is related to the supplier or buyer.\n" +
				"It may be that the part exists, but it is just not yet been related to buyer on this order.\n" +
				"Saving this order will attempt to match the part with this order.");
		}

		protected override void CheckJO_Quantity()
		{
			base.CheckJO_Quantity();
			if (Parent.JO_Quantity != Parent.JO_Calc_TotalQuantityOrdered)
			{
				Parent.JO_QuantityInfo.AddWarning("The quantity ordered on the order line doesn't match the total of the deliveries.");
			}
		}
	}
}
