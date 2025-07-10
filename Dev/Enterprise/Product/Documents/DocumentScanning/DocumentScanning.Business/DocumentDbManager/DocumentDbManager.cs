using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Semaphores.Common;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentScanning.Business
{
	public class DocumentDbManager : NonPersistentBusinessObject, IObsoleteValidation
	{
		public DocumentDbManager(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public StorageDatabaseInfoCollection StorageDatabaseCollection
		{
			get
			{
				if (storageDatabaseCollection == null)
				{
					StorageDatabaseInfoCollection tempCollection = new StorageDatabaseInfoCollection(Factory);
					tempCollection.Load();
					storageDatabaseCollection = tempCollection;
				}

				return storageDatabaseCollection;
			}
		}

		StorageDatabaseInfoCollection storageDatabaseCollection;

		public void Save()
		{
			using (ISemaphoreHandle semaphoreHandle = EnvProxy.Instance.SemaphoreProvider.CreateSemaphoreHandle(new DocDbManagerSemaphore()))
			{
				if (semaphoreHandle.Success)
				{
					SetDbReadOnlyStates();
				}
				else
				{
					string message = Res.GetString(
						"ce2dbddd-1346-4482-9fe2-cd18c21ad4f4",
						"Cannot save now, as [{0}] is currently making changes to the eDocs databases.",
						DocDbManagerSemaphore.GetUserHoldingSemaphore(semaphoreHandle, Factory));
					throw new DocDbManagerException(message);
				}
			}
		}

		void SetDbReadOnlyStates()
		{
			foreach (StorageDatabaseInfo storageDbInfo in StorageDatabaseCollection)
			{
				AlterDbReadOnlyState(storageDbInfo);
			}

			StorageDatabaseCollection.HasChanges = false;
		}

		void AlterDbReadOnlyState(StorageDatabaseInfo storageDbInfo)
		{
			if (storageDbInfo.OriginalReadOnly != storageDbInfo.NewReadOnly)
			{
				var conn = ((IDbConnected)Factory).Connection;

				if (storageDbInfo.NewReadOnly)
				{
					conn.AlterDbWriteableStateForDocManager(storageDbInfo.DatabaseName, false);
				}
				else
				{
					conn.AlterDbWriteableStateForDocManager(storageDbInfo.DatabaseName, true);
				}
			}
		}
	}
}
