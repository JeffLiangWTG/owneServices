namespace Enterprise.Customs.EU.Business.Declaration
{
	public interface ICusFiscalReferenceProviderWithValidationDecider
	{
		ICusFiscalReferenceValidationDecider ValidationDecider { get; }
	}
}
