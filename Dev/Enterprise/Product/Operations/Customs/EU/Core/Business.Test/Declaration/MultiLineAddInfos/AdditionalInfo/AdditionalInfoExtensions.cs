
namespace Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.Testing
{
	public static class AdditionalInfoExtensions
	{
		public static IAdditionalInfoValidationDecider GetValidationDecider(this AdditionalInfo additionalInfo)
		{
			return additionalInfo?.GetValidationProvider()?.ValidationDecider;
		}

		public static IAdditionalInfosProviderWithValidationDecider GetValidationProvider(this AdditionalInfo additionalInfo)
		{
			return additionalInfo?.Parent as IAdditionalInfosProviderWithValidationDecider;
		}
	}
}
