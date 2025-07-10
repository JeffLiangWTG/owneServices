using CargoWise.EntityFramework;

using Enterprise.Freight.Forwarding.Orders.Business;

namespace Enterprise.Client.Wow
{
	public class WoolworthsOrderLineDeliveryContainerValidation : JobOrderLineDeliverContainerValidation
	{
		public WoolworthsOrderLineDeliveryContainerValidation(WoolworthsOrderLineDeliverContainer parent)
			: base(parent)
		{
		}

		public new WoolworthsOrderLineDeliverContainer Parent
		{
			get { return (WoolworthsOrderLineDeliverContainer)base.Parent; }
		}

		protected override void CheckJ5_MasterBill()
		{
			base.CheckJ5_MasterBill();
			MandatoryValidation.CheckEntered(Parent.J5_MasterBillInfo);
		}

		protected override void CheckJ5_RV_NKArrivalVessel()
		{
			base.ValidateJ5_RV_NKArrivalVessel();
			MandatoryValidation.CheckEntered(Parent.J5_RV_NKArrivalVesselInfo);
		}

		protected override void CheckJ5_Voyage()
		{
			base.ValidateJ5_Voyage();
			MandatoryValidation.CheckEntered(Parent.J5_VoyageInfo);
		}
	}
}
