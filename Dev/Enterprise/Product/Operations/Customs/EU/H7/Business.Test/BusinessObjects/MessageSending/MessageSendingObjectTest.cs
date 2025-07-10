using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.Business.Testing
{
	[TestedType(typeof(MessageSendingObject))]
	public abstract class MessageSendingObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestBillNumber()
		{
			var sendingObject = GetNewBusinessObject() as MessageSendingObject;
			AssertEquals("MessageSendingObject's bill number should be equal to ABL_BillNumber", "BILL1234", sendingObject.BillNumber);
		}

		public void TestLocalReferenceNumber()
		{
			var sendingObject = GetNewBusinessObject() as MessageSendingObject;
			AssertEquals("MessageSendingObject's LRN should be equal to LocalReferenceNumber of partent bill", "1234", sendingObject.LocalReferenceNumber);
		}

		public void TestMessageStatus()
		{
			var sendingObject = GetNewBusinessObject() as MessageSendingObject;
			AssertEquals("MessageSendingObject's message status should be equal to message status of partent bill", "TS1", sendingObject.MessageStatus);
		}

		public void TestEntryStatus()
		{
			var sendingObject = GetNewBusinessObject() as MessageSendingObject;
			AssertEquals("MessageSendingObject's entry status should be equal to cargo status of partent bill", "TS2", sendingObject.EntryStatus);
		}

		public void TestSubStyle()
		{
			var sendingObject = GetNewBusinessObject() as MessageSendingObject;
			AssertEquals("MessageSendingObject's SubStyle should be equal to Substyle of partent bill", "A", sendingObject.SubStyle);
		}

		public void TestMRN()
		{
			var sendingObject = GetNewBusinessObject() as MessageSendingObject;
			AssertEquals("MessageSendingObject's MRN should be equal to MRN of partent bill", "999", sendingObject.MRN);
		}

		public void TestCustomsStatus()
		{
			var sendingObject = GetNewBusinessObject() as MessageSendingObject;
			AssertEquals("MessageSendingObject's Custom Status should be equal to Bill Status of partent bill", "TS3", sendingObject.CustomsStatus);
		}

		public void TestReferenceNumber()
		{
			var sendingObject = GetNewBusinessObject() as MessageSendingObject;
			AssertEquals("MessageSendingObject's Reference Number should be equal to Bill UCR Number", "UCR001", sendingObject.ReferenceNumber);
		}

		public void TestPropertiesMaxLength()
		{
			var sendingObject = GetNewBusinessObject() as MessageSendingObject;
			AssertEquals($"Max length of AmendmentInvalidationReason is {ExpectedMaxLengthOfAmendmentInvalidationReason}", ExpectedMaxLengthOfAmendmentInvalidationReason, sendingObject.AmendmentInvalidationReasonInfo.MaxLength);
			AssertEquals("Max length of AmendmentReasonCode is 2", 2, sendingObject.AmendmentReasonCodeInfo.MaxLength);
			AssertEquals("Max length of QueryType is 512", 512, sendingObject.QueryTypeInfo.MaxLength);
			AssertEquals("Max length of OperationCode is 1", 1, sendingObject.OperationCodeInfo.MaxLength);
			AssertEquals("Max length of RevokeReasonDescription is 512", 512, sendingObject.RevokeReasonDescriptionInfo.MaxLength);
		}

		protected virtual int ExpectedMaxLengthOfAmendmentInvalidationReason => 512;

		public void TestActionList()
		{
			var sendingObject = GetNewBusinessObject() as MessageSendingObject;
			AssertContainsExactElementsInAnyOrder(ExpectedActionList, sendingObject.ActionList);
		}

		public void TestSubStyleList()
		{
			var sendingObject = GetNewBusinessObject() as MessageSendingObject;
			AssertContainsExactElementsInAnyOrder(ExpectedSubStyleList, sendingObject.SubStyleList);
		}

		public void TestAmendmentReasonCodeList()
		{
			var sendingObject = GetNewBusinessObject() as MessageSendingObject;
			AssertContainsExactElementsInAnyOrder(ExpectedAmendmentReasonCodeList, sendingObject.AmendmentReasonCodeList);
		}

		public void TestQueryTypeList()
		{
			var sendingObject = GetNewBusinessObject() as MessageSendingObject;
			AssertContainsExactElementsInAnyOrder(ExpectedQueryTypeList, sendingObject.QueryTypeList);
		}

		public void TestCaptionResourceString()
		{
			var sendingObject = GetNewBusinessObject() as MessageSendingObject;
			CombineAssertions(() =>
			{
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(sendingObject.AmendmentInvalidationReasonInfo, (string[])null, AmendmentInvalidationReasonCaption);
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(sendingObject.AmendmentReasonCodeInfo, (string[])null, AmendmentReasonCodeCaption);
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(sendingObject.QueryTypeInfo, (string[])null, QueryTypeCaption);
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(sendingObject.OperationCodeInfo, (string[])null, OperationCodeCaption);
			});
		}

		protected virtual string AmendmentInvalidationReasonCaption => "Amendment / Invalidation Reason";

		protected virtual string AmendmentReasonCodeCaption => "Amendment Reason Code";

		protected virtual string QueryTypeCaption => "Query By Reference";

		protected virtual string OperationCodeCaption => "Operation Code";

		public void TestSubStyle_ReadOnly()
		{
			var sendingObject = GetNewBusinessObject() as MessageSendingObject;
			AssertEquals("SubStyle is read only", true, sendingObject.SubStyleInfo.ReadOnly);
		}

		public void TestMRN_ReadOnly()
		{
			var sendingObject = GetNewBusinessObject() as MessageSendingObject;
			AssertEquals("MRN is read only", true, sendingObject.MRNInfo.ReadOnly);
		}

		public void TestCustomStatus_ReadOnly()
		{
			var sendingObject = GetNewBusinessObject() as MessageSendingObject;
			AssertEquals("Custom Status is read only", true, sendingObject.CustomsStatusInfo.ReadOnly);
		}

		public void TestReferenceNumber_ReadOnly()
		{
			var sendingObject = GetNewBusinessObject() as MessageSendingObject;
			AssertEquals("Reference Number is read only", true, sendingObject.ReferenceNumberInfo.ReadOnly);
		}

		public void TestMessageStatus_ReadOnly()
		{
			var sendingObject = GetNewBusinessObject() as MessageSendingObject;
			AssertEquals("Message Status is read only", true, sendingObject.MessageStatusInfo.ReadOnly);
		}

		public void TestActionForSendingCustomDeclaration()
		{
			var sendingObject = GetNewBusinessObject() as MessageSendingObject;
			AssertEquals(ExpectedActionForSendingCustomsDeclaration, sendingObject.ActionForSendingCustomsDeclaration);
		}

		public virtual void TestIsAmendmentAction()
		{
			var sendingObject = GetNewBusinessObject() as MessageSendingObject;
			Assert(!sendingObject.IsAmendmentAction);
		}

		public virtual void TestIsCancellationAction()
		{
			var sendingObject = GetNewBusinessObject() as MessageSendingObject;
			Assert(!sendingObject.IsCancellationAction);
		}

		public void TestMessageSender()
		{
			var sendingObject = GetNewBusinessObject() as MessageSendingObject;

			if (ExpectedMessageSenderType == null)
			{
				AssertNull("Please override 'ExpectedMessageSenderType' to provide the correct type of message sender", sendingObject.CreateSender());
			}
			else
			{
				AssertType($"Expect message sender to be instance of type {ExpectedMessageSenderType.Name}", ExpectedMessageSenderType, sendingObject.CreateSender());
			}
		}

		#region Implement

		protected abstract CodeDescriptionPairList ExpectedActionList { get; }

		protected abstract CodeDescriptionPairList ExpectedSubStyleList { get; }

		protected virtual CodeDescriptionPairList ExpectedAmendmentReasonCodeList => new();

		protected virtual CodeDescriptionPairList ExpectedQueryTypeList => new ();

		protected virtual CodeDescriptionPairList ExpectedOperationCodeList => new();

		protected virtual ZString ExpectedActionForSendingCustomsDeclaration => ZString.Empty;

		protected virtual Type ExpectedMessageSenderType => null;

		protected sealed override BusinessObject GetNewBusinessObject()
		{
			var bill = GetMessageSendingObjectParentBill();
			bill.ABL_BillNumber = "BILL1234";
			bill.LocalReferenceNumber = "1234";
			bill.ABL_MessageStatus = "TS1";
			bill.ABL_CargoStatus = "TS2";
			bill.ABL_ShipmentType = "A";
			bill.MovementReferenceNumber = "999";
			bill.ABL_BillStatus = "TS3";
			bill.ABL_UCRNumber = "UCR001";

			return GetMessageSendingObject(bill);
		}

		protected abstract AsycudaBill GetMessageSendingObjectParentBill();

		protected abstract MessageSendingObject GetMessageSendingObject(AsycudaBill bill);

		#endregion
	}
}
