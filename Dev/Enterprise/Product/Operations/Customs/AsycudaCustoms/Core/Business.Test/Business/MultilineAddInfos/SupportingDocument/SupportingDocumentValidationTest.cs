using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	class SupportingDocumentValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_Code()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("UDDOC", "UDDOC-User Defined Supporting Documents");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Botswana, "UDDOC", "CD2", "RefCusCodeList Distracter", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			Factory.Save();
			var invoiceHeader = Factory.NewWithValidTestData<JobDeclaration>().Invoices.AddNew();
			var supDoc = ((ISupportingDocumentsProvider)invoiceHeader).SupportingDocuments.AddNew();
			var targetInfo = supDoc.CSI_CodeInfo;
			supDoc.CSI_Code = "XXX";
			AssertHasMessageErrorContaining(targetInfo, ListValidation.InvalidCodeMessageError);
			supDoc.CSI_Code = "CD2";
			AssertNoMessageErrorContaining(targetInfo, ListValidation.InvalidCodeMessageError);
		}
	}
}
