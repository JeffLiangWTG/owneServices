using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.EU.DataTransfer.CusTempStorage
{
	public class CusTempStorageJobHeaderDataObjectProvider : ICusTempStorageJobHeaderDataObjectProvider
	{
		public ITopLevelDataObjectWriter GetTempStorageJobHeaderDataObjectWriter(IDataWritingManager manager) => new CusTempStorageJobHeaderDataObjectWriter(manager);
	}
}
