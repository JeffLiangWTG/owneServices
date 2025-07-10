using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Module.Testing
{
	sealed class EntryHeaderFilterLookupsTest : TestCaseWithFactory
	{
		public void TestCustomsOfficeList()
		{
			AssertType<Business.FullCustomsOfficeCodeCollection>("CustomsOfficeList type", filterLookups.CustomsOfficeList);
		}

		public void TestEntryStyleList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeList("IE", Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, "B1", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateCusCodeList("IE", Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, "B2", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			Factory.Save();

			AssertContainsExactElementsInAnyOrder("EntryStyleList", new ZString[] { "IM", "EX", "CO", "EU", }, filterLookups.EntryStyleList.GetAllCodes());

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry("IE"))
			{
				AssertContainsExactElementsInAnyOrder("EntryStyleList", new ZString[] { "IM", "EX", "CO", "EU", "B1", "B2" }, filterLookups.EntryStyleList.GetAllCodes());
			}
		}

		public void TestGoodsOriginList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeList("IE", "CO15", "IT", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateCusCodeList("IE", "IM15", "QQ", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateCusCodeList("IE", "EX15", "EU", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateCusCodeList("IE", "CO15", "XX", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateCusCodeList("IE", "IM15", "IT", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));

			Factory.Save();

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry("IE"))
			{
				AssertContainsExactElementsInAnyOrder("GoodsOriginList", new[] { "IT", "QQ", "EU", "XX" }, filterLookups.GoodsOriginList.GetAllCodes());
			}
		}

		public void TestGoodsDestinationList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeList("ES", "CO17", "UK", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateCusCodeList("ES", "IM17", "QQ", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateCusCodeList("ES", "EX17", "EU", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateCusCodeList("ES", "CO17", "ZZ", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateCusCodeList("ES", "IM17", "UK", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));

			Factory.Save();

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry("ES"))
			{
				AssertContainsExactElementsInAnyOrder("GoodsOriginList", new[] { "UK", "QQ", "EU", "ZZ" }, filterLookups.GoodsDestinationList.GetAllCodes());
			}
		}

		public void TestWarehousesList()
		{
			AssertNotNull("Warehouses List", filterLookups.Warehouses);
		}

		public void TestLocalClientsList()
		{
			AssertNotNull("Local Clients List", filterLookups.LocalClients);
		}

		public void TestExportExitStatusList()
		{
			AssertNotNull("Export Exit Status List", filterLookups.ExportExitStatusList);
			var list = filterLookups.ExportExitStatusList;
			AssertEquals("CodesAsString", "EXT, ERR, REM, PRD, EXR, COX, REJ", list.CodesAsString);
		}

		public void TestRequestedProcedureList()
		{
			var currentCountry = GlbCompany.CurrentCompany.Country.Code;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(currentCountry, "A", "CPC1", "11", "111", "CPC1 Desc", "IMP", group: "H1,H2");
			helper.CreateRefCusProcedure(currentCountry, "A", "CPC2", "22", "222", "CPC2 Desc", "EXP", group: "H2,H7");
			helper.CreateRefCusProcedure(currentCountry, "B", "CPC3", "33", "333", "CPC3 Desc", "XXX", group: "H1");

			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ProcedureCode, "Procedure Code");
			helper.CreateNewOrGetExistingCusCodeList("EUN", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ProcedureCode, "CPC1", "CPC1 Desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("EUN", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ProcedureCode, "CPC2", "CPC2 Desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("EUN", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ProcedureCode, "CPC3", "CPC3 Desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var list = filterLookups.RequestedProcedureList;
			AssertEquals("CodesAsString", "CPC1, CPC2", list.CodesAsString);
			AssertSame(list, filterLookups.RequestedProcedureList);
		}

		public void TestPreviousProcedureCodeList()
		{
			var currentCountry = GlbCompany.CurrentCompany.Country.Code;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(currentCountry, "A", "01", "P1", "111", "CPC1 Desc", "IMP", group: "H1,H2");
			helper.CreateRefCusProcedure(currentCountry, "B", "02", "P2", "111", "CPC1 Desc", "IMP", group: "H1,H2");
			helper.CreateRefCusProcedure(currentCountry, "B", "04", "P3", "333", "CPC3 Desc", "XXX", group: "H1");

			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ProcedureCode, "Procedure Code");
			helper.CreateNewOrGetExistingCusCodeList("EUN", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ProcedureCode, "P1", "Pre1 Desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("EUN", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ProcedureCode, "P2", "Pre2 Desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("EUN", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ProcedureCode, "P2", "Pre2 Desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("EUN", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ProcedureCode, "P3", "Pre3 Desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var list = filterLookups.PreviousProcedureCodeList;
			AssertEquals("CodesAsString", "P1, P2", list.CodesAsString);
			AssertSame(list, filterLookups.PreviousProcedureCodeList);
		}

		public void TestAdditionalProcedureCodeList()
		{
			var currentCountry = GlbCompany.CurrentCompany.Country.Code;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(currentCountry, "A", "01", "P1", "C01", "CPC1 Desc", "IMP", group: "H1,H2");
			helper.CreateRefCusProcedure(currentCountry, "B", "01", "P2", "C02", "CPC1 Desc", "EXP", group: "H2");
			helper.CreateRefCusProcedure(currentCountry, "B", "04", "P4", "C03", "CPC3 Desc", "XXX", group: "H1");

			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping(currentCountry);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdvancedProcedureCode, "Procedure Code");
			helper.CreateNewOrGetExistingCusCodeList(currentCountry, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdvancedProcedureCode, "C01", "C01 Desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(currentCountry, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdvancedProcedureCode, "C02", "C02 Desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(currentCountry, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdvancedProcedureCode, "C03", "C03 Desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var list = filterLookups.AdditionalProcedureCodeList;
			AssertEquals("CodesAsString", "C01, C02", list.CodesAsString);
			AssertSame(list, filterLookups.AdditionalProcedureCodeList);
		}

		protected override void SetUp()
		{
			base.SetUp();
			filterBusinessObject = new EntryHeaderFilterBusinessObject();
			filterLookups = new EntryHeaderFilterLookups(filterBusinessObject);
		}

		EntryHeaderFilterBusinessObject filterBusinessObject;
		EntryHeaderFilterLookups filterLookups;
	}
}
