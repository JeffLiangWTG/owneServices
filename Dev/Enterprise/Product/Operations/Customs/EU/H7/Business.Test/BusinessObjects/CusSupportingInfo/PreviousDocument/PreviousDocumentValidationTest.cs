using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.EU.H7.Business.Testing
{
	sealed class PreviousDocumentValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_Code()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfImportDirection, "DC40I");
			helper.CreateNewOrGetExistingCusCodeList("LV",
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfImportDirection, "Code", "A Test Desc",
				ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			Factory.Save();

			var bill = Factory.NewWithValidTestData<AsycudaBill>();
			var previousDoc = bill.PreviousDocuments.AddNew();

			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(previousDoc.CSI_CodeInfo);
			ValidationTestHelper.AssertInvalidCodeMessageError(previousDoc.CSI_CodeInfo, "Test", "Code");

			var packedItem = bill.PackedItems.AddNew();
			previousDoc = packedItem.PreviousDocuments.AddNew();

			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(previousDoc.CSI_CodeInfo);
			ValidationTestHelper.AssertInvalidCodeMessageError(previousDoc.CSI_CodeInfo, "Test", "Code");
		}

		public void TestCheckCSI_ReferenceNumber()
		{
			var bill = Factory.NewWithValidTestData<AsycudaBill>();
			var previousDoc = bill.PreviousDocuments.AddNew();

			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(previousDoc.CSI_ReferenceNumberInfo);

			var packedItem = bill.PackedItems.AddNew();
			previousDoc = packedItem.PreviousDocuments.AddNew();

			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(previousDoc.CSI_ReferenceNumberInfo);
		}
	}
}
