using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.Test
{
	public class StorageDocsMasterKeyProviderTest : TestCaseWithFactory
	{
		[TestDate(2000, 01, 01, 11, 11, 11)]
		public void TestGetCurrentMasterKey_GetMasterKey()
		{
			var originalMK = StorageDocsMasterKeyProvider.GetCurrentMasterKey();
			var originalMK2 = StorageDocsMasterKeyProvider.GetCurrentMasterKey();

			AssertEquals("Retrieving master key twice at same time should return the same key", originalMK.PK, originalMK2.PK);
			AssertEquals(originalMK.SCK_KeyValue, originalMK2.SCK_KeyValue);
			AssertEquals(ZDateTime.UtcNow, originalMK.SCK_SystemCreateTimeUtc);

			TestDateAttribute.Date = ZDateTime.UtcNow.ToDateTime().AddDays(DocManagerRegistry.Instance.EDocsEncryptionMasterKeyRotationPeriod.Value + 1);
			AssertEquals(new ZDateTime(2001, 01, 01, 11, 11, 11), ZDateTime.UtcNow);

			var mk = StorageDocsMasterKeyProvider.GetCurrentMasterKey();
			var mk2 = StorageDocsMasterKeyProvider.GetCurrentMasterKey();

			AssertEquals("Retrieving master key twice at same time should return the same key", mk.PK, mk2.PK);
			AssertEquals(mk.SCK_KeyValue, mk2.SCK_KeyValue);

			AssertNotEquals("New key should be different from the original key as the original key expires", originalMK.PK, mk.PK);
			AssertNotEquals(originalMK.SCK_KeyValue, mk.SCK_KeyValue);

			var mk3 = StorageDocsMasterKeyProvider.GetMasterKey(mk.PK);
			AssertEquals(mk.PK, mk3.PK);
			AssertEquals(mk.SCK_KeyValue, mk3.SCK_KeyValue);

			var mk4 = StorageDocsMasterKeyProvider.GetMasterKey(originalMK.PK);
			AssertEquals(originalMK.PK, mk4.PK);
			AssertEquals(originalMK.SCK_KeyValue, mk4.SCK_KeyValue);
		}

		[UseSnapshotProtection(true)]
		public void TestGetCurrentMasterKeyInMultipleThreads()
		{
			try
			{
				var tasks = new List<Task>();
				for (var i = 0; i < 5; i++)
				{
					tasks.Add(Task.Factory.StartNew(() =>
					{
						using (Db.DisposableActionForDbConnection())
						{
							_ = StorageDocsMasterKeyProvider.GetCurrentMasterKey();
						}
					}));
				}

				Task.WaitAll(tasks.ToArray());
				AssertEquals("There should be only 1 master key created", 1, Factory.GetDatabaseCount(typeof(StorageDocsMasterKey)));
			}
			finally
			{
				// Delete the master key created by sub threads
				TestCaseHelper.ClearTable($"{StorageDocsMasterKeySchema.Constants.TableName}");
			}
		}

		[TestDate(2022, 12, 25, 5, 16, 33)]
		public void TestGetCurrentMasterKey_NoMasterKey()
		{
			var masterKey = StorageDocsMasterKeyProvider.GetCurrentMasterKey();
			AssertNotNull(masterKey);
			AssertEquals("PK", false, masterKey.PK.IsEmpty);
			AssertEquals("Value Length", 16, masterKey.SCK_KeyValue.Length);
			AssertEquals("Value Not Empty", false, masterKey.SCK_KeyValue.IsEmpty);
			AssertEquals("SCK_SystemCreateTimeUtc", ZDateTime.UtcNow, masterKey.SCK_SystemCreateTimeUtc);
		}

		public void TestGetMasterKey_NoMasterKey()
		{
			AssertExceptionThrown<S3CryptoException>(() => StorageDocsMasterKeyProvider.GetMasterKey(Guid.NewGuid()));
			ErrorReporter.HasBeenReported(nameof(S3CryptoException));
			ErrorReporter.Clear();
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestCaseHelper.ClearTable($"{StorageDocsMasterKeySchema.Constants.TableName}");
			AssertEquals("MasteyKey table is empty", false, Factory.Exists(typeof(StorageDocsMasterKey), new ZQuery()));
		}
	}
}
