using System;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(EDocsStorageProviderRegistryDataType))]
	sealed class EDocsStorageProviderRegistryDataTypeTest : RegistryDataTypeTestCase<EDocsStorageProviderRegistryDataType>
	{
		protected override bool IsValidatedOnSetEvenIfEqualDefaultValue => true;

		protected override object GetNullRepresentation()
		{
			return StringRegistryDataTypeTest.GetNullStringRepresentation();
		}

		protected override EDocsStorageProviderRegistryDataType GetNewDataType()
		{
			return new EDocsStorageProviderRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			return new[]
			{
				new ValidSampleAndBinaryValueInDB("DB", DataType.Serialise("DB")),
				new ValidSampleAndBinaryValueInDB("S3", DataType.Serialise("S3"))
			};
		}

		[UseSnapshotProtection]
		public void TestValidateBeforeRegistryFormSaveCore_ForDocDatabaseWriteable()
		{
			SystemDataRegistry.Instance.EDocsStorageServiceUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "someurl");
			SystemDataRegistry.Instance.DocManagerStorageBucketName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "somebucket");
			SystemDataRegistry.Instance.EDocsStorageAccess.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "key/access");

			var eDocsProvider = SystemDataRegistry.Instance.EDocsStorageProvider;
			AssertEquals(Core.Constants.EDocsStorageProviders.Code.DB, eDocsProvider.Value);

			using (var adminConn = Db.NewAdminConnection())
			{
				var docDatabaseSD001 = Db.DatabaseName.Trim() + "_SD001";
				var docDatabaseSD002 = Db.DatabaseName.Trim() + "_SD002";
				try
				{
					AdoTestUtils.CreateDbIfNotExists(adminConn, docDatabaseSD001);
					AdoTestUtils.CreateDbIfNotExists(adminConn, docDatabaseSD002);

					adminConn.AlterDbWriteableStateForDocManager(docDatabaseSD001, false);
					adminConn.AlterDbWriteableStateForDocManager(docDatabaseSD002, false);
					var exception = AssertExceptionThrown<RegistryValidationException>(() => DataType.ValidateBeforeRegistryFormSave(eDocsProvider, Core.Constants.EDocsStorageProviders.Code.S3, Guid.Empty, Guid.Empty, Guid.Empty));
					AssertEquals("Some of the eDocs storage databases are read-only, please ensure all eDocs storage databases are writable before enabling S3 compatible storage.", exception.Message);
					adminConn.AlterDbWriteableStateForDocManager(docDatabaseSD001, false);
					adminConn.AlterDbWriteableStateForDocManager(docDatabaseSD002, true);
					exception = AssertExceptionThrown<RegistryValidationException>(() => DataType.ValidateBeforeRegistryFormSave(eDocsProvider, Core.Constants.EDocsStorageProviders.Code.S3, Guid.Empty, Guid.Empty, Guid.Empty));
					AssertEquals("Some of the eDocs storage databases are read-only, please ensure all eDocs storage databases are writable before enabling S3 compatible storage.", exception.Message);

					adminConn.AlterDbWriteableStateForDocManager(docDatabaseSD001, true);
					adminConn.AlterDbWriteableStateForDocManager(docDatabaseSD002, false);
					exception = AssertExceptionThrown<RegistryValidationException>(() => DataType.ValidateBeforeRegistryFormSave(eDocsProvider, Core.Constants.EDocsStorageProviders.Code.S3, Guid.Empty, Guid.Empty, Guid.Empty));
					AssertEquals("Some of the eDocs storage databases are read-only, please ensure all eDocs storage databases are writable before enabling S3 compatible storage.", exception.Message);

					adminConn.AlterDbWriteableStateForDocManager(docDatabaseSD001, true);
					adminConn.AlterDbWriteableStateForDocManager(docDatabaseSD002, true);
					AssertNoExceptionThrown(() => DataType.ValidateBeforeRegistryFormSave(eDocsProvider, Core.Constants.EDocsStorageProviders.Code.S3, Guid.Empty, Guid.Empty, Guid.Empty));
				}
				finally
				{
					adminConn.AlterDbWriteableStateForDocManager(docDatabaseSD001, true);
					adminConn.AlterDbWriteableStateForDocManager(docDatabaseSD002, true);
					AdoTestUtils.DropDbIfExists(adminConn, docDatabaseSD001);
					AdoTestUtils.DropDbIfExists(adminConn, docDatabaseSD002);
				}
			}
		}

		public void TestValidateBeforeRegistryFormSaveCore_ForDisableS3()
		{
			using (SystemDataRegistry.Instance.EDocsStorageProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.EDocsStorageProviders.Code.S3))
			{
				var eDocsProvider = SystemDataRegistry.Instance.EDocsStorageProvider;
				AssertEquals(Core.Constants.EDocsStorageProviders.Code.S3, eDocsProvider.Value);

				AssertNoExceptionThrown(() => DataType.ValidateBeforeRegistryFormSave(eDocsProvider, Core.Constants.EDocsStorageProviders.Code.DB, Guid.Empty, Guid.Empty, Guid.Empty));
				AssertEquals("Continue", UnitTestUserNotification.Instance.LastConfirmationStringShown);

				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Cancel);
				var exception = AssertExceptionThrown<RegistryValidationException>(() => DataType.ValidateBeforeRegistryFormSave(eDocsProvider, Core.Constants.EDocsStorageProviders.Code.DB, Guid.Empty, Guid.Empty, Guid.Empty));
				AssertEquals("Disabling S3 Storage canceled.", exception.Message);
			}
		}

		public void TestValidateBeforeRegistryFormSaveCore_ForEnableS3()
		{
			SystemDataRegistry.Instance.EDocsStorageProvider.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.EDocsStorageProviders.Code.DB);

			SystemDataRegistry.Instance.EDocsStorageServiceUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);
			SystemDataRegistry.Instance.DocManagerStorageBucketName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);
			SystemDataRegistry.Instance.EDocsStorageAccess.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);

			var exception = AssertExceptionThrown<RegistryValidationException>(() => DataType.ValidateBeforeRegistryFormSave(SystemDataRegistry.Instance.EDocsStorageProvider, Core.Constants.EDocsStorageProviders.Code.S3, Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals(@"The following Registry item(s) must be configured before enabling S3 compatible storage:
System -> DocManager -> S3 Storage -> S3 Storage URL
System -> DocManager -> S3 Storage -> S3 Bucket Name
System -> DocManager -> S3 Storage -> S3 Storage Credentials", exception.Message);

			SystemDataRegistry.Instance.EDocsStorageServiceUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ttt.wtg");

			exception = AssertExceptionThrown<RegistryValidationException>(() => DataType.ValidateBeforeRegistryFormSave(SystemDataRegistry.Instance.EDocsStorageProvider, Core.Constants.EDocsStorageProviders.Code.S3, Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals(@"The following Registry item(s) must be configured before enabling S3 compatible storage:
System -> DocManager -> S3 Storage -> S3 Bucket Name
System -> DocManager -> S3 Storage -> S3 Storage Credentials", exception.Message);

			SystemDataRegistry.Instance.DocManagerStorageBucketName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "test");

			exception = AssertExceptionThrown<RegistryValidationException>(() => DataType.ValidateBeforeRegistryFormSave(SystemDataRegistry.Instance.EDocsStorageProvider, Core.Constants.EDocsStorageProviders.Code.S3, Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals(@"The following Registry item(s) must be configured before enabling S3 compatible storage:
System -> DocManager -> S3 Storage -> S3 Storage Credentials", exception.Message);

			SystemDataRegistry.Instance.EDocsStorageAccess.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Key/Access");

			AssertNoExceptionThrown("No exception throw if critical S3 registry are set", () => DataType.ValidateBeforeRegistryFormSave(SystemDataRegistry.Instance.EDocsStorageProvider, Core.Constants.EDocsStorageProviders.Code.S3, Guid.Empty, Guid.Empty, Guid.Empty));
		}
	}
}
