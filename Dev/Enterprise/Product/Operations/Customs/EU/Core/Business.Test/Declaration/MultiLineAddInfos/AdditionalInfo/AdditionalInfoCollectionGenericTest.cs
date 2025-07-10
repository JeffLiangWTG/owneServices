namespace Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.Testing;

public abstract class AdditionalInfoCollectionGenericTest<TAdditionalInfo> : Customs.Business.Testing.CusSupportingInfoCollectionTest<TAdditionalInfo>
	where TAdditionalInfo : AdditionalInfo
{
	protected virtual AdditionalInfoCollection<TAdditionalInfo> GetAdditionalInfoCollection()
	{
		var declaration = Factory.New<JobDeclaration>();
		return new AdditionalInfoCollection<TAdditionalInfo>(declaration);
	}

	protected override Customs.Business.CusSupportingInfoCollection<TAdditionalInfo> GetCusSupportingInfoCollection() => GetAdditionalInfoCollection();
}
