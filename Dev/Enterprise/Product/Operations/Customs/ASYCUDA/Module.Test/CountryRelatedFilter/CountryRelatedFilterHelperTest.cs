using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Module.Testing
{
	[TestedType(typeof(CountryRelatedFilterHelper))]
	sealed class CountryRelatedFilterHelperTest : BusinessObjectLookupsTestCase
	{
		public void TestMessageStatusList()
		{
			var filter = GetNewModuleFilter("MsgStatus");
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				AssertEquals("ACK, AWA, ERR, ", ((CodeDescriptionPairList)filter.Property2List).CodesAsString);
			}

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Core.Constants.CountryCodes.Singapore))
			{
				AssertEquals("ACP, AWA, CAN, ERR, NOT, REG, SNT, UNK, UPD, WRN", ((CodeDescriptionPairList)filter.Property2List).CodesAsString);
				var filter2 = GetNewModuleFilter("MsgStatus");
				AssertSame(filter.Property2List, filter2.Property2List);
			}
		}

		public void TestCargoStatusList()
		{
			var filter = GetNewModuleFilter("CargoStatus");

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				AssertContainsExactElementsInAnyOrder(new[] { "FUL", "LAS", "PAR" }, ((CodeDescriptionPairList)filter.Property2List).GetAllCodes());
			}
		}

		public void TestCustomsStatusList()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsManifestStatus, "CustomsStatus");
			var za8 = helper.CreateNewOrGetExistingCusCodeList("ZA", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsManifestStatus, "8", "Proceed to Border (SACU clearances)", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(za8.PK, "IAllowCancel", "true");
			helper.CreateNewOrGetExistingCusCodeListAttribute(za8.PK, "INotify", "");
			helper.CreateNewOrGetExistingCusCodeListAttribute(za8.PK, "ISendEntryDocs", "");
			helper.CreateNewOrGetExistingCusCodeListAttribute(za8.PK, "IUpdateCustomsStatus", "");
			helper.CreateNewOrGetExistingCusCodeListAttribute(za8.PK, "IUpdateEntryNumber", "");
			var za9 = helper.CreateNewOrGetExistingCusCodeList("ZA", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsManifestStatus, "9", "Already on Customs system", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(za9.PK, "INotify", "");
			helper.CreateNewOrGetExistingCusCodeListAttribute(za9.PK, "IUpdateEntryNumber", "");
			Factory.Save();
			var filter = GetNewModuleFilter("CustomsStatus");
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Core.Constants.CountryCodes.Vanuatu))
			{
				AssertEquals("NOT, REG, STO", ((CodeDescriptionPairList)filter.Property2List).CodesAsString);
			}

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				AssertEquals("8, 9", ((CodeDescriptionPairList)filter.Property2List).CodesAsString);
			}
		}

		public void TestCustomsOfficeList()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			helper.CreateNewOrGetExistingCusCodeList("ZZ", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, "VU", "Vanuatu", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("ZZ", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, "PG", "Papua New Guinea", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("ZZ", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, "SB", "Solomon Islands", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var vuSAIR = helper.CreateNewOrGetExistingCusCodeList("VU", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "SAIR", "Customs Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(vuSAIR.PK, "PORT", "VUSAN");
			helper.CreateTransportModeForCusCodeList(vuSAIR.PK, "AIR");
			var vuSAIR2 = helper.CreateNewOrGetExistingCusCodeList("VU", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "VULI", "Customs Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(vuSAIR2.PK, "PORT", "VUVLI");
			helper.CreateTransportModeForCusCodeList(vuSAIR2.PK, "AIR");
			helper.CreateNewOrGetExistingCusCodeList("PG", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "JAS", "Customs Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("SB", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "HIRH", "Honiara Point Cruz", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			var filter = GetNewModuleFilter("CustomsOffice");
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Core.Constants.CountryCodes.Vanuatu))
			{
				AssertEquals("SAIR, VULI", ((CodeDescriptionPairList)filter.Property2List).CodesAsString);
			}

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Core.Constants.CountryCodes.SolomonIslands))
			{
				AssertEquals("HIRH", ((CodeDescriptionPairList)filter.Property2List).CodesAsString);
			}

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Core.Constants.CountryCodes.PapuaNewGuinea))
			{
				AssertEquals("JAS", ((CodeDescriptionPairList)filter.Property2List).CodesAsString);
			}
		}

		public void TestCustomsNumberTypeList()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsEntryNumberTypes, "CustomsEntryNumberTypes");
			helper.CreateNewOrGetExistingCusCodeList("ZZ", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, "VU", "Vanuatu", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("ZZ", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, "PG", "Papua New Guinea", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("ZZ", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, "SB", "Solomon Islands", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var vuSAIR = helper.CreateNewOrGetExistingCusCodeList("VU", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsEntryNumberTypes, "SAIR", "Customs Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(vuSAIR.PK, "PORT", "VUSAN");
			helper.CreateTransportModeForCusCodeList(vuSAIR.PK, "AIR");
			var vuSAIR2 = helper.CreateNewOrGetExistingCusCodeList("VU", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsEntryNumberTypes, "VULI", "Customs Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(vuSAIR2.PK, "PORT", "VUVLI");
			helper.CreateTransportModeForCusCodeList(vuSAIR2.PK, "AIR");
			helper.CreateNewOrGetExistingCusCodeList("PG", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsEntryNumberTypes, "JAS", "Customs Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("SB", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsEntryNumberTypes, "HIRH", "Honiara Point Cruz", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			var filter = GetNewModuleFilter("CustomsNumberType");
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Core.Constants.CountryCodes.Vanuatu))
			{
				AssertEquals("SAIR, VULI", ((CodeDescriptionPairList)filter.Property2List).CodesAsString);
			}

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Core.Constants.CountryCodes.SolomonIslands))
			{
				AssertEquals("HIRH", ((CodeDescriptionPairList)filter.Property2List).CodesAsString);
			}

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Core.Constants.CountryCodes.PapuaNewGuinea))
			{
				AssertEquals("JAS", ((CodeDescriptionPairList)filter.Property2List).CodesAsString);
			}
		}

		CountryRelatedFilter GetNewModuleFilter(ZString type)
		{
			switch (type)
			{
				case "CustomsStatus":
					return new CountryRelatedFilter("CustomsStatus", (val1, val2) => new ZQuery(), Factory, FieldType.TextDropEdit, 100, CountryRelatedFilterHelper.ListGetters.CustomsStatusGetter);
				case "CustomsOffice":
					return new CountryRelatedFilter("CustomsOffice", (val1, val2) => new ZQuery(), Factory, FieldType.TextDropEdit, 100, CountryRelatedFilterHelper.ListGetters.CustomsOfficeGetter);
				case "CustomsNumberType":
					return new CountryRelatedFilter("CustomsNumberType", (val1, val2) => new ZQuery(), Factory, FieldType.TextDropEdit, 100, CountryRelatedFilterHelper.ListGetters.CustomsNumberTypeGetter);
				case "CargoStatus":
					return new CountryRelatedFilter("CargoStatus", (val1, val2) => new ZQuery(), Factory, FieldType.TextDropEdit, 100, CountryRelatedFilterHelper.ListGetters.CargoStatusGetter);
				default:
					return new CountryRelatedFilter("moo", (val1, val2) => new ZQuery(), Factory, FieldType.TextDropEdit, 100, CountryRelatedFilterHelper.ListGetters.MessageStatusGetter);
			}
		}
	}
}
