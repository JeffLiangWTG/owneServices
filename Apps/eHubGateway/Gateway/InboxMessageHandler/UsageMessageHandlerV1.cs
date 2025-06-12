using System.Configuration;
using CargoWise.eHub.Common;
using CargoWise.Billing.API;

namespace CargoWise.eHub.Gateway
{
	public class UsageMessageHandlerV1 : UsageMessageHandler<V1.UsageTransaction>
	{
		public UsageMessageHandlerV1()
		{
			AddSchemas("CargoWise.eHub.Gateway.InboxMessageHandler.UsageTransactionSchemas.v1.xsd", "http://www.edi.com.au/EnterpriseService/#Usage_1.1");
		}
		
		public override UsageTransaction CreateUsageTransaction(V1.UsageTransaction transaction) => new CargoWise.Billing.API.UsageTransaction
		{
			UsageCount = transaction.Count,
			ServiceOccuredUTC = transaction.ServiceOccuredUTC,
			AdditionalRefs = transaction.AdditionalRefs
		};
	}
}
