using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.Testing
{
	class AESMessageSendingActionValidationTest : TestCaseWithFactory
	{
		public void TestCheckSendingActionType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "REX";
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			cusEntryHeader.CH_CEI_Instruction = instruction.PK;
			var sendingAction = new AESMessageSendingAction(cusEntryHeader);
			sendingAction.MessageType = AESOutgoingMessageTypeList.Codes.ReExportAmendment;
			sendingAction.ShouldSend = true;
			var message = "It is invalid to send a Re-export Amendment (IE573) when no MRN is available.";
			sendingAction.Validation.ValidateMessageType();
			AssertHasMessageErrorContaining("When MRN is empty", sendingAction.MessageTypeInfo, message);
		}

		public void TestCheckShouldSend()
		{
			var errorMessage = "This entry has no linked Entry Instruction. Please check if there are any Invoice Lines linked to the corresponding Entry Instruction.";

			var declaration = Factory.New<JobDeclaration>();
			var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
			var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var sendingAction1 = new AESMessageSendingAction(entryHeader1);
			var sendingAction2 = new AESMessageSendingAction(entryHeader2);

			sendingAction1.MessageType = AESOutgoingMessageTypeList.Codes.ExportOriginal;
			sendingAction1.ShouldSend = true;
			sendingAction1.Validation.ValidateShouldSend();
			AssertHasError("Entry 1 - No linked entry instruction", sendingAction1.ShouldSendInfo, errorMessage);

			entryHeader1.CH_CEI_Instruction = instruction.PK;
			sendingAction1 = new AESMessageSendingAction(entryHeader1);
			sendingAction1.Validation.ValidateShouldSend();
			AssertNoError("Entry 1 - Linked entry instruction", sendingAction1.ShouldSendInfo, errorMessage);

			sendingAction2.MessageType = AESOutgoingMessageTypeList.Codes.ExportOriginal;
			sendingAction2.ShouldSend = true;
			sendingAction2.Validation.ValidateShouldSend();
			AssertHasError("Entry 2 - No linked entry instruction", sendingAction2.ShouldSendInfo, errorMessage);
		}

		public void TestCheckMessageType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;

			var errorMessage = "Message Type should be Export Declaration Original(515) when Message Status is empty";

			entryHeader.CH_Status = null;
			var sendingAction = new AESMessageSendingAction(entryHeader);
			var targetInfo = sendingAction.MessageTypeInfo;
			sendingAction.MessageType = AESOutgoingMessageTypeList.Codes.ReExportAmendment;
			AssertHasMessageErrorContaining("If logical status is empty then should be chosen.", targetInfo, errorMessage);

			sendingAction.MessageType = AESOutgoingMessageTypeList.Codes.ExportOriginal;
			sendingAction.Validation.ValidateMessageType();
			AssertNoMessageError("Should be clear when valid value on MessageType.", targetInfo, errorMessage);

			errorMessage = "Message Type should be Export Declaration Amendment(513) when Entry Status is Amendment Requested";

			entryHeader.CH_Status = LogicalStatusList.Codes.Sent;
			entryHeader.CH_EntryStatus = AESEntryStatusList.Codes.AmendmentRequested;
			sendingAction.Validation.ValidateMessageType();
			AssertHasMessageErrorContaining("If ch_entrystatus is amendmentrequested then  exportamendment should be chosen", targetInfo, errorMessage);

			sendingAction.MessageType = AESOutgoingMessageTypeList.Codes.ExportAmendment;
			sendingAction.Validation.ValidateMessageType();
			AssertNoMessageError("Should be clear when valid value on MessageType.", targetInfo, errorMessage);

			errorMessage = "Message Type should not be Export Declaration Original(515) when Entry Status is Pre-lodged";

			entryHeader.CH_EntryStatus = AESEntryStatusList.Codes.Prelodged;
			sendingAction.MessageType = AESOutgoingMessageTypeList.Codes.ExportOriginal;
			sendingAction.Validation.ValidateMessageType();
			AssertHasMessageErrorContaining("If ch_entrystatus is pre-lodged, then exportoriginal should not be selected", targetInfo, errorMessage);

			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			sendingAction.MessageType = AESOutgoingMessageTypeList.Codes.ExportAmendment;
			sendingAction.Validation.ValidateMessageType();
			AssertNoMessageError("Should be clear when valid value on MessageType.", targetInfo, errorMessage);

			var messageFor614 = "Message Type SHOULD BE \"Exit Cancellation\"(614) when Entry Status is \"Cancellation Requested by Customs\".";
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.ReExport;
			sendingAction.MessageType = AESOutgoingMessageTypeList.Codes.ReExport;
			AssertNoMessageErrors("REX, Empty, 570, validation passes.", targetInfo);
			entryHeader.CH_EntryStatus = AESEntryStatusList.Codes.CancellationRequestedByCustoms;
			entryHeader.MovementReferenceNumberSetter("22IEDU4EX151267639");
			sendingAction.Validation.ValidateMessageType();
			AssertHasMessageError("REX, CAR, MRN has value, 570, has error(expecting 614).", targetInfo, messageFor614);
			sendingAction.MessageType = AESOutgoingMessageTypeList.Codes.ExitCancellation;
			AssertNoMessageError("REX, CAR, MRN has value, 614, validation passes.", targetInfo, messageFor614);

			errorMessage = "Message Type should be Re-Export Notification(570) when MRN is empty";
			entryHeader.MovementReferenceNumberSetter(ZString.Empty);
			sendingAction.Validation.ValidateMessageType();
			AssertHasMessageError("REX, CAR, MRN empty, 614, has error(expecting 570).", targetInfo, errorMessage);
			sendingAction.MessageType = AESOutgoingMessageTypeList.Codes.ReExport;
			AssertNoMessageError("REX, CAR, MRN empty, 570, validation passes.", targetInfo, errorMessage);

			errorMessage = "Message Type should be Exit Summary Declaration Original(615) when MRN is empty";
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.ExitSummary;
			sendingAction.MessageType = AESOutgoingMessageTypeList.Codes.ExitAmendment;
			AssertHasMessageError("EXS, CAR, MRN empty, 613, has error(expecting 615).", targetInfo, errorMessage);
			sendingAction.MessageType = AESOutgoingMessageTypeList.Codes.ExitOriginal;
			AssertNoMessageError("EXS, CAR, MRN empty, 615, validation passes.", targetInfo, errorMessage);

			entryHeader.MovementReferenceNumberSetter("22IEDU4EX151267639");
			sendingAction.Validation.ValidateMessageType();
			AssertHasMessageError("EXS, CAR, MRN has value, 615, has error(expecting 614).", targetInfo, messageFor614);
			sendingAction.MessageType = AESOutgoingMessageTypeList.Codes.ExitCancellation;
			AssertNoMessageError("EXS, CAR, MRN has value, 614, validation passes.", targetInfo, messageFor614);
		}

		public void TestCheckMessageType_AmendmentNotAllowed()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;

			var sendingAction = new AESMessageSendingAction(entryHeader);
			var targetInfo = sendingAction.MessageTypeInfo;

			var errorMessage = "Message type Export Declaration Amendment (IE513) cannot be used when the entry status is Released for Export (IE529)";
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			entryHeader.CH_EntryStatus = AESEntryStatusList.Codes.ReleasedForExport;
			sendingAction.MessageType = AESOutgoingMessageTypeList.Codes.ExportAmendment;

			var stylesToBeChecked = new string[] { "B1", "B2", "B3", "B4" };
			var subStylesThatShouldShowTheError = new string[] { "A", "Y" };

			CheckValidation();

			stylesToBeChecked = new string[] { "C1" };
			subStylesThatShouldShowTheError = System.Array.Empty<string>();

			CheckValidation();

			var previous511Message = Factory.New<AESInboundEDIMessage>();
			previous511Message.EM_MessageType = AESOutgoingMessageTypeList.Codes.ExportPresentation;
			previous511Message.EM_MessageText = "<text>";
			previous511Message.EM_LinkTable = entryHeader.TableName;
			previous511Message.EM_LinkUniqueID = entryHeader.PK;
			previous511Message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			previous511Message.EM_LinkedObject = entryHeader;
			entryHeader.Messages.Add(previous511Message);
			instruction.CusAuthorizationUsages.RemoveAndDeleteAll();
			Factory.Save();

			stylesToBeChecked = new string[] { "B1", "B2", "B3" };
			subStylesThatShouldShowTheError = new string[] { "D" };

			CheckValidation();

			stylesToBeChecked = new string[] { "B4" };
			subStylesThatShouldShowTheError = new string[] { "D", "F" };

			CheckValidation();

			stylesToBeChecked = new string[] { "C1" };
			subStylesThatShouldShowTheError = System.Array.Empty<string>();

			CheckValidation();

			entryHeader.Messages.RemoveAndDeleteAll();
			instruction.CusAuthorizationUsages.RemoveAndDeleteAll();
			Factory.Save();

			errorMessage = "Message type Exit Summary Declaration Amendment (IE613) cannot be used when the entry status is Released for Export (IE529)";
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.ExitSummary;
			entryHeader.CH_EntryStatus = AESEntryStatusList.Codes.ReleasedForExport;
			sendingAction.MessageType = AESOutgoingMessageTypeList.Codes.ExitAmendment;

			stylesToBeChecked = new string[] { "A1", "A2" };
			subStylesThatShouldShowTheError = new string[] { "A" };

			CheckValidation();

			errorMessage = "Message type Re-Export Declaration Amendment (IE573) cannot be used when the entry status is Released for Exit (IE525)";
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.ReExport;
			entryHeader.CH_EntryStatus = AESEntryStatusList.Codes.ReleasedForExit;
			sendingAction.MessageType = AESOutgoingMessageTypeList.Codes.ReExportAmendment;

			stylesToBeChecked = new string[] { "A3" };
			subStylesThatShouldShowTheError = new string[] { "A" };

			CheckValidation();

			errorMessage = "Message type Export Declaration Amendment (IE513) cannot be used when the entry status is Controlled for Export";
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			entryHeader.CH_EntryStatus = AESEntryStatusList.Codes.ControlledForExport;
			sendingAction.MessageType = AESOutgoingMessageTypeList.Codes.ExportAmendment;

			stylesToBeChecked = new string[] { "B1", "B2", "B3", "B4" };
			subStylesThatShouldShowTheError = new string[] { "A", "Y" };

			CheckValidation();

			previous511Message = Factory.New<AESInboundEDIMessage>();
			previous511Message.EM_MessageType = AESOutgoingMessageTypeList.Codes.ExportPresentation;
			previous511Message.EM_MessageText = "<text>";
			previous511Message.EM_LinkTable = entryHeader.TableName;
			previous511Message.EM_LinkUniqueID = entryHeader.PK;
			previous511Message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			previous511Message.EM_LinkedObject = entryHeader;
			entryHeader.Messages.Add(previous511Message);
			instruction.CusAuthorizationUsages.RemoveAndDeleteAll();
			Factory.Save();

			stylesToBeChecked = new string[] { "B1", "B2", "B3" };
			subStylesThatShouldShowTheError = new string[] { "D" };

			CheckValidation();

			stylesToBeChecked = new string[] { "B4" };
			subStylesThatShouldShowTheError = new string[] { "D", "F" };

			CheckValidation();

			stylesToBeChecked = new string[] { "C1" };
			subStylesThatShouldShowTheError = System.Array.Empty<string>();

			CheckValidation();

			entryHeader.Messages.RemoveAndDeleteAll();
			instruction.CusAuthorizationUsages.RemoveAndDeleteAll();
			Factory.Save();

			errorMessage = "Message type Exit Summary Declaration Amendment (IE613) cannot be used when the entry status is Controlled for Export";
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.ExitSummary;
			entryHeader.CH_EntryStatus = AESEntryStatusList.Codes.ControlledForExport;
			sendingAction.MessageType = AESOutgoingMessageTypeList.Codes.ExitAmendment;

			stylesToBeChecked = new string[] { "A1", "A2" };
			subStylesThatShouldShowTheError = new string[] { "A" };

			CheckValidation();

			errorMessage = "Message type Re-Export Declaration Amendment (IE573) cannot be used when the entry status is Controlled for Export";
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.ReExport;
			entryHeader.CH_EntryStatus = AESEntryStatusList.Codes.ControlledForExport;
			sendingAction.MessageType = AESOutgoingMessageTypeList.Codes.ReExportAmendment;

			stylesToBeChecked = new string[] { "A3" };
			subStylesThatShouldShowTheError = new string[] { "A" };

			CheckValidation();

			void CheckValidation()
			{
				var noErrorSubStyleList = new EntrySubStyleList().GetAllCodes().Except(subStylesThatShouldShowTheError).ToArray();

				foreach (string errorStyle in stylesToBeChecked)
				{
					instruction.CEI_Style = errorStyle;

					foreach (string errorSubStyle in subStylesThatShouldShowTheError)
					{
						instruction.CEI_SubStyle = errorSubStyle;
						sendingAction.Validation.ValidateMessageType();
						AssertHasMessageError($"Message type '{sendingAction.MessageType}' is not allowed when entry status is '{entryHeader.CH_EntryStatus}', CEI_Style '{errorStyle}', CEI_SubStyle '{errorSubStyle}'.", targetInfo, errorMessage);
					}

					foreach (string noErrorSubStyle in noErrorSubStyleList)
					{
						instruction.CEI_SubStyle = noErrorSubStyle;
						sendingAction.Validation.ValidateMessageType();
						AssertNoMessageError($"Message type '{sendingAction.MessageType}' should be allowed when entry status is '{entryHeader.CH_EntryStatus}', CEI_Style '{errorStyle}', CEI_SubStyle '{noErrorSubStyle}'.", targetInfo, errorMessage);
					}
				}
			}
		}

		public void TestCheckMessageType_ExportPresentation()
		{
			string[] allowedEntryStatusesList = { AESEntryStatusList.Codes.PendingControl, AESEntryStatusList.Codes.Prelodged };
			string[] notAllowedEntryStatusesList = new AESEntryStatusList().GetAllCodes().Except(allowedEntryStatusesList).ToArray();

			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var sendingAction = new AESMessageSendingAction(entryHeader);
			var targetInfo = sendingAction.MessageTypeInfo;
			entryHeader.CH_Status = "ABC";
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			entryHeader.CH_CEI_Instruction = instruction.PK;
			sendingAction.MessageType = AESOutgoingMessageTypeList.Codes.ExportPresentation;

			var errorMessage = "Export Presentation Notification should only be sent when Customs Status is Pending Control or Pre-Lodged";

			CombineAssertions(() =>
			{
				foreach (string notAllowedEntryStatus in notAllowedEntryStatusesList)
				{
					entryHeader.CH_EntryStatus = notAllowedEntryStatus;
					sendingAction.Validation.ValidateMessageType();
					AssertHasMessageError($"Should have an error, EntryStatus {notAllowedEntryStatus}", targetInfo, errorMessage);
				}

				foreach (string allowedEntryStatus in allowedEntryStatusesList)
				{
					entryHeader.CH_EntryStatus = allowedEntryStatus;
					sendingAction.Validation.ValidateMessageType();
					AssertNoMessageError($"Should NOT have an error, EntryStatus {allowedEntryStatus}", targetInfo, errorMessage);
				}
			});

			string[] allowedSubStyleList = { "D", "F" };
			string[] notAllowedSubStyleList = new EntrySubStyleList().GetAllCodes().Except(allowedSubStyleList).ToArray();

			errorMessage = "Export Presentation Notification should only be sent for Preliminary Declarations (Sub-Style D or F)";

			CombineAssertions(() =>
			{
				foreach (string notAllowedEntrySubStyle in notAllowedSubStyleList)
				{
					instruction.CEI_SubStyle = notAllowedEntrySubStyle;
					sendingAction.Validation.ValidateMessageType();
					AssertHasMessageError($"Should have an error, EntrySubStyle {notAllowedEntrySubStyle}", targetInfo, errorMessage);
				}

				foreach (string allowedEntrySubStyle in allowedSubStyleList)
				{
					instruction.CEI_SubStyle = allowedEntrySubStyle;
					sendingAction.Validation.ValidateMessageType();
					AssertNoMessageError($"Should NOT have an error, EntrySubStyle {allowedEntrySubStyle}", targetInfo, errorMessage);
				}
			});
		}

		public void TestCheckMessageType_ReleaseAmendment()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			var errorMessage = "Message type Post Release Amendment (IEX13) cannot be used when the entry status is not Released for Export (IE529)";
			var sendingAction = new AESMessageSendingAction(entryHeader);
			var targetInfo = sendingAction.MessageTypeInfo;
			sendingAction.MessageType = AESOutgoingMessageTypeList.Codes.ReleaseAmendment;
			AssertHasMessageError(targetInfo, errorMessage);

			entryHeader.MovementReferenceNumberSetter("22IEDU4E151267639");
			sendingAction.Validation.ValidateMessageType();
			AssertNoMessageError(targetInfo, errorMessage);

			sendingAction.MessageType = AESOutgoingMessageTypeList.Codes.ExportAmendment;
			AssertNoMessageError(targetInfo, errorMessage);
		}

		public void TestSendExportCancellationHasCancellationReason()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			entryHeader.CH_EntryStatus = AESEntryStatusList.Codes.MrnAllocated;
			entryHeader.CH_Status = LogicalStatusList.Codes.Accepted;

			var sendingAction = new AESMessageSendingAction(entryHeader);
			sendingAction.ShouldSend = true;
			sendingAction.MessageType = AESOutgoingMessageTypeList.Codes.ExportOriginal;
			AssertNoMessageErrors("Should be clear when MessageType does not require Annotation.", sendingAction.AnnotationInfo);
			sendingAction.MessageType = AESOutgoingMessageTypeList.Codes.ExportCancellation;
			sendingAction.ShouldSend = true;
			AssertHasMessageErrorContaining("Should have errors when MessageType does require Annotation and Annotation is empty.", sendingAction.AnnotationInfo, MandatoryValidation.YouHaveNotEntered);
			sendingAction.Annotation = "ExportCancellation";
			AssertNoMessageErrors("Should be clear when valid value on MessageType.", sendingAction.AnnotationInfo);
		}

		public void TestCheckEntryStatus()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_Status = LogicalStatusList.Codes.Sent;
			entryHeader.CH_EntryStatus = AESEntryStatusList.Codes.Cancelled;

			var errorMessage = "Entry has been canceled. No further processing allowed.";

			var sendingAction = new AESMessageSendingAction(entryHeader);
			var targetInfo = sendingAction.EntryStatusInfo;

			sendingAction.ShouldSend = true;
			AssertHasError("Should have 'Entry has been canceled. No further processing allowed.' message error while CH_EntryStatus is CAN", targetInfo, errorMessage);

			entryHeader.CH_EntryStatus = AESEntryStatusList.Codes.Prelodged;
			sendingAction.Validation.ValidateEntryStatus();
			AssertNoError("Should not have 'Entry has been canceled. No further processing allowed.' message error while CH_EntryStatus is not CAN", targetInfo, errorMessage);
		}

		public void TestCheckEntryStatus_REQ()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var sendingAction = new AESMessageSendingAction(entryHeader);
			var targetInfo = sendingAction.MessageTypeInfo;
			entryHeader.CH_EntryStatus = "REQ";
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			entryHeader.CH_CEI_Instruction = instruction.PK;
			sendingAction.MessageType = AESOutgoingMessageTypeList.Codes.ExportOriginal;

			var errorMessage = "Message type Export Declaration (IE515) cannot be used when the entry status is Requested Proof of Exit";
			sendingAction.Validation.ValidateMessageType();
			AssertHasMessageError("Cannot send 515 when CH_Status = 'REQ'", targetInfo, errorMessage);

			sendingAction.MessageType = AESOutgoingMessageTypeList.Codes.ExportCancellation;
			sendingAction.Validation.ValidateMessageType();
			AssertNoMessageError(targetInfo, errorMessage);

			sendingAction.MessageType = AESOutgoingMessageTypeList.Codes.ExportAmendment;
			errorMessage = "Message type Export Declaration Amendment (IE513) cannot be used when the entry status is Requested Proof of Exit";
			sendingAction.Validation.ValidateMessageType();
			AssertHasMessageError("Cannot send 513 when CH_Status = 'REQ'", targetInfo, errorMessage);

			sendingAction.MessageType = AESOutgoingMessageTypeList.Codes.ExportCancellation;
			sendingAction.Validation.ValidateMessageType();
			AssertNoMessageError("Cannot send 513 when CH_Status = 'REQ'", targetInfo, errorMessage);
		}
	}
}
