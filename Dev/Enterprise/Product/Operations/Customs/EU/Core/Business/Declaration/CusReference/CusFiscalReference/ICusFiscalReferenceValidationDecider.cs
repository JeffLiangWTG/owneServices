namespace Enterprise.Customs.EU.Business.Declaration
{
	public interface ICusFiscalReferenceValidationDecider
	{
		bool IsFiscalReferenceAvailabilityValidationActive { get; }

		bool IsRuleR0010Active { get; }
	}
}
