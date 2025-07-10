namespace Enterprise.Customs.EU.Business.Declaration
{
	public sealed class UCC6ImportCusFiscalReferenceValidationDecider : ICusFiscalReferenceValidationDecider
	{
		public bool IsFiscalReferenceAvailabilityValidationActive => true;

		public bool IsRuleR0010Active => true;
	}
}
