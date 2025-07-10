using System;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;

namespace Enterprise.DocumentScanning.Business.Test
{
	internal sealed class StorageDatabaseInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestSetToReadOnlyWhenEDocsStorageProviderIsS3()
		{
			var docDatabase = Db.DatabaseName.Trim() + "_SD001";

			using (SystemDataRegistry.Instance.EDocsStorageProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.EDocsStorageProviders.Code.DB))
			{
				var storageDatabaseInfo = new StorageDatabaseInfo();

				storageDatabaseInfo.DatabaseName = docDatabase;
				Assert(!storageDatabaseInfo.HasErrors);
				storageDatabaseInfo.NewReadOnly = true;
				Assert("No error if EDocsStorageProvider is not S3.", !storageDatabaseInfo.HasErrors);
			}

			using (SystemDataRegistry.Instance.EDocsStorageProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.EDocsStorageProviders.Code.S3))
			{
				var storageDatabaseInfo = new StorageDatabaseInfo();
				storageDatabaseInfo.DatabaseName = docDatabase;
				Assert(!storageDatabaseInfo.HasErrors);
				storageDatabaseInfo.NewReadOnly = true;
				Assert("Should show error if EDocsStorageProvider is S3 and NewReadOnly is true.", storageDatabaseInfo.HasErrors);
			}
		}
	}
}
