using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using Enterprise.DbUpgrader.Resource.Version;

namespace Enterprise.DbUpgrader.Startup
{
	interface IReadonlyStoregeDocsKeeper
	{
		void AllowWrite(AdminConnection connection);
		void RestoreReadonlyState(AdminConnection connection);
	}

	static class ReadonlyStoregeDocsKeeperFactory
	{
		public static IReadonlyStoregeDocsKeeper New(IVersionChangeInfo upgradeVersionInfo, AdminConnection connection)
		{
			return SchemaVersion.DocManager.IsBetweenOrEqualToTopVersion(upgradeVersionInfo.DbReferenceVersion_Schema, SchemaVersion.Application)
				? new ReadonlyStoregeDocsKeeper(connection)
				: new NoDocManagerUpgradeReadonlyStoregeDocsKeeper();
		}

		class NoDocManagerUpgradeReadonlyStoregeDocsKeeper : IReadonlyStoregeDocsKeeper
		{
			void IReadonlyStoregeDocsKeeper.AllowWrite(AdminConnection connection)
			{
			}

			void IReadonlyStoregeDocsKeeper.RestoreReadonlyState(AdminConnection connection)
			{
			}
		}
	}

	class ReadonlyStoregeDocsKeeper : IReadonlyStoregeDocsKeeper
	{
		public ReadonlyStoregeDocsKeeper(AdminConnection connection)
		{
			readOnlyStorageDocDatabases = connection
				.GetReadOnlyDocManagerDbList()
				.ToList();
		}

		public void AllowWrite(AdminConnection connection)
			=> AlterStorageDocDatabaseReadWriteMode(connection, writeable: true);

		public void RestoreReadonlyState(AdminConnection connection)
			=> AlterStorageDocDatabaseReadWriteMode(connection, writeable: false);

		void AlterStorageDocDatabaseReadWriteMode(AdminConnection connection, bool writeable)
		{
			foreach (var databaseName in readOnlyStorageDocDatabases)
			{
				connection.AlterDbWriteableState(databaseName, writeable);
			}
		}

		/// <summary>
		/// List of readonly StorageDoc databases involved in the upgrade process
		/// </summary>
		readonly List<string> readOnlyStorageDocDatabases;
	}
}
