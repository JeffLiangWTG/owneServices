using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.TemporaryStorage.Business;

public sealed class CusTempStorageRegHeaderAppCodesListProvider :
	Integration.Customs.EU.ICusTempStorageRegHeaderAppCodesListProvider,
	DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider
{
	public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList() => new TemporaryStorageApplicationCodesList();
}
