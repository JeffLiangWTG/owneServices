namespace Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos
{
	public interface IAdditionalInfoValidationDecider
	{
		bool IsBR2038Rule { get; }

		bool IsC0612Rule { get; }
	}
}
