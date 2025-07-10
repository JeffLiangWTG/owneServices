using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.Business.Test;

[TestedType(typeof(UploadDocumentsSendingActionFilteredCollectionView<UploadDocumentsSendingAction>))]
public class UploadDocumentsSendingActionFilteredCollectionViewTest : NonPersistentBusinessObjectCollectionViewTestCase<UploadDocumentsSendingActionFilteredCollectionView<UploadDocumentsSendingAction>>
{
	public void TestFilter()
	{
		var collectionViewToTest = GetCollectionToTest();
		var filter1 = new ZQuery(AsycudaBillSchema.ABL_BillNumber, "Test123");
		collectionViewToTest.Load(filter1);
		AssertEquals(1, collectionViewToTest.Count);

		var filter2 = new ZQuery(AsycudaBillSchema.ABL_BillNumber, "###");
		collectionViewToTest.Load(filter2);
		AssertEquals(0, collectionViewToTest.Count);
	}

	protected override UploadDocumentsSendingActionFilteredCollectionView<UploadDocumentsSendingAction> GetCollectionToTest()
	{
		var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
		var bill = manifestHeader.Bills.AddNew();
		bill.ABL_BillNumber = "Test123";
		var requestedDocument = bill.RequestedDocuments.AddNew();
		requestedDocument.CSI_Status = RequestedDocumentStatusList.Codes.RequestOpened;
		var uploadDocumentSendingAction = new UploadDocumentsSendingAction(bill);
		var uploadDocumentsSendingActionCollection = new MessageSendingObjectCollection<UploadDocumentsSendingAction>(Factory)
		{
			uploadDocumentSendingAction
		};
		Factory.Save();

		return new UploadDocumentsSendingActionFilteredCollectionView<UploadDocumentsSendingAction>(uploadDocumentsSendingActionCollection);
	}

	protected override BusinessObject GetNewElementToAddToTheCollection()
	{
		var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
		var bill = manifestHeader.Bills.AddNew();
		var requestedDocument = bill.RequestedDocuments.AddNew();
		requestedDocument.CSI_Status = RequestedDocumentStatusList.Codes.RequestOpened;
		return new UploadDocumentsSendingAction(bill);
	}
}
