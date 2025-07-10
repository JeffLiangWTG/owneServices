using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Billing.Business;
using Enterprise.DocumentScanning.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Enterprise.DocumentScanning.Business
{
	public class AWSPersister : IExternalPersister
	{
		public AWSPersister() : this(null) { }

		public AWSPersister(AWSPersisterConfig config)
		{
			this.config = config ?? new AWSPersisterConfig();
		}

		protected readonly AWSPersisterConfig config;

		public bool AllowWrite
		{
			get
			{
				if (allowWrite == null)
				{
					var systemCodeInS3Bucket = GetSystemCodeFromS3Bucket();
					var shouldOverwriteLicenseCode = ShouldOverwriteLicenseCode(systemCodeInS3Bucket);
					if (systemCodeInS3Bucket.IsEmpty || shouldOverwriteLicenseCode)
					{
						WriteSystemCodeToS3Bucket();
						allowWrite = true;

						if (shouldOverwriteLicenseCode)
						{
							SendLicenseCodeOverwriteNotification(systemCodeInS3Bucket, S3ClaimSystemCode);
						}
					}
					else
					{
						allowWrite = systemCodeInS3Bucket.Equals(S3ClaimSystemCode);
					}
				}

				return allowWrite.Value;
			}
		}

		protected bool? allowWrite;

		enum ActionType
		{
			Upload,
			Download
		}

		public struct EDocsExternalStorageSize
		{
			public long CalculatedSize;

			public long S3BucketSize;

			public DateTime CalculateDateTimeUtc;

			public bool DoNotUseCalculatedSize;
		}

		(string name, object value)[] GetBucketPropertiesToReport(string key, ActionType actionType, long fileSize)
		{
			return new[]
			{
				(UsageProperties.DocumentS3TimeStamp, ZDateTime.UtcNow),
				(UsageProperties.DocumentS3BucketName, config.BucketName),
				(UsageProperties.DocumentS3PrimaryKey, key),
				(UsageProperties.DocumentS3Size, fileSize.ToString()),
				(UsageProperties.DocumentS3ActionType, (object)actionType.ToString())
			};
		}

		void GetBucketPropertiesAndReportToUsageCollector(string key, ActionType actionType, long fileSize)
		{
			if (!DocManagerRegistry.Instance.EnableS3UsageReport.Value)
			{
				return;
			}

			var properties = GetBucketPropertiesToReport(key, actionType, fileSize);
			ReportBucketPropertiesToUsageCollector(properties);
		}

		protected virtual void ReportBucketPropertiesToUsageCollector((string name, object value)[] properties)
		{
			UsageCollector.Report(UsageFeatures.Codes.DocumentS3Transfer, properties);
		}

		public Stream RetrieveStream(ZGuid primaryKey, ZString versionId)
		{
			var (stream, _) = RetrieveStreamCore(primaryKey.ToString(), versionId);
			return stream;
		}

		public (Stream stream, string versionId) RetrieveStream(ZGuid primaryKey)
		{
			return RetrieveStreamCore(primaryKey.ToString(), null);
		}

		string CombineFolderIfExists(string key)
		{
			var folderPath = config.FolderPath;
			return string.IsNullOrEmpty(folderPath) ? key : $"{folderPath}/{key}";
		}

		(Stream stream, string versionId) RetrieveStreamCore(string key, ZString? versionId, bool reportUsage = true)
		{
			try
			{
				var client = GetClient();
				var request = new GetObjectRequest
				{
					BucketName = config.BucketName,
					Key = CombineFolderIfExists(key),
					VersionId = versionId
				};

				var response = GetResultFromAsyncCall(client.GetObjectAsync(request));
				versionId = response.VersionId;

				if (response.ResponseStream.CanSeek)
				{
					if (reportUsage)
					{
						GetBucketPropertiesAndReportToUsageCollector(key, ActionType.Download, response.ResponseStream.Length);
					}
					return (response.ResponseStream, versionId);
				}

				using (var responseStream = response.ResponseStream)
				{
					var memoryStream = new MemoryStream();
					responseStream.CopyTo(memoryStream);
					memoryStream.Position = 0;

					if (reportUsage)
					{
						GetBucketPropertiesAndReportToUsageCollector(key, ActionType.Download, memoryStream.Length);
					}
					return (memoryStream, versionId);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				var persisterException = GetPersisterException(ex, key);

				if (persisterException != null)
				{
					throw persisterException;
				}

				throw;
			}
		}

#if DEBUG
		public
#else
		internal
#endif
		PutObjectRequest GetPutObjectRequest(Stream stream, string key)
		{
			var putRequest = new PutObjectRequest
			{
				BucketName = config.BucketName,
				Key = CombineFolderIfExists(key),
				InputStream = stream,
				UseChunkEncoding = config.UseChunkEncoding,
				StorageClass = config.AWSS3StorageClass,
				Headers = { ContentLength = stream.Length }
			};

			return putRequest;
		}

		public (bool isSaved, string versionId) SaveStream(Stream stream, ZGuid primaryKey)
		{
			return SaveStreamCore(stream, primaryKey.ToString());
		}

		(bool isSaved, string versionId) SaveStreamCore(Stream stream, string key, bool reportUsage = true)
		{
			try
			{
				var putRequest = GetPutObjectRequest(stream, key);
				var client = GetClient();

				if (config.WriteVerificationMode)
				{
					putRequest.MD5Digest = AmazonS3Util.GenerateMD5ChecksumForStream(stream);
				}

				var streamSize = stream.Length;
				var response = GetResultFromAsyncCall(client.PutObjectAsync(putRequest));
				if (reportUsage)
				{
					GetBucketPropertiesAndReportToUsageCollector(key, ActionType.Upload, streamSize);
				}

				return (response.HttpStatusCode == HttpStatusCode.OK, response.VersionId);
			}
			catch (AmazonS3Exception ex) when (ex.ErrorCode == AmazonS3ExceptionMD5DigestErrorCode)
			{
				return (false, null);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				var persisterException = GetPersisterException(ex, key);
				if (persisterException != null)
				{
					throw persisterException;
				}

				throw;
			}
		}

		const string AmazonS3ExceptionMD5DigestErrorCode = "InvalidDigest";

		public bool Delete(ZGuid primaryKey)
		{
			try
			{
				var client = GetClient();
				{
					var deleteRequest = new DeleteObjectRequest
					{
						BucketName = config.BucketName,
						Key = CombineFolderIfExists(primaryKey.ToString())
					};

					// If DeleteObject action is successful, the service sends back an HTTP 204 response with HttpStatusCode is NoContent. https://docs.aws.amazon.com/sdkfornet/v3/apidocs/items/S3/TS3Client.html
					var responseDelete = GetResultFromAsyncCall(client.DeleteObjectAsync(deleteRequest));
					return responseDelete.HttpStatusCode == HttpStatusCode.OK ||
							responseDelete.HttpStatusCode == HttpStatusCode.NotFound ||
							responseDelete.HttpStatusCode == HttpStatusCode.NoContent;
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				var persisterException = GetPersisterException(ex, primaryKey.ToString());
				if (persisterException != null)
				{
					throw persisterException;
				}

				throw;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Description String")]
		public long GetBucketSizeFromInternalAPI()
		{
			const string UrlTemplate = "https://s3billing.wtg.ws/bucket/{0}.{1}";
			const string BucketObjectName = "bucket";
			const string BucketSizePropertyName = "size_bytes";

			var serviceUrlString = config.ServiceURL?.Trim();
			var serviceUrlWithoutProtocol = Uri.TryCreate(serviceUrlString, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
				? serviceUrlString?.Substring(uri.GetLeftPart(UriPartial.Scheme).Length)
				: serviceUrlString;

			using (var client = GetHttpClient())
			{
				var endpointUrl = new Uri(string.Format(CultureInfo.InvariantCulture, UrlTemplate, config.BucketName, serviceUrlWithoutProtocol));
				var responseMessage = GetResultFromAsyncCall(client.GetAsync(endpointUrl));

				if (responseMessage.StatusCode == HttpStatusCode.NotFound)
				{
					return 0L;
				}

				if (responseMessage.StatusCode != HttpStatusCode.OK)
				{
					throw new BucketSizeWebException($"The request was failed to retrieve bucket size. Status Code: '{responseMessage.StatusCode:d}'");
				}

				var payload = GetResultFromAsyncCall(responseMessage.Content.ReadAsStringAsync());
				var payloadJObject = JObject.Parse(payload);
				var sizeToken = payloadJObject[BucketObjectName]?[BucketSizePropertyName];

				if (sizeToken == null)
				{
					ErrorReporter.ReportOnce("BucketSizeParseError", $"The bucket size metric is not returned in the payload with format '{BucketObjectName}:{BucketSizePropertyName}'. The returned json data is: {payload}");

					throw new BucketSizeWebException("There was an error while parsing the bucket size from the response.");
				}

				return sizeToken.Value<long>();
			}
		}

		public static long SizeInByteToSizeInMb(long size)
		{
			return (long)Math.Ceiling(size / 1024d / 1024d);
		}

		long GetCalculatedExternalStorageSize()
		{
			var sizeData = DocManagerRegistry.Instance.EDocsExternalStorageSize.Value;
			if (string.IsNullOrEmpty(sizeData))
			{
				throw new Exception("Failed to read external storage size data from database.");
			}

			if (sizeData.Contains(nameof(EDocsExternalStorageSize.CalculatedSize)))
			{
				var sizeObject = JsonConvert.DeserializeObject<EDocsExternalStorageSize>(sizeData);
				return sizeObject.DoNotUseCalculatedSize ? sizeObject.S3BucketSize : sizeObject.CalculatedSize;
			}
			else
			{
				var sizeObject = JsonConvert.DeserializeObject<Tuple<long, DateTime>>(sizeData);
				return sizeObject.Item1;
			}
		}

		public long GetBucketSizeInMb()
		{
			var bucketSize = DocManagerRegistry.Instance.UseCalculatedExternalStorageSize.Value ? GetCalculatedExternalStorageSize() : GetBucketSizeFromInternalAPI();
			return SizeInByteToSizeInMb(bucketSize);
		}

		protected virtual HttpClient GetHttpClient() => new HttpClient();

		public long GetObjectSize(ZGuid primaryKey)
		{
			try
			{
				return GetObjectMetadata(primaryKey).Headers.ContentLength;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				var persisterException = GetPersisterException(ex, primaryKey.ToString());

				if (persisterException != null)
				{
					if (persisterException is ExternalStorageObjectNotFoundException)
					{
						return 0L;
					}

					throw persisterException;
				}

				throw;
			}
		}

		public GetObjectMetadataResponse GetObjectMetadata(ZGuid primaryKey)
		{
			var client = GetClient();
			{
				var request = new GetObjectMetadataRequest()
				{
					BucketName = config.BucketName,
					Key = CombineFolderIfExists(primaryKey.ToString())
				};
				return GetResultFromAsyncCall(client.GetObjectMetadataAsync(request));
			}
		}

		protected virtual IAmazonS3 GetClient()
		{
			// only AWSS3Client required settings are used as key
			var clientCacheKey = config.ServiceURL + config.AccessKeyAndSecret + config.UsePathStyleAddressing + config.ConnectionTimeOut;
			if (CachedClients.TryGetValue(clientCacheKey, out var client))
			{
				return client;
			}

			client = CreateAmazonS3Client();
			if (!DoesS3BucketExist(client))
			{
				throw new ExternalStorageInvalidBucketNameException(
					$"S3 Bucket with name {config.BucketName} doesn't exist.",
					Core.Constants.EDocsStorageProviders.Code.S3,
					null);
			}

			CachedClients.Add(clientCacheKey, client);

			return client;
		}

		IAmazonS3 CreateAmazonS3Client()
		{
			var clientConfig = new AmazonS3Config();

			clientConfig.ServiceURL = config.ServiceURL;
			clientConfig.ForcePathStyle = config.UsePathStyleAddressing;

			var totalTimeout = config.ConnectionTimeOut;

			if (totalTimeout > 0)
			{
				clientConfig.Timeout = new TimeSpan(0, 0, totalTimeout);
			}

			// According to AWS documentation, the default ReadWriteTimeout is 5 minutes, but it is set to the max value for AmazonS3Client, unless it is explicitly set.
			// https://docs.aws.amazon.com/sdk-for-net/v3/developer-guide/retries-timeouts.html
			// In AmazonS3Config.Initialize(), it is set to max value (24 days) during construction
			// We need to reset it to the default 5 minutes to avoid hanging in DoesS3BucketExistV2() call to AWS in some situation (~1/millions)
#if NETFRAMEWORK
			clientConfig.ReadWriteTimeout = TimeSpan.FromMinutes(5);
#endif

			return new AmazonS3Client(config.AccessKeyId, config.SecretAccessKey, clientConfig);
		}

		T GetResultFromAsyncCall<T>(Task<T> task)
		{
			return task.ConfigureAwait(false).GetAwaiter().GetResult();
		}

		protected virtual bool DoesS3BucketExist(IAmazonS3 client) => GetResultFromAsyncCall(AmazonS3Util.DoesS3BucketExistV2Async(client, config.BucketName));

		[ThreadStatic]
		static Dictionary<string, IAmazonS3> cachedClients;
#if DEBUG
		public
#else
		internal
#endif
		static Dictionary<string, IAmazonS3> CachedClients
		{
			get => cachedClients ?? (cachedClients = new Dictionary<string, IAmazonS3>());
			set => cachedClients = value;
		}

		readonly string[] awsNotFoundErrorCodes = { "NoSuchKey", "NotFound", "NoSuchVersion" };

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Error Description")]
		Exception GetPersisterException(Exception ex, string key)
		{
			const string NetworkExceptionMessage = "There was a network issue while accessing the object.";
			const string GeneralExceptionMessage = "There was an error while accessing the document.";

			if (ex is ExternalStorageAccessException)
			{
				return ex;
			}

			if (ex is WebException)
			{
				return new ExternalStorageNetworkException(NetworkExceptionMessage, Core.Constants.EDocsStorageProviders.Code.S3, ex);
			}

			if (ex is AmazonS3Exception s3Exception)
			{
				if (s3Exception.ErrorCode.In(awsNotFoundErrorCodes))
				{
					return new ExternalStorageObjectNotFoundException(key, "Document doesn't exist in external storage.", Core.Constants.EDocsStorageProviders.Code.S3, s3Exception);
				}

				if (s3Exception.StatusCode == HttpStatusCode.BadGateway)
				{
					return new ExternalStorageNetworkException(NetworkExceptionMessage, Core.Constants.EDocsStorageProviders.Code.S3, ex);
				}

				if (s3Exception.ErrorCode == "InvalidBucketName" && s3Exception.StatusCode == HttpStatusCode.BadRequest)
				{
					return new ExternalStorageInvalidBucketNameException($"S3 bucket name '{config.BucketName}' is invalid.", Core.Constants.EDocsStorageProviders.Code.S3, ex);
				}

				return new ExternalStorageException(GeneralExceptionMessage + System.Environment.NewLine + ex.Message, Core.Constants.EDocsStorageProviders.Code.S3, ex);
			}

			if (ex is AmazonServiceException || ex is AmazonClientException)
			{
				return new ExternalStorageException(GeneralExceptionMessage + System.Environment.NewLine + ex.Message, Core.Constants.EDocsStorageProviders.Code.S3, ex);
			}

			if (ex is UriFormatException)
			{
				return new ExternalStorageUriFormatException($"S3 storage url '{config.ServiceURL}' is invalid format.", Core.Constants.EDocsStorageProviders.Code.S3, ex);
			}

			return null;
		}

		ZString GetSystemCodeFromS3Bucket()
		{
			var result = ZString.Empty;
			RetryAction(() =>
			{
				try
				{
					var (stream, _) = RetrieveStreamCore(S3BucektClaimFile, null, false);
					result = Encoding.UTF8.GetString(stream.ToByteArray());
				}
				catch (ExternalStorageObjectNotFoundException)
				{
					// Swallow the NotFound exception as this could be expected when the License file doesn't exist.
				}
			});

			return result;
		}

		void WriteSystemCodeToS3Bucket()
		{
			RetryAction(() =>
			{
				using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(S3ClaimSystemCode)))
				{
					var (isSaved, versionId) = SaveStreamCore(stream, S3BucektClaimFile, false);
					if (!isSaved)
					{
						throw new Exception("Failed to write system code to S3 Bucket.");
					}
				}
			});
		}

		string S3ClaimSystemCode
		{
			get
			{
				var key = ObjectFactory.Get<IProductRegistration>().Key;
				return key.EnterpriseCode + key.ServerCode + $",{key.DatabaseType}";
			}
		}

		void RetryAction(Action action)
		{
			var retries = 3;
			while (true)
			{
				try
				{
					action();
					break;
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					if (--retries == 0)
					{
						throw;
					}
					Thread.Sleep(1000);
				}
			}
		}

		bool ShouldOverwriteLicenseCode(string code)
		{
			// Allow production system to overwrite the system code uploaded from other system.
			var isProductionSystem = EnvProxy.Instance.IsProductionSystem;
			var isProductionSystemCode = code.EndsWith(DatabaseTypes.Codes.Production);
			return isProductionSystem && !isProductionSystemCode;
		}

		void SendLicenseCodeOverwriteNotification(string oldCode, string newCode)
		{
			// In extream edge case, if a bucket has been claimed by a non-prod system, and then is reclaiamed by a prod system,
			// we send a notification email to the hosted email address, so we can track it.
			var caption = Res.GetString("D70F5EC2-38BC-45E7-B22E-2ED96377C397", "S3 Bucket License file overwritten");
			var message = Res.GetString(
				"350C8B53-1CFF-4914-BD1D-441ED3F16F31",
				"The license file of S3 bucket {0} has been overwritten, system code is updated from {1} to {2}. This could happen if the bucket was used by non-production system, but now it's used by production system. Please verify the new system code matches to your production system code in the format of '<Enterprise Code><Server Code>,PRD'.",
				config.BucketName,
				oldCode,
				newCode);

			UnattendedUserNotification.Instance.ShowWarning(message, caption, sendToPostMaster: true);
		}

		const string S3BucektClaimFile = "License.txt";
	}
}
