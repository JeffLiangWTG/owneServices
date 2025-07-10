using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.Business.Testing
{
	class DocumentsSendingActionValidationTest : TestCaseWithFactory
	{
		public void TestCheckMRN()
		{
			var validation = documentsSendingAction.Validation;
			var targetInfo = documentsSendingAction.MovementReferenceInfo;
			validation.ValidateAll();
			AssertHasError("MRN validate should work when empty MRN.", targetInfo, "The entry need to have a Movement Reference Number to send.");

			testBizObjs.entryHeaderWrapper.EntryHeader.MovementReferenceNumberSetter("MRN00001");
			targetInfo.ClearAllNotifications();
			validation.ValidateAll();
			AssertNoErrors("Should pass the Validation with valid MRN.", targetInfo);
		}

		public void TestCheckAdditionalInfosAndSupportingDocuments()
		{
			var addInfoMessage = "Cannot send message without any Additional Information(populated from Entry Instruction -> Documents Requested) input.";
			var supDocMessage = "Cannot send message without any eDoc selected.";

			var validation = documentsSendingAction.Validation;
			validation.ValidateAll();
			AssertHasRowError("Should have error with empty AdditionalInfo list.", documentsSendingAction, addInfoMessage);
			AssertHasRowError("Should have error with empty SupportingDocument list.", documentsSendingAction, supDocMessage);

			var requestedDocuments = testBizObjs.entryHeaderWrapper.EntryHeader.EntryInstruction.RequestedDocuments;
			var requestedDocument1 = requestedDocuments.AddNew();
			requestedDocument1.CSI_Code = "9001";
			requestedDocument1.CSI_Status = EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.RequestOpened;
			documentsSendingAction.AddInfoCollection.LoadElements();
			validation.ValidateAll();
			AssertNoRowError("AddInfo validate should pass with non-empty AddInfoCollection.", documentsSendingAction, addInfoMessage);
			AssertHasRowError("Should have error with empty SupportingDocument list.", documentsSendingAction, supDocMessage);

			var sup1 = testBizObjs.entryHeaderWrapper.Declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Sup1.pdf", "CIV");
			documentsSendingAction.SupportingDocuments.AddNew().EDoc = sup1.UniqueKey;
			validation.ValidateAll();
			AssertNoRowErrors("BO should pass both collection validations with both valid items.", documentsSendingAction);
		}

		protected override void SetUp()
		{
			base.SetUp();
			testBizObjs = MessageProviderTestHelper.SetupBasicTestBizObjs(Factory);
			documentsSendingAction = new DocumentsSendingAction(testBizObjs.entryHeaderWrapper.EntryHeader);
		}

		(EntryHeaderWrapper entryHeaderWrapper, EntryLineWrapper entryLineWrapper) testBizObjs;
		DocumentsSendingAction documentsSendingAction;
	}
}
