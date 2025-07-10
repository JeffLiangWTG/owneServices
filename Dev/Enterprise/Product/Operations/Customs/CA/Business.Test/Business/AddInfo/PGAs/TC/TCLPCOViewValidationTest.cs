using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class TCLPCOViewValidationTest : TestCaseWithFactory
	{
		public void TestCheckCA_AuthorizationCountry()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.CA_TCInd = YesNoList.Codes.Yes;

			var pgaHeader = invoiceLine.TCPGAHeader;
			pgaHeader.CA_VPRProgramInd = YesNoList.Codes.Yes;
			pgaHeader.CA_SubProgram = TCPGAVehicleProgramCodes.Codes.VFS;

			var lpco = pgaHeader.LPCOViews.AddNew();
			lpco.Validation.ValidateCLP_RN_NKAuthorizationCountry();
			AssertNoMessageErrorContaining(lpco.CLP_RN_NKAuthorizationCountryInfo, MandatoryValidation.YouHaveNotEntered);

			lpco.CLP_Type = LPCODocumentTypeQualifier.Codes._4004;
			lpco.Validation.ValidateCLP_RN_NKAuthorizationCountry();
			AssertHasMessageErrorContaining(lpco.CLP_RN_NKAuthorizationCountryInfo, MandatoryValidation.YouHaveNotEntered);
		}
	}
}
