using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	class DEOfficeCodeValidationTest : CusCodeDataValidationTest
	{
		public void TestCheckCY_Code_WhenDeclarationIsImport()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = MessageTypeList.Codes.Import;

			var office1 = dec.CustomsOffices.AddNew();
			office1.CY_Code = "AAA";
			AssertHasErrorContaining(office1.CY_CodeInfo, ListValidation.InvalidCodeError);

			office1.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent;
			AssertNoErrorContaining(office1.CY_CodeInfo, ListValidation.InvalidCodeError);
		}

		public void TestCheckCY_Data()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, parent: eunZZZ);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			var codeDE000001 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE000001", "Central Community Transit Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeDE000001.PK, RefCusCodeListAttributeTypes.Codes.ROLE, "EXP");
			Factory.Save();

			var dec = Factory.New<JobDeclaration>();
			var office = dec.CustomsOffices.AddNew();
			office.CY_Code = EuOfficeCodesTypes.Codes.SupplementaryDeclarationOffice;
			office.CY_Data = "DE000001";
			AssertNoMessageErrorContaining(office.CY_DataInfo, "fulfill this role");

			office.CY_Code = "DJC";
			office.CY_Data = "DE000001";
			office.Validation.ValidateCY_Data();
			AssertHasMessageErrorContaining(office.CY_DataInfo, "fulfill this role");

			office.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfExport;
			office.CY_Data = "DE000001";
			office.Validation.ValidateCY_Data();
			AssertNoMessageErrorContaining(office.CY_DataInfo, "fulfill this role");
		}

		public void TestCheckCY_Data_EXT()
		{
			const string message = "The Office of Exit must match the Office of Export.";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_CustomsOffice = "DE00001";
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			NUnit.Framework.Assert.Multiple(() =>
			{
				var office = declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExit, "12345");
				AssertNoMessageError("No entry instruction with CEI_Style '***9**'", office.CY_DataInfo, message);

				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000901;
				office.Validation.ValidateCY_Data();
				AssertHasMessageError("Has entry instruction with CEI_Style '***9**'", office.CY_DataInfo, message);

				office.CY_Data = "DE00001";
				AssertNoMessageError("Customs office 'EXT' same as 'EXP'", office.CY_DataInfo, message);
			});
		}

		public void TestCheckCY_Data_PRE()
		{
			const string message = "The first two digits of the customs office with purpose 'PRE' must match country/region of [15] Origin.";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GoodsOrigin = Core.Constants.CountryCodes.Germany;
			NUnit.Framework.Assert.Multiple(() =>
			{
				var office = declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfPresentation, "AU00001");
				AssertHasMessageError("Not start with 'DE'", office.CY_DataInfo, message);

				office.CY_Data = "DE00001";
				AssertNoMessageError("Starts with 'DE'", office.CY_DataInfo, message);
			});
		}
	}
}
