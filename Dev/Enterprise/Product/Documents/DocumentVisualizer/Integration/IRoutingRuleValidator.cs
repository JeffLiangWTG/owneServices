namespace Enterprise.DocumentVisualizer.Integration
{
	public interface IRoutingRuleValidator
	{
		bool IsValid(string interchange);
		(bool ValidationResult, string[] RecipientIds) IsValidWithRecipientIdsRetrieved(string interchange);
	}
}
