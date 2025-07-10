using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;

namespace Enterprise.Customs.IE.Business.Testing
{
	class UploadDocumentsSendingActionValidationTest : TestCaseWithFactory
	{
		public void TestCheckMovementReferenceNumber()
		{
			var validation = documentsSendingAction.Validation;
			var targetInfo = documentsSendingAction.MovementReferenceNumberInfo;
			var errorMessage = "MRN cannot be empty. MRN is generated when an acceptance message is received from the customs.";

			documentsSendingAction.ShouldSend = ZBool.False;
			validation.ValidateAll();
			AssertNoError("No error if action is not selected for sending.", targetInfo, errorMessage);

			documentsSendingAction.ShouldSend = ZBool.True;
			validation.ValidateAll();
			AssertHasError(targetInfo, errorMessage);

			testBizObjs.entryHeaderWrapper.EntryHeader.MovementReferenceNumberSetter("MRN0000001");
			validation.ValidateAll();
			AssertNoError("Should pass the Validation with valid MRN.", targetInfo, errorMessage);
		}

		public void TestCheckLocalReferenceNumber()
		{
			var validation = documentsSendingAction.Validation;
			var targetInfo = documentsSendingAction.LocalReferenceNumberInfo;
			var errorMessage = "LRN cannot be empty. LRN is generated when a customs declaration (IM415) is sent to the customs.";

			documentsSendingAction.ShouldSend = ZBool.False;
			validation.ValidateAll();
			AssertNoError("No error if action is not selected for sending.", targetInfo, errorMessage);

			documentsSendingAction.ShouldSend = ZBool.True;
			validation.ValidateAll();
			AssertHasError(targetInfo, errorMessage);

			testBizObjs.entryHeaderWrapper.EntryHeader.CH_BGMReference = "LRN00001";
			validation.ValidateAll();
			AssertNoError("Should pass the Validation with valid LRN.", targetInfo, errorMessage);
		}

		public void TestAlternativeDateOfAcceptance()
		{
			var fallbackProcedureMessage = "Alternative Date of Acceptance is required for Fallback Procedure";

			documentsSendingAction.ShouldSend = ZBool.False;
			documentsSendingAction.CustomsJustification = "Test";
			documentsSendingAction.Validation.ValidateAll();
			AssertNoRowError("No error if action is not selected for sending.", documentsSendingAction, fallbackProcedureMessage);

			documentsSendingAction.ShouldSend = ZBool.True;
			documentsSendingAction.CustomsJustification = string.Empty;
			AssertNoRowError("Should not have error when nothing filled in", documentsSendingAction, fallbackProcedureMessage);

			documentsSendingAction.CustomsJustification = "Test";
			documentsSendingAction.Validation.ValidateAll();
			AssertHasRowError("Should have error when not all fields filled in", documentsSendingAction, fallbackProcedureMessage);
		}

		public void TestCustomsReference()
		{
			var fallbackProcedureMessage = "Customs Reference is required for Fallback Procedure";

			documentsSendingAction.ShouldSend = ZBool.False;
			documentsSendingAction.CustomsJustification = "Test";
			documentsSendingAction.Validation.ValidateAll();
			AssertNoRowError("No error if action is not selected for sending.", documentsSendingAction, fallbackProcedureMessage);

			documentsSendingAction.ShouldSend = ZBool.True;
			documentsSendingAction.CustomsJustification = string.Empty;
			AssertNoRowError("Should not have error when nothing filled in", documentsSendingAction, fallbackProcedureMessage);

			documentsSendingAction.CustomsJustification = "Test";
			documentsSendingAction.Validation.ValidateAll();
			AssertHasRowError("Should have error when not all fields filled in", documentsSendingAction, fallbackProcedureMessage);
		}

		public void TestCustomsJustification()
		{
			var fallbackProcedureMessage = "Customs Justification is required for Fallback Procedure";

			documentsSendingAction.ShouldSend = ZBool.False;
			documentsSendingAction.CustomsReferenceNumber = "Test";
			documentsSendingAction.Validation.ValidateAll();
			AssertNoRowError("No error if action is not selected for sending.", documentsSendingAction, fallbackProcedureMessage);

			documentsSendingAction.ShouldSend = ZBool.True;
			documentsSendingAction.CustomsReferenceNumber = string.Empty;

			AssertNoRowError("Should not have error when nothing filled in", documentsSendingAction, fallbackProcedureMessage);

			documentsSendingAction.CustomsReferenceNumber = "Test";
			documentsSendingAction.Validation.ValidateAll();
			AssertHasRowError("Should have error when not all fields filled in", documentsSendingAction, fallbackProcedureMessage);
		}

		public void TestCheckHasOpenDocumentsOrIsControl()
		{
			var warningMessage = "The customs authorities have not requested any documents.";

			documentsSendingAction.ShouldSend = ZBool.False;
			documentsSendingAction.Validation.ValidateAll();
			AssertNoRowWarningContaining("No warnings if action is not selected for sending.", documentsSendingAction, warningMessage);

			documentsSendingAction.ShouldSend = ZBool.True;
			documentsSendingAction.EntryHeader.CH_EntryStatus = "$%$";
			var reqDoc = documentsSendingAction.EntryHeader.EntryInstruction.RequestedDocuments.AddNew();
			reqDoc.CSI_Status = "#$#";
			documentsSendingAction.Validation.ValidateAll();
			AssertHasRowWarning("Should have warnings when should send and has NO OPEN documents requested by customs", documentsSendingAction, warningMessage);

			var reqDoc2 = documentsSendingAction.EntryHeader.EntryInstruction.RequestedDocuments.AddNew();
			reqDoc2.CSI_Status = EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.RequestOpened;
			documentsSendingAction.Validation.ValidateAll();
			AssertNoRowWarningContaining("No warnings when should send and has OPEN documents", documentsSendingAction, warningMessage);

			reqDoc2.CSI_Status = "#$#";
			documentsSendingAction.EntryHeader.CH_EntryStatus = AISEntryStatusList.Codes.Control;
			AssertNoRowWarningContaining("No warnings when should send and CH_EntryStatus = CON", documentsSendingAction, warningMessage);
		}

		protected override void SetUp()
		{
			base.SetUp();
			testBizObjs = MessageProviderTestHelper.SetupBasicTestBizObjs(Factory);
			documentsSendingAction = new UploadDocumentsSendingAction(testBizObjs.entryHeaderWrapper.EntryHeader);
		}

		(EntryHeaderWrapper entryHeaderWrapper, EntryLineWrapper entryLineWrapper) testBizObjs;
		UploadDocumentsSendingAction documentsSendingAction;
	}
}
