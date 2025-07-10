using System;
using System.Globalization;
using System.IO;
using System.Transactions;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.ScavengingImportServiceTask
{
	internal static class ScavengingPartitioner
	{
		internal static ZDateTime LatestPartitionTime
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				const string fileGroupNamePrefix = "CIHGROUP";
				const string script = "SELECT TOP 1 name FROM sys.filegroups WHERE name LIKE @prefix ORDER BY data_space_id DESC";
				Db.Connection.ExecuteReader(script,
					command => command.AddParameter("@prefix", System.Data.SqlDbType.VarChar, $"{fileGroupNamePrefix}%"),
					reader =>
					{
						var fileGroupName = reader.GetString(0);
						if (fileGroupName.StartsWith(fileGroupNamePrefix, StringComparison.OrdinalIgnoreCase))
						{
							var fileGroupDateString = fileGroupName.Replace(fileGroupNamePrefix, string.Empty);
							ZDateTime.TryParseExact(fileGroupDateString, out result, "yyyyMM");
						}
					});

				return result;
			}
		}

		public static void EnsureDBPartitionExists(ZDateTime operationTime)
		{
			var partitionSchemeExists = false;
			Db.Connection.ExecuteReader("SELECT 1 FROM sys.partition_schemes WHERE name = 'CIHPARTITIONSCHEME'", _ =>
			{
				partitionSchemeExists = true;
			});

			if (partitionSchemeExists)
			{
				var sameMonth = !LatestPartitionTime.IsEmpty &&
					LatestPartitionTime.Year == operationTime.Year &&
					LatestPartitionTime.Month == operationTime.Month;

				if (!sameMonth && !PartitionExists(operationTime))
				{
					CreatePartition(operationTime);
				}
			}
		}

		internal static void CreatePartition(ZDateTime operationTime)
		{
			var timeSuffix = operationTime.AddMonths(1).ToString("yyyyMM", CultureInfo.InvariantCulture);
			var dbFileName = Db.Connection.ExecuteScalar<string>("SELECT physical_name FROM sys.database_files WHERE name='Odyssey_Data'");
			var dbFilePath = Path.GetDirectoryName(dbFileName);
			var nextMonth = new DateTime(operationTime.Year, operationTime.Month + 1, 1).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
			var culture = CultureInfo.InvariantCulture;
			using (var scope = Globals.IsTest ? null : new TransactionScope())
			{
				var script = string.Format(culture, @"ALTER DATABASE CURRENT ADD FILEGROUP [CIHGROUP{0}]
ALTER DATABASE CURRENT ADD FILE ( NAME = N'CIHData{0}', FILENAME = N'{1}\DATACIH_Data{0}.ndf' , SIZE = {2}KB , FILEGROWTH = {2}KB ) TO FILEGROUP [CIHGROUP{0}]",
					timeSuffix, dbFilePath, Globals.IsTest ? 10240 : 1048576);

				Db.NewAdminConnection().ExecuteNonQuery(script);

				script = string.Format(culture, @"ALTER PARTITION SCHEME [CIHPARTITIONSCHEME] NEXT USED [CIHGROUP{0}]
ALTER PARTITION FUNCTION [CIHPARTITIONFUNCTION]() SPLIT RANGE ('{1}')",
				timeSuffix, nextMonth);

				Db.Connection.ExecuteNonQuery(script);
				scope?.Complete();
			}
		}

		public static bool PartitionExists(ZDateTime operationTime)
		{
			var partitionTime = operationTime.AddMonths(1).ToString("yyyyMM", CultureInfo.InvariantCulture);
			var result = false;
			var script = string.Format(CultureInfo.InvariantCulture, @"
SELECT 1 FROM sys.filegroups WHERE name='CIHGROUP{0}'
", partitionTime);
			Db.Connection.ExecuteReader(script, (_) =>
			{
				result = true;
			});

			return result;
		}

		public static int GetRowCountOfFileGroup(ZDateTime partitionTime)
		{
			var culture = CultureInfo.InvariantCulture;
			var fileGroupName = string.Format(culture, "CIHGROUP{0}", partitionTime.ToString("yyyyMM", culture));
			var result = 0;
			var countScript = string.Format(culture, "SELECT row_count FROM sys.dm_db_partition_stats JOIN sys.allocation_units ON partition_id = container_id WHERE FILEGROUP_NAME(data_space_id) = '{0}'", fileGroupName);
			Db.Connection.ExecuteReader(countScript, reader =>
			{
				result = (int)reader.GetInt64(0);
			});

			return result;
		}

		public static void TruncatePartition(ZDateTime partitionTime, string tableName)
		{
			var culture = CultureInfo.InvariantCulture;
			var suffix = partitionTime.ToString("yyyyMM", culture);
			var partitionNumber = -1;
			Db.Connection.ExecuteReader(string.Format(culture, "SELECT partition_number FROM sys.dm_db_partition_stats JOIN sys.allocation_units ON partition_id = container_id WHERE FILEGROUP_NAME(data_space_id) = 'CIHGROUP{0}'", suffix), reader =>
			{
				partitionNumber = reader.GetInt32(0);
			});

			if (partitionNumber > 0)
			{
				var script = string.Format(culture, "TRUNCATE TABLE {0} WITH (PARTITIONS({1}))", tableName, partitionNumber);
				Db.NewAdminConnection().ExecuteNonQuery(script);

				if (!Globals.IsTest)
				{
					script = string.Format(culture, "ALTER DATABASE CURRENT REMOVE FILE CIHData{0}", suffix);
					Db.NewAdminConnection().ExecuteNonQuery(script);

					script = string.Format(culture, "ALTER DATABASE CURRENT REMOVE FILEGROUP CIHGROUP{0}", suffix);
					Db.NewAdminConnection().ExecuteNonQuery(script);
				}
			}
			else if (!Globals.IsTest)
			{
				string key = "ScavengingImportServiceTask|ScavengingPartitionHelper|PartitionMissingError";
				ErrorReporter.ReportOnce(key, $"Cannot find FileGroup named CIHGROUP{suffix} for table [{tableName}]");
			}

			if (!Globals.IsTest)
			{
				var key = "ScavengingImportServiceTask|ScavengingPartitionHelper|OldTableNeedTruncate";
				ErrorReporter.ReportOnce(key, "WI00219105: SqlServer has been updated to a higher version than 2014, the old table partitions need truncate, please contact to ALZ(Albert Zhao) or ROPE team.");
			}
		}
	}
}
