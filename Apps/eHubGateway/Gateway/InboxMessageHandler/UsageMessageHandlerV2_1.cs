using System.Configuration;
using CargoWise.eHub.Common;
using CargoWise.Billing.API;

namespace CargoWise.eHub.Gateway
{
	public class UsageMessageHandlerV2_1 : UsageMessageHandler<V2_1.UsageTransaction>
	{
		public UsageMessageHandlerV2_1()
		{
			AddSchemas("CargoWise.eHub.Gateway.InboxMessageHandler.UsageTransactionSchemas.v2_1.xsd", "http://www.edi.com.au/EnterpriseService/#Usage_2.1");
		}

		public override UsageTransaction CreateUsageTransaction(V2_1.UsageTransaction transaction) => new UsageTransaction
		{
			UsageCount = transaction.UsageCount,
			ServiceOccuredUTC = transaction.ServiceOccuredUTC,
			AdditionalRefs = transaction.AdditionalRefs,
			EnterpriseCode = transaction.EnterpriseCode,
			ServerCode = transaction.ServerCode,
			Environment = transaction.Environment,
			CompanyCode = transaction.CompanyCode,
			CompanyName = transaction.CompanyName,
			BranchCode = transaction.BranchCode,
			UsageCode = transaction.UsageCode
		};
	}
}
