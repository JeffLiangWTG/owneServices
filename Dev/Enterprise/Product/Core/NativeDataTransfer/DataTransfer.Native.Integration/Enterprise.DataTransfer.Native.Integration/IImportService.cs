namespace Enterprise.DataTransfer.Native.Integration
{
	public interface IImportService
	{
		bool CanBeImported(string tableName);
		void Import();
		void GenerateAndSaveXSD(string tableName);
		int GenerateAndSaveAllXSDs(string directoryName);
	}
}
