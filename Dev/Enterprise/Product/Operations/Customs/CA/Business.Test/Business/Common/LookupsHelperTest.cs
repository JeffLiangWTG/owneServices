using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal.Testing;
using static Enterprise.Customs.CA.Business.UniversalReferenceConstants;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class LookupsHelperTest : BusinessObjectLookupsTestCase
	{
		public void TestMethods()
		{
			AssertEquals(typeof(CACFIAEndUseCodesCollection), LookupsHelper.CFIAEndUseCodes(Factory).GetType());
			AssertEquals(typeof(CACFIAMiscCodesCollection), LookupsHelper.CFIAMiscIDCodes(Factory).GetType());

			AssertTypeAndCaching(LookupsHelper.TreatmentCodes, typeof(CodeDescriptionPairList));
			AssertTypeAndCaching(LookupsHelper.CFIAStatesOfOrigin, typeof(USStatesList));
			AssertTypeAndCaching(LookupsHelper.CanadianProvinces, typeof(CanadianProvinceList));
			AssertTypeAndCaching(LookupsHelper.ValueForDutyCodes, typeof(ValueForDutyCodes));
			AssertTypeAndCaching(LookupsHelper.ImportReasonCodes, typeof(ImportReasonCodes));
			AssertTypeAndCaching(LookupsHelper.GSTStatusCodes, typeof(GSTStatusCodes));
			AssertTypeAndCaching(LookupsHelper.ETExemptionCodes, typeof(ExciseTaxExemptionCodes));
			AssertTypeAndCaching(LookupsHelper.CA_PGAIndicatorList, typeof(YesNoList));

			AssertTypeAndCaching(LookupsHelper.StatesOfOriginBase, true, "", typeof(CanadianProvinceList));
			AssertTypeAndCaching(LookupsHelper.StatesOfOriginBase, false, Core.Constants.CountryCodes.Canada, typeof(CanadianProvinceList));
			AssertTypeAndCaching(LookupsHelper.StatesOfOriginBase, false, Core.Constants.CountryCodes.UnitedStates, typeof(USStatesList));
			AssertTypeAndCaching(LookupsHelper.StatesOfOriginBase, false, Core.Constants.CountryCodes.China, typeof(CodeDescriptionPairList));
			AssertEquals(0, LookupsHelper.StatesOfOriginBase(Factory, false, Core.Constants.CountryCodes.China).Count);
		}

		void AssertTypeAndCaching<T>(Func<BusinessObjectFactory, T> func, Type type)
		{
			var value1 = func(Factory);
			AssertEquals(type, value1.GetType());
			var value2 = func(Factory);
			AssertSame(value1, value2);
		}

		void AssertTypeAndCaching<T>(Func<BusinessObjectFactory, bool, ZString, T> func, bool isExport, ZString origin, Type type)
		{
			var value1 = func(Factory, isExport, origin);
			AssertEquals(type, value1.GetType());
			var value2 = func(Factory, isExport, origin);
			AssertSame(value1, value2);
		}

		public void TestTreatmentCodesByOriginAndExport()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreatePreferenceForCountry("02", "Most-Favoured-Nation", Core.Constants.CountryCodes.Canada);
			helper.CreatePreferenceForCountry("03", "General Tariff", Core.Constants.CountryCodes.Canada);
			helper.CreatePreferenceForCountry("100", "Normal Third Country Tariff Duty (Including Ceilings)", Core.Constants.CountryCodes.UnitedKingdom);

			var list = LookupsHelper.TreatmentCodesByOriginAndExport(Factory, Core.Constants.CountryCodes.Canada, string.Empty, string.Empty);
			Assert(list.ContainsOnly("02", "03"));

			list = LookupsHelper.TreatmentCodesByOriginAndExport(Factory, Core.Constants.CountryCodes.UnitedKingdom, string.Empty, string.Empty);
			AssertEquals(0, list.Count);
		}

		public void TestCasualImportCommodityList()
		{
			CreateTestDataForCasualImportCommodity();
			var list = LookupsHelper.CasualImportCommodityList(Factory);
			AssertEquals(3, list.Count);
			Assert(list.ContainsOnly("Beer", "Cigars", "Cannabis"));
		}

		public void TestGetCasualImportCommodityType()
		{
			CreateTestDataForCasualImportCommodity();
			AssertEquals(CasualImportConstants.CasualImpCommodityType.Alcohol, LookupsHelper.GetCasualImportCommodityType(Factory, "Beer"));
			AssertEquals(CasualImportConstants.CasualImpCommodityType.Tobacco, LookupsHelper.GetCasualImportCommodityType(Factory, "Cigars"));
			AssertEquals(CasualImportConstants.CasualImpCommodityType.Tobacco, LookupsHelper.GetCasualImportCommodityType(Factory, "Cannabis"));
			AssertEquals("", LookupsHelper.GetCasualImportCommodityType(Factory, "X"));
			AssertEquals("", LookupsHelper.GetCasualImportCommodityType(Factory, ""));
		}

		void CreateTestDataForCasualImportCommodity()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Canada, RefCusCodeListType.Codes.CasualImportCommodity,
				"Beer", "Alcohol - Beer", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributes.Names.Type, CasualImportConstants.CasualImpCommodityType.Alcohol);
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Canada, RefCusCodeListType.Codes.CasualImportCommodity,
				"Cigars", "Cigars", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributes.Names.Type, CasualImportConstants.CasualImpCommodityType.Tobacco);
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Canada, RefCusCodeListType.Codes.CasualImportCommodity,
				"Cannabis", "Cannabis", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributes.Names.Type, CasualImportConstants.CasualImpCommodityType.Tobacco);
			Factory.Save();
		}
	}
}
