using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture;
using Moq;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.EU.Business.Testing
{
	class GuaranteeCountrySpecificInstructionBaseOnlyTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestValueFromAndValueToFieldType_ADD() => AssertValueFromAndValueToFieldType(PermitRuleCodeList.Codes.ADD, FieldType.TextCodeFindBox, FieldType.Text);

		[ExpectNoExceptions]
		public void TestValueFromAndValueToFieldType_CUS() => AssertValueFromAndValueToFieldType(PermitRuleCodeList.Codes.CUS, FieldType.TextCodeFindBox, FieldType.Text);

		[ExpectNoExceptions]
		public void TestValueFromAndValueToFieldType_INV() => AssertValueFromAndValueToFieldType(PermitRuleCodeList.Codes.INV, FieldType.TextDropEdit, FieldType.TextDropEdit);

		[ExpectNoExceptions]
		public void TestValueFromAndValueToFieldType_TRA() => AssertValueFromAndValueToFieldType(EUGuaranteeTypeList.Codes.TRA, FieldType.Text, FieldType.Text);

		[ExpectNoExceptions]
		public void TestValueFromAndValueToFieldType_COD() => AssertValueFromAndValueToFieldType(EUGuaranteeTypeList.Codes.COD, FieldType.Text, FieldType.Text);

		[ExpectNoExceptions]
		public void TestValueFromAndValueToFieldType_TSP() => AssertValueFromAndValueToFieldType(PermitRuleCodeList.Codes.TSP, FieldType.TextCodeFindBox, FieldType.Text);

		[ExpectNoExceptions]
		public void TestValueFromAndValueToFieldType_PCP() => AssertValueFromAndValueToFieldType(PermitRuleCodeList.Codes.PCP, FieldType.Text, FieldType.Text);

		[ExpectNoExceptions]
		public void TestValueFromAndValueToFieldType_PCV() => AssertValueFromAndValueToFieldType(PermitRuleCodeList.Codes.PCV, FieldType.Text, FieldType.Text);

		[ExpectNoExceptions]
		public void TestValueFromAndValueToFieldType_PCD() => AssertValueFromAndValueToFieldType(PermitRuleCodeList.Codes.PCD, FieldType.Text, FieldType.Text);

		[ExpectNoExceptions]
		public void TestSupportsAdditionalCustomsReferences()
		{
			var guaranteeCountrySpecificInstruction = new GuaranteeCountrySpecificInstruction(Factory);
			NUnit.Framework.Assert.That(guaranteeCountrySpecificInstruction.SupportsAdditionalCustomsReferences("COD"), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(!guaranteeCountrySpecificInstruction.SupportsAdditionalCustomsReferences("AAA"), NUnit.Framework.Is.True);
		}

		public void TestGetSubTypeList()
		{
			CombineAssertions(() =>
			{
				var subTypeList = guaranteeCountrySpecificInstruction.GetSubTypeList(EUGuaranteeTypeList.Codes.TRA);
				AssertContainsExactElementsInAnyOrder("Sub Type List", new ZString[] { "1", "2", "3", "5", "6", "7", "8", "9", "0", "A", "B" }, subTypeList.GetAllCodes());

				subTypeList = guaranteeCountrySpecificInstruction.GetSubTypeList(EUGuaranteeTypeList.Codes.COD);
				AssertContainsExactElementsInAnyOrder("Sub Type List", new ZString[] { "1", "2", "3", "5", "6", "7", "8", "9", "0", "A", "B" }, subTypeList.GetAllCodes());

				subTypeList = guaranteeCountrySpecificInstruction.GetSubTypeList("DEF");
				NUnit.Framework.Assert.That(subTypeList.Count, NUnit.Framework.Is.EqualTo(0), "No SubType list has been defined for EU guarantees");
			});
		}

		[ExpectNoExceptions]
		public void TestGetTypeList_NctsIsEnabled() => AssertGetTypeList(isNctsEnabled: true, "COD, IMP, TRA, TST, ZZZ");

		[ExpectNoExceptions]
		public void TestGetTypeList_NctsIsDisabled() => AssertGetTypeList(isNctsEnabled: false, "IMP, TST, ZZZ");

		[ExpectNoExceptions]
		public void TestPermitGuaranteeType()
		{
			NUnit.Framework.Assert.That(guaranteeCountrySpecificInstruction.PermitGuaranteeType, NUnit.Framework.Is.EqualTo(EUGuaranteeTypeList.Codes.TRA));
		}

		[ExpectNoExceptions]
		public void TestAdditionalCustomsReferenceTypes()
		{
			var guaranteeHeader = Factory.New<CusGuaranteeHeader>();
			var list = guaranteeHeader.CountrySpecificInstruction.AdditionalCustomsReferenceTypes;

			NUnit.Framework.Assert.That(list.Count, NUnit.Framework.Is.EqualTo(13));
			NUnit.Framework.Assert.That(list[0].Code, NUnit.Framework.Is.EqualTo("B1"));
			NUnit.Framework.Assert.That(list[1].Code, NUnit.Framework.Is.EqualTo("B2"));
			NUnit.Framework.Assert.That(list[2].Code, NUnit.Framework.Is.EqualTo("B3"));
			NUnit.Framework.Assert.That(list[3].Code, NUnit.Framework.Is.EqualTo("D1"));
			NUnit.Framework.Assert.That(list[4].Code, NUnit.Framework.Is.EqualTo("D2"));
			NUnit.Framework.Assert.That(list[5].Code, NUnit.Framework.Is.EqualTo("D3"));
			NUnit.Framework.Assert.That(list[6].Code, NUnit.Framework.Is.EqualTo("G1"));
			NUnit.Framework.Assert.That(list[7].Code, NUnit.Framework.Is.EqualTo("G2"));
			NUnit.Framework.Assert.That(list[8].Code, NUnit.Framework.Is.EqualTo("H1"));
			NUnit.Framework.Assert.That(list[9].Code, NUnit.Framework.Is.EqualTo("H2"));
			NUnit.Framework.Assert.That(list[10].Code, NUnit.Framework.Is.EqualTo("H3"));
			NUnit.Framework.Assert.That(list[11].Code, NUnit.Framework.Is.EqualTo("H4"));
			NUnit.Framework.Assert.That(list[12].Code, NUnit.Framework.Is.EqualTo("TR"));
		}

		protected override void SetUp()
		{
			base.SetUp();
			guaranteeCountrySpecificInstruction = new GuaranteeCountrySpecificInstruction(Factory);
		}
		GuaranteeCountrySpecificInstruction guaranteeCountrySpecificInstruction;

		void AssertValueFromAndValueToFieldType(ZString ruleCode, FieldType expectedValueFromFieldType, FieldType expectedValueToFieldType) => CombineAssertions(() =>
		{
			NUnit.Framework.Assert.That(guaranteeCountrySpecificInstruction.GetValueFromFieldType(ruleCode), NUnit.Framework.Is.EqualTo(expectedValueFromFieldType.ToString()).Using(CustomComparers.TypeComparison), "Value From Field Type");
			NUnit.Framework.Assert.That(guaranteeCountrySpecificInstruction.GetValueToFieldType(ruleCode), NUnit.Framework.Is.EqualTo(expectedValueToFieldType.ToString()).Using(CustomComparers.TypeComparison), "Value To Field Type");
		});

		[ExpectNoExceptions]
		void AssertGetTypeList(bool isNctsEnabled, string codesAsString)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var mockSettings = new Mock<Integration.Customs.Shared.INctsSettings>();
			mockSettings.Setup(x => x.IsNctsEnabled).Returns(isNctsEnabled);
			CargoWise.Application.ObjectFactory.Substitute(mockSettings.Object);

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GuaranteeType, "Guarantee Types");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GuaranteeType, "ZZZ", "Description for ZZZ", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var typeList = guaranteeCountrySpecificInstruction.GetTypeList(Core.Constants.CountryCodes.Latvia);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(typeList.CodesAsString, NUnit.Framework.Is.EqualTo(codesAsString), "List Codes");
				NUnit.Framework.Assert.That(typeList.GetDescriptionFromCode("ZZZ"), NUnit.Framework.Is.EqualTo("Description for ZZZ"), "RefCusCodeList Description");
			});
		}
	}
}
