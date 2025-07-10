using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.GB.H7.Business.Testing
{
	[TestedType(typeof(MessageSendingObject))]
	sealed class MessageSendingObjectTest : EU.H7.Business.Testing.MessageSendingObjectTest
	{
		public void TestMessageTypeDefaultAndSubStyleDefault_InitialCustomsMessage()
		{
			var bill = Factory.New<AsycudaBill>();
			var messageSendingObject = new MessageSendingObject(bill);
			AssertNullOrEmpty("Pre-req: New bill should not have message status and reco as sending initial customs message", messageSendingObject.MessageStatus);
		}

		public void TestSubStyleValue_UseShipmentTypeSetInBillsGrid()
		{
			var bill = Factory.New<AsycudaBill>();
			bill.ABL_ShipmentType = EntrySubStyleCodeList.Codes.A;
			var messageSendingObject = new MessageSendingObject(bill);

			AssertEquals("Sub style uses shipment type from bills grid", EntrySubStyleCodeList.Codes.A, messageSendingObject.SubStyle);
		}

		public void TestSubStyleValue_ShouldResetByActionValue()
		{
			var bill = Factory.New<AsycudaBill>();
			var messageSendingObject = new MessageSendingObject(bill);

			AssertEquals("Action's default value is NEW", H7EDIMessageTypeList.Codes.NewDeclaration, messageSendingObject.Action);
		}

		public void TestSubStyleCaption()
		{
			var bill = Factory.New<AsycudaBill>();
			var messageSendingObject = new MessageSendingObject(bill);

			var resourceStringDataAttribute = messageSendingObject.SubStyleInfo.GetAttribute<ResourceStringDataAttribute>();
			CombineAssertions(() =>
			{
				AssertEquals("Additional Declaration Type", resourceStringDataAttribute.Caption);
				AssertEquals("Add. Decl. Type", resourceStringDataAttribute.ShortCaption);
				AssertEquals("Add. Decl. Type", resourceStringDataAttribute.MediumCaption);
				AssertEquals("Code identifying both the type of declaration and whether or not the goods have arrived at the goods location.", resourceStringDataAttribute.FullDescription);
			});
		}

		public void TestSubStyleReadOnly()
		{
			var bill = Factory.New<AsycudaBill>();
			var messageSendingObject = new MessageSendingObject(bill);
			AssertEquals("SubStyle is read only", true, messageSendingObject.SubStyleInfo.ReadOnly);
		}

		public void TestEntryType_ReadOnly()
		{
			var sendingObject = GetNewBusinessObject() as MessageSendingObject;
			AssertEquals("EntryType is read only", true, sendingObject.EntryTypeInfo.ReadOnly);
		}

		public void TestAmendmentCodeAndReason_ReadOnly()
		{
			var sendingObject = GetNewBusinessObject() as MessageSendingObject;
			sendingObject.Action = H7EDIMessageTypeList.Codes.NewDeclaration;
			Assert("AmendmentCode is read only", sendingObject.AmendmentReasonCodeInfo.ReadOnly);
			Assert("Amendment Reason is read only", sendingObject.AmendmentInvalidationReasonInfo.ReadOnly);

			sendingObject.Action = H7EDIMessageTypeList.Codes.ArrivalNotification;
			Assert("AmendmentCode is editable", !sendingObject.AmendmentReasonCodeInfo.ReadOnly);
			Assert("Amendment Reason is editable", !sendingObject.AmendmentInvalidationReasonInfo.ReadOnly);

			sendingObject.Action = H7EDIMessageTypeList.Codes.CancelDeclaration;
			Assert("AmendmentCode is editable", !sendingObject.AmendmentReasonCodeInfo.ReadOnly);
			Assert("Amendment Reason is editable", !sendingObject.AmendmentInvalidationReasonInfo.ReadOnly);
		}

		public void TestQueryType_ReadOnly()
		{
			var sendingObject = GetNewBusinessObject() as MessageSendingObject;
			foreach(CodeDescriptionPair action in sendingObject.ActionList)
			{
				sendingObject.Action = action.Code;
				if (action.Code != H7EDIMessageTypeList.Codes.QueryDeclaration)
				{
					Assert("QueryTyp is read only", sendingObject.QueryTypeInfo.ReadOnly);
				}
				else
				{
					Assert("QueryTyp is editable", !sendingObject.QueryTypeInfo.ReadOnly);
				}
			}
		}

		public void TestDefaultAmendmentReasonCode()
		{
			var sendingObject = GetNewBusinessObject() as MessageSendingObject;
			sendingObject.AmendmentReasonCode = "2";
			sendingObject.AmendmentInvalidationReason = "duplicate reason";
			sendingObject.Action = H7EDIMessageTypeList.Codes.ArrivalNotification;
			AssertEquals("16", sendingObject.AmendmentReasonCode);
			AssertEquals("", sendingObject.AmendmentInvalidationReason);

			sendingObject.AmendmentInvalidationReason = "reason";
			sendingObject.Action = H7EDIMessageTypeList.Codes.NewDeclaration;
			AssertEquals("", sendingObject.AmendmentReasonCode);
			AssertEquals("", sendingObject.AmendmentInvalidationReason);
		}

		public void TestDefaultQueryType()
		{
			var sendingObject = GetNewBusinessObject() as MessageSendingObject;
			sendingObject.Action = H7EDIMessageTypeList.Codes.QueryDeclaration;
			AssertEquals(H7QueryTypeList.Codes.MRNSummary, sendingObject.QueryType);

			sendingObject.Action = H7EDIMessageTypeList.Codes.NewDeclaration;
			AssertEquals("", sendingObject.QueryType);
		}

		protected override CodeDescriptionPairList ExpectedActionList => new H7EDIMessageTypeList();

		protected override CodeDescriptionPairList ExpectedSubStyleList => new EntrySubStyleListImport();

		protected override CodeDescriptionPairList ExpectedAmendmentReasonCodeList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(AmendmentCancellationReasonCode.Codes.C_NotRequired, AmendmentCancellationReasonCode.Descriptions.C_NotRequired);
				result.AddPair(AmendmentCancellationReasonCode.Codes.C_Duplicate, AmendmentCancellationReasonCode.Descriptions.C_Duplicate);
				result.AddPair(AmendmentCancellationReasonCode.Codes.C_Other, AmendmentCancellationReasonCode.Descriptions.C_Other);
				result.AddPair(AmendmentCancellationReasonCode.Codes.C_GoodsPresentationNotice, AmendmentCancellationReasonCode.Descriptions.C_GoodsPresentationNotice);
				return result;
			}
		}

		protected override CodeDescriptionPairList ExpectedQueryTypeList => new H7QueryTypeList();

		protected override string AmendmentInvalidationReasonCaption => "Amendment Reason";

		protected override EU.H7.Business.AsycudaBill GetMessageSendingObjectParentBill()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			return bill;
		}

		protected override EU.H7.Business.MessageSendingObject GetMessageSendingObject(EU.H7.Business.AsycudaBill bill) => new MessageSendingObject(bill);
	}
}
