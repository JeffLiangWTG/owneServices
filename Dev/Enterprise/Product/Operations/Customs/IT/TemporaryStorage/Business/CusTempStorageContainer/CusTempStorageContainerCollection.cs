using Enterprise.Customs.Business;

namespace Enterprise.Customs.IT.TemporaryStorage.Business;

public class CusTempStorageContainerCollection : CusCodeDataCollection<CusTempStorageContainer>
{
	public CusTempStorageContainerCollection(CusTempStorageRegLine master)
		: base(master, CusCodeDataTypeList.Codes.TemporaryStorageContainer)
	{
	}
}
