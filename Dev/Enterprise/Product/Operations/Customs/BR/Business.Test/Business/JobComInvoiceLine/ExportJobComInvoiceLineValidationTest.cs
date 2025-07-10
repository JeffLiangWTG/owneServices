using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business.CountryCompliance;

namespace Enterprise.Customs.BR.Business.Testing
{
	class ExportJobComInvoiceLineValidationTest : JobComInvoiceLineValidationTest
	{
		public void TestCheckJI_RN_NKCountryOfExport()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(invoiceLine.JI_RN_NKCountryOfExportInfo);
		}

		public override void TestCheckJI_Procedure()
		{
			ReferenceTestDataHelper.CreateRefCusProcedureForExport(declaration.Factory);
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(invoiceLine.JI_ProcedureInfo, "00000", "80001");
		}

		public void TestCheckComplementaryDescription()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_LegalDocument = LegalDocumentList.Codes.NoInvoice;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.ComplementaryDescription = "Test";

			AssertHasMessageErrorContaining(invoiceLine.ComplementaryDescriptionInfo, "Legal Document is SNF – No Invoice. Complementary description should not have a value.");

			invoiceLine.ComplementaryDescription = ZString.Empty;
			AssertNoMessageErrorContaining(invoiceLine.ComplementaryDescriptionInfo, "Legal Document is SNF – No Invoice. Complementary description should not have a value.");
		}

		protected override string JobMessageType => BRJobMessageTypeList.Codes.Export;

		public void TestCheckJI_NFENumber()
		{
			invoiceLine.JI_NFeNumber = "ABC";
			AssertHasMessageError(invoiceLine.JI_NFeNumberInfo, "Invalid Number");
			invoiceLine.JI_NFeNumber = "123";
			AssertNoMessageError(invoiceLine.JI_NFeNumberInfo, "Invalid Number");
		}

		public void TestCheckJI_NFeItemNumber()
		{
			invoiceLine.JI_NFeItemNumber = "ABC";
			AssertHasMessageError(invoiceLine.JI_NFeItemNumberInfo, "Invalid Number");
			invoiceLine.JI_NFeItemNumber = "123";
			AssertNoMessageError(invoiceLine.JI_NFeItemNumberInfo, "Invalid Number");
		}

		public void TestCheckJI_NFeNumber_NFE()
		{
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_LegalDocument = BrazilComplianceInfo.ComplianceSubTypeCodes.CNE;
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_NFeNumber = ZString.Empty;
			AssertNoMessageErrorContaining(invoiceLine.JI_NFeNumberInfo, ExportJobComInvoiceLineValidation.MessageLegalDocumentIsNFEAndNFENumberFieldIsBlank);
			instruction.CEI_LegalDocument = BrazilComplianceInfo.ComplianceSubTypeCodes.NFE;
			invoiceLine.JI_NFeNumber = ZString.Empty;
			AssertHasMessageErrorContaining(invoiceLine.JI_NFeNumberInfo, ExportJobComInvoiceLineValidation.MessageLegalDocumentIsNFEAndNFENumberFieldIsBlank);
			invoiceLine.JI_NFeNumber = "99999999999999999";
			AssertNoMessageErrorContaining(invoiceLine.JI_NFeNumberInfo, ExportJobComInvoiceLineValidation.MessageLegalDocumentIsNFEAndNFENumberFieldIsBlank);
		}

		public void TestCheckJI_NFeNumber()
		{
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_NFeItemNumber = "777";
			invoiceLine.JI_NFeNumber = ZString.Empty;
			AssertHasMessageErrorContaining(invoiceLine.JI_NFeNumberInfo, ExportJobComInvoiceLineValidation.MessageHasNFEItemNumberAndNFeNumberFieldIsBlank);
			invoiceLine.JI_NFeNumber = "8888888888";
			AssertNoMessageErrorContaining(invoiceLine.JI_NFeNumberInfo, ExportJobComInvoiceLineValidation.MessageHasNFEItemNumberAndNFeNumberFieldIsBlank);
			invoiceLine.JI_NFeItemNumber = ZString.Empty;
			invoiceLine.JI_NFeNumber = "99999999999999999";
			AssertHasMessageErrorContaining(invoiceLine.JI_NFeItemNumberInfo, ExportJobComInvoiceLineValidation.MessageHasNFENumberAndNFEItemNumberFieldIsBlank);
			invoiceLine.JI_NFeItemNumber = "888";
			AssertNoMessageErrorContaining(invoiceLine.JI_NFeItemNumberInfo, ExportJobComInvoiceLineValidation.MessageHasNFENumberAndNFEItemNumberFieldIsBlank);
		}

		public void TestCheckJI_FinancedValue()
		{
			invoiceLine.JI_FinancedValue = -0.1m;
			AssertHasError(invoiceLine.JI_FinancedValueInfo, "Financed Value cannot be negative.");
		}

		public void TestCheckJI_AgentCommissionPercentage()
		{
			invoiceLine.JI_AgentCommissionPercentage = -0.1m;
			AssertHasError(invoiceLine.JI_AgentCommissionPercentageInfo, "Agent Commission(%) cannot be negative.");
			invoiceLine.JI_AgentCommissionPercentage = 101m;
			AssertHasMessageError(invoiceLine.JI_AgentCommissionPercentageInfo, "Agent Commission must be up to 100");
			invoiceLine.JI_AgentCommissionPercentage = 100m;
			AssertNoMessageError(invoiceLine.JI_AgentCommissionPercentageInfo, "Agent Commission must be up to 100");
		}

		public void TestCheckJI_CPCs()
		{
			invoiceLine.JI_SecondCPC = "X";
			invoiceLine.JI_ThirdCPC = "X";
			invoiceLine.JI_FourthCPC = "X";
			AssertHasMessageErrorContaining(invoiceLine.JI_SecondCPCInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageErrorContaining(invoiceLine.JI_ThirdCPCInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageErrorContaining(invoiceLine.JI_FourthCPCInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckJI_IntendedTermDays()
		{
			invoiceLine.JI_IntendedTermDays = -1;
			AssertHasError(invoiceLine.JI_IntendedTermDaysInfo, "Intended Term (in days) cannot be negative.");
			AssertNoMessageErrors(invoiceLine.JI_IntendedTermDaysInfo);
			invoiceLine.JI_IntendedTermDays = 0;
			AssertNoNotifications(invoiceLine.JI_IntendedTermDaysInfo);
			invoiceLine.JI_IntendedTermDays = 9999;
			AssertNoNotifications(invoiceLine.JI_IntendedTermDaysInfo);
			invoiceLine.JI_IntendedTermDays = 10000;
			AssertHasMessageError(invoiceLine.JI_IntendedTermDaysInfo, "Intended term must be up to 4 digit");
			AssertNoErrors(invoiceLine.JI_IntendedTermDaysInfo);
		}

		public void TestCheckJI_DigitalServiceDossier()
		{
			invoiceLine.JI_DigitalServiceDossier = "ABC";
			AssertHasMessageError(invoiceLine.JI_DigitalServiceDossierInfo, "Digital Service Dossiers should be only numbers");
			invoiceLine.JI_DigitalServiceDossier = ZString.Empty;
			AssertNoNotifications(invoiceLine.JI_DigitalServiceDossierInfo);
			invoiceLine.JI_DigitalServiceDossier = "99999999999999999";
			AssertNoNotifications(invoiceLine.JI_DigitalServiceDossierInfo);
		}
	}
}
