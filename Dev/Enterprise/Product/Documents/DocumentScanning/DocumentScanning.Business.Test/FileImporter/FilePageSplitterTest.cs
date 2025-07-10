using System.Linq;
using Enterprise.DocumentEngine.PreviewableDocument;
using Moq;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.Test
{
	class FilePageSplitterTests : TestCase
	{
		public void TestUsesDocumentResultWhenCoverSheetIsTrue()
		{
			var document = new Mock<IPreviewableDocument>();
			document.Setup(m => m.NumberOfPages).Returns(5);

			var splitter = new FilePageSplitter(true, "SHP", "DOC");
			Assert("They should all use DocumentResult since IsCoverSheet is true", splitter.SplitFile(document.Object).All(s => !s.UseBarcodeType));

			splitter = new FilePageSplitter(false, "SHP", "DOC");
			Assert("They should all use BaseBarcode since IsCoverSheet is false", splitter.SplitFile(document.Object).All(s => s.UseBarcodeType));
		}
	}
}
