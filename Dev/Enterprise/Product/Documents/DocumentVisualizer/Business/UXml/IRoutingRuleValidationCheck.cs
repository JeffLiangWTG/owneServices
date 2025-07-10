using System.ServiceModel;
using Enterprise.DocumentVisualizer.Business.RoutingRuleValidationWebService;

namespace Enterprise.DocumentVisualizer.Business
{
	public interface IRoutingRuleValidationCheck
	{
		RoutingRuleValidatorResponse SendValidationCheck(RoutingEvaluationOCMInput routingEvaluationOCMInput, ChannelFactory<IRoutingRuleValidationWebService> channelFactory);
	}
}
