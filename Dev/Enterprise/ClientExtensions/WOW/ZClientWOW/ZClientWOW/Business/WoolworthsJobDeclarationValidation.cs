using Enterprise.Freight.Forwarding.Orders.Business;

namespace Enterprise.Client.Wow
{
	public class WoolworthsJobDeclarationValidation : Enterprise.Customs.Business.AutoJobDeclarationValidation
	{
		public WoolworthsJobDeclarationValidation(WoolworthsJobDeclaration parent) : base(parent)
		{
		}

		public new WoolworthsJobDeclaration Parent
		{
			get { return (WoolworthsJobDeclaration)base.Parent; }
		}

		public void RunOrderInvoiceMismatchValidation()
		{
			ValidateJE_RL_NKPortOfLoading();
			//			Parent.DocsAndCartage.Validation.ValidateJP_OA_DeliveryAddress();
		}

		#region Validation for when Invoice Line/Orders not consistent

		protected override void CheckJE_RL_NKPortOfLoading()
		{
			base.CheckJE_RL_NKPortOfLoading();

			bool isInconsistent = false;
			foreach (WoolworthsJobComInvoiceLine invoiceLine in Parent.InvoiceLines)
			{
				foreach (OrderLineDelivery delivery in invoiceLine.OrderLineDeliveries)
				{
					if (Parent.JE_RL_NKPortOfLoading != delivery.OrderLine.Order.JD_RL_NKPortOfLoading)
					{
						isInconsistent = true;
						break;
					}
				}
			}

			if (isInconsistent)
			{
				Parent.JE_RL_NKPortOfLoadingInfo.AddWarning("Port of loading not consistent across Declaration and Order.");
			}
		}

		#endregion
	}
}
