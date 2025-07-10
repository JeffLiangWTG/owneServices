using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(DocumentSendingActionValidation))]
	class DocumentSendingActionValidationTest : TestCaseWithFactory
	{
		public void TestCheckAdditionalInfosAndSupportingDocuments()
		{
			var addInfoMessage = "Cannot send message without any Additional Informations.";
			var supDocMessage = "Cannot send message without any eDoc selected.";

			var validation = documentSendingAction.Validation;
			validation.ValidateAll();

			AssertHasRowError("Should have error with empty AdditionalInfo list.", documentSendingAction, addInfoMessage);
			AssertHasRowError("Should have error with empty SupportingDocument list.", documentSendingAction, supDocMessage);

			var additonalInfo = documentSendingAction.AddInfoCollection.AddNew();
			additonalInfo.DocumentType = "N380";
			additonalInfo.DocumentInformation = "Commercial Inv.";
			validation.ValidateAll();

			AssertNoRowError("AddInfo validate should pass with non-empty AddInfoCollection.", documentSendingAction, addInfoMessage);
			AssertHasRowError("Should have error with empty SupportingDocument list.", documentSendingAction, supDocMessage);

			var sup1 = header.DocManagerInfo.AddFileOrDocument(new byte[1], "Sup1.pdf", "CIV");
			documentSendingAction.SupportingDocuments.AddNew().EDoc = sup1.UniqueKey;
			validation.ValidateAll();

			AssertNoRowErrors("No error with filled SupportingDocument list and Add Info", documentSendingAction);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			documentSendingAction = new DocumentSendingAction(header);
		}
		NctsHeader header;
		DocumentSendingAction documentSendingAction;
	}
}
