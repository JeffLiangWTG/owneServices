namespace Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos
{
	public interface IAdditionalInfosProviderWithValidationDecider : IAdditionalInfosProvider
	{
		IAdditionalInfoValidationDecider ValidationDecider { get; }
	}
}
