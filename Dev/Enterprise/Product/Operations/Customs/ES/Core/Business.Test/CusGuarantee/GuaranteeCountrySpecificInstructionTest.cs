using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.ES.Business.Testing
{
	class GuaranteeCountrySpecificInstructionTest : TestCaseWithFactory
	{
		public void TestGetTypeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GuaranteeType, "Guarantee Types");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Spain, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GuaranteeType, "ZZZ", "Description for ZZZ", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			var guaranteeHeader = Factory.New<CusGuaranteeHeader>();
			var typeList = guaranteeHeader.CountrySpecificInstruction.GetTypeList(Core.Constants.CountryCodes.Spain);

			CombineAssertions(() =>
			{
				AssertCodeDescriptionPairList(typeList,
					("COD", "Ongoing/comprehensive guarantee"),
					("IMP", "Import"),
					("TRA", "Transit"),
					("TST", "Temporary Storage"),
					("ZZZ", "Description for ZZZ")
				);
			});
		}

		public void TestGetRuleCodeList()
		{
			var guarantee = Factory.New<CusGuaranteeHeader>();
			var listrule = guarantee.CountrySpecificInstruction.GetRuleCodeList("", "");
			AssertEquals(PermitRuleCodeList.Codes.CUS, listrule.GetCodeFromDescription("Customs Office"));
		}

		public void TestGetRuleCodeListForModule()
		{
			var guarantee = Factory.New<CusGuaranteeHeader>();
			var listrule = guarantee.CountrySpecificInstruction.GetRuleCodeListForModule().GetAllCodes();
			AssertEquals(true, listrule.Contains(PermitRuleCodeList.Codes.CUS));
		}

		public void TestGetValueFromFieldType()
		{
			var guarantee = Factory.New<CusGuaranteeHeader>();
			AssertEquals(nameof(FieldType.TextCodeFindBox), guarantee.CountrySpecificInstruction.GetValueFromFieldType(PermitRuleCodeList.Codes.CUS));
			AssertEquals(nameof(FieldType.Text), guarantee.CountrySpecificInstruction.GetValueFromFieldType("XXX"));
		}
	}
}
