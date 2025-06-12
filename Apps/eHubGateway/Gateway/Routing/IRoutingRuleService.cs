using CargoWise.eHub.Common;

namespace CargoWise.eHub.Gateway.Routing
{
	public interface IRoutingRuleService
	{
		bool IsXHMessage(eHubGatewayMessage message, string senderID, out string resolvedRecipient);
	}
}
