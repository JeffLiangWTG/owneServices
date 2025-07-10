using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.Business.Test
{
	[TestedType(typeof(UploadDocumentsSendingAction))]
	class UploadDocumentsSendingActionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestMovementReference()
		{
			testBizObj.MovementReferenceNumber = "MRN001";
			AssertEquals("MovementReference", "MRN001", uploadDocumentsSendingAction.MovementReferenceNumber);
		}

		public void TestMovementReference_Caption()
		{
			var resData = ZPropertyInfoExtensions.GetAttribute<ResourceStringDataAttribute>(uploadDocumentsSendingAction.MovementReferenceNumberInfo);
			AssertEquals("MovementReferenceInfo full description", "Movement Reference Number", resData.FullDescription);
			AssertEquals("MovementReferenceInfo Caption", "MRN", resData.Caption);
		}

		public void TestAction_ReadOnly()
		{
			Assert(uploadDocumentsSendingAction.ActionInfo.ReadOnly);
		}

		public void TestAddInfoCollection()
		{
			var requestedDocument = testBizObj.RequestedDocuments.AddNew();
			requestedDocument.CSI_Status = EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.RequestOpened;

			AssertEquals("Expected collection size", 1, uploadDocumentsSendingAction.AddInfoCollection.Count);
		}

		protected override BusinessObject GetNewBusinessObject() => uploadDocumentsSendingAction;

		protected override void SetUp()
		{
			base.SetUp();

			var header = Factory.New<AsycudaManifestHeader>();
			testBizObj = header.Bills.AddNew();
			uploadDocumentsSendingAction = new UploadDocumentsSendingAction(testBizObj);
		}

		AsycudaBill testBizObj;
		UploadDocumentsSendingAction uploadDocumentsSendingAction;
	}
}
