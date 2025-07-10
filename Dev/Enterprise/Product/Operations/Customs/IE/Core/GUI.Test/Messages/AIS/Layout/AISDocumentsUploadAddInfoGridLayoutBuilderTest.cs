using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Messaging;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.GUI.Testing
{
	[TestedType(typeof(AISDocumentsUploadAddInfoGridLayoutBuilder<UploadDocumentsSendingAction>))]
	sealed class AISDocumentsUploadAddInfoGridLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<AISDocumentsUploadAddInfoGridLayoutBuilder<UploadDocumentsSendingAction>, UploadDocumentsSendingAction, AISDocumentsUploadAddInfoGridControlBag>
	{
		protected override int ExpectedMaxColumns => 1;

		protected override AISDocumentsUploadAddInfoGridLayoutBuilder<UploadDocumentsSendingAction> GetColumnLayoutBuilderForTesting() => new AISDocumentsUploadAddInfoGridLayoutBuilder<UploadDocumentsSendingAction>();

		public void TestAddInfosGridVisible()
		{
			var entryHeader = Factory.NewWithValidTestData<CusEntryHeader>();
			var uploadDocumentsSendingAction =  new UploadDocumentsSendingAction(entryHeader);
			uploadDocumentsSendingAction.MessageType = AISUploadDocumentsMessageTypeList.Codes.IM483;
			AssertEquals(false, Layout.IsVisible(AISDocumentsUploadAddInfoGridControlBag.Instance.AddInfosGrid, uploadDocumentsSendingAction));
			uploadDocumentsSendingAction.MessageType = AISUploadDocumentsMessageTypeList.Codes.IM446;
			AssertEquals(false, Layout.IsVisible(AISDocumentsUploadAddInfoGridControlBag.Instance.AddInfosGrid, uploadDocumentsSendingAction));
		}

		public void TestAddInfosIM483GridVisible()
		{
			var entryHeader = Factory.NewWithValidTestData<CusEntryHeader>();
			var uploadDocumentsSendingAction = new UploadDocumentsSendingAction(entryHeader);
			uploadDocumentsSendingAction.MessageType = AISUploadDocumentsMessageTypeList.Codes.IM483;
			AssertEquals(true, Layout.IsVisible(AISDocumentsUploadAddInfoGridControlBag.Instance.AddInfosIM483Grid, uploadDocumentsSendingAction));
			uploadDocumentsSendingAction.MessageType = AISUploadDocumentsMessageTypeList.Codes.IM446;
			AssertEquals(true, Layout.IsVisible(AISDocumentsUploadAddInfoGridControlBag.Instance.AddInfosIM483Grid, uploadDocumentsSendingAction));
		}

		PanelLayout Layout => layout ?? (layout = new AISDocumentsUploadAddInfoGridLayout().Layout);
		PanelLayout layout;
	}
}
