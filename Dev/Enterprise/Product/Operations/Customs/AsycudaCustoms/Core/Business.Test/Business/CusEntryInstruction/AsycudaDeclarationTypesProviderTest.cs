using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	[Customs.Business.Testing.AsycudaCustomsCountries("NA", "LS", "BW", "SZ")]
	class AsycudaDeclarationTypesProviderTest : CodeDescriptionPairListProviderTest
	{
		public override void TestIsReturningCorrectCollection()
		{
			SetupRefData();
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.Risk, Core.Constants.CountryCodes.Namibia, ZDate.Today, true))
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.Risk, Core.Constants.CountryCodes.Botswana, ZDate.Today, true))
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.Risk, Core.Constants.CountryCodes.Lesotho, ZDate.Today, false))
			{
				var expectedList = new CodeDescriptionPairList();
				expectedList.AddPair("EX1", "EX1");
				expectedList.AddPair("IM6", "IM6");
				expectedList.AddPair("IM7", "IM7");
				AssertListEqual(expectedList, CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList());
			}
			var procedureCodes = new CodeDescriptionPairList();
			foreach (var cpc in RefCusProcedureCollection.LoadCustomsProcedureCodesForCountry(new BusinessObjectFactory(), GlbCompany.CurrentCompany.Country.Code, ZDateTime.Today))
			{
				procedureCodes.AddPairIfNotExist(cpc.ZZ6_ProcedureCode, cpc.ZZ6_Description);
			}
			procedureCodes.Sort();
			AssertListEqual(CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList(), procedureCodes);
		}

		protected override DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider()
		{
			return new AsycudaDeclarationTypesProvider();
		}

		void SetupRefData()
		{
			var factory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(factory);
			var parentDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping, "ZZ");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Botswana, "Botswana", parentDataGrouping);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Namibia, "Namibia", parentDataGrouping);
			var codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty;
			helper.CreateNewOrGetExistingCusCodeType(codeType, "Entry style");
			helper.CreateNewOrGetExistingCusCodeList("ZZ", codeType, "ESD", "ensty1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Botswana, codeType, "AAA", "AAA dec", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Namibia, codeType, "BBB", "BBB dec", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Lesotho, codeType, "XXX", "XXX dec", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			factory.Save();
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Botswana, "A", "11", "11", "111", "One", "IMP", group: "IM7");
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Botswana, "A", "22", "22", "222", "Two", "IMP", group: "IM6");
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Namibia, "B", "33", "33", "333", "Three", "IMP", group: "IM7");
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Namibia, "A", "44", "44", "444", "Four", "EXP", group: "EX1");
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Lesotho, "A", "55", "55", "555", "Five", "IMP", group: "IM5");
			factory.Save();
		}
	}
}
