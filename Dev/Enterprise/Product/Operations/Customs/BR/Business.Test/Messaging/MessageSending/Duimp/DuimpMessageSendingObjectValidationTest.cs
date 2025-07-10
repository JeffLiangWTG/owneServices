using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business.Testing
{
	class DuimpMessageSendingObjectValidationTest : TestCaseWithFactory
	{
		public void TestCheckMessageType_DEL()
		{
			var expectedMessage = "There is no Entry Number.";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.CDI;

			CombineAssertions(() =>
			{
				var messageSending = new DuimpMessageSendingObject(entryHeader);
				messageSending.MessageType = ImportEntryActionCodeList.Codes.DEL;
				messageSending.ShouldSend = true;
				messageSending.Validation.ValidateMessageType();
				AssertHasErrorContaining(messageSending.MessageTypeInfo, expectedMessage);

				messageSending.ShouldSend = false;
				messageSending.Validation.ValidateMessageType();
				AssertNoError(messageSending.MessageTypeInfo, expectedMessage);

				messageSending.ShouldSend = true;
				entryHeader.EntryNumber = "111111";
				messageSending.Validation.ValidateMessageType();
				AssertNoError(messageSending.MessageTypeInfo, expectedMessage);

				messageSending.ShouldSend = true;
				foreach (var messageType in new ImportEntryActionCodeList().GetAllCodes().Except(new[] { ImportEntryActionCodeList.Codes.DEL }))
				{
					messageSending.MessageType = messageType;
					AssertNoError(messageSending.MessageTypeInfo, expectedMessage);
				}
			});
		}

		public void TestCheckMessageType_DIA_REG()
		{
			var expectedMessage = "There are changes that have not been submitted to Customs. Please send an ORI - Original message before sending this message.";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.CDI;
			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			var entryLine2 = entryHeader.AllEntryLines.AddNew();

			CombineAssertions(() =>
			{
				var messageSending = new DuimpMessageSendingObject(entryHeader);
				foreach (var messageType in new[] { ImportEntryActionCodeList.Codes.DIA, ImportEntryActionCodeList.Codes.REG })
				{
					entryHeader.EntryNumber = "111111";
					entryHeader.CH_CustomsPostedStatus = CustomsPostedStatusList.Codes.Accepted;
					entryLine1.CL_CustomsPostedStatus = CustomsPostedStatusList.Codes.Accepted;
					entryLine2.CL_CustomsPostedStatus = CustomsPostedStatusList.Codes.Deleted;

					messageSending.ShouldSend = true;
					messageSending.MessageType = messageType;
					AssertNoError(messageSending.MessageTypeInfo, expectedMessage);

					entryHeader.EntryNumber = ZString.Empty;
					messageSending.Validation.ValidateMessageType();
					AssertHasError("EntryNumber is empty", messageSending.MessageTypeInfo, expectedMessage);

					entryHeader.EntryNumber = "111111";
					entryHeader.CH_CustomsPostedStatus = CustomsPostedStatusList.Codes.Active;
					messageSending.Validation.ValidateMessageType();
					AssertHasError("CH_CustomsPostedStatus is Active", messageSending.MessageTypeInfo, expectedMessage);

					entryHeader.CH_CustomsPostedStatus = CustomsPostedStatusList.Codes.UpdatePending;
					messageSending.Validation.ValidateMessageType();
					AssertHasError("CH_CustomsPostedStatus is UpdatePending", messageSending.MessageTypeInfo, expectedMessage);

					entryHeader.CH_CustomsPostedStatus = CustomsPostedStatusList.Codes.Accepted;
					entryLine1.CL_CustomsPostedStatus = CustomsPostedStatusList.Codes.UpdatePending;
					messageSending.Validation.ValidateMessageType();
					AssertHasError("CL_CustomsPostedStatus is UpdatePending", messageSending.MessageTypeInfo, expectedMessage);

					entryLine1.CL_CustomsPostedStatus = CustomsPostedStatusList.Codes.Active;
					messageSending.Validation.ValidateMessageType();
					AssertHasError("CL_CustomsPostedStatus is Active", messageSending.MessageTypeInfo, expectedMessage);

					entryLine1.CL_CustomsPostedStatus = CustomsPostedStatusList.Codes.DeletePending;
					messageSending.Validation.ValidateMessageType();
					AssertHasError("CL_CustomsPostedStatus is DeletePending", messageSending.MessageTypeInfo, expectedMessage);

					messageSending.ShouldSend = false;
					AssertNoError("ShouldSend is false", messageSending.MessageTypeInfo, expectedMessage);
				}

				messageSending.ShouldSend = true;
				foreach (var messageType in new ImportEntryActionCodeList().GetAllCodes().Except(new[] { ImportEntryActionCodeList.Codes.DIA, ImportEntryActionCodeList.Codes.REG }))
				{
					messageSending.MessageType = ImportEntryActionCodeList.Codes.DEL;
					AssertNoError($"Message Type is {messageType}", messageSending.MessageTypeInfo, expectedMessage);
				}
			});
		}

		public void TestCheckShouldSend()
		{
			var expectedMessage = "There is message waiting for response. Please wait until the messages are responded.";

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.CDI;

			CombineAssertions(() =>
			{
				var messageSending = new DuimpMessageSendingObject(entryHeader);
				messageSending.ShouldSend = true;
				AssertNoError(messageSending.ShouldSendInfo, expectedMessage);

				entryHeader.CH_Status = BRMessageStatusList.Codes.AwaitingResponse;
				messageSending.Validation.ValidateShouldSend();
				AssertHasError(messageSending.ShouldSendInfo, expectedMessage);

				messageSending.ShouldSend = false;
				AssertNoError(messageSending.ShouldSendInfo, expectedMessage);
			});
		}
	}
}
