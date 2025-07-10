using System.Collections;

using CargoWise.EntityFramework;

using Enterprise.Freight.Forwarding.Orders.Business;

namespace Enterprise.Client.Wow
{
	class WoolworthsOrderValidation : JobOrderHeaderValidation
	{
		public WoolworthsOrderValidation(WoolworthsOrder parent)
			: base(parent)
		{
		}

		public new WoolworthsOrder Parent
		{
			get { return (WoolworthsOrder)base.Parent; }
		}

		protected override void CheckJD_OrderNumber()
		{
			if (!WowDataRegistry.Instance.EnableOrderNumberFountain)
			{
				base.CheckJD_OrderNumber();
			}
		}

		protected override void CheckJD_OA_BuyerAddress()
		{
			base.CheckJD_OA_BuyerAddress();

			var hash = new Hashtable();
			for (int i = 0; i < Parent.DeliveryPopulationWarnings.Count; i++)
			{
				hash[Parent.DeliveryPopulationWarnings[i]] = null;
			}

			var list = new ArrayList(hash.Keys);
			list.Sort();

			foreach (string warning in list)
			{
				Parent.JD_OA_BuyerAddressInfo.AddWarning(warning);
			}
		}

		protected override void CheckJD_RX_NKOrderCurrency()
		{
			base.CheckJD_RX_NKOrderCurrency();
			MandatoryValidation.CheckEntered(Parent.JD_RX_NKOrderCurrencyInfo);
		}
	}
}
