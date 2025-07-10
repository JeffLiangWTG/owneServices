using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class PHACPGAHeaderAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCA_Category()
		{
			header.AddInfoValidation.ValidateCA_Category();
			AssertNoMessageErrorContaining(header.CA_CategoryInfo, MandatoryValidation.YouHaveNotEntered);

			header.CA_HAPProgramInd = Customs.Business.YesNoList.Codes.Yes;
			header.AddInfoValidation.ValidateCA_Category();
			AssertHasMessageErrorContaining(header.CA_CategoryInfo, MandatoryValidation.YouHaveNotEntered);

			header.CA_Category = "~";
			AssertHasMessageError(header.CA_CategoryInfo, ListValidation.InvalidCodeMessageError);
			header.CA_Category = PHACCategories.Codes.PH01;
			AssertNoMessageError(header.CA_CategoryInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckCA_IntendedUseCode()
		{
			header.AddInfoValidation.ValidateCA_IntendedUseCode();
			AssertNoMessageErrorContaining(header.CA_IntendedUseCodeInfo, MandatoryValidation.YouHaveNotEntered);

			header.CA_HAPProgramInd = Customs.Business.YesNoList.Codes.Yes;
			header.AddInfoValidation.ValidateCA_IntendedUseCode();
			AssertHasMessageErrorContaining(header.CA_IntendedUseCodeInfo, MandatoryValidation.YouHaveNotEntered);

			header.CA_IntendedUseCode = "~";
			AssertHasMessageError(header.CA_IntendedUseCodeInfo, ListValidation.InvalidCodeMessageError);
			header.CA_IntendedUseCode = PHACEndUseCodes.Codes.PH01;
			AssertNoMessageError(header.CA_IntendedUseCodeInfo, ListValidation.InvalidCodeMessageError);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.CA_PHACInd = YesNoList.Codes.Yes;

			header = invoiceLine.PHACPGAHeader;
		}
		PHACPGAHeader header;

		#endregion
	}
}
