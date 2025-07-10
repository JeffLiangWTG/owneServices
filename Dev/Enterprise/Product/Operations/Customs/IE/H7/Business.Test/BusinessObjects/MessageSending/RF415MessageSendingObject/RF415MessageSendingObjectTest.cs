using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.IE.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.H7.Business.Testing
{
	[TestedType(typeof(RF415MessageSendingObject))]
	sealed class RF415MessageSendingObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestShouldSendDefaults()
		{
			var messageSendingObject = (RF415MessageSendingObject)GetNewBusinessObject();
			Assert(messageSendingObject.ShouldSend);
		}

		public void TestMovementReferenceNumber()
		{
			var messageSendingObject = (RF415MessageSendingObject)GetNewBusinessObject();
			AssertEquals(bill.MovementReferenceNumber, messageSendingObject.MovementReferenceNumber);
		}

		public void TestBillNumber()
		{
			var messageSendingObject = (RF415MessageSendingObject)GetNewBusinessObject();
			AssertEquals(bill.ABL_BillNumber, messageSendingObject.BillNumber);
		}

		public void TestRefundTypeDefaults()
		{
			var messageSendingObject = (RF415MessageSendingObject)GetNewBusinessObject();
			AssertEquals(RF415RefundTypes.Codes.Repayment, messageSendingObject.RefundType);
		}

		public void TestSenderType()
		{
			var messageSendingObject = (RF415MessageSendingObject)GetNewBusinessObject();
			AssertType<IEMessageSender>(messageSendingObject.CreateSender());
		}

		public void TestDocumentSendingObjectCollection()
		{
			var messageSendingObject = (RF415MessageSendingObject)GetNewBusinessObject();
			AssertType<RF415DocumentSendingObjectCollection>(messageSendingObject.DocumentSendingObjectCollection);
		}

		public void TestLookups()
		{
			var messageSendingObject = (RF415MessageSendingObject)GetNewBusinessObject();
			AssertType<RF415MessageSendingObjectLookups>(messageSendingObject.Lookups);
		}

		public void TestAction()
		{
			var messageSendingObject = (RF415MessageSendingObject)GetNewBusinessObject();
			AssertEquals("Action should always be 'F15'", AISOutgoingMessageTypeList.Codes.ApplicationForRemissionOfCustomsDebtF15, messageSendingObject.Action);
		}

		public void TestMessageSendingObjectValidation()
		{
			var messageSendingObject = (RF415MessageSendingObject)GetNewBusinessObject();

			CombineAssertions("Validation should be applied when Send? is checked", () =>
			{
				AssertHasErrors(messageSendingObject.OfficeOfDebtInfo);
				AssertHasErrors(messageSendingObject.LegalBasisInfo);
				AssertHasErrors(messageSendingObject.DescriptionOfGroundsInfo);
				AssertHasErrors(messageSendingObject.AmountInfo);
			});

			messageSendingObject.ShouldSend = false;

			CombineAssertions("No validation when Send? is not checked", () =>
			{
				AssertNoErrors(messageSendingObject.OfficeOfDebtInfo);
				AssertNoErrors(messageSendingObject.LegalBasisInfo);
				AssertNoErrors(messageSendingObject.DescriptionOfGroundsInfo);
				AssertNoErrors(messageSendingObject.AmountInfo);
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new RF415MessageSendingObject(bill);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.New<AsycudaManifestHeader>();
			bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "B1234";
			var expectedMovementReferenceNumber = "MRN1234";
			var cusEntryNumber = CusEntryNumber.New(bill, CusEntryNumberTypes.Standard.MovementReferenceNumber, bill.Header.AMA_RN_NKCountry);
			cusEntryNumber.CE_EntryNum = expectedMovementReferenceNumber;
		}

		AsycudaBill bill;
	}
}
