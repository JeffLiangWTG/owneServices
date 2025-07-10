using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class ReferenceInvoiceManualValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_ReferenceNumber()
		{
			var referenceCusSupporting = Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew().ReferenceInvoiceManualCollection.AddNew();
			var targetInfo = referenceCusSupporting.CSI_ReferenceNumberInfo;
			var warningMessage1 = "The entered CNPJ is not valid.";
			referenceCusSupporting.CSI_ReferenceNumber = ZString.Empty;
			referenceCusSupporting.RunPreSaveValidation();
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			referenceCusSupporting.CSI_ReferenceNumber = "03142520000128";
			referenceCusSupporting.RunPreSaveValidation();
			AssertHasMessageErrorContaining(targetInfo, warningMessage1);
			referenceCusSupporting.CSI_ReferenceNumber = "03142520000127";
			referenceCusSupporting.RunPreSaveValidation();
			AssertNoMessageErrorContaining(targetInfo, warningMessage1);
		}

		public void TestCheckCSI_AdditionalDescription()
		{
			var cSupporting = Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew().ReferenceInvoiceManualCollection.AddNew();
			var targetInfo = cSupporting.CSI_AdditionalDescriptionInfo;
			cSupporting.CSI_AdditionalDescription = "2020/08";
			cSupporting.RunPreSaveValidation();
			AssertNoErrorContaining(targetInfo, "Incorrect format YYYY/MM.");
			cSupporting.CSI_AdditionalDescription = "Test";
			cSupporting.RunPreSaveValidation();
			AssertHasErrorContaining(targetInfo, "Incorrect format YYYY/MM.");
		}
	}
}
