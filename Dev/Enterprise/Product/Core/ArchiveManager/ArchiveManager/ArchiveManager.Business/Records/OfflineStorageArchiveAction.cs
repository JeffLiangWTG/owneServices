using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ArchiveManager.Integration;
using Enterprise.DocumentScanning.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ArchiveManager.Business.Records
{
	public class OfflineStorageArchiveAction : IArchiveAction
	{
		public OfflineStorageArchiveAction(IArchiveSet set)
		{
			this.set = set;
		}
		readonly IArchiveSet set;

		public ITransactionManager BeginTransactionWithManager()
		{
			return Db.Connection.BeginTransactionWithManager();
		}

		public void Execute()
		{
			const string sql = @"UPDATE dbo.StorageMain SET SM_CD2 = {0} WHERE SM_PK = '{1}'";
			var storageMainPK = set.MainArchiveItem.PK;
			var formattedSql = string.Format(sql, int.MaxValue, storageMainPK);

			var masterFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			masterFactory.NameForDebugging = "DocumentFactoryForArchiveOffline";
			masterFactory.RefreshEnabled = false;
			var storageMain = masterFactory.Load<StorageMain>(storageMainPK);

			if (storageMain != null)
			{
				storageMain.SM_OffLine = ZDateTime.Now;
				storageMain.eDocs.DeleteAll();
			}

			masterFactory.Save();

			_ = Db.Connection.ExecuteNonQuery(formattedSql);
		}
	}
}
