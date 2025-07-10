using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Messaging;
using static Enterprise.Customs.IE.Business.Constants;

namespace Enterprise.Customs.IE.Business.Testing
{
	class AISMessageSendingActionValidationTest : TestCaseWithFactory
	{
		public void TestCheckShouldSend()
		{
			var errorMessage = "This entry has no linked Entry Instruction. Please check if there are any Invoice Lines linked to the corresponding Entry Instruction.";

			var declaration = Factory.New<JobDeclaration>();
			var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
			var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var sendingAction1 = new AISMessageSendingAction(entryHeader1);
			var sendingAction2 = new AISMessageSendingAction(entryHeader2);

			sendingAction1.MessageType = AISOutgoingMessageTypeList.Codes.CustomsDeclaration;
			sendingAction1.ShouldSend = true;
			sendingAction1.Validation.ValidateShouldSend();
			AssertHasError("Entry 1 - No linked entry instruction", sendingAction1.ShouldSendInfo, errorMessage);

			entryHeader1.CH_CEI_Instruction = instruction.PK;
			sendingAction1 = new AISMessageSendingAction(entryHeader1);
			sendingAction1.Validation.ValidateShouldSend();
			AssertNoError("Entry 1 - Linked entry instruction", sendingAction1.ShouldSendInfo, errorMessage);

			sendingAction2.MessageType = AISOutgoingMessageTypeList.Codes.CustomsDeclaration;
			sendingAction2.ShouldSend = true;
			sendingAction2.Validation.ValidateShouldSend();
			AssertHasError("Entry 2 - No linked entry instruction", sendingAction2.ShouldSendInfo, errorMessage);
		}

		public void TestCheckMessageType_BR5153()
		{
			const string message = "[BR5153] A declaration with '00100' Additional Information and Requested Procedure '51' cannot be amended (IM413). It must first be invalidated (IM414) and a new declaration (IM415) must be submitted.";

			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceLine = declaration.Invoices.FirstOrAddNew<JobComInvoiceHeader>().InvoiceLines.FirstOrAddNew<JobComInvoiceLine>();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes._51;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			var sendingAction = new AISMessageSendingAction(entryHeader);
			sendingAction.MessageType = AISOutgoingMessageTypeList.Codes.AmendmentRequest;
			AssertNoMessageError("BR5153: pass when no 00100 and no C601.", sendingAction.MessageTypeInfo, message);

			instruction.AdditionalInfos.AddNew(AdditionalInformationCodes._00100, string.Empty).CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			instruction.SupportingDocuments.AddNew(SupportingDocumentCodes.AuthorisationInwardProcessingProcedure, string.Empty);

			sendingAction.Validation.ValidateMessageType();
			AssertHasMessageError("BR5153: warn for 413 when INF 00100 exists.", sendingAction.MessageTypeInfo, message);

			sendingAction.MessageType = AISOutgoingMessageTypeList.Codes.CustomsDeclaration;
			AssertNoMessageError("BR5153: invalid for non-413 messages.", sendingAction.MessageTypeInfo, message);
		}
	}
}
