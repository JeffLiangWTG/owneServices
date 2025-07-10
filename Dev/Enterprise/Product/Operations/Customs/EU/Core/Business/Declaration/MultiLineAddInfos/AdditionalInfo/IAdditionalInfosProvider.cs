namespace Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos
{
	public interface IAdditionalInfosProvider
	{
		IAdditionalInfoCollection<AdditionalInfo> AdditionalInfos { get; }
	}
}
