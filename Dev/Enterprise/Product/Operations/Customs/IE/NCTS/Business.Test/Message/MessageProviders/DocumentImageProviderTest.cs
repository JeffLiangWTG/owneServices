using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Moq;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	sealed class DocumentImageProviderTest : DataProviderTestCase<DocumentImageProvider>
	{
		public void TestSize()
		{
			AssertEquals("Size", "12.5MB", Provider.Size);
		}

		public void TestSize_InBytes()
		{
			var mock = new Mock<IeDoc>();
			mock.Setup(o => o.FileSizeInMB).Returns(0.00001);
			var provider = new DocumentImageProvider(mock.Object);
			AssertEquals("0.00001MB=0.01024KB=10.486B", "10.486B", provider.Size);
		}

		public void TestSize_InKB()
		{
			var mock = new Mock<IeDoc>();
			mock.Setup(o => o.FileSizeInMB).Returns(0.01);
			var provider = new DocumentImageProvider(mock.Object);
			AssertEquals("0.01MB=10.24KB", "10.24KB", provider.Size);
		}

		public void TestSize_InGB()
		{
			var mock = new Mock<IeDoc>();
			mock.Setup(o => o.FileSizeInMB).Returns(20480);
			var provider = new DocumentImageProvider(mock.Object);
			AssertEquals("20480MB=20GB", "20GB", provider.Size);
		}

		protected override DocumentImageProvider GetProvider()
		{
			var mock = new Mock<IeDoc>();
			mock.Setup(o => o.FileSizeInMB).Returns(12.5);
			mock.Setup(o => o.ImageData).Returns(new byte[1024]);
			return new DocumentImageProvider(mock.Object);
		}
	}
}
