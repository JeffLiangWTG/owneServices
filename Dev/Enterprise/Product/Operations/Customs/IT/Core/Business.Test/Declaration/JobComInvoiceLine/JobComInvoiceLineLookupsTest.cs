using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class JobComInvoiceLineLookupsTest : EU.Business.Declaration.Testing.JobComInvoiceLineLookupsTest
{
	public void TestCPCListLookup()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var proc4000 = helper.CreateRefCusProcedure("IT", "IM", "40", "00", "", "4000 desc", "IMP");
		var proc4010 = helper.CreateRefCusProcedure("IT", "IM", "40", "10", "", "4010 desc", "IMP");
		var proc4020 = helper.CreateRefCusProcedure("IT", "IM", "40", "20", "", "4020 desc", "IMP");
		var proc5100 = helper.CreateRefCusProcedure("IT", "IM", "51", "00", "", "5100 desc", "IMP");

		var proc1000 = helper.CreateRefCusProcedure("IT", "EX", "10", "00", "", "1000 desc", "EXP");
		var proc1020 = helper.CreateRefCusProcedure("IT", "EX", "10", "20", "", "1020 desc", "EXP");
		var proc3000 = helper.CreateRefCusProcedure("IT", "EX", "30", "00", "", "3000 desc", "EXP");

		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		declaration.JE_MessageType = "IMP";
		entryInstruction.CEI_Procedure = "40";
		var cpcList = invoiceLine.Lookups.CPCList;
		AssertEquals(3, cpcList.Count);
		AssertEquals(true, cpcList.Contains(proc4000));
		AssertEquals(true, cpcList.Contains(proc4010));
		AssertEquals(true, cpcList.Contains(proc4020));
		AssertEquals(false, cpcList.Contains(proc5100));
		AssertEquals(false, cpcList.Contains(proc1000));
		AssertEquals(false, cpcList.Contains(proc1020));
		AssertEquals(false, cpcList.Contains(proc3000));

		declaration.JE_MessageType = "EXP";
		entryInstruction.CEI_Procedure = "40";
		cpcList = invoiceLine.Lookups.CPCList;
		AssertEquals(0, cpcList.Count);

		declaration.JE_MessageType = "EXP";
		entryInstruction.CEI_Procedure = "10";
		cpcList = invoiceLine.Lookups.CPCList;
		AssertEquals(2, cpcList.Count);
		AssertEquals(false, cpcList.Contains(proc4000));
		AssertEquals(false, cpcList.Contains(proc4010));
		AssertEquals(false, cpcList.Contains(proc4020));
		AssertEquals(false, cpcList.Contains(proc5100));
		AssertEquals(true, cpcList.Contains(proc1000));
		AssertEquals(true, cpcList.Contains(proc1020));
		AssertEquals(false, cpcList.Contains(proc3000));

		declaration.JE_MessageType = "IMP";
		entryInstruction.CEI_Procedure = "";
		cpcList = invoiceLine.Lookups.CPCList;
		AssertEquals(4, cpcList.Count);
		AssertEquals(true, cpcList.Contains(proc4000));
		AssertEquals(true, cpcList.Contains(proc4010));
		AssertEquals(true, cpcList.Contains(proc4020));
		AssertEquals(true, cpcList.Contains(proc5100));
		AssertEquals(false, cpcList.Contains(proc1000));
		AssertEquals(false, cpcList.Contains(proc1020));
		AssertEquals(false, cpcList.Contains(proc3000));

		declaration.JE_MessageType = "EXP";
		entryInstruction.CEI_Procedure = "";
		cpcList = invoiceLine.Lookups.CPCList;
		AssertEquals(3, cpcList.Count);
		AssertEquals(false, cpcList.Contains(proc4000));
		AssertEquals(false, cpcList.Contains(proc4010));
		AssertEquals(false, cpcList.Contains(proc4020));
		AssertEquals(false, cpcList.Contains(proc5100));
		AssertEquals(true, cpcList.Contains(proc1000));
		AssertEquals(true, cpcList.Contains(proc1020));
		AssertEquals(true, cpcList.Contains(proc3000));
	}

	public void TestSupplementaryQuantityUOMs()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Italy, Universal.Constants.TariffTypes.Import);
		Factory.Save();

		var tariffWithMultipleUnits = helper.CreateTariff(Core.Constants.CountryCodes.Italy, tariffType.PK, "1111111111", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateTariffUOM(tariffWithMultipleUnits, UnitOfMeasureTypes.AdditionalUOMType, "NAR");
		helper.CreateTariffUOM(tariffWithMultipleUnits, UnitOfMeasureTypes.AdditionalUOMType, "XXX");

		helper.CreateTariff(Core.Constants.CountryCodes.Italy, tariffType.PK, "3333333333", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

		var tariffWithOneUnit = helper.CreateTariff(Core.Constants.CountryCodes.Italy, tariffType.PK, "2222222222", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateTariffUOM(tariffWithOneUnit, UnitOfMeasureTypes.AdditionalUOMType, "SSS");

		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

		invoiceLine.JI_Tariff = "";
		var unitOfMeasures = invoiceLine.Lookups.SupplementaryQuantityUOMs;
		AssertEquals("No UOMs available", 0, unitOfMeasures.Count());

		invoiceLine.JI_Tariff = "1";
		unitOfMeasures = invoiceLine.Lookups.SupplementaryQuantityUOMs;
		AssertEquals("No UOMs available", 0, unitOfMeasures.Count());

		invoiceLine.JI_Tariff = "3333333333";
		unitOfMeasures = invoiceLine.Lookups.SupplementaryQuantityUOMs;
		AssertEquals("No UOMs available", 0, unitOfMeasures.Count());

		invoiceLine.JI_Tariff = "2222222222";
		unitOfMeasures = invoiceLine.Lookups.SupplementaryQuantityUOMs;
		AssertEquals("1 UOM available", 1, unitOfMeasures.Count());
		Assert("SSS is available", unitOfMeasures.Contains("SSS"));

		invoiceLine.JI_Tariff = "1111111111";
		unitOfMeasures = invoiceLine.Lookups.SupplementaryQuantityUOMs;
		AssertEquals("2 UOMs available", 2, unitOfMeasures.Count());
		Assert("NAR is available", unitOfMeasures.Contains("NAR"));
		Assert("XXX is available", unitOfMeasures.Contains("XXX"));
	}

	public void TestItalyStateList()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		var italyStatesList = invoiceLine.Lookups.ItalyStateList;
		CombineAssertions("Italian states are listed", () =>
		{
			AssertEquals("Contains MI?", true, italyStatesList.ContainsCode("MI"));
			AssertEquals("Contains PD?", true, italyStatesList.ContainsCode("PD"));
			AssertEquals("Contains VI?", true, italyStatesList.ContainsCode("VI"));
		});
	}

	public void TestCountryOfExportList()
	{
		SetUpExportCountryRefData();
		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

		declaration.JE_EntryStyle = "EX";
		AssertEquals("When EntryStyle = 'EX', CountryOfExportList CodesAsString", "ZZ", ((CodeDescriptionPairList)invoiceLine.Lookups.CountryOfExportList).CodesAsString);

		declaration.JE_EntryStyle = "CO";
		AssertEquals("When EntryStyle = 'CO', CountryOfExportList CodesAsString", "CC", ((CodeDescriptionPairList)invoiceLine.Lookups.CountryOfExportList).CodesAsString);

		declaration.JE_EntryStyle = "EU";
		AssertEquals("When EntryStyle = 'EU', CountryOfExportList CodesAsString", "EE", ((CodeDescriptionPairList)invoiceLine.Lookups.CountryOfExportList).CodesAsString);
	}

	public void TestCountryOfExportListWhenParentDeclarationIsNull()
	{
		var invoiceLine = Factory.New<JobComInvoiceLine>();
		AssertEquals("CountryOfExportList CodesAsString", "", ((CodeDescriptionPairList)invoiceLine.Lookups.CountryOfExportList).CodesAsString);
	}

	public void TestPrimaryPreferenceList_WhenDeclarationGoodsOrigin_IsTurkey()
	{
		DutyCalculatorStrategyTurkeyNonImpositionOfCustomsDutiesTest
			.SetupRatesAndTariff(Factory);
		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		declaration.JE_GoodsOrigin = "TR";
		var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
		invoiceLine.JI_Tariff = "4016999190";
		invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.SouthAfrica;
		AssertContainsExactElementsInExactOrder("When JE_GoodsOrigin is TR", new[] { "100", "400" }, GetPrimaryPreferenceListCodes());

		declaration.JE_GoodsOrigin = "FR";
		AssertContainsExactElementsInAnyOrder("When JE_GoodsOrigin is not FE", new[] { "100" }, GetPrimaryPreferenceListCodes());

		string[] GetPrimaryPreferenceListCodes()
			=> (invoiceLine.Lookups.PrimaryPreferenceList as CodeDescriptionPairList).GetAllCodes();
	}

	void SetUpExportCountryRefData()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN");
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy, parent: grouping);

		helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EX15, "Export country/territory for entry style EX");
		helper.CreateCusCodeList(Core.Constants.CountryCodes.Italy, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EX15, "ZZ", "Test ZZ", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
		helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CO15, "Export country/territory for entry style CO");
		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Italy, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CO15, "CC", "Test CC", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), RefCusCodeListAttributeTypes.Codes.Direction, EU.Business.UniversalReferenceConstants.RefCusCodeListDirectionType.Export);
		helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EU15, "Export country/territory for entry style EU");
		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Italy, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EU15, "EE", "Test EE", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), RefCusCodeListAttributeTypes.Codes.Direction, EU.Business.UniversalReferenceConstants.RefCusCodeListDirectionType.Export);
	}
}
