using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ES.Manifest.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	[TestedType(typeof(G3MessageSendingObjectParent))]
	class G3MessageSendingObjectParentTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new G3MessageSendingObjectParent(Factory.New<AsycudaManifestHeader>());
		}

		public void TestSendingObjectsCollectionForCreateMessage()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var sendingObjectParent = new G3MessageSendingObjectParent(header);
			var sendingObjects = sendingObjectParent.SendingObjectsCollection;

			CombineAssertions(() =>
			{
				AssertEquals("Expected one sending object", 1, sendingObjects.Count);
				AssertEquals("Bill", bill, sendingObjects[0].Bill);
				AssertEquals("Action", G3MessageTypes.Codes.G3Declaration, sendingObjects[0].Action);
			});
		}

		public void TestSendingObjectsCollectionForRevokeMessage_WhenBillDoesNotHaveAcceptedG3DMessage_ShouldNotDisplay()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.Bills.AddNew();

			var sendingObjectParent = new G3MessageSendingObjectParent(header, true);
			var sendingObjects = sendingObjectParent.SendingObjectsCollection;

			AssertEquals(0, sendingObjects.Count);
		}

		public void TestSendingObjectsCollectionForRevokeMessage_WhenBillHasAcceptedG3DMessage_ShouldDisplay()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.G3MRNToRevoke = "MRN000123";
			var bill = header.Bills.AddNew();

			var cusEntryNum = bill.CustomsEntryNumbers.AddNew();
			cusEntryNum.CE_EntryNum = "MRN000123";
			cusEntryNum.CE_EntryType = "MRN";
			cusEntryNum.CE_EntryLineReference = "G3";

			var sendingObjectParent = new G3MessageSendingObjectParent(header, true);
			var sendingObjects = sendingObjectParent.SendingObjectsCollection;

			CombineAssertions(() =>
			{
				AssertEquals("Expected one sending object", 1, sendingObjects.Count);
				AssertEquals("Bill", bill, sendingObjects[0].Bill);
				AssertEquals("Action", G3MessageTypes.Codes.G3Revoke, sendingObjects[0].Action);
			});
		}

		public void TestOverrideRevokeReason()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.Bills.AddNew();
			header.Bills.AddNew();

			var parent = new G3MessageSendingObjectParent(header);
			var sendingObject1 = parent.SendingObjectsCollection[0];
			var sendingObject2 = parent.SendingObjectsCollection[1];
			sendingObject1.RevokeReason = ESH7G3RevokeReasonList.Codes.G3003;

			CombineAssertions(() =>
			{
				parent.OverrideDefaultRevokeReason = true;
				parent.RevokeReason = ESH7G3RevokeReasonList.Codes.G3002;
				parent.RevokeReasonDescription = "Test Revoke Reason description haha";
				AssertEquals("Revoke Reason of object 1 should be overriden", ESH7G3RevokeReasonList.Codes.G3002, sendingObject1.RevokeReason);
				AssertEquals("Revoke Reason Description of object 1 should be overriden", "Test Revoke Reason description haha", sendingObject1.RevokeReasonDescription);
				AssertEquals("Revoke Reason of object 2 should be overriden", ESH7G3RevokeReasonList.Codes.G3002, sendingObject2.RevokeReason);
				AssertEquals("Revoke Reason Description of object 2 should be overriden", "Test Revoke Reason description haha", sendingObject2.RevokeReasonDescription);

				parent.OverrideDefaultRevokeReason = false;
				AssertEquals("Revoke Reason of object 1 should be set to default", ESH7G3RevokeReasonList.Codes.G3001, sendingObject1.RevokeReason);
				AssertEquals("Revoke Reason description of object 1 should be set to default", ZString.Empty, sendingObject1.RevokeReasonDescription);
				AssertEquals("Revoke Reason of object 2 should be set to default", ESH7G3RevokeReasonList.Codes.G3001, sendingObject2.RevokeReason);
				AssertEquals("Revoke Reason description of object 2 should be set to default", ZString.Empty, sendingObject2.RevokeReasonDescription);
			});
		}

		public void TestRevokeReasonList()
		{
			var sendingObjectParent = GetNewBusinessObject() as G3MessageSendingObjectParent;
			AssertContainsExactElementsInAnyOrder(ExpectedRevokeReasonList, sendingObjectParent.RevokeReasonList);
		}

		public void TestHeader()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.Bills.AddNew();

			var sendingObjectParent = new G3MessageSendingObjectParent(header);
			AssertSame("Expected header", header, sendingObjectParent.Header);
		}

		public void TestSelectedSendingObjects()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.Bills.AddNew();
			header.Bills.AddNew();
			header.Bills.AddNew();

			var parent = new G3MessageSendingObjectParent(header);

			var sendingObject1 = parent.SendingObjectsCollection[0];
			var sendingObject2 = parent.SendingObjectsCollection[1];
			parent.SendingObjectsCollection[2].ShouldSend = false;

			var selectedSendingObjects = parent.SelectedSendingObjects;

			CombineAssertions(() =>
			{
				AssertEquals("Expected two selected sending objects", 2, selectedSendingObjects.Count());
				AssertSame("Sending object 1", sendingObject1, selectedSendingObjects.FirstOrDefault());
				AssertSame("Sending object 2", sendingObject2, selectedSendingObjects.LastOrDefault());
			});
		}

		public void TestSendingObjects_AdditionalWarnings()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "123+456";

			var sendingObjectParent = new G3MessageSendingObjectParent(header);
			var additionalWarnings = sendingObjectParent.AdditionalWarnings;

			AssertEquals("Expected one sending object", "Bill Number: When ES Customs processes G3 messages, non alphanumeric characters will be ignored, and lowercase letters will be accepted but will be converted to uppercase. Eg: Test / 00-1* will be converted and recorded as TEST001.", additionalWarnings);
		}

		CodeDescriptionPairList ExpectedRevokeReasonList
		{
			get
			{
				return new CodeDescriptionPairList
				{
					new CodeDescriptionPair("G3001", "Shipment not arrived"),
					new CodeDescriptionPair("G3002", "Shipment duplicated"),
					new CodeDescriptionPair("G3003", "Shipment type rectification"),
				};
			}
		}
	}
}
