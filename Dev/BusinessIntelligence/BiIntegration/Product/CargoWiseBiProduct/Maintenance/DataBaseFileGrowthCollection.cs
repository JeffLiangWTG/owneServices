using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using CargoWise.Data;

namespace CargoWise.Bi.Maintenance
{
	public class DataBaseFileGrowthCollection
	{
		public DataBaseFileGrowthCollection(DbConnection connection, string databaseName)
		{
			this.connection = connection;
			this.databaseName = databaseName;

			GetFilesToSetFileGrowth(FileType.Data);
			GetFilesToSetFileGrowth(FileType.Log);
		}
		readonly DbConnection connection;
		readonly string databaseName;

		public ReadOnlyCollection<string> DataFiles
		{
			get
			{
				return dataFiles.AsReadOnly();
			}
		}

		readonly List<string> dataFiles = new List<string>();

		public ReadOnlyCollection<string> LogFiles
		{
			get
			{
				return logFiles.AsReadOnly();
			}
		}

		readonly List<string> logFiles = new List<string>();

		void GetFilesToSetFileGrowth(FileType fileType)
		{
			var sqlText = string.Format(
				CultureInfo.InvariantCulture,
				"select [name] from [{0}].sys.database_files where [type] = @fileType and ([is_percent_growth] = 1 or ([growth]*8/1024) <> 128)",
				/*0*/databaseName
			);
			using (var cmd = connection.Command(sqlText))
			{
				cmd.AddParameter("@fileType", SqlDbType.Int, (int)fileType);
				var fileList = DataUtils.GetListOfValuesFromCommand(cmd);
				if (fileType == FileType.Data)
				{
					dataFiles.AddRange(fileList);
				}
				else if (fileType == FileType.Log)
				{
					logFiles.AddRange(fileList);
				}
			}
		}

		enum FileType
		{
			Data = 0,
			Log = 1
		}
	}
}