using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class ExportDeclarationMessageSendingObjectValidationTest : TestCaseWithFactory
	{
		public void TestCheckMessageTypeForExport()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.CDE;
			var messageSending = new ExportDeclarationMessageSendingObject(entryHeader);
			AssertNoErrorContaining(messageSending.MessageTypeInfo, "Rectification Messages should only be sent if the Entry contains a MRN Number.");

			messageSending.MessageType = ExportEntryActionCodeList.Codes.RET;
			messageSending.Validation.ValidateMessageType();
			AssertHasErrorContaining(messageSending.MessageTypeInfo, "Rectification Messages should only be sent if the Entry contains a MRN Number.");

			entryHeader.MovementReferenceNumberSetter("20BR0000274180");
			messageSending.Validation.ValidateMessageType();
			AssertNoErrorContaining(messageSending.MessageTypeInfo, "Rectification Messages should only be sent if the Entry contains a MRN Number.");

			messageSending.MessageType = ExportEntryActionCodeList.Codes.ORI;
			messageSending.Validation.ValidateMessageType();
			AssertHasErrorContaining(messageSending.MessageTypeInfo, "Original message has already been sent. Entry already contains a MRN Number. Use RET Message Type if you want rectify/ amend the Entry");

			entryHeader.MovementReferenceNumberSetter("");
			entryHeader.CH_Status = BRMessageStatusList.Codes.AwaitingResponse;
			messageSending.Validation.ValidateMessageType();
			AssertNoErrorContaining(messageSending.MessageTypeInfo, "Original message has already been sent. Entry already contains a MRN Number. Use RET Message Type if you want rectify/ amend the Entry");
		}

		public void TestCheckShouldSend()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.CDD;

			var messageSending = new ExportDeclarationMessageSendingObject(entryHeader);
			messageSending.ShouldSend = false;
			entryHeader.CH_Status = BRMessageStatusList.Codes.AwaitingResponse;
			AssertNoError(messageSending.ShouldSendInfo, "There is message waiting for response. Please wait until the messages are responded.");

			messageSending.ShouldSend = true;
			AssertHasError(messageSending.ShouldSendInfo, "There is message waiting for response. Please wait until the messages are responded.");

			entryHeader.CH_Status = BRMessageStatusList.Codes.Accepted;
			messageSending.Validation.ValidateShouldSend();
			AssertNoError(messageSending.ShouldSendInfo, "There is message waiting for response. Please wait until the messages are responded.");
		}

		public void TestCheckVOCReason()
		{
			var listCodesShouldNotHaveNotification = new string[] { "", "E10", "E80", "E81", "E82", "E83" };
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.CDE;
			var messageSending = new ExportDeclarationMessageSendingObject(entryHeader);

			messageSending.ShouldSend = true;
			messageSending.MessageType = ExportEntryActionCodeList.Codes.RET;
			messageSending.VOCReason = ZString.Empty;
			foreach (var code in listCodesShouldNotHaveNotification)
			{
				entryHeader.CH_EntryStatus = code;
				messageSending.Validation.ValidateVOCReason();
				AssertNoMessageError(messageSending.VOCReasonInfo, "You have not entered a Rectification Reason.");
			}

			entryHeader.CH_EntryStatus = "E56";
			messageSending.Validation.ValidateVOCReason();
			AssertHasMessageError(messageSending.VOCReasonInfo, "You have not entered a Rectification Reason.");

			messageSending.MessageType = ExportEntryActionCodeList.Codes.ORI;
			messageSending.Validation.ValidateVOCReason();
			AssertNoMessageError(messageSending.VOCReasonInfo, "You have not entered a Rectification Reason.");

			messageSending.MessageType = ExportEntryActionCodeList.Codes.RET;
			messageSending.ShouldSend = false;
			messageSending.Validation.ValidateVOCReason();
			AssertNoMessageError(messageSending.VOCReasonInfo, "You have not entered a Rectification Reason.");

			messageSending.ShouldSend = true;
			messageSending.VOCReason = "TEST";
			AssertNoMessageError(messageSending.MessageTypeInfo, "You have not entered a Rectification Reason.");
		}
	}
}
