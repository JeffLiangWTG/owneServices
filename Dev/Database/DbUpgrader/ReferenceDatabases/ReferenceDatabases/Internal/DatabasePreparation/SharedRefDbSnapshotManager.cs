using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;

namespace Enterprise.DbUpgrader.ReferenceDatabases
{
	class SharedRefDbSnapshotManager
	{
		readonly DateTime snapshotCreateTime;
		public const string TimeStampFormat = "yyyyMMdd_HHmmss";
		const string SnapshotSeparator = "_snapshot-";
		readonly string sourceRefDb;
		readonly string snapshotTimeStamp;
		public readonly string SnapshotRefDb;

		public SharedRefDbSnapshotManager(string sourceRefDb, DateTime snapshotCreateTime)
		{
			this.snapshotCreateTime = snapshotCreateTime;
			this.sourceRefDb = Argument.NotNullOrEmpty(sourceRefDb, nameof(sourceRefDb));
			snapshotTimeStamp = snapshotCreateTime.ToString(TimeStampFormat);
			SnapshotRefDb = sourceRefDb + SnapshotSeparator + snapshotTimeStamp;
		}

		public IDisposable Create(AdminConnection connection)
		{
			using (var newConnection = Db.NewAdminConnection())
			{
				DropOldSnapshots(newConnection);
				CreateSnapshotIfNotExists(newConnection, snapshotTimeStamp, SnapshotRefDb);
			}
			return new DisposableAction(() =>
			{
				DropSnapshots(connection, SnapshotRefDb);
			});
		}

		protected void CreateSnapshotIfNotExists(AdminConnection connection, string timeStamp, string snapshot)
		{
			var sourceRefDbDataFileInfo = GetSourceRefDbDataFileInfo(connection);
			var snapshotDbDataFileInfo = string.Join(", ", sourceRefDbDataFileInfo.Select(r => $"(NAME = '{r.LogicalName}', FILENAME ='{GetSnapshotDbFullFileName(r.PhysicalName, timeStamp)}')"));
			if (!connection.DatabaseExists(snapshot))
			{
				connection.ExecuteNonQuery($@"CREATE DATABASE [{snapshot}] ON {snapshotDbDataFileInfo} AS SNAPSHOT OF [{sourceRefDb}];");
			}
		}

		void DropOldSnapshots(AdminConnection connection)
		{
			var existingSnapShots = new List<string>();
			connection.ExecuteReader($@"SELECT ss.name FROM sys.databases ss INNER JOIN sys.databases src ON src.database_id = ss.source_database_id WHERE src.name ='{sourceRefDb}'", r => existingSnapShots.Add(r.GetString(0)));
			DropSnapshots(connection, existingSnapShots.Where(IsOldOrInvalid).ToArray());
		}

		static void DropSnapshots(AdminConnection connection, params string[] snapshots)
		{
			foreach (var snapshot in snapshots)
			{
				if (connection.DatabaseExists(snapshot))
				{
					DbConnectionKiller.KillOtherConnections(connection, snapshot);
					connection.ExecuteNonQuery($"DROP DATABASE [{snapshot}];");
				}
			}
		}

		static bool TryGetSnapshotCreateDate(string snapshot, out DateTime createTime)
		{
			var strings = snapshot.Split(new[] { SnapshotSeparator }, StringSplitOptions.RemoveEmptyEntries);
			var timeStamp = strings.Length == 2 ? strings[1] : string.Empty;
			createTime = DateTime.MinValue;
			return !string.IsNullOrEmpty(timeStamp) && DateTime.TryParseExact(timeStamp, TimeStampFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out createTime);
		}

		string GetSnapshotDbFullFileName(string sourceRefDbPhysicalName, string timeStamp)
		{
			var folder = Path.GetDirectoryName(sourceRefDbPhysicalName);
			var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(sourceRefDbPhysicalName);
			return Path.Combine(folder, $"{fileNameWithoutExtension}-{timeStamp}.ss");
		}

		IEnumerable<DbFileInfo> GetSourceRefDbDataFileInfo(AdminConnection connection)
		{
			var list = new List<DbFileInfo>();
			connection.ExecuteReader($@"SELECT name, physical_name FROM [{sourceRefDb}].sys.database_files WHERE type = 0;", r =>
			{
				list.Add(new DbFileInfo(r.GetString(0), r.GetString(1)));
			});
			return list;
		}

		bool IsOldOrInvalid(string snapshot)
		{
			return !TryGetSnapshotCreateDate(snapshot, out var snapshotCreateDate) || snapshotCreateTime - snapshotCreateDate > TimeSpan.FromDays(1);
		}

		readonly struct DbFileInfo
		{
			public DbFileInfo(string logicalName, string physicalName)
			{
				LogicalName = logicalName;
				PhysicalName = physicalName;
			}

			public string LogicalName { get; }
			public string PhysicalName { get; }
		}
	}
}
