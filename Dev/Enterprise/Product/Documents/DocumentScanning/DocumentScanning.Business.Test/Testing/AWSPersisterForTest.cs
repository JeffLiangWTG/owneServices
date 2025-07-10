using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Runtime.Internal.Util;
using Amazon.S3;
using Amazon.S3.Model;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Moq;

namespace Enterprise.DocumentScanning.Business.Test
{
	public class AWSPersisterForTest : AWSPersister
	{
		public AWSPersisterForTest(AWSPersisterConfig config) : base(config) { }

		public AWSPersisterForTest(bool supportUpload = false, bool supportDownload = false)
		{
			this.supportUpload = supportUpload;
			this.supportDownload = supportDownload;
			allowWrite = null;
		}

		readonly bool supportUpload;
		readonly bool supportDownload;

		public bool FailToSave
		{
			get => failToSave;
			set
			{
				clientMock = null;
				failToSave = value;
			}
		}

		bool failToSave;

		public void DisableCheckCanWriteToExternalStoarge()
		{
			allowWrite = true;
		}

		public HttpClient BucketSizeClient { get; set; }

		protected override HttpClient GetHttpClient()
		{
			return BucketSizeClient ?? base.GetHttpClient();
		}

		public static ZBlob ImageDataForTest => ZBlob.FromAscii("Sphinx of black quartz, judge my vow.");

		protected override IAmazonS3 GetClient() => DoNotMock ? base.GetClient() : ClientMock.Object;

		public bool DoNotMock;
		public void ClearCachedClients() => CachedClients = null;

		public ZBlob UploadedBytesForTest { get; set; } = ZBlob.Empty;

		public string VersionIdForTest { get; set; }

		Mock<IAmazonS3> CreateClientMock()
		{
			var getResponse = new GetObjectResponse();
			var mock = new Mock<IAmazonS3>(MockBehavior.Strict);

			if (supportUpload)
			{
				var putResponse = new PutObjectResponse
				{
					HttpStatusCode = HttpStatusCode.OK,
					VersionId = VersionIdForTest
				};

				mock.Setup(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), CancellationToken.None)).Callback((PutObjectRequest request, CancellationToken token) =>
				{
					UploadedBytesForTest = request.InputStream.ToByteArray();
					if (FailToSave && SystemDataRegistry.Instance.DocManagerParanoidModeEnabled.Value)
					{
						var s3Exception = new AmazonS3Exception("Error making request with Error Code InvalidDigest and Http Status Code BadRequest. No further error information was returned by the service.");
						s3Exception.ErrorCode = "InvalidDigest";
						s3Exception.StatusCode = HttpStatusCode.BadRequest;
						throw s3Exception;
					}
				}).Returns(Task.FromResult(putResponse));
			}

			if (supportDownload)
			{
				void GetResponseCallBack()
				{
					if (UploadedBytesForTest.IsEmpty || FailToSave)
					{
						var memoryStream = new MemoryStream(ImageDataForTest);
						getResponse.ResponseStream = new HashStreamForTest(memoryStream, null, FailToSave ? 0 : ImageDataForTest.Length);
					}
					else
					{
						var responseStream = new MemoryStream(UploadedBytesForTest);
						getResponse.ResponseStream = responseStream;
					}
				}

				mock.Setup(x => x.GetObjectAsync(It.IsAny<string>(), It.IsAny<string>(), CancellationToken.None)).Callback(() => GetResponseCallBack()).Returns(Task.FromResult(getResponse));
				mock.Setup(x => x.GetObjectAsync(It.IsAny<GetObjectRequest>(), CancellationToken.None)).Callback(() => GetResponseCallBack()).Returns(Task.FromResult(getResponse));
			}

			return mock;
		}

		public Mock<IAmazonS3> ClientMock
		{
			get => clientMock ?? (clientMock = CreateClientMock());
			set => clientMock = value;
		}

		Mock<IAmazonS3> clientMock;

		protected override bool DoesS3BucketExist(IAmazonS3 client)
		{
			DoesS3BucketExistInvoked = true;
			return true;
		}

		public bool DoesS3BucketExistInvoked { get; set; }

		public IAmazonS3 GetBaseClient()
		{
			return base.GetClient();
		}

		public AWSPersisterConfig ConfigExposed => config;
	}

	class HashStreamForTest : HashStream
	{
		public HashStreamForTest(Stream baseStream, byte[] expectedHash, long expectedLength) : base(baseStream, expectedHash, expectedLength)
		{
			Algorithm = new HashingWrapperMD5();
		}
	}
}
