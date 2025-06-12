using CargoWise.eHub.Common;

namespace CargoWise.eHub.Gateway
{
	public class BillingMessageHandlerV1 : BillingMessageHandler
	{
		public BillingMessageHandlerV1()
		{
			AddSchemas("CargoWise.eHub.Gateway.InboxMessageHandler.BillingTransactionSchemas.v1.xsd", "");
		}

		internal override void ApplyTransforms(BillingTransaction transaction, string schema)
		{
            BillingMessageHandler.PopulateCategory(transaction);
		}

        internal override bool IsCW1Sender(string senderID, eHubGatewayMessage message)
        {
            var result = true;
            if (!string.IsNullOrEmpty(senderID) && senderID.Length != 9)
            {
                result = false;
            }
            return result;
        }
	}
}
