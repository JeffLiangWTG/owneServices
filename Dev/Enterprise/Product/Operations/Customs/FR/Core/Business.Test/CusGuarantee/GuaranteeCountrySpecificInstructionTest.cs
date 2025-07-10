using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.FR.Business.CusGuarantee.Testing
{
	class GuaranteeCountrySpecificInstructionTest : TestCaseWithFactory
	{
		public void TestGetSubTypeList()
		{
			var guaranteeHeader = Factory.New<CusGuaranteeHeader>();
			var subTypeList = guaranteeHeader.CountrySpecificInstruction.GetSubTypeList(GuaranteeTypeList.Codes.DEF);
			AssertEquals("Only one sub type is tied to type DEF", 1, subTypeList.Count);
			AssertEquals("CdE", subTypeList[0].Code);
			AssertEquals("Crédit d'Enlèvement", subTypeList[0].Description);

			subTypeList = guaranteeHeader.CountrySpecificInstruction.GetSubTypeList(GuaranteeTypeList.Codes.COD);
			AssertContainsExactElementsInAnyOrder("Sub Type List", new ZString[] { "1", "2", "3", "5", "6", "7", "8", "9", "0", "A", "B" }, subTypeList.GetAllCodes());
		}

		public void TestGetTypeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GuaranteeType, "Guarantee Types");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GuaranteeType, "ZZZ", "Description for ZZZ", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			var guaranteeHeader = Factory.New<CusGuaranteeHeader>();
			var typeList = guaranteeHeader.CountrySpecificInstruction.GetTypeList(Core.Constants.CountryCodes.France);
			AssertEquals("AI2", typeList[0].Code);
			AssertEquals("Exemption de ré-exportation", typeList[0].Description);
			AssertEquals("ALT", typeList[1].Code);
			AssertEquals("Autoliquidation (ATVAI)", typeList[1].Description);
			AssertEquals("COD", typeList[2].Code);
			AssertEquals("Crédit d'Opérations Diverses", typeList[2].Description);
			AssertEquals("DEF", typeList[3].Code);
			AssertEquals("Ajournement externe", typeList[3].Description);
			AssertEquals("ZZZ", typeList[4].Code);
			AssertEquals("Description for ZZZ", typeList[4].Description);
		}

		public void TestPermitGuaranteeType()
		{
			var guaranteeCountrySpecificInstruction = new GuaranteeCountrySpecificInstruction(Factory);
			AssertEquals("COD", guaranteeCountrySpecificInstruction.PermitGuaranteeType);
		}

		public void TestGetRuleCodeList()
		{
			var guarantee = Factory.New<CusGuaranteeHeader>();
			var ruleCodeList = guarantee.CountrySpecificInstruction.GetRuleCodeList("", "");
			AssertContainsExactElementsInAnyOrder(new string[] { PermitRuleCodeList.Codes.ADD, PermitRuleCodeList.Codes.INV, PermitRuleCodeList.Codes.MOD, PermitRuleCodeList.Codes.CUS, PermitRuleCodeList.Codes.LAP, PermitRuleCodeList.Codes.TSP }, ruleCodeList.GetAllCodes());

			ruleCodeList = guarantee.CountrySpecificInstruction.GetRuleCodeList(GuaranteeTypeList.Codes.AI2, "");
			AssertContainsExactElementsInAnyOrder(new string[] { PermitRuleCodeList.Codes.ADD, PermitRuleCodeList.Codes.INV, PermitRuleCodeList.Codes.MOD, PermitRuleCodeList.Codes.CAN, PermitRuleCodeList.Codes.CUS, PermitRuleCodeList.Codes.LAP, PermitRuleCodeList.Codes.TSP }, ruleCodeList.GetAllCodes());

			var ruleCodeList2 = guarantee.CountrySpecificInstruction.GetRuleCodeList(GuaranteeTypeList.Codes.AI2, "");
			AssertEquals("same cache", ruleCodeList2, ruleCodeList);

			ruleCodeList = guarantee.CountrySpecificInstruction.GetRuleCodeList(GuaranteeTypeList.Codes.ALT, "");
			AssertContainsExactElementsInAnyOrder(new string[] { PermitRuleCodeList.Codes.ADD, PermitRuleCodeList.Codes.INV, PermitRuleCodeList.Codes.MOD, PermitRuleCodeList.Codes.CAN, PermitRuleCodeList.Codes.CUS, PermitRuleCodeList.Codes.LAP, PermitRuleCodeList.Codes.TSP }, ruleCodeList.GetAllCodes());

			ruleCodeList = guarantee.CountrySpecificInstruction.GetRuleCodeList(GuaranteeTypeList.Codes.COD, "");
			AssertContainsExactElementsInAnyOrder(new string[] { PermitRuleCodeList.Codes.ADD, PermitRuleCodeList.Codes.INV, PermitRuleCodeList.Codes.MOD, PermitRuleCodeList.Codes.ENT, PermitRuleCodeList.Codes.CUS, PermitRuleCodeList.Codes.LAP, PermitRuleCodeList.Codes.TSP, PermitRuleCodeList.Codes.PCP, PermitRuleCodeList.Codes.PCD, PermitRuleCodeList.Codes.PCV }, ruleCodeList.GetAllCodes());

			ruleCodeList = guarantee.CountrySpecificInstruction.GetRuleCodeList(GuaranteeTypeList.Codes.DEF, "");
			AssertContainsExactElementsInAnyOrder(new string[] { PermitRuleCodeList.Codes.ADD, PermitRuleCodeList.Codes.INV, PermitRuleCodeList.Codes.MOD, PermitRuleCodeList.Codes.ENT, PermitRuleCodeList.Codes.CUS, PermitRuleCodeList.Codes.LAP, PermitRuleCodeList.Codes.TSP }, ruleCodeList.GetAllCodes());
		}

		public void TestGetRuleCodeListForModule()
		{
			AssertContainsExactElementsInAnyOrder(new string[] { PermitRuleCodeList.Codes.ADD, PermitRuleCodeList.Codes.INV, PermitRuleCodeList.Codes.MOD, PermitRuleCodeList.Codes.CAN, PermitRuleCodeList.Codes.ENT, PermitRuleCodeList.Codes.CUS, PermitRuleCodeList.Codes.LAP, PermitRuleCodeList.Codes.TSP, PermitRuleCodeList.Codes.PCP, PermitRuleCodeList.Codes.PCD, PermitRuleCodeList.Codes.PCV }, Factory.New<CusGuaranteeHeader>().CountrySpecificInstruction.GetRuleCodeListForModule().GetAllCodes());
		}

		public void TestGetMatchingType()
		{
			var guarantee = Factory.New<CusGuaranteeHeader>();
			AssertEquals(PermitMatchingType.SingleValue, guarantee.CountrySpecificInstruction.GetMatchingType(PermitRuleCodeList.Codes.MOD));
			AssertEquals(PermitMatchingType.SingleValue, guarantee.CountrySpecificInstruction.GetMatchingType(PermitRuleCodeList.Codes.CAN));
			AssertEquals(PermitMatchingType.SingleValue, guarantee.CountrySpecificInstruction.GetMatchingType(PermitRuleCodeList.Codes.ADD));
			AssertEquals(PermitMatchingType.Range, guarantee.CountrySpecificInstruction.GetMatchingType("XXX"));
		}

		public void TestGetValueFromFieldType()
		{
			var guarantee = Factory.New<CusGuaranteeHeader>();
			AssertEquals(nameof(FieldType.TextCodeFindBox), guarantee.CountrySpecificInstruction.GetValueFromFieldType(PermitRuleCodeList.Codes.ADD));
			AssertEquals(nameof(FieldType.TextDropEdit), guarantee.CountrySpecificInstruction.GetValueFromFieldType(PermitRuleCodeList.Codes.MOD));
			AssertEquals(nameof(FieldType.TextDropEdit), guarantee.CountrySpecificInstruction.GetValueFromFieldType(PermitRuleCodeList.Codes.CAN));
			AssertEquals(nameof(FieldType.TextDropEdit), guarantee.CountrySpecificInstruction.GetValueFromFieldType(PermitRuleCodeList.Codes.ENT));
			AssertEquals(nameof(FieldType.Text), guarantee.CountrySpecificInstruction.GetValueFromFieldType("XXX"));
		}
	}
}
