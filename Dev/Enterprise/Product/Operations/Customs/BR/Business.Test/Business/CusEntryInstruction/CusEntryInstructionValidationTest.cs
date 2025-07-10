using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business.Testing
{
	class CusEntryInstructionValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateMaxCountOfEntryInstructionsForImportSiscomex()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			var instruction1 = declaration.CustomsEntryInstructions.AddNew();
			instruction1.CEI_Description = "TEST1";
			AssertNoRowError("First Instruction must NOT contain message error for ISW", instruction1, CusEntryInstructionValidation.NotAllowMultiplyEntryInstructionsMessage);

			var instruction2 = declaration.CustomsEntryInstructions.AddNew();
			instruction2.CEI_Description = "TEST2";
			instruction1.Validation.ValidateAll();
			AssertNoRowError("First Instruction must NOT contain message error for ISW", instruction1, CusEntryInstructionValidation.NotAllowMultiplyEntryInstructionsMessage);
			AssertHasRowError("Second Instruction must contain message error for ISW", instruction2, CusEntryInstructionValidation.NotAllowMultiplyEntryInstructionsMessage);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			instruction1.Validation.ValidateAll();
			instruction2.Validation.ValidateAll();
			AssertNoRowError("First Instruction must NOT contain message error for EXP", instruction1, CusEntryInstructionValidation.NotAllowMultiplyEntryInstructionsMessage);
			AssertNoRowError("Second Instruction must NOT contain message error for EXP", instruction2, CusEntryInstructionValidation.NotAllowMultiplyEntryInstructionsMessage);
		}

		public void TestCheckUCRNumber()
		{
			var validMasterUCRs = new string[2] { "1BR01831941200000000000000000062021", "1BR64571045200000000000000000280023" };
			var invalidMasterUCRs = new string[2] { "12313131313132132132111111111111111", "1BRR1111111255555555555555555555555" };

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			var instruction = declaration.CustomsEntryInstructions.AddNew();

			CombineAssertions(() =>
			{
				foreach (var number in invalidMasterUCRs)
				{
					instruction.UCRNumber = number;
					AssertNoMessageError($"Should NOT have message Error when UCRNumber = {number} and MessageType = {declaration.JE_MessageType}", instruction.UCRNumberInfo, "The entered UCR Number does not match either the CNPJ format: <year, 1>BR<CNPJ, 8><decade, 1><reference, 23> or the CPF format: <year, 1>BR<CPF, 11><decade, 1><reference, 20>.");
				}

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
				instruction.UCRNumber = ZString.Empty;
				AssertNoMessageError($"Should NOT have message Error when UCRNumber is empty", instruction.UCRNumberInfo, "The entered UCR Number does not match either the CNPJ format: <year, 1>BR<CNPJ, 8><decade, 1><reference, 23> or the CPF format: <year, 1>BR<CPF, 11><decade, 1><reference, 20>.");
				foreach (var number in validMasterUCRs)
				{
					instruction.UCRNumber = number;
					AssertNoMessageError($"Should NOT have message Error when UCRNumber = {number} and MessageType = {declaration.JE_MessageType}", instruction.UCRNumberInfo, "The entered UCR Number does not match either the CNPJ format: <year, 1>BR<CNPJ, 8><decade, 1><reference, 23> or the CPF format: <year, 1>BR<CPF, 11><decade, 1><reference, 20>.");
				}

				foreach (var number in invalidMasterUCRs)
				{
					instruction.UCRNumber = number;
					AssertHasMessageError($"Should have message Error when UCRNumber = {number} and MessageType = {declaration.JE_MessageType}", instruction.UCRNumberInfo, "The entered UCR Number does not match either the CNPJ format: <year, 1>BR<CNPJ, 8><decade, 1><reference, 23> or the CPF format: <year, 1>BR<CPF, 11><decade, 1><reference, 20>.");
				}
			});
		}

		public void TestCheckBillNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(instruction.BillNumberInfo);

			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(instruction.BillNumberInfo);

			declaration.JE_TransportMode = TransportTypeList.Codes.Road;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(instruction.BillNumberInfo);

			declaration.JE_TransportMode = TransportTypeList.Codes.Mail;
			ValidationTestHelper.AssertFieldIsNotMandatory(instruction.BillNumberInfo);

			declaration.JE_TransportMode = TransportTypeList.Codes.Road;
			declaration.JE_DispatchModality = DispatchModalityCodes.Codes.AnticipatedFractionalDelivery;
			ValidationTestHelper.AssertFieldIsNotMandatory(instruction.BillNumberInfo);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			ValidationTestHelper.AssertFieldIsNotMandatory(instruction.BillNumberInfo);
		}

		public void TestCheckCEI_LegalDocument()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var instruction = jobDeclaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_LegalDocument = "";
			AssertHasMessageErrorContaining(instruction.CEI_LegalDocumentInfo, MandatoryValidation.YouHaveNotEntered);
			instruction.CEI_LegalDocument = "TST";
			AssertNoMessageErrors(instruction.CEI_LegalDocumentInfo);
		}

		public void TestCheckCEI_SpecialCustomsClearance()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_LegalDocument = LegalDocumentList.Codes.NoInvoice;
			instruction.CEI_SpecialCustomsClearance = SpecialCustomsClearanceList.Codes._2001;
			AssertEquals(ZString.Empty, instruction.CEI_DetailWithoutLegalDoc);
			instruction.CEI_SpecialCustomsClearance = SpecialCustomsClearanceList.Codes._2002;
			AssertEquals(DetailWithoutLegalDocList.Codes._3004, instruction.CEI_DetailWithoutLegalDoc);
		}

		public void TestCheckCEI_DetailWithoutLegalDoc()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var scenario1 = "Special Clearance is 2002-Early Boarding. Content of the field Details of the operation without Invoice must be 3004 – Early Boarding";
			var scenario2 = "Content of the field Details of the operation without Invoice is not allowed. According to NOTÍCIA SISCOMEX EXPORTAÇÃO Nº 013/2021 this entry must be created directly in Single Window SISCOMEX.";
			instruction.CEI_SpecialCustomsClearance = SpecialCustomsClearanceList.Codes._2002;
			instruction.CEI_LegalDocument = LegalDocumentList.Codes.NoInvoice;
			instruction.CEI_DetailWithoutLegalDoc = DetailWithoutLegalDocList.Codes._3003;

			AssertHasMessageErrors(scenario1, instruction.CEI_DetailWithoutLegalDocInfo);
			instruction.CEI_SpecialCustomsClearance = SpecialCustomsClearanceList.Codes._2002;
			instruction.CEI_DetailWithoutLegalDoc = DetailWithoutLegalDocList.Codes._3004;
			AssertNoMessageErrors(scenario1, instruction.CEI_DetailWithoutLegalDocInfo);
			AssertNoMessageErrors(scenario2, instruction.CEI_DetailWithoutLegalDocInfo);
			instruction.CEI_DetailWithoutLegalDoc = DetailWithoutLegalDocList.Codes._3005;
			AssertHasMessageErrors(scenario2, instruction.CEI_DetailWithoutLegalDocInfo);
			instruction.CEI_DetailWithoutLegalDoc = "X";
			AssertHasMessageErrorContaining(instruction.CEI_DetailWithoutLegalDocInfo, ListValidation.InvalidCodeMessageError);

			instruction.CEI_LegalDocument = LegalDocumentList.Codes.ElectronicLogisticInvoice;
			AssertNoMessageErrors(scenario2, instruction.CEI_DetailWithoutLegalDocInfo);

			instruction.CEI_LegalDocument = LegalDocumentList.Codes.NoInvoice;
			instruction.CEI_DetailWithoutLegalDoc = ZString.Empty;
			AssertHasMessageErrors(scenario2, instruction.CEI_DetailWithoutLegalDocInfo);
		}

		public void TestCheckCEI_AFRMMMethodOfCalculation()
		{
			ReferenceTestDataHelper.CreateAfrmmTaxes(Factory);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;

			var instruction = declaration.CustomsEntryInstructions.AddNew();

			ValidationTestHelper.AssertInvalidCodeMessageError(instruction.CEI_AFRMMMethodOfCalculationInfo, "XXXX", "FMM1");

			declaration.JE_TransportMode = TransportTypeList.Codes.Lake;
			ValidationTestHelper.AssertInvalidCodeMessageError(instruction.CEI_AFRMMMethodOfCalculationInfo, "XXXX", "FMM1");

			declaration.JE_TransportMode = TransportTypeList.Codes.River;
			ValidationTestHelper.AssertInvalidCodeMessageError(instruction.CEI_AFRMMMethodOfCalculationInfo, "XXXX", "FMM1");

			instruction.CEI_AFRMMMethodOfCalculation = ZString.Empty;
			AssertNoNotifications("CEI_AFRMMMethodOfCalculation must NOT have notification", instruction.CEI_AFRMMMethodOfCalculationInfo);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			instruction.CEI_AFRMMMethodOfCalculation = "XXXX";
			AssertNoNotifications("CEI_AFRMMMethodOfCalculation must NOT have notification", instruction.CEI_AFRMMMethodOfCalculationInfo);

			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			instruction.CEI_AFRMMMethodOfCalculation = "XXXX";
			AssertNoNotifications("CEI_AFRMMMethodOfCalculation must NOT have notification", instruction.CEI_AFRMMMethodOfCalculationInfo);
		}

		public void TestCheckCEI_AFRMMRateOverride()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_AFRMMMethodOfCalculation = "FMM1";

			entryInstruction.IsAFRMMRateOverridden = true;
			entryInstruction.CEI_AFRMMRateOverride = 0m;
			AssertHasMessageErrorContaining(entryInstruction.CEI_AFRMMRateOverrideInfo, MandatoryValidation.YouHaveNotEntered);

			entryInstruction.CEI_AFRMMRateOverride = 10m;
			AssertNoMessageErrorContaining(entryInstruction.CEI_AFRMMRateOverrideInfo, MandatoryValidation.YouHaveNotEntered);

			entryInstruction.IsAFRMMRateOverridden = true;
			entryInstruction.CEI_AFRMMRateOverride = 10m;
			AssertNoMessageErrorContaining(entryInstruction.CEI_AFRMMRateOverrideInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckCEI_UtilizationFeeOverride()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

			entryInstruction.IsAFRMMRateOverridden = true;
			entryInstruction.CEI_UtilizationFeeOverride = 0m;
			AssertHasMessageErrorContaining(entryInstruction.CEI_UtilizationFeeOverrideInfo, MandatoryValidation.YouHaveNotEntered);

			entryInstruction.CEI_UtilizationFeeOverride = 10m;
			AssertNoMessageErrorContaining(entryInstruction.CEI_UtilizationFeeOverrideInfo, MandatoryValidation.YouHaveNotEntered);

			entryInstruction.IsAFRMMRateOverridden = true;
			entryInstruction.CEI_UtilizationFeeOverride = 10m;
			AssertNoMessageErrorContaining(entryInstruction.CEI_UtilizationFeeOverrideInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckCEI_AdditionalInformationOption()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.Validation.ValidateCEI_AdditionalInformationOption();
			AssertNoMessageErrors(entryInstruction.AdditionalInformationOptionDescriptionInfo);

			entryInstruction.AdditionalInformation = new string('1', CusEntryInstruction.Schema.ImportAdditionalInformationMaxLength / 2);
			entryInstruction.AdditionalInformationManual = new string('1', CusEntryInstruction.Schema.ImportAdditionalInformationMaxLength / 2);
			entryInstruction.Validation.ValidateCEI_AdditionalInformationOption();
			AssertNoMessageErrors(entryInstruction.AdditionalInformationOptionDescriptionInfo);

			entryInstruction.AdditionalInformation += "A";
			entryInstruction.AdditionalInformationManual += "A";
			AssertNoMessageErrors(entryInstruction.AdditionalInformationOptionDescriptionInfo);

			entryInstruction.AdditionalInformationOptionDescription = AdditionalInformationOptions.Descriptions.SystemGeneratedAndFreeText;
			AssertHasMessageError(entryInstruction.AdditionalInformationOptionDescriptionInfo, "You are reporting size 7802 when the expected size is 7800.");

			entryInstruction.AdditionalInformationOptionDescription = AdditionalInformationOptions.Descriptions.FreeTextAndSystemGenerated;
			AssertHasMessageError(entryInstruction.AdditionalInformationOptionDescriptionInfo, "You are reporting size 7802 when the expected size is 7800.");

			entryInstruction.AdditionalInformationOptionDescription = AdditionalInformationOptions.Descriptions.OnlyFreeText;
			entryInstruction.AdditionalInformation = new string('1', CusEntryInstruction.Schema.ImportAdditionalInformationMaxLength);
			entryInstruction.AdditionalInformationManual = new string('1', CusEntryInstruction.Schema.ImportAdditionalInformationMaxLength);
			AssertNoMessageErrors(entryInstruction.AdditionalInformationOptionDescriptionInfo);

			entryInstruction.AdditionalInformationManual += "A";
			AssertHasMessageError(entryInstruction.AdditionalInformationOptionDescriptionInfo, "You are reporting size 7801 when the expected size is 7800.");

			entryInstruction.AdditionalInformationOptionDescription = AdditionalInformationOptions.Descriptions.OnlySystemGenerated;
			entryInstruction.AdditionalInformation = new string('1', CusEntryInstruction.Schema.ImportAdditionalInformationMaxLength);
			entryInstruction.AdditionalInformationManual = new string('1', CusEntryInstruction.Schema.ImportAdditionalInformationMaxLength);
			AssertNoMessageErrors(entryInstruction.AdditionalInformationOptionDescriptionInfo);

			entryInstruction.AdditionalInformation += "A";
			AssertHasMessageError(entryInstruction.AdditionalInformationOptionDescriptionInfo, "You are reporting size 7801 when the expected size is 7800.");
		}
	}
}
