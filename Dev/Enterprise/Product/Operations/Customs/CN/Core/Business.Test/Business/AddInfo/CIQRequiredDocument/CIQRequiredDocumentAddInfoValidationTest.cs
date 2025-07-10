using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CN.Business.Testing
{
	class CIQRequiredDocumentAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckXC_DocumentType()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var testItem = instruction.CIQRequiredDocuments.AddNew();
			var targetInfo = testItem.XC_DocumentTypeInfo;
			testItem.AddInfoValidation.ValidateXC_DocumentType();
			AssertHasErrorContaining(targetInfo, MandatoryValidation.MustBeEntered);
			testItem.XC_DocumentType = "XX";
			AssertHasMessageErrorContaining(targetInfo, ListValidation.InvalidCodeMessageError);
			testItem.XC_DocumentType = "20";
			AssertNoMessageErrors(targetInfo);
			var anotherItem = instruction.CIQRequiredDocuments.AddNew();
			anotherItem.XC_DocumentType = "21";
			testItem.XC_DocumentType = "21";
			AssertHasMessageErrorContaining(targetInfo, "Duplicated document type. Please chose another one.");
			testItem.XC_DocumentType = "24";
			AssertHasMessageErrorContaining(targetInfo, "You cannot require '入境货物检验检疫证明（申请不出具）' and '入境货物检验检疫证明（申请出具）' at the same time.");
		}

		public void TestCheckXC_NumberOfOriginals()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var testItem = instruction.CIQRequiredDocuments.AddNew();
			testItem.XC_NumberOfOriginals = 0;
			var targetInfo = testItem.XC_NumberOfOriginalsInfo;
			testItem.AddInfoValidation.ValidateXC_NumberOfOriginals();
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.ValueCannotBeZero);
			testItem.XC_NumberOfOriginals = -1;
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.ValueCannotBeNegative);
			testItem.XC_NumberOfOriginals = 10;
			AssertNoMessageErrors(targetInfo);
			testItem.XC_DocumentType = "24";
			testItem.XC_NumberOfOriginals = 0;
			AssertNoMessageErrors(targetInfo);
		}

		public void TestCheckXC_NumberOfCopies()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var testItem = instruction.CIQRequiredDocuments.AddNew();
			testItem.XC_NumberOfCopies = 0;
			var targetInfo = testItem.XC_NumberOfCopiesInfo;
			testItem.AddInfoValidation.ValidateXC_NumberOfCopies();
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.ValueCannotBeZero);
			testItem.XC_NumberOfCopies = -1;
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.ValueCannotBeNegative);
			testItem.XC_NumberOfCopies = 10;
			AssertNoMessageErrors(targetInfo);
			testItem.XC_DocumentType = "24";
			testItem.XC_NumberOfCopies = 0;
			AssertNoMessageErrors(targetInfo);
		}

		public void TestValidationModeProvider()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var testItem = instruction.CIQRequiredDocuments.AddNew();
			ValidationExtensionsTest.AssertValidationModeProvider(declaration, testItem.AddInfoValidation.ValidationModeProvider);

			instruction = Factory.New<CusEntryInstruction>();
			testItem = instruction.CIQRequiredDocuments.AddNew();
			AssertNull(testItem.AddInfoValidation.ValidationModeProvider);
		}
	}
}
