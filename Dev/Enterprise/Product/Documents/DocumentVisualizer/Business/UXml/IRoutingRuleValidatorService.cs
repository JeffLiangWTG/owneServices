namespace Enterprise.DocumentVisualizer.Business
{
	public interface IRoutingRuleValidatorService
	{
		RoutingRuleValidatorResponse PerformValidationCheck(RoutingRuleValidatorRequest request);
	}
}
