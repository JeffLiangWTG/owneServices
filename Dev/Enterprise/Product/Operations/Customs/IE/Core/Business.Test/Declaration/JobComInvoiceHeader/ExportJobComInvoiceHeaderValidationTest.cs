using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.IE;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	class ExportJobComInvoiceHeaderValidationTest : JobComInvoiceHeaderValidationTest<ExportJobComInvoiceHeaderValidation>
	{
		public void TestValidationForMissingMandatoryCharges()
		{
			var dec = Factory.New<JobDeclaration>();
			var invoiceHeader = dec.Invoices.AddNew();
			var instruction = Factory.New<CusEntryInstruction>();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			instruction.CEI_Style = ExportDeclarationTypeList.Codes.B1;
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			invoiceHeader.Validation.ValidateJZ_IncoTerm();
			AssertHasMessageErrorContaining(invoiceHeader.JZ_IncoTermInfo, "The following charges are missing");
		}

		public void TestJZ_Incoterm_MessageType()
		{
			(_, var invoice, _) = SetupData();
			var instruction = Factory.New<CusEntryInstruction>();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			instruction.CEI_Style = ExportDeclarationTypeList.Codes.B1;
			invoice.JZ_IncoTerm = ZString.Empty;
			AssertHasMessageErrorContaining(invoice.JZ_IncoTermInfo, "Please enter an Incoterm.");
		}

		public void TestCheckJZ_IncotermMandatoryIfStatisticalValueRequired()
		{
			(_, var invoice, _) = SetupData();
			var instruction = Factory.New<CusEntryInstruction>();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			instruction.CEI_Style = ExportDeclarationTypeList.Codes.B1;
			invoice.JZ_IncoTerm = string.Empty;
			AssertHasMessageErrorContaining($"JZ_Incoterm is required if statistical value is required on all related instructions", invoice.JZ_IncoTermInfo, MandatoryValidation.MustBeEntered);
			invoice.JZ_IncoTerm = "FOB";
			AssertNoMessageErrorContaining($"JZ_Incoterm is entered", invoice.JZ_IncoTermInfo, MandatoryValidation.MustBeEntered);
			instruction.CEI_Style = ExportDeclarationTypeList.Codes.B3;
			invoice.JZ_IncoTerm = string.Empty;
			AssertNoMessageErrors($"JZ_Incoterm is not required if statistical value is required on all related instructions", invoice.JZ_IncoTermInfo);
		}

		public void TestJZ_IncoTermPlace()
		{
			(_, var invoice, _) = SetupData();

			var errorMessageJZIncoTermPlace = $"Please do not enter an {invoice.JZ_IncoTermPlaceInfo.HumanReadableName}. This should be empty when {invoice.JZ_IncoTermInfo.HumanReadableName} is {Core.Constants.IncoTerms.Other}.";

			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.Other;
			invoice.JZ_IncoTermPlace = ZString.Empty;
			AssertNoMessageError("No error when JZ_IncoTermPlace is empty", invoice.JZ_IncoTermPlaceInfo, errorMessageJZIncoTermPlace);

			invoice.JZ_IncoTermPlace = "Some place";
			AssertHasMessageError("Has error when JZ_IncoTermPlace is populated", invoice.JZ_IncoTermPlaceInfo, errorMessageJZIncoTermPlace);

			var errorMessageJZIncoTermPlaceRequired = $"This field is mandatory when a country is entered into {invoice.ZG_AgreedPlaceCodeInfo.HumanReadableName}.";

			invoice.JZ_IncoTerm = "ABC";
			invoice.ZG_AgreedPlaceCode = "YZ";
			invoice.JZ_IncoTermPlace = ZString.Empty;
			AssertHasMessageErrorContaining("Has error when JZ_IncoTermPlace is empty", invoice.JZ_IncoTermPlaceInfo, errorMessageJZIncoTermPlaceRequired);

			invoice.JZ_IncoTermPlace = "Some place";
			AssertNoMessageError("No error when JZ_IncoTermPlace is populated", invoice.JZ_IncoTermPlaceInfo, errorMessageJZIncoTermPlaceRequired);
		}

		public void TestJZ_AdditionalTerms()
		{
			(_, var invoice, _) = SetupData();
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.Other;
			invoice.JZ_AdditionalTerms = ZString.Empty;
			AssertHasMessageErrorContaining("JZ_AdditionalTerms is empty", invoice.JZ_AdditionalTermsInfo, MandatoryValidation.YouHaveNotEntered);

			invoice.JZ_IncoTerm = "ABC";
			invoice.JZ_AdditionalTerms = ZString.Empty;
			var errorMessageJZAdditionalTerms = $"Please do not enter a {invoice.JZ_AdditionalTermsInfo.HumanReadableName}. This should be empty when {invoice.JZ_IncoTermInfo.HumanReadableName} is not {Core.Constants.IncoTerms.Other}.";
			AssertNoMessageError("No error when JZ_AdditionalTerms is empty", invoice.JZ_AdditionalTermsInfo, errorMessageJZAdditionalTerms);
			invoice.JZ_AdditionalTerms = "ZXY";
			AssertHasMessageErrorContaining("Has error when JZ_AdditionalTerms is populated", invoice.JZ_AdditionalTermsInfo, errorMessageJZAdditionalTerms);
		}

		public void TestJZ_ValuationCode()
		{
			// Tran. Nature
			(_, var invoice, var declaration) = SetupData();
			var entry = declaration.CustomsEntryInstructions.AddNew();
			entry.CEI_Style = "A1";
			entry.CEI_SubStyle = "A";
			var line = invoice.InvoiceLines.AddNew();
			line.JI_CEI = entry.PK;

			invoice.JZ_ValuationCode = ZString.Empty;
			AssertHasMessageErrorContaining($"Tran. Nature should be required when JE_MessageType is EXP", invoice.JZ_ValuationCodeInfo, "You have not entered a ");
		}

		public void TestCheckJZ_IncoTerm()
		{
			var messageError = "Please enter an Incoterm. Incoterm is needed to execute correct calculation for the required statistical value.";
			(_, var invoice, var declaration) = SetupData();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var validation = invoice.Validation;
			var line = invoice.InvoiceLines.AddNew();
			line.JI_CEI = instruction.PK;

			CombineAssertions(() =>
			{
				instruction.CEI_Style = ExportDeclarationTypeList.Codes.B1;
				invoice.JZ_IncoTerm = string.Empty;
				validation.ValidateJZ_IncoTerm();
				AssertHasMessageErrorContaining($"Incoterm empty, declaration type is B1 or B2 - show error", invoice.JZ_IncoTermInfo, messageError);
				instruction.CEI_Style = ExportDeclarationTypeList.Codes.B3;
				validation.ValidateJZ_IncoTerm();
				AssertNoMessageErrorContaining($"Incoterm empty, declaration type is not B1 or B2 - no error", invoice.JZ_IncoTermInfo, messageError);
				instruction.CEI_Style = ExportDeclarationTypeList.Codes.B1;
				invoice.JZ_IncoTerm = "FOB";
				validation.ValidateJZ_IncoTerm();
				AssertNoMessageErrorContaining($"Incoterm not empty, declaration type is B1 or B2 - no error", invoice.JZ_IncoTermInfo, messageError);
			});
		}

		public void TestCheckSupportingDocumentsRequiredCodes()
		{
			var rowMessageError = "Supporting Documents must contain at least one of the following codes: D005, D008, N325, N380, N864, N935, 1N09, 1N21, 1N22, 1N99, followed by the value of an invoice number.";
			var euRowMessageError = "At least one Supporting Document of the following types is needed for this Invoice";
			(_, var invoice, var declaration) = SetupData();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;

			invoice.Validation.ValidateAll();
			AssertHasRowMessageError("No supporting documents", invoice, rowMessageError);
			var instructionSupportingDoc = instruction.SupportingDocuments.AddNew();
			instructionSupportingDoc.CSI_Code = "N380";
			instructionSupportingDoc.CSI_ReferenceNumber = "INVREF01";
			invoice.Validation.ValidateAll();
			AssertNoRowMessageError("Valid supporting doc entered on instruction", invoice, rowMessageError);

			instructionSupportingDoc.CSI_Code = "N381";
			invoice.Validation.ValidateAll();
			AssertHasRowMessageError("No valid supporting doc entered - invalid code", invoice, rowMessageError);
			AssertNoRowMessageErrorContaining(invoice, euRowMessageError);

			var invoiceHeaderSupportingDoc = invoice.SupportingDocuments.AddNew();
			invoiceHeaderSupportingDoc.CSI_Code = "D005";
			invoiceHeaderSupportingDoc.CSI_ReferenceNumber = "INVREF02";
			invoice.Validation.ValidateAll();
			AssertNoRowMessageError("Valid supporting doc entered on invoice", invoice, rowMessageError);

			invoiceHeaderSupportingDoc.CSI_ReferenceNumber = ZString.Empty;
			invoice.Validation.ValidateAll();
			AssertHasRowMessageError("No valid supporting doc entered - no reference number", invoice, rowMessageError);
		}

		protected override string MessageType => IEJobMessageTypeList.Codes.Export;
	}
}
