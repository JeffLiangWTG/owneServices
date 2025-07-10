using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CH.Business.Testing;

class PreviousDocumentValidationTest : BusinessObjectValidationTestCase
{
	public void TestCode()
	{
		RefCusCodeTestHelper.CreatePreviousDocumentsList(Factory);

		PreviousDocument.Parent.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;

		CombineAssertions("CSI_Code", () =>
		{
			PreviousDocument.Validation.ValidateCSI_Code();
			AssertHasMessageErrorContaining(PreviousDocument.CSI_CodeInfo, MandatoryValidation.YouHaveNotEntered);

			PreviousDocument.CSI_Code = RefCusCodeTestHelper.InvalidPreviousDocumentsListExport;
			AssertHasMessageErrorContaining(PreviousDocument.CSI_CodeInfo, ListValidation.InvalidCodeMessageError);

			PreviousDocument.CSI_Code = RefCusCodeTestHelper.ValidPreviousDocumentsListExport;
			AssertNoMessageErrorContaining(PreviousDocument.CSI_CodeInfo, ListValidation.InvalidCodeMessageError);
		});
	}

	public void TestReferenceNumber()
	{
		CombineAssertions("CSI_ReferenceNumber", () =>
		{
			PreviousDocument.Validation.ValidateCSI_ReferenceNumber();
			AssertHasMessageErrorContaining(PreviousDocument.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

			PreviousDocument.CSI_ReferenceNumber = "235";
			AssertNoMessageErrorContaining(PreviousDocument.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}

	PreviousDocument PreviousDocument => previousDocument ?? (previousDocument = Factory.New<JobDeclaration>().Invoices.AddNew().PreviousDocuments.AddNew());
	PreviousDocument previousDocument;
}
