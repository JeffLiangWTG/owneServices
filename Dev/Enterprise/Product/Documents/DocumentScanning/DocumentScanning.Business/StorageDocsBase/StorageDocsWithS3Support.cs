using System;
using System.Data;
using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentScanning.Business
{
	public abstract class StorageDocsWithS3Support : AutoStorageDocs
	{
		protected StorageDocsWithS3Support(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region External Storage Save and Retrieve With Compression and Encryption

		public bool SaveToExternalStorage()
		{
			IsMovingToExternalStorage = false;

			if (ExternalPersister != null && !IsDeleted)
			{
				return SaveBytesToExternalStorage(SC_ImageDataFromDb, ExternalPersister);
			}

			return IsMovingToExternalStorage;
		}

		public bool SaveBytesToExternalStorage(byte[] plainData, IExternalPersister externalPersister)
		{
			var compressedData = (byte[])ZCompressor.GetCompressedVersion(plainData, StorageDocsSchema.Constants.SC_ImageData);
			byte[] dataKey;
			if (HasDataKey)
			{
				// reuse current datakey if already exist
				dataKey = StorageDocsEncryptionHelper.DecryptDataKey(SC_EncryptedDataKey, StorageDocsMasterKeyProvider.GetMasterKey(SC_SCK_MasterKey).SCK_KeyValue);
			}
			else
			{
				dataKey = StorageDocsEncryptionHelper.NewDataKey();
			}

			var encryptedData = StorageDocsEncryptionHelper.Encrypt(compressedData, dataKey);

			using (var memoryStream = new MemoryStream(encryptedData))
			{
				var (isSaved, versionId) = externalPersister.SaveStream(memoryStream, PK);

				if (isSaved)
				{
					var currentMasterKey = StorageDocsMasterKeyProvider.GetCurrentMasterKey();
					if (!HasDataKey || SC_SCK_MasterKey != currentMasterKey.PK)
					{
						// Encrypt data key if it is not set or re-encrypt if SC_SCK_MasterKey is not current
						SC_EncryptedDataKey = StorageDocsEncryptionHelper.Encrypt(dataKey, currentMasterKey.SCK_KeyValue);
						SC_SCK_MasterKey = currentMasterKey.PK;
					}
					SC_ExternalStorageSize = encryptedData.Length;
					SC_UncompressedSize = plainData.Length;
					SC_VersionID = versionId;

					IsMovingToExternalStorage = true;

					return IsMovingToExternalStorage;
				}
			}

			return false;
		}

		public (byte[] content, int originalSize) RetrieveFromExternalStorage(IExternalPersister externalPersister, bool forceS3CryptoException = false)
		{
			using (var retrievedStream = RetrieveFromExternalStorageCore(externalPersister))
			{
				var retrievedData = retrievedStream.ToByteArray();
				var originalSize = retrievedData.Length;
				if (HasDataKey)
				{
					var dataKey = StorageDocsEncryptionHelper.DecryptDataKey(SC_EncryptedDataKey, StorageDocsMasterKeyProvider.GetMasterKey(SC_SCK_MasterKey).SCK_KeyValue);

					try
					{
						retrievedData = StorageDocsEncryptionHelper.DecryptDocument(retrievedData, dataKey, PK.ToGuid());
					}
					catch (S3CryptoException ex)
					{
						if (forceS3CryptoException)
						{
							throw;
						}

						// We retrieve and try again in case the data was corrupted during the first download
						using var retrievedStream2 = RetrieveFromExternalStorageCore(externalPersister);
						var retrievedData2 = retrievedStream2.ToByteArray();

						if (retrievedData2.SequenceEqual(retrievedData))
						{
							// If the second download return the same data as the first, we have a real decryption issue!
							ErrorReporter.ReportOnce(nameof(S3CryptoException), $"Cannot decrypt document pk: {PK}.", ex);
							throw;
						}

						try
						{
							retrievedData = StorageDocsEncryptionHelper.DecryptDocument(retrievedData2, dataKey, PK.ToGuid());
						}
						catch (S3CryptoException ex2)
						{
							if (retrievedData2.Length != externalPersister.GetObjectSize(PK))
							{
								throw new ExternalStorageNetworkException("There was a network issue while accessing the document, please try again later.", Core.Constants.EDocsStorageProviders.Code.S3, null);
							}
							else
							{
								// If the second download can't be decrypted and the size of the download are same in the bucket, we have a real issue!
								ErrorReporter.ReportOnce(nameof(S3CryptoException), $"Cannot decrypt document pk: {PK}.", ex2);
								throw;
							}
						}
					}
				}

				return ((byte[])ZCompressor.GetUncompressedVersion(retrievedData, StorageDocsSchema.Constants.SC_ImageData),
						originalSize);
			}
		}

		Stream RetrieveFromExternalStorageCore(IExternalPersister externalPersister)
		{
			var useVersionID = SystemDataRegistry.Instance.UseVersionID.Value;
			if (useVersionID && !SC_VersionID.IsEmpty)
			{
				return externalPersister.RetrieveStream(PK, SC_VersionID);
			}
			else
			{
				try
				{
					var (stream, versionId) = externalPersister.RetrieveStream(PK);
					if (useVersionID && !string.IsNullOrEmpty(versionId) && !versionId.Equals(NullVersionId))
					{
						using (SuspendSettingHasChanges())
						{
							SC_VersionID = versionId;
						}
					}
					return stream;
				}
				catch (ExternalStorageObjectNotFoundException) when (SC_UncompressedSize == 0)
				{
					// Original document is empty
					return new MemoryStream();
				}
			}
		}

		static readonly string NullVersionId = "null";

		internal bool HasDataKey => !SC_EncryptedDataKey.IsEmpty && !SC_SCK_MasterKey.IsEmpty;

		IExternalPersister externalPersister;
		string externalPersisterType;

		public IExternalPersister ExternalPersister
		{
			get
			{
				if (externalPersister == null || externalPersisterType != SystemDataRegistry.Instance.EDocsStorageProvider.Value)
				{
					externalPersisterType = SystemDataRegistry.Instance.EDocsStorageProvider.Value;
					externalPersister = ObjectFactory.Get<IExternalPersisterProvider>().GetExternalPersister(externalPersisterType);
				}

				return externalPersister;
			}
		}

		#endregion

		public override ZBlob SC_ImageData
		{
			get
			{
				if (SC_ImageDataFromDb.IsEmpty && IsInDatabase && ExternalPersister != null)
				{
					return GetImageDataFromExternalStorage(ExternalPersister);
				}

				return SC_ImageDataFromDb;
			}
			set
			{
				hasSC_ImageDataBeenUpdated = SetPropertyValue(SC_ImageDataInfo, value) && !IsMovingToExternalStorage;

				if (!IsValidationSuspended)
				{
					Validation.ValidateSC_ImageData();
				}

				if (!value.IsEmpty)
				{
					UpdateUncompressedSizeWithSuspendingSettingHasChanges(value.Length);
				}
			}
		}

		public bool HasSC_ImageDataBeenUpdated => hasSC_ImageDataBeenUpdated;
		protected bool hasSC_ImageDataBeenUpdated;

		protected void UpdateUncompressedSizeWithSuspendingSettingHasChanges(int size)
		{
			using (SuspendSettingHasChanges())
			{
				SC_UncompressedSize = size;
			}
		}

		public ZBlob SC_ImageDataFromDb => base.SC_ImageData;

		public ZBlob GetImageDataFromExternalStorage(IExternalPersister externalPersister)
		{
			if (!currentExternalImageData.HasValue)
			{
				(currentExternalImageData, _) = RetrieveFromExternalStorage(externalPersister);
			}
			return currentExternalImageData.Value;
		}

		public override Stream GetSC_ImageDataReader()
		{
			var stream = GetSC_ImageDataReaderDirect();

			if (IsInDatabase)
			{
				// Currently three types of stream can be returned from GetSC_ImageDataReader
				// 1. MemoryStream, if the binary data is already loaded and no lazy-loading
				// 2. DeflateStream, if the data is compressed in the db
				// 3. SqlBinaryFieldStream, if data is not compressed
				// SqlBinaryFiledStream contains one open DataReader.
				if (!stream.CanSeek || stream is SqlBinaryFieldStream)
				{
					var memoryWrapper = new MemoryStream();
					stream.CopyTo(memoryWrapper);
					stream.Dispose();
					memoryWrapper.Position = 0;
					stream = memoryWrapper;
				}

				if (stream.Length == 0)
				{
					var externalStoragePersister = ExternalPersister;

					if (externalStoragePersister != null)
					{
						stream.Dispose();
						// We retrieve the data from external storage without using cache to keep the same behaviour as GetSC_ImageDataReaderDirect()
						var (retrievedData, _) = RetrieveFromExternalStorage(externalStoragePersister);
						stream = new MemoryStream(retrievedData);
						currentExternalImageData = retrievedData;
					}
				}
			}

			return stream;
		}

		public Stream GetSC_ImageDataReaderDirect()
		{
			return CreateNewFactory().GetBinaryFieldStream(this, StorageDocsSchema.SC_ImageData);
		}

		public override void Delete()
		{
			AddStorageDocsToDeleteQueueIfNeeded();
			base.Delete();
		}

		protected virtual bool IsOrphan() => false;

		void AddStorageDocsToDeleteQueueIfNeeded()
		{
			if (ExternalPersister != null && !IsOrphan())
			{
				var toDelete = Factory.New<StorageDocsToDelete>();
				toDelete.SCD_StorageDocIdentifier = PK;
			}
		}

		public override bool HasChanges
		{
			get => !IsMovingToExternalStorage && base.HasChanges;
		}

		public override bool ShouldSkipUpdateAuditColumns
		{
			get => IsInDatabase && !HasChanges;
		}

		public bool IsMovingToExternalStorage { get; private set; }

		internal ZBlob? currentExternalImageData;
	}
}
