namespace Enterprise.Customs.EU.Business
{
	public interface ICustomsOfficeValidationDecider
	{
		bool IsRuleR0676Active { get; }
	}
}
