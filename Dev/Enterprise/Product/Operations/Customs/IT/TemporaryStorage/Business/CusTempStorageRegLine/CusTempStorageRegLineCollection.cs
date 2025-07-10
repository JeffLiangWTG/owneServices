namespace Enterprise.Customs.IT.TemporaryStorage.Business;

public class CusTempStorageRegLineCollection : EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineCollection<CusTempStorageRegLine>
{
	public CusTempStorageRegLineCollection(CusTempStorageRegHeader header) : base(header)
	{
	}
}
