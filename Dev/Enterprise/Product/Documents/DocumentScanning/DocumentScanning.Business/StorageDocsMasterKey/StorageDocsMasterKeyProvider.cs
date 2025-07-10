using System;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentScanning.Business
{
	public static class StorageDocsMasterKeyProvider
	{
		public static StorageDocsMasterKey GetMasterKey(ZGuid materKeyPK)
		{
			var masterKey = FactoryInstance.Load<StorageDocsMasterKey>(materKeyPK);
			if (masterKey == null)
			{
				var errorMessage = $"MasterKey {materKeyPK} could not be found.";
				ErrorReporter.ReportOnce(nameof(S3CryptoException), errorMessage);
				throw new S3CryptoException(errorMessage, null);
			}
			return masterKey;
		}

		public static StorageDocsMasterKey GetCurrentMasterKey()
		{
			var masterKey = GetLatestMasterKey();
			if (masterKey == null || IsMasterKeyExpired(masterKey))
			{
				masterKey = CreateNewMasterKeyWithSqlLock(masterKey);
			}

			return masterKey;
		}

		static StorageDocsMasterKey GetLatestMasterKey()
		{
			var query = new ZDBOnlyQuery(typeof(StorageDocsMasterKey));
			query.ReLoadExistingRows = true;
			query.MaximumRows = 1;
			query.OrderBy = StorageDocsMasterKeySchema.Constants.SCK_SystemCreateTimeUtc + " DESC";
			var masterKey = FactoryInstance.LoadTop1<StorageDocsMasterKey>(query);
			return masterKey;
		}

		static bool IsMasterKeyExpired(StorageDocsMasterKey masterKey)
		{
			var rotationDays = DocManagerRegistry.Instance.EDocsEncryptionMasterKeyRotationPeriod.Value;
			return masterKey.SCK_SystemCreateTimeUtc.AddDays(rotationDays) < ZDateTime.UtcNow;
		}

		static StorageDocsMasterKey CreateNewMasterKeyWithSqlLock(StorageDocsMasterKey currentMasterKey)
		{
			// Default to current master key, in case of any issues in creating new master key.
			var masterKey = currentMasterKey;

			var maxRetries = 3;
			for (var i = 0; i < maxRetries; i++)
			{
				var lockedRunResult = Db.Connection.RunLocked(StorageDocsMasterKeyLockKey, isFirstRun =>
				{
					masterKey = GetLatestMasterKey();
					if (masterKey == null || IsMasterKeyExpired(masterKey))
					{
						masterKey = FactoryInstance.New<StorageDocsMasterKey>();
						masterKey.SCK_KeyValue = StorageDocsEncryptionHelper.NewMasterKey();
						FactoryInstance.Save();
					}
				});

				if (lockedRunResult == LockedProcessResult.Completed)
				{
					break;
				}

				Thread.Sleep(300);
			}

			return masterKey;
		}

		public static BusinessObjectFactory FactoryInstance
		{
			get { return factoryInstance ??= new BusinessObjectFactory(); }
		}

		[ThreadStatic]
		static BusinessObjectFactory factoryInstance;

		const string StorageDocsMasterKeyLockKey = nameof(StorageDocsMasterKeyLockKey);
	}
}
