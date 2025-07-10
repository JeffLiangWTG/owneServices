using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Messaging;
using CusEntryHeader = Enterprise.Customs.IE.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.IE.Business.Testing
{
	sealed class CusEntryHeaderMessageSendingActionTestBaseOnly : TestCaseWithFactory
	{
		public void TestMessageStatus()
		{
			cusEntryHeader.CH_Status = "SNT";
			AssertEquals("MessageStatus", "SNT", new CusEntryHeaderMessageSendingActionForTesting(cusEntryHeader).MessageStatus);
			var captionAttr = DataBoundResourceStrings.GetDataForProperty(testMessageSendingAction.MessageStatusInfo);
			AssertEquals("MessageStatusInfo should have correct Caption", "Message Status", captionAttr.Caption);
		}

		public void TestSubStyle_Caption()
		{
			var captionAttr = DataBoundResourceStrings.GetDataForProperty(testMessageSendingAction.SubStyleInfo);
			AssertEquals("SubStyleInfo should have correct Caption", "Sub Style", captionAttr.Caption);
		}

		public void TestDescription_Caption()
		{
			var captionAttr = DataBoundResourceStrings.GetDataForProperty(testMessageSendingAction.DescriptionInfo);
			AssertEquals("DescriptionInfo should have correct Caption", "Description", captionAttr.Caption);
		}

		public void TestLocalReferenceNumber_Caption()
		{
			var captionAttr = DataBoundResourceStrings.GetDataForProperty(testMessageSendingAction.LocalReferenceNumberInfo);
			CombineAssertions(() =>
			{
				AssertEquals("LocalReferenceNumberInfo should have correct Caption", "LRN", captionAttr.Caption);
				AssertEquals("LocalReferenceNumberInfo should have correct FullDescription", "Local Reference Number", captionAttr.FullDescription);
			});
		}

		public void TestEntryStatus_Caption()
		{
			var captionAttr = DataBoundResourceStrings.GetDataForProperty(testMessageSendingAction.EntryStatusInfo);
			AssertEquals("EntryStatusInfo should have correct Caption", "Entry Status", captionAttr.Caption);
		}

		public void TestDeclarationType_Caption()
		{
			var captionAttr = DataBoundResourceStrings.GetDataForProperty(testMessageSendingAction.DeclarationTypeInfo);
			AssertEquals("DeclarationTypeInfo should have correct Caption", "Declaration Type", captionAttr.Caption);
		}

		public void TestMessageType()
		{
			cusEntryHeader.Declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			var aISTestMessageSendingAction = new AISMessageSendingAction(cusEntryHeader);
			AssertNoExceptionThrown("Message Type should have default value without exception", () => _ = aISTestMessageSendingAction.MessageType);

			aISTestMessageSendingAction.MessageType = AISOutgoingMessageTypeList.Codes.AmendmentRequest;
			AssertEquals("Message Type should have correct value", "413", aISTestMessageSendingAction.MessageType);

			aISTestMessageSendingAction.MessageTypeForDisplay = AISOutgoingMessageTypeListForDisplay.Codes.InvalidationRequest;
			AssertEquals("Message Type should have correct value", "414", aISTestMessageSendingAction.MessageType);

			cusEntryHeader.Declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			var aESTestMessageSendingAction = new AESMessageSendingAction(cusEntryHeader);
			AssertNoExceptionThrown("Message Type should have default value without exception", () => _ = aESTestMessageSendingAction.MessageType);

			aESTestMessageSendingAction.MessageType = AESOutgoingMessageTypeList.Codes.ExportAmendment;
			AssertEquals("Message Type should have correct value", "513", aESTestMessageSendingAction.MessageType);

			aESTestMessageSendingAction.MessageTypeForDisplay = AESOutgoingMessageTypeListForDisplay.Codes.ExportCancellation;
			AssertEquals("Message Type should have correct value", "514", aESTestMessageSendingAction.MessageType);
		}

		public void TestMessageType_MaxLength()
		{
			AssertEquals("MessageTypeInfo should have correct MaxLength", EDIMessage.Schema.EM_MessageTypeMaxLength, testMessageSendingAction.MessageTypeInfo.MaxLength);
		}

		public void TestMessageTypeForDisplay()
		{
			cusEntryHeader.Declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			var aISTestMessageSendingAction = new AISMessageSendingAction(cusEntryHeader);
			AssertNoExceptionThrown("Message Type should have default value without exception", () => _ = aISTestMessageSendingAction.MessageTypeForDisplay);

			aISTestMessageSendingAction.MessageType = AISOutgoingMessageTypeList.Codes.AmendmentRequest;
			AssertEquals("Message Type For Display should have correct value", "IM413", aISTestMessageSendingAction.MessageTypeForDisplay);

			aISTestMessageSendingAction.MessageTypeForDisplay = AISOutgoingMessageTypeListForDisplay.Codes.InvalidationRequest;
			AssertEquals("Message Type For Display should have correct value", "IM414", aISTestMessageSendingAction.MessageTypeForDisplay);

			cusEntryHeader.Declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			var aESTestMessageSendingAction = new AESMessageSendingAction(cusEntryHeader);
			AssertNoExceptionThrown("Message Type should have default value without exception", () => _ = aESTestMessageSendingAction.MessageTypeForDisplay);

			aESTestMessageSendingAction.MessageType = AESOutgoingMessageTypeList.Codes.ExportAmendment;
			AssertEquals("Message Type For Display should have correct value", "CC513C", aESTestMessageSendingAction.MessageTypeForDisplay);

			aESTestMessageSendingAction.MessageTypeForDisplay = AESOutgoingMessageTypeListForDisplay.Codes.ExportCancellation;
			AssertEquals("Message Type For Display should have correct value", "CC514C", aESTestMessageSendingAction.MessageTypeForDisplay);
		}

		public void TestMessageTypeForDisplay_MaxLength()
		{
			AssertEquals("MessageTypeInfo should have correct MaxLength", 6, testMessageSendingAction.MessageTypeForDisplayInfo.MaxLength);
		}

		public void TestMessageTypeForDisplay_Caption()
		{
			var captionAttr = DataBoundResourceStrings.GetDataForProperty(testMessageSendingAction.MessageTypeForDisplayInfo);
			AssertEquals("MessageTypeInfo should have correct Caption", "Message Type", captionAttr.Caption);
		}

		public void TestMessageTypeDescription()
		{
			cusEntryHeader.Declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			var aISTestMessageSendingAction = new AISMessageSendingAction(cusEntryHeader);

			var captionAttr = DataBoundResourceStrings.GetDataForProperty(aISTestMessageSendingAction.MessageTypeDescriptionInfo);
			AssertEquals("MessageTypeDescriptionInfo should have correct Caption", "Message Type Description", captionAttr.Caption);

			aISTestMessageSendingAction.MessageType = AISOutgoingMessageTypeList.Codes.AmendmentRequest;
			AssertEquals("MessageTypeDescription should have correct value", "Amendment", aISTestMessageSendingAction.MessageTypeDescription);
		}

		public void TestCreateSender()
		{
			var sender = testMessageSendingAction.CreateSender();
			AssertType<MessageSenderForTesting>("Created sender should have correct type.", sender);
		}

		public void TestLookups()
		{
			AssertType<CusEntryHeaderMessageSendingActionLookupsForTesting>("Should have created correct Lookups.", testMessageSendingAction.Lookups);
		}

		CusEntryHeaderMessageSendingAction testMessageSendingAction;
		CusEntryHeader cusEntryHeader;

		protected override void SetUp()
		{
			base.SetUp();
			cusEntryHeader = (CusEntryHeader)Factory.New<JobDeclaration>().ActiveEntryHeaders.AddNew();
			testMessageSendingAction = new CusEntryHeaderMessageSendingActionForTesting(cusEntryHeader);
		}
	}
}
