
namespace Enterprise.Client.TNT.ServiceTasks.Testing
{
	static class TNTTestFileUtils
	{
		public static string GetProjectNameSpace()
		{
			return typeof(TNTTestFileUtils).Namespace;
		}

		public static string GetDataImportExportFilePath(string fileName)
		{
			const string DataImportExportDirectory = "DataManipulation.DataImportExport.Testing";
			return GetResourcePath(DataImportExportDirectory, fileName);
		}

		public static string GetDataManipulationFilePath(string fileName)
		{
			const string DataImportExportDirectory = "DataManipulation.Testing";
			return GetResourcePath(DataImportExportDirectory, fileName);
		}

		public static string GetNADInterfaceFilePath(string fileName)
		{
			const string DataImportExportDirectory = "DataManipulation.NADInterface.TestFiles";
			return GetResourcePath(DataImportExportDirectory, fileName);
		}

		static string GetResourcePath(string directory, string fileName)
		{
			return $"{GetProjectNameSpace()}.{directory}.{fileName}";
		}
	}
}
