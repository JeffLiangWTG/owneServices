using System.Configuration;
using CargoWise.eHub.Common;
using CargoWise.Billing.API;

namespace CargoWise.eHub.Gateway
{
	public class UsageMessageHandlerV2 : UsageMessageHandler<V2.UsageTransaction>
	{
		public UsageMessageHandlerV2()
		{
			AddSchemas("CargoWise.eHub.Gateway.InboxMessageHandler.UsageTransactionSchemas.v2.xsd", "http://www.edi.com.au/EnterpriseService/#Usage_2.0");
		}

		public override UsageTransaction CreateUsageTransaction(V2.UsageTransaction transaction) => new UsageTransaction
		{
			UsageCount = transaction.UsageCount,
			ServiceOccuredUTC = transaction.ServiceOccuredUTC,
			AdditionalRefs = transaction.AdditionalRefs
		};
	}
}
