using System;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.DocumentScanning.Business
{
	public class StorageDatabaseInfoCollection : NonPersistentBusinessObjectCollection<StorageDatabaseInfo>
	{
		public StorageDatabaseInfoCollection(BusinessObjectFactory factory)
			: base(factory) { }

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		public override void Load()
		{
			this.RemoveAll();
			DynamicBusinessObjectCollection loadCollection = GetLoadingUntypedCollection();

			foreach (DynamicBusinessObject bizObj in loadCollection)
			{
				this.Add(new StorageDatabaseInfo()
				{
					DatabaseNumber = new ZInt(bizObj["DbNumber"]),
					DatabaseName = new ZString(bizObj["DbName"]),
					SizeInMb = new ZInt(bizObj["DbSizeMb"]),
					OriginalReadOnly = new ZBool(bizObj["DbReadOnly"]),
					NewReadOnly = new ZBool(bizObj["DbReadOnly"])
				});
			}

			this.HasChanges = false;
		}

		DynamicBusinessObjectCollection GetLoadingUntypedCollection()
		{
			string loadSql = "EXEC ep_StorageDocsMerge_AllDbSizes;";
			DynamicBusinessObjectCollection loadCollection = new DynamicBusinessObjectCollection(Factory);

			try
			{
				loadCollection.Load(loadSql);
			}
			catch (SqlException ex)
			{
				bool handled = false;

				try
				{
					var errorMatch = new DbErrorMatch(ex);

					if (errorMatch.ExceptionType == DbErrorType.InaccessibleFiles)
					{
						HandleUnconvertedReadonlyDatabasesAfterSqlUpgrade();
						loadCollection.Load(loadSql);
						handled = true;
					}
				}
				catch { handled = false; }

				if (!handled)
				{
					throw;
				}
			}

			return loadCollection;
		}

		void HandleUnconvertedReadonlyDatabasesAfterSqlUpgrade()
		{
			using (var adminConn = CargoWise.Data.Db.NewAdminConnection())
			using (adminConn.UseMasterDb())
			{
				string sqlText = String.Format(@"
					DECLARE @SqlCmd nvarchar(max) = '';
					SELECT @SqlCmd = @SqlCmd + 'ALTER DATABASE [' + name + '] SET read_write; ALTER DATABASE [' + name + '] SET read_only;'
						FROM sys.databases
						WHERE [name] like '{0}[_]SD[0-9][0-9][0-9]' AND is_read_only = 1 AND [state] = 3;
					IF (@SqlCmd != '') EXEC (@SqlCmd);",
					Db.DatabaseName);
				adminConn.ExecuteNonQuery(sqlText);
			}
		}

		#region Implementation

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new StorageDatabaseInfo();
		}

		#endregion
	}
}
