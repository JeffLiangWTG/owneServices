using System;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.IE.Messaging;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.IE.H7.Business.Testing
{
	[TestedType(typeof(MessageSendingObject))]
	sealed class MessageSendingObjectTest : EU.H7.Business.Testing.MessageSendingObjectTest
	{
		public void TestMessageTypeDefaultAndSubStyleDefault_InitialCustomsMessage()
		{
			var bill = Factory.New<AsycudaBill>();
			var messageSendingObject = new MessageSendingObject(bill);

			AssertNullOrEmpty("Pre-req: New bill should not have message status and reco as sending initial customs message", messageSendingObject.MessageStatus);

			AssertEquals("Message type should default to '415' when sending initial customs message", AISOutgoingMessageTypeList.Codes.CustomsDeclaration, messageSendingObject.Action);
			AssertEquals("Message sub style should defailt to 'A' when message type '415' is selected", SubStyleCodeList.Codes.NormalDeclaration, messageSendingObject.SubStyle);
		}

		public void TestPersistSubStyleByBillShipmentType()
		{
			var bill = Factory.NewWithValidTestData<AsycudaBill>();
			bill.ABL_MessageStatus = AISOutgoingMessageTypeList.Codes.AmendmentRequest;

			var messageSendingObject = new MessageSendingObject(bill);
			messageSendingObject.SubStyle = SubStyleCodeList.Codes.PreliminaryDeclaration;

			AssertEquals("Sub style is mapped to ABL_ShipmentType", SubStyleCodeList.Codes.PreliminaryDeclaration, bill.ABL_ShipmentType);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedBill = newFactory.Load<AsycudaBill>(bill.PK);
			var messageSendingObject2 = new MessageSendingObject(loadedBill);

			CombineAssertions("Sub style is persisted by ABL_ShipmentType", () =>
			{
				AssertEquals("ABL_ShipmentType", SubStyleCodeList.Codes.PreliminaryDeclaration, loadedBill.ABL_ShipmentType);
				AssertEquals("SubStyle", SubStyleCodeList.Codes.PreliminaryDeclaration, messageSendingObject2.SubStyle);
			});
		}

		public void TestSubStyleValue_ShouldResetByActionValue()
		{
			var bill = Factory.New<AsycudaBill>();
			var messageSendingObject = new MessageSendingObject(bill);

			AssertEquals("Pre-req: Message type should default to '415'", AISOutgoingMessageTypeList.Codes.CustomsDeclaration, messageSendingObject.Action);
			AssertEquals("Pre-req: sub style should default to 'A'", SubStyleCodeList.Codes.NormalDeclaration, messageSendingObject.SubStyle);

			messageSendingObject.Action = AISOutgoingMessageTypeList.Codes.InvalidationRequest;
			AssertEquals("Sub style should be cleared when message type is not '415'", ZString.Empty, messageSendingObject.SubStyle);

			messageSendingObject.Action = AISOutgoingMessageTypeList.Codes.CustomsDeclaration;
			AssertEquals("Sub style should default to 'A'", SubStyleCodeList.Codes.NormalDeclaration, messageSendingObject.SubStyle);

			messageSendingObject.Action = AISOutgoingMessageTypeList.Codes.InvalidationRequest;
			AssertEquals("Sub style should be cleared when message type is not '415'", ZString.Empty, messageSendingObject.SubStyle);

			messageSendingObject.Action = AISOutgoingMessageTypeList.Codes.AmendmentRequest;
			AssertEquals("Sub style should default to 'A'", SubStyleCodeList.Codes.NormalDeclaration, messageSendingObject.SubStyle);
		}

		public void TestSubStyleValue_OriginalValueUsedIfSetInBillsGrid()
		{
			var bill = Factory.New<AsycudaBill>();
			bill.ABL_ShipmentType = SubStyleCodeList.Codes.PreliminaryDeclaration;

			var messageSendingObject = new MessageSendingObject(bill);

			AssertEquals("Pre-req: Message type should default to '415'", AISOutgoingMessageTypeList.Codes.CustomsDeclaration, messageSendingObject.Action);
			AssertEquals("First Sub style should be 'D'", SubStyleCodeList.Codes.PreliminaryDeclaration, messageSendingObject.SubStyle);

			messageSendingObject.Action = AISOutgoingMessageTypeList.Codes.InvalidationRequest;
			AssertEquals("Sub style should be cleared when message type is not '415'", ZString.Empty, messageSendingObject.SubStyle);

			messageSendingObject.Action = AISOutgoingMessageTypeList.Codes.CustomsDeclaration;
			AssertEquals("Sub style should return to 'D'", SubStyleCodeList.Codes.PreliminaryDeclaration, messageSendingObject.SubStyle);
		}

		public void TestAmendmentInvalidationReasonCouldBeEditedWhenMessageTypeIs413And414()
		{
			var bill = Factory.NewWithValidTestData<AsycudaBill>();
			var messageSendingObject = new MessageSendingObject(bill);

			var readOnlyForMessageSendingObject = typeof(MessageSendingObject).GetProperty("AmendmentInvalidationReason_ReadOnly", BindingFlags.Instance | BindingFlags.NonPublic);

			AssertEquals("Pre-req: Message type should default to '415'", AISOutgoingMessageTypeList.Codes.CustomsDeclaration, messageSendingObject.Action);
			AssertEquals("The column 'AmendmentInvalidationReason' is not editable at beginning", readOnlyForMessageSendingObject.GetValue(messageSendingObject).ToString(), "True");

			messageSendingObject.Action = AISOutgoingMessageTypeList.Codes.AmendmentRequest;
			AssertEquals("Value of 'AmendmentInvalidationReason' has been saved in the messageSendingObject when message type is IM413", readOnlyForMessageSendingObject.GetValue(messageSendingObject).ToString(), "False");
			
			messageSendingObject.Action = AISOutgoingMessageTypeList.Codes.InvalidationRequest;
			AssertEquals("Value of 'AmendmentInvalidationReason' has been saved in the messageSendingObject when message type is IM414", readOnlyForMessageSendingObject.GetValue(messageSendingObject).ToString(), "False");

			messageSendingObject.Action = AISOutgoingMessageTypeList.Codes.PresentationNotification;
			AssertEquals("Value of 'AmendmentInvalidationReason' is still empty in the messageSendingObject",  readOnlyForMessageSendingObject.GetValue(messageSendingObject).ToString(), "True");
		}

		public void TestSubStyle_Caption()
		{
			var messageSendingObject = GetNewBusinessObject() as MessageSendingObject;
			var resourceStringDataAttribute = messageSendingObject.SubStyleInfo.GetAttribute<ResourceStringDataAttribute>();
			var billShipmentTypeResourceStringDataAttribute = messageSendingObject.Bill.ABL_ShipmentTypeInfo.GetAttribute<ResourceStringDataAttribute>();
			CombineAssertions(() =>
			{
				AssertEquals("Additional Declaration Type", resourceStringDataAttribute.Caption);
				AssertEquals("Add. Decl. Type", resourceStringDataAttribute.ShortCaption);
				AssertEquals("[11 02 001 000] Additional Declaration Type", resourceStringDataAttribute.FullDescription);
				AssertEquals("Caption", billShipmentTypeResourceStringDataAttribute.Caption, resourceStringDataAttribute.Caption);
				AssertEquals("ShortCaption", billShipmentTypeResourceStringDataAttribute.ShortCaption, resourceStringDataAttribute.ShortCaption);
				AssertEquals("Full Description", billShipmentTypeResourceStringDataAttribute.FullDescription, resourceStringDataAttribute.FullDescription);
			});
		}

		protected override CodeDescriptionPairList ExpectedActionList
		{
			get
			{
				var result = new AISOutgoingMessageTypeList();
				result.RemoveCode(AISOutgoingMessageTypeList.Codes.D3ElectronicTransportDocument);
				result.RemoveCode(AISOutgoingMessageTypeList.Codes.EntryIntoTheDeclarantRecords);
				result.RemoveCode(AISOutgoingMessageTypeList.Codes.ProvideSupportingDocuments);
				result.RemoveCode(AISOutgoingMessageTypeList.Codes.DocumentsReceived);
				result.RemoveCode(AISOutgoingMessageTypeList.Codes.ApplicationForRemissionOfCustomsDebtR15);
				result.RemoveCode(AISOutgoingMessageTypeList.Codes.ApplicationForRemissionOfCustomsDebtF15);
				return result;
			}
		}

		protected override CodeDescriptionPairList ExpectedSubStyleList => new SubStyleCodeList();

		protected override EU.H7.Business.AsycudaBill GetMessageSendingObjectParentBill()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			return bill;
		}

		protected override EU.H7.Business.MessageSendingObject GetMessageSendingObject(EU.H7.Business.AsycudaBill bill) => new MessageSendingObject(bill);

		protected override ZString ExpectedActionForSendingCustomsDeclaration => AISOutgoingMessageTypeList.Codes.CustomsDeclaration;

		protected override Type ExpectedMessageSenderType => typeof(IEMessageSender);
	}
}
