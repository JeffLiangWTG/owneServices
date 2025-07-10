
namespace Enterprise.DataTransfer.Integration
{
	public interface IDataImporterControllingSave : IDataImporter, IOnlySaveDataWhenNoRecordsHaveErrors
	{
	}
}
