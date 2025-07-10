using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.Testing
{
	class AISUCC5MessageSendingActionValidationTest : TestCaseWithFactory
	{
		public void TestCheckMessageType()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions[0];
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;

			AssertCH_CustomsMessageRemarksMandatory("H1");
			AssertCH_CustomsMessageRemarksNotMandatory("H2");
			AssertCH_CustomsMessageRemarksMandatory("H3");
			AssertCH_CustomsMessageRemarksMandatory("H4");
			AssertCH_CustomsMessageRemarksMandatory("H5");
			AssertCH_CustomsMessageRemarksNotMandatory("H6");
			AssertCH_CustomsMessageRemarksMandatory("I1");

			void AssertCH_CustomsMessageRemarksMandatory(ZString style)
			{
				entryInstruction.CEI_Style = style;
				entryHeader.CH_CustomsMessageRemarks = ZString.Empty;
				var sendingAction = new AISUCC5MessageSendingAction(entryHeader);
				sendingAction.MessageType = AISOutgoingMessageTypeList.Codes.AmendmentRequest;
				AssertHasMessageErrorContaining($"CH_CustomsMessageRemarks is blank, CEI_Style = {entryInstruction.CEI_Style}", sendingAction.MessageTypeInfo, "You have not entered a Reason for Amendment.");

				entryHeader.CH_CustomsMessageRemarks = "Amendment Reason";
				sendingAction = new AISUCC5MessageSendingAction(entryHeader);
				sendingAction.MessageType = AISOutgoingMessageTypeList.Codes.AmendmentRequest;
				AssertNoMessageErrorContaining($"CH_CustomsMessageRemarks is set, CEI_Style = {entryInstruction.CEI_Style}", sendingAction.MessageTypeInfo, "You have not entered a Reason for Amendment.");
			}

			void AssertCH_CustomsMessageRemarksNotMandatory(ZString style)
			{
				entryInstruction.CEI_Style = style;
				entryHeader.CH_CustomsMessageRemarks = ZString.Empty;
				var sendingAction = new AISUCC5MessageSendingAction(entryHeader);
				sendingAction.MessageType = AISOutgoingMessageTypeList.Codes.AmendmentRequest;
				AssertNoMessageErrorContaining($"CH_CustomsMessageRemarks is blank, CEI_Style = {entryInstruction.CEI_Style}", sendingAction.MessageTypeInfo, "You have not entered a Reason for Amendment.");
			}
		}
	}
}
