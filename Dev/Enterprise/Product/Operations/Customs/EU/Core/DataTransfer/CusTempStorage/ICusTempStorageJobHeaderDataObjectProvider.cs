using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.EU.DataTransfer.CusTempStorage
{
	public interface ICusTempStorageJobHeaderDataObjectProvider
	{
		ITopLevelDataObjectWriter GetTempStorageJobHeaderDataObjectWriter(IDataWritingManager manager);
	}
}
