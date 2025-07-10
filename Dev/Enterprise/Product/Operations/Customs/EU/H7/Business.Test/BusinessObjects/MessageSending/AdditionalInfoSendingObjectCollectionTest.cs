using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.Business.Test
{
	[TestedType(typeof(AdditionalInfoSendingObjectCollection))]
	class AdditionalInfoSendingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<AdditionalInfoSendingObjectCollection>
	{
		public void TestMaxCount()
		{
			AssertEquals("Should have set MaxCount.", 99, ((ISupportMaxCountValidation)Collection).MaxCountValidator.MaxCount);
		}

		public void TestLoadElements()
		{
			var anotherRequestedDocument = testBizObj.RequestedDocuments.AddNew();
			anotherRequestedDocument.CSI_Status = EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.RequestCancelled;
			anotherRequestedDocument.CSI_Code = "ADD2";

			Collection.LoadElements();
			AssertEquals("Count", 1, Collection.Count);
			AssertEquals("DocumentType", "ADD1", Collection[0].DocumentType);

			anotherRequestedDocument.CSI_Status = EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.RequestOpened;
			Collection.LoadElements();
			AssertEquals("ADD2 should be included as status changes to OPE", 2, Collection.Count);
			AssertEquals("ADD2 should be included as status changes to OPE", true, Collection.Cast<AdditionalInfoSendingObject>().Any(info => info.DocumentType == "ADD2"));
		}

		public void TestComplementaryInformationIsSupplied()
		{
			var anotherRequestedDocument = testBizObj.RequestedDocuments.AddNew();
			anotherRequestedDocument.CSI_Status = EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.RequestOpened;
			anotherRequestedDocument.CSI_Code = "ADD2";
			anotherRequestedDocument.RequestInformation = "ExtraInfo";

			Collection.LoadElements();
			AssertEquals("ComplementaryInformation", "ExtraInfo", Collection[1].DocumentInformation);
		}

		protected override AdditionalInfoSendingObjectCollection GetCollectionToTest() =>
			new AdditionalInfoSendingObjectCollection(testBizObj, parentObject);

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var bill = testBizObj;
			var newRequestedDocument = bill.RequestedDocuments.AddNew();
			return new AdditionalInfoSendingObject(bill, null, newRequestedDocument);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var header = Factory.New<AsycudaManifestHeader>();
			testBizObj = header.Bills.AddNew();
			requestedDocument = testBizObj.RequestedDocuments.AddNew();
			requestedDocument.CSI_Code = "ADD1";
			requestedDocument.CSI_Status =
				EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.RequestOpened;
			parentObject = new UploadDocumentsSendingAction(testBizObj);
		}

		EU.Business.RequestedDocument requestedDocument;
		AsycudaBill testBizObj;
		UploadDocumentsSendingAction parentObject;
	}
}
