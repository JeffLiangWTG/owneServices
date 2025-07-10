using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	public class CommonGuaranteeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckPW_ValidityLimitation()
		{
			var guarantee = Factory.New<JobDeclaration>().Guarantees.AddNew();
			guarantee.PW_ValidityLimitation = guarantee.Lookups.List71NonEcContractingCountriesList[0].Code;
			AssertNoMessageErrorContaining(guarantee.PW_ValidityLimitationInfo, ListValidation.InvalidCodeMessageError);
			guarantee.PW_ValidityLimitation = "";
			AssertNoMessageErrorContaining(guarantee.PW_ValidityLimitationInfo, ListValidation.InvalidCodeMessageError);
			guarantee.PW_ValidityLimitation = "XX";
			AssertHasMessageErrorContaining(guarantee.PW_ValidityLimitationInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckPW_BondType()
		{
			var guarantee = Factory.New<JobDeclaration>().Guarantees.AddNew();
			guarantee.PW_BondType = guarantee.Lookups.BondTypeList[0].Code;
			AssertNoMessageErrorContaining(guarantee.PW_BondTypeInfo, ListValidation.InvalidCodeMessageError);
			guarantee.PW_BondType = "";
			AssertNoMessageErrorContaining(guarantee.PW_BondTypeInfo, ListValidation.InvalidCodeMessageError);
			guarantee.PW_BondType = "X";
			AssertHasMessageErrorContaining(guarantee.PW_BondTypeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckPW_BondFiledPort()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedKingdom, parent: eunZZZ);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy, parent: eunZZZ);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office Code");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Universal.RefCusCodeListAttributeTypes.Codes.ROLE, "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.CountryCodes.UnitedKingdom);
			helper.CreateNewOrGetExistingCusCodeList("GB000383", Core.Constants.CountryCodes.UnitedKingdom, "Salford National Clearance Hub", new ZString[] { "CGU", "EXP" });
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Universal.RefCusCodeListAttributeTypes.Codes.ROLE, "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.CountryCodes.Italy);
			helper.CreateNewOrGetExistingCusCodeList("IT304199", Core.Constants.CountryCodes.Italy, "PESCARA", new ZString[] { "DES" });
			Factory.Save();

			var guarantee = Factory.New<JobDeclaration>().Guarantees.AddNew();
			AssertNoMessageErrorContaining(guarantee.PW_BondFiledPortInfo, ListValidation.InvalidCodeMessageError);
			guarantee.PW_BondFiledPort = "IT304199";
			AssertHasMessageErrorContaining(guarantee.PW_BondFiledPortInfo, ListValidation.InvalidCodeMessageError);
			guarantee.PW_BondFiledPort = "GB000383";
			AssertNoMessageErrorContaining(guarantee.PW_BondFiledPortInfo, ListValidation.InvalidCodeMessageError);
		}
	}
}
