namespace CargoWise.eHub.Gateway
{
	public class BillingMessageHandlerV4 : BillingMessageHandler
	{
		public BillingMessageHandlerV4()
		{
			AddSchemas("CargoWise.eHub.Gateway.InboxMessageHandler.BillingTransactionSchemas.v4.xsd", "http://www.edi.com.au/EnterpriseService/#Billing_1.4");
		}
	}
}
