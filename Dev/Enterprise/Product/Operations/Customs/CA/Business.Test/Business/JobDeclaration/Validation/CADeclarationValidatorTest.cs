using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.CA;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CADeclarationValidatorTest : TestCaseWithFactory
	{
		public void TestCurrentValidationTypeRequired()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("ACROSS initially", ValidateForMessageType.ACROSS, declaration.DeclarationValidator.CurrentValidationTypeRequired);
			declaration.JE_EntryAuthorisationDate = ZDateTime.Now;
			AssertEquals("B3 now", ValidateForMessageType.B3CUSDEC, declaration.DeclarationValidator.CurrentValidationTypeRequired);
			declaration.DeclarationValidator.OverrideValidationType = ValidateForMessageType.None;
			AssertEquals("Override none", ValidateForMessageType.None, declaration.DeclarationValidator.CurrentValidationTypeRequired);
			declaration.DeclarationValidator.OverrideValidationType = ValidateForMessageType.ACROSS;
			AssertEquals("Override ACROSS", ValidateForMessageType.ACROSS, declaration.DeclarationValidator.CurrentValidationTypeRequired);
			declaration.DeclarationValidator.OverrideValidationType = ValidateForMessageType.Default;
			AssertEquals("B3 again", ValidateForMessageType.B3CUSDEC, declaration.DeclarationValidator.CurrentValidationTypeRequired);
			declaration.JE_EntryAuthorisationDate = ZDateTime.Empty;
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.Confirming;
			AssertEquals("ACROSS again", ValidateForMessageType.ACROSS, declaration.DeclarationValidator.CurrentValidationTypeRequired);
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.ExWarehouse20;
			AssertEquals("B3 now", ValidateForMessageType.B3CUSDEC, declaration.DeclarationValidator.CurrentValidationTypeRequired);
			declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			AssertEquals("B3 again", ValidateForMessageType.B3CUSDEC, declaration.DeclarationValidator.CurrentValidationTypeRequired);

			var declarationValidator = declaration.DeclarationValidator;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = ZString.Empty;
			declaration.IsEnableACROSSValidation = true;
			declaration.IsEnableB3Validation = true;
			AssertEquals("CurrentValidationTypeRequired", ValidateForMessageType.Both, declarationValidator.CurrentValidationTypeRequired);
			declaration.IsEnableACROSSValidation = true;
			declaration.IsEnableB3Validation = false;
			AssertEquals("CurrentValidationTypeRequired", ValidateForMessageType.ACROSS, declarationValidator.CurrentValidationTypeRequired);
			declaration.IsEnableACROSSValidation = false;
			declaration.IsEnableB3Validation = true;
			AssertEquals("CurrentValidationTypeRequired", ValidateForMessageType.B3CUSDEC, declarationValidator.CurrentValidationTypeRequired);
			declaration.IsEnableACROSSValidation = false;
			declaration.IsEnableB3Validation = false;
			AssertEquals("CurrentValidationTypeRequired", ValidateForMessageType.None, declarationValidator.CurrentValidationTypeRequired);

			var cadEntry = declaration.ActiveEntryHeaders.AddNew();
			cadEntry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;

			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse201;
			AssertEquals("CAD now", ValidateForMessageType.B3CUSDEC, declaration.DeclarationValidator.CurrentValidationTypeRequired);
		}

		public void TestIsIID()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			Assert("Not IsIID", !declaration.IsIID);
			Assert("Not DeclarationValidator.IsIID", !declaration.DeclarationValidator.IsIID);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			Assert("IsIID", declaration.IsIID);
			Assert("DeclarationValidator.IsIID", declaration.DeclarationValidator.IsIID);
		}

		public void TestIsValidationRequired()
		{
			var declaration = Factory.New<JobDeclaration>();

			AssertIsValidationRequiredForNonWHOrNonLVS(declaration, ValidateForMessageType.Default, false, false, false, false);
			AssertIsValidationRequiredForNonWHOrNonLVS(declaration, ValidateForMessageType.Default, true, false, true, false);
			AssertIsValidationRequiredForNonWHOrNonLVS(declaration, ValidateForMessageType.Default, false, true, false, true);
			AssertIsValidationRequiredForNonWHOrNonLVS(declaration, ValidateForMessageType.Default, true, true, true, true);
			AssertIsValidationRequiredForWH(declaration, ValidateForMessageType.Default, false, true);
			AssertIsValidationRequiredForLVS(declaration, ValidateForMessageType.Default, false, true);
			AssertIsValidationRequiredForLVX(declaration, ValidateForMessageType.Default, false, true);

			AssertIsValidationRequiredForNonWHOrNonLVS(declaration, ValidateForMessageType.ACROSS, false, false, true, false);
			AssertIsValidationRequiredForNonWHOrNonLVS(declaration, ValidateForMessageType.ACROSS, true, false, true, false);
			AssertIsValidationRequiredForNonWHOrNonLVS(declaration, ValidateForMessageType.ACROSS, false, true, true, false);
			AssertIsValidationRequiredForNonWHOrNonLVS(declaration, ValidateForMessageType.ACROSS, true, true, true, false);
			AssertIsValidationRequiredForWH(declaration, ValidateForMessageType.ACROSS, true, false);
			AssertIsValidationRequiredForLVS(declaration, ValidateForMessageType.ACROSS, true, false);
			AssertIsValidationRequiredForLVX(declaration, ValidateForMessageType.ACROSS, true, false);

			AssertIsValidationRequiredForNonWHOrNonLVS(declaration, ValidateForMessageType.B3CUSDEC, false, false, false, true);
			AssertIsValidationRequiredForNonWHOrNonLVS(declaration, ValidateForMessageType.B3CUSDEC, true, false, false, true);
			AssertIsValidationRequiredForNonWHOrNonLVS(declaration, ValidateForMessageType.B3CUSDEC, false, true, false, true);
			AssertIsValidationRequiredForNonWHOrNonLVS(declaration, ValidateForMessageType.B3CUSDEC, true, true, false, true);
			AssertIsValidationRequiredForWH(declaration, ValidateForMessageType.B3CUSDEC, false, true);
			AssertIsValidationRequiredForLVS(declaration, ValidateForMessageType.B3CUSDEC, false, true);
			AssertIsValidationRequiredForLVX(declaration, ValidateForMessageType.B3CUSDEC, false, true);

			AssertIsValidationRequiredForNonWHOrNonLVS(declaration, ValidateForMessageType.None, false, false, false, false);
			AssertIsValidationRequiredForNonWHOrNonLVS(declaration, ValidateForMessageType.None, true, false, false, false);
			AssertIsValidationRequiredForNonWHOrNonLVS(declaration, ValidateForMessageType.None, false, true, false, false);
			AssertIsValidationRequiredForNonWHOrNonLVS(declaration, ValidateForMessageType.None, true, true, false, false);
			AssertIsValidationRequiredForWH(declaration, ValidateForMessageType.None, false, false);
			AssertIsValidationRequiredForLVS(declaration, ValidateForMessageType.None, false, false);
			AssertIsValidationRequiredForLVX(declaration, ValidateForMessageType.None, false, false);
		}

		void AssertIsValidationRequiredForNonWHOrNonLVS(JobDeclaration declaration, ValidateForMessageType validateForMessageType, ZBool isEnableACROSSValidation, ZBool isEnableB3Validation, ZBool expectedACROSSValue, ZBool expectedB3Value)
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = ZString.Empty;
			var declarationValidator = declaration.DeclarationValidator;
			declarationValidator.OverrideValidationType = validateForMessageType;
			declaration.IsEnableACROSSValidation = isEnableACROSSValidation;
			declaration.IsEnableB3Validation = isEnableB3Validation;
			AssertEquals("ACROSS IsValidationRequired", expectedACROSSValue, declarationValidator.IsValidationRequired(ValidateForMessageType.ACROSS));
			AssertEquals("B3 IsValidationRequired", expectedB3Value, declarationValidator.IsValidationRequired(ValidateForMessageType.B3CUSDEC));
		}

		void AssertIsValidationRequiredForWH(JobDeclaration declaration, ValidateForMessageType validateForMessageType, ZBool expectedACROSSValue, ZBool expectedB3Value)
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.Warehouse10;
			declaration.DeclarationValidator.OverrideValidationType = validateForMessageType;
			AssertIsValidationRequired(declaration, expectedACROSSValue, expectedB3Value);

			var cadEntry = declaration.ActiveEntryHeaders.AddNew();
			cadEntry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;

			declaration.JE_MessageSubType = CADEntryTypeList.Codes.Warehouse101;
			AssertIsValidationRequired(declaration, expectedACROSSValue, expectedB3Value);

			declaration.JE_MessageSubType = CADEntryTypeList.Codes.Warehouse102;
			AssertIsValidationRequired(declaration, expectedACROSSValue, expectedB3Value);
		}

		void AssertIsValidationRequiredForLVS(JobDeclaration declaration, ValidateForMessageType validateForMessageType, ZBool expectedACROSSValue, ZBool expectedB3Value)
		{
			declaration.JE_MessageSubType = ZString.Empty;
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			declaration.DeclarationValidator.OverrideValidationType = validateForMessageType;
			AssertIsValidationRequired(declaration, expectedACROSSValue, expectedB3Value);
		}

		void AssertIsValidationRequiredForLVX(JobDeclaration declaration, ValidateForMessageType validateForMessageType, ZBool expectedACROSSValue, ZBool expectedB3Value)
		{
			declaration.JE_MessageSubType = ZString.Empty;
			declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			declaration.DeclarationValidator.OverrideValidationType = validateForMessageType;
			AssertIsValidationRequired(declaration, expectedACROSSValue, expectedB3Value);
		}

		void AssertIsValidationRequired(JobDeclaration declaration, ZBool expectedACROSSValue, ZBool expectedB3Value)
		{
			AssertIsValidationRequired(declaration, true, true, expectedACROSSValue, expectedB3Value);
			AssertIsValidationRequired(declaration, false, true, expectedACROSSValue, expectedB3Value);
			AssertIsValidationRequired(declaration, true, false, expectedACROSSValue, expectedB3Value);
			AssertIsValidationRequired(declaration, false, false, expectedACROSSValue, expectedB3Value);
		}

		void AssertIsValidationRequired(JobDeclaration declaration, ZBool isEnableACROSSValidation, ZBool isEnableB3Validation, ZBool expectedACROSSValue, ZBool expectedB3Value)
		{
			declaration.IsEnableACROSSValidation = isEnableACROSSValidation;
			declaration.IsEnableB3Validation = isEnableB3Validation;
			var declarationValidator = declaration.DeclarationValidator;
			AssertEquals("ACROSS IsValidationRequired", expectedACROSSValue, declarationValidator.IsValidationRequired(ValidateForMessageType.ACROSS));
			AssertEquals("B3 IsValidationRequired", expectedB3Value, declarationValidator.IsValidationRequired(ValidateForMessageType.B3CUSDEC));
		}
	}
}
