using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.S3;
using CargoWise.Common;
using Enterprise.Registry.Business;

namespace Enterprise.DocumentScanning.Business
{
	public class AWSPersisterConfig
	{
		public AWSPersisterConfig() { }

		public AWSPersisterConfig(string bucketName, string serviceURL, string accessKeyAndSecret, string aWSS3StorageClass)
		{
			serviceURLOverride = serviceURL;
			bucketNameOverride = bucketName;
			accessKeyAndSecretOverride = accessKeyAndSecret;
			aWSS3StorageClassOverride = aWSS3StorageClass;
		}

		public string BucketName
		{
			get
			{
				var bucketName = GetBucketNameAndFolder().bucket;
				if (bucketName.IsNullOrEmpty())
				{
					throw new ExternalStorageAccessException(string.Empty, "Bucket Name can not be empty.", SystemDataRegistry.Instance.EDocsStorageProvider.Value, null);
				}
				return bucketName;
			}
		}

		readonly string bucketNameOverride;

		public string FolderPath => GetBucketNameAndFolder().path;

		public string ServiceURL
		{
			get
			{
				var serviceURL = serviceURLOverride ?? SystemDataRegistry.Instance.EDocsStorageServiceUrl.Value.Trim();
				if (serviceURL.IsNullOrEmpty())
				{
					throw new ExternalStorageAccessException(string.Empty, "Service URL can not be empty.", SystemDataRegistry.Instance.EDocsStorageProvider.Value, null);
				}
				return serviceURL;
			}
		}

		readonly string serviceURLOverride;

		public string AccessKeyAndSecret => accessKeyAndSecretOverride ?? SystemDataRegistry.Instance.EDocsStorageAccess.Value;

		readonly string accessKeyAndSecretOverride;

		public S3StorageClass AWSS3StorageClass
		{
			get
			{
				var storageClassStringValue = aWSS3StorageClassOverride ?? SystemDataRegistry.Instance.AWSS3StorageClass.Value;
				return storageClassStringValue.IsNullOrEmpty() ? null : S3StorageClass.FindValue(storageClassStringValue);
			}
		}

		readonly string aWSS3StorageClassOverride;

		public int ConnectionTimeOut => SystemDataRegistry.Instance.EDocsStorageConnectionTimeout.Value;

		public bool UseChunkEncoding => SystemDataRegistry.Instance.UseChunkEncoding.Value;

		public bool UsePathStyleAddressing => SystemDataRegistry.Instance.UsePathStyleAddressing.Value;

		public bool WriteVerificationMode => SystemDataRegistry.Instance.DocManagerParanoidModeEnabled.Value;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Description String")]
		void GetKeyAndSecret()
		{
			const string TagAccessKey = "KeyId";
			const string TagSecretKey = "Secret";
			const char TagSplitter = ';';
			const char KeyValueSplitter = '=';

			if (string.IsNullOrWhiteSpace(AccessKeyAndSecret))
			{
				throw new ExternalStorageAccessException(string.Empty, "Access credentials were not provided.", SystemDataRegistry.Instance.EDocsStorageProvider.Value, null);
			}

			var credentials = AccessKeyAndSecret.Split(TagSplitter).Select(x =>
			{
				var keyValue = x.Trim().Split(KeyValueSplitter);
				return new KeyValuePair<string, string>(keyValue.First().Trim(), keyValue.Last().Trim());
			});

			var accessKeyId = credentials.FirstOrDefault(x => x.Key.Equals(TagAccessKey, StringComparison.OrdinalIgnoreCase));
			var secretAccessKey = credentials.FirstOrDefault(x => x.Key.Equals(TagSecretKey, StringComparison.OrdinalIgnoreCase));

			if (string.IsNullOrEmpty(accessKeyId.Value) || string.IsNullOrEmpty(secretAccessKey.Value))
			{
				throw new ExternalStorageAccessException(string.Empty, "Access credentials were invalid format.", SystemDataRegistry.Instance.EDocsStorageProvider.Value, null);
			}

			this.accessKeyId = accessKeyId.Value;
			this.secretAccessKey = secretAccessKey.Value;
		}

		(string bucket, string path) GetBucketNameAndFolder()
		{
			var bucketName = bucketNameOverride ?? SystemDataRegistry.Instance.DocManagerStorageBucketName.Value.Trim();
			var indexOfFirstSlash = bucketName.IndexOf('/');
			if (indexOfFirstSlash == -1)
			{
				return (bucketName, string.Empty);
			}

			return (bucketName.Substring(0, indexOfFirstSlash), bucketName.Substring(indexOfFirstSlash + 1));
		}

		public string AccessKeyId
		{
			get
			{
				if (accessKeyId == null)
				{
					GetKeyAndSecret();
				}
				return accessKeyId;
			}
		}
		string accessKeyId;

		public string SecretAccessKey
		{
			get
			{
				if (secretAccessKey == null)
				{
					GetKeyAndSecret();
				}
				return secretAccessKey;
			}
		}
		string secretAccessKey;
	}
}
