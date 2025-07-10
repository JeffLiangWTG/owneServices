using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	[TestedType(typeof(DocumentRequestSendingActionParent<DocumentRequestSendingAction>))]
	class DocumentRequestSendingActionParentTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new DocumentRequestSendingActionParent<DocumentRequestSendingAction>(Factory.New<AsycudaManifestHeader>());
		}

		public void TestSendingObjectsCollection()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "Bill1";
			var cusEntryNumber = bill.CustomsEntryNumbers.AddNew();
			cusEntryNumber.CE_EntryType = "CLR";
			cusEntryNumber.CE_EntryLineReference = "H7";
			cusEntryNumber.CE_EntryNum = "12345";
			bill.H7MovementReferenceNumber = "ABCD";
			bill.DocManagerInfo().AddFileOrDocument(new byte[1], "ABCD_H7_AEAT_CLR.pdf", "CIV");

			var bill2 = header.Bills.AddNew();
			bill2.ABL_BillNumber = "Bill2";
			var cusEntryNumber2 = bill2.CustomsEntryNumbers.AddNew();
			cusEntryNumber2.CE_EntryType = "CLR";
			cusEntryNumber2.CE_EntryLineReference = "H7";
			cusEntryNumber2.CE_EntryNum = "12345";
			bill2.H7MovementReferenceNumber = "BCDE";

			var bill3 = header.Bills.AddNew();
			bill3.ABL_BillNumber = "Bill3";

			var parent = new DocumentRequestSendingActionParent<DocumentRequestSendingActionForTest>(header);
			var sendingObjectCollection = parent.SendingObjectsCollection;
			var sendingObject = sendingObjectCollection.Single() as DocumentRequestSendingActionForTest;

			CombineAssertions(() =>
			{
				AssertType<DocumentRequestSendingActionForTest>(sendingObject);
				AssertEquals("Bill2", sendingObject.BillNumber);
			});
		}

		class DocumentRequestSendingActionForTest : DocumentRequestSendingAction
		{
			public DocumentRequestSendingActionForTest(AsycudaBill bill) : base(bill)
			{
			}
		}
	}
}
