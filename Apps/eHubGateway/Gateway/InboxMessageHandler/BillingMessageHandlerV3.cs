namespace CargoWise.eHub.Gateway
{
	public class BillingMessageHandlerV3 : BillingMessageHandler
	{
		public BillingMessageHandlerV3()
		{
			AddSchemas("CargoWise.eHub.Gateway.InboxMessageHandler.BillingTransactionSchemas.v3.xsd", "http://www.edi.com.au/EnterpriseService/#Billing_1.3");
		}
	}
}
