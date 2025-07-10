using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.H7.Business.Testing
{
	public class SupportingDocumentValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_CodeMandatoryValidation()
		{
			var supportingDocument = Factory.New<SupportingDocument>();
			supportingDocument.CSI_ReferenceNumber = "abcd";

			supportingDocument.Validation.ValidateCSI_Code();
			AssertHasMessageErrorContaining(supportingDocument.CSI_CodeInfo, MandatoryValidation.YouHaveNotEntered);

			supportingDocument.CSI_ReferenceNumber = "";
			supportingDocument.Validation.ValidateCSI_Code();
			AssertNoMessageErrors(supportingDocument.CSI_CodeInfo);
		}

		public void TestCheckCSI_CodeListValidation()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("ES"))
			{
				var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
				helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "supporting document type");
				var mockTypeCodes = new List<string> { "Y986", "Y987" };
				mockTypeCodes.ForEach(li => helper.CreateCusCodeList(Core.Constants.CountryCodes.Spain,
					Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection,
					li,
					"desc",
					ZDateTime.MinSmallDateTimeValue,
					ZDateTime.MaxSmallDateTime));
				Factory.Save();

				var header = Factory.New<AsycudaManifestHeader>();
				header.AMA_ApplicationCode = "LV2";
				var documents = header.SupportingDocuments;
				var supportingDoc = documents.AddNew();

				supportingDoc.CSI_Code = "Y989";
				supportingDoc.Validation.ValidateCSI_Code();
				AssertHasMessageErrorContaining(supportingDoc.CSI_CodeInfo, ListValidation.InvalidCodeMessageError);

				supportingDoc.CSI_Code = "Y987";
				supportingDoc.Validation.ValidateCSI_Code();
				AssertNoNotifications(supportingDoc.CSI_CodeInfo);
			}
		}

		public void TestCheckCSI_ReferenceNumber()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var documents = header.SupportingDocuments;
			var supportingDocument = documents.AddNew();
			supportingDocument.CSI_Code = "TEST";

			supportingDocument.Validation.ValidateCSI_ReferenceNumber();
			AssertHasMessageErrorContaining(supportingDocument.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

			supportingDocument.CSI_ReferenceNumber = "Reference Number";
			supportingDocument.Validation.ValidateCSI_ReferenceNumber();
			AssertNoMessageErrors(supportingDocument.CSI_ReferenceNumberInfo);

			supportingDocument.CSI_Code = "";
			supportingDocument.CSI_ReferenceNumber = "";
			supportingDocument.Validation.ValidateCSI_ReferenceNumber();
			AssertNoMessageErrors(supportingDocument.CSI_ReferenceNumberInfo);
		}
	}
}
