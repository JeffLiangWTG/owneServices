using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class HCLPCOViewValidationTest : TestCaseWithFactory
	{
		public void TestCheckCA_RefNo()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.CA_HCInd = YesNoList.Codes.Yes;

			var header = invoiceLine.HCPGAHeader;
			header.CA_BBCProgramInd = YesNoList.Codes.Yes;
			header.CA_IntendedUseCodeBBC = HCIntendedUseCode.Codes.HC01;
			header.CA_CategoryBBC = HCCategories.Codes.HC02;

			var lpco = header.LPCOViews.AddNew();

			lpco.CLP_Type = "5003";
			lpco.Validation.ValidateCLP_RefNo();
			AssertNoMessageErrorContaining(lpco.CLP_RefNoInfo, MandatoryValidation.YouHaveNotEntered);

			lpco.CLP_Type = "5002";
			lpco.Validation.ValidateCLP_RefNo();
			AssertHasMessageErrorContaining(lpco.CLP_RefNoInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckCA_DIFRefNumberOrLocation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.CA_HCInd = YesNoList.Codes.Yes;

			var header = invoiceLine.HCPGAHeader;
			header.CA_BBCProgramInd = YesNoList.Codes.Yes;
			header.CA_IntendedUseCodeBBC = HCIntendedUseCode.Codes.HC01;
			header.CA_CategoryBBC = HCCategories.Codes.HC02;

			var lpco = header.LPCOViews.AddNew();

			lpco.CLP_Type = "5003";
			lpco.Validation.ValidateCLP_DIFRefNumberOrLocation();
			AssertHasWarning(lpco.CLP_DIFRefNumberOrLocationInfo, "It is strongly recommended to provide an image of the Proof of Prescription as this will facilitate communication in case of a referral.");

			lpco.CLP_DIFRefNumberOrLocation = "xxx";
			AssertNoWarnings(lpco.CLP_DIFRefNumberOrLocationInfo);

			lpco.CLP_Type = "5002";
			lpco.CLP_DIFRefNumberOrLocation = string.Empty;
			lpco.Validation.ValidateCLP_DIFRefNumberOrLocation();
			AssertNoWarning(lpco.CLP_DIFRefNumberOrLocationInfo, "It is strongly recommended to provide an image of the Proof of Prescription as this will facilitate communication in case of a referral.");

			header.CA_BBCProgramInd = YesNoList.Codes.No;
			header.CA_DSEProgramInd = YesNoList.Codes.Yes;
			lpco = header.LPCOViews.AddNew();
			lpco.Validation.ValidateCLP_DIFRefNumberOrLocation();
			AssertHasMessageErrorContaining(lpco.CLP_DIFRefNumberOrLocationInfo, MandatoryValidation.YouHaveNotEntered);

			lpco.CLP_DIFRefNumberOrLocation = "xxx";
			AssertNoMessageErrorContaining(lpco.CLP_TypeInfo, MandatoryValidation.YouHaveNotEntered);
		}
	}
}
