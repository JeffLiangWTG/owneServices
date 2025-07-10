using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.FR.Business.Declaration;
using CusEntryInstruction = Enterprise.Customs.FR.Business.Declaration.CusEntryInstruction;

namespace Enterprise.Customs.FR.Business.MessageSending.Testing
{
	public class DeltaIEJobDeclarationMessageSendingObjectValidationTest : MessageSendingObjectValidationTest
	{
		public void TestCheckVOCReason()
		{
			var messageObject = GenerateDeltaIEDeclarationSendingObject();
			messageObject.MessageType = DeltaIESendMessageSubTypeList.Codes.Invalidation;
			messageObject.Validation.ValidateVOCReason();
			AssertNoMessageErrors("When message type is not 413, Invalidation Reason is not mandatory.", messageObject.VOCReasonInfo);

			messageObject.MessageType = DeltaIESendMessageSubTypeList.Codes.AmendmentRequest;
			messageObject.Validation.ValidateVOCReason();
			AssertNoMessageErrors("When message type is 413, Invalidation Reason is not mandatory.", messageObject.VOCReasonInfo);

			messageObject.VOCReason = "XXXXX";
			messageObject.Validation.ValidateVOCReason();
			AssertNoMessageErrors("When message type is 413, Invalidation Reason is not mandatory.", messageObject.VOCReasonInfo);
		}

		public void TestCheckMessageType()
		{
			var messageObject = GenerateDeltaIEDeclarationSendingObject();
			AssertNoErrorContaining("MessageType has value 415 by default => no error.", messageObject.MessageTypeInfo, ListValidation.InvalidCodeError);
			AssertNoErrorContaining("MessageType has value 415 by default => no error.", messageObject.MessageTypeInfo, MandatoryValidation.MustBeEntered);

			messageObject.MessageType = "XXX";
			AssertHasErrorContaining("MessageType has an invalid code. => error.", messageObject.MessageTypeInfo, ListValidation.InvalidCodeError);

			messageObject.MessageType = ZString.Empty;
			AssertHasErrorContaining("MessageType is empty => error.", messageObject.MessageTypeInfo, MandatoryValidation.MustBeEntered);
		}

		public void TestCheckChangeAcknowledgementIndicator_WhenNotReadOnly()
		{
			DeltaIEJobDeclarationMessageSendingObjectLookupsTest.SetUpCusCodeList_INVMO(Factory);
			DeltaIEJobDeclarationMessageSendingObjectLookupsTest.SetUpCusCodeList_RECMO(Factory);
			var messageObject = GenerateDeltaIEDeclarationSendingObject();
			CombineAssertions("Motivation is required to be entered when it's not ReadOnly and ShouldSend is true.", () =>
			{
				messageObject.MessageType = DeltaIESendMessageSubTypeList.Codes.Invalidation;
				Assert("Prerequisite: Motivation is not ReadOnly for 414.", !messageObject.ChangeAcknowledgementIndicatorInfo.ReadOnly);

				messageObject.ShouldSend = true;
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(messageObject.ChangeAcknowledgementIndicatorInfo);
				ValidationTestHelper.AssertInvalidCodeMessageError(messageObject.ChangeAcknowledgementIndicatorInfo, "XXXXX", "INV01");

				messageObject.ShouldSend = false;
				AssertNoNotifications(messageObject.ChangeAcknowledgementIndicatorInfo);

				messageObject.MessageType = DeltaIESendMessageSubTypeList.Codes.AmendmentRequest;
				Assert("Prerequisite: Motivation is not ReadOnly for 413.", !messageObject.ChangeAcknowledgementIndicatorInfo.ReadOnly);

				messageObject.ShouldSend = true;
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(messageObject.ChangeAcknowledgementIndicatorInfo);
				ValidationTestHelper.AssertInvalidCodeMessageError(messageObject.ChangeAcknowledgementIndicatorInfo, "XXXXX", "MODIF");

				messageObject.ShouldSend = false;
				AssertNoNotifications(messageObject.ChangeAcknowledgementIndicatorInfo);
			});
		}

		public void TestCheckChangeAcknowledgementIndicator_WhenReadOnly()
		{
			var messageObject = GenerateDeltaIEDeclarationSendingObject();
			CombineAssertions("Motivation is not required to be entered if it's ReadOnly.", () =>
			{
				messageObject.MessageType = DeltaIESendMessageSubTypeList.Codes.ImportDeclaration;
				Assert("Prerequisite: Motivation is ReadOnly for 415.", messageObject.ChangeAcknowledgementIndicatorInfo.ReadOnly);

				messageObject.Validation.ValidateChangeAcknowledgementIndicator();
				AssertNoNotifications("No message errors or errors for Motivation even if it's empty.", messageObject.ChangeAcknowledgementIndicatorInfo);
			});
		}

		public void TestCheckChangeAcknowledgementIndicator_NotificationsRemoved_WhenMessageTypeChanged()
		{
			var messageObject = GenerateDeltaIEDeclarationSendingObject();
			CombineAssertions(() =>
			{
				messageObject.MessageType = DeltaIESendMessageSubTypeList.Codes.Invalidation;
				messageObject.Validation.ValidateChangeAcknowledgementIndicator();
				AssertHasMessageErrorContaining("Message Error if Motivation is empty and message type is 414.", messageObject.ChangeAcknowledgementIndicatorInfo, MandatoryValidation.YouHaveNotEntered);

				messageObject.MessageType = DeltaIESendMessageSubTypeList.Codes.ImportDeclaration;
				AssertNoNotifications("All notifications on ChangeAcknowledgementIndicatorInfo should be removed when message type is changed.", messageObject.ChangeAcknowledgementIndicatorInfo);

				messageObject.MessageType = DeltaIESendMessageSubTypeList.Codes.AmendmentRequest;
				messageObject.Validation.ValidateChangeAcknowledgementIndicator();
				AssertHasMessageErrorContaining("Message Error if Motivation is empty and message type is 413.", messageObject.ChangeAcknowledgementIndicatorInfo, MandatoryValidation.YouHaveNotEntered);

				messageObject.MessageType = DeltaIESendMessageSubTypeList.Codes.ImportDeclaration;
				AssertNoNotifications("All notifications on ChangeAcknowledgementIndicatorInfo should be removed when message type is changed.", messageObject.ChangeAcknowledgementIndicatorInfo);
			});
		}

		DeltaIEJobDeclarationMessageSendingObject GenerateDeltaIEDeclarationSendingObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			declaration.JE_MessageType = MessageSubTypeList.Codes.IMC;
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_JE = declaration.PK;
			var line = invoice.InvoiceLines.AddNew();
			line.JI_JZ = invoice.PK;

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_EntryStatus = "ACC";
			entry.MovementReferenceNumberSetter("ABC", ZDateTime.BrettsBirthday);
			entry.CH_CustomsMessageRemarks = "Amendment";
			entry.CH_BGMReference = "1GB945390992000-B00001002";

			var entryInstruction = Factory.NewWithValidTestData<CusEntryInstruction>();
			entry.CH_CEI_Instruction = entryInstruction.PK;
			entry.EntryInstruction.CEI_SubStyle = "41";
			entryInstruction.CEI_JE = declaration.PK;

			var testItem = new DeltaIEJobDeclarationMessageSendingObject(entry);

			return testItem;
		}
	}
}
