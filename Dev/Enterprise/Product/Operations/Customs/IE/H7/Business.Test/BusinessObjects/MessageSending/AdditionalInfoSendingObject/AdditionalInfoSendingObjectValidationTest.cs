using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.IE.H7.Business.Testing
{
	sealed class AdditionalInfoSendingObjectValidationTest : BusinessObjectValidationTestCase
	{
		public void TestDocumentType()
		{
			var validCodeForLV1 = "0IE5";
			var validCodeForLV2 = "0IE";

			var today = ZDateTime.Now;
			var yesterday = today.AddDays(-1);
			var tomorrow = today.AddDays(1);

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.IEUCC6V1, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, validCodeForLV1, "TIR Carnet", yesterday, tomorrow);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, validCodeForLV2, "TIR Carnet", yesterday, tomorrow);
			Factory.Save();

			var sendingObject = CreateSendingObject();
			var header = sendingObject.Bill.Header;

			CombineAssertions(() =>
			{
				sendingObject.DocumentType = string.Empty;
				sendingObject.ReferenceNumber = string.Empty;
				sendingObject.Validation.ValidateDocumentType();
				AssertNoMessageErrors("Expected no error when DocumentType and ReferenceNumber are empty", sendingObject.DocumentTypeInfo);

				sendingObject.ReferenceNumber = "Ref001";
				sendingObject.Validation.ValidateDocumentType();
				AssertHasMessageError("Expected error when DocumentType is empty but ReferenceNumber is entered", sendingObject.DocumentTypeInfo, notEnteredDocumentTypeMessageError);

				// Test for LV1
				header.AMA_ApplicationCode = CodeDescriptionPairLists.IEH7ManifestTypes.Codes.LV1;
				sendingObject.DocumentType = validCodeForLV2;
				sendingObject.Validation.ValidateDocumentType();
				AssertHasMessageError("Expected error when DocumentType is invalid for LV1", sendingObject.DocumentTypeInfo, invalidDocumentTypeMessageError);

				sendingObject.DocumentType = validCodeForLV1;
				sendingObject.Validation.ValidateDocumentType();
				AssertNoMessageErrors("Expected no error when DocumentType is valid for LV1", sendingObject.DocumentTypeInfo);

				// Test for LV2
				header.AMA_ApplicationCode = CodeDescriptionPairLists.IEH7ManifestTypes.Codes.LV2;
				sendingObject.DocumentType = validCodeForLV1;
				sendingObject.Validation.ValidateDocumentType();
				AssertHasMessageError("Expected error when DocumentType is invalid for LV2", sendingObject.DocumentTypeInfo, invalidDocumentTypeMessageError);

				sendingObject.DocumentType = validCodeForLV2;
				sendingObject.Validation.ValidateDocumentType();
				AssertNoMessageErrors("Expected no error when DocumentType is valid for LV2", sendingObject.DocumentTypeInfo);
			});
		}

		public void TestReferenceNumber()
		{
			var sendingObject = CreateSendingObject();

			CombineAssertions(() =>
			{
				sendingObject.DocumentType = string.Empty;
				sendingObject.ReferenceNumber = string.Empty;
				sendingObject.Validation.ValidateReferenceNumber();
				AssertNoMessageErrors("Expected no error when ReferenceNumber and DocumentType are empty", sendingObject.ReferenceNumberInfo);

				sendingObject.DocumentType = "DOC";
				sendingObject.Validation.ValidateReferenceNumber();
				AssertHasMessageError("Expected error when ReferenceNumber is empty but DocumentType is entered", sendingObject.ReferenceNumberInfo, notEnteredReferenceNumberMessageError);

				sendingObject.ReferenceNumber = "Ref001";
				sendingObject.Validation.ValidateReferenceNumber();
				AssertNoMessageErrors("Expected no error when ReferenceNumber and DocumentType are entered", sendingObject.ReferenceNumberInfo);
			});
		}

		AdditionalInfoSendingObject CreateSendingObject()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var requestedDocument = bill.RequestedDocuments.AddNew();

			return new AdditionalInfoSendingObject(bill, null, requestedDocument);
		}

		readonly string invalidDocumentTypeMessageError = "[BR0020] The code you have selected is not in the list.";
		readonly string notEnteredDocumentTypeMessageError = "[BR20313] You have not entered a Document Type.";
		readonly string notEnteredReferenceNumberMessageError = "[BR20313] You have not entered a Reference Number.";
	}
}
