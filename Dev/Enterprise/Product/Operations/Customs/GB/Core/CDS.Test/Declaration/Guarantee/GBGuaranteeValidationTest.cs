using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.CDS.Testing
{
	public class GBGuaranteeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckEntryInstructionID()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_Style = ImportDeclarationTypeList.Codes.DeclarationForReleaseForFreeCirculationOrEndUse;

			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_Style = ImportDeclarationTypeList.Codes.DeclarationForCustomsWarehousing;

			var guarantee = declaration.Guarantees.AddNew();
			guarantee.EntryInstructionID = ZGuid.Invalid;

			AssertHasError(guarantee.EntryInstructionIDInfo, "Enter a valid selection.");
		}

		public void TestCheckPW_BondType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var guarantee = declaration.Guarantees.AddNew();
			guarantee.PW_BondType = guarantee.Lookups.BondTypeList[0].Code;
			AssertNoMessageErrorContaining(guarantee.PW_BondTypeInfo, "list");
			guarantee.PW_BondType = "";
			AssertNoMessageErrorContaining(guarantee.PW_BondTypeInfo, "list");
			guarantee.PW_BondType = "X";
			AssertHasMessageErrorContaining(guarantee.PW_BondTypeInfo, "list");
		}

		public void TestCheckPW_Password()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var guarantee = declaration.Guarantees.AddNew();
			guarantee.PW_Password = "";
			AssertNoMessageErrorContaining(guarantee.PW_PasswordInfo, "list");
			guarantee.PW_Password = "X";
			AssertNoMessageErrorContaining(guarantee.PW_PasswordInfo, "list");
			guarantee.PW_Password = "Z";
			AssertHasMessageErrorContaining(guarantee.PW_PasswordInfo, "list");
		}

		public void TestCheckPW_BondNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var guarantee = declaration.Guarantees.AddNew();
			guarantee.Validation.ValidatePW_BondNumber();
			AssertNoMessageErrorContaining(guarantee.PW_BondNumberInfo, "Please set only the reference or the GRN field but not both");

			guarantee.PW_BondNumber2 = "123";
			guarantee.PW_BondNumber = "123";
			AssertHasMessageErrorContaining(guarantee.PW_BondNumberInfo, "Please set only the reference or the GRN field but not both");

			guarantee.PW_BondType = "";
			guarantee.Validation.ValidatePW_BondNumber();
			AssertNoMessageErrorContaining(guarantee.PW_BondNumberInfo, "Please set only the reference or the GRN field but not both");

			guarantee.PW_BondNumber = "123";
			AssertNoMessageErrorContaining(guarantee.PW_BondNumberInfo, "Please set only the reference or the GRN field but not both");
		}

		public void TestCheckPW_BondNumber2()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var guarantee = declaration.Guarantees.AddNew();
			guarantee.Validation.ValidatePW_BondNumber2();
			AssertNoMessageErrorContaining(guarantee.PW_BondNumber2Info, "Please set only the reference or the GRN field but not both");

			guarantee.PW_BondNumber = "123";
			guarantee.PW_BondNumber2 = "123";
			AssertHasMessageErrorContaining(guarantee.PW_BondNumber2Info, "Please set only the reference or the GRN field but not both");

			guarantee.PW_BondType = "";
			guarantee.Validation.ValidatePW_BondNumber2();
			AssertNoMessageErrorContaining(guarantee.PW_BondNumber2Info, "Please set only the reference or the GRN field but not both");

			guarantee.PW_BondNumber2 = "123";
			AssertNoMessageErrorContaining(guarantee.PW_BondNumber2Info, "Please set only the reference or the GRN field but not both");
		}

		public void TestCheckPW_HolderIdentification()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var guarantee = declaration.Guarantees.AddNew();
			guarantee.Validation.ValidatePW_HolderIdentification();
			AssertNoMessageErrorContaining(guarantee.PW_HolderIdentificationInfo, "was unable to determine which of Other Guarantee Reference and GRN to set automatically");

			guarantee.PW_BondNumber = "";
			guarantee.PW_BondNumber2 = "";
			guarantee.PW_HolderIdentification = "123";
			AssertHasMessageErrorContaining(guarantee.PW_HolderIdentificationInfo, "was unable to determine which of Other Guarantee Reference and GRN to set automatically");
		}
	}
}
