using System;
using System.IO;

namespace CargoWise.Common
{
	public static class CommonProgramData
	{
		/// <summary>
		/// Gets the directory on the current machine for temporary files which are shared between all users (generally underneath 'C:\ProgramData\CargoWise edi').
		/// </summary>
		/// <param name="subCategoryDirectory">Use this parameter to segregate different types of data into different subdirectories.</param>
		/// <param name="serverName">The server currently connected to.</param>
		/// <param name="databaseName">The database currently using.</param>
		/// <returns>The name of the directory, for e.g.: 'C:\ProgramData\CargoWise edi\Document Engine\localhost\Database Name'</returns>
		public static string GetCargoWiseDirectory(string subCategoryDirectory, string serverName, string databaseName)
		{
			Argument.NotNull(databaseName, nameof(databaseName));
			Argument.NotNull(serverName, nameof(serverName)); // Suggested By ReviewBot 
			Argument.NotNull(subCategoryDirectory, nameof(subCategoryDirectory)); // Suggested By ReviewBot 
			return Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
				"CargoWise edi", // default application name
				subCategoryDirectory,
				serverName,
				databaseName);
		}
	}
}
