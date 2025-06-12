namespace CargoWise.eHub.Gateway
{
	public class BillingMessageHandlerV2 : BillingMessageHandler
	{
		public BillingMessageHandlerV2()
		{
			AddSchemas("CargoWise.eHub.Gateway.InboxMessageHandler.BillingTransactionSchemas.v2.xsd", "http://www.edi.com.au/EnterpriseService/#Billing_1.2");
		}
	}
}
