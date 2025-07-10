using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Enterprise.Dat.Implementation
{
	[SuppressMessage("CargoWiseOne", "CW1116:UseEnterpriseCoreDataWrapperClasses", Justification = "This assembly has no reference to enterprise libraries")]
	public class DbBackupInfo
	{
		public DbBackupInfo()
		{
		}

		public IEnumerable<LogicalNameAndPhysicalFileSuffix> DatabaseFiles
		{
			get { return databaseFiles; }
		}

		public void LoadBackupInfo(System.Data.Common.DbConnection sqlConnection, string backupFilePath)
		{
			int dataFileIndex = 1;
			int logFileIndex = 1;

			string sqlText = String.Format("RESTORE FILELISTONLY FROM DISK = '{0}'", backupFilePath);

			using (var cmd = sqlConnection.CreateCommand()) // DAT setup prior to executing application code
			{
				cmd.CommandText = sqlText;
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						string fileType = reader["Type"].ToString();
						string fileLogicalName = reader["LogicalName"].ToString();

						if (fileType == "D")
						{
							databaseFiles.Add(new LogicalNameAndPhysicalFileSuffix(fileLogicalName, (dataFileIndex == 1 ? "_Data.mdf" : ("_Data" + dataFileIndex.ToString("00") + ".ndf"))));
							dataFileIndex++;
						}
						else if (fileType == "L")
						{
							databaseFiles.Add(new LogicalNameAndPhysicalFileSuffix(fileLogicalName, "_Log" + (logFileIndex == 1 ? "" : logFileIndex.ToString("00")) + ".ldf"));
							logFileIndex++;
						}
					}
				}
			}

			if (databaseFiles.Count == 0)
			{
				throw new Exception("Unable to load database file logical name from backup file " + backupFilePath);
			}
		}

		readonly List<LogicalNameAndPhysicalFileSuffix> databaseFiles = new List<LogicalNameAndPhysicalFileSuffix>();

		public struct LogicalNameAndPhysicalFileSuffix
		{
			public LogicalNameAndPhysicalFileSuffix(string logicalName, string physicalFileSuffix)
			{
				this.logicalName = logicalName;
				this.physicalFileSuffix = physicalFileSuffix;
			}

			public string LogicalName { get { return logicalName; } }
			readonly string logicalName;

			public string PhysicalFileSuffix { get { return physicalFileSuffix; } }
			readonly string physicalFileSuffix;
		}
	}
}
