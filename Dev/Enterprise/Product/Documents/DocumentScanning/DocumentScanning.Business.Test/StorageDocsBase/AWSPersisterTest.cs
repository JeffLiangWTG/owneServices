using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Billing.Business;
using Enterprise.Billing.Business.Testing;
using Enterprise.DocumentScanning.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.MailManager.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.Test
{
	sealed class AWSPersisterTest : TransactionedTestCase
	{
		public void TestCachedClient()
		{
			SystemDataRegistry.Instance.DocManagerStorageBucketName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "testbucket101");
			SystemDataRegistry.Instance.EDocsStorageServiceUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://s3.test.url.com");
			SystemDataRegistry.Instance.EDocsStorageAccess.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "KeyId=KKKJJJMMM111000111;Secret=7h15115453cR3+");
			SystemDataRegistry.Instance.EDocsStorageConnectionTimeout.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 56);
			SystemDataRegistry.Instance.UseChunkEncoding.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			SystemDataRegistry.Instance.UsePathStyleAddressing.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			SystemDataRegistry.Instance.DocManagerParanoidModeEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			SystemDataRegistry.Instance.AWSS3StorageClass.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");

			var persister = new AWSPersisterForTest() { DoNotMock = true };
			var client1 = persister.GetBaseClient();
			persister = new AWSPersisterForTest() { DoNotMock = true };
			var client2 = persister.GetBaseClient();
			AssertEquals("client1 and client2 should use the same cache", client1, client2);

			SystemDataRegistry.Instance.EDocsStorageServiceUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://s3.test2.url.com");
			persister = new AWSPersisterForTest() { DoNotMock = true };
			client2 = persister.GetBaseClient();
			AssertNotEquals("client1 and client2 should use different cache after ServiceURL registry changed", client1, client2);

			// Override URL and ignore BucketName
			var config = new AWSPersisterConfig("testbucket101", "https://overrideURL.wtg.zone", "KeyId=KKKJJJMMM111000111;Secret=7h15115453cR3+", "");
			persister = new AWSPersisterForTest(config) { DoNotMock = true };
			var client3 = persister.GetBaseClient();

			config = new AWSPersisterConfig("testbucket202", "https://overrideURL.wtg.zone", "KeyId=KKKJJJMMM111000111;Secret=7h15115453cR3+", "GLACIER_IR");
			persister = new AWSPersisterForTest(config) { DoNotMock = true };
			var client4 = persister.GetBaseClient();

			AssertEquals("client3 and client4 should use the same cache as their URL and KeySecret are the same", client3, client4);
			AssertNotEquals("client3 and client2 should use different cache as their URL and KeySecret are different", client2, client3);

			// Override Secret
			config = new AWSPersisterConfig("testbucket101", "https://overrideURL.wtg.zone", "KeyId=overrideKeyId;Secret=overrideSecret", "");
			persister = new AWSPersisterForTest(config) { DoNotMock = true };
			client3 = persister.GetBaseClient();

			config = new AWSPersisterConfig("testbucket308", "https://overrideURL.wtg.zone", "KeyId=overrideKeyId;Secret=overrideSecret", "GLACIER");
			persister = new AWSPersisterForTest(config) { DoNotMock = true };
			client4 = persister.GetBaseClient();

			AssertEquals("client3 and client4 should use the same cache as their URL and KeySecret are the same", client3, client4);
			AssertNotEquals("client3 and client2 should use different cache as their URL and KeySecret are different", client2, client3);

			// No override
			config = new AWSPersisterConfig();
			persister = new AWSPersisterForTest(config) { DoNotMock = true };
			var client5 = persister.GetBaseClient();
			AssertEquals("client5 should use the same cache as client2 when no override in config", client2, client5);

			// PathStyleEncoding
			SystemDataRegistry.Instance.UsePathStyleAddressing.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			persister = new AWSPersisterForTest(config) { DoNotMock = true };
			var client6 = persister.GetBaseClient();
			AssertNotEquals("client6 should a different cache after PathStyleEncoding changed", client2, client6);

			//Timeout
			SystemDataRegistry.Instance.EDocsStorageConnectionTimeout.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 57);
			persister = new AWSPersisterForTest(config) { DoNotMock = true };
			var client7 = persister.GetBaseClient();
			AssertNotEquals("client7 should a different cache after ConnectionTimeout changed", client6, client7);
		}

		public void TestCachedClientMultithreads()
		{
			var persister = new AWSPersisterForTest() { DoNotMock = true };
			var client1 = persister.GetBaseClient();
			var client2 = persister.GetBaseClient();
			AssertEquals("client1 and client2 should use the same cache", client1, client2);

			var secondThread = new Thread(() =>
			{
				var clientX = persister.GetBaseClient();
				AssertNotEquals("clientX on a different thread should be using a different cache", client1, clientX);
			});

			secondThread.Start();
			secondThread.Join();
		}

		public void TestAccessCacheFromDifferentThread()
		{
			var requestingThread = new Thread(() =>
			{
				var persister = new AWSPersister();
				AssertNotNull("Access cachedClient from second thread", AWSPersister.CachedClients);
			});

			requestingThread.Start();
			requestingThread.Join();

			AssertNotNull("Access cachedClient from main thread again", AWSPersister.CachedClients);
		}

		public void TestAWSPersisterWithConfig()
		{
			var data = new byte[] { 0, 1, 2, 3 };
			var ms = new MemoryStream(data);
			var pk = Guid.NewGuid();
			var persister = new AWSPersisterForTest() { DoNotMock = true };
			var putRequest = persister.GetPutObjectRequest(ms, pk.ToString());
			persister.ClearCachedClients();
			var client = persister.GetBaseClient();

			// PutRequest general
			AssertEquals("InputStream", ms, putRequest.InputStream);
			AssertEquals("InputStream position", 0, putRequest.InputStream.Position);
			AssertEquals("Key", pk.ToString(), putRequest.Key);

			// Default registry settings
			AssertEquals("StorageClass-default", null, putRequest.StorageClass);
			AssertEquals("BucketName-default/initial", "test", putRequest.BucketName);
			AssertEquals("UseChunkEncoding-default", true, putRequest.UseChunkEncoding);
			AssertEquals("ServiceURL", "https://s3.test.local/", client.Config.ServiceURL);
			AssertEquals("ConnectionTimeout", TimeSpan.FromSeconds(30), client.Config.Timeout);

			// New settings from registry
			SystemDataRegistry.Instance.AWSS3StorageClass.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "GLACIER_IR");
			SystemDataRegistry.Instance.DocManagerStorageBucketName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "superdupper");
			SystemDataRegistry.Instance.UseChunkEncoding.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			SystemDataRegistry.Instance.EDocsStorageServiceUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://s3.test.url.com");
			SystemDataRegistry.Instance.EDocsStorageConnectionTimeout.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 56);

			//AWSPersister.cachedClient = null;
			putRequest = persister.GetPutObjectRequest(new MemoryStream(), Guid.NewGuid().ToString());
			client = persister.GetBaseClient();
			AssertEquals("StorageClass", S3StorageClass.GlacierInstantRetrieval, putRequest.StorageClass);
			AssertEquals("BucketName", "superdupper", putRequest.BucketName);
			AssertEquals("UseChunkEncoding", false, putRequest.UseChunkEncoding);
			AssertEquals("ServiceURL", "https://s3.test.url.com/", client.Config.ServiceURL);
			AssertEquals("ConnectionTimeout", TimeSpan.FromSeconds(56), client.Config.Timeout);

			// Override with AWSPersisterConfig-with URL
			var config = new AWSPersisterConfig("overrideBucket", "http://overrideURL.s3", "KeyId=overrideKeyId;Secret=overrideSecret", "DEEP_ARCHIVE");
			persister = new AWSPersisterForTest(config);
			putRequest = persister.GetPutObjectRequest(new MemoryStream(), Guid.NewGuid().ToString());
			client = persister.GetBaseClient();
			AssertEquals("StorageClass", S3StorageClass.DeepArchive, putRequest.StorageClass);
			AssertEquals("BucketName", "overrideBucket", putRequest.BucketName);
			AssertEquals("ServiceURL", "http://overrideURL.s3/", client.Config.ServiceURL);
		}

		public void TestInvalidEDocsStorageServiceUrlShouldThrowExternalStorageException()
		{
			using (SystemDataRegistry.Instance.EDocsStorageAccess.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "KeyId=testkeyid;Secret=testsecret"))
			using (SystemDataRegistry.Instance.EDocsStorageServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "InlivadUrl"))
			{
				var awsPersister = new AWSPersister();
				var exception = AssertExceptionThrown<ExternalStorageException>(() => awsPersister.SaveStream(new MemoryStream(), ZGuid.NewZGuid()));
			}
		}

		public void TestInvalidPrefixEDocsStorageServiceUrlShouldThrowExternalStorageException()
		{
			using (SystemDataRegistry.Instance.EDocsStorageAccess.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "KeyId=testkeyid;Secret=testsecret"))
			using (SystemDataRegistry.Instance.EDocsStorageServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "htttp://valid.url"))
			{
				var awsPersister = new AWSPersister();
				var exception = AssertExceptionThrown<ExternalStorageException>(() => awsPersister.SaveStream(new MemoryStream(), ZGuid.NewZGuid()));
			}
		}

		public void TestGetBucketSize()
		{
			// Arrange
			DocManagerRegistry.Instance.UseCalculatedExternalStorageSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
			httpMessageHandlerMock
				.Protected()
				.Setup<Task<HttpResponseMessage>>(
					"SendAsync",
					ItExpr.IsAny<HttpRequestMessage>(),
					ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(new HttpResponseMessage
				{
					StatusCode = HttpStatusCode.OK,
					Content = new StringContent(@"{
""bucket"": {
					""bucket_name"": ""test_bucket"",
					""object_count"": 1000,
					""size_bytes"": 1000
}
}")
				});

			var httpClient = new HttpClient(httpMessageHandlerMock.Object);
			var testAWSPersisterMock = new Mock<AWSPersisterForTest>(false, true);
			testAWSPersisterMock.Protected().Setup<HttpClient>("GetHttpClient").Returns(httpClient);

			// Act
			var size = testAWSPersisterMock.Object.GetBucketSizeInMb();

			// Assert
			AssertEquals("Bucket size is 1 MB", 1, size);
		}

		public void TestGetBucketSize_BucketNameNotFoundAndDoesNotExist()
		{
			// Arrange
			DocManagerRegistry.Instance.UseCalculatedExternalStorageSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
			httpMessageHandlerMock
				.Protected()
				.Setup<Task<HttpResponseMessage>>(
					"SendAsync",
					ItExpr.IsAny<HttpRequestMessage>(),
					ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(new HttpResponseMessage
				{
					StatusCode = HttpStatusCode.NotFound,
					Content = new StringContent(@"The specified bucket cannot be found")
				});

			var httpClient = new HttpClient(httpMessageHandlerMock.Object);
			var testAWSPersisterMock = new Mock<AWSPersisterForTest>(false, true);
			testAWSPersisterMock.Protected().Setup<HttpClient>("GetHttpClient").Returns(httpClient);
			testAWSPersisterMock.Protected().Setup<bool>("DoesS3BucketExist", ItExpr.IsAny<IAmazonS3>()).Returns(false);

			using (SystemDataRegistry.Instance.DocManagerStorageBucketName.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "test_bucket"))
			using (SystemDataRegistry.Instance.EDocsStorageServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "test_bucket.url"))
			{
				// Act
				var size = testAWSPersisterMock.Object.GetBucketSizeInMb();

				// Assert
				AssertEquals("Bucket size is 0 MB", 0, size);
			}
		}

		public void TestGetBucketSize_BucketNameNotFoundAndDoesExistAlready()
		{
			// Arrange
			DocManagerRegistry.Instance.UseCalculatedExternalStorageSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
			httpMessageHandlerMock
				.Protected()
				.Setup<Task<HttpResponseMessage>>(
					"SendAsync",
					ItExpr.IsAny<HttpRequestMessage>(),
					ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(new HttpResponseMessage
				{
					StatusCode = HttpStatusCode.NotFound,
					Content = new StringContent(@"The specified bucket cannot be found")
				});

			var httpClient = new HttpClient(httpMessageHandlerMock.Object);
			var testAWSPersisterMock = new Mock<AWSPersisterForTest>(false, true);
			testAWSPersisterMock.Protected().Setup<HttpClient>("GetHttpClient").Returns(httpClient);
			testAWSPersisterMock.Protected().Setup<bool>("DoesS3BucketExist", ItExpr.IsAny<IAmazonS3>()).Returns(true);

			using (SystemDataRegistry.Instance.DocManagerStorageBucketName.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "test_bucket"))
			using (SystemDataRegistry.Instance.EDocsStorageServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "test_bucket.url"))
			{
				// Act
				AssertEquals(0, testAWSPersisterMock.Object.GetBucketSizeInMb());
			}
		}

		public void TestGetBucketSize_HttpOrHttpsInServiceUrlShouldBeTrimmed()
		{
			// Arrange
			DocManagerRegistry.Instance.UseCalculatedExternalStorageSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var trimmedUri = string.Empty;
			var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
			httpMessageHandlerMock
				.Protected()
				.Setup<Task<HttpResponseMessage>>(
					"SendAsync",
					ItExpr.IsAny<HttpRequestMessage>(),
					ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(new HttpResponseMessage
				{
					StatusCode = HttpStatusCode.OK,
					Content = new StringContent(@"{
""bucket"": {
					""bucket_name"": ""test_bucket"",
					""object_count"": 1000,
					""size_bytes"": 1000
}
}")
				}).Callback<HttpRequestMessage, CancellationToken>((m, n) => trimmedUri = m.RequestUri.ToString());
			var testAWSPersisterMock = new Mock<AWSPersisterForTest>(false, true);
			testAWSPersisterMock.Protected().Setup<bool>("DoesS3BucketExist", ItExpr.IsAny<IAmazonS3>()).Returns(true);

			using (SystemDataRegistry.Instance.DocManagerStorageBucketName.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "invalid-bucket"))
			{
				using (var httpClient = new HttpClient(httpMessageHandlerMock.Object))
				using (SystemDataRegistry.Instance.EDocsStorageServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://invalid.url"))
				{
					// Prepare
					testAWSPersisterMock.Protected().Setup<HttpClient>("GetHttpClient").Returns(httpClient);

					// Act
					testAWSPersisterMock.Object.GetBucketSizeInMb();

					// Assert
					AssertEquals("https://s3billing.wtg.ws/bucket/invalid-bucket.invalid.url", trimmedUri);
				}

				using (var httpClient = new HttpClient(httpMessageHandlerMock.Object))
				using (SystemDataRegistry.Instance.EDocsStorageServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://invalid.url"))
				{
					// Prepare
					testAWSPersisterMock.Protected().Setup<HttpClient>("GetHttpClient").Returns(httpClient);
					trimmedUri = string.Empty;

					// Act
					testAWSPersisterMock.Object.GetBucketSizeInMb();

					// Assert
					AssertEquals("https://s3billing.wtg.ws/bucket/invalid-bucket.invalid.url", trimmedUri);
				}

				using (var httpClient = new HttpClient(httpMessageHandlerMock.Object))
				using (SystemDataRegistry.Instance.EDocsStorageServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "anyschema://invalid.url"))
				{
					// Prepare
					testAWSPersisterMock.Protected().Setup<HttpClient>("GetHttpClient").Returns(httpClient);
					trimmedUri = string.Empty;

					// Act
					testAWSPersisterMock.Object.GetBucketSizeInMb();

					// Assert
					AssertEquals("https://s3billing.wtg.ws/bucket/invalid-bucket.anyschema://invalid.url", trimmedUri);
				}
			}
		}

		public void TestGetBucketSize_NetworkIssue()
		{
			// Arrange
			DocManagerRegistry.Instance.UseCalculatedExternalStorageSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
			httpMessageHandlerMock
				.Protected()
				.Setup<Task<HttpResponseMessage>>(
					"SendAsync",
					ItExpr.IsAny<HttpRequestMessage>(),
					ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(new HttpResponseMessage
				{
					StatusCode = HttpStatusCode.GatewayTimeout,
					Content = new StringContent(@"Timeout")
				});

			var httpClient = new HttpClient(httpMessageHandlerMock.Object);
			var testAWSPersisterMock = new Mock<AWSPersisterForTest>(false, true);
			testAWSPersisterMock.Protected().Setup<HttpClient>("GetHttpClient").Returns(httpClient);

			// Act
			var exception = AssertExceptionThrown<BucketSizeWebException>(() => testAWSPersisterMock.Object.GetBucketSizeInMb());

			// Assert
			AssertEquals("The request was failed to retrieve bucket size. Status Code: '504'", exception.Message);
		}

		public void TestGetBucketSize_ParseErrorShouldBeReported()
		{
			// Arrange
			DocManagerRegistry.Instance.UseCalculatedExternalStorageSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
			const string Content = @"{
""wrong_bucket"": {
					""bucket_name"": ""test_bucket"",
					""object_count"": 1000,
					""size"": 1000
}
}";
			httpMessageHandlerMock
				.Protected()
				.Setup<Task<HttpResponseMessage>>(
					"SendAsync",
					ItExpr.IsAny<HttpRequestMessage>(),
					ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(new HttpResponseMessage
				{
					StatusCode = HttpStatusCode.OK,
					Content = new StringContent(Content)
				});

			var httpClient = new HttpClient(httpMessageHandlerMock.Object);
			var testAWSPersisterMock = new Mock<AWSPersisterForTest>(false, true);
			testAWSPersisterMock.Protected().Setup<HttpClient>("GetHttpClient").Returns(httpClient);
			ErrorReporter.Clear();

			// Act
			var exception = AssertExceptionThrown<BucketSizeWebException>(() => testAWSPersisterMock.Object.GetBucketSizeInMb());

			// Assert
			AssertEquals("BucketSizeParseError", ErrorReporter.LastKeyReported);
			AssertEquals($"The bucket size metric is not returned in the payload with format 'bucket:size_bytes'. The returned json data is: {Content}", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();

			AssertEquals("There was an error while parsing the bucket size from the response.", exception.Message);
		}

		public void TestGetCalculatedExternalStorageSize()
		{
			using (DocManagerRegistry.Instance.UseCalculatedExternalStorageSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				// Arrange
				var testAwsPersister = new AWSPersisterForTest();
				var jsonStr = JsonConvert.SerializeObject(new Tuple<long, DateTime>(2 * 1024 * 1024 + 1, ZDateTime.UtcNow.ToDateTime()));
				DocManagerRegistry.Instance.EDocsExternalStorageSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, jsonStr);

				// Act
				var size = testAwsPersister.GetBucketSizeInMb();

				// Assert
				AssertEquals("Bucket size is 3 MB", 3, size);
			}
		}

		public void TestGetCalculatedExternalStorageSizeNewFormat()
		{
			using (DocManagerRegistry.Instance.UseCalculatedExternalStorageSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				// Arrange
				var testAwsPersister = new AWSPersisterForTest();
				var jsonStr = JsonConvert.SerializeObject(
					new AWSPersister.EDocsExternalStorageSize
					{
						CalculatedSize = 2 * 1024 * 1024 + 1,
						CalculateDateTimeUtc = ZDateTime.UtcNow.ToDateTime()
					});
				DocManagerRegistry.Instance.EDocsExternalStorageSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, jsonStr);

				// Act
				var size = testAwsPersister.GetBucketSizeInMb();

				// Assert
				AssertEquals("Bucket size is 3 MB", 3, size);
			}
		}

		public void TestGetCalculatedExternalStorageSize_SizeDataNotExist()
		{
			using (DocManagerRegistry.Instance.UseCalculatedExternalStorageSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				// Arrange
				DocManagerRegistry.Instance.EDocsExternalStorageSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);

				// Act
				var testAwsPersister = new AWSPersisterForTest();
				AssertExceptionThrown<Exception>("Throw exception if no size data in database", "Failed to read external storage size data from database.", () => testAwsPersister.GetBucketSizeInMb());
			}
		}

		public void TestGetClientWhenEDocsStorageAccessIsInvalid()
		{
			using (SystemDataRegistry.Instance.EDocsStorageAccess.DataType.SuspendValidation())
			{
				using (SystemDataRegistry.Instance.EDocsStorageAccess.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "KeyId=testid,Secret=blabla"))
				{
					var testClient = new AWSPersisterForTest();
					AssertExceptionThrown<ExternalStorageAccessException>("semicolon", () => testClient.GetBaseClient());
				}

				using (SystemDataRegistry.Instance.EDocsStorageAccess.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "KeyId:testid;Secret:blabla"))
				{
					var testClient = new AWSPersisterForTest();
					AssertExceptionThrown<ExternalStorageAccessException>("key value splitter", () => testClient.GetBaseClient());
				}

				using (SystemDataRegistry.Instance.EDocsStorageAccess.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Key=testid;Secret=blabla"))
				{
					var testClient = new AWSPersisterForTest();
					AssertExceptionThrown<ExternalStorageAccessException>("KeyId", () => testClient.GetBaseClient());
				}

				using (SystemDataRegistry.Instance.EDocsStorageAccess.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "KeyId=testid;password=blabla"))
				{
					var testClient = new AWSPersisterForTest();
					AssertExceptionThrown<ExternalStorageAccessException>("Secret", () => testClient.GetBaseClient());
				}

				using (SystemDataRegistry.Instance.EDocsStorageAccess.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, " KeyId = testid ; Secret = blabla "))
				{
					var testClient = new AWSPersisterForTest();
					AssertNoExceptionThrown("blank space should be trimmed.", () => testClient.GetBaseClient());
				}
			}
		}

		public void TestBucketName()
		{
			// Providing URL means we are dealing with Non Amazon AWS S3 and BucketName should be in mixed case
			SystemDataRegistry.Instance.EDocsStorageServiceUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "htts://someNonAmazonS3.com");

			// Clear BucketName registry to revert back to default to make this test happy
			SystemDataRegistry.Instance.DocManagerStorageBucketName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, null);

			var awsPersister = new AWSPersisterForTest();

			AssertExceptionThrown<ExternalStorageAccessException>(() => { _ = awsPersister.ConfigExposed.BucketName; });

			SystemDataRegistry.Instance.DocManagerStorageBucketName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "SomeRandomBucketName");
			AssertEquals("SomeRandomBucketName", awsPersister.ConfigExposed.BucketName);
		}

		public void TestSaveAndRetrieveStream()
		{
			using (ExternalPersisterProviderTestHelper.MockExternalPersisterForS3(supportUpload: true, supportDownload: true))
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			using (var stream = resourceRetriever.GetStream("Enterprise.DocumentScanning.Business.Test.TestDocs.LargeImageWithBarcode.tif"))
			{
				var pk = ZGuid.NewZGuid();
				var awsPersister = (AWSPersisterForTest)ObjectFactory.Get<IExternalPersisterProvider>().GetExternalPersister("S3");

				// Upload
				AssertEquals("Upload stream", true, awsPersister.SaveStream(stream, pk).isSaved);

				// Download
				var (retrievedData, _) = awsPersister.RetrieveStream(pk);
				var expectedValue = resourceRetriever.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.LargeImageWithBarcode.tif");
				AssertEquals("Retrieved eDocs is same with the original", expectedValue, retrievedData.ToByteArray());
			}
		}

		public void TestDefaultSaveAndDelete()
		{
			using (var stream = new MemoryStream(AWSPersisterForTest.ImageDataForTest))
			{
				var pk = Guid.NewGuid();
				var putRequest = new PutObjectRequest();
				var getResponse = new GetObjectResponse();
				var isDeleted = false;
				var s3ClientMock = new Mock<IAmazonS3>(MockBehavior.Strict);
				s3ClientMock.Setup(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), CancellationToken.None)).Callback((PutObjectRequest request, CancellationToken token) =>
				{
					putRequest = request;
					var responseStream = new MemoryStream();
					request.InputStream.CopyTo(responseStream);
					responseStream.Position = 0;

					getResponse.ResponseStream = responseStream;
				}).Returns(Task.FromResult(new PutObjectResponse { HttpStatusCode = HttpStatusCode.OK }));

				s3ClientMock.Setup(x => x.GetObjectAsync(It.IsAny<GetObjectRequest>(), CancellationToken.None)).Callback((GetObjectRequest request, CancellationToken token) =>
				{
					if (isDeleted)
					{
						throw new ExternalStorageObjectNotFoundException(pk.ToString(), "deleted", "s3", null);
					}
				}).Returns(Task.FromResult(getResponse));

				s3ClientMock.Setup(x => x.DeleteObjectAsync(It.IsAny<DeleteObjectRequest>(), CancellationToken.None)).Callback((DeleteObjectRequest request, CancellationToken token) =>
				{
					isDeleted = true;
				}).Returns(Task.FromResult(new DeleteObjectResponse { HttpStatusCode = HttpStatusCode.OK }));

				var awsPersister = new AWSPersisterForTest();
				awsPersister.ClientMock = s3ClientMock;

				AssertEquals("Should be saved successfully", true, awsPersister.SaveStream(stream, pk).isSaved);

				var (retrievedData, _) = awsPersister.RetrieveStream(pk);
				var retrievedImageData = retrievedData.ToByteArray();
				var retrievedBlob = new ZBlob(retrievedImageData);
				AssertEquals("Should be retrieved successfully", AWSPersisterForTest.ImageDataForTest, retrievedBlob);

				//Remove from S3
				AssertEquals("Should delete successfully", true, awsPersister.Delete(pk));
				AssertExceptionThrown<ExternalStorageObjectNotFoundException>("After delete, it should throw ExternalStorageObjectNotFoundException", () => awsPersister.RetrieveStream(pk));
			}
		}

		public void TestDeleteWithFolderPathInS3BucketName()
		{
			using (SystemDataRegistry.Instance.DocManagerStorageBucketName.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "TestBucket/SubFolder"))
			{
				var s3ClientMock = new Mock<IAmazonS3>(MockBehavior.Strict);
				s3ClientMock.Setup(x => x.DeleteObjectAsync(It.IsAny<DeleteObjectRequest>(), CancellationToken.None)).Callback((DeleteObjectRequest request, CancellationToken token) =>
				{
					AssertEquals("TestBucket", request.BucketName);
					AssertEquals(true, request.Key.StartsWith("SubFolder"));
				}).Returns(Task.FromResult(new DeleteObjectResponse { HttpStatusCode = HttpStatusCode.OK }));

				var awsPersister = new AWSPersisterForTest();
				awsPersister.ClientMock = s3ClientMock;
				awsPersister.Delete(Guid.NewGuid());
			}
		}

		public void TestGetMetadataWithFolderPathInS3BucketName()
		{
			using (SystemDataRegistry.Instance.DocManagerStorageBucketName.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "TestBucket/SubFolder"))
			{
				var s3ClientMock = new Mock<IAmazonS3>(MockBehavior.Strict);
				s3ClientMock.Setup(x => x.GetObjectMetadataAsync(It.IsAny<GetObjectMetadataRequest>(), CancellationToken.None)).Callback((GetObjectMetadataRequest request, CancellationToken token) =>
				{
					AssertEquals("TestBucket", request.BucketName);
					AssertEquals(true, request.Key.StartsWith("SubFolder"));
				}).Returns(Task.FromResult(new GetObjectMetadataResponse { HttpStatusCode = HttpStatusCode.OK }));

				var awsPersister = new AWSPersisterForTest();
				awsPersister.ClientMock = s3ClientMock;
				awsPersister.GetObjectMetadata(Guid.NewGuid());
			}
		}

		public void TestParanoidSave()
		{
			SystemDataRegistry.Instance.DocManagerParanoidModeEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var awsPersister = new AWSPersisterForTest(true, true);
			using (var stream = new MemoryStream(AWSPersisterForTest.ImageDataForTest))
			{
				var pk = ZGuid.NewZGuid();
				awsPersister.FailToSave = true;
				var response = awsPersister.SaveStream(stream, pk);
				AssertEquals(false, response.isSaved);

				awsPersister.FailToSave = false;
				response = awsPersister.SaveStream(stream, pk);
				AssertEquals(true, response.isSaved);

				SystemDataRegistry.Instance.DocManagerParanoidModeEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

				awsPersister.FailToSave = true;
				response = awsPersister.SaveStream(stream, pk);
				AssertEquals(true, response.isSaved);

				awsPersister.FailToSave = false;
				response = awsPersister.SaveStream(stream, pk);
				AssertEquals(true, response.isSaved);
			}
		}

		public void TestSaveStream_VersionIdShouldBeReturned()
		{
			// Arrange
			using (var stream = new MemoryStream(AWSPersisterForTest.ImageDataForTest))
			{
				const string VersionId = "versionIdForTesting";
				var s3ClientMock = new Mock<IAmazonS3>(MockBehavior.Strict);
				s3ClientMock.Setup(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), CancellationToken.None)).Returns(Task.FromResult(new PutObjectResponse { HttpStatusCode = HttpStatusCode.OK, VersionId = VersionId }));
				var awsPersister = new AWSPersisterForTest { ClientMock = s3ClientMock };

				// Act
				var result = awsPersister.SaveStream(stream, Guid.NewGuid());

				// Assert
				AssertEquals("Should be saved successfully", true, result.isSaved);
				AssertEquals("Version ID should be returned", VersionId, result.versionId);
			}
		}

		public void TestRetrieveStreamWithVersionId()
		{
			// Arrange
			const string VersionId = "versionIdForTesting";
			var s3ClientMock = new Mock<IAmazonS3>(MockBehavior.Strict);
			var response = new GetObjectResponse();

			s3ClientMock.Setup(x => x.GetObjectAsync(It.IsAny<GetObjectRequest>(), CancellationToken.None)).Callback((GetObjectRequest request, CancellationToken token) =>
			{
				if (request.VersionId == VersionId)
				{
					response.ResponseStream = new MemoryStream(new byte[] { 1, 2, 4, 3 });
				}
				else
				{
					throw new AmazonS3Exception("Not found");
				}
			}).Returns(Task.FromResult(response));
			var awsPersister = new AWSPersisterForTest { ClientMock = s3ClientMock };

			// Act
			var result = awsPersister.RetrieveStream(Guid.NewGuid(), VersionId);

			// Assert
			AssertNotNull("Stream will be returned if version id matches", result);
		}

		public void TestRetrieveStreamWithVersionId_ExceptionIsThrownOutIfNotFoundForSpecifiedVersion()
		{
			// Arrange
			const string VersionId = "versionIdForTesting";
			var s3ClientMock = new Mock<IAmazonS3>(MockBehavior.Strict);
			s3ClientMock.Setup(x => x.GetObjectAsync(It.IsAny<GetObjectRequest>(), CancellationToken.None)).Throws(new AmazonS3Exception("No Such Key") { ErrorCode = "NoSuchKey" });
			var awsPersister = new AWSPersisterForTest { ClientMock = s3ClientMock };

			// Act & Assert
			AssertExceptionThrown<ExternalStorageObjectNotFoundException>("NoSuchKey", () => awsPersister.RetrieveStream(Guid.NewGuid(), VersionId));

			// Re-arrange/act/assert for NotFound code
			s3ClientMock.Setup(x => x.GetObjectAsync(It.IsAny<GetObjectRequest>(), CancellationToken.None)).Throws(new AmazonS3Exception("Not found") { ErrorCode = "NotFound" });
			AssertExceptionThrown<ExternalStorageObjectNotFoundException>("NotFound", () => awsPersister.RetrieveStream(Guid.NewGuid(), VersionId));

			// Re-arrange/act/assert for NoSuchVersion code
			s3ClientMock.Setup(x => x.GetObjectAsync(It.IsAny<GetObjectRequest>(), CancellationToken.None)).Throws(new AmazonS3Exception("Not Such Version") { ErrorCode = "NoSuchVersion" });
			AssertExceptionThrown<ExternalStorageObjectNotFoundException>("NoSuchVersion", () => awsPersister.RetrieveStream(Guid.NewGuid(), VersionId));
		}

		public void TestRetrieveStreamWithFolderPathInS3BucketName()
		{
			using (SystemDataRegistry.Instance.DocManagerStorageBucketName.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "TestBucket/SubFolder"))
			{
				var s3ClientMock = new Mock<IAmazonS3>(MockBehavior.Strict);
				s3ClientMock.Setup(x => x.GetObjectAsync(It.IsAny<GetObjectRequest>(), CancellationToken.None)).Callback((GetObjectRequest request, CancellationToken token) =>
				{
					AssertEquals("TestBucket", request.BucketName);
					AssertEquals(true, request.Key.StartsWith("SubFolder"));
				}).Returns(Task.FromResult(new GetObjectResponse() { ResponseStream = new MemoryStream() }));
				var awsPersister = new AWSPersisterForTest { ClientMock = s3ClientMock };
				_ = awsPersister.RetrieveStream(Guid.NewGuid());
			}
		}

		public void TestTimeoutSettings()
		{
			using (SystemDataRegistry.Instance.EDocsStorageAccess.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "KeyId=somekey;Secret=somesecret"))
			{
				using (SystemDataRegistry.Instance.EDocsStorageConnectionTimeout.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 20))
				{
					var awsPersister = new AWSPersisterForTest();
					var amazonS3Client = awsPersister.GetBaseClient();
					AssertEquals("Timeout should set according to the registry value: 20s", TimeSpan.FromSeconds(20), amazonS3Client.Config.Timeout);
#if NETFRAMEWORK
					AssertEquals("ReadWriteTimeout should default to 5 minutes.", TimeSpan.FromSeconds(300), amazonS3Client.Config.ReadWriteTimeout);
#endif
				}

				using (SystemDataRegistry.Instance.EDocsStorageConnectionTimeout.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
				{
					var awsPersister = new AWSPersisterForTest();
					var amazonS3Client = awsPersister.GetBaseClient();
					AssertEquals("Timeout should set according to Max if the registry value is 0", TimeSpan.FromMilliseconds(int.MaxValue), amazonS3Client.Config.Timeout);
#if NETFRAMEWORK
					AssertEquals("ReadWriteTimeout should default to 5 minutes.", TimeSpan.FromSeconds(300), amazonS3Client.Config.ReadWriteTimeout);
#endif
				}
			}
		}

		public void TestRetrieveStream_WhereReturnedStreamSupportsSeeking()
		{
			var awsPersister = new AWSPersisterForTest(false, true);
			var (stream, _) = awsPersister.RetrieveStream(ZGuid.NewZGuid());

			Assert("Should support seeking", stream.CanSeek);
		}

		public void TestCanHandleS3RegistriesWithTrailingSpace()
		{
			SystemDataRegistry.Instance.EDocsStorageServiceUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://s3.test.local ");
			SystemDataRegistry.Instance.DocManagerStorageBucketName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "test ");
			SystemDataRegistry.Instance.EDocsStorageAccess.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "KeyId=dude;Secret=top");

			using (var stream = new MemoryStream(AWSPersisterForTest.ImageDataForTest))
			{
				var config = new AWSPersisterConfig();
				var awsPersister = new AWSPersisterForTest(config);
				awsPersister.ClientMock = GetS3ClientMockWithUrlAndBucketException(config);
				AssertEquals("Should be saved successfully", true, awsPersister.SaveStream(stream, Guid.NewGuid()).isSaved);
			}
		}

		public void TestExternalStorageInvalidBucketNameException()
		{
			using (var stream = new MemoryStream(AWSPersisterForTest.ImageDataForTest))
			{
				var config = new AWSPersisterConfig("test ", "https://s3.test.local", "", "");
				var awsPersister = new AWSPersisterForTest(config);
				awsPersister.ClientMock = GetS3ClientMockWithUrlAndBucketException(config);
				AssertExceptionThrown(typeof(ExternalStorageInvalidBucketNameException), "S3 bucket name 'test ' is invalid.", () => awsPersister.SaveStream(stream, Guid.NewGuid()));
			}
		}

		public void TestExternalStorageUriFormatException()
		{
			using (var stream = new MemoryStream(AWSPersisterForTest.ImageDataForTest))
			{
				var config = new AWSPersisterConfig("test", "https://s3.test.local ", "", "");
				var awsPersister = new AWSPersisterForTest(config);
				awsPersister.ClientMock = GetS3ClientMockWithUrlAndBucketException(config);
				AssertExceptionThrown(typeof(ExternalStorageUriFormatException), "S3 storage url 'https://s3.test.local ' is invalid format.", () => awsPersister.SaveStream(stream, Guid.NewGuid()));
			}
		}

		Mock<IAmazonS3> GetS3ClientMockWithUrlAndBucketException(AWSPersisterConfig config)
		{
			var s3ClientMock = new Mock<IAmazonS3>(MockBehavior.Strict);
			s3ClientMock.Setup(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), CancellationToken.None)).Callback((PutObjectRequest request, CancellationToken token) =>
			{
				if (config.BucketName.Length > config.BucketName.Trim().Length)
				{
					var amzException = new AmazonS3Exception("Invalid bucket name");
					amzException.ErrorCode = "InvalidBucketName";
					amzException.StatusCode = HttpStatusCode.BadRequest;
					throw amzException;
				}

				if (config.ServiceURL.Length > config.ServiceURL.Trim().Length)
				{
					var ex = new UriFormatException("Invalid URL");
					throw ex;
				}
			}).Returns(Task.FromResult(new PutObjectResponse { HttpStatusCode = HttpStatusCode.OK }));
			return s3ClientMock;
		}

		public void TestSaveStreamCanHandleInvalidDigest()
		{
			using (var stream = new MemoryStream(AWSPersisterForTest.ImageDataForTest))
			{
				var s3ClientMock = new Mock<IAmazonS3>(MockBehavior.Strict);
				var s3Exception = new AmazonS3Exception("Error making request with Error Code InvalidDigest and Http Status Code BadRequest. No further error information was returned by the service.");
				s3Exception.ErrorCode = "InvalidDigest";
				s3Exception.StatusCode = HttpStatusCode.BadRequest;
				s3ClientMock.Setup(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), CancellationToken.None)).Throws(s3Exception);
				var awsPersister = new AWSPersisterForTest { ClientMock = s3ClientMock };

				var (isSaved, versionId) = awsPersister.SaveStream(stream, Guid.NewGuid());

				AssertEquals("Should not saved successfully", expected: false, isSaved);
				AssertEquals("Should not return version id", null, versionId);
			}
		}

		public void TestSaveStreamWithFolderPathInS3BucketName()
		{
			using (var stream = new MemoryStream(AWSPersisterForTest.ImageDataForTest))
			using (SystemDataRegistry.Instance.DocManagerStorageBucketName.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "TestBucket/SubFolder"))
			{
				var s3ClientMock = new Mock<IAmazonS3>(MockBehavior.Strict);
				s3ClientMock.Setup(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), CancellationToken.None)).Callback((PutObjectRequest request, CancellationToken token) =>
				{
					AssertEquals("TestBucket", request.BucketName);
					AssertEquals(true, request.Key.StartsWith("SubFolder"));
				}).Returns(Task.FromResult(new PutObjectResponse { HttpStatusCode = HttpStatusCode.OK }));
				var awsPersister = new AWSPersisterForTest { ClientMock = s3ClientMock };
				_ = awsPersister.SaveStream(stream, Guid.NewGuid());
			}
		}

		public void TestCheckBucketNotInvokedForCachedClients()
		{
			var awsPersister = new AWSPersisterForTest { DoNotMock = true };
			awsPersister.ClearCachedClients();

			awsPersister.GetBaseClient();
			AssertEquals("Call DoesS3BucketExist should create bucket", true, awsPersister.DoesS3BucketExistInvoked);

			awsPersister.DoesS3BucketExistInvoked = false;
			awsPersister.GetBaseClient();
			AssertEquals("Call DoesS3BucketExist should not create bucket", false, awsPersister.DoesS3BucketExistInvoked);
		}

		public void TestAllowWriteToExternalStorage_NoLicenceInS3()
		{
			// Arrange
			var retriveActionInvoked = false;
			var writeActionInvoked = false;
			var s3ClientMock = GetS3ClientMockForAllowWriteToExternalStorageTests();

			var getObjectResponse = new GetObjectResponse();
			s3ClientMock.Setup(x => x.GetObjectAsync(It.IsAny<GetObjectRequest>(), CancellationToken.None)).Callback((GetObjectRequest request, CancellationToken token) =>
			{
				retriveActionInvoked = true;
				AssertEquals("Licence file name", LicenceFile, request.Key);
				throw new AmazonS3Exception(string.Empty) { ErrorCode = "NotFound" };
			}).Returns(Task.FromResult(getObjectResponse));

			var putObjectResponse = new PutObjectResponse();
			s3ClientMock.Setup(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), CancellationToken.None)).Callback((PutObjectRequest request, CancellationToken token) =>
			{
				writeActionInvoked = true;
				var systemCode = Encoding.UTF8.GetString(request.InputStream.ToByteArray());
				AssertEquals("Licence file name", LicenceFile, request.Key);
				AssertEquals("Licence system code", "EntSvr,TST", systemCode);
				putObjectResponse.HttpStatusCode = HttpStatusCode.OK;
			}).Returns(Task.FromResult(putObjectResponse));

			var awsPersister = new AWSPersisterForTest { ClientMock = s3ClientMock };

			// Act
			var result = awsPersister.AllowWrite;

			// Assert
			AssertEquals("Should invoke retrieve system code from S3", true, retriveActionInvoked);
			AssertEquals("Should invoke write system code to S3", true, writeActionInvoked);
			AssertEquals("Should be able to write to External Storage", true, result);
		}

		public void TestAllowWriteToExternalStorage_FailToGetSystemCodeFromS3()
		{
			// Arrange
			var s3ClientMock = GetS3ClientMockForAllowWriteToExternalStorageTests();
			var getObjectResponse = new GetObjectResponse();
			s3ClientMock.Setup(x => x.GetObjectAsync(It.IsAny<GetObjectRequest>(), CancellationToken.None)).Callback((GetObjectRequest request, CancellationToken token) =>
			{
				throw new Exception("Random error");
			}).Returns(Task.FromResult(getObjectResponse));

			var awsPersister = new AWSPersisterForTest { ClientMock = s3ClientMock };

			AssertExceptionThrown<Exception>(
				"Should fail to get system code from S3 bucket as exception is thrown in GetObject",
				"Random error",
				() => _ = awsPersister.AllowWrite);
		}

		public void TestAllowWriteToExternalStorage_FailToWriteSystemCodeToS3()
		{
			// Arrange
			var s3ClientMock = GetS3ClientMockForAllowWriteToExternalStorageTests();
			var getObjectResponse = new GetObjectResponse();
			s3ClientMock.Setup(x => x.GetObjectAsync(It.IsAny<GetObjectRequest>(), CancellationToken.None)).Callback((GetObjectRequest request, CancellationToken token) =>
			{
				throw new AmazonS3Exception(string.Empty) { ErrorCode = "NotFound" };
			}).Returns(Task.FromResult(getObjectResponse));

			var putObjectResponse = new PutObjectResponse();
			s3ClientMock.Setup(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), CancellationToken.None)).Callback((PutObjectRequest request, CancellationToken token) =>
			{
				putObjectResponse.HttpStatusCode = HttpStatusCode.InternalServerError;
			}).Returns(Task.FromResult(putObjectResponse));

			var awsPersister = new AWSPersisterForTest { ClientMock = s3ClientMock };

			AssertExceptionThrown<Exception>(
				"Should fail to write system code to S3 bucket as response is set to InternalServerError",
				"Failed to write system code to S3 Bucket.",
				() => _ = awsPersister.AllowWrite);
		}

		public void TestAllowWriteToExternalStorage_CorrectLicenceInS3()
		{
			// Arrange
			var s3ClientMock = GetS3ClientMockForAllowWriteToExternalStorageTests();
			var retriveActionInvoked = false;
			var writeActionInvoked = false;

			var getObjectResponse = new GetObjectResponse();
			s3ClientMock.Setup(x => x.GetObjectAsync(It.IsAny<GetObjectRequest>(), CancellationToken.None)).Callback((GetObjectRequest request, CancellationToken token) =>
			{
				retriveActionInvoked = true;
				AssertEquals("Licence file name", LicenceFile, request.Key);
				getObjectResponse.ResponseStream = new MemoryStream(Encoding.UTF8.GetBytes($"{EnterpriseCode}{ServerCode},{DatabaseTypes.Codes.Test}"));
			}).Returns(Task.FromResult(getObjectResponse));

			var putObjectResponse = new PutObjectResponse();
			s3ClientMock.Setup(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), CancellationToken.None)).Callback((PutObjectRequest request, CancellationToken token) =>
			{
				writeActionInvoked = true;
			}).Returns(Task.FromResult(putObjectResponse));

			var awsPersister = new AWSPersisterForTest { ClientMock = s3ClientMock };

			// Act
			var result = awsPersister.AllowWrite;

			// Assert
			AssertEquals("Should invoke retrieve system code from S3", true, retriveActionInvoked);
			AssertEquals("Should NOT invoke write system code to S3", false, writeActionInvoked);
			AssertEquals("Should be able to write to External Storage", true, result);
		}

		public void TestAllowWriteToExternalStorage_WrongLicenceInS3()
		{
			// Arrange
			var s3ClientMock = GetS3ClientMockForAllowWriteToExternalStorageTests();
			var retriveActionInvoked = false;
			var writeActionInvoked = false;

			var getObjectResponse = new GetObjectResponse();
			s3ClientMock.Setup(x => x.GetObjectAsync(It.IsAny<GetObjectRequest>(), CancellationToken.None)).Callback((GetObjectRequest request, CancellationToken token) =>
			{
				retriveActionInvoked = true;
				AssertEquals("Licence file name", LicenceFile, request.Key);
				getObjectResponse.ResponseStream = new MemoryStream(Encoding.UTF8.GetBytes("ABCDEF"));
			}).Returns(Task.FromResult(getObjectResponse));

			var putObjectResponse = new PutObjectResponse();
			s3ClientMock.Setup(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), CancellationToken.None)).Callback((PutObjectRequest request, CancellationToken token) =>
			{
				writeActionInvoked = true;
			}).Returns(Task.FromResult(putObjectResponse));

			var awsPersister = new AWSPersisterForTest { ClientMock = s3ClientMock };

			// Act
			var result = awsPersister.AllowWrite;

			// Assert
			AssertEquals("Should invoke retrieve system code from S3", true, retriveActionInvoked);
			AssertEquals("Should NOT invoke write system code to S3", false, writeActionInvoked);
			AssertEquals("Should NOT be able to write to External Storage", false, result);
		}

		public void TestAllowWriteToExternalStorage_ProductionCanOverwriteTestLicenceInS3()
		{
			// Arrange
			var s3ClientMock = GetS3ClientMockForAllowWriteToExternalStorageTests(DatabaseTypes.Codes.Production);
			var retriveActionInvoked = false;
			var writeActionInvoked = false;

			var getObjectResponse = new GetObjectResponse();
			s3ClientMock.Setup(x => x.GetObjectAsync(It.IsAny<GetObjectRequest>(), CancellationToken.None)).Callback((GetObjectRequest request, CancellationToken token) =>
			{
				retriveActionInvoked = true;
				AssertEquals("Licence file name", LicenceFile, request.Key);
				getObjectResponse.ResponseStream = new MemoryStream(Encoding.UTF8.GetBytes($"{EnterpriseCode}{ServerCode},{DatabaseTypes.Codes.Test}"));
			}).Returns(Task.FromResult(getObjectResponse));

			var putObjectResponse = new PutObjectResponse();
			s3ClientMock.Setup(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), CancellationToken.None)).Callback((PutObjectRequest request, CancellationToken token) =>
			{
				writeActionInvoked = true;
				var newCode = Encoding.UTF8.GetString(request.InputStream.ReadFully());
				AssertEquals($"{EnterpriseCode}{ServerCode},{DatabaseTypes.Codes.Production}", newCode);
				putObjectResponse.HttpStatusCode = HttpStatusCode.OK;
			}).Returns(Task.FromResult(putObjectResponse));

			var awsPersister = new AWSPersisterForTest { ClientMock = s3ClientMock };

			// Act
			var result = awsPersister.AllowWrite;

			// Assert
			AssertEquals("Should invoke retrieve system code from S3", true, retriveActionInvoked);
			AssertEquals("Should invoke write system code to S3", true, writeActionInvoked);
			AssertEquals("Should be able to write to External Storage", true, result);

			var mail = new BusinessObjectFactory().LoadTop1<MailItem>(new ZQuery());
			AssertEquals(EnvProxy.Instance.Registry.HostedNotificationsEmailOverride, mail.MailRecipients[0].EmailAddress);
			AssertContains("S3 Bucket License file overwritten", mail.MI_Subject);
			AssertContains("The license file of S3 bucket test has been overwritten, system code is updated from EntSvr,TST to EntSvr,PRD. This could happen if the bucket was used by non-production system, but now it's used by production system. Please verify the new system code matches to your production system code in the format of '<Enterprise Code><Server Code>,PRD'.", mail.MI_Body);
		}

		public void TestAllowWriteToExternalStorage_TestCannotOverwriteProductionLicenceInS3()
		{
			// Arrange
			var s3ClientMock = GetS3ClientMockForAllowWriteToExternalStorageTests();
			var retriveActionInvoked = false;
			var writeActionInvoked = false;

			var getObjectResponse = new GetObjectResponse();
			s3ClientMock.Setup(x => x.GetObjectAsync(It.IsAny<GetObjectRequest>(), CancellationToken.None)).Callback((GetObjectRequest request, CancellationToken token) =>
			{
				retriveActionInvoked = true;
				AssertEquals("Licence file name", LicenceFile, request.Key);
				getObjectResponse.ResponseStream = new MemoryStream(Encoding.UTF8.GetBytes($"{EnterpriseCode}{ServerCode},{DatabaseTypes.Codes.Production}"));
			}).Returns(Task.FromResult(getObjectResponse));

			var putObjectResponse = new PutObjectResponse();
			s3ClientMock.Setup(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), CancellationToken.None)).Callback((PutObjectRequest request, CancellationToken token) =>
			{
				writeActionInvoked = true;
			}).Returns(Task.FromResult(putObjectResponse));

			var awsPersister = new AWSPersisterForTest { ClientMock = s3ClientMock };

			// Act
			var result = awsPersister.AllowWrite;

			// Assert
			AssertEquals("Should invoke retrieve system code from S3", true, retriveActionInvoked);
			AssertEquals("Should NOT invoke write system code to S3", false, writeActionInvoked);
			AssertEquals("Should NOT be able to write to External Storage", false, result);
		}

		public void TestAllowWriteToExternalStorage_DoNotReportS3Usage()
		{
			var pk = ZGuid.NewZGuid();
			using (ExternalPersisterProviderTestHelper.MockExternalPersisterForS3(supportUpload: true, supportDownload: true))
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				var awsPersister = (AWSPersisterForTest)ObjectFactory.Get<IExternalPersisterProvider>().GetExternalPersister("S3");
				var factory = new BusinessObjectFactory();
				var usageCollector = new UsageCollectorTestHelper(factory);
				var existingUsageMessages = usageCollector.LoadUsageMessages(UsageFeatures.Codes.DocumentS3Transfer);

				using (var stream = resourceRetriever.GetStream("Enterprise.DocumentScanning.Business.Test.TestDocs.LargeImageWithBarcode.tif"))
				{
					_ = awsPersister.AllowWrite;
				}

				var updatedUsageMessages = usageCollector.LoadUsageMessages(UsageFeatures.Codes.DocumentS3Transfer);
				var numberOfNewUsageMessages = updatedUsageMessages.Length - existingUsageMessages.Length;

				AssertEquals($"Retrieve and write S3BucektClaimFile shouldn't report S3 usage", 0, numberOfNewUsageMessages);
			}
		}

		public void TestReportProperties()
		{
			var pk = ZGuid.NewZGuid();
			using (ExternalPersisterProviderTestHelper.MockExternalPersisterForS3(supportUpload: true, supportDownload: true))
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			using (DocManagerRegistry.Instance.EnableS3UsageReport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var awsPersister = (AWSPersisterForTest)ObjectFactory.Get<IExternalPersisterProvider>().GetExternalPersister("S3");
				var factory = new BusinessObjectFactory();
				var usageCollector = new UsageCollectorTestHelper(factory);
				long expectedFileSize;
				var existingUsageMessages = usageCollector.LoadUsageMessages(UsageFeatures.Codes.DocumentS3Transfer);

				using (var stream = resourceRetriever.GetStream("Enterprise.DocumentScanning.Business.Test.TestDocs.LargeImageWithBarcode.tif"))
				{
					expectedFileSize = stream.Length;
					awsPersister.SaveStream(stream, pk);
					awsPersister.RetrieveStream(pk, new ZString("versionId"));
				}

				var updatedUsageMessages = usageCollector.LoadUsageMessages(UsageFeatures.Codes.DocumentS3Transfer);
				var numberOfNewUsageMessages = updatedUsageMessages.Length - existingUsageMessages.Length;

				var sb = new StringBuilder();
				if (updatedUsageMessages.Length > existingUsageMessages.Length + 2)
				{
					sb.AppendLine("Messages returned were:");
					foreach (var message in updatedUsageMessages)
					{
						if (!existingUsageMessages.Contains(message))
						{
							sb.AppendLine(message.EM_MessageText).AppendLine();
						}
					}
				}
				string reportedMessages = sb.ToString();

				AssertEquals($"Expected 2 updatedUsageMessages to be loaded after the reports, but counted {numberOfNewUsageMessages}. \n{reportedMessages}", 2, numberOfNewUsageMessages);

				var expectedMessageFormat = @"{{
  ""FeatureCode"": ""{0}"",
  ""Module"": ""{1}"",
  ""FeatureDescription"": ""Document S3 Transfer"",
  ""OrganisationName"": ""EDI CUSTOMS BROKERS"",
  ""DocumentS3TimeStamp"": ""{2}"",
  ""DocumentS3BucketName"": ""test"",
  ""DocumentS3PrimaryKey"": ""{3}"",
  ""DocumentS3Size"": ""{4}"",
  ""DocumentS3ActionType"": ""{5}""
}}";

				var actualDownloadMessage = updatedUsageMessages[0].EM_MessageText.ToString();
				var actualDownloadTimeWithoutAmPm = ExtractTimestampFromUsageCollectorReport(actualDownloadMessage);
				var actualDownloadDateTime = DateTime.ParseExact(actualDownloadTimeWithoutAmPm, "MM/dd/yyyy HH:mm:ss", null);
				var expectedDownloadMessage = string.Format(expectedMessageFormat, UsageFeatures.Codes.DocumentS3Transfer, UsageFeatures.Modules.DocumentS3Transfer, actualDownloadTimeWithoutAmPm, pk, expectedFileSize, "Download");

				AssertEquals("UsageCollector returned unexpected values during the download.\n", expectedDownloadMessage, actualDownloadMessage);
				NUnit.Framework.Assert.That(actualDownloadDateTime, NUnit.Framework.Is.EqualTo(ZDateTime.UtcNow.ToDateTime()).Within(5).Seconds, "The reported download timestamp must be within 5 seconds of the expected timestamp.\n");

				var actualUploadMessage = updatedUsageMessages[1].EM_MessageText.ToString();
				var actualUploadTimeWithoutAmPm = ExtractTimestampFromUsageCollectorReport(actualUploadMessage);
				var actualUploadDateTime = DateTime.ParseExact(actualUploadTimeWithoutAmPm, "MM/dd/yyyy HH:mm:ss", null);
				var expectedUploadMessage = string.Format(expectedMessageFormat, UsageFeatures.Codes.DocumentS3Transfer, UsageFeatures.Modules.DocumentS3Transfer, actualUploadTimeWithoutAmPm, pk, expectedFileSize, "Upload");

				AssertEquals("UsageCollector returned unexpected values during the upload.\n", expectedUploadMessage, actualUploadMessage);
				NUnit.Framework.Assert.That(actualUploadDateTime, NUnit.Framework.Is.EqualTo(ZDateTime.UtcNow.ToDateTime()).Within(5).Seconds, "The reported upload timestamp must be within 5 seconds of the expected timestamp.\n");
			}
		}

		public void TestNoReportsGeneratedForFailedActions()
		{
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			using (DocManagerRegistry.Instance.EnableS3UsageReport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var config = new AWSPersisterConfig("test ", "https://s3.test.local", "", "");
				var awsPersisterWithSaveStreamError = new AWSPersisterForTest(config);
				awsPersisterWithSaveStreamError.ClientMock = GetS3ClientMockWithUrlAndBucketException(config);

				var s3ClientMock = new Mock<IAmazonS3>(MockBehavior.Strict);
				s3ClientMock.Setup(x => x.GetObjectAsync(It.IsAny<GetObjectRequest>(), CancellationToken.None)).Throws(new AmazonS3Exception("No Such Key") { ErrorCode = "NoSuchKey" });
				var awsPersisterWithRestrieveStreamError = new AWSPersisterForTest { ClientMock = s3ClientMock };

				var factory = new BusinessObjectFactory();
				var usageCollector = new UsageCollectorTestHelper(factory);
				var existingUsageMessages = usageCollector.LoadUsageMessages(UsageFeatures.Codes.DocumentS3Transfer);

				using (var stream = resourceRetriever.GetStream("Enterprise.DocumentScanning.Business.Test.TestDocs.1MB.dat"))
				{
					AssertExceptionThrown(typeof(ExternalStorageInvalidBucketNameException), "S3 bucket name 'test ' is invalid.", () => awsPersisterWithSaveStreamError.SaveStream(stream, Guid.NewGuid()));
					AssertExceptionThrown<ExternalStorageObjectNotFoundException>("NoSuchKey", () => awsPersisterWithRestrieveStreamError.RetrieveStream(Guid.NewGuid(), "versionIdForTesting"));
				}

				var updatedUsageMessages = usageCollector.LoadUsageMessages(UsageFeatures.Codes.DocumentS3Transfer);
				var numberOfNewUsageMessages = updatedUsageMessages.Length - existingUsageMessages.Length;
				string messagesReturned = "";

				var sb = new StringBuilder();
				foreach (var message in updatedUsageMessages)
				{
					if (!existingUsageMessages.Contains(message))
					{
						sb.AppendLine(message.EM_MessageText).AppendLine();
					}
				}
				messagesReturned = sb.ToString();

				AssertEquals($"Expected 0 updatedUsageMessages to be loaded after the reports, but counted {numberOfNewUsageMessages}. Messages returned were:\n\n{messagesReturned}", 0, numberOfNewUsageMessages);
			}
		}

		public void TestNoReportGeneratedIfEnableS3UsageReportRegistryIsTurnedOff()
		{
			var pk = ZGuid.NewZGuid();
			using (ExternalPersisterProviderTestHelper.MockExternalPersisterForS3(supportUpload: true, supportDownload: true))
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			using (DocManagerRegistry.Instance.EnableS3UsageReport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var awsPersister = (AWSPersisterForTest)ObjectFactory.Get<IExternalPersisterProvider>().GetExternalPersister("S3");
				var factory = new BusinessObjectFactory();
				var usageCollector = new UsageCollectorTestHelper(factory);
				var existingUsageMessages = usageCollector.LoadUsageMessages(UsageFeatures.Codes.DocumentS3Transfer);

				using (var stream = resourceRetriever.GetStream("Enterprise.DocumentScanning.Business.Test.TestDocs.LargeImageWithBarcode.tif"))
				{
					_ = awsPersister.SaveStream(stream, pk);
					_ = awsPersister.RetrieveStream(pk, new ZString("versionId"));
				}

				var updatedUsageMessages = usageCollector.LoadUsageMessages(UsageFeatures.Codes.DocumentS3Transfer);
				var numberOfNewUsageMessages = updatedUsageMessages.Length - existingUsageMessages.Length;

				AssertEquals($"Expected 0 updatedUsageMessages to be loaded after the reports, but counted {numberOfNewUsageMessages}.", 0, numberOfNewUsageMessages);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			SystemDataRegistry.Instance.EDocsStorageServiceUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://s3.test.local");
			SystemDataRegistry.Instance.DocManagerStorageBucketName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "test");
			SystemDataRegistry.Instance.EDocsStorageAccess.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "KeyId=dude;Secret=top");
		}

		Mock<IAmazonS3> GetS3ClientMockForAllowWriteToExternalStorageTests(string databaseType = null)
		{
			var prodReg = new Mock<IProductRegistration>();
			var prodRegKey = new Mock<IProductRegistrationKey>();
			prodReg.Setup(x => x.Key).Returns(prodRegKey.Object);
			prodRegKey.Setup(x => x.EnterpriseCode).Returns(EnterpriseCode);
			prodRegKey.Setup(x => x.ServerCode).Returns(ServerCode);
			prodRegKey.Setup(x => x.DatabaseType).Returns(databaseType ?? DatabaseTypes.Codes.Test);
			prodRegKey.Setup(x => x.HostedLocation).Returns("SYD");
			ObjectFactory.Substitute(prodReg.Object);

			return new Mock<IAmazonS3>(MockBehavior.Strict);
		}

		[DeveloperOnlyTest]
		public void TestRealDefaultSaveAndDelete()
		{
			using (SystemDataRegistry.Instance.EDocsStorageProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.EDocsStorageProviders.Code.S3))
			using (SystemDataRegistry.Instance.DocManagerParanoidModeEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, temporaryValue: true))
			using (SystemDataRegistry.Instance.EDocsStorageAccess.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "KeyId=GQVKLEH8SGACEVDNY5YR;Secret=D2AQztGSYx9tT35uCXpaYogw4kIY1mNBv9v73Xxk"))
			using (SystemDataRegistry.Instance.EDocsStorageServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://au2s3-1.wtg.ws"))
			using (SystemDataRegistry.Instance.DocManagerStorageBucketName.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "wisetechedocs-unit-tests"))
			{
				using (var stream = new MemoryStream(AWSPersisterForTest.ImageDataForTest))
				{
					var pk = Guid.NewGuid();
					var awsPersister = new AWSPersisterForTest();
					awsPersister.DoNotMock = true;

					AssertEquals("Should be saved successfully", expected: true, awsPersister.SaveStream(stream, pk).isSaved);
					var (retrievedData, _) = awsPersister.RetrieveStream(pk);
					var retrievedImageData = retrievedData.ToByteArray();
					var retrievedBlob = new ZBlob(retrievedImageData);
					AssertEquals("Should be retrieved successfully", AWSPersisterForTest.ImageDataForTest, retrievedBlob);

					//Remove from S3
					AssertEquals("Should delete successfully", expected: true, awsPersister.Delete(pk));
					AssertExceptionThrown<ExternalStorageObjectNotFoundException>("After delete, it should throw ExternalStorageObjectNotFoundException", () => awsPersister.RetrieveStream(pk));
				}
			}
		}

		string ExtractTimestampFromUsageCollectorReport(string downloadMessage)
		{
			var startIndex = downloadMessage.IndexOf("\"DocumentS3TimeStamp\": \"") + "\"DocumentS3TimeStamp\": \"".Length;
			var endIndex = downloadMessage.IndexOf("\"", startIndex);
			return downloadMessage.Substring(startIndex, endIndex - startIndex - 0);
		}

		const string LicenceFile = "License.txt";
		const string EnterpriseCode = "Ent";
		const string ServerCode = "Svr";
	}
}
