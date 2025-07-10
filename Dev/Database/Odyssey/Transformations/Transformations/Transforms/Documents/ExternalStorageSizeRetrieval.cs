using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Amazon.S3;
using Amazon.S3.Model;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Documents
{
	public class ExternalStorageSizeRetrieval : DataTransformation
	{
		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			var startAfter = ExtProperty.Database.Select(MainDbConnection, START_AFTER_PROPERTY);
			var hasStartAfter = startAfter != null;

			if (!StorageDocsTableExists())
			{
				// If StorageDocs table doesn't exist, the transformation can be skipped.
				manager.ShowInfoMessage($"Skipping ExternalStorageSizeRetrieval transformation as the {StorageDocsDbName}.dbo.{StorageDocsTableName} table does not exist.");
				return;
			}

			var client = GetAmazonS3ClientFromRegistry();
			var bucketName = GetBucketNameFromRegistry();
			if (client is null || bucketName is null)
			{
				// If bucket details can't be retrieved, the transformation can be skipped.
				manager.ShowInfoMessage("Skipping ExternalStorageSizeRetrieval transformation as the S3 bucket details are missing or incomplete.");
				return;
			}

			var latestResponse = new ListObjectsV2Response { IsTruncated = true };
			var continuationToken = "";
			string updateStatement;
			while (!token.IsCancellationRequested && latestResponse.IsTruncated)
			{
				// Get next page of elements from S3 bucket
				if (string.IsNullOrEmpty(continuationToken) && hasStartAfter)
				{
					latestResponse = DoListRequest(client, bucketName, "", startAfter);
				}
				else
				{
					latestResponse = DoListRequest(client, bucketName, continuationToken);
				}

				// Execute an UPDATE statement that pushes the filesizes into the bucket
				using (var transactionManager = MainDbConnection.BeginTransactionWithManager())
				{
					if (latestResponse.KeyCount > 0)
					{
						foreach (var objectChunk in IEnumerableExtensions.Chunk(latestResponse.S3Objects, NUM_FILES_TO_UPDATE))
						{
							updateStatement = BuildUpdateStatement(objectChunk);
							MainDbConnection.ExecuteNonQuery(updateStatement);
						}
					}

					// Handle properties used for resuming operation
					if (latestResponse.IsTruncated)
					{
						continuationToken = latestResponse.NextContinuationToken;
						ExtProperty.Database.Update(MainDbConnection, START_AFTER_PROPERTY, latestResponse.S3Objects.Last().Key);
					}
					else
					{
						ExtProperty.Database.Delete(MainDbConnection, START_AFTER_PROPERTY);
					}

					transactionManager.CommitTransaction();
				}
			}
			token.ThrowIfCancellationRequested();
		}

		bool StorageDocsTableExists()
		{
			var query = $@"SELECT IIF(OBJECT_ID('{StorageDocsDbName}.dbo.{StorageDocsTableName}') IS NOT NULL, 1, 0)";
			return Convert.ToBoolean(MainDbConnection.Command(query).ExecuteScalar());
		}

		string GetStringFromRegistry(string itemName)
		{
			return Encoding.Unicode.GetString(RegistryHelper.GetStmDataValue(itemName) ?? Array.Empty<byte>());
		}

		string GetBucketNameFromRegistry()
		{
			var bucketName = GetStringFromRegistry("DocManagerStorageBucketName");
			if (string.IsNullOrEmpty(bucketName))
			{
				manager.ShowInfoMessage("DocManagerStorageBucketName registry item missing during ExternalStorageSizeRetrieval transformation.");
				return null;
			}
			return bucketName;
		}

		protected virtual AmazonS3Client GetAmazonS3ClientFromRegistry()
		{
			var keyAndSecret = GetStringFromRegistry("EDocsStorageAccess");
			if (string.IsNullOrEmpty(keyAndSecret))
			{
				manager.ShowInfoMessage("EDocsStorageAccess registry item missing during ExternalStorageSizeRetrieval transformation.");
				return null;
			}

			var splitKeyAndSecret = keyAndSecret.Split(';');
			if (!(splitKeyAndSecret.Length >= 2 && splitKeyAndSecret[0].StartsWith("KeyId=") && splitKeyAndSecret[1].StartsWith("Secret=")))
			{
				manager.ShowInfoMessage("EDocsStorageAccess registry item in invalid format during ExternalStorageSizeRetrieval transformation.");
				return null;
			}
			var keyId = splitKeyAndSecret[0].Remove(0, "KeyId=".Length);
			var secret = splitKeyAndSecret[1].Remove(0, "Secret=".Length);

			var url = GetStringFromRegistry("EDocsStorageServiceUrl");
			if (string.IsNullOrEmpty(url))
			{
				manager.ShowInfoMessage("EDocsStorageServiceUrl registry item missing during ExternalStorageSizeRetrieval transformation.");
				return null;
			}

			return new AmazonS3Client(keyId, secret, new AmazonS3Config()
			{
				ServiceURL = url,
				ForcePathStyle = true,
			});
		}

		public ListObjectsV2Response DoListRequest(AmazonS3Client client, string bucketName, string continuationToken = "", string startAfter = "")
		{
			var listObjectsRequest = new ListObjectsV2Request()
			{
				BucketName = bucketName,
				MaxKeys = NUM_FILES_TO_REQUEST,
			};

			if (!string.IsNullOrEmpty(continuationToken))
			{
				listObjectsRequest.ContinuationToken = continuationToken;
			}
			if (!string.IsNullOrEmpty(startAfter))
			{
				listObjectsRequest.StartAfter = startAfter;
			}

#if NETFRAMEWORK
			return client.ListObjectsV2(listObjectsRequest);
#else
			return client.ListObjectsV2Async(listObjectsRequest).Result;
#endif
		}

		string BuildUpdateStatement(IEnumerable<S3Object> files)
		{
			var sbCase = new StringBuilder();
			var sbWhere = new StringBuilder();
			foreach (var file in files)
			{
				// Only push in files with GUID names and positive, 32-bit file sizes
				if (Guid.TryParse(file.Key, out _) && file.Size >= 0 && file.Size <= int.MaxValue)
				{
					sbCase.Append($"WHEN '{file.Key}' THEN {file.Size}\n\t\t");
					sbWhere.Append($"'{file.Key}',\n\t");
				}
			}
			sbCase.Remove(sbCase.Length - 3, 3);
			sbWhere.Remove(sbWhere.Length - 3, 3);

			return $@"UPDATE {StorageDocsDbName}.dbo.{StorageDocsTableName}
SET SC_ExternalStorageSize =
	CASE SC_PK
		{sbCase}
		ELSE SC_ExternalStorageSize
	END
FROM {StorageDocsDbName}.dbo.{StorageDocsTableName} WITH(FORCESEEK, INDEX(PK_UX__SC_PK))
WHERE SC_PK IN (
	{sbWhere}
);";
		}

		public override string UserDescription => "Retrieve document sizes from the connected AWS bucket for the StorageDocs table";

		protected string StorageDocsDbName { get; set; } = $"{Db.DatabaseName}{Db.SDDatabaseAffix}001";
		protected string StorageDocsTableName { get; set; } = "StorageDocs";

		const int NUM_FILES_TO_REQUEST = 1000;
		const int NUM_FILES_TO_UPDATE = NUM_FILES_TO_REQUEST / 10;
		const string START_AFTER_PROPERTY = "ExternalStorageSize_StartAfter";
		DbConnection MainDbConnection => Db.Connection;
		readonly RegistryTransformationHelper RegistryHelper = new RegistryTransformationHelper();
	}
}
