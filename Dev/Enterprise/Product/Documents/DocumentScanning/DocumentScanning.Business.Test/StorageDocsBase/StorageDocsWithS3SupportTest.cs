using System;
using System.Data;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentScanning.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.Test
{
	public abstract class StorageDocsWithS3SupportTest : EnterpriseBusinessObjectTestCase
	{
		#region Retrieve with Versioning Supported

		public void TestRetrieveFromExternalStorageWithVersioningNotSupported()
		{
			// Arrange
			var doc = PrepareTestDocument(1, null);
			doc.SC_VersionID = "random";
			MasterFactory.Save();

			var s3ClientMock = GetS3ClientMockForS3Versioning("CorrectVersion");
			SetUpS3Registries();

			// Act
			using (ExternalPersisterProviderTestHelper.MockExternalPersisterForS3(new AWSPersisterForTest { ClientMock = s3ClientMock }))
			{
				var result = doc.SC_ImageData;

				// Assert
				AssertEquals("S3 object is retrieved successfully as VersionID is null in GetObjectRequest", false, result.IsEmpty);
				AssertEquals("random", doc.SC_VersionID);
				AssertEquals(false, doc.HasChanges);
			}
		}

		public void TestRetrieveFromExternalStorageWithVersioningSupported_SC_VersionIDIsEmpty()
		{
			// Arrange
			var doc = PrepareTestDocument(1, null);
			var s3ClientMock = GetS3ClientMockForS3Versioning("null");

			SetUpS3Registries();

			// Act
			using (SystemDataRegistry.Instance.UseVersionID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (ExternalPersisterProviderTestHelper.MockExternalPersisterForS3(new AWSPersisterForTest { ClientMock = s3ClientMock }))
			{
				var result = doc.SC_ImageData;

				// Assert
				AssertEquals("S3 object is retrieved as version Id passed in GetObjectRequest is 'null'", false, result.IsEmpty);
				AssertEquals("", doc.SC_VersionID);
				AssertEquals(false, doc.HasChanges);
			}
		}

		public void TestRetrieveFromExternalStorageWithVersioningSupported_SC_VersionIDIsNotEmpty()
		{
			// Arrange
			var doc = PrepareTestDocument(1, null);
			doc.SC_VersionID = "testing";
			MasterFactory.Save();

			var s3ClientMock = GetS3ClientMockForS3Versioning("testing");
			SetUpS3Registries();

			// Act
			using (SystemDataRegistry.Instance.UseVersionID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (ExternalPersisterProviderTestHelper.MockExternalPersisterForS3(new AWSPersisterForTest { ClientMock = s3ClientMock }))
			{
				var result = doc.SC_ImageData;

				// Assert
				AssertEquals("S3 object is retrieved as version Id passed for GetObjectRequest is 'testing'", false, result.IsEmpty);
				AssertEquals("testing", doc.SC_VersionID);
				AssertEquals(false, doc.HasChanges);
			}
		}

		public void TestRetrieveFromExternalStorageWithVersioningSupported_VersionIdNotFound_NonNullVersionId()
		{
			// Arrange
			var doc = PrepareTestDocument(1, null);
			doc.SC_VersionID = "Testing";

			var s3ClientMock = GetS3ClientMockForS3Versioning("CorrectId");
			SetUpS3Registries();

			// Act
			using (SystemDataRegistry.Instance.UseVersionID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (ExternalPersisterProviderTestHelper.MockExternalPersisterForS3(new AWSPersisterForTest { ClientMock = s3ClientMock }))
			{
				var exceptionThrown = false;
				try
				{
					_ = doc.SC_ImageData;
				}
				catch (Exception ex)
				{
					// Assert
					exceptionThrown = true;
					var notFoundException = ex.InnerException as AmazonS3Exception;
					AssertNotNull("Inner Exception is AmazonS3Exception", notFoundException);
					AssertEquals("NoSuchKey", notFoundException.ErrorCode);
				}
				Assert("Exception is thrown out", exceptionThrown);
			}
		}

		public void TestRetrieveFromExternalStorageWithVersioningSupported_EmptyVersionId_NoChanges()
		{
			// Arrange
			var (doc, org) = SetupOldS3StorageDocs();
			var systemLastEditUser = doc.SC_SystemLastEditUser;
			var systemLastEditTimeUtc = doc.SC_SystemLastEditTimeUtc;

			var s3ClientMock = GetS3ClientMockForS3Versioning("CorrectId");
			SetUpS3Registries();

			// Act
			using (SystemDataRegistry.Instance.UseVersionID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (ExternalPersisterProviderTestHelper.MockExternalPersisterForS3(new AWSPersisterForTest { ClientMock = s3ClientMock }))
			{
				// Assert
				var result = doc.SC_ImageData;
				AssertEquals("S3 object is retrieved the latest version", false, result.IsEmpty);
				AssertEquals("CorrectId", doc.SC_VersionID);
				AssertEquals(false, doc.HasChanges);

				org.OH_FullName = "Test1";
				MasterFactory.Save();

				doc.Reload();
				AssertEquals(string.Empty, doc.SC_VersionID);
				AssertEquals(systemLastEditUser, doc.SC_SystemLastEditUser);
				AssertEquals(systemLastEditTimeUtc, doc.SC_SystemLastEditTimeUtc);
			}
		}

		public void TestRetrieveFromExternalStorageWithVersioningSupported_EmptyVersionId_WhenHasChanges_AfterRetrieving()
		{
			var (doc, org) = SetupOldS3StorageDocs();
			var systemLastEditUser = doc.SC_SystemLastEditUser;
			var systemLastEditTimeUtc = doc.SC_SystemLastEditTimeUtc;

			var s3ClientMock = GetS3ClientMockForS3Versioning("CorrectId");
			SetUpS3Registries();

			// Act
			using (SystemDataRegistry.Instance.UseVersionID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (ExternalPersisterProviderTestHelper.MockExternalPersisterForS3(new AWSPersisterForTest { ClientMock = s3ClientMock }))
			{
				// Assert
				var result = doc.SC_ImageData;
				AssertEquals("S3 object is retrieved the latest version", false, result.IsEmpty);
				AssertEquals("CorrectId", doc.SC_VersionID);
				AssertEquals(false, doc.HasChanges);

				doc.SC_Desc = "Updated Desc";
				AssertEquals(true, doc.HasChanges);

				org.OH_FullName = "Test1";
				MasterFactory.Save();

				doc.Reload();
				AssertEquals("CorrectId", doc.SC_VersionID);
				AssertNotEquals(systemLastEditUser, doc.SC_SystemLastEditUser);
				AssertNotEquals(systemLastEditTimeUtc, doc.SC_SystemLastEditTimeUtc);
			}
		}

		public void TestRetrieveFromExternalStorageWithVersioningSupported_EmptyVersionId_WhenHasChanges_BeforeRetrieving()
		{
			var (doc, org) = SetupOldS3StorageDocs();
			var systemLastEditUser = doc.SC_SystemLastEditUser;
			var systemLastEditTimeUtc = doc.SC_SystemLastEditTimeUtc;

			var s3ClientMock = GetS3ClientMockForS3Versioning("CorrectId");
			SetUpS3Registries();

			// Act
			doc.SC_Desc = "Updated Desc";
			AssertEquals(true, doc.HasChanges);

			using (SystemDataRegistry.Instance.UseVersionID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (ExternalPersisterProviderTestHelper.MockExternalPersisterForS3(new AWSPersisterForTest { ClientMock = s3ClientMock }))
			{
				// Assert
				var result = doc.SC_ImageData;
				AssertEquals("S3 object is retrieved the latest version", false, result.IsEmpty);
				AssertEquals("CorrectId", doc.SC_VersionID);
				AssertEquals(true, doc.HasChanges);

				MasterFactory.Save();

				doc.Reload();
				AssertEquals("CorrectId", doc.SC_VersionID);
				AssertNotEquals(systemLastEditUser, doc.SC_SystemLastEditUser);
				AssertNotEquals(systemLastEditTimeUtc, doc.SC_SystemLastEditTimeUtc);
			}
		}

		public void TestRetrieveFromExternalStorageWithVersioningSupported_EmptyVersionId_NoAuditColumnUpdated_WithoutSaving()
		{
			// Arrange
			var (doc, org) = SetupOldS3StorageDocs();
			var systemLastEditUser = doc.SC_SystemLastEditUser;
			var systemLastEditTimeUtc = doc.SC_SystemLastEditTimeUtc;

			var s3ClientMock = GetS3ClientMockForS3Versioning("CorrectId");
			SetUpS3Registries();

			// Act
			using (SystemDataRegistry.Instance.UseVersionID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (ExternalPersisterProviderTestHelper.MockExternalPersisterForS3(new AWSPersisterForTest { ClientMock = s3ClientMock }))
			{
				// Assert
				var result = doc.SC_ImageData;
				AssertEquals("S3 object is retrieved the latest version", false, result.IsEmpty);
				AssertEquals("CorrectId", doc.SC_VersionID);
				AssertEquals(false, doc.HasChanges);

				doc.Reload();
				AssertEquals(string.Empty, doc.SC_VersionID);
				AssertEquals(systemLastEditUser, doc.SC_SystemLastEditUser);
				AssertEquals(systemLastEditTimeUtc, doc.SC_SystemLastEditTimeUtc);
			}
		}

		(StorageDocs, OrgHeader) SetupOldS3StorageDocs()
		{
			// Arrange
			var storageMain = MasterFactory.NewWithValidTestData<StorageMain>();
			var org = MasterFactory.NewWithValidTestData<OrgHeader>();
			storageMain.SM_ParentFK = org.PK;
			MasterFactory.Save();

			var pk = Guid.NewGuid();
			var sql = @"INSERT INTO dbo.StorageDocs
(SC_PK, SC_SM, SC_DocType, SC_Desc, SC_IsSystemGenerated, SC_ImageData, SC_Date, SC_SystemCreateTimeUtc, SC_SystemLastEditTimeUtc, SC_SystemLastEditUser)
VALUES
(@PK, @FK, 'MSC', 'MSC', 'N', 0x, '2024-01-01', '2024-01-01', '2024-01-01', 'TET')";
			DbCommand command;

			command = Db.Connection.Command(sql);// this is a test!
			command.AddParameter("@PK", SqlDbType.UniqueIdentifier, pk);
			command.AddParameter("@FK", SqlDbType.UniqueIdentifier, storageMain.PK.ToGuid());
			command.ExecuteNonQuery();

			var doc = MasterFactory.Load<StorageDocs>(pk);
			AssertNotNull(doc);

			var systemLastEditUser = doc.SC_SystemLastEditUser;
			var systemLastEditTimeUtc = doc.SC_SystemLastEditTimeUtc;

			AssertEquals("TET", systemLastEditUser);
			AssertLessThanOrEqualTo(systemLastEditTimeUtc, ZDateTime.UtcNow);

			return (doc, org);
		}

		public void TestRetrieveFromExternalStorageWithVersioningSupported_NotFound()
		{
			// Arrange
			var doc = PrepareTestDocument(1, null);
			doc.SC_VersionID = "Test";
			doc.SC_UncompressedSize = 100;
			var s3ClientMock = new Mock<IAmazonS3>(MockBehavior.Strict);
			s3ClientMock.Setup(x => x.GetObjectAsync(It.IsAny<GetObjectRequest>(), CancellationToken.None)).Throws(new AmazonS3Exception("No Such Key") { ErrorCode = "NoSuchKey" });

			SetUpS3Registries();

			// Act
			using (SystemDataRegistry.Instance.UseVersionID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (ExternalPersisterProviderTestHelper.MockExternalPersisterForS3(new AWSPersisterForTest { ClientMock = s3ClientMock }))
			{
				var exceptionThrown = false;
				try
				{
					_ = doc.SC_ImageData;
				}
				catch (Exception ex)
				{
					// Assert
					exceptionThrown = true;
					var notFoundException = ex.InnerException as AmazonS3Exception;
					AssertNotNull("Inner Exception is AmazonS3Exception", notFoundException);
					AssertEquals("NoSuchKey", notFoundException.ErrorCode);
				}
				Assert("Exception is thrown out", exceptionThrown);
			}
		}

		#endregion

		#region Save And Retrieve to S3 with Encryption

		public void TestSaveAndRetrieve()
		{
			SystemDataRegistry.Instance.EDocsStorageProvider.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.EDocsStorageProviders.Code.DB);
			var dataForTest = LargeImageWithBarcodeTifBytes;
			var doc = PrepareTestDocument(1, dataForTest);

			SetUpS3Registries();

			using (ExternalPersisterProviderTestHelper.MockExternalPersisterForS3(supportUpload: true, supportDownload: true))
			{
				AssertEquals("No data key before upload", false, doc.HasDataKey);

				// Save to S3
				var awsPersister = (AWSPersisterForTest)ObjectFactory.Get<IExternalPersisterProvider>().GetExternalPersister("S3");
				awsPersister.VersionIdForTest = "randomVersionID";
				AssertEquals("SaveToExternalStorage with encryption", true, doc.SaveToExternalStorage());
				doc.SC_ImageData = ZBlob.Empty;
				AssertNoExceptionThrown(() => doc.ParentMain.Factory.Save());

				AssertEquals("IsMovingToExternalStorage", true, doc.IsMovingToExternalStorage);
				AssertEquals("Data key is set after uploaded", true, doc.HasDataKey);
				AssertEquals("SC_UncompressedSize", dataForTest.Length, doc.SC_UncompressedSize);
				AssertEquals("SC_VersionID", "randomVersionID", doc.SC_VersionID);

				// Retrieve from S3
				Assert("DB's ImageData should be empty", doc.SC_ImageDataFromDb.IsEmpty);
				AssertEquals("Retrieved eDocs", dataForTest, doc.SC_ImageData);

				// Reload eDoc from new factory and Retrieve from S3 again
				var newMasterFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
				var docReloaded = newMasterFactory.GetFactory(1).Load<StorageDocsBase>(doc.PK);
				Assert("DB's ImageData should be empty", docReloaded.SC_ImageDataFromDb.IsEmpty);
				AssertEquals("Retrieved data from reloaded doc", dataForTest, docReloaded.SC_ImageData);

				// Decrypt and decompress uploaded data manually
				var persister = (AWSPersisterForTest)ObjectFactory.Get<IExternalPersisterProvider>().GetExternalPersister("S3");
				var uploadedDataInS3 = persister.UploadedBytesForTest;
				AssertEquals("SC_ExternalStorageSize", uploadedDataInS3.Length, doc.SC_ExternalStorageSize);

				var masterKey = newMasterFactory.Load<StorageDocsMasterKey>(doc.SC_SCK_MasterKey);
				var dataKey = StorageDocsEncryptionHelper.DecryptDataKey(doc.SC_EncryptedDataKey, masterKey.SCK_KeyValue);
				var decryptedUploadedData = StorageDocsEncryptionHelper.DecryptDocument(uploadedDataInS3, dataKey, doc.PK.ToGuid());
				var uncompressedDecryptedUploadedData = (byte[])ZCompressor.GetUncompressedVersion(decryptedUploadedData, StorageDocsSchema.Constants.SC_ImageData);

				AssertEquals("Decrypt and decompress uploaded data", dataForTest, uncompressedDecryptedUploadedData);
			}
		}

		public void TestSaveAndRetrieve_CryptoException()
		{
			SystemDataRegistry.Instance.EDocsStorageProvider.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.EDocsStorageProviders.Code.DB);
			var dataForTest = LargeImageWithBarcodeTifBytes;
			var doc = PrepareTestDocument(1, dataForTest);

			SetUpS3Registries();

			using (ExternalPersisterProviderTestHelper.MockExternalPersisterForS3(supportUpload: true, supportDownload: true))
			{
				AssertEquals("No data key before upload", false, doc.HasDataKey);

				// Save to S3
				AssertEquals("SaveToExternalStorage with encryption", true, doc.SaveToExternalStorage());
				doc.SC_ImageData = ZBlob.Empty;
				doc.ParentMain.Factory.Save();

				// Retrieve from S3 without issue
				Assert("DB's ImageData should be empty", doc.SC_ImageDataFromDb.IsEmpty);
				AssertEquals("Retrieved eDocs", dataForTest, doc.SC_ImageData);

				var dataKey = doc.SC_EncryptedDataKey;

				// Now mess up with a wrong data keys (decryptable)
				doc.SC_EncryptedDataKey = StorageDocsEncryptionHelper.Encrypt(StorageDocsEncryptionHelper.NewDataKey(), StorageDocsMasterKeyProvider.GetMasterKey(doc.SC_SCK_MasterKey).SCK_KeyValue);
				doc.currentExternalImageData = null;
				AssertExceptionThrown<S3CryptoException>("Wrong data key", () => _ = doc.SC_ImageData);
				AssertEquals("Should report S3CryptoException", true, ErrorReporter.HasBeenReported("S3CryptoException"));
				ErrorReporter.Clear();

				// Then mess up with un-decryptable data keys
				doc.SC_EncryptedDataKey = StorageDocsEncryptionHelper.GetRandomBytes(44);
				doc.currentExternalImageData = null;
				AssertExceptionThrown<S3CryptoException>("Corrupted data key", () => _ = doc.SC_ImageData);
				AssertEquals("Should report S3CryptoException", true, ErrorReporter.HasBeenReported("S3CryptoException"));
				ErrorReporter.Clear();

				// Throw away master key table
				doc.SC_EncryptedDataKey = dataKey;
				var orgMasterKeyPK = doc.SC_SCK_MasterKey;
				doc.SC_SCK_MasterKey = Guid.NewGuid();
				AssertExceptionThrown<S3CryptoException>("Wrong master key", () => _ = doc.SC_ImageData);
				AssertEquals("Should report S3CryptoException", true, ErrorReporter.HasBeenReported("S3CryptoException"));
				ErrorReporter.Clear();

				// Restore master key and data key, and it should be opened
				doc.SC_SCK_MasterKey = orgMasterKeyPK;
				AssertEquals("Retrieved eDocs", dataForTest, doc.SC_ImageData);
			}
		}

		public void TestSaveAndRetrieve_BackwardCompatible_UncompressedAndUnencryptedData()
		{
			SystemDataRegistry.Instance.EDocsStorageProvider.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.EDocsStorageProviders.Code.DB);
			var dataForTest = LargeImageWithBarcodeTifBytes;
			var doc = PrepareTestDocument(1, dataForTest);

			SetUpS3Registries();

			using (ExternalPersisterProviderTestHelper.MockExternalPersisterForS3(supportUpload: true, supportDownload: true))
			{
				var awsPersister = (AWSPersisterForTest)ObjectFactory.Get<IExternalPersisterProvider>().GetExternalPersister("S3");

				// save raw data to S3
				AssertEquals("Upload uncompressed data", true, awsPersister.SaveStream(new MemoryStream(dataForTest), doc.PK).isSaved);
				AssertEquals("Original data is not compressed", false, Compressor.IsCompressed(dataForTest));
				AssertEquals("Uploaded data is not compressed", false, Compressor.IsCompressed(awsPersister.UploadedBytesForTest));
				AssertEquals("Doc is not encrypted", false, doc.HasDataKey);
				var (content, size) = doc.RetrieveFromExternalStorage(awsPersister);
				AssertEquals("Can retrieve uncompressed data", dataForTest, content);
				AssertEquals("Retrieved original size is correct", dataForTest.Length, size);
			}
		}

		public void TestSaveAndRetrieve_BackwardCompatible_CompressedButUnencryptedData()
		{
			SystemDataRegistry.Instance.EDocsStorageProvider.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.EDocsStorageProviders.Code.DB);
			var dataForTest = LargeImageWithBarcodeTifBytes;
			var doc = PrepareTestDocument(1, dataForTest);

			SetUpS3Registries();

			using (ExternalPersisterProviderTestHelper.MockExternalPersisterForS3(supportUpload: true, supportDownload: true))
			{
				var awsPersister = (AWSPersisterForTest)ObjectFactory.Get<IExternalPersisterProvider>().GetExternalPersister("S3");

				// save compressed data to S3
				var compressedData = (byte[])ZCompressor.GetCompressedVersion(dataForTest, StorageDocsSchema.Constants.SC_ImageData);
				AssertEquals("Upload compressed but not encrypted data", true, awsPersister.SaveStream(new MemoryStream(compressedData), doc.PK).isSaved);
				AssertEquals("Uploaded data is compressed", true, Compressor.IsCompressed(awsPersister.UploadedBytesForTest));
				AssertEquals("Doc is not encrypted", false, doc.HasDataKey);
				var (content, size) = doc.RetrieveFromExternalStorage(awsPersister);
				AssertEquals("Can retrieve uncompressed data", dataForTest, content);
				AssertEquals("Retrieved original size is correct", compressedData.Length, size);
			}
		}

		public void TestSaveAndRetrieve_BackwardCompatible_OldEncryptionLogic()
		{
			SystemDataRegistry.Instance.EDocsStorageProvider.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.EDocsStorageProviders.Code.DB);
			var dataForTest = LargeImageWithBarcodeTifBytes;
			var doc = PrepareTestDocument(1, dataForTest);

			SetUpS3Registries();

			using (ExternalPersisterProviderTestHelper.MockExternalPersisterForS3(supportUpload: true, supportDownload: true))
			{
				var awsPersister = (AWSPersisterForTest)ObjectFactory.Get<IExternalPersisterProvider>().GetExternalPersister("S3");

				// save compressed data to S3
				var compressedData = (byte[])ZCompressor.GetCompressedVersion(dataForTest, StorageDocsSchema.Constants.SC_ImageData);
				var dataKey = StorageDocsEncryptionHelper.NewDataKey();
				var encryptedDataWithOldLogic = StorageDocsEncryptionHelperForTest.EncryptDocumentWithPK(compressedData, dataKey, doc.PK.ToGuid());
				AssertEquals("Upload compressed + encrypted data with old logic", true, awsPersister.SaveStream(new MemoryStream(encryptedDataWithOldLogic), doc.PK).isSaved);

				var currentMasterKey = StorageDocsMasterKeyProvider.GetCurrentMasterKey();
				doc.SC_EncryptedDataKey = StorageDocsEncryptionHelper.Encrypt(dataKey, currentMasterKey.SCK_KeyValue);
				doc.SC_SCK_MasterKey = currentMasterKey.PK;
				doc.SC_ImageData = ZBlob.Empty;
				doc.currentExternalImageData = null;

				// Now test if uploaded data with old encryption logic can be opened
				AssertEquals("Doc is encrypted", true, doc.HasDataKey);
				var (content, size) = doc.RetrieveFromExternalStorage(awsPersister);
				AssertEquals("Can retrieve uncompressed data", dataForTest, content);
				AssertEquals("Retrieved original size is correct", encryptedDataWithOldLogic.Length, size);
			}
		}

		public void TestSaveAndRetrieve_RetrievalError_Intermittent()
		{
			SystemDataRegistry.Instance.EDocsStorageProvider.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.EDocsStorageProviders.Code.DB);
			var dataForTest = LargeImageWithBarcodeTifBytes;
			var doc = PrepareTestDocument(1, dataForTest);

			SetUpS3Registries();

			doc.SC_SCK_MasterKey = StorageDocsMasterKeyProvider.GetCurrentMasterKey().PK;
			var dataKey = StorageDocsEncryptionHelper.NewDataKey();
			doc.SC_EncryptedDataKey = StorageDocsEncryptionHelper.Encrypt(dataKey, StorageDocsMasterKeyProvider.GetCurrentMasterKey().SCK_KeyValue);
			var encryptedData = StorageDocsEncryptionHelper.Encrypt(dataForTest, dataKey);
			doc.ParentMain.Factory.Save();

			var clientMock = new Mock<IAmazonS3>(MockBehavior.Strict);

			clientMock.Setup(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), CancellationToken.None)).Returns(Task.FromResult(new PutObjectResponse { HttpStatusCode = HttpStatusCode.OK }));

			// returns corrupted data first then the right data
			clientMock.SetupSequence(x => x.GetObjectAsync(It.IsAny<GetObjectRequest>(), CancellationToken.None))
					.Returns(Task.FromResult(new GetObjectResponse() { ResponseStream = new MemoryStream(new byte[] { 1, 2, 3 }) }))
					.Returns(Task.FromResult(new GetObjectResponse() { ResponseStream = new MemoryStream(encryptedData) }));

			using (ExternalPersisterProviderTestHelper.MockExternalPersisterForS3(new AWSPersisterForTest { ClientMock = clientMock }))
			{
				var awsPersister = (AWSPersisterForTest)ObjectFactory.Get<IExternalPersisterProvider>().GetExternalPersister("S3");
				doc.SaveToExternalStorage();
				doc.SC_ImageData = ZBlob.Empty;
				doc.ParentMain.Factory.Save();

				AssertEquals("Can retrieve", dataForTest, doc.RetrieveFromExternalStorage(awsPersister).content);
			}
		}

		public void TestSaveAndRetrieve_RetrievalError_Extented()
		{
			SystemDataRegistry.Instance.EDocsStorageProvider.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.EDocsStorageProviders.Code.DB);
			var dataForTest = LargeImageWithBarcodeTifBytes;
			var doc = PrepareTestDocument(1, dataForTest);

			SetUpS3Registries();

			var clientMock = new Mock<IAmazonS3>(MockBehavior.Strict);
			clientMock.Setup(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), CancellationToken.None)).Returns(Task.FromResult(new PutObjectResponse { HttpStatusCode = HttpStatusCode.OK }));

			// return corrupted data everytime
			clientMock.SetupSequence(x => x.GetObjectAsync(It.IsAny<GetObjectRequest>(), CancellationToken.None))
					.Returns(Task.FromResult(new GetObjectResponse() { ResponseStream = new MemoryStream(new byte[] { 1, 2, 3 }) }))
					.Returns(Task.FromResult(new GetObjectResponse() { ResponseStream = new MemoryStream(new byte[] { 4, 5, 6 }) }));

			var metadataResponse = new GetObjectMetadataResponse();
			metadataResponse.Headers.ContentLength = 10;

			clientMock.Setup(x => x.GetObjectMetadataAsync(It.IsAny<GetObjectMetadataRequest>(), CancellationToken.None)).Returns(Task.FromResult(metadataResponse));
			using (ExternalPersisterProviderTestHelper.MockExternalPersisterForS3(new AWSPersisterForTest { ClientMock = clientMock }))
			{
				var awsPersister = (AWSPersisterForTest)ObjectFactory.Get<IExternalPersisterProvider>().GetExternalPersister("S3");
				doc.SaveToExternalStorage();
				doc.SC_ImageData = ZBlob.Empty;
				doc.ParentMain.Factory.Save();

				AssertExceptionThrown<ExternalStorageNetworkException>("Retrieva error", "There was a network issue while accessing the document, please try again later.", () => doc.RetrieveFromExternalStorage(awsPersister));
			}
		}

		void TestSaveAndRetrieve_0ByteFile(bool useVersionID)
		{
			var doc = PrepareTestDocument(1, null);
			doc.SC_UncompressedSize = 0;

			var s3ClientMock = new Mock<IAmazonS3>(MockBehavior.Strict);
			s3ClientMock.Setup(x => x.GetObjectAsync(It.IsAny<GetObjectRequest>(), CancellationToken.None)).Throws(new AmazonS3Exception("File Not Found") { ErrorCode = "NotFound" });

			SetUpS3Registries();

			using (SystemDataRegistry.Instance.UseVersionID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, useVersionID))
			using (ExternalPersisterProviderTestHelper.MockExternalPersisterForS3(new AWSPersisterForTest { ClientMock = s3ClientMock }))
			{
				ZBlob? data = null;
				AssertNoExceptionThrown("Retrieving 0 byte file doesn't throw exception", () => data = doc.SC_ImageData);
				AssertEquals("Data returned from 0 byte file should be 0", 0, data.Value.Length);
			}
		}
		public void TestSaveAndRetrieve_0ByteFileWithoutVersionID()
		{
			TestSaveAndRetrieve_0ByteFile(false);
		}

		public void TestSaveAndRetrieve_0ByteFileWithVersionID()
		{
			TestSaveAndRetrieve_0ByteFile(true);
		}

		public StorageDocsBase PrepareTestDocument(int dbNumber, byte[] data)
		{
			TestCaseHelper.ClearTable(AutoStorageMain.Schema.TableName);
			var testDbHelper = new DocManagerDBHelperTestClass();

			if (!testDbHelper.DatabaseExists(dbNumber))
			{
				testDbHelper.CreateDatabase(dbNumber);
			}

			var doc1DatabaseName = testDbHelper.GetDatabaseName(dbNumber);
			TestCaseHelper.ClearTable($"{doc1DatabaseName}..{AutoStorageDocs.Schema.TableName}");

			var doc = GetNewTestBizO(MasterFactory.GetFactory(dbNumber));
			doc.SC_ImageData = data;
			doc.ParentMain.SM_DB = dbNumber;

			MasterFactory.Save();

			return doc;
		}

		#endregion

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetSC_DataReader_NoOpenDataReaderInReturnedStream()
		{
			// Arrange
			var documentFactory = new DocumentFactoryProvider().GetFactory(Factory);
			var storageMain = documentFactory.New<StorageMain>();
			storageMain.SM_DB = 1;
			var storageDoc = storageMain.Documents.AddNew();
			storageDoc.SC_ImageData = File.ReadAllBytes(Path.Combine(BaseSourcePath, "Enterprise", "Product", "Documents", "DocumentScanning", "DocumentScanning.Business.Test", "TestDocs", "200kb.tif"));
			documentFactory.Save();

			// Act
			var newMasterFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			var documentInNewFactory = newMasterFactory.GetFactory(1).Load<StorageDocsBase>(storageDoc.PK);

			using (documentInNewFactory.GetSC_ImageDataReader())
			using (var command = Db.Connection.Command("SELECT COUNT(SM_PK) FROM dbo.StorageMain"))
			{
				// Assert
				AssertNoExceptionThrown(() => command.ExecuteScalar());
			}
		}

		public void TestSaveToExternalStorage_ShouldNotRetrieve()
		{
			SystemDataRegistry.Instance.EDocsStorageProvider.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.EDocsStorageProviders.Code.DB);
			var documentFactory = new DocumentFactoryProvider().GetFactory(Factory);
			var storageMain = documentFactory.New<StorageMain>();
			storageMain.SM_DB = 1;
			var doc = storageMain.Documents.AddNew();
			doc.SC_ImageData = new byte[] { 1, 2, 3 };
			documentFactory.Save();

			AssertNotNull(doc.SC_ImageData);

			SetUpS3Registries();

			// The mock only support upload, if download is called, the test will fail
			using (ExternalPersisterProviderTestHelper.MockExternalPersisterForS3(supportUpload: true, supportDownload: false))
			{
				AssertEquals(true, doc.SaveToExternalStorage());
				AssertEquals(true, doc.IsMovingToExternalStorage);

				doc.SC_ImageData = ZBlob.Empty;

				AssertNoExceptionThrown(() => documentFactory.Save());
			}
		}

		public void TestIsMovingToExternalStorageShouldBeFalseIfIsDeleted()
		{
			var documentFactory = new DocumentFactoryProvider().GetFactory(Factory);
			var doc = documentFactory.NewWithParent(typeof(StorageDocs));
			doc.ParentMain.SM_ParentFK = Factory.NewWithValidTestData<OrgHeader>().PK;
			doc.ParentMain.SM_DB = 2;
			doc.SC_FileName = "TestForViewLocalFile";
			doc.SC_DataType = "xls";
			doc.SC_ImageData = resourceRetriever.Value.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.Test.xls");
			doc.ReadOnly = false;

			documentFactory.Save();

			doc.Delete();
			Assert("Should be false.", !doc.IsMovingToExternalStorage);
		}

		public void TestDeleteWhenS3Enabled()
		{
			using (SystemDataRegistry.Instance.EDocsStorageProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.EDocsStorageProviders.Code.S3))
			{
				var doc = PrepareTestDocument(1, new byte[] { 1, 2, 3 });
				doc.ParentMain.SM_ParentFK = Factory.NewWithValidTestData<OrgHeader>().PK;
				MasterFactory.Save();

				TestDateIncrementalAttribute.Span = new TimeSpan(1, 0, 5, 0);
				doc.Delete();
				MasterFactory.Save();

				AssertEquals("When S3 Enabled, Deleted eDocs should be added to StorageDocsToDelete table", true, new BusinessObjectFactory().Exists(typeof(StorageDocsToDelete), new ZQuery(StorageDocsToDeleteSchema.SCD_StorageDocIdentifier, doc.PK)));
			}

			using (SystemDataRegistry.Instance.EDocsStorageProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.EDocsStorageProviders.Code.DB))
			{
				var doc = PrepareTestDocument(1, new byte[] { 1, 2, 3 });
				doc.ParentMain.SM_ParentFK = Factory.NewWithValidTestData<OrgHeader>().PK;
				MasterFactory.Save();

				TestDateIncrementalAttribute.Span = new TimeSpan(1, 0, 5, 0);
				doc.Delete();
				MasterFactory.Save();

				AssertEquals("When S3 Disable, Deleted eDocs should not be added to StorageDocsToDelete table", false, new BusinessObjectFactory().Exists(typeof(StorageDocsToDelete), new ZQuery(StorageDocsToDeleteSchema.SCD_StorageDocIdentifier, doc.PK)));
			}
		}

		public void TestSetImageDataShouldIgnoreS3Exception()
		{
			SystemDataRegistry.Instance.eDocsMaximumFilesize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);
			SystemDataRegistry.Instance.EDocsStorageProvider.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.EDocsStorageProviders.Code.DB);
			var dataForTest = new byte[100];
			var doc = PrepareTestDocument(1, dataForTest);

			doc.SC_ImageData = ZBlob.Empty;
			doc.ParentMain.Factory.Save();

			using (SystemDataRegistry.Instance.EDocsStorageProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.EDocsStorageProviders.Code.S3))
			using (ExternalPersisterProviderTestHelper.MockExternalPersisterForS3WithThrowAmazonS3Exception())
			{
				// To be able to set SC_ImageData of an existing eDocs where ImageData already in S3, one must have retrieved it successfully from S3 before and cached at currentExternalImageData
				// If the first retrieval was failed, exception should have been thrown and they wont be able to edit and set SC_ImageData again
				doc.currentExternalImageData = dataForTest;
				AssertNoExceptionThrown(() => doc.SC_ImageData = new byte[1]);
				doc.ParentMain.Factory.Save();
			}
		}

		[UseSnapshotProtection]
		public void TestDataReaderIsThreadSafe()
		{
			using (RunNonTransactioned())
			{
				var masterFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
				var numberedFactory = masterFactory.GetFactory(1);
				var doc = StorageFile.NewWithParent_DEBUG(numberedFactory);
				var smallTifTestFile = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.small.tif");
				doc.SetSC_ImageDataSource(new FileStreamSource(smallTifTestFile));
				masterFactory.Save();

				numberedFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory()).GetFactory(1);
				doc = numberedFactory.Load<StorageFile>(doc.PK);
				_ = doc.ParentMain; //triggers the getter in main thread

				var mainThreadReadEvent = new ManualResetEvent(false);
				var extraThreadReadEvent = new ManualResetEvent(false);
				var streamType = typeof(MemoryStream);
				var thread = new Thread(() =>
				{
					using (Db.DisposableActionForDbConnection())
					using (var reader = doc.GetSC_ImageDataReader())
					{
						AssertType("Precondition", streamType, reader);
						reader.ReadByte();
						extraThreadReadEvent.Set();
						mainThreadReadEvent.WaitOne();
						reader.ReadByte();
					}
				});

				thread.Start();
				extraThreadReadEvent.WaitOne();

				using (var reader = doc.GetSC_ImageDataReader())
				{
					AssertType("Precondition", streamType, reader);
					reader.ReadByte();
					mainThreadReadEvent.Set();
					reader.ReadByte();
					thread.Join();
				}
			}
		}

		public void TestSetImageData_ImageIsNotUpdatedWhenMovingToS3()
		{
			SystemDataRegistry.Instance.EDocsStorageProvider.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.EDocsStorageProviders.Code.DB);
			var documentFactory = new DocumentFactoryProvider().GetFactory(Factory);
			var storageMain = documentFactory.New<StorageMain>();
			storageMain.SM_DB = 1;
			var doc = storageMain.Documents.AddNew();
			doc.SC_ImageData = new byte[] { 1, 2, 3 };
			documentFactory.Save();

			AssertEquals("Content is not empty", expected: false, doc.SC_ImageData.IsEmpty);

			SetUpS3Registries();

			// The mock only support upload, if download is called, the test will fail
			using (ExternalPersisterProviderTestHelper.MockExternalPersisterForS3(supportUpload: true, supportDownload: false))
			{
				AssertEquals(true, doc.SaveToExternalStorage());
				AssertEquals(true, doc.IsMovingToExternalStorage);

				doc.SC_ImageData = ZBlob.Empty;

				var fieldInfo = typeof(StorageDocsWithS3Support).GetField("hasSC_ImageDataBeenUpdated", BindingFlags.NonPublic | BindingFlags.Instance);

				AssertNotNull("Field hasSC_ImageDataBeenUpdated should exist", fieldInfo);
				AssertEquals("hasSC_ImageDataBeenUpdated should be false when moving to S3", expected: false, fieldInfo.GetValue(doc));
			}
		}

		public void TestDeleteDocument_OrphanDocuments()
		{
			SetUpS3Registries();
			using (ExternalPersisterProviderTestHelper.MockExternalPersisterForS3(supportUpload: false, supportDownload: false))
			{
				var nonOrphanDoc = PrepareTestDocument(1, new byte[] { 1, 2, 3 });
				nonOrphanDoc.Delete();
				MasterFactory.Save();

				AssertEquals("non-orphan doc is the owner of the S3 content, so deleting non-orphan doc should delete S3 content",
					true, MasterFactory.Exists(typeof(StorageDocsToDelete), new ZQuery(StorageDocsToDeleteSchema.SCD_StorageDocIdentifier, nonOrphanDoc.PK)));

				var orphanDoc1 = PrepareTestDocument(1, new byte[] { 1, 2, 3 });
				orphanDoc1.ParentMain.SM_DB = 2;
				orphanDoc1.Delete();
				MasterFactory.Save();

				AssertEquals("orphan doc is no longer the owner of the S3 content, so deleting orphan doc should NOT delete S3 content",
					false, MasterFactory.Exists(typeof(StorageDocsToDelete), new ZQuery(StorageDocsToDeleteSchema.SCD_StorageDocIdentifier, orphanDoc1.PK)));

				var orphanDoc2 = PrepareTestDocument(1, new byte[] { 1, 2, 3 });
				orphanDoc2.ParentMain.Delete();
				orphanDoc2.Delete();
				MasterFactory.Save();

				AssertEquals("orphan doc is no longer the owner of the S3 content, so deleting orphan doc should NOT delete S3 content",
					false, MasterFactory.Exists(typeof(StorageDocsToDelete), new ZQuery(StorageDocsToDeleteSchema.SCD_StorageDocIdentifier, orphanDoc2.PK)));
			}
		}

		[TestDate(2024, 11, 11, 11, 11, 11)]
		public void TestRetrieveFromExternalStorageNotUpdateSC_Date()
		{
			var doc = PrepareTestDocument(1, null);
			MasterFactory.Save();

			var originalSC_Date = doc.SC_Date;
			AssertEquals("Orignal SC_ImageData Length", 0, doc.SC_ImageData.Length);
			TestDateAttribute.AddMinutes(10);
			AssertEquals("Original SC_Date", originalSC_Date, doc.SC_Date);

			SetUpS3Registries();
			using (ExternalPersisterProviderTestHelper.MockExternalPersisterForS3(false, true))
			{
				var result = doc.SC_ImageData;

				AssertEquals("The content from S3 file should not be empty", false, result.IsEmpty);
				doc.SC_Desc = "Make a change";
				MasterFactory.Save();
				AssertEquals("Should not update SC_Date", originalSC_Date, doc.SC_Date);
			}
		}

		#region Implementation

		protected DocumentFactory MasterFactory => masterFactory ?? (masterFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory()));

		DocumentFactory masterFactory;

		protected abstract StorageDocsBase GetNewTestBizO(NumberedBusinessObjectFactory factory);

		public static void SetUpS3Registries()
		{
			SystemDataRegistry.Instance.EDocsStorageProvider.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.EDocsStorageProviders.Code.S3);
			SystemDataRegistry.Instance.EDocsStorageServiceUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://s3.test.local");
			SystemDataRegistry.Instance.DocManagerStorageBucketName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "test");
			SystemDataRegistry.Instance.EDocsStorageAccess.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "KeyId=dude;Secret=top");
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		byte[] LargeImageWithBarcodeTifBytes => resourceRetriever.Value.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.LargeImageWithBarcode.tif");

		Mock<IAmazonS3> GetS3ClientMockForS3Versioning(string expectedVersionId)
		{
			var response = new GetObjectResponse();
			var s3ClientMock = new Mock<IAmazonS3>(MockBehavior.Strict);
			s3ClientMock.Setup(x => x.GetObjectAsync(It.IsAny<GetObjectRequest>(), CancellationToken.None)).Callback((GetObjectRequest request, CancellationToken token) =>
			{
				if (request.VersionId == null || request.VersionId == expectedVersionId)
				{
					response.ResponseStream = new MemoryStream(new byte[] { 1, 2, 3, 4 });
					response.VersionId = expectedVersionId.IsNullOrEmpty() ? null : expectedVersionId;
				}
				else
				{
					throw new AmazonS3Exception("No Such Key") { ErrorCode = "NoSuchKey" };
				}
			}).Returns(Task.FromResult(response));

			return s3ClientMock;
		}

		#endregion
	}
}
