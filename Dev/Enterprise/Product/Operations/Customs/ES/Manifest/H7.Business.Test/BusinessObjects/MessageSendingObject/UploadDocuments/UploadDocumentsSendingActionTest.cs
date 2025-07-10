using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	[TestedType(typeof(UploadDocumentsSendingAction))]
	sealed class UploadDocumentsSendingActionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDefaultValue()
		{
			var uploadDocumentsSendingAction = GetNewBusinessObject() as UploadDocumentsSendingAction;
			AssertEquals(uploadDocumentsSendingAction.ClearanceRequested, true);
			AssertEquals(uploadDocumentsSendingAction.Action, DeclarationMessageTypeList.Codes.H7Annexes);
		}

		public void TestClearanceRequestedCaption()
		{
			var uploadDocumentsSendingAction = GetNewBusinessObject() as UploadDocumentsSendingAction;
			var dataForPropertyWithMultipleResourceKey = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(uploadDocumentsSendingAction.ClearanceRequestedInfo, null);
			AssertEquals("Caption", "Clearance Requested", dataForPropertyWithMultipleResourceKey.Caption);
		}

		public void TestG3LocalReferenceNumber()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.G3LocalReferenceNumber = "G3LRN123";

			var messageSendingObject = new UploadDocumentsSendingAction(bill);
			var resourceStringDataAttribute = messageSendingObject.G3LocalReferenceNumberInfo.GetAttribute<ResourceStringDataAttribute>();

			CombineAssertions(() =>
			{
				AssertEquals("Expected value", "G3LRN123", messageSendingObject.G3LocalReferenceNumber);
				AssertEquals("Expection short caption", "LRN (G3)", resourceStringDataAttribute.ShortCaption);
				AssertEquals("Expection caption", "G3 Local Reference Number", resourceStringDataAttribute.Caption);
				AssertEquals("Expection description", "A system-generated local reference number to uniquely identify each single G3 declaration.", resourceStringDataAttribute.FullDescription);
				Assert("Expection readonly", messageSendingObject.G3LocalReferenceNumberInfo.ReadOnly);
			});
		}

		public void TestG3MovementReferenceNumber()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.G3MovementReferenceNumber = "G3MRN123";

			var messageSendingObject = new UploadDocumentsSendingAction(bill);
			var resourceStringDataAttribute = messageSendingObject.G3MovementReferenceNumberInfo.GetAttribute<ResourceStringDataAttribute>();

			CombineAssertions(() =>
			{
				AssertEquals("Expected value", "G3MRN123", messageSendingObject.G3MovementReferenceNumber);
				AssertEquals("Expection short caption", "MRN (G3)", resourceStringDataAttribute.ShortCaption);
				AssertEquals("Expection caption", "G3 Movement Reference Number", resourceStringDataAttribute.Caption);
				AssertEquals("Expection description", "A unique identifier issued by the relevant Customs authority that enables the Customs authority to identify and process your shipment in the customs system.", resourceStringDataAttribute.FullDescription);
				Assert("Expection readonly", messageSendingObject.G3MovementReferenceNumberInfo.ReadOnly);
			});
		}

		public void TestH7MovementReferenceNumber()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.H7MovementReferenceNumber = "H7MRN123";

			var messageSendingObject = new UploadDocumentsSendingAction(bill);
			var resourceStringDataAttribute = messageSendingObject.H7MovementReferenceNumberInfo.GetAttribute<ResourceStringDataAttribute>();

			CombineAssertions(() =>
			{
				AssertEquals("Expected value", "H7MRN123", messageSendingObject.H7MovementReferenceNumber);
				AssertEquals("Expection short caption", "MRN (H7)", resourceStringDataAttribute.ShortCaption);
				AssertEquals("Expection caption", "H7 Movement Reference Number", resourceStringDataAttribute.Caption);
				AssertEquals("Expection description", "A unique identifier issued by the relevant Customs authority that enables the Customs authority to identify and process your shipment in the customs system.", resourceStringDataAttribute.FullDescription);
				Assert("Expection readonly", messageSendingObject.H7MovementReferenceNumberInfo.ReadOnly);
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var testBizObj = header.Bills.AddNew();
			return new UploadDocumentsSendingAction(testBizObj);
		}
	}
}
