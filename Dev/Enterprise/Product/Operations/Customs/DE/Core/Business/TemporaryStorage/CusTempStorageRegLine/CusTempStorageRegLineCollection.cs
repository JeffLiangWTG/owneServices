namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class CusTempStorageRegLineCollection : EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineCollection<CusTempStorageRegLine>
	{
		public CusTempStorageRegLineCollection(CusTempStorageRegHeader header) : base(header)
		{
		}
	}
}
