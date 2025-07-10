using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Declaration.Testing;

class AddInfoJobComInvoiceLineLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestRegionOfDestinationList_DropEdit() => CombineAssertions(() =>
	{
		var helper = new ESUniversalReferenceTestDataHelper(Factory);
		var countryCode = Core.Constants.CountryCodes.Spain;
		helper.CreateCusCodeListTerritory(countryCode, "01", "Test 1");
		helper.CreateCusCodeListNorthAfricanTerritory(countryCode, "02", "Test 2");
		helper.CreateCusCodeListCanaryIsland(countryCode, "03", "Test 3");
		_ = helper.CreateCusCodeList("EUN", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsFiscalTerritories, "04", "Test 4", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));

		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.JobComInvoiceLines.AddNew();
		var lookups = invoiceLine.AddInfoLookups;
		AssertContainsExactElementsInExactOrder("Same list as helper", LookupsHelper.RegionOfDestinationDropEditList(Factory), lookups.RegionOfDestinationList);
		AssertSame("List is cached", LookupsHelper.RegionOfDestinationDropEditList(Factory), lookups.RegionOfDestinationList);
	});

	[TestDate(2021, 03, 29)]
	public void TestRegionOfDestinationList_CodeFindBox() => CombineAssertions(() =>
	{
		var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
		var eunCode = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EuropeanUnionEUN;
		var esCode = Core.Constants.CountryCodes.Spain;
		var grouping = helper.CreateNewOrGetExistingDataGrouping(eunCode);
		helper.CreateNewOrGetExistingDataGrouping(esCode, parent: grouping);
		helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.CL142, "Region Of Destination");
		helper.CreateCusCodeList(eunCode, UniversalReferenceConstants.RefCusCodeListTypes.CL142, "DE", "DE", new ZDateTime(2021, 01, 01), new ZDateTime(2021, 12, 31));
		helper.CreateCusCodeList(eunCode, UniversalReferenceConstants.RefCusCodeListTypes.CL142, "IE", "IE", new ZDateTime(2021, 01, 01), new ZDateTime(2021, 02, 01));
		helper.CreateCusCodeList(esCode, UniversalReferenceConstants.RefCusCodeListTypes.CL142, "28", "MADRID", new ZDateTime(2021, 01, 01), new ZDateTime(2021, 12, 31));

		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.JobComInvoiceLines.AddNew();
		invoiceLine.ZG_CountryOfDestination = Core.Constants.CountryCodes.France;
		var lookups = invoiceLine.AddInfoLookups;

		AssertContainsExactElementsInAnyOrder("Same list as helper", LookupsHelper.RegionOfDestinationCodeFindBoxList(Factory), lookups.RegionOfDestinationList);
		AssertSame("List is cached", LookupsHelper.RegionOfDestinationCodeFindBoxList(Factory), lookups.RegionOfDestinationList);
	});

	public void TestExciseCodeList()
	{
		var helper = new ESUniversalReferenceTestDataHelper(Factory);
		var countryCode = Core.Constants.CountryCodes.Spain;
		helper.CreateCusCodeListCanaryIsland(countryCode, "61", "Test 61");

		var impTariffType = helper.CreateTariffType(countryCode, "IMP");
		var esexcTariffType = helper.CreateTariffType(countryCode, "ESEXC");
		var canexcTariffType = helper.CreateTariffType(countryCode, "CANEX");
		var zTariffType = helper.CreateTariffType(countryCode, "ZZZZ");
		Factory.Save();
		var tariff = helper.LoadOrCreateNewTariff(countryCode, impTariffType.PK, "11112222", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
		var tariffExcise = helper.LoadOrCreateNewTariff(countryCode, esexcTariffType.PK, "0A0", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), "desc1");
		var tariffExciseExpired = helper.LoadOrCreateNewTariff(countryCode, esexcTariffType.PK, "0A1", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(-2), "expired");
		var tariffExcise2 = helper.LoadOrCreateNewTariff(countryCode, esexcTariffType.PK, "1PL", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), "desc2");
		var tariffExcise3 = helper.LoadOrCreateNewTariff(countryCode, canexcTariffType.PK, "0A7", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), "desc3");
		var tariffExcise4 = helper.LoadOrCreateNewTariff(countryCode, canexcTariffType.PK, "0A3", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), "desc4");
		var tariffExcise5 = helper.LoadOrCreateNewTariff(countryCode, zTariffType.PK, "NoExcise", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), "desc5");

		helper.CreateTariffRelationship(tariffExcise.PK, impTariffType.PK, tariff.ZZ1_TariffCode);
		helper.CreateTariffRelationship(tariffExciseExpired.PK, impTariffType.PK, tariff.ZZ1_TariffCode);
		helper.CreateTariffRelationship(tariffExcise2.PK, impTariffType.PK, tariff.ZZ1_TariffCode);
		helper.CreateTariffRelationship(tariffExcise3.PK, impTariffType.PK, tariff.ZZ1_TariffCode);
		helper.CreateTariffRelationship(tariffExcise4.PK, impTariffType.PK, tariff.ZZ1_TariffCode);
		helper.CreateTariffRelationship(tariffExcise5.PK, impTariffType.PK, tariff.ZZ1_TariffCode);

		var tariffWithoutExcises = helper.LoadOrCreateNewTariff(countryCode, impTariffType.PK, "33332222", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		declaration.ZG_DestinationState = CustomsFiscalTerritoriesList.GetCanaryIslandsList(Factory).GetAllCodes()[0];
		var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();

		CombineAssertions(() =>
		{
			invoiceLine.JI_Tariff = tariff.ZZ1_TariffCode;
			var list = invoiceLine.AddInfoLookups.ExciseCodeList;
			AssertEquals("ExciseCodeList Contains codes 0A7 and 0A3 for tariff and canary island destination", "0A7 - desc3\r\n0A3 - desc4", list.ElementsAsString);
			AssertEquals("ExciseCodeList is cached", list, invoiceLine.AddInfoLookups.ExciseCodeList);

			invoiceLine.JI_Tariff = ZString.Empty;
			AssertEquals("Empty ExciseCodeList when the tariff is empty", ZString.Empty, invoiceLine.AddInfoLookups.ExciseCodeList.ElementsAsString);

			invoiceLine.JI_Tariff = tariffWithoutExcises.ZZ1_TariffCode;
			AssertEquals("Empty ExciseCodeList when InvoiceLine Tariff has not child Excise Tariff", ZString.Empty, invoiceLine.AddInfoLookups.ExciseCodeList.ElementsAsString);

			invoiceLine.JI_Tariff = tariff.ZZ1_TariffCode;
			declaration.ZG_DestinationState = "ZZ";
			AssertEquals("ExciseCodeList Contains codes 0A0 for tariff and not canary island destination", "0A0 - desc1", invoiceLine.AddInfoLookups.ExciseCodeList.ElementsAsString);
		});
	}

	public void TestExciseExcemptionList()
	{
		var addInfo = new AddInfoJobComInvoiceLine(Factory.New<JobComInvoiceLine>().JI_AddInfoInfo);
		AssertEquals("Lookups.ExciseExcemptionList.CodesAsString", "B, D, E, N, 0, S", addInfo.Lookups.ExciseExemptionList.CodesAsString);
	}

	public void TestREAProductCodeList()
	{
		var esCode = Core.Constants.CountryCodes.Spain;

		var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);

		var impTariffType = helper.CreateNewOrGetExistingTariffType(esCode, Customs.Business.UniversalReferenceConstants.CusTariffTypes.ImportTariff);
		Factory.Save();
		var tariff = helper.LoadOrCreateNewTariff(esCode, impTariffType.PK, "11111111", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));

		var reaRateType = helper.CreateNewOrGetExistingRateType(esCode, UniversalReferenceConstants.RateTypeList.REA, "REA - AY Tax Rebate");
		var canaRateType = helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.REACodeDescriptions, "REA Descriptions");

		var rateCodeAYD = helper.LoadOrCreateNewCusRateCode(Factory, "AYD", reaRateType.PK);
		var rateCodeAYT = helper.LoadOrCreateNewCusRateCode(Factory, "AYT", reaRateType.PK);

		var rateAYD = helper.CreateRate(tariff, rateCodeAYD.PK, ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
		var rateAYT = helper.CreateRate(tariff, rateCodeAYT.PK, ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));

		helper.CreateCusApplicability(rateAYD, null, ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1), "T001");
		helper.CreateCusApplicability(rateAYT, null, ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1), "T002");
		helper.CreateCusApplicability(rateAYT, null, ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1), "T003");

		helper.CreateNewOrGetExistingCusCodeList(esCode, canaRateType.ZZK_CodeType, "T001", "Description 1", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
		helper.CreateNewOrGetExistingCusCodeList(esCode, canaRateType.ZZK_CodeType, "T002", "Description 2", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
		helper.CreateNewOrGetExistingCusCodeList(esCode, canaRateType.ZZK_CodeType, "T003", "Description 3", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));

		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		var invLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invLine.JI_Tariff = tariff.ZZ1_TariffCode;
		invLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Spain;

		CombineAssertions(() =>
		{
			invLine.ZG_IsREADirectConsumption = true; //AYD
			AssertEquals("Lookups.REAProductCodeList.CodesAsString", "T001 - Description 1", invLine.AddInfoLookups.REAProductCodeList.ElementsAsString);

			invLine.ZG_IsREADirectConsumption = false; //AYT
			AssertEquals("Lookups.REAProductCodeList.CodesAsString", "T002 - Description 2\r\nT003 - Description 3", invLine.AddInfoLookups.REAProductCodeList.ElementsAsString);
		});
	}

	public void TestAIEMTypeCodeList()
	{
		var helper = new ESUniversalReferenceTestDataHelper(Factory);
		var countryCode = Core.Constants.CountryCodes.Spain;
		helper.CreateCusCodeListCanaryIsland(countryCode, "61", "Test 61");

		var impTariffType = helper.CreateTariffType(countryCode, "IMP");
		var aiemTariffType = helper.CreateTariffType(countryCode, "AIEM");
		var zTariffType = helper.CreateTariffType(countryCode, "ZZZZ");
		Factory.Save();
		var tariff = helper.LoadOrCreateNewTariff(countryCode, impTariffType.PK, "11112222", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
		var tariffAIEM = helper.LoadOrCreateNewTariff(countryCode, aiemTariffType.PK, "11112222_AIEM01", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), "desc1");
		var tariffAIEM2 = helper.LoadOrCreateNewTariff(countryCode, aiemTariffType.PK, "11112222_AIEM02", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), "desc2");
		var tariffAIEMExpired = helper.LoadOrCreateNewTariff(countryCode, aiemTariffType.PK, "11112222_AIEM03", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(-2), "expired");
		var tariffAIEM3 = helper.LoadOrCreateNewTariff(countryCode, aiemTariffType.PK, "11112222AIEM03", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), "desc3");
		var tariffAIEM4 = helper.LoadOrCreateNewTariff(countryCode, zTariffType.PK, "11112222_NOAIEM04", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), "desc4");

		helper.CreateTariffRelationship(tariffAIEM.PK, impTariffType.PK, tariff.ZZ1_TariffCode);
		helper.CreateTariffRelationship(tariffAIEM2.PK, impTariffType.PK, tariff.ZZ1_TariffCode);
		helper.CreateTariffRelationship(tariffAIEMExpired.PK, impTariffType.PK, tariff.ZZ1_TariffCode);
		helper.CreateTariffRelationship(tariffAIEM3.PK, impTariffType.PK, tariff.ZZ1_TariffCode);
		helper.CreateTariffRelationship(tariffAIEM4.PK, impTariffType.PK, tariff.ZZ1_TariffCode);

		var tariffWithoutAIEM = helper.LoadOrCreateNewTariff(countryCode, impTariffType.PK, "33332222", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		declaration.ZG_DestinationState = CustomsFiscalTerritoriesList.GetCanaryIslandsList(Factory).GetAllCodes()[0];
		var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
		invoiceLine.JI_Tariff = tariff.ZZ1_TariffCode;
		var list = invoiceLine.AddInfoLookups.AIEMTypeCodeList;
		AssertEquals("AIEMTypeCodeList Contains codes AIEM01 and AIEM02 with descriptions", "AIEM01 - desc1\r\nAIEM02 - desc2", list.ElementsAsString);
		AssertEquals("AIEMTypeCodeList is cached", list, invoiceLine.AddInfoLookups.AIEMTypeCodeList);

		invoiceLine.JI_Tariff = ZString.Empty;
		AssertEquals("Empty AIEMTypeCodeList when the tariff is empty", ZString.Empty, invoiceLine.AddInfoLookups.AIEMTypeCodeList.ElementsAsString);

		invoiceLine.JI_Tariff = tariffWithoutAIEM.ZZ1_TariffCode;
		AssertEquals("Empty AIEMTypeCodeList when InvoiceLine Tariff has not child AIEM Tariff", ZString.Empty, invoiceLine.AddInfoLookups.AIEMTypeCodeList.ElementsAsString);

		invoiceLine.JI_Tariff = tariff.ZZ1_TariffCode;
		declaration.ZG_DestinationState = "ZZ";
		AssertEquals("Empty AIEMTypeCodeList when DestinationStateIsCanaryIsland is false", ZString.Empty, invoiceLine.AddInfoLookups.AIEMTypeCodeList.ElementsAsString);
	}
}
