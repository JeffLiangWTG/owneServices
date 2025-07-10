using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class ReimportCountryCodeValidationTest : CusCodeDataValidationTest
	{
		public void TestCheckCY_Code_ListValidation()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				countryCode.CY_Code = "~";
				AssertHasMessageError("CEI_Style='1*****', invalid code", countryCode.CY_CodeInfo, ListValidation.InvalidCodeMessageError);

				var entryInstruction = (CusEntryInstruction)countryCode.Parent;
				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._200000;
				countryCode.CY_Code = "~";
				AssertNoMessageError("CEI_Style!='1*****', invalid code", countryCode.CY_CodeInfo, ListValidation.InvalidCodeMessageError);
			});
		}

		public void TestCheckCY_Code_Unique()
		{
			const string countryCodeHasAlreadyBeenEntered = "Country/Region Code has already been entered.";
			var instruction = (CusEntryInstruction)countryCode.Parent;
			countryCode.CY_Code = Core.Constants.CountryCodes.China;

			NUnit.Framework.Assert.Multiple(() =>
			{
				instruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._200000;
				var countryCode2 = instruction.ReimportCountryCodes.AddNew(Core.Constants.CountryCodes.China);
				AssertNoMessageError("CEI_Style!='1*****', duplicate code", countryCode2.CY_CodeInfo, countryCodeHasAlreadyBeenEntered);

				instruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._110000;
				countryCode2.CY_Code = Core.Constants.CountryCodes.China;
				AssertHasMessageError("CEI_Style='1*****', duplicate code", countryCode2.CY_CodeInfo, countryCodeHasAlreadyBeenEntered);

				countryCode2.CY_Code = Core.Constants.CountryCodes.Australia;
				AssertNoMessageError("CEI_Style='1*****', unique code", countryCode2.CY_CodeInfo, countryCodeHasAlreadyBeenEntered);
			});
		}

		public void TestCheckCY_Code_NotDE()
		{
			const string reimportCountryDERequired = "The selected Type (Procedure) requires the Reimport Country/Region to be 'DE'.";
			var entryInstruction = (CusEntryInstruction)countryCode.Parent;

			NUnit.Framework.Assert.Multiple(() =>
			{
				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._110000;
				countryCode.CY_Code = Core.Constants.CountryCodes.Belgium;
				AssertNoMessageError("CEI_Style!='12****', CY_Code!='DE'", countryCode.CY_CodeInfo, reimportCountryDERequired);

				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._120200;
				countryCode.CY_Code = Core.Constants.CountryCodes.Belgium;
				AssertHasMessageError("CEI_Style='12****', CY_Code!='DE'", countryCode.CY_CodeInfo, reimportCountryDERequired);

				countryCode.CY_Code = Core.Constants.CountryCodes.Germany;
				AssertNoMessageError("CEI_Style='12****', CY_Code='DE'", countryCode.CY_CodeInfo, reimportCountryDERequired);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			countryCode = Factory.CreateReimportCountryCode();
		}

		ReimportCountryCode countryCode;
	}
}
