using Enterprise.Customs.FR.Business.CusTempStorage;

namespace Enterprise.Customs.FR.Module
{
	public class TempStorageRegisterFilterBusinessObject : EU.TemporaryStorage.Module.TempStorageRegisterFilterBusinessObject
	{
		protected override EU.TemporaryStorage.Business.CusTempStorageRegHeader GetCusTempStorageRegHeaderForLookups() => Factory.GetNull<CusTempStorageRegHeader>();
	}
}
