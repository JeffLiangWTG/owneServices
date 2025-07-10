using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class ECCCLPCOViewValidationTest : TestCaseWithFactory
	{
		public void TestCheckCA_Qty()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			invoiceLine.CA_ECCCInd = YesNoList.Codes.Yes;

			var pgaHeader = invoiceLine.ECCCPGAHeader;
			var lpco = pgaHeader.LPCOViews.AddNew();

			lpco.Validation.ValidateCLP_AlternativeQuotaQuantity();
			AssertNoMessageErrorContaining(lpco.CLP_AlternativeQuotaQuantityInfo, "value cannot be zero.");

			pgaHeader.CA_ODSProgramInd = YesNoList.Codes.Yes;
			lpco.Validation.ValidateCLP_AlternativeQuotaQuantity();
			AssertHasMessageErrorContaining(lpco.CLP_AlternativeQuotaQuantityInfo, "value cannot be zero.");

			lpco.CLP_AlternativeQuotaQuantity = 10m;
			AssertNoMessageErrorContaining(lpco.CLP_AlternativeQuotaQuantityInfo, "value cannot be zero.");
		}

		public void TestCheckCA_DIFRefNumberOrLocation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			invoiceLine.CA_ECCCInd = YesNoList.Codes.Yes;

			var pgaHeader = invoiceLine.ECCCPGAHeader;
			pgaHeader.CA_WENProgramInd = YesNoList.Codes.Yes;

			var lpco = pgaHeader.LPCOViews.AddNew();
			lpco.Validation.ValidateCLP_DIFRefNumberOrLocation();
			AssertHasMessageErrorContaining(lpco.CLP_DIFRefNumberOrLocationInfo, MandatoryValidation.YouHaveNotEntered);

			lpco.CLP_DIFRefNumberOrLocation = "AUSYD";
			AssertNoMessageErrorContaining(lpco.CLP_DIFRefNumberOrLocationInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckCA_LPCOEndDate()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			invoiceLine.CA_ECCCInd = YesNoList.Codes.Yes;

			var pgaHeader = invoiceLine.ECCCPGAHeader;
			pgaHeader.CA_WENProgramInd = YesNoList.Codes.Yes;

			var lpco = pgaHeader.LPCOViews.AddNew();
			lpco.Validation.ValidateCLP_EndDate();
			AssertHasMessageErrorContaining(lpco.CLP_EndDateInfo, MandatoryValidation.YouHaveNotEntered);

			lpco.CLP_EndDate = ZDate.Today;
			AssertNoMessageErrorContaining(lpco.CLP_EndDateInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckCA_LPCOIssueDate()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			invoiceLine.CA_ECCCInd = YesNoList.Codes.Yes;

			var pgaHeader = invoiceLine.ECCCPGAHeader;
			pgaHeader.CA_WENProgramInd = YesNoList.Codes.Yes;

			var lpco = pgaHeader.LPCOViews.AddNew();
			lpco.Validation.ValidateCLP_IssueDate();
			AssertHasMessageErrorContaining(lpco.CLP_IssueDateInfo, MandatoryValidation.YouHaveNotEntered);

			lpco.CLP_IssueDate = ZDate.Today;
			AssertNoMessageErrorContaining(lpco.CLP_IssueDateInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckCA_RefNo()
		{
			var messageError = @"Please input valid format as ""ODSHA-{0}-YY-###"", ""YY"" represents the two digit calendar year, and ""#"" should be an alpha-numeric.";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			invoiceLine.CA_ECCCInd = YesNoList.Codes.Yes;

			var pgaHeader = invoiceLine.ECCCPGAHeader;
			pgaHeader.CA_ODSProgramInd = YesNoList.Codes.Yes;
			var lpco = pgaHeader.LPCOViews.AddNew();

			lpco.CLP_RefNo = "ODSHA-PER-17-6a9";
			AssertNoMessageErrorContaining(lpco.CLP_RefNoInfo, string.Format(messageError, "PER"));

			lpco.CLP_Type = LPCODocumentTypeQualifier.Codes._8010;
			lpco.CLP_RefNo = "AAA";
			AssertHasMessageErrorContaining(lpco.CLP_RefNoInfo, string.Format(messageError, "PER"));

			lpco.CLP_RefNo = "ODSHA-ALL-17-6a9";
			AssertNoMessageErrorContaining(lpco.CLP_RefNoInfo, string.Format(messageError, "ALL"));

			lpco.CLP_Type = LPCODocumentTypeQualifier.Codes._8011;
			lpco.CLP_RefNo = "ODSHA-ALL-17-6~9";
			AssertHasMessageErrorContaining(lpco.CLP_RefNoInfo, string.Format(messageError, "ALL"));

			lpco.CLP_RefNo = "ODSHA-TRA-17-6a9";
			AssertNoMessageErrorContaining(lpco.CLP_RefNoInfo, string.Format(messageError, "TRA"));

			lpco.CLP_Type = LPCODocumentTypeQualifier.Codes._8012;
			lpco.CLP_RefNo = "ODSHA-TRA-t7-6a9";
			AssertHasMessageErrorContaining(lpco.CLP_RefNoInfo, string.Format(messageError, "TRA"));
		}
	}
}
