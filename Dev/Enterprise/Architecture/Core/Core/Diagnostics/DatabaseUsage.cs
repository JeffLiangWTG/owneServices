using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Bi.Common;
using CargoWise.Data;

namespace Enterprise.ZArchitecture.Core.Diagnostics
{
	public enum DbGroupEnum
	{
		Main = 1,
		eDocs = 2,
		UserRepository = 3,
		Audit = 4,
		EDW = 5,
		Other = 6,
		S3Bucket = 7,
	}

	public struct DbGroupSize
	{
		public DbGroupEnum DbGroup;
		public long DiskSizeMb;
		public long UsedSizeMb;

		public long AllocationBufferMb
		{
			get
			{
				if (allocationBufferMb_UsePtyInstead == null)
				{
					allocationBufferMb_UsePtyInstead = GetAllocationBuffer(UsedSizeMb);
				}

				return allocationBufferMb_UsePtyInstead.Value;
			}
		}
		long? allocationBufferMb_UsePtyInstead;

		internal static long GetAllocationBuffer(long dbSize)
		{
			const double bufferFactor = .2;
			return (long)Math.Ceiling(dbSize * bufferFactor);
		}
	}

	public class DbSizeInfoCollection
	{
		public IEnumerable<DbGroupSize> GetDbGroupSizes()
		{
			var dbGroupSizes =
				from dbSizeInfo in DbSizeInfoList
				group dbSizeInfo by dbSizeInfo.DbGroup into sizeGroup
				orderby sizeGroup.Key
				select new DbGroupSize()
				{
					DbGroup = sizeGroup.Key,
					UsedSizeMb = sizeGroup.Sum(dsi => dsi.UsedDataSizeMb),
					DiskSizeMb = sizeGroup.Sum(dsi => dsi.DiskSizeMb),
				};

			return dbGroupSizes;
		}

		protected List<DbSizeInfo> DbSizeInfoList
		{
			get
			{
				if (dbSizeInfoList_UsePtyInstead == null)
				{
					var auxList = new List<DbSizeInfo>();

					GetDbSizeInfoListFromMainConnection(auxList);
					GetDbSizeInfoListFromAuditConnection(auxList);
					GetDbSizeInfoListFromDataWarehouseConnection(auxList);

					dbSizeInfoList_UsePtyInstead = auxList;
				}

				return dbSizeInfoList_UsePtyInstead;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void GetDbSizeInfoListFromMainConnection(List<DbSizeInfo> auxList)
		{
			using (var cmd = Connection.Command("ep_DatabaseSetInfo"))
			{
				cmd.CommandType = CommandType.StoredProcedure;

				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						auxList.Add(new DbSizeInfo(
							reader["DbName"].ToString(),
							Convert.ToInt64(reader["UsedSizeMb"]),
							Convert.ToInt64(reader["DiskSizeMb"]))
						);
					}
				}
			}
		}

		void GetDbSizeInfoListFromAuditConnection(List<DbSizeInfo> auxList)
		{
			var auditServer = BiServers.LoadAuditServerUsingCacheIfPossible(Connection);
			var biDbName = Connection.CurrentDatabase + Db.AuditDatabaseSuffix;
			GetDbSizeInfoListFromBiDatabase(auxList, auditServer, biDbName);
		}

		void GetDbSizeInfoListFromDataWarehouseConnection(List<DbSizeInfo> auxList)
		{
			var dwServer = BiServers.LoadDataWarehouseServerUsingCacheIfPossible(Connection);
			var biDbName = Connection.CurrentDatabase + Db.EdwDatabaseSuffix;
			GetDbSizeInfoListFromBiDatabase(auxList, dwServer, biDbName);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void GetDbSizeInfoListFromBiDatabase(List<DbSizeInfo> auxList, string biServer, string biDbName)
		{
			if (!string.IsNullOrEmpty(biServer))
			{
				using (var biConnection = Db.NewAdminConnection(biServer, Db.SqlMasterDb))
				{
					if (biConnection.DatabaseExists(biDbName))
					{
						using (((ICurrentDbControl)biConnection).UseDatabase(biDbName))
						using (var cmd = biConnection.Command("ep_DatabaseSetInfo"))
						{
							cmd.CommandType = CommandType.StoredProcedure;

							using (var reader = cmd.ExecuteReader())
							{
								while (reader.Read())
								{
									auxList.Add(new DbSizeInfo(
										reader["DbName"].ToString(),
										Convert.ToInt64(reader["UsedSizeMb"]),
										Convert.ToInt64(reader["DiskSizeMb"]))
									);
								}
							}
						}
					}
				}
			}
		}

		List<DbSizeInfo> dbSizeInfoList_UsePtyInstead;

		protected virtual DbConnection Connection { get { return Db.Connection; } }
	}

	public class DbSizeInfo
	{
		internal DbSizeInfo(string dbName, long usedDataSizeMb, long diskSizeMb)
		{
			this.dbName = dbName;
			this.usedDataSizeMb = usedDataSizeMb;
			this.diskSizeMb = diskSizeMb;
		}

		internal string DbName
		{
			get { return dbName; }
		}
		readonly string dbName;

		internal DbGroupEnum DbGroup
		{
			get
			{
				if (dbGroup == null)
				{
					dbGroup = GetDbGroup(dbName);
				}
				return dbGroup.Value;
			}
		}
		DbGroupEnum? dbGroup;

		internal DbGroupEnum GetDbGroup(string dbName)
		{
			if (DataUtils.IsDbNameAlphaNumeric(dbName))
			{
				return DbGroupEnum.Main;
			}
			else if (Regex.IsMatch(dbName, Db.StorageDocDbSuffixSqlPattern + "$", RegexOptions.IgnoreCase))
			{
				return DbGroupEnum.eDocs;
			}
			else if (Regex.IsMatch(dbName, DbUserRepository.RepositoryDbSuffix + "$", RegexOptions.IgnoreCase))
			{
				return DbGroupEnum.UserRepository;
			}
			else if (Regex.IsMatch(dbName, Db.AuditDatabaseSuffix + "$", RegexOptions.IgnoreCase))
			{
				return DbGroupEnum.Audit;
			}
			else if (Regex.IsMatch(dbName, Db.EdwDatabaseSuffix + "$", RegexOptions.IgnoreCase))
			{
				return DbGroupEnum.EDW;
			}
			else
			{
				return DbGroupEnum.Other;
			}
		}

		internal long DiskSizeMb
		{
			get { return diskSizeMb; }
		}
		readonly long diskSizeMb;

		internal long UsedDataSizeMb
		{
			get { return usedDataSizeMb; }
		}
		readonly long usedDataSizeMb;
	}
}
