using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.ExitControl.Business.Testing
{
	class DocumentsSendingActionValidationTest : TestCaseWithFactory
	{
		public void TestCheckMRN()
		{
			var validation = documentsSendingAction.Validation;
			var targetInfo = documentsSendingAction.MovementReferenceInfo;
			validation.ValidateAll();
			AssertHasError("MRN validate should work when empty MRN.", targetInfo, "The Exit Report need to have a Movement Reference Number to send.");

			var consignment = Factory.New<CusExitConsignment>();
			consignment.CXC_MovementReference = "MRN001";
			cusExitReport.CER_CXC_Consignment = consignment.PK;
			validation.ValidateAll();
			AssertNoErrors("Should pass the Validation with valid MRN.", targetInfo);
		}

		public void TestCheckAdditionalInfosAndSupportingDocuments()
		{
			var addInfoMessage = "Cannot send message without any Additional Information input.";
			var supDocMessage = "Cannot send message without any eDoc selected.";

			var validation = documentsSendingAction.Validation;
			validation.ValidateAll();
			AssertHasRowError("Should have error with empty AdditionalInfo list.", documentsSendingAction, addInfoMessage);
			AssertHasRowError("Should have error with empty SupportingDocument list.", documentsSendingAction, supDocMessage);

			documentsSendingAction.AddInfoCollection.AddNew().DocumentType = "9001";
			validation.ValidateAll();
			AssertNoRowError("AddInfo validate should pass with non-empty AddInfoCollection.", documentsSendingAction, addInfoMessage);
			AssertHasRowError("Should have error with empty SupportingDocument list.", documentsSendingAction, supDocMessage);

			var sup1 = cusExitReport.Header.DocManagerInfo.AddFileOrDocument(new byte[1], "Sup1.pdf", "CIV");
			documentsSendingAction.SupportingDocuments.AddNew().EDoc = sup1.UniqueKey;
			validation.ValidateAll();
			AssertNoRowErrors("BO should pass both collection validations with both valid items.", documentsSendingAction);
		}

		protected override void SetUp()
		{
			base.SetUp();
			cusExitReport = Factory.New<CusExitHeader>().CusExitReports.AddNew();
			documentsSendingAction = new DocumentsSendingAction(cusExitReport);
		}

		CusExitReport cusExitReport;
		DocumentsSendingAction documentsSendingAction;
	}
}
