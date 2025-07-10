using System;
using System.IO;
using System.Net.Http;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.DocumentScanning.Integration;
using Moq;
using Moq.Protected;

namespace Enterprise.DocumentScanning.Business.Test
{
	public static class ExternalPersisterProviderTestHelper
	{
		public static IDisposable MockExternalPersisterForS3(AWSPersister persister)
		{
			var persisterProviderMock = new Mock<IExternalPersisterProvider>();
			persisterProviderMock.Setup(p => p.GetExternalPersister(Core.Constants.EDocsStorageProviders.Code.S3)).Returns(persister);

			return ObjectFactory.Substitute(persisterProviderMock.Object);
		}

		public static IDisposable MockExternalPersisterForS3(bool supportUpload, bool supportDownload, bool disableCheckAllowWriteToExternalStorage = false)
		{
			var persisterProviderMock = new Mock<IExternalPersisterProvider>();
			var persister = new AWSPersisterForTest(supportUpload, supportDownload);
			persisterProviderMock.Setup(p => p.GetExternalPersister(Core.Constants.EDocsStorageProviders.Code.S3)).Returns(persister);
			if (disableCheckAllowWriteToExternalStorage)
			{
				persister.DisableCheckCanWriteToExternalStoarge();
			}

			return ObjectFactory.Substitute(persisterProviderMock.Object);
		}

		public static IDisposable MockExternalPersisterForS3WithThrowAmazonS3Exception()
		{
			var persisterMock = new Mock<IExternalPersister>();
			var exception = new ExternalStorageException("There was an error while accessing the document.", Core.Constants.EDocsStorageProviders.Code.S3, null);
			persisterMock.Setup(p => p.RetrieveStream(It.IsAny<ZGuid>())).Throws(exception);
			persisterMock.Setup(p => p.Delete(It.IsAny<ZGuid>())).Throws(exception);
			persisterMock.Setup(p => p.SaveStream(It.IsAny<MemoryStream>(), It.IsAny<ZGuid>())).Throws(exception);
			persisterMock.Setup(p => p.AllowWrite).Returns(true);

			var persisterProviderMock = new Mock<IExternalPersisterProvider>();
			persisterProviderMock.Setup(p => p.GetExternalPersister(Core.Constants.EDocsStorageProviders.Code.S3)).Returns(persisterMock.Object);

			return ObjectFactory.Substitute(persisterProviderMock.Object);
		}

		public static IDisposable MockExternalPersisterForExternalStorageObjectNotFoundException()
		{
			var persisterMock = new Mock<IExternalPersister>();
			var exception = new ExternalStorageObjectNotFoundException(Guid.NewGuid().ToString(), "Document doesn't exist in external storage.", Core.Constants.EDocsStorageProviders.Code.S3, null);
			persisterMock.Setup(p => p.RetrieveStream(It.IsAny<ZGuid>())).Throws(exception);
			var persisterProviderMock = new Mock<IExternalPersisterProvider>();
			persisterProviderMock.Setup(p => p.GetExternalPersister(Core.Constants.EDocsStorageProviders.Code.S3)).Returns(persisterMock.Object);
			return ObjectFactory.Substitute(persisterProviderMock.Object);
		}

		public static IDisposable MockExternalPersisterForS3WithSpecifiedHttpClient(HttpClient httpClient)
		{
			var persisterMock = new Mock<AWSPersisterForTest>(false, true);
			persisterMock.Protected().Setup<HttpClient>("GetHttpClient").Returns(httpClient);

			var persisterProviderMock = new Mock<IExternalPersisterProvider>();
			persisterProviderMock.Setup(p => p.GetExternalPersister(Core.Constants.EDocsStorageProviders.Code.S3)).Returns(persisterMock.Object);

			return ObjectFactory.Substitute(persisterProviderMock.Object);
		}

		public static void CleanMocks() => ObjectFactory.DisposeSubstitutions();
	}
}
