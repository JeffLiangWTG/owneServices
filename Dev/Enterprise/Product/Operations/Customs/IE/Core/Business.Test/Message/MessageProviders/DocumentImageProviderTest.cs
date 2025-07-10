using System;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Moq;

namespace Enterprise.Customs.IE.Business.Testing
{
	class DocumentImageProviderTest : DataProviderTestCase<DocumentImageProvider>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("Document missing", () => new DocumentImageProvider(null));
			});
		}

		public void TestId()
		{
			AssertNotNullOrEmpty("Id", Provider.Id);
		}

		public void TestFilename()
		{
			AssertEquals("Filename", fileName, Provider.Filename);
		}

		public void TestFileType()
		{
			AssertEquals("FileType", docType, Provider.FileType);
		}

		public void TestSize()
		{
			AssertEquals("Size", "13107200", Provider.Size);
		}

		public void TestDocument()
		{
			AssertEquals("Document should return eDoc UniqueKey.", uniqueKey.ToGuid().ToByteArray(), Provider.Document);
		}

		protected override DocumentImageProvider GetProvider()
		{
			var mock = new Mock<IeDoc>();
			mock.Setup(o => o.UniqueKey).Returns(uniqueKey);
			mock.Setup(o => o.FileName).Returns(fileName);
			mock.Setup(o => o.DocType).Returns(docType);
			mock.Setup(o => o.FileSizeInMB).Returns(fileSizeInMB);
			mock.Setup(o => o.ImageData).Returns(new ZBlob(new byte[5]));

			return new DocumentImageProvider(mock.Object);
		}

		readonly ZGuid uniqueKey = ZGuid.NewZGuid();
		readonly ZString fileName = "Test file.docx";
		readonly ZDecimal fileSizeInMB = 12.5;
		readonly ZString docType = "DOC";
	}
}
