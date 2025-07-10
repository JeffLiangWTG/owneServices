namespace Enterprise.Accounting.Business.Riba
{
	public interface ICollectionBatchFileGenerator
	{
		string GetFileData();
		bool AttachFileToEdoc(string fileData);
	}
}
